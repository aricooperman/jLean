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
    /// Stop Market Order Type Definition
    */
    public class StopLimitOrder : Order
    {
        /**
        /// Stop price for this stop market order.
        */
        public BigDecimal StopPrice { get; internal set; }

        /**
        /// Signal showing the "StopLimitOrder" has been converted into a Limit Order
        */
        public boolean StopTriggered { get; internal set; }

        /**
        /// Limit price for the stop limit order
        */
        public BigDecimal LimitPrice { get; internal set; }

        /**
        /// StopLimit Order Type
        */
        public @Override OrderType Type
        {
            get { return OrderType.StopLimit; }
        }

        /**
        /// Default constructor for JSON Deserialization:
        */
        public StopLimitOrder() {
        }

        /**
        /// New Stop Market Order constructor - 
        */
         * @param symbol">Symbol asset we're seeking to trade
         * @param quantity">Quantity of the asset we're seeking to trade
         * @param limitPrice">Maximum price to fill the order
         * @param time">Time the order was placed
         * @param stopPrice">Price the order should be filled at if a limit order
         * @param tag">User defined data tag for this order
        public StopLimitOrder(Symbol symbol, int quantity, BigDecimal stopPrice, BigDecimal limitPrice, DateTime time, String tag = "")
            : base(symbol, quantity, time, tag) {
            StopPrice = stopPrice;
            LimitPrice = limitPrice;

            if( tag == "") {
                //Default tag values to display stop price in GUI.
                Tag = "Stop Price: " + stopPrice.toString( "C") + " Limit Price: " + limitPrice.toString( "C");
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
            if( request.StopPrice.HasValue) {
                StopPrice = request.StopPrice.Value;
            }
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
            return String.format( "%1$s at stop %2$s limit %3$s", base.toString(), StopPrice.SmartRounding(), LimitPrice.SmartRounding());
        }

        /**
        /// Creates a deep-copy clone of this order
        */
        @returns A copy of this order
        public @Override Order Clone() {
            order = new StopLimitOrder {StopPrice = StopPrice, LimitPrice = LimitPrice};
            CopyTo(order);
            return order;
        }
    }
}