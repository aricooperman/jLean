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
 * Stop Market Order Type Definition
 */
public class StopLimitOrder extends Order {
   
    /**
     * Stop price for this stop market order.
     */
    private BigDecimal stopPrice;

    /**
     * Signal showing the "StopLimitOrder" has been converted into a Limit Order
     */
    private boolean stopTriggered;

    /**
     * Limit price for the stop limit order
     */
    private BigDecimal limitPrice;

    /**
     * StopLimit Order Type
     */
    @Override
    public OrderType getType() {
        return OrderType.StopLimit;
    }

    public BigDecimal getStopPrice() {
        return stopPrice;
    }
    
    protected void setStopPrice( BigDecimal stopPrice ) {
        this.stopPrice = stopPrice;
    }
    
    public boolean isStopTriggered() {
        return stopTriggered;
    }
    
    protected void setStopTriggered( boolean stopTriggered ) {
        this.stopTriggered = stopTriggered;
    }
    
    public BigDecimal getLimitPrice() {
        return limitPrice;
    }
    
    protected void setLimitPrice( BigDecimal limitPrice ) {
        this.limitPrice = limitPrice;
    }
    
    /**
     * Default constructor for JSON Deserialization:
     */
    /*public*/ StopLimitOrder() { }

    /**
     * New Stop Market Order constructor - 
     * @param symbol Symbol asset we're seeking to trade
     * @param quantity Quantity of the asset we're seeking to trade
     * @param limitPrice Maximum price to fill the order
     * @param time Time the order was placed
     * @param stopPrice Price the order should be filled at if a limit order
     * @param tag User defined data tag for this order
     */
    public StopLimitOrder( Symbol symbol, int quantity, BigDecimal stopPrice, BigDecimal limitPrice, LocalDateTime time ) {
        this( symbol, quantity, stopPrice, limitPrice, time, "" );
    }
    
    /**
     * New Stop Market Order constructor - 
     * @param symbol Symbol asset we're seeking to trade
     * @param quantity Quantity of the asset we're seeking to trade
     * @param limitPrice Maximum price to fill the order
     * @param time Time the order was placed
     * @param stopPrice Price the order should be filled at if a limit order
     * @param tag User defined data tag for this order
     */
    public StopLimitOrder( Symbol symbol, int quantity, BigDecimal stopPrice, BigDecimal limitPrice, LocalDateTime time, String tag ) {
        super( symbol, quantity, time, tag );
        this.stopPrice = stopPrice;
        this.limitPrice = limitPrice;

        if( tag.equals( "" ) ) {
            //Default tag values to display stop price in GUI.
            final NumberFormat formatter = NumberFormat.getCurrencyInstance();
            this.tag = "Stop Price: " + formatter.format( stopPrice ) + " Limit Price: " + formatter.format( limitPrice );
        }
    }

    /**
     * Gets the order value in units of the security's quote currency
     * @param security The security matching this order's symbol
     */
    @Override
    protected BigDecimal getValueImpl( Security security ) {
        // selling, so higher price will be used
        if( quantity < 0 )
            return limitPrice.max( security.getPrice() ).multiply( BigDecimal.valueOf( quantity ) );

        // buying, so lower price will be used
        if( quantity > 0 )
            return limitPrice.min( security.getPrice() ).multiply( BigDecimal.valueOf( quantity ) );

        return BigDecimal.ZERO;
    }

    /**
     * Modifies the state of this order to match the update request
     * @param request The request to update this order object
     */
    @Override
    public void applyUpdateOrderRequest( UpdateOrderRequest request ) {
        super.applyUpdateOrderRequest( request );
        stopPrice = request.getStopPrice().orElse( stopPrice );
        limitPrice = request.getLimitPrice().orElse( limitPrice );
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        return String.format( "%1$s at stop %2$s limit %3$s", super.toString(), Extensions.smartRounding( stopPrice ), Extensions.smartRounding( limitPrice ) );
    }

    /**
     * Creates a deep-copy clone of this order
     * @returns A copy of this order
     */
    @Override
    public Order clone() {
        final StopLimitOrder order = new StopLimitOrder();
        order.stopPrice = this.stopPrice;
        order.limitPrice = this.limitPrice;
        copyTo( order );
        return order;
    }
}
