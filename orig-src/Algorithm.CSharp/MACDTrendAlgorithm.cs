﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;

package com.quantconnect.lean.Algorithm.Examples
{
    /**
     * MACD Example Algorithm
    */
    public class MACDTrendAlgorithm : QCAlgorithm
    {
        private DateTime previous;
        private MovingAverageConvergenceDivergence macd;
        private String Symbol = "SPY";

        /**
         * Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        */
        public @Override void Initialize() {
            SetStartDate(2009, 01, 01);
            SetEndDate(2015, 01, 01);

            AddSecurity(SecurityType.Equity, Symbol);

            // define our daily macd(12,26) with a 9 day signal
            macd = MACD(Symbol, 9, 26, 9, MovingAverageType.Exponential, Resolution.Daily);
        }

        /**
         * OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        */
         * @param data TradeBars IDictionary object with your stock data
        public void OnData(TradeBars data) {
            // only once per day
            if( previous.Date == Time.Date) return;

            if( !macd.IsReady) return;

            holding = Portfolio[Symbol];

            BigDecimal signalDeltaPercent = (macd - macd.Signal)/macd.Fast;
            tolerance = 0.0025m;

            // if our macd is greater than our signal, then let's go long
            if( holding.Quantity <= 0 && signalDeltaPercent > tolerance) // 0.01%
            {
                // longterm says buy as well
                SetHoldings(Symbol, 1.0);
            }
            // of our macd is less than our signal, then let's go short
            else if( holding.Quantity >= 0 && signalDeltaPercent < -tolerance) {
                Liquidate(Symbol);
            }

            // plot both lines
            Plot( "MACD", macd, macd.Signal);
            Plot(Symbol, "Open", data[Symbol].Open);
            Plot(Symbol, macd.Fast, macd.Slow);

            previous = Time;
        }
    }
}