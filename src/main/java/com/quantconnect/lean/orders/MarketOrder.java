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

import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.orders.OrderTypes.OrderType;
import com.quantconnect.lean.securities.Security;

/**
 * Market order type definition
*/
public class MarketOrder extends Order {
    
    /**
     * Added a default constructor for JSON Deserialization:
     */
    /*public*/ MarketOrder() { }

    /**
     * New market order constructor
     * @param symbol Symbol asset we're seeking to trade
     * @param quantity Quantity of the asset we're seeking to trade
     * @param time Time the order was placed
     */
    public MarketOrder( Symbol symbol, int quantity, LocalDateTime time ) {
        this( symbol, quantity, time, "" );
    }
    
    /**
     * New market order constructor
     * @param symbol Symbol asset we're seeking to trade
     * @param quantity Quantity of the asset we're seeking to trade
     * @param time Time the order was placed
     * @param tag User defined data tag for this order
     */
    public MarketOrder( Symbol symbol, int quantity, LocalDateTime time, String tag ) {
        super( symbol, quantity, time, tag );
    }
    
    /**
     * Market Order Type
     */
    @Override
    public OrderType getType() {
        return OrderType.Market;
    }

    /**
     * Gets the order value in units of the security's quote currency
     * @param security The security matching this order's symbol
     */
    @Override
    protected BigDecimal getValueImpl( Security security ) {
        return security.getPrice().multiply( BigDecimal.valueOf( quantity ) );
    }

    /**
     * Creates a deep-copy clone of this order
     * @returns A copy of this order
    */
    @Override
    public Order clone() {
        final MarketOrder order = new MarketOrder();
        copyTo( order );
        return order;
    }
}
