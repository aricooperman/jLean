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
using System.Collections.Generic;
using QuantConnect.Data.Market;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Orders.Fills;
using QuantConnect.Orders.Slippage;
using QuantConnect.Securities;
using QuantConnect.Securities.Equity;
using QuantConnect.Securities.Option;
using QuantConnect.Util;

package com.quantconnect.lean.Brokerages
{
    /**
     * Provides a default implementation of <see cref="IBrokerageModel"/> that allows all orders and uses
     * the default transaction models
    */
    public class DefaultBrokerageModel : IBrokerageModel
    {
        /**
         * The default markets for the backtesting brokerage
        */
        public static final ImmutableMap<SecurityType,String> DefaultMarketMap = new Map<SecurityType,String>
        {
            {SecurityType.Base, Market.USA},
            {SecurityType.Equity, Market.USA},
            {SecurityType.Option, Market.USA},
            {SecurityType.Forex, Market.FXCM},
            {SecurityType.Cfd, Market.FXCM}
        }.ToReadOnlyDictionary();

        /**
         * Gets or sets the account type used by this model
        */
        public AccountType AccountType
        {
            get; 
            private set;
        }

        /**
         * Gets a map of the default markets to be used for each security type
        */
        public ImmutableMap<SecurityType,String> DefaultMarkets
        {
            get { return DefaultMarketMap; }
        }

        /**
         * Initializes a new instance of the <see cref="DefaultBrokerageModel"/> class
        */
         * @param accountType The type of account to be modelled, defaults to 
         * <see cref="QuantConnect.AccountType.Margin"/>
        public DefaultBrokerageModel(AccountType accountType = AccountType.Margin) {
            AccountType = accountType;
        }

        /**
         * Returns true if the brokerage could accept this order. This takes into account
         * order type, security type, and order size limits.
        */
         * 
         * For example, a brokerage may have no connectivity at certain times, or an order rate/size limit
         * 
         * @param security The security being ordered
         * @param order The order to be processed
         * @param message If this function returns false, a brokerage message detailing why the order may not be submitted
        @returns True if the brokerage could process the order, false otherwise
        public boolean CanSubmitOrder(Security security, Order order, out BrokerageMessageEvent message) {
            message = null;
            return true;
        }

        /**
         * Returns true if the brokerage would allow updating the order as specified by the request
        */
         * @param security The security of the order
         * @param order The order to be updated
         * @param request The requested update to be made to the order
         * @param message If this function returns false, a brokerage message detailing why the order may not be updated
        @returns True if the brokerage would allow updating the order, false otherwise
        public boolean CanUpdateOrder(Security security, Order order, UpdateOrderRequest request, out BrokerageMessageEvent message) {
            message = null;
            return true;
        }

        /**
         * Returns true if the brokerage would be able to execute this order at this time assuming
         * market prices are sufficient for the fill to take place. This is used to emulate the 
         * brokerage fills in backtesting and paper trading. For example some brokerages may not perform
         * executions during extended market hours. This is not intended to be checking whether or not
         * the exchange is open, that is handled in the Security.Exchange property.
        */
         * @param security The security being traded
         * @param order The order to test for execution
        @returns True if the brokerage would be able to perform the execution, false otherwise
        public boolean CanExecuteOrder(Security security, Order order) {
            return true;
        }

        /**
         * Applies the split to the specified order ticket
        */
         * 
         * This default implementation will update the orders to maintain a similar market value
         * 
         * @param tickets The open tickets matching the split event
         * @param split The split event data
        public void ApplySplit(List<OrderTicket> tickets, Split split) {
            // by default we'll just update the orders to have the same notional value
            splitFactor = split.splitFactor;
            tickets.ForEach(ticket -> ticket.Update(new UpdateOrderFields
            {
                Quantity = (OptionalInt) (ticket.Quantity/splitFactor),
                LimitPrice = ticket.OrderType.IsLimitOrder() ? ticket.Get(OrderField.LimitPrice)*splitFactor : (Optional<BigDecimal>) null,
                StopPrice = ticket.OrderType.IsStopOrder() ? ticket.Get(OrderField.StopPrice)*splitFactor : (Optional<BigDecimal>) null
            }));
        }

        /**
         * Gets the brokerage's leverage for the specified security
        */
         * @param security The security's whose leverage we seek
        @returns The leverage for the specified security
        public BigDecimal GetLeverage(Security security) {
            switch (security.Type) {
                case SecurityType.Equity:
                    return 2m;

                case SecurityType.Forex:
                case SecurityType.Cfd:
                    return 50m;

                case SecurityType.Base:
                case SecurityType.Commodity:
                case SecurityType.Option:
                case SecurityType.Future:
                default:
                    return 1m;
            }
        }

        /**
         * Gets a new fill model that represents this brokerage's fill behavior
        */
         * @param security The security to get fill model for
        @returns The new fill model for this brokerage
        public IFillModel GetFillModel(Security security) {
            return new ImmediateFillModel();
        }

        /**
         * Gets a new fee model that represents this brokerage's fee structure
        */
         * @param security The security to get a fee model for
        @returns The new fee model for this brokerage
        public IFeeModel GetFeeModel(Security security) {
            switch (security.Type) {
                case SecurityType.Base:
                    return new ConstantFeeModel(0m);

                case SecurityType.Forex:
                case SecurityType.Equity:
                    return new InteractiveBrokersFeeModel();

                case SecurityType.Commodity:
                case SecurityType.Option:
                case SecurityType.Future:
                case SecurityType.Cfd:
                default:
                    return new ConstantFeeModel(0m);
            }
        }

        /**
         * Gets a new slippage model that represents this brokerage's fill slippage behavior
        */
         * @param security The security to get a slippage model for
        @returns The new slippage model for this brokerage
        public ISlippageModel GetSlippageModel(Security security) {
            switch (security.Type) {
                case SecurityType.Base:
                case SecurityType.Equity:
                    return new ConstantSlippageModel(0);

                case SecurityType.Forex:
                case SecurityType.Cfd:
                    return new SpreadSlippageModel();

                case SecurityType.Commodity:
                case SecurityType.Option:
                case SecurityType.Future:
                default:
                    return new ConstantSlippageModel(0);
            }
        }

        /**
         * Gets a new settlement model for the security
        */
         * @param security The security to get a settlement model for
         * @param accountType The account type
        @returns The settlement model for this brokerage
        public ISettlementModel GetSettlementModel(Security security, AccountType accountType) {
            if( accountType == AccountType.Cash) {
                switch (security.Type) {
                    case SecurityType.Equity:
                        return new DelayedSettlementModel(Equity.DefaultSettlementDays, Equity.DefaultSettlementTime);

                    case SecurityType.Option:
                        return new DelayedSettlementModel(Option.DefaultSettlementDays, Option.DefaultSettlementTime);
                }
            }

            return new ImmediateSettlementModel();
        }

    }
}