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

using System.Collections.Generic;
using QuantConnect.Data.Market;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Orders.Fills;
using QuantConnect.Orders.Slippage;
using QuantConnect.Securities;
using QuantConnect.Securities.Equity;

package com.quantconnect.lean.Brokerages
{
    /**
    /// Provides tradier specific properties
    */
    public class TradierBrokerageModel : DefaultBrokerageModel
    {
        private static final EquityExchange EquityExchange = 
            new EquityExchange(MarketHoursDatabase.FromDataFolder().GetExchangeHours(Market.USA, null, SecurityType.Equity, TimeZones.NewYork));
        
        /**
        /// Initializes a new instance of the <see cref="DefaultBrokerageModel"/> class
        */
         * @param accountType">The type of account to be modelled, defaults to 
        /// <see cref="QuantConnect.AccountType.Margin"/>
        public TradierBrokerageModel(AccountType accountType = AccountType.Margin)
            : base(accountType) {
        }

        /**
        /// Returns true if the brokerage could accept this order. This takes into account
        /// order type, security type, and order size limits.
        */
        /// 
        /// For example, a brokerage may have no connectivity at certain times, or an order rate/size limit
        /// 
         * @param security">The security of the order
         * @param order">The order to be processed
         * @param message">If this function returns false, a brokerage message detailing why the order may not be submitted
        @returns True if the brokerage could process the order, false otherwise
        public @Override boolean CanSubmitOrder(Security security, Order order, out BrokerageMessageEvent message) {
            message = null;

            securityType = order.SecurityType;
            if( securityType != SecurityType.Equity) {
                message = new BrokerageMessageEvent(BrokerageMessageType.Warning, "NotSupported",
                    "This model only supports equities."
                    );
                
                return false;
            }

            if( !CanExecuteOrder(security, order)) {
                message = new BrokerageMessageEvent(BrokerageMessageType.Warning, "ExtendedMarket",
                    "Tradier does not support extended market hours trading.  Your order will be processed at market open."
                    );
            }

            // tradier order limits
            return true;
        }

        /**
        /// Returns true if the brokerage would allow updating the order as specified by the request
        */
         * @param security">The security of the order
         * @param order">The order to be updated
         * @param request">The requested update to be made to the order
         * @param message">If this function returns false, a brokerage message detailing why the order may not be updated
        @returns True if the brokerage would allow updating the order, false otherwise
        public @Override boolean CanUpdateOrder(Security security, Order order, UpdateOrderRequest request, out BrokerageMessageEvent message) {
            message = null;

            // Tradier doesn't allow updating order quantities
            if( request.Quantity != null && request.Quantity != order.Quantity) {
                message = new BrokerageMessageEvent(BrokerageMessageType.Warning, "UpdateRejected",
                    "Traider does not support updating order quantities."
                    );

                return false;
            }

            return true;
        }

        /**
        /// Returns true if the brokerage would be able to execute this order at this time assuming
        /// market prices are sufficient for the fill to take place. This is used to emulate the 
        /// brokerage fills in backtesting and paper trading. For example some brokerages may not perform
        /// executions during extended market hours. This is not intended to be checking whether or not
        /// the exchange is open, that is handled in the Security.Exchange property.
        */
         * @param security">The security being ordered
         * @param order">The order to test for execution
        @returns True if the brokerage would be able to perform the execution, false otherwise
        public @Override boolean CanExecuteOrder(Security security, Order order) {
            EquityExchange.SetLocalDateTimeFrontier(security.Exchange.LocalTime);

            cache = security.GetLastData();
            if( cache == null ) {
                return false;
            }

            // tradier doesn't support after hours trading
            if( !EquityExchange.IsOpenDuringBar(cache.Time, cache.EndTime, false)) {
                return false;
            }
            return true;
        }

        /**
        /// Applies the split to the specified order ticket
        */
         * @param tickets">The open tickets matching the split event
         * @param split">The split event data
        public @Override void ApplySplit(List<OrderTicket> tickets, Split split) {
            // tradier cancels reverse splits
            splitFactor = split.splitFactor;
            if( splitFactor > 1.0m) {
                tickets.ForEach(ticket -> ticket.Cancel( "Tradier Brokerage cancels open orders on reverse split symbols"));
            }
            else
            {
                base.ApplySplit(tickets, split);
            }
        }

        /**
        /// Gets a new fill model that represents this brokerage's fill behavior
        */
         * @param security">The security to get fill model for
        @returns The new fill model for this brokerage
        public @Override IFillModel GetFillModel(Security security) {
            return new ImmediateFillModel();
        }

        /**
        /// Gets a new fee model that represents this brokerage's fee structure
        */
         * @param security">The security to get a fee model for
        @returns The new fee model for this brokerage
        public @Override IFeeModel GetFeeModel(Security security) {
            return new ConstantFeeModel(1m);
        }

        /**
        /// Gets a new slippage model that represents this brokerage's fill slippage behavior
        */
         * @param security">The security to get a slippage model for
        @returns The new slippage model for this brokerage
        public @Override ISlippageModel GetSlippageModel(Security security) {
            return new SpreadSlippageModel();
        }

    }
}
