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
import java.time.LocalDateTime;

/**
 * Represents the model responsible for applying cash settlement rules
 */
public interface ISettlementModel {
    /**
     * Applies cash settlement rules
     * @param portfolio The algorithm's portfolio
     * @param security The fill's security
     * @param applicationTimeUtc The fill time (in UTC)
     * @param currency The currency symbol
     * @param amount The amount of cash to apply
     */
    void applyFunds( SecurityPortfolioManager portfolio, Security security, LocalDateTime applicationTimeUtc, String currency, BigDecimal amount );
}
