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
 * Stop Market Order Class Definition
 */
public class StopMarketOrder extends Order {
        
    /**
     * Stop price for this stop market order.
     */
    private BigDecimal stopPrice;

    public BigDecimal getStopPrice() {
        return stopPrice;
    }

    /**
     * StopMarket Order Type
     */
    @Override
    public OrderType getType() {
        return OrderType.StopMarket;
    }

    /**
     * Default constructor for JSON Deserialization:
     */
    /*public*/ StopMarketOrder() { }

    /**
     * New Stop Market Order constructor - 
     * @param symbol Symbol asset we're seeking to trade
     * @param quantity Quantity of the asset we're seeking to trade
     * @param time Time the order was placed
     * @param stopPrice Price the order should be filled at if a limit order
     */
    public StopMarketOrder( Symbol symbol, int quantity, BigDecimal stopPrice, LocalDateTime time ) {
        this( symbol, quantity, stopPrice, time, "" );
    }
    
    /**
     * New Stop Market Order constructor - 
     * @param symbol Symbol asset we're seeking to trade
     * @param quantity Quantity of the asset we're seeking to trade
     * @param time Time the order was placed
     * @param stopPrice Price the order should be filled at if a limit order
     * @param tag User defined data tag for this order
     */
    public StopMarketOrder( Symbol symbol, int quantity, BigDecimal stopPrice, LocalDateTime time, String tag ) {
        super( symbol, quantity, time, tag );
        this.stopPrice = stopPrice;

        if( tag.equals( "" ) ) {
            //Default tag values to display stop price in GUI.
            this.tag = "Stop Price: " + NumberFormat.getCurrencyInstance().format( stopPrice );
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
            return stopPrice.max( security.getPrice() ).multiply( BigDecimal.valueOf( quantity ) );

        // buying, so lower price will be used
        if( quantity > 0 )
            return stopPrice.min( security.getPrice() ).multiply( BigDecimal.valueOf( quantity ) );

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
    }

    /**
     * Returns a String that represents the current object.
     * @returns  String that represents the current object.
     */
    @Override
    public String toString() {
        return String.format( "%1$s at stop %2$s", super.toString(), Extensions.smartRounding( stopPrice ) );
    }

    /**
     * Creates a deep-copy clone of this order
     * @returns A copy of this order
     */
    @Override
    public Order clone() {
        final StopMarketOrder order = new StopMarketOrder();
        order.stopPrice = this.stopPrice;
        copyTo( order );
        return order;
    }
}
