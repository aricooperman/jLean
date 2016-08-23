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
    /**
     * Gets the start time for this segment
     */
    @JsonProperty( "start" )
    private Duration Start;
    //{ get; private set; }

    /**
     * Gets the end time for this segment
     */
    @JsonProperty( "end" )
    private Duration End;
    //{ get; private set; }

    /**
     * Gets the market hours state for this segment
     */
    @JsonProperty( "state" )
    private MarketHoursState State;
    //{ get; private set; }

    /**
     * Initializes a new instance of the <see cref="MarketHoursSegment"/> class
     * @param state The state of the market during the specified times
     * @param start The start time of the segment
     * @param end The end time of the segment
     */
    public MarketHoursSegment(MarketHoursState state, Duration start, Duration end) {
        this.Start = start;
        this.End = end;
        this.State = state;
    }

    /**
     * Gets a new market hours segment representing being open all day
     */
    public static MarketHoursSegment openAllDay() {
        return new MarketHoursSegment( MarketHoursState.Market, Duration.ZERO, Time.OneDay );
    }

    /**
    /// Gets a new market hours segment representing being open all day
    */
    public static MarketHoursSegment closedAllDay() {
        return new MarketHoursSegment( MarketHoursState.Closed, Duration.ZERO, Time.OneDay );
    }

    /**
    /// Determines whether or not the specified time is contained within this segment
    */
     * @param time">The time to check
    @returns True if this segment contains the specified time, false otherwise
    public boolean Contains(TimeSpan time) {
        return time >= Start && time < End;
    }

    /**
    /// Determines whether or not the specified time range overlaps with this segment
    */
     * @param start">The start of the range
     * @param end">The end of the range
    @returns True if the specified range overlaps this time segment, false otherwise
    public boolean Overlaps(TimeSpan start, Duration end) {
        return Start < end && End > start;
    }

    /**
    /// Returns a String that represents the current object.
    */
    @returns 
    /// A String that represents the current object.
    /// 
    public @Override String toString() {
        return String.format( "%1$s: %2$s-%3$s", State, Start, End);
    }
}