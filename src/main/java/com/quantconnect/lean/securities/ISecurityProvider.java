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

package com.quantconnect.lean.securities;

import java.math.BigDecimal;

import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.securities.Security;

/**
 * Represents a type capable of fetching the holdings for the specified symbol
 */
public interface ISecurityProvider {
    /**
     * Retrieves a summary of the holdings for the specified symbol
     * @param symbol">The symbol to get holdings for
     * @returns The holdings for the symbol or null if the symbol is invalid and/or not in the portfolio
     */
    Security getSecurity( Symbol symbol );

    /**
     * Extension method to return the quantity of holdings, if no holdings are present, then zero is returned.
     * @param provider">The <see cref="ISecurityProvider"/>
     * @param symbol">The symbol we want holdings quantity for
     * @returns The quantity of holdings for the specified symbol
     */
    default BigDecimal getHoldingsQuantity( Symbol symbol ) {
        final Security security = getSecurity( symbol );
        return security == null ? BigDecimal.ZERO : security.Holdings.getQuantity();
    }
}
