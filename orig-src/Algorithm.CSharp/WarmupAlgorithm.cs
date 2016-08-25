using System;
using QuantConnect.Data;
using QuantConnect.Indicators;

package com.quantconnect.lean.Algorithm.CSharp
{
    public class WarmupAlgorithm : QCAlgorithm
    {
        private boolean first = true;
        private static final String Symbol = "SPY";
        private static final int FastPeriod = 60;
        private static final int SlowPeriod = 3600;
        private ExponentialMovingAverage fast, slow;

        public @Override void Initialize() {
            SetStartDate(2013, 10, 08);  //Set Start Date
            SetEndDate(2013, 10, 11);    //Set End Date
            SetCash(100000);             //Set Strategy Cash
            // Find more symbols here: http://quantconnect.com/data
            AddSecurity(SecurityType.Equity, Symbol, Resolution.Second);

            fast = EMA(Symbol, FastPeriod);
            slow = EMA(Symbol, SlowPeriod);

            SetWarmup(SlowPeriod);
        }
        /**
         * OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        */
         * @param data Slice object keyed by symbol containing the stock data
        public @Override void OnData(Slice data) {
            if( first && !IsWarmingUp) {
                first = false;
                Console.WriteLine( "Fast: " + fast.Samples);
                Console.WriteLine( "Slow: " + slow.Samples);
            }
            if( fast > slow) {
                SetHoldings(Symbol, 1);
            }
            else
            {
                SetHoldings(Symbol, -1);
            }
        }
    }
}