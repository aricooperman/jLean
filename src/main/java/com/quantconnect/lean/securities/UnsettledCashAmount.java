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
 *
*/

package com.quantconnect.lean.securities;

import java.math.BigDecimal;
import java.time.LocalDateTime;

/**
 * Represents a pending cash amount waiting for settlement time
 */
public class UnsettledCashAmount {
    
    /**
     * The settlement time (in UTC)
     */
    private LocalDateTime settlementTimeUtc;

    /**
     * The currency symbol
     */
    private String currency;

    /**
     * The amount of cash
     */
    private BigDecimal amount;

    /**
     * Creates a new instance of the <see cref="UnsettledCashAmount"/> class
     */
    public UnsettledCashAmount( LocalDateTime settlementTimeUtc, String currency, BigDecimal amount ) {
        this.settlementTimeUtc = settlementTimeUtc;
        this.currency = currency;
        this.amount = amount;
    }

    public LocalDateTime getSettlementTimeUtc() {
        return settlementTimeUtc;
    }
    
    public String getCurrency() {
        return currency;
    }
    
    public BigDecimal getAmount() {
        return amount;
    }
}
