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

using System;
using System.Collections.Generic;
using QuantConnect.Data.Market;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Orders.Fills;
using QuantConnect.Orders.Slippage;
using QuantConnect.Securities;

package com.quantconnect.lean.Brokerages
{
    /**
    /// Models brokerage transactions, fees, and order
    */
    public interface IBrokerageModel
    {
        /**
        /// Gets or sets the account type used by this model
        */
        AccountType AccountType
        {
            get;
        }

        /**
        /// Gets a map of the default markets to be used for each security type
        */
        IReadOnlyMap<SecurityType,String> DefaultMarkets { get; }

        /**
        /// Returns true if the brokerage could accept this order. This takes into account
        /// order type, security type, and order size limits.
        */
        /// 
        /// For example, a brokerage may have no connectivity at certain times, or an order rate/size limit
        /// 
         * @param security">The security being ordered
         * @param order">The order to be processed
         * @param message">If this function returns false, a brokerage message detailing why the order may not be submitted
        @returns True if the brokerage could process the order, false otherwise
        boolean CanSubmitOrder(Security security, Order order, out BrokerageMessageEvent message);

        /**
        /// Returns true if the brokerage would allow updating the order as specified by the request
        */
         * @param security">The security of the order
         * @param order">The order to be updated
         * @param request">The requested updated to be made to the order
         * @param message">If this function returns false, a brokerage message detailing why the order may not be updated
        @returns True if the brokerage would allow updating the order, false otherwise
        boolean CanUpdateOrder(Security security, Order order, UpdateOrderRequest request, out BrokerageMessageEvent message);

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
        boolean CanExecuteOrder(Security security, Order order);

        /**
        /// Applies the split to the specified order ticket
        */
         * @param tickets">The open tickets matching the split event
         * @param split">The split event data
        void ApplySplit(List<OrderTicket> tickets, Split split);

        /**
        /// Gets the brokerage's leverage for the specified security
        */
         * @param security">The security's whose leverage we seek
        @returns The leverage for the specified security
        BigDecimal GetLeverage(Security security);

        /**
        /// Gets a new fill model that represents this brokerage's fill behavior
        */
         * @param security">The security to get fill model for
        @returns The new fill model for this brokerage
        IFillModel GetFillModel(Security security);

        /**
        /// Gets a new fee model that represents this brokerage's fee structure
        */
         * @param security">The security to get a fee model for
        @returns The new fee model for this brokerage
        IFeeModel GetFeeModel(Security security);

        /**
        /// Gets a new slippage model that represents this brokerage's fill slippage behavior
        */
         * @param security">The security to get a slippage model for
        @returns The new slippage model for this brokerage
        ISlippageModel GetSlippageModel(Security security);

        /**
        /// Gets a new settlement model for the security
        */
         * @param security">The security to get a settlement model for
         * @param accountType">The account type
        @returns The settlement model for this brokerage
        ISettlementModel GetSettlementModel(Security security, AccountType accountType);

    }

    /**
    /// Provides factory method for creating an <see cref="IBrokerageModel"/> from the <see cref="BrokerageName"/> enum
    */
    public static class BrokerageModel
    {
        /**
        /// Creates a new <see cref="IBrokerageModel"/> for the specified <see cref="BrokerageName"/>
        */
         * @param brokerage">The name of the brokerage
         * @param accountType">The account type
        @returns The model for the specified brokerage
        public static IBrokerageModel Create(BrokerageName brokerage, AccountType accountType) {
            switch (brokerage) {
                case BrokerageName.Default:
                    return new DefaultBrokerageModel(accountType);

                case BrokerageName.InteractiveBrokersBrokerage:
                    return new InteractiveBrokersBrokerageModel(accountType);

                case BrokerageName.TradierBrokerage:
                    return new TradierBrokerageModel(accountType);
                    
                case BrokerageName.OandaBrokerage:
                    return new OandaBrokerageModel(accountType);
                    
                case BrokerageName.FxcmBrokerage:
                    return new FxcmBrokerageModel(accountType);
                    
                default:
                    throw new ArgumentOutOfRangeException( "brokerage", brokerage, null );
            }
        }
    }
}
