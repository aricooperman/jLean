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
using QuantConnect.Indicators;

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
     * This algorithm demonstrates using the history provider to retrieve data
     * to warm up indicators before data is received
    */
    public class WarmupHistoryAlgorithm : QCAlgorithm
    {
        private ExponentialMovingAverage fast, slow;

        /**
         * Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        */
        public @Override void Initialize() {
            SetStartDate(2013, 10, 07);  //Set Start Date
            SetEndDate(2013, 10, 11);    //Set End Date
            SetCash(100000);             //Set Strategy Cash
            // Find more symbols here: http://quantconnect.com/data
            AddSecurity(SecurityType.Forex, "EURUSD", Resolution.Second);

            fast = EMA( "EURUSD", 60);
            slow = EMA( "EURUSD", 3600);

            // 3601 because rolling window waits for one to fall off the back to be considered ready
            history = History( "EURUSD", 3601);
            foreach (bar in history) {
                fast.Update(bar.EndTime, bar.Close);
                slow.Update(bar.EndTime, bar.Close);
            }

            Log( String.format( "FAST IS %1$s READY. Samples: %2$s", fast.IsReady ? "" : "NOT", fast.Samples));
            Log( String.format( "SLOW IS %1$s READY. Samples: %2$s", slow.IsReady ? "" : "NOT", slow.Samples));
        }

        /**
         * OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        */
         * @param data Slice object keyed by symbol containing the stock data
        public @Override void OnData(Slice data) {
            if( fast > slow) {
                SetHoldings( "EURUSD", 1);
            }
            else
            {
                SetHoldings( "EURUSD", -1);
            }
        }
    }
}