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

package com.quantconnect.lean.Securities.Cfd
{
    /**
     * CFD exchange class - information and helper tools for CFD exchange properties
    */
     * <seealso cref="SecurityExchange"/>
    public class CfdExchange : SecurityExchange
    {
        /**
         * Number of trading days per year for this security, used for performance statistics.
        */
        public @Override int TradingDaysPerYear
        {
            // 365 - Saturdays = 313;
            get { return 313; }
        }

        /**
         * Initializes a new instance of the <see cref="CfdExchange"/> class using the specified
         * exchange hours to determine open/close times
        */
         * @param exchangeHours Contains the weekly exchange schedule plus holidays
        public CfdExchange(SecurityExchangeHours exchangeHours)
            : base(exchangeHours) {
        }
    }
}
