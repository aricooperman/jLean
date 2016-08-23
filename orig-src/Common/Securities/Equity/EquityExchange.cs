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

package com.quantconnect.lean.Securities.Equity
{
    /**
    /// Equity exchange information 
    */
    /// <seealso cref="SecurityExchange"/>
    public class EquityExchange : SecurityExchange
    {
        /**
        /// Number of trading days in an equity calendar year - 252
        */
        public @Override int TradingDaysPerYear
        {
            get { return 252; }
        }

        /**
        /// Initializes a new instance of the <see cref="EquityExchange"/> class using market hours
        /// derived from the market-hours-database for the USA Equity market
        */
        public EquityExchange()
            : base(MarketHoursDatabase.FromDataFolder().GetExchangeHours(Market.USA, null, SecurityType.Equity, TimeZones.NewYork)) {
        }

        /**
        /// Initializes a new instance of the <see cref="EquityExchange"/> class using the specified
        /// exchange hours to determine open/close times
        */
         * @param exchangeHours">Contains the weekly exchange schedule plus holidays
        public EquityExchange(SecurityExchangeHours exchangeHours)
            : base(exchangeHours) {
        }
    }
}