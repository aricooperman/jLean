using System;
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities;

package com.quantconnect.lean.Algorithm.CSharp
{
    public class HistoryAndWarmupRegressionAlgorithm : QCAlgorithm
    {
        private static final String SPY    = "SPY";
        private static final String GOOG   = "GOOG";
        private static final String IBM    = "IBM";
        private static final String BAC    = "BAC";
        private static final String GOOGL  = "GOOGL";

        private readonly Map<Symbol, SymbolData> _sd = new Map<Symbol, SymbolData>();

        public @Override void Initialize() {
            SetStartDate(2013, 10, 08);
            SetEndDate(2013, 10, 11);

            SetCash(1000000);

            AddSecurity(SecurityType.Equity, SPY, Resolution.Minute);
            AddSecurity(SecurityType.Equity, IBM, Resolution.Minute);
            AddSecurity(SecurityType.Equity, BAC, Resolution.Minute);

            AddSecurity(SecurityType.Equity, GOOG, Resolution.Daily);
            AddSecurity(SecurityType.Equity, GOOGL, Resolution.Daily);

            foreach (security in Securities) {
                _sd.Add(security.Key, new SymbolData(security.Key, this));
            }

            // we want to warm up our algorithm
            SetWarmup(SymbolData.RequiredBarsWarmup);
        }

        public @Override void OnData(Slice data) {
            // we are only using warmup for indicator spooling, so wait for us to be warm then continue
            if( IsWarmingUp) return;

            foreach (sd in _sd.Values) {
                lastPriceTime = sd.Close.Current.Time;
                // only make decisions when we have data on our requested resolution
                if( lastPriceTime.RoundDown(sd.Security.Resolution.ToTimeSpan()) == lastPriceTime) {
                    sd.Update();
                }
            }
        }

        public @Override void OnOrderEvent(OrderEvent fill) {
            SymbolData sd;
            if( _sd.TryGetValue(fill.Symbol, out sd)) {
                sd.OnOrderEvent(fill);
            }
        }

        class SymbolData
        {
            public static final int RequiredBarsWarmup = 40;
            public static final BigDecimal PercentTolerance = 0.001m;
            public static final BigDecimal PercentGlobalStopLoss = 0.01m;
            private static final int LotSize = 10;

            public readonly Symbol Symbol;
            public readonly Security Security;

            public int Quantity
            {
                get { return Security.Holdings.Quantity; }
            }

            public readonly Identity Close;
            public readonly AverageDirectionalIndex ADX;
            public readonly ExponentialMovingAverage EMA;
            public readonly MovingAverageConvergenceDivergence MACD;

            private readonly QCAlgorithm _algorithm;

            private OrderTicket _currentStopLoss;

            public SymbolData(Symbol symbol, QCAlgorithm algorithm) {
                Symbol = symbol;
                Security = algorithm.Securities[symbol];

                Close = algorithm.Identity(symbol);
                ADX = algorithm.ADX(symbol, 14);
                EMA = algorithm.EMA(symbol, 14);
                MACD = algorithm.MACD(symbol, 12, 26, 9);

                // if we're receiving daily 

                _algorithm = algorithm;
            }

            public boolean IsReady
            {
                get { return Close.IsReady && ADX.IsReady & EMA.IsReady && MACD.IsReady; }
            }

            public boolean IsUptrend
            {
                get
                {
                    static final BigDecimal tolerance = 1 + PercentTolerance;

                    return MACD.Signal > MACD*tolerance
                        && EMA > Close*tolerance;
                }
            }

            public boolean IsDowntrend
            {
                get
                {
                    static final BigDecimal tolerance = 1 - PercentTolerance;

                    return MACD.Signal < MACD*tolerance
                        && EMA < Close*tolerance;
                }
            }

            public void OnOrderEvent(OrderEvent fill) {
                if( fill.Status != OrderStatus.Filled) {
                    return;
                }

                // if we just finished entering, place a stop loss as well
                if( Security.Invested) {
                    stop = Security.Holdings.IsLong 
                        ? fill.FillPrice*(1 - PercentGlobalStopLoss) 
                        : fill.FillPrice*(1 + PercentGlobalStopLoss);

                    _currentStopLoss = _algorithm.StopMarketOrder(Symbol, -Quantity, stop, "StopLoss at: " + stop);
                }
                // check for an exit, cancel the stop loss
                else
                {
                    if( _currentStopLoss != null && _currentStopLoss.Status.IsOpen()) {
                        // cancel our current stop loss
                        _currentStopLoss.Cancel( "Exited position");
                        _currentStopLoss = null;
                    }
                }
            }

            public void Update() {
                OrderTicket ticket;
                TryEnter(out ticket);
                TryExit(out ticket);
            }

            public boolean TryEnter(out OrderTicket ticket) {
                ticket = null;
                if( Security.Invested) {
                    // can't enter if we're already in
                    return false;
                }

                int qty = 0;
                BigDecimal limit = 0m;
                if( IsUptrend) {
                    // 100 order lots
                    qty = LotSize;
                    limit = Security.Low;
                }
                else if( IsDowntrend) {
                    limit = Security.High;
                    qty = -LotSize;
                }
                if( qty != 0) {
                    ticket = _algorithm.LimitOrder(Symbol, qty, limit, "TryEnter at: " + limit);
                }
                return qty != 0;
            }

            public boolean TryExit(out OrderTicket ticket) {
                static final BigDecimal exitTolerance = 1 + 2 * PercentTolerance;

                ticket = null;
                if( !Security.Invested) {
                    // can't exit if we haven't entered
                    return false;
                }

                BigDecimal limit = 0m;
                if( Security.Holdings.IsLong && Close*exitTolerance < EMA) {
                    limit = Security.High;
                }
                else if( Security.Holdings.IsShort && Close > EMA*exitTolerance) {
                    limit = Security.Low;
                }
                if( limit != 0) {
                    ticket = _algorithm.LimitOrder(Symbol, -Quantity, limit, "TryExit at: " + limit);
                }
                return -Quantity != 0;
            }
        }
    }
}