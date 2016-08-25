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

import com.quantconnect.lean.orders.OrderTypes.OrderDirection;
import com.quantconnect.lean.orders.OrderTypes.OrderStatus;
import com.quantconnect.lean.securities.CashBook;
import com.quantconnect.lean.Symbol;

/**
 * Order Event - Messaging class signifying a change in an order state and record the change in the user's algorithm portfolio 
 */
public class OrderEvent implements Cloneable {
   
    /**
     * Id of the order this event comes from.
     */
    public int orderId;

    /**
     * Easy access to the order symbol associated with this event.
     */
    public Symbol symbol;

    /**
     * The date and time of this event (UTC).
     */
    public LocalDateTime utcTime;

    /**
     * Status message of the order.
     */
    public OrderStatus status;

    /**
     * The fee associated with the order (always positive value).
     */
    public BigDecimal orderFee;

    /**
     * Fill price information about the order
     */
    public BigDecimal fillPrice;

    /**
     * Currency for the fill price
     */
    public String fillPriceCurrency;

    /**
     * Number of shares of the order that was filled in this event.
     */
    public int fillQuantity;

    /**
     * Public Property Absolute Getter of Quantity -Filled
     */
    public int getAbsoluteFillQuantity() {
        return Math.abs( fillQuantity );
    }

    /**
     * Order direction.
     */
    private OrderDirection direction;

    public OrderDirection getDirection() {
        return direction;
    }

    /**
     * Any message from the exchange.
     */
    public String message;

    /**
     * Order Event Constructor.
     * @param orderId Id of the parent order
     * @param symbol Asset Symbol
     * @param utcTime Date/time of this event
     * @param status Status of the order
     * @param direction The direction of the order this event belongs to
     * @param fillPrice Fill price information if applicable.
     * @param fillQuantity Fill quantity
     * @param orderFee The order fee
     */
    public OrderEvent( int orderId, Symbol symbol, LocalDateTime utcTime, OrderStatus status, OrderDirection direction, BigDecimal fillPrice, 
            int fillQuantity, BigDecimal orderFee ) {
        this( orderId, symbol, utcTime, status, direction, fillPrice, fillQuantity, orderFee, "" );
    }

    
    /**
     * Order Event Constructor.
     * @param orderId Id of the parent order
     * @param symbol Asset Symbol
     * @param utcTime Date/time of this event
     * @param status Status of the order
     * @param direction The direction of the order this event belongs to
     * @param fillPrice Fill price information if applicable.
     * @param fillQuantity Fill quantity
     * @param orderFee The order fee
     * @param message Message from the exchange
     */
    public OrderEvent( int orderId, Symbol symbol, LocalDateTime utcTime, OrderStatus status, OrderDirection direction, BigDecimal fillPrice, 
            int fillQuantity, BigDecimal orderFee, String message ) {
        this.orderId = orderId;
        this.symbol = symbol;
        this.utcTime = utcTime;
        this.status = status;
        this.direction = direction;
        this.fillPrice = fillPrice;
        this.fillPriceCurrency = null;
        this.fillQuantity = fillQuantity;
        this.orderFee = orderFee.abs();
        this.message = message;
    }

    /**
     * Helper Constructor using Order to Initialize.
     * @param order Order for this order status
     * @param utcTime Date/time of this event
     * @param orderFee The order fee
     */
    public OrderEvent( Order order, LocalDateTime utcTime, BigDecimal orderFee ) {
        this( order, utcTime, orderFee, "" );
    }
    
    /**
     * Helper Constructor using Order to Initialize.
     * @param order Order for this order status
     * @param utcTime Date/time of this event
     * @param orderFee The order fee
     * @param message Message from exchange or QC.
     */
    public OrderEvent( Order order, LocalDateTime utcTime, BigDecimal orderFee, String message ) {
        this.orderId = order.id;
        this.symbol = order.symbol;
        this.status = order.status;
        this.direction = order.getDirection();

        //Initialize to zero, manually set fill quantity
        this.fillQuantity = 0;
        this.fillPrice = BigDecimal.ZERO;
        this.fillPriceCurrency = order.priceCurrency;

        this.utcTime = utcTime;
        this.orderFee = orderFee.abs();
        this.message = message;
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        String message = fillQuantity == 0 
            ? String.format( "Time: %1$s OrderID: %2$s Symbol: %3$s Status: %4$s", utcTime, orderId, symbol, status ) 
            : String.format( "Time: %1$s OrderID: %2$s Symbol: %3$s Status: %4$s Quantity: %5$s FillPrice: %6$s %7$s", 
                    utcTime, orderId, symbol, status, fillQuantity, fillPrice, fillPriceCurrency );

        // attach the order fee so it ends up in logs properly
        if( orderFee.signum() != 0 ) 
            message += String.format( " OrderFee: %1$s %2$s", orderFee, CashBook.ACCOUNT_CURRENCY );
        
        return message;
    }

    /**
     * Returns a clone of the current object.
     * @returns The new clone object
     */
    public OrderEvent clone() {
        try {
            return (OrderEvent)super.clone();
        }
        catch( CloneNotSupportedException e ) {
            throw new RuntimeException( e );
        }
    }
}
