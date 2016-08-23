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
using QuantConnect.Data.Consolidators;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Algorithm.Examples
{
    /**
    /// This algorithm shows how to initialize and use the RenkoConsolidator
    */
    public class RenkoConsolidatorAlgorithm : QCAlgorithm
    {
        /**
        /// Initializes the algorithm state.
        */
        public @Override void Initialize() {
            SetStartDate(2012, 01, 01);
            SetEndDate(2013, 01, 01);

            AddSecurity(SecurityType.Equity, "SPY");

            // this is the simple constructor that will perform the renko logic to the Value
            // property of the data it receives.

            // break SPY into $2.5 renko bricks and send that data to our 'OnRenkoBar' method
            renkoClose = new RenkoConsolidator(2.5m);
            renkoClose.DataConsolidated += (sender, consolidated) =>
            {
                // call our event handler for renko data
                HandleRenkoClose(consolidated);
            };

            // register the consolidator for updates
            SubscriptionManager.AddConsolidator( "SPY", renkoClose);


            // this is the full constructor that can accept a value selector and a volume selector
            // this allows us to perform the renko logic on values other than Close, even computed values!

            // break SPY into (2*o + h + l + 3*c)/7
            renko7bar = new RenkoConsolidator<TradeBar>(2.5m, x -> (2*x.Open + x.High + x.Low + 3*x.Close)/7m, x -> x.Volume);
            renko7bar.DataConsolidated += (sender, consolidated) =>
            {
                HandleRenko7Bar(consolidated);
            };

            // register the consolidator for updates
            SubscriptionManager.AddConsolidator( "SPY", renko7bar);
        }

        /**
        /// We're doing our analysis in the OnRenkoBar method, but the framework verifies that this method exists, so we define it.
        */
        public void OnData(TradeBars data) {
        }

        /**
        /// This function is called by our renkoClose consolidator defined in Initialize()
        */
         * @param data">The new renko bar produced by the consolidator
        public void HandleRenkoClose(RenkoBar data) {
            if( !Portfolio.Invested) {
                SetHoldings(data.Symbol, 1.0);
            }
            Console.WriteLine( "CLOSE - %1$s - %2$s %3$s", data.Time.toString( "o"), data.Open, data.Close);
        }

        /**
        /// This function is called by our renko7bar onsolidator defined in Initialize()
        */
         * @param data">The new renko bar produced by the consolidator
        public void HandleRenko7Bar(RenkoBar data) {
            Console.WriteLine( "7BAR  - %1$s - %2$s %3$s", data.Time.toString( "o"), data.Open, data.Close);
        }
    }
}
