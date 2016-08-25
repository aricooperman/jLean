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

using System;
using QuantConnect.Data;

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
     * Basic template algorithm simply initializes the date range and cash
    */
    public class LimitFillRegressionAlgorithm : QCAlgorithm
    {
        /**
         * Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        */
        public @Override void Initialize() {
            SetStartDate(2013, 10, 07);  //Set Start Date
            SetEndDate(2013, 10, 11);    //Set End Date
            SetCash(100000);             //Set Strategy Cash
            // Find more symbols here: http://quantconnect.com/data
            AddSecurity(SecurityType.Equity, "SPY", Resolution.Second);
        }

        /**
         * OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        */
         * @param data TradeBars IDictionary object with your stock data
        public @Override void OnData(Slice data) {
            if( data.Bars.ContainsKey( "SPY")) {
                if( Time.TimeOfDay.Ticks%Duration.ofHours(1).Ticks == 0) {
                    boolean goLong = Time < StartDate + Duration.ofTicks((EndDate - StartDate).Ticks/2);
                    int negative = goLong ? 1 : -1;
                    LimitOrder( "SPY", negative*10, data["SPY"].Price);
                }
            }
        }
    }
}