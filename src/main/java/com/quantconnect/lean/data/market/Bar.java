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

package com.quantconnect.lean.data.market;

import java.math.BigDecimal;

/**
 * Base Bar Class: Open, High, Low, Close and Period.
 */
public class Bar implements IBar, Cloneable {

    /**
     * Opening price of the bar: Defined as the price at the start of the time period.
     */
    private BigDecimal open;

    /**
     * High price of the bar during the time period.
     */
    private BigDecimal high;

    /**
     * Low price of the bar during the time period.
     */
    private BigDecimal low;

    /**
     * Closing price of the bar. Defined as the price at Start Time + TimeSpan.
     */
    private BigDecimal close;

    /**
     * Default initializer to setup an empty bar.
     */
    public Bar() { }

    /**
     * Initializer to setup a bar with a given information.
     * @param open Decimal Opening Price
     * @param high Decimal High Price of this bar
     * @param low Decimal Low Price of this bar
     * @param close Decimal Close price of this bar
     */
    public Bar( BigDecimal open, BigDecimal high, BigDecimal low, BigDecimal close ) {
        this.open = open;
        this.high = high;
        this.low = low;
        this.close = close;
    }

    public BigDecimal getOpen() {
        return open;
    }

    public void setOpen( BigDecimal open ) {
        this.open = open;
    }

    public BigDecimal getHigh() {
        return high;
    }

    public void setHigh( BigDecimal high ) {
        this.high = high;
    }

    public BigDecimal getLow() {
        return low;
    }

    public void setLow( BigDecimal low ) {
        this.low = low;
    }

    public BigDecimal getClose() {
        return close;
    }

    public void setClose( BigDecimal close ) {
        this.close = close;
    }

    /**
     * Updates the bar with a new value. This will aggregate the OHLC bar
     * @param value The new value
     */
    public void update( BigDecimal value ) {
        // Do not accept zero as a new value
        if( value.signum() == 0) 
            return;

        if( open.signum() == 0) 
            open = high = low = close = value;
        if( value.compareTo( high ) > 0 ) 
            high = value;
        if( value.compareTo( low ) < 0 ) 
            low = value;
        
        close = value;
    }

    /**
     * Returns a clone of this bar
     */
    public Bar clone() {
        return new Bar( open, high, low, close );
    }
}
