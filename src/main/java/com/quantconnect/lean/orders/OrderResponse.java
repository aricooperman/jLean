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

package com.quantconnect.lean.orders;

/**
 * Represents a response to an <see cref="OrderRequest"/>. See <see cref="OrderRequest.Response"/> property for
 * a specific request's response value
 */
public class OrderResponse {
    
    /**
     * Gets an <see cref="OrderResponse"/> for a request that has not yet been processed
     */
    public static final OrderResponse UNPROCESSED = new OrderResponse( Integer.MIN_VALUE, OrderResponseErrorCode.None, "The request has not yet been processed." );
    
    private int orderId;
    private String errorMessage;
    private OrderResponseErrorCode errorCode;

    
    /**
     * Gets the order id
     */
    public int getOrderId() {
        return orderId;
    }

    /**
     * Gets the error message if the <see cref="ErrorCode"/> does not equal <see cref="OrderResponseErrorCode.None"/>, otherwise
     * gets <see cref="string.Empty"/>
     */
    public String getErrorMessage() {
        return errorMessage;
    }

    /**
     * Gets the error code for this response.
     */
    public OrderResponseErrorCode getErrorCode() {
        return errorCode;
    }

    /**
     * Gets true if this response represents a successful request, false otherwise
     * If this is an unprocessed response, IsSuccess will return false.
     */
    public boolean isSuccess() {
        return isProcessed() && !isError();
    }

    /**
     * Gets true if this response represents an error, false otherwise
     */
    public boolean isError() {
        return isProcessed() && errorCode != OrderResponseErrorCode.None;
    }

    /**
     * Gets true if this response has been processed, false otherwise
     */
    public boolean isProcessed() {
        return this != UNPROCESSED;
    }

    /**
     * Initializes a new instance of the <see cref="OrderResponse"/> class
     * @param orderId The order id
     * @param errorCode The error code of the response, specify <see cref="OrderResponseErrorCode.None"/> for no error
     * @param errorMessage The error message, applies only if the <paramref name="errorCode"/> does not equal <see cref="OrderResponseErrorCode.None"/>
     */
    private OrderResponse( int orderId, OrderResponseErrorCode errorCode, String errorMessage ) {
        this.orderId = orderId;
        this.errorCode = errorCode;
        if( errorCode != OrderResponseErrorCode.None )
            errorMessage = errorMessage != null ? errorMessage : "An unexpected error ocurred.";
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        if( this == UNPROCESSED )
            return "Unprocessed";

        if( isError() )
            return String.format( "Error: %1$s - %2$s", errorCode, errorMessage );
        
        return "Success";
    }

//        #region Statics - implicit(int), Unprocessed constant, reponse factory methods

    /**
     * Helper method to create a successful response from a request
     */
    public static OrderResponse success( OrderRequest request ) {
        return new OrderResponse( request.orderId, OrderResponseErrorCode.None, null );
    }

    /**
     * Helper method to create an error response from a request
     */
    public static OrderResponse error(OrderRequest request, OrderResponseErrorCode errorCode, String errorMessage) {
        return new OrderResponse(request.orderId, errorCode, errorMessage );
    }

    /**
     * Helper method to create an error response due to an invalid order status
     */
    public static OrderResponse invalidStatus( OrderRequest request, Order order ) {
        return error( request, OrderResponseErrorCode.InvalidOrderStatus,
                String.format( "Unable to update order with id %1$s because it already has %2$s status", request.orderId, order.getStatus() ) );
    }

    /**
     * Helper method to create an error response due to a bad order id
     */
    public static OrderResponse unableToFindOrder( OrderRequest request ) {
        return error(request, OrderResponseErrorCode.UnableToFindOrder,
                String.format( "Unable to locate order with id %1$s.", request.orderId));
    }

    /**
     * Helper method to create an error response due to a zero order quantity
     */
    public static OrderResponse zeroQuantity( OrderRequest request ) {
        return error( request, OrderResponseErrorCode.OrderQuantityZero,
                String.format( "Unable to %1$s order with id %2$s that have zero quantity.", request.getOrderRequestType().toString().toLowerCase(), request.orderId ) );
    }

    /**
     * Helper method to create an error response due to algorithm still in warmup mode
     */
    public static OrderResponse warmingUp( OrderRequest request ) {
        return error( request, OrderResponseErrorCode.AlgorithmWarmingUp, "Algorithm in warmup." );
    }
}