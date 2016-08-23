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

package com.quantconnect.lean.Orders
{
    /**
    /// Defines a request to submit a new order
    */
    public class SubmitOrderRequest : OrderRequest
    {
        /**
        /// Gets <see cref="Orders.OrderRequestType.Submit"/>
        */
        public @Override OrderRequestType OrderRequestType
        {
            get { return OrderRequestType.Submit; }
        }

        /**
        /// Gets the security type of the symbol
        */
        public SecurityType SecurityType
        {
            get; private set;
        }

        /**
        /// Gets the symbol to be traded
        */
        public Symbol Symbol
        {
            get; private set;
        }

        /**
        /// Gets the order type od the order
        */
        public OrderType OrderType
        {
            get; private set;
        }

        /**
        /// Gets the quantity of the order
        */
        public int Quantity
        {
            get; private set;
        }

        /**
        /// Gets the limit price of the order, zero if not a limit order
        */
        public BigDecimal LimitPrice
        {
            get; private set;
        }

        /**
        /// Gets the stop price of the order, zero if not a stop order
        */
        public BigDecimal StopPrice
        {
            get; private set;
        }

        /**
        /// Initializes a new instance of the <see cref="SubmitOrderRequest"/> class.
        /// The <see cref="OrderRequest.OrderId"/> will default to <see cref="OrderResponseErrorCode.UnableToFindOrder"/>
        */
         * @param orderType">The order type to be submitted
         * @param securityType">The symbol's <see cref="SecurityType"/>
         * @param symbol">The symbol to be traded
         * @param quantity">The number of units to be ordered
         * @param stopPrice">The stop price for stop orders, non-stop orers this value is ignored
         * @param limitPrice">The limit price for limit orders, non-limit orders this value is ignored
         * @param time">The time this request was created
         * @param tag">A custom tag for this request
        public SubmitOrderRequest(OrderType orderType, SecurityType securityType, Symbol symbol, int quantity, BigDecimal stopPrice, BigDecimal limitPrice, DateTime time, String tag)
            : base(time, (int) OrderResponseErrorCode.UnableToFindOrder, tag) {
            SecurityType = securityType;
            Symbol = symbol;
            OrderType = orderType;
            Quantity = quantity;
            LimitPrice = limitPrice;
            StopPrice = stopPrice;
        }

        /**
        /// Sets the <see cref="OrderRequest.OrderId"/>
        */
         * @param orderId">The order id of the generated order
        internal void SetOrderId(int orderId) {
            OrderId = orderId;
        }

        /**
        /// Returns a String that represents the current object.
        */
        @returns 
        /// A String that represents the current object.
        /// 
        /// <filterpriority>2</filterpriority>
        public @Override String toString() {
            // create a proxy order object to steal his to String method
            proxy = Order.CreateOrder(this);
            return String.format( "%1$s UTC: Submit Order: (%2$s) - %3$s {3}", Time, OrderId, proxy, Tag) + " Status: " + Status;
        }
    }
}