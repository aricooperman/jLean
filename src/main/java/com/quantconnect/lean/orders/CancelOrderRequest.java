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
 * Defines a request to cancel an order 
 */
public class CancelOrderRequest extends OrderRequest {
    
    /**
     * Gets <see cref="Orders.OrderRequestType.Cancel"/>
     */
    @Override
    public OrderRequestType getOrderRequestType() {
        return OrderRequestType.Cancel;
    }

    /**
     * Initializes a new instance of the <see cref="CancelOrderRequest"/> class
     * @param time The time this cancelation was requested
     * @param orderId The order id to be canceled
     * @param tag A new tag for the order
    */
    public CancelOrderRequest( LocalDateTime time, int orderId, String tag ) {
        super( time, orderId, tag );
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        return String.format( "%1$s UTC: Cancel Order: (%2$s) - %3$s", getTime(), orderId, getTag() ) + " Status: " + getStatus();
    }
}
