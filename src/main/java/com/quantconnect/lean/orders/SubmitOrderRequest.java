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

import java.math.BigDecimal;
import java.time.LocalDateTime;

import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.orders.OrderTypes.OrderType;

/**
 * Defines a request to submit a new order
 */
public class SubmitOrderRequest extends OrderRequest {
    
    private SecurityType securityType;
    private Symbol symbol;
    private OrderType orderType;
    private int quantity;
    private BigDecimal limitPrice;
    private BigDecimal stopPrice;
    
    /**
     * Gets <see cref="Orders.OrderRequestType.Submit"/>
     */
    @Override
    public OrderRequestType getOrderRequestType() {
        return OrderRequestType.Submit;
    }
    
    /**
     * Initializes a new instance of the <see cref="SubmitOrderRequest"/> class.
     * The <see cref="OrderRequest.OrderId"/> will default to <see cref="OrderResponseErrorCode.UnableToFindOrder"/>
     * @param orderType The order type to be submitted
     * @param securityType The symbol's <see cref="SecurityType"/>
     * @param symbol The symbol to be traded
     * @param quantity The number of units to be ordered
     * @param stopPrice The stop price for stop orders, non-stop orers this value is ignored
     * @param limitPrice The limit price for limit orders, non-limit orders this value is ignored
     * @param time The time this request was created
     * @param tag A custom tag for this request
     */
    public SubmitOrderRequest(OrderType orderType, SecurityType securityType, Symbol symbol, int quantity, BigDecimal stopPrice, 
            BigDecimal limitPrice, LocalDateTime time, String tag ) {
        super( time, OrderResponseErrorCode.UnableToFindOrder.getCode(), tag );
        this.securityType = securityType;
        this.symbol = symbol;
        this.orderType = orderType;
        this.quantity = quantity;
        this.limitPrice = limitPrice;
        this.stopPrice = stopPrice;
    }

    /**
     * Gets the security type of the symbol
     */
    public SecurityType getSecurityType() {
        return securityType;
    }

    /**
     * Gets the symbol to be traded
     */
    public Symbol getSymbol() {
        return symbol;
    }

    /**
     * Gets the order type od the order
     */
    public OrderType getOrderType() {
        return orderType;
    }

    /**
     * Gets the quantity of the order
     */
    public int getQuantity() {
        return quantity;
    }

    /**
     * Gets the limit price of the order, zero if not a limit order
     */
    public BigDecimal getLimitPrice() {
        return limitPrice;
    }

    /**
     * Gets the stop price of the order, zero if not a stop order
     */
    public BigDecimal getStopPrice() {
        return stopPrice;
    }

    /**
     * Sets the <see cref="OrderRequest.OrderId"/>
     * @param orderId The order id of the generated order
     */
    public void setOrderId( int orderId ) {
        this.orderId = orderId;
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        // create a proxy order object to steal his to String method
        final Order proxy = Order.createOrder( this );
        return String.format( "%1$s UTC: Submit Order: (%2$s) - %3$s %4$s", getTime(), getOrderId(), proxy, getTag() ) + " Status: " + getStatus();
    }
}
