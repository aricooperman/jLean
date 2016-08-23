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

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
    /// This algorithm shows how you can define your own custom models.
    */
    public class CustomModelsAlgorithm : QCAlgorithm
    {
        private Security _security;
        public @Override void Initialize() {
            SetStartDate(2012, 01, 01);
            SetEndDate(2012, 02, 01);
            AddSecurity(SecurityType.Equity, "SPY", Resolution.Hour);

            // set our models
            _security = Securities["SPY"];
            _security.FeeModel = new CustomFeeModel(this);
            _security.FillModel = new CustomFillModel(this);
            _security.SlippageModel = new CustomSlippageModel(this);
        }

        public void OnData(TradeBars data) {
            openOrders = Transactions.GetOpenOrders( "SPY");
            if( openOrders.Count != 0) return;

            if( Time.Day > 10 && _security.Holdings.Quantity <= 0) {
                quantity = CalculateOrderQuantity( "SPY", .5m);
                Log( "MarketOrder: " + quantity);
                MarketOrder( "SPY", quantity, asynchronous: true); // async needed for partial fill market orders
            }
            else if( Time.Day > 20 && _security.Holdings.Quantity >= 0) {
                quantity = CalculateOrderQuantity( "SPY", -.5m);
                Log( "MarketOrder: " + quantity);
                MarketOrder( "SPY", quantity, asynchronous: true); // async needed for partial fill market orders
            }
        }

        public class CustomFillModel : ImmediateFillModel
        {
            private final QCAlgorithm _algorithm;
            private final Random _random = new Random(387510346); // seed it for reproducibility
            private final Map<long, decimal> _absoluteRemainingByOrderId = new Map<long, decimal>();

            public CustomFillModel(QCAlgorithm algorithm) {
                _algorithm = algorithm;
            }

            public @Override OrderEvent MarketFill(Security asset, MarketOrder order) {
                // this model randomly fills market orders

                BigDecimal absoluteRemaining;
                if( !_absoluteRemainingByOrderId.TryGetValue(order.Id, out absoluteRemaining)) {
                    absoluteRemaining = order.AbsoluteQuantity;
                    _absoluteRemainingByOrderId.Add(order.Id, order.AbsoluteQuantity);
                }

                fill = base.MarketFill(asset, order);
                absoluteFillQuantity = (int) (Math.Min(absoluteRemaining, _random.Next(0, 2*(int)order.AbsoluteQuantity)));
                fill.FillQuantity = Math.Sign(order.Quantity) * absoluteFillQuantity;

                if( absoluteRemaining == absoluteFillQuantity) {
                    fill.Status = OrderStatus.Filled;
                    _absoluteRemainingByOrderId.Remove(order.Id);
                }
                else
                {
                    absoluteRemaining = absoluteRemaining - absoluteFillQuantity;
                    _absoluteRemainingByOrderId[order.Id] = absoluteRemaining;
                    fill.Status = OrderStatus.PartiallyFilled;
                }

                _algorithm.Log( "CustomFillModel: " + fill);

                return fill;
            }
        }

        public class CustomFeeModel : IFeeModel
        {
            private final QCAlgorithm _algorithm;

            public CustomFeeModel(QCAlgorithm algorithm) {
                _algorithm = algorithm;
            }

            public BigDecimal GetOrderFee(Security security, Order order) {
                // custom fee math
                fee = Math.Max(1m, security.Price*order.AbsoluteQuantity*0.00001m);

                _algorithm.Log( "CustomFeeModel: " + fee);
                return fee;
            }
        }

        public class CustomSlippageModel : ISlippageModel
        {
            private final QCAlgorithm _algorithm;

            public CustomSlippageModel(QCAlgorithm algorithm) {
                _algorithm = algorithm;
            }

            public BigDecimal GetSlippageApproximation(Security asset, Order order) {
                // custom slippage math
                slippage = asset.Price*0.0001m*(decimal) Math.Log10(2*(double) order.AbsoluteQuantity);

                _algorithm.Log( "CustomSlippageModel: " + slippage);
                return slippage;
            }
        }
    }
}
