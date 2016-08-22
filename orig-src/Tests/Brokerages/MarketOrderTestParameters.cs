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
    public class MarketOrderTestParameters : OrderTestParameters
    {
        public MarketOrderTestParameters(Symbol symbol)
            : base(symbol) {
        }

        public @Override Order CreateShortOrder(int quantity) {
            return new MarketOrder(Symbol, -Math.Abs(quantity), DateTime.Now);
        }

        public @Override Order CreateLongOrder(int quantity) {
            return new MarketOrder(Symbol, Math.Abs(quantity), DateTime.Now);
        }

        public @Override boolean ModifyOrderToFill(IBrokerage brokerage, Order order, BigDecimal lastMarketPrice) {
            // NOP
            // market orders should fill without modification
            return false;
        }

        public @Override OrderStatus ExpectedStatus
        {
            // all market orders should fill
            get { return OrderStatus.Filled; }
        }
    }
}