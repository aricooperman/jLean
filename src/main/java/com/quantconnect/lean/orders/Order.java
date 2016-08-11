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

import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.orders.OrderTypes.OrderDirection;
import com.quantconnect.lean.orders.OrderTypes.OrderDuration;
import com.quantconnect.lean.orders.OrderTypes.OrderStatus;
import com.quantconnect.lean.orders.OrderTypes.OrderType;
import com.quantconnect.lean.Global.SecurityType;

/*
 *  Order struct for placing new trade
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

    /// Get the absolute quantity for this order
    public int getAbsoluteQuantity() {
        return Math.abs( quantity );
    }

    /// Gets the executed value of this order. If the order has not yet filled,
    /// then this will return zero.
    public BigDecimal getValue() {
        return price.multiply( BigDecimal.valueOf( quantity ) );
    }

    /// Added a default constructor for JSON Deserialization:
    protected Order() {
        this( Symbol.EMPTY, 0, LocalDateTime.now(), "" );
    }

    /// <summary>
    /// New order constructor
    /// </summary>
    /// <param name="symbol">Symbol asset we're seeking to trade</param>
    /// <param name="quantity">Quantity of the asset we're seeking to trade</param>
    /// <param name="time">Time the order was placed</param>
    /// <param name="tag">User defined data tag for this order</param>
    protected Order( Symbol symbol, int quantity, LocalDateTime time ) {
        this( symbol, quantity, time, "" );
    }
    
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

    /// <summary>
    /// Gets the value of this order at the given market price in units of the account currency
    /// NOTE: Some order types derive value from other parameters, such as limit prices
    /// </summary>
    /// <param name="security">The security matching this order's symbol</param>
    /// <returns>The value of this order given the current market price</returns>
    public BigDecimal getValue( Security security ) {
        value = getValueImpl( security );
        return value*security.QuoteCurrency.ConversionRate*security.SymbolProperties.ContractMultiplier;
    }

    /// Gets the order value in units of the security's quote currency for a single unit.
    /// A single unit here is a single share of stock, or a single barrel of oil, or the
    /// cost of a single share in an option contract.
    /// <param name="security">The security matching this order's symbol</param>
    protected abstract BigDecimal getValueImpl( Security security );

    /// Modifies the state of this order to match the update request
    /// <param name="request">The request to update this order object</param>
    public /*virtual*/ void ApplyUpdateOrderRequest( UpdateOrderRequest request ) {
        if( request.OrderId != id )
            throw new IllegalArgumentException( "Attempted to apply updates to the incorrect order!" );

        if( request.Quantity.HasValue )
            quantity = request.Quantity.Value;

        if( request.Tag != null )
            tag = request.Tag;
    }

    /// Returns a String that represents the current object.
    /// <returns>
    /// A String that represents the current object.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public String toString() {
        return String.format( "OrderId: %d %s {2} order for %d unit%s of %s", id, status, Type, quantity, quantity == 1 ? "" : "s", symbol );
    }

    /// Creates a deep-copy clone of this order
    /// <returns>A copy of this order</returns>
    public abstract Order clone();

    /// Copies base Order properties to the specified order
    /// <param name="order">The target of the copy</param>
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

    /// Creates an <see cref="Order"/> to match the specified <paramref name="request"/>
    /// <param name="request">The <see cref="SubmitOrderRequest"/> to create an order for</param>
    /// <returns>The <see cref="Order"/> that matches the request</returns>
    public static Order createOrder( SubmitOrderRequest request ) {
        Order order;
        switch( request.OrderType ) {
            case OrderType.Market:
                order = new MarketOrder( request.Symbol, request.Quantity, request.Time, request.Tag );
                break;
            case OrderType.Limit:
                order = new LimitOrder( request.Symbol, request.Quantity, request.LimitPrice, request.Time, request.Tag );
                break;
            case OrderType.StopMarket:
                order = new StopMarketOrder( request.Symbol, request.Quantity, request.StopPrice, request.Time, request.Tag );
                break;
            case OrderType.StopLimit:
                order = new StopLimitOrder( request.Symbol, request.Quantity, request.StopPrice, request.LimitPrice, request.Time, request.Tag );
                break;
            case OrderType.MarketOnOpen:
                order = new MarketOnOpenOrder( request.Symbol, request.Quantity, request.Time, request.Tag );
                break;
            case OrderType.MarketOnClose:
                order = new MarketOnCloseOrder( request.Symbol, request.Quantity, request.Time, request.Tag );
                break;
            default:
                throw new IllegalArgumentException();
        }
        
        order.status = OrderStatus.New;
        order.id = request.OrderId;
        
        if( request.Tag != null )
            order.tag = request.Tag;

        return order;
    }

    /// Order Expiry on a specific UTC time.
    public LocalDateTime DurationValue;
}
