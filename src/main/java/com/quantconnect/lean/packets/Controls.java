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
 *
*/

package com.quantconnect.lean.packets;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * Specifies values used to control algorithm limits
 */
public class Controls {
    
    /**
     * The maximum number of minute symbols
     */
    @JsonProperty( "iMinuteLimit" )
    public int minuteLimit = 500;
    
    /**
     * The maximum number of second symbols
     */
    @JsonProperty( "iSecondLimit" )
    public int secondLimit = 100;

    /**
     * The maximum number of tick symbol
     */
    @JsonProperty( "iTickLimit" )
    public int tickLimit = 30;

    /**
     * Initializes a new default instance of the <see cref="Controls"/> class
     */
    public Controls() { }

    public Controls( int minuteLimit, int secondLimit, int tickLimit ) {
        this.minuteLimit = minuteLimit;
        this.secondLimit = secondLimit;
        this.tickLimit = tickLimit;
    }
}