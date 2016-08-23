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

/**
 *  Represents common properties for a specific security, uniquely identified by market, symbol and security type
 */
public class SymbolProperties {
    
    /**
     *  The description of the security
     */
    private String description;

    /**
     * The quote currency of the security
     */
    private String quoteCurrency;

    /**
     *  The contract multiplier for the security
     */
    private BigDecimal contractMultiplier;

    /**
     * The pip size (tick size) for the security
     */
    private BigDecimal pipSize;

    /**
     *  The lot size (lot size of the order) for the security
     */
    private BigDecimal lotSize;

    
    public String getDescription() {
        return description;
    }

    public String getQuoteCurrency() {
        return quoteCurrency;
    }

    public BigDecimal getContractMultiplier() {
        return contractMultiplier;
    }

    public BigDecimal getPipSize() {
        return pipSize;
    }

    public BigDecimal getLotSize() {
        return lotSize;
    }

    /**
     * Creates an instance of the <see cref="SymbolProperties"/> class
     */
    public SymbolProperties( String description, String quoteCurrency, BigDecimal contractMultiplier, BigDecimal pipSize, BigDecimal lotSize) {
        this.description = description;
        this.quoteCurrency = quoteCurrency;
        this.contractMultiplier = contractMultiplier;
        this.pipSize = pipSize;
        this.lotSize = lotSize;
    }

    /**
     * Gets a default instance of the <see cref="SymbolProperties"/> class for the specified <paramref name="quoteCurrency"/>
     * @param quoteCurrency The quote currency of the symbol
     * @returns A default instance of the <see cref="SymbolProperties"/> class
     */
    public static SymbolProperties getDefault( String quoteCurrency ) {
        return new SymbolProperties( "", quoteCurrency.toUpperCase(), BigDecimal.ONE, new BigDecimal( "0.01" ), BigDecimal.ONE );
    }
}
