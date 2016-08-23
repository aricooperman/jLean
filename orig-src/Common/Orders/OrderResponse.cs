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

package com.quantconnect.lean.Orders
{
    /**
    /// Represents a response to an <see cref="OrderRequest"/>. See <see cref="OrderRequest.Response"/> property for
    /// a specific request's response value
    */
    public class OrderResponse
    {
        /**
        /// Gets the order id
        */
        public int OrderId
        {
            get; private set;
        }

        /**
        /// Gets the error message if the <see cref="ErrorCode"/> does not equal <see cref="OrderResponseErrorCode.None"/>, otherwise
        /// gets <see cref="string.Empty"/>
        */
        public String ErrorMessage
        {
            get; private set;
        }

        /**
        /// Gets the error code for this response.
        */
        public OrderResponseErrorCode ErrorCode
        {
            get; private set;
        }

        /**
        /// Gets true if this response represents a successful request, false otherwise
        /// If this is an unprocessed response, IsSuccess will return false.
        */
        public boolean IsSuccess
        {
            get { return IsProcessed && !IsError; }
        }

        /**
        /// Gets true if this response represents an error, false otherwise
        */
        public boolean IsError
        {
            get { return IsProcessed && ErrorCode != OrderResponseErrorCode.None; }
        }

        /**
        /// Gets true if this response has been processed, false otherwise
        */
        public boolean IsProcessed
        {
            get { return this != Unprocessed; }
        }

        /**
        /// Initializes a new instance of the <see cref="OrderResponse"/> class
        */
         * @param orderId">The order id
         * @param errorCode">The error code of the response, specify <see cref="OrderResponseErrorCode.None"/> for no error
         * @param errorMessage">The error message, applies only if the <paramref name="errorCode"/> does not equal <see cref="OrderResponseErrorCode.None"/>
        private OrderResponse(int orderId, OrderResponseErrorCode errorCode, String errorMessage) {
            OrderId = orderId;
            ErrorCode = errorCode;
            if( errorCode != OrderResponseErrorCode.None) {
                ErrorMessage = errorMessage ?? "An unexpected error ocurred.";
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
            if( this == Unprocessed) {
                return "Unprocessed";
            }

            if( IsError) {
                return String.format( "Error: %1$s - %2$s", ErrorCode, ErrorMessage);
            }
            return "Success";
        }

        #region Statics - implicit(int), Unprocessed constant, reponse factory methods

        /**
        /// Gets an <see cref="OrderResponse"/> for a request that has not yet been processed
        */
        public static final OrderResponse Unprocessed = new OrderResponse(int.MinValue, OrderResponseErrorCode.None, "The request has not yet been processed.");

        /**
        /// Helper method to create a successful response from a request
        */
        public static OrderResponse Success(OrderRequest request) {
            return new OrderResponse(request.OrderId, OrderResponseErrorCode.None, null );
        }

        /**
        /// Helper method to create an error response from a request
        */
        public static OrderResponse Error(OrderRequest request, OrderResponseErrorCode errorCode, String errorMessage) {
            return new OrderResponse(request.OrderId, errorCode, errorMessage);
        }

        /**
        /// Helper method to create an error response due to an invalid order status
        */
        public static OrderResponse InvalidStatus(OrderRequest request, Order order) {
            return Error(request, OrderResponseErrorCode.InvalidOrderStatus,
                String.format( "Unable to update order with id %1$s because it already has %2$s status", request.OrderId, order.Status));
        }

        /**
        /// Helper method to create an error response due to a bad order id
        */
        public static OrderResponse UnableToFindOrder(OrderRequest request) {
            return Error(request, OrderResponseErrorCode.UnableToFindOrder,
                String.format( "Unable to locate order with id %1$s.", request.OrderId));
        }

        /**
        /// Helper method to create an error response due to a zero order quantity
        */
        public static OrderResponse ZeroQuantity(OrderRequest request) {
            static final String format = "Unable to %1$s order with id %2$s that have zero quantity.";
            return Error(request, OrderResponseErrorCode.OrderQuantityZero,
                String.format(format, request.OrderRequestType.toString().toLowerCase(), request.OrderId));
        }

        /**
        /// Helper method to create an error response due to algorithm still in warmup mode
        */
        public static OrderResponse WarmingUp(OrderRequest request) {
            return Error(request, OrderResponseErrorCode.AlgorithmWarmingUp, "Algorithm in warmup.");
        }

        #endregion
    }
}