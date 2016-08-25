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
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Orders.Fills;
using QuantConnect.Orders.Slippage;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Brokerages
{
    /**
     * Oanda Brokerage Model Implementation for Back Testing.
    */
    public class OandaBrokerageModel : DefaultBrokerageModel
    {
        /**
         * The default markets for the fxcm brokerage
        */
        public new static final ImmutableMap<SecurityType,String> DefaultMarketMap = new Map<SecurityType,String>
        {
            {SecurityType.Base, Market.USA},
            {SecurityType.Equity, Market.USA},
            {SecurityType.Option, Market.USA},
            {SecurityType.Forex, Market.Oanda},
            {SecurityType.Cfd, Market.Oanda}
        }.ToReadOnlyDictionary();

        /**
         * Gets a map of the default markets to be used for each security type
        */
        public @Override ImmutableMap<SecurityType,String> DefaultMarkets
        {
            get { return DefaultMarketMap; }
        }

        /**
         * Initializes a new instance of the <see cref="DefaultBrokerageModel"/> class
        */
         * @param accountType The type of account to be modelled, defaults to 
         * <see cref="QuantConnect.AccountType.Margin"/>
        public OandaBrokerageModel(AccountType accountType = AccountType.Margin)
            : base(accountType) {
        }

        /**
         * Returns true if the brokerage could accept this order. This takes into account
         * order type, security type, and order size limits.
        */
         * 
         * For example, a brokerage may have no connectivity at certain times, or an order rate/size limit
         * 
         * @param security">
         * @param order The order to be processed
         * @param message If this function returns false, a brokerage message detailing why the order may not be submitted
        @returns True if the brokerage could process the order, false otherwise
        public @Override boolean CanSubmitOrder(Security security, Order order, out BrokerageMessageEvent message) {
            message = null;

            // validate security type
            if( security.Type != SecurityType.Forex && security.Type != SecurityType.Cfd) {
                message = new BrokerageMessageEvent(BrokerageMessageType.Warning, "NotSupported",
                    "This model does not support " + security.Type + " security type."
                    );

                return false;
            }

            // validate order type
            if( order.Type != OrderType.Limit && order.Type != OrderType.Market && order.Type != OrderType.StopMarket) {
                message = new BrokerageMessageEvent(BrokerageMessageType.Warning, "NotSupported",
                    "This model does not support " + order.Type + " order type."
                    );

                return false;
            }

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
        public @Override boolean CanExecuteOrder(Security security, Order order) {
            return order.DurationValue == DateTime.MaxValue || order.DurationValue <= order.Time.AddMonths(3);
        }

        /**
         * Gets a new fill model that represents this brokerage's fill behavior
        */
         * @param security The security to get fill model for
        @returns The new fill model for this brokerage
        public @Override IFillModel GetFillModel(Security security) {
            return new ImmediateFillModel();
        }

        /**
         * Gets a new fee model that represents this brokerage's fee structure
        */
         * @param security The security to get a fee model for
        @returns The new fee model for this brokerage
        public @Override IFeeModel GetFeeModel(Security security) {
            return new ConstantFeeModel(0m);
        }

        /**
         * Gets a new slippage model that represents this brokerage's fill slippage behavior
        */
         * @param security The security to get a slippage model for
        @returns The new slippage model for this brokerage
        public @Override ISlippageModel GetSlippageModel(Security security) {
            return new SpreadSlippageModel();
        }
    }
}