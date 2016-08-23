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

package com.quantconnect.lean.securities;

/**
 * Specifies the open/close state for a <see cref="MarketHoursSegment"/>
 */
public enum MarketHoursState {
    
    /**
     * The market is not open
     */
    Closed( "closed" ),

    /**
     * The market is open, but before normal trading hours
     */
    PreMarket( "premarket" ),

    /**
     * The market is open and within normal trading hours
     */
    Market( "market" ),

    /**
     * The market is open, but after normal trading hours
    */
    PostMarket( "postmarket" );
    
    private final String value;

    MarketHoursState( String value ) {
        this.value = value;
    }
    
    public String toString() {
        return value;
    }
}