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
import java.util.ArrayList;
import java.util.List;

import com.quantconnect.lean.SecurityType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.orders.OrderTypes.OrderDirection;
import com.quantconnect.lean.orders.OrderTypes.OrderDuration;
import com.quantconnect.lean.orders.OrderTypes.OrderStatus;
import com.quantconnect.lean.orders.OrderTypes.OrderType;
import com.quantconnect.lean.securities.Security;

/**
 *  Order class for placing new trade
 */
public abstract class Order implements Cloneable {
    
    /// Order ID.
    protected int id; 

    /// Order id to process before processing this order.
    protected int contingentId;

    /// Brokerage Id for this order for when the brokerage splits orders into multiple pieces
    protected List<String> brokerId;

    /// Symbol of the Asset
    protected Symbol symbol;

    /// Price of the Order.
    protected BigDecimal price;

    /// Currency for the order price
    protected String priceCurrency;

    /// Time the order was created.
    protected LocalDateTime time;

    /// Number of shares to execute.
    protected int quantity;

    /// Status of the Order
    protected OrderStatus status;

    /// Order duration - GTC or Day. Day not supported in backtests.
    protected OrderDuration duration;

    /// Tag the order with some custom data
    protected String tag;

    /// Order Expiry on a specific UTC time.
    public LocalDateTime DurationValue;

    
    public int getId() {
        return id;
    }

    public int getContingentId() {
        return contingentId;
    }

    public List<String> getBrokerId() {
        return brokerId;
    }

    public Symbol getSymbol() {
        return symbol;
    }

    public BigDecimal getPrice() {
        return price;
    }

    public String getPriceCurrency() {
        return priceCurrency;
    }

    public LocalDateTime getTime() {
        return time;
    }

    public int getQuantity() {
        return quantity;
    }

    public OrderStatus getStatus() {
        return status;
    }

    public OrderDuration getDuration() {
        return duration;
    }

    public String getTag() {
        return tag;
    }

    /// Order Type
    public abstract OrderType getType();

    /// The symbol's security type
    public SecurityType getSecurityType() { 
        return symbol.getId().getSecurityType();
    }

    /// Order Direction Property based off Quantity.
    public OrderDirection getDirection() {
        if( quantity > 0 )
            return OrderDirection.Buy;
            
        if( quantity < 0 ) 
            return OrderDirection.Sell;

        return OrderDirection.Hold;
    }

    /**
     * Get the absolute quantity for this order
     * @return
     */
    public int getAbsoluteQuantity() {
        return Math.abs( quantity );
    }

    /**
     * Gets the executed value of this order. If the order has not yet filled,
     * then this will return zero.
     * @return
     */
    public BigDecimal getValue() {
        return price.multiply( BigDecimal.valueOf( quantity ) );
    }

    /// Added a default constructor for JSON Deserialization:
    /*protected*/ Order() {
        this( Symbol.EMPTY, 0, LocalDateTime.now(), "" );
    }

    /**
     * New order constructor
     * @param symbol
    /// <param name="symbol Symbol asset we're seeking to trade
     * @param quantity
    /// <param name="quantity Quantity of the asset we're seeking to trade
     * @param time
    /// <param name="time Time the order was placed
     */
    protected Order( Symbol symbol, int quantity, LocalDateTime time ) {
        this( symbol, quantity, time, "" );
    }

    /**
     * New order constructor
     * @param symbol
    /// <param name="symbol Symbol asset we're seeking to trade
     * @param quantity
    /// <param name="quantity Quantity of the asset we're seeking to trade
     * @param time
    /// <param name="time Time the order was placed
    /// <param name="tag User defined data tag for this order
     */
    protected Order( Symbol symbol, int quantity, LocalDateTime time, String tag ) {
        this.time = time;
        this.price = BigDecimal.ZERO;
        this.priceCurrency = null;
        this.quantity = quantity;
        this.symbol = symbol;
        this.status = OrderStatus.None;
        this.tag = tag;
        this.duration = OrderDuration.GTC;
        this.brokerId = new ArrayList<String>();
        this.contingentId = 0;
        this.DurationValue = LocalDateTime.MAX;

    }

    /**
     * Gets the value of this order at the given market price in units of the account currency
     * NOTE: Some order types derive value from other parameters, such as limit prices
     * @param security security The security matching this order's symbol
     * @return The value of this order given the current market price
     */
    public BigDecimal getValue( Security security ) {
        final BigDecimal value = getValueImpl( security );
        return value.multiply( security.getQuoteCurrency().getConversionRate() ).multiply( security.getSymbolProperties().getContractMultiplier() );
    }

    /**
     * Gets the order value in units of the security's quote currency for a single unit.
     * A single unit here is a single share of stock, or a single barrel of oil, or the
     * cost of a single share in an option contract.
     * @param security security The security matching this order's symbol
     * @return
     */
    protected abstract BigDecimal getValueImpl( Security security );

    /**
     * Modifies the state of this order to match the update request
     * @param request The request to update this order object
     */
    public void applyUpdateOrderRequest( UpdateOrderRequest request ) {
        if( request.orderId != id )
            throw new IllegalArgumentException( "Attempted to apply updates to the incorrect order!" );

        quantity = request.getQuantity().orElse( quantity );

        if( request.getTag() != null )
            tag = request.getTag();
    }

    /// Returns a String that represents the current object.
    /// <returns>
    /// A String that represents the current object.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public String toString() {
        return String.format( "OrderId: %d %s %3$s order for %d unit%s of %s", id, status, getType(), quantity, quantity == 1 ? "" : "s", symbol );
    }

    /// Creates a deep-copy clone of this order
    /// <returns>A copy of this order</returns>
    public abstract Order clone();

    /// Copies base Order properties to the specified order
    /// <param name="order The target of the copy
    protected void copyTo( Order order ) {
        order.id = id;
        order.time = time;
        order.brokerId = new ArrayList<>( brokerId );
        order.contingentId = contingentId;
        order.duration = duration;
        order.price = price;
        order.priceCurrency = priceCurrency;
        order.quantity = quantity;
        order.status = status;
        order.symbol = symbol;
        order.tag = tag;
    }

    /**
     * Creates an <see cref="Order"/> to match the specified <paramref name="request"/>
     * @param request The <see cref="SubmitOrderRequest"/> to create an order for
     * @return The <see cref="Order"/> that matches the request
     */
    public static Order createOrder( SubmitOrderRequest request ) {
        Order order;
        switch( request.getOrderType() ) {
            case Market:
                order = new MarketOrder( request.getSymbol(), request.getQuantity(), request.getTime(), request.getTag() );
                break;
            case Limit:
                order = new LimitOrder( request.getSymbol(), request.getQuantity(), request.getLimitPrice(), request.getTime(), request.getTag() );
                break;
            case StopMarket:
                order = new StopMarketOrder( request.getSymbol(), request.getQuantity(), request.getStopPrice(), request.getTime(), request.getTag() );
                break;
            case StopLimit:
                order = new StopLimitOrder( request.getSymbol(), request.getQuantity(), request.getStopPrice(), request.getLimitPrice(), request.getTime(), request.getTag() );
                break;
            case MarketOnOpen:
                order = new MarketOnOpenOrder( request.getSymbol(), request.getQuantity(), request.getTime(), request.getTag() );
                break;
            case MarketOnClose:
                order = new MarketOnCloseOrder( request.getSymbol(), request.getQuantity(), request.getTime(), request.getTag() );
                break;
            default:
                throw new IllegalArgumentException();
        }
        
        order.status = OrderStatus.New;
        order.id = request.orderId;
        
        if( request.getTag() != null )
            order.tag = request.getTag();

        return order;
    }
}
