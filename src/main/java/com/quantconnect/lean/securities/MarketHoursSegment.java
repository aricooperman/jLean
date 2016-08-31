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

import java.time.Duration;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * Represents the state of an exchange during a specified time range
 */
//TODO [JsonObject(MemberSerialization.OptIn)]
public class MarketHoursSegment {
    
    @JsonProperty( "start" )
    private Duration start;

    /**
     * Gets the start time for this segment
     */
    public Duration getStart() {
        return start;
    }

    @JsonProperty( "end" )
    private Duration end;

    /**
     * Gets the end time for this segment
     */
    public Duration getEnd() {
        return end;
    }

    @JsonProperty( "state" )
    private MarketHoursState state;

    /**
     * Gets the market hours state for this segment
     */
    public MarketHoursState getState() {
        return state;
    }

    /**
     * Initializes a new instance of the <see cref="MarketHoursSegment"/> class
     * @param state The state of the market during the specified times
     * @param start The start time of the segment
     * @param end The end time of the segment
     */
    public MarketHoursSegment( MarketHoursState state, Duration start, Duration end) {
        this.start = start;
        this.end = end;
        this.state = state;
    }

    /**
     * Gets a new market hours segment representing being open all day
     */
    public static MarketHoursSegment openAllDay() {
        return new MarketHoursSegment( MarketHoursState.Market, Duration.ZERO, Duration.ofDays( 1 ) );
    }

    /**
     * Gets a new market hours segment representing being open all day
     */
    public static MarketHoursSegment closedAllDay() {
        return new MarketHoursSegment( MarketHoursState.Closed, Duration.ZERO, Duration.ofDays( 1 ) );
    }

    /**
     * Determines whether or not the specified time is contained within this segment
     * @param time The time to check
     * @returns True if this segment contains the specified time, false otherwise
     */
    public boolean contains( Duration time ) {
        return time.compareTo( start ) >= 0 && time.compareTo( end ) < 0;
    }

    /**
     * Determines whether or not the specified time range overlaps with this segment
     * @param start The start of the range
     * @param end The end of the range
     * @returns True if the specified range overlaps this time segment, false otherwise
     */
    public boolean overlaps( Duration start, Duration end ) {
        return this.start.compareTo( end ) < 0 && this.end.compareTo( start ) > 0;
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        return String.format( "%1$s: %2$s-%3$s", state, start, end );
    }
}