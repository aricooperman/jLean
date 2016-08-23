using System;
using System.Collections.Generic;

package com.quantconnect.lean.Orders
{
    /**
    /// Defines a request to update an order's values
    */
    public class UpdateOrderRequest : OrderRequest
    {
        /**
        /// Gets <see cref="Orders.OrderRequestType.Update"/>
        */
        public @Override OrderRequestType OrderRequestType
        {
            get { return OrderRequestType.Update; }
        }

        /**
        /// Gets the new quantity of the order, null to not change the quantity
        */
        public int? Quantity { get; private set; }

        /**
        /// Gets the new limit price of the order, null to not change the limit price
        */
        public decimal? LimitPrice { get; private set; }

        /**
        /// Gets the new stop price of the order, null to not change the stop price
        */
        public decimal? StopPrice { get; private set; }

        /**
        /// Initializes a new instance of the <see cref="UpdateOrderRequest"/> class
        */
         * @param time">The time the request was submitted
         * @param orderId">The order id to be updated
         * @param fields">The fields defining what should be updated
        public UpdateOrderRequest(DateTime time, int orderId, UpdateOrderFields fields)
            : base(time, orderId, fields.Tag) {
            Quantity = fields.Quantity;
            LimitPrice = fields.LimitPrice;
            StopPrice = fields.StopPrice;
        }

        /**
        /// Returns a String that represents the current object.
        */
        @returns 
        /// A String that represents the current object.
        /// 
        /// <filterpriority>2</filterpriority>
        public @Override String toString() {
            updates = new List<String>();
            if( Quantity.HasValue) {
                updates.Add( "Quantity: " + Quantity.Value);
            }
            if( LimitPrice.HasValue) {
                updates.Add( "LimitPrice: " + LimitPrice.Value.SmartRounding());
            }
            if( StopPrice.HasValue) {
                updates.Add( "StopPrice: " + StopPrice.Value.SmartRounding());
            }
            return String.format( "%1$s UTC: Update Order: (%2$s) - %3$s {3} Status: {4}", Time, OrderId, String.join( ", ", updates), Tag, Status);
        }
    }
}