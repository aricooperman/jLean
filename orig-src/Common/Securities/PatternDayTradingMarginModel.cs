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

package com.quantconnect.lean.Securities
{
    /**
     * Represents a simple margining model where margin/leverage depends on market state (open or close).
     * During regular market hours, leverage is 4x, otherwise 2x
    */
    public class PatternDayTradingMarginModel : SecurityMarginModel
    {
        private final BigDecimal _closedMarginCorrectionFactor;

        /**
         * Initializes a new instance of the <see cref="PatternDayTradingMarginModel" />
        */
        public PatternDayTradingMarginModel()
            : this(2.0m, 4.0m) {
        }

        /**
         * Initializes a new instance of the <see cref="PatternDayTradingMarginModel" />
        */
         * @param closedMarketLeverage Leverage used outside regular market hours
         * @param openMarketLeverage Leverage used during regular market hours
        public PatternDayTradingMarginModel( BigDecimal closedMarketLeverage, BigDecimal openMarketLeverage)
            : base(openMarketLeverage) {
            _closedMarginCorrectionFactor = openMarketLeverage/closedMarketLeverage;
        }

        /**
         * Sets the leverage for the applicable securities, i.e, equities
        */
         * 
         * Do nothing, we use a constant leverage for this model
         * 
         * @param security The security to set leverage to
         * @param leverage The new leverage
        public @Override void SetLeverage(Security security, BigDecimal leverage) {
        }

        /**
         * The percentage of an order's absolute cost that must be held in free cash in order to place the order
        */
        protected @Override BigDecimal GetInitialMarginRequirement(Security security) {
            return base.GetInitialMarginRequirement(security)*GetMarginCorrectionFactor(security);
        }

        /**
         * The percentage of the holding's absolute cost that must be held in free cash in order to avoid a margin call
        */
        protected @Override BigDecimal GetMaintenanceMarginRequirement(Security security) {
            return base.GetMaintenanceMarginRequirement(security)*GetMarginCorrectionFactor(security);
        }

        /**
         * Get margin correction factor if not in regular market hours
        */
         * @param security The security to apply conditional leverage to
        @returns The margin correction factor
        private BigDecimal GetMarginCorrectionFactor(Security security) {
            // when the market is open the base type returns the correct values
            // when the market is closed, we need to multiply by a correction factor
            return security.Exchange.ExchangeOpen ? 1m : _closedMarginCorrectionFactor;
        }
    }
}
