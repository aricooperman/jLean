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
    /// Order Event - Messaging class signifying a change in an order state and record the change in the user's algorithm portfolio 
    */
    public class OrderEvent
    {
        /**
        /// Id of the order this event comes from.
        */
        public int OrderId;

        /**
        /// Easy access to the order symbol associated with this event.
        */
        public Symbol Symbol;

        /**
        /// The date and time of this event (UTC).
        */
        public DateTime UtcTime;

        /**
        /// Status message of the order.
        */
        public OrderStatus Status;

        /**
        /// The fee associated with the order (always positive value).
        */
        public BigDecimal OrderFee;

        /**
        /// Fill price information about the order
        */
        public BigDecimal FillPrice;

        /**
        /// Currency for the fill price
        */
        public String FillPriceCurrency;

        /**
        /// Number of shares of the order that was filled in this event.
        */
        public int FillQuantity;

        /**
        /// Public Property Absolute Getter of Quantity -Filled
        */
        public int AbsoluteFillQuantity 
        {
            get 
            {
                return Math.Abs(FillQuantity);
            }
        }

        /**
        /// Order direction.
        */
        public OrderDirection Direction
        {
            get; private set;
        }

        /**
        /// Any message from the exchange.
        */
        public String Message;

        /**
        /// Order Event Constructor.
        */
         * @param orderId">Id of the parent order
         * @param symbol">Asset Symbol
         * @param utcTime">Date/time of this event
         * @param status">Status of the order
         * @param direction">The direction of the order this event belongs to
         * @param fillPrice">Fill price information if applicable.
         * @param fillQuantity">Fill quantity
         * @param orderFee">The order fee
         * @param message">Message from the exchange
        public OrderEvent(int orderId, Symbol symbol, DateTime utcTime, OrderStatus status, OrderDirection direction, BigDecimal fillPrice, int fillQuantity, BigDecimal orderFee, String message = "") {
            OrderId = orderId;
            Symbol = symbol;
            UtcTime = utcTime;
            Status = status;
            Direction = direction;
            FillPrice = fillPrice;
            FillPriceCurrency = string.Empty;
            FillQuantity = fillQuantity;
            OrderFee = Math.Abs(orderFee);
            Message = message;
        }

        /**
        /// Helper Constructor using Order to Initialize.
        */
         * @param order">Order for this order status
         * @param utcTime">Date/time of this event
         * @param orderFee">The order fee
         * @param message">Message from exchange or QC.
        public OrderEvent(Order order, DateTime utcTime, BigDecimal orderFee, String message = "") {
            OrderId = order.Id;
            Symbol = order.Symbol;
            Status = order.Status;
            Direction = order.Direction;

            //Initialize to zero, manually set fill quantity
            FillQuantity = 0;
            FillPrice = 0;
            FillPriceCurrency = order.PriceCurrency;

            UtcTime = utcTime;
            OrderFee = Math.Abs(orderFee);
            Message = message;
        }

        /**
        /// Returns a String that represents the current object.
        */
        @returns 
        /// A String that represents the current object.
        /// 
        /// <filterpriority>2</filterpriority>
        public @Override String toString() {
            message = FillQuantity == 0 
                ? String.format( "Time: %1$s OrderID: %2$s Symbol: %3$s Status: {3}", UtcTime, OrderId, Symbol, Status) 
                : String.format( "Time: %1$s OrderID: %2$s Symbol: %3$s Status: {3} Quantity: {4} FillPrice: {5} {6}", UtcTime, OrderId, Symbol, Status, FillQuantity, FillPrice, FillPriceCurrency);

            // attach the order fee so it ends up in logs properly
            if( OrderFee != 0m) message += String.format( " OrderFee: %1$s %2$s", OrderFee, CashBook.AccountCurrency);
            
            return message;
        }

        /**
        /// Returns a clone of the current object.
        */
        @returns The new clone object
        public OrderEvent Clone() {
            return (OrderEvent)MemberwiseClone();
        }
    }

} // End QC Namespace:
