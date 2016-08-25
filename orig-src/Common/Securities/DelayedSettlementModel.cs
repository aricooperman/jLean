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

package com.quantconnect.lean.Securities
{
    /**
     * Represents the model responsible for applying cash settlement rules
    */
     * This model applies cash settlement after T+N days
    public class DelayedSettlementModel : ISettlementModel
    {
        private final int _numberOfDays;
        private final Duration _timeOfDay;

        /**
         * Creates an instance of the <see cref="DelayedSettlementModel"/> class
        */
         * @param numberOfDays The number of days required for settlement
         * @param timeOfDay The time of day used for settlement
        public DelayedSettlementModel(int numberOfDays, Duration timeOfDay) {
            _numberOfDays = numberOfDays;
            _timeOfDay = timeOfDay;
        }

        /**
         * Applies cash settlement rules
        */
         * @param portfolio The algorithm's portfolio
         * @param security The fill's security
         * @param applicationTimeUtc The fill time (in UTC)
         * @param currency The currency symbol
         * @param amount The amount of cash to apply
        public void ApplyFunds(SecurityPortfolioManager portfolio, Security security, DateTime applicationTimeUtc, String currency, BigDecimal amount) {
            if( amount > 0) {
                // positive amount: sell order filled

                portfolio.UnsettledCashBook[currency].AddAmount(amount);

                // find the correct settlement date (usually T+3 or T+1)
                settlementDate = applicationTimeUtc.ConvertFromUtc(security.Exchange.TimeZone).Date;
                for (i = 0; i < _numberOfDays; i++) {
                    settlementDate = settlementDate.AddDays(1);

                    // only count days when market is open
                    if( !security.Exchange.Hours.IsDateOpen(settlementDate))
                        i--;
                }

                // use correct settlement time
                settlementTimeUtc = settlementDate.Add(_timeOfDay).ConvertToUtc(security.Exchange.Hours.TimeZone);

                portfolio.AddUnsettledCashAmount(new UnsettledCashAmount(settlementTimeUtc, currency, amount));
            }
            else
            {
                // negative amount: buy order filled

                portfolio.CashBook[currency].AddAmount(amount);
            }
        }
    }
}
