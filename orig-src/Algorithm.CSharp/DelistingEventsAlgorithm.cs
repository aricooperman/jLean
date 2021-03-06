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
 *
*/

using System;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Orders;

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
     * Showcases the delisting event of QCAlgorithm
    */
     * 
     * The data for this algorithm isn't in the github repo, so this will need to be run on the QC site
     * 
    public class DelistingEventsAlgorithm : QCAlgorithm
    {
        /**
         * Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        */
        public @Override void Initialize() {
            SetStartDate(2007, 05, 16);  //Set Start Date
            SetEndDate(2007, 05, 25);    //Set End Date
            SetCash(100000);             //Set Strategy Cash
            // Find more symbols here: http://quantconnect.com/data
            AddSecurity(SecurityType.Equity, "AAA", Resolution.Daily);
            AddSecurity(SecurityType.Equity, "SPY", Resolution.Daily);
        }

        /**
         * OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        */
         * @param data Slice object keyed by symbol containing the stock data
        public @Override void OnData(Slice data) {
            if( Transactions.OrdersCount == 0) {
                SetHoldings( "AAA", 1);
                Debug( "Purchased Stock");
            }

            foreach (kvp in data.Bars) {
                symbol = kvp.Key;
                tradeBar = kvp.Value;
                Console.WriteLine( "OnData(Slice): %1$s: %2$s: %3$s", Time, symbol, tradeBar.Close.toString( "0.00"));
            }

            // the slice can also contain delisting data: data.Delistings in a dictionary string->Delisting
        }

        public void OnData(Delistings data) {
            foreach (kvp in data) {
                symbol = kvp.Key;
                delisting = kvp.Value;
                if( delisting.Type == DelistingType.Warning) {
                    Console.WriteLine( "OnData(Delistings): %1$s: %2$s will be delisted at end of day today.", Time, symbol);
                }
                if( delisting.Type == DelistingType.Delisted) {
                    Console.WriteLine( "OnData(Delistings): %1$s: %2$s has been delisted.", Time, symbol);
                }
            }
        }

        public @Override void OnOrderEvent(OrderEvent orderEvent) {
            Console.WriteLine( "OnOrderEvent(OrderEvent): %1$s: %2$s", Time, orderEvent);
        }
    }
}
