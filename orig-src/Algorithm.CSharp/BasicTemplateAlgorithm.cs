/*
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

using QuantConnect.Data;

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
    /// Basic template algorithm simply initializes the date range and cash
    */
    public class BasicTemplateAlgorithm : QCAlgorithm
    {
        /**
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        */
        public @Override void Initialize() {
            SetStartDate(2013, 10, 07);  //Set Start Date
            SetEndDate(2013, 10, 11);    //Set End Date
            SetCash(100000);             //Set Strategy Cash
            // Find more symbols here: http://quantconnect.com/data
            AddEquity( "SPY", Resolution.Second);
        }

        /**
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        */
         * @param data">Slice object keyed by symbol containing the stock data
        public @Override void OnData(Slice data) {
            if( !Portfolio.Invested) {
                SetHoldings( "SPY", 1);
                Debug( "Purchased Stock");
            }
        }
    }
}