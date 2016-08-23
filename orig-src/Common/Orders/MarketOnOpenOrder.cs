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
using QuantConnect.Securities;

package com.quantconnect.lean.Orders
{
    /**
    /// Market on Open order type, submits a market order when the exchange opens
    */
    public class MarketOnOpenOrder : Order
    {
        /**
        /// MarketOnOpen Order Type
        */
        public @Override OrderType Type
        {
            get { return OrderType.MarketOnOpen; }
        }

        /**
        /// Intiializes a new instance of the <see cref="MarketOnOpenOrder"/> class.
        */
        public MarketOnOpenOrder() {
        }

        /**
        /// Intiializes a new instance of the <see cref="MarketOnOpenOrder"/> class.
        */
         * @param symbol">The security's symbol being ordered
         * @param quantity">The number of units to order
         * @param time">The current time
         * @param tag">A user defined tag for the order
        public MarketOnOpenOrder(Symbol symbol, int quantity, DateTime time, String tag = "")
            : base(symbol, quantity, time, tag) {
        }

        /**
        /// Gets the order value in units of the security's quote currency
        */
         * @param security">The security matching this order's symbol
        protected @Override BigDecimal GetValueImpl(Security security) {
            return Quantity*security.Price;
        }

        /**
        /// Creates a deep-copy clone of this order
        */
        @returns A copy of this order
        public @Override Order Clone() {
            order = new MarketOnOpenOrder();
            CopyTo(order);
            return order;
        }
    }
}
