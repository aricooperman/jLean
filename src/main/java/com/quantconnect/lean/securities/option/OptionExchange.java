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

package com.quantconnect.lean.securities.option;

import com.quantconnect.lean.Global;
import com.quantconnect.lean.securities.SecurityExchange;
import com.quantconnect.lean.securities.SecurityExchangeHours;

/**
 * Option exchange class - information and helper tools for option exchange properties
 * <seealso cref="SecurityExchange"/>
 */
public class OptionExchange extends SecurityExchange {
    /**
     * Number of trading days per year for this security, 252.
     * Used for performance statistics to calculate sharpe ratio accurately
     */
    @Override
    public int getTradingDaysPerYear() {
        return Global.TRADING_DAYS_PER_YEAR;
    }

    /**
     * Initializes a new instance of the <see cref="OptionExchange"/> class using the specified
     * exchange hours to determine open/close times
     * @param exchangeHours Contains the weekly exchange schedule plus holidays
     */
    public OptionExchange( SecurityExchangeHours exchangeHours ) {
        super( exchangeHours );
    }
}