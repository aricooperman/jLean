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
using QuantConnect.Orders.Fees;
using QuantConnect.Orders.Fills;
using QuantConnect.Orders.Slippage;
using QuantConnect.Securities.Interfaces;

package com.quantconnect.lean.Securities 
{
    /**
    /// Default security transaction model for user defined securities.
    */
    public class SecurityTransactionModel : ISecurityTransactionModel
    {
        private final IFillModel _fillModel;
        private final IFeeModel _feeModel;
        private final ISlippageModel _slippageModel;

        /**
        /// Initializes a new default instance of the <see cref="SecurityTransactionModel"/> class.
        /// This will use default slippage and fill models.
        */
        public SecurityTransactionModel() {
            _slippageModel = new SpreadSlippageModel();
            _fillModel = new ImmediateFillModel();
            _feeModel = new ConstantFeeModel(0);
        }

        /**
        /// Initializes a new instance of the <see cref="SecurityTransactionManager"/> class
        */
         * @param fillModel">The fill model to use
         * @param feeModel">The order fee model to use
         * @param slippageModel">The slippage model to use
        public SecurityTransactionModel(IFillModel fillModel, IFeeModel feeModel, ISlippageModel slippageModel) {
            _fillModel = fillModel;
            _feeModel = feeModel;
            _slippageModel = slippageModel;
        }

        /**
        /// Default market fill model for the base security class. Fills at the last traded price.
        */
         * @param asset">Security asset we're filling
         * @param order">Order packet to model
        @returns Order fill information detailing the average price and quantity filled.
        /// <seealso cref="StopMarketFill(Security, StopMarketOrder)"/>
        /// <seealso cref="LimitFill(Security, LimitOrder)"/>
        public virtual OrderEvent MarketFill(Security asset, MarketOrder order) {
            return _fillModel.MarketFill(asset, order);
        }

        /**
        /// Default stop fill model implementation in base class security. (Stop Market Order Type)
        */
         * @param asset">Security asset we're filling
         * @param order">Order packet to model
        @returns Order fill information detailing the average price and quantity filled.
        /// <seealso cref="MarketFill(Security, MarketOrder)"/>
        /// <seealso cref="LimitFill(Security, LimitOrder)"/>
        public virtual OrderEvent StopMarketFill(Security asset, StopMarketOrder order) {
            return _fillModel.StopMarketFill(asset, order);
        }

        /**
        /// Default stop limit fill model implementation in base class security. (Stop Limit Order Type)
        */
         * @param asset">Security asset we're filling
         * @param order">Order packet to model
        @returns Order fill information detailing the average price and quantity filled.
        /// <seealso cref="StopMarketFill(Security, StopMarketOrder)"/>
        /// <seealso cref="LimitFill(Security, LimitOrder)"/>
        /// 
        ///     There is no good way to model limit orders with OHLC because we never know whether the market has 
        ///     gapped past our fill price. We have to make the assumption of a fluid, high volume market.
        /// 
        ///     Stop limit orders we also can't be sure of the order of the H - L values for the limit fill. The assumption
        ///     was made the limit fill will be done with closing price of the bar after the stop has been triggered..
        /// 
        public virtual OrderEvent StopLimitFill(Security asset, StopLimitOrder order) {
            return _fillModel.StopLimitFill(asset, order);
        }

        /**
        /// Default limit order fill model in the base security class.
        */
         * @param asset">Security asset we're filling
         * @param order">Order packet to model
        @returns Order fill information detailing the average price and quantity filled.
        /// <seealso cref="StopMarketFill(Security, StopMarketOrder)"/>
        /// <seealso cref="MarketFill(Security, MarketOrder)"/>
        public virtual OrderEvent LimitFill(Security asset, LimitOrder order) {
            return _fillModel.LimitFill(asset, order);
        }

        /**
        /// Market on Open Fill Model. Return an order event with the fill details
        */
         * @param asset">Asset we're trading with this order
         * @param order">Order to be filled
        @returns Order fill information detailing the average price and quantity filled.
        public OrderEvent MarketOnOpenFill(Security asset, MarketOnOpenOrder order) {
            return _fillModel.MarketOnOpenFill(asset, order);
        }

        /**
        /// Market on Close Fill Model. Return an order event with the fill details
        */
         * @param asset">Asset we're trading with this order
         * @param order">Order to be filled
        @returns Order fill information detailing the average price and quantity filled.
        public OrderEvent MarketOnCloseFill(Security asset, MarketOnCloseOrder order) {
            return _fillModel.MarketOnCloseFill(asset, order);
        }

        /**
        /// Get the slippage approximation for this order
        */
         * @param security">Security asset we're filling
         * @param order">Order packet to model
        @returns decimal approximation for slippage
        public virtual BigDecimal GetSlippageApproximation(Security security, Order order) {
            return _slippageModel.GetSlippageApproximation(security, order);
        }

        /**
        /// Default implementation returns 0 for fees.
        */
         * @param security">The security matching the order
         * @param order">The order to compute fees for
        @returns The cost of the order in units of the account currency
        public virtual BigDecimal GetOrderFee(Security security, Order order) {
            return Math.Abs(_feeModel.GetOrderFee(security, order));
        }
    }
}
