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
import java.util.Optional;
import java.util.OptionalInt;

/**
 * Specifies the data in an order to be updated
 */
public class UpdateOrderFields {
  
    /**
     * Specify to update the quantity of the order
     */
    private OptionalInt quantity;

    /**
     * Specify to update the limit price of the order
     */
    private Optional<BigDecimal> limitPrice;

    /**
     * Specify to update the stop price of the order
     */
    private Optional<BigDecimal> stopPrice;

    /**
     * Specify to update the order's tag
     */
    private String tag;
    

    public OptionalInt getQuantity() {
        return quantity;
    }

    public void setQuantity( OptionalInt quantity ) {
        this.quantity = quantity;
    }

    public Optional<BigDecimal> getLimitPrice() {
        return limitPrice;
    }

    public void setLimitPrice( Optional<BigDecimal> limitPrice ) {
        this.limitPrice = limitPrice;
    }

    public Optional<BigDecimal> getStopPrice() {
        return stopPrice;
    }

    public void setStopPrice( Optional<BigDecimal> stopPrice ) {
        this.stopPrice = stopPrice;
    }

    public String getTag() {
        return tag;
    }

    public void setTag( String tag ) {
        this.tag = tag;
    }
}