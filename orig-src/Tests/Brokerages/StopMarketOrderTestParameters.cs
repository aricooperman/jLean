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
using QuantConnect.Interfaces;
using QuantConnect.Orders;

package com.quantconnect.lean.Tests.Brokerages
{
    public class StopMarketOrderTestParameters : OrderTestParameters
    {
        private final BigDecimal _highLimit;
        private final BigDecimal _lowLimit;

        public StopMarketOrderTestParameters(Symbol symbol, BigDecimal highLimit, BigDecimal lowLimit)
            : base(symbol) {
            _highLimit = highLimit;
            _lowLimit = lowLimit;
        }

        public @Override Order CreateShortOrder(int quantity) {
            return new StopMarketOrder(Symbol, -Math.Abs(quantity), _lowLimit, DateTime.Now);
        }

        public @Override Order CreateLongOrder(int quantity) {
            return new StopMarketOrder(Symbol, Math.Abs(quantity), _highLimit, DateTime.Now);
        }

        public @Override boolean ModifyOrderToFill(IBrokerage brokerage, Order order, BigDecimal lastMarketPrice) {
            stop = (StopMarketOrder)order;
            previousStop = stop.StopPrice;
            if( order.Quantity > 0) {
                // for stop buys we need to decrease the stop price
                stop.StopPrice = Math.Min(stop.StopPrice, Math.Max(stop.StopPrice / 2, lastMarketPrice));
            }
            else
            {
                // for stop sells we need to increase the stop price
                stop.StopPrice = Math.Max(stop.StopPrice, Math.Min(stop.StopPrice * 2, lastMarketPrice));
            }
            return stop.StopPrice != previousStop;
        }

        public @Override OrderStatus ExpectedStatus
        {
            // default limit orders will only be submitted, not filled
            get { return OrderStatus.Submitted; }
        }
    }
}