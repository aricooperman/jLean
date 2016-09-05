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

package com.quantconnect.lean.securities.forex;

import com.quantconnect.lean.Global;
import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.Market;
import com.quantconnect.lean.securities.MarketHoursDatabase;
import com.quantconnect.lean.securities.SecurityExchange;
import com.quantconnect.lean.securities.SecurityExchangeHours;

/**
 * Forex exchange class - information and helper tools for forex exchange properties
 * <seealso cref="SecurityExchange"/>
 */
public class ForexExchange extends SecurityExchange {
    
    /**
     * Number of trading days per year for this security, used for performance statistics.
     */
    @Override
    public int getTradingDaysPerYear() {
        // 365 - Saturdays = 313;
        return 313;
    }

    /**
     * Initializes a new instance of the <see cref="ForexExchange"/> class using market hours
     * derived from the market-hours-database for the FXCM Forex market
     */
    public ForexExchange() {
        this( MarketHoursDatabase.fromDataFolder().getExchangeHours( Market.FXCM, null, SecurityType.Forex, Global.NEW_YORK_TZ_ID ) );
    }

    /**
     * Initializes a new instance of the <see cref="ForexExchange"/> class using the specified
     * exchange hours to determine open/close times
     * @param exchangeHours Contains the weekly exchange schedule plus holidays
     */
    public ForexExchange( SecurityExchangeHours exchangeHours ) {
        super( exchangeHours );
    }
}