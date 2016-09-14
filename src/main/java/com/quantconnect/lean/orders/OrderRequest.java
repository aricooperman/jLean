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

import java.time.LocalDateTime;

/**
 * Represents a request to submit, update, or cancel an order
 */
public abstract class OrderRequest {

    protected int orderId;

    private OrderRequestStatus status;
    private LocalDateTime time;
    private String tag;
    private OrderResponse response;
    
    /**
     * Gets the type of this order request
     */
    public abstract OrderRequestType getOrderRequestType();

    /**
     * Gets the status of this request
     */
    public OrderRequestStatus getStatus() {
        return status;
    }

    /**
     * Gets the time the request was created
     */
    public LocalDateTime getTime() {
        return time;
    }

    /**
     * Gets the order id the request acts on
     */
    public int getOrderId() {
        return orderId;
    }

    /**
     * Gets a tag for this request
     */
    public String getTag() {
        return tag;
    }

    /**
     * Gets the response for this request. If this request was never processed then this
     * will equal <see cref="OrderResponse.Unprocessed"/>. This value is never equal to null.
     */
    public OrderResponse getResponse() {
        return response;
    }

    /**
     * Initializes a new instance of the <see cref="OrderRequest"/> class
     * @param time The time this request was created
     * @param orderId The order id this request acts on, specify zero for <see cref="SubmitOrderRequest"/>
     * @param tag A custom tag for the request
    */
    protected OrderRequest( LocalDateTime time, int orderId, String tag ) {
        this.time = time;
        this.orderId = orderId;
        this.tag = tag;
        this.response = OrderResponse.UNPROCESSED;
        this.status = OrderRequestStatus.Unprocessed;
    }

    public void setResponse( OrderResponse response ) {
        setResponse( response, OrderRequestStatus.Error );
    }
    
    /**
     * Sets the <see cref="Response"/> for this request
     * @param response The response to this request
     * @param status The current status of this request
     */
    public void setResponse( OrderResponse response, OrderRequestStatus status ) {
        if( response == null )
            throw new NullPointerException( "Response can not be null" );

        // if the response is an error, ignore the input status
        this.status = response.isError() ? OrderRequestStatus.Error : status;
        this.response = response;
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        return String.format( "%1$s UTC: Order: (%2$s) - %3$s Status: %4$s", time, orderId, tag, status );
    }
}