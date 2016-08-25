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
 * Market on close order type - submits a market order on exchange close
*/
public class MarketOnCloseOrder extends Order {
    
    /**
     * MarketOnClose Order Type
     */
    @Override
    public OrderType getType() {
        return OrderType.MarketOnClose;
    }

    /**
     * Initializes a new instance of the <see cref="MarketOnCloseOrder"/> class.
     */
    public MarketOnCloseOrder() { }

    /**
     * Initializes a new instance of the <see cref="MarketOnCloseOrder"/> class.
     * @param symbol The security's symbol being ordered
     * @param quantity The number of units to order
     * @param time The current time
     * @param tag A user defined tag for the order
     */
    public MarketOnCloseOrder( Symbol symbol, int quantity, LocalDateTime time ) {
        this( symbol, quantity, time, "" );
    }
    
    /**
     * Initializes a new instance of the <see cref="MarketOnCloseOrder"/> class.
     * @param symbol The security's symbol being ordered
     * @param quantity The number of units to order
     * @param time The current time
     * @param tag A user defined tag for the order
     */
    public MarketOnCloseOrder( Symbol symbol, int quantity, LocalDateTime time, String tag ) {
        super( symbol, quantity, time, tag );
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
        final MarketOnCloseOrder order = new MarketOnCloseOrder();
        copyTo( order );
        return order;
    }
}
