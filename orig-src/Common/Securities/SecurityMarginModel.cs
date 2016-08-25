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
using QuantConnect.Orders;

package com.quantconnect.lean.Securities
{
    /**
     * Represents a simple, constant margining model by specifying the percentages of required margin.
    */
    public class SecurityMarginModel : ISecurityMarginModel
    {
        private BigDecimal _initialMarginRequirement;
        private BigDecimal _maintenanceMarginRequirement;

        /**
         * Initializes a new instance of the <see cref="SecurityMarginModel"/>
        */
         * @param initialMarginRequirement The percentage of an order's absolute cost
         * that must be held in free cash in order to place the order
         * @param maintenanceMarginRequirement The percentage of the holding's absolute
         * cost that must be held in free cash in order to avoid a margin call
        public SecurityMarginModel( BigDecimal initialMarginRequirement, BigDecimal maintenanceMarginRequirement) {
            if( initialMarginRequirement < 0 || initialMarginRequirement > 1) {
                throw new IllegalArgumentException( "Initial margin requirement must be between 0 and 1");
            }

            if( maintenanceMarginRequirement < 0 || maintenanceMarginRequirement > 1) {
                throw new IllegalArgumentException( "Maintenance margin requirement must be between 0 and 1");
            }

            _initialMarginRequirement = initialMarginRequirement;
            _maintenanceMarginRequirement = maintenanceMarginRequirement;
        }

        /**
         * Initializes a new instance of the <see cref="SecurityMarginModel"/>
        */
         * @param leverage The leverage
        public SecurityMarginModel( BigDecimal leverage) {
            if( leverage < 1) {
                throw new IllegalArgumentException( "Leverage must be greater than or equal to 1.");
            }

            _initialMarginRequirement = 1/leverage;
            _maintenanceMarginRequirement = 1/leverage;
        }

        /**
         * Gets the current leverage of the security
        */
         * @param security The security to get leverage for
        @returns The current leverage in the security
        public BigDecimal GetLeverage(Security security) {
            return 1/GetMaintenanceMarginRequirement(security);
        }

        /**
         * Sets the leverage for the applicable securities, i.e, equities
        */
         * 
         * This is added to maintain backwards compatibility with the old margin/leverage system
         * 
         * @param security">
         * @param leverage The new leverage
        public void SetLeverage(Security security, BigDecimal leverage) {
            if( leverage < 1) {
                throw new IllegalArgumentException( "Leverage must be greater than or equal to 1.");
            }

            BigDecimal margin = 1/leverage;
            _initialMarginRequirement = margin;
            _maintenanceMarginRequirement = margin;
        }

        /**
         * Gets the total margin required to execute the specified order in units of the account currency including fees
        */
         * @param security The security to compute initial margin for
         * @param order The order to be executed
        @returns The total margin in terms of the currency quoted in the order
        public BigDecimal GetInitialMarginRequiredForOrder(Security security, Order order) {
            //Get the order value from the non-abstract order classes (MarketOrder, LimitOrder, StopMarketOrder)
            //Market order is approximated from the current security price and set in the MarketOrder Method in QCAlgorithm.
            orderFees = security.FeeModel.GetOrderFee(security, order);

            orderValue = order.GetValue(security) * GetInitialMarginRequirement(security);
            return orderValue + Math.Sign(orderValue) * orderFees;
        }

        /**
         * Gets the margin currently alloted to the specified holding
        */
         * @param security The security to compute maintenance margin for
        @returns The maintenance margin required for the 
        public BigDecimal GetMaintenanceMargin(Security security) {
            return security.Holdings.AbsoluteHoldingsCost*GetMaintenanceMarginRequirement(security);
        }

        /**
         * Gets the margin cash available for a trade
        */
         * @param portfolio The algorithm's portfolio
         * @param security The security to be traded
         * @param direction The direction of the trade
        @returns The margin available for the trade
        public BigDecimal GetMarginRemaining(SecurityPortfolioManager portfolio, Security security, OrderDirection direction) {
            holdings = security.Holdings;

            if( direction == OrderDirection.Hold) {
                return portfolio.MarginRemaining;
            }

            //If the order is in the same direction as holdings, our remaining cash is our cash
            //In the opposite direction, our remaining cash is 2 x current value of assets + our cash
            if( holdings.IsLong) {
                switch (direction) {
                    case OrderDirection.Buy:
                        return portfolio.MarginRemaining;

                    case OrderDirection.Sell:
                        return 
                            // portion of margin to close the existing position
                            GetMaintenanceMargin(security) +
                            // portion of margin to open the new position
                            security.Holdings.AbsoluteHoldingsValue * GetInitialMarginRequirement(security) +
                            portfolio.MarginRemaining;
                }
            }
            else if( holdings.IsShort) {
                switch (direction) {
                    case OrderDirection.Buy:
                        return
                            // portion of margin to close the existing position
                            GetMaintenanceMargin(security) +
                            // portion of margin to open the new position
                            security.Holdings.AbsoluteHoldingsValue * GetInitialMarginRequirement(security) +
                            portfolio.MarginRemaining;

                    case OrderDirection.Sell:
                        return portfolio.MarginRemaining;
                }
            }

            //No holdings, return cash
            return portfolio.MarginRemaining;
        }

        /**
         * Generates a new order for the specified security taking into account the total margin
         * used by the account. Returns null when no margin call is to be issued.
        */
         * @param security The security to generate a margin call order for
         * @param netLiquidationValue The net liquidation value for the entire account
         * @param totalMargin The total margin used by the account in units of base currency
        @returns An order object representing a liquidation order to be executed to bring the account within margin requirements
        public SubmitOrderRequest GenerateMarginCallOrder(Security security, BigDecimal netLiquidationValue, BigDecimal totalMargin) {
            // leave a buffer in default implementation
            static final BigDecimal marginBuffer = 0.10m;

            if( totalMargin <= netLiquidationValue*(1 + marginBuffer)) {
                return null;
            }

            if( !security.Holdings.Invested) {
                return null;
            }

            if( security.QuoteCurrency.ConversionRate == BigDecimal.ZERO) {
                // check for div 0 - there's no conv rate, so we can't place an order
                return null;
            }

            // compute the amount of quote currency we need to liquidate in order to get within margin requirements
            deltaInQuoteCurrency = (totalMargin - netLiquidationValue)/security.QuoteCurrency.ConversionRate;

            // compute the number of shares required for the order, rounding up
            unitPriceInQuoteCurrency = security.Price * security.SymbolProperties.ContractMultiplier;
            int quantity = (int) (Math.Round(deltaInQuoteCurrency/unitPriceInQuoteCurrency, MidpointRounding.AwayFromZero)/GetMaintenanceMarginRequirement(security));

            // don't try and liquidate more share than we currently hold, minimum value of 1, maximum value for absolute quantity
            quantity = Math.Max(1, Math.Min((int)security.Holdings.AbsoluteQuantity, quantity));
            if( security.Holdings.IsLong) {
                // adjust to a sell for long positions
                quantity *= -1;
            }

            return new SubmitOrderRequest(OrderType.Market, security.Type, security.Symbol, quantity, 0, 0, security.LocalTime.ConvertToUtc(security.Exchange.TimeZone), "Margin Call");
        }

        /**
         * The percentage of an order's absolute cost that must be held in free cash in order to place the order
        */
        protected BigDecimal GetInitialMarginRequirement(Security security) {
            return _initialMarginRequirement;
        }

        /**
         * The percentage of the holding's absolute cost that must be held in free cash in order to avoid a margin call
        */
        protected BigDecimal GetMaintenanceMarginRequirement(Security security) {
            return _maintenanceMarginRequirement;
        }
    }
}