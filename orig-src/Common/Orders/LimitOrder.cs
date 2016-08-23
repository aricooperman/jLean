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
    /// Limit order type definition
    */
    public class LimitOrder : Order
    {
        /**
        /// Limit price for this order.
        */
        public BigDecimal LimitPrice { get; internal set; }

        /**
        /// Limit Order Type
        */
        public @Override OrderType Type
        {
            get { return OrderType.Limit; }
        }

        /**
        /// Added a default constructor for JSON Deserialization:
        */
        public LimitOrder() {
        }

        /**
        /// New limit order constructor
        */
         * @param symbol">Symbol asset we're seeking to trade
         * @param quantity">Quantity of the asset we're seeking to trade
         * @param time">Time the order was placed
         * @param limitPrice">Price the order should be filled at if a limit order
         * @param tag">User defined data tag for this order
        public LimitOrder(Symbol symbol, int quantity, BigDecimal limitPrice, DateTime time, String tag = "")
            : base(symbol, quantity, time, tag) {
            LimitPrice = limitPrice;

            if( tag == "") {
                //Default tag values to display limit price in GUI.
                Tag = "Limit Price: " + limitPrice.toString( "C");
            }
        }

        /**
        /// Gets the order value in units of the security's quote currency
        */
         * @param security">The security matching this order's symbol
        protected @Override BigDecimal GetValueImpl(Security security) {
            // selling, so higher price will be used
            if( Quantity < 0) {
                return Quantity*Math.Max(LimitPrice, security.Price);
            }

            // buying, so lower price will be used
            if( Quantity > 0) {
                return Quantity*Math.Min(LimitPrice, security.Price);
            }

            return 0m;
        }

        /**
        /// Modifies the state of this order to match the update request
        */
         * @param request">The request to update this order object
        public @Override void ApplyUpdateOrderRequest(UpdateOrderRequest request) {
            base.ApplyUpdateOrderRequest(request);
            if( request.LimitPrice.HasValue) {
                LimitPrice = request.LimitPrice.Value;
            }
        }

        /**
        /// Returns a String that represents the current object.
        */
        @returns 
        /// A String that represents the current object.
        /// 
        /// <filterpriority>2</filterpriority>
        public @Override String toString() {
            return String.format( "%1$s at limit %2$s", base.toString(), LimitPrice.SmartRounding());
        }

        /**
        /// Creates a deep-copy clone of this order
        */
        @returns A copy of this order
        public @Override Order Clone() {
            order = new LimitOrder {LimitPrice = LimitPrice};
            CopyTo(order);
            return order;
        }
    }
}
