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
    /// Represents a request to submit, update, or cancel an order
    */
    public abstract class OrderRequest
    {
        /**
        /// Gets the type of this order request
        */
        public abstract OrderRequestType OrderRequestType
        {
            get;
        }

        /**
        /// Gets the status of this request
        */
        public OrderRequestStatus Status
        {
            get; private set;
        }

        /**
        /// Gets the time the request was created
        */
        public DateTime Time
        {
            get; private set;
        }

        /**
        /// Gets the order id the request acts on
        */
        public int OrderId
        {
            get; protected set;
        }

        /**
        /// Gets a tag for this request
        */
        public String Tag
        {
            get; private set;
        }

        /**
        /// Gets the response for this request. If this request was never processed then this
        /// will equal <see cref="OrderResponse.Unprocessed"/>. This value is never equal to null.
        */
        public OrderResponse Response
        {
            get; private set;
        }

        /**
        /// Initializes a new instance of the <see cref="OrderRequest"/> class
        */
         * @param time">The time this request was created
         * @param orderId">The order id this request acts on, specify zero for <see cref="SubmitOrderRequest"/>
         * @param tag">A custom tag for the request
        protected OrderRequest(DateTime time, int orderId, String tag) {
            Time = time;
            OrderId = orderId;
            Tag = tag;
            Response = OrderResponse.Unprocessed;
            Status = OrderRequestStatus.Unprocessed;
        }

        /**
        /// Sets the <see cref="Response"/> for this request
        */
         * @param response">The response to this request
         * @param status">The current status of this request
        public void SetResponse(OrderResponse response, OrderRequestStatus status = OrderRequestStatus.Error) {
            if( response == null ) {
                throw new ArgumentNullException( "response", "Response can not be null");
            }

            // if the response is an error, ignore the input status
            Status = response.IsError ? OrderRequestStatus.Error : status;
            Response = response;
        }

        /**
        /// Returns a String that represents the current object.
        */
        @returns 
        /// A String that represents the current object.
        /// 
        /// <filterpriority>2</filterpriority>
        public @Override String toString() {
            return String.format( "%1$s UTC: Order: (%2$s) - %3$s Status: {3}", Time, OrderId, Tag, Status);
        }
    }
}