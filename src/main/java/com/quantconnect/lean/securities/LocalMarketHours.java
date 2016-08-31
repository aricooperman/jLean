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

import java.time.DayOfWeek;
import java.time.Duration;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Collections;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

/**
 * Represents the market hours under normal conditions for an exchange and a specific day of the week in terms of local time
 */
public class LocalMarketHours {
    
    private final boolean hasPreMarket;
    private final boolean hasPostMarket;
    private final boolean isOpenAllDay;
    private final boolean isClosedAllDay;
    private final DayOfWeek dayOfWeek;
    private final MarketHoursSegment[] segments;

    /**
     * Gets whether or not this exchange is closed all day
     */
    public boolean isClosedAllDay() {
        return isClosedAllDay;
    }

    /**
     * Gets whether or not this exchange is closed all day
     */
    public boolean isOpenAllDay() {
        return isOpenAllDay;
    }

    /**
     * Gets the day of week these hours apply to
     */
    public DayOfWeek getDayOfWeek() {
        return dayOfWeek;
    }

    /**
     * Gets the individual market hours segments that define the hours of operation for this day
     */
    public Iterable<MarketHoursSegment> getSegments() {
        return Collections.unmodifiableList( Arrays.asList( segments ) );
    }

    /**
     * Initializes a new instance of the <see cref="LocalMarketHours"/> class
     * @param day The day of the week these hours are applicable
     * @param segments The open/close segments defining the market hours for one day
     */
    public LocalMarketHours( DayOfWeek day, MarketHoursSegment... segments ) {
        this( day, Arrays.asList( segments ) );
    }

    /**
     * Initializes a new instance of the <see cref="LocalMarketHours"/> class
     * @param day The day of the week these hours are applicable
     * @param segments The open/close segments defining the market hours for one day
     */
    public LocalMarketHours( DayOfWeek day, Collection<MarketHoursSegment> segments ) {
        this.dayOfWeek = day;
        // filter out the closed states, we'll assume closed if no segment exists
        this.segments = (segments != null ? segments : Collections.<MarketHoursSegment>emptySet()).stream()
                .filter( x -> x.getState() != MarketHoursState.Closed )
                .toArray( i -> new MarketHoursSegment[i] );
        this.isClosedAllDay = this.segments.length == 0;
        this.isOpenAllDay = this.segments.length == 1 
            && this.segments[0].getStart().equals( Duration.ZERO ) 
            && this.segments[0].getEnd().equals( Duration.ofDays( 1 ) )
            && this.segments[0].getState() == MarketHoursState.Market;

        boolean preMarket = false, postMarket = false;
        for( MarketHoursSegment segment : segments ) {
            final MarketHoursState state = segment.getState();
            if( state == MarketHoursState.PreMarket )
                preMarket = true;
            
            if( state == MarketHoursState.PostMarket )
                postMarket = true;
        }
        
        hasPreMarket = preMarket;
        hasPostMarket = postMarket;
    }

    /**
     * Initializes a new instance of the <see cref="LocalMarketHours"/> class from the specified open/close times
     * @param day The day of week these hours apply to
     * @param extendedMarketOpen The extended market open time
     * @param marketOpen The regular market open time, must be greater than or equal to the extended market open time
     * @param marketClose The regular market close time, must be greater than the regular market open time
     * @param extendedMarketClose The extended market close time, must be greater than or equal to the regular market close time
     */
    public LocalMarketHours( DayOfWeek day, Duration extendedMarketOpen, Duration marketOpen, Duration marketClose, Duration extendedMarketClose ) {
        this.dayOfWeek = day;

        final List<MarketHoursSegment> segments = new ArrayList<MarketHoursSegment>();

        if( extendedMarketOpen != marketOpen ) {
            hasPreMarket = true;
            segments.add(new MarketHoursSegment(MarketHoursState.PreMarket, extendedMarketOpen, marketOpen));
        }
        else
            this.hasPreMarket = false;

        if( marketOpen != Duration.ZERO || marketClose != Duration.ZERO )
            segments.add(new MarketHoursSegment(MarketHoursState.Market, marketOpen, marketClose));

        if( marketClose != extendedMarketClose ) {
            hasPostMarket = true;
            segments.add(new MarketHoursSegment(MarketHoursState.PostMarket, marketClose, extendedMarketClose));
        }
        else
            hasPostMarket = true;

        this.segments = segments.toArray( new MarketHoursSegment[segments.size()] );
        this.isClosedAllDay = this.segments.length == 0;
        this.isOpenAllDay = false;

        // perform some sanity checks
        if( marketOpen.compareTo( extendedMarketOpen ) < 0 )
            throw new IllegalArgumentException( "Extended market open time must be less than or equal to market open time.");
        
        if( marketClose.compareTo( marketOpen ) < 0 )
            throw new IllegalArgumentException( "Market close time must be after market open time.");
        
        if( extendedMarketClose.compareTo( marketClose ) < 0 )
            throw new IllegalArgumentException( "Extended market close time must be greater than or equal to market close time.");
    }

    /**
     * Initializes a new instance of the <see cref="LocalMarketHours"/> class from the specified open/close times
     * using the market open as the extended market open and the market close as the extended market close, effectively
     * removing any 'extended' session from these exchange hours
     * @param day The day of week these hours apply to
     * @param marketOpen The regular market open time
     * @param marketClose The regular market close time, must be greater than the regular market open time
     */
    public LocalMarketHours( DayOfWeek day, Duration marketOpen, Duration marketClose ) {
        this( day, marketOpen, marketOpen, marketClose, marketClose );
    }

    /**
     * Gets the market opening time of day
     * @param time The reference time, the open returned will be the first open after the specified time if there are multiple market open segments
     * @param extendedMarket True to include extended market hours, false for regular market hours
     * @returns The market's opening time of day
     */
    public Optional<Duration> getMarketOpen( Duration time, boolean extendedMarket ) {
        for (int i = 0; i < segments.length; i++) {
            if( segments[i].getState() == MarketHoursState.Closed || segments[i].getEnd().compareTo( time ) <= 0 )
                continue;

            if( extendedMarket && hasPreMarket) {
                if( segments[i].getState() == MarketHoursState.PreMarket )
                    return Optional.of( segments[i].getStart() );
            }
            else if( segments[i].getState() == MarketHoursState.Market )
                return Optional.of( segments[i].getStart() );
        }

        // we couldn't locate an open segment after the specified time
        return Optional.empty();
    }

    /**
     * Gets the market closing time of day
     * @param time The reference time, the close returned will be the first close after the specified time if there are multiple market open segments
     * @param extendedMarket True to include extended market hours, false for regular market hours
     * @returns The market's closing time of day
     */
    public Optional<Duration> getMarketClose( Duration time, boolean extendedMarket ) {
        for (int i = 0; i < segments.length; i++) {
            if( segments[i].getState() == MarketHoursState.Closed || segments[i].getEnd().compareTo( time ) <= 0 )
                continue;

            if( extendedMarket && hasPostMarket) {
                if( segments[i].getState() == MarketHoursState.PostMarket )
                    return Optional.of( segments[i].getEnd() );
            }
            else if( segments[i].getState() == MarketHoursState.Market )
                return Optional.of( segments[i].getEnd() );
        }
        
        // we couldn't locate an open segment after the specified time
        return Optional.empty();
    }

    /**
     * Determines if the exchange is open at the specified time
     * @param time The time of day to check
     * @param extendedMarket True to check exended market hours, false to check regular market hours
     * @returns True if the exchange is considered open, false otherwise
     */
    public boolean isOpen( Duration time, boolean extendedMarket ) {
        for (int i = 0; i < segments.length; i++ ) {
            if( segments[i].getState() == MarketHoursState.Closed )
                continue;

            if( segments[i].contains( time ) )
                return extendedMarket || segments[i].getState() == MarketHoursState.Market;
        }

        // if we didn't find a segment then we're closed
        return false;
    }

    /**
     * Determines if the exchange is open during the specified interval
     * @param start The start time of the interval
     * @param end The end time of the interval
     * @param extendedMarket True to check exended market hours, false to check regular market hours
     * @returns True if the exchange is considered open, false otherwise
     */
    public boolean isOpen( Duration start, Duration end, boolean extendedMarket ) {
        if( start == end )
            return isOpen( start, extendedMarket );
        
        for( int i = 0; i < segments.length; i++ ) {
            if( segments[i].getState() == MarketHoursState.Closed )
                continue;

            if( extendedMarket || segments[i].getState() == MarketHoursState.Market ) {
                if( segments[i].overlaps( start, end ) )
                    return true;
            }
        }

        // if we didn't find a segment then we're closed
        return false;
    }

    /**
     * Gets a <see cref="LocalMarketHours"/> instance that is always closed
     * @param dayOfWeek The day of week
     * @returns A <see cref="LocalMarketHours"/> instance that is always closed
     */
    public static LocalMarketHours closedAllDay( DayOfWeek dayOfWeek ) {
        return new LocalMarketHours( dayOfWeek );
    }

    /**
     * Gets a <see cref="LocalMarketHours"/> instance that is always open
     * @param dayOfWeek The day of week
     * @returns A <see cref="LocalMarketHours"/> instance that is always open
     */
    public static LocalMarketHours openAllDay( DayOfWeek dayOfWeek ) {
        return new LocalMarketHours( dayOfWeek, new MarketHoursSegment( MarketHoursState.Market, Duration.ZERO, Duration.ofDays( 1 ) ) );
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        if( isClosedAllDay() )
            return "Closed All Day";
        
        if( isOpenAllDay() )
            return "Open All Day";
        
        return getDayOfWeek() + ": " + Arrays.stream( segments ).map( MarketHoursSegment::toString ).collect( Collectors.joining( " | " ) );
    }
}