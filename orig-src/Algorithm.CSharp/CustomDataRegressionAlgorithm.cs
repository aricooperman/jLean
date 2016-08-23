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

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
    /// Regression algorithm for custom data
    */
    public class CustomDataRegressionAlgorithm : QCAlgorithm
    {
        /**
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        */
        public @Override void Initialize() {
            SetStartDate(2011, 9, 13);
            SetEndDate(2015, 12, 01);

            //Set the cash for the strategy:
            SetCash(100000);

            //Define the symbol and "type" of our generic data:
            resolution = LiveMode ? Resolution.Second : Resolution.Daily;
            AddData<Bitcoin>( "BTC", resolution);
        }

        /**
        /// Event Handler for Bitcoin Data Events: These Bitcoin objects are created from our 
        /// "Bitcoin" type below and fired into this event handler.
        */
         * @param data">One(1) Bitcoin Object, streamed into our algorithm synchronised in time with our other data streams
        public void OnData(Bitcoin data) {
            //If we don't have any bitcoin "SHARES" -- invest"
            if( !Portfolio.Invested) {
                //Bitcoin used as a tradable asset, like stocks, futures etc. 
                if( data.Close != 0) {
                    Order( "BTC", (Portfolio.Cash / Math.Abs(data.Close + 1)));
                }
            }
        }
    }
}