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

using System;
using Newtonsoft.Json;

package com.quantconnect.lean.Packets
{

    /**
     * Market today information class
    */
    public class MarketToday
    {
        /**
         * Date this packet was generated.
        */
        @JsonProperty( "date")]
        public DateTime Date { get; set; }

        /**
         * Given the dates and times above, what is the current market status - open or closed.
        */
        @JsonProperty( "status")]
        public String Status = "";

        /**
         * Premarket hours for today
        */
        @JsonProperty( "premarket")]
        public MarketHours PreMarket;

        /**
         * Normal trading market hours for today
        */
        @JsonProperty( "open")]
        public MarketHours Open;

        /**
         * Post market hours for today
        */
        @JsonProperty( "postmarket")]
        public MarketHours PostMarket;

        /**
         * Default constructor (required for JSON serialization)
        */
        public MarketToday() { }
    }

    /**
     * Market open hours model for pre, normal and post market hour definitions.
    */
    public class MarketHours
    {
        /**
         * Start time for this market hour category
        */
        @JsonProperty( "start")]
        public DateTime Start;

        /**
         * End time for this market hour category
        */
        @JsonProperty( "end")]
        public DateTime End;

        /**
         * Market hours initializer given an hours since midnight measure for the market hours today
        */
         * @param referenceDate Reference date used for as base date from the specified hour offsets
         * @param defaultStart Time in hours since midnight to start this open period.
         * @param defaultEnd Time in hours since midnight to end this open period.
        public MarketHours(DateTime referenceDate, double defaultStart, double defaultEnd) {
            Start = referenceDate.Date.AddHours(defaultStart);
            End = referenceDate.Date.AddHours(defaultEnd);
            if( defaultEnd == 24) {
                // when we mark it as the end of the day other code that relies on .TimeOfDay has issues
                End = End.AddTicks(-1);
            }
        }
    }
}
