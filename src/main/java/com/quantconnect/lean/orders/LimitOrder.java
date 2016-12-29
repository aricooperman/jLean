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
import java.text.NumberFormat;
import java.time.LocalDateTime;

import com.quantconnect.lean.Extensions;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.orders.OrderTypes.OrderType;
import com.quantconnect.lean.securities.Security;

/**
 * Limit order type definition
 */
public class LimitOrder extends Order {
 
    /**
     * Limit price for this order.
     */
    private BigDecimal limitPrice;

    public BigDecimal getLimitPrice() {
        return limitPrice;
    }

    void setLimitPrice( final BigDecimal limitPrice ) {
        this.limitPrice = limitPrice;
    }

    /**
     * Limit Order Type
     */
    @Override
    public OrderType getType() {
        return OrderType.Limit;
    }

    /**
     * Added a default constructor for JSON Deserialization:
     */
    public LimitOrder() { }

    /**
     * New limit order constructor
     * @param symbol Symbol asset we're seeking to trade
     * @param quantity Quantity of the asset we're seeking to trade
     * @param time Time the order was placed
     * @param limitPrice Price the order should be filled at if a limit order
     */
    public LimitOrder( final Symbol symbol, final int quantity, final BigDecimal limitPrice, final LocalDateTime time ) {
        this( symbol, quantity, limitPrice, time, "" );
    }
        
    /**
     * New limit order constructor
     * @param symbol Symbol asset we're seeking to trade
     * @param quantity Quantity of the asset we're seeking to trade
     * @param time Time the order was placed
     * @param limitPrice Price the order should be filled at if a limit order
     * @param tag User defined data tag for this order
     */
    public LimitOrder( final Symbol symbol, final int quantity, final BigDecimal limitPrice, final LocalDateTime time, final String tag ) {
        super( symbol, quantity, time, tag );
        this.limitPrice = limitPrice;

        if( "".equals( tag ) ) {
            //Default tag values to display limit price in GUI.
            this.tag = "Limit Price: " + NumberFormat.getCurrencyInstance().format( limitPrice );
        }
    }

    /**
     * Gets the order value in units of the security's quote currency
     * @param security The security matching this order's symbol
     */
    @Override
    protected BigDecimal getValueImpl( final Security security ) {
        // selling, so higher price will be used
        if( quantity < 0 )
            return limitPrice.max( security.getPrice() ).multiply( BigDecimal.valueOf( quantity ) );

        // buying, so lower price will be used
        if( quantity > 0)
            return limitPrice.min( security.getPrice() ).multiply( BigDecimal.valueOf( quantity ) );

        return BigDecimal.ZERO;
    }

    /**
     * Modifies the state of this order to match the update request
     * @param request The request to update this order object
     */
    @Override
    public void applyUpdateOrderRequest( final UpdateOrderRequest request ) {
        super.applyUpdateOrderRequest( request );
        if( request.getLimitPrice().isPresent() )
            limitPrice = request.getLimitPrice().get();
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
     @Override
    public String toString() {
        return String.format( "%1$s at limit %2$s", super.toString(), Extensions.smartRounding( limitPrice ) );
    }

    /**
     * Creates a deep-copy clone of this order
     * @returns A copy of this order
     */
    @Override
    public Order clone() {
        final LimitOrder order = new LimitOrder();
        order.limitPrice = limitPrice;
        copyTo( order );
        return order;
    }
}
