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
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.time.temporal.ChronoUnit;
import java.util.Arrays;
import java.util.Collections;
import java.util.EnumMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Optional;
import java.util.Set;
import java.util.function.Function;
import java.util.stream.Collectors;
import java.util.stream.StreamSupport;

import com.quantconnect.lean.Extensions;


/**
 * Represents the schedule of a security exchange. This includes daily regular and extended market hours
 * as well as holidays
 * 
 * This type assumes that IsOpen will be called with increasingly future times, that is, the calls should never back
 * track in time. This assumption is required to prevent time zone conversions on every call.
 */
public class SecurityExchangeHours {
    private final ZoneId timeZone;
    private final Set<LocalDate> holidays;

    // these are listed individually for speed
    private final LocalMarketHours sunday;
    private final LocalMarketHours monday;
    private final LocalMarketHours tuesday;
    private final LocalMarketHours wednesday;
    private final LocalMarketHours thursday;
    private final LocalMarketHours friday;
    private final LocalMarketHours saturday;
    private final Map<DayOfWeek,LocalMarketHours> openHoursByDay;

    /**
     * Gets the time zone this exchange resides in
     */
    public ZoneId getTimeZone() {
        return timeZone;
    }

    /**
     * Gets the holidays for the exchange
     */
    public Set<LocalDate> getHolidays() {
        return holidays; //TODO read only
    }

    /**
     * Gets the market hours for this exchange
     */    
    public Map<DayOfWeek,LocalMarketHours> getMarketHours() {
        return openHoursByDay;
    }

    /**
     * Gets a <see cref="SecurityExchangeHours"/> instance that is always open
     */
    public static SecurityExchangeHours alwaysOpen( ZoneId timeZone ) {
        final DayOfWeek[] dayOfWeeks = DayOfWeek.values();
        return new SecurityExchangeHours( timeZone,
            Collections.emptySet(),
            Arrays.stream( dayOfWeeks ).collect( Collectors.toMap( Function.identity(), LocalMarketHours::openAllDay ) ) );
    }

    /**
     * Initializes a new instance of the <see cref="SecurityExchangeHours"/> class
     * @param timeZone The time zone the dates and hours are represented in
     * @param holidayDates The dates this exchange is closed for holiday
     * @param marketHoursForEachDayOfWeek The exchange's schedule for each day of the week
     */
    public SecurityExchangeHours( final ZoneId timeZone, final Iterable<LocalDate> holidayDates, final Map<DayOfWeek,LocalMarketHours> marketHoursForEachDayOfWeek ) {
        this.timeZone = timeZone;
        this.holidays = StreamSupport.stream( holidayDates.spliterator(), false ).collect( Collectors.toCollection( HashSet::new ) );
        // make a copy of the dictionary for internal use
        this.openHoursByDay = new EnumMap<DayOfWeek,LocalMarketHours>( marketHoursForEachDayOfWeek );

        this.sunday = setMarketHoursForDay( DayOfWeek.SUNDAY );
        this.monday = setMarketHoursForDay( DayOfWeek.MONDAY );
        this.tuesday = setMarketHoursForDay( DayOfWeek.TUESDAY );
        this.wednesday = setMarketHoursForDay( DayOfWeek.WEDNESDAY );
        this.thursday = setMarketHoursForDay( DayOfWeek.THURSDAY );
        this.friday = setMarketHoursForDay( DayOfWeek.FRIDAY );
        this.saturday = setMarketHoursForDay( DayOfWeek.SATURDAY );
    }

    /**
     * Determines if the exchange is open at the specified local date time.
     * @param localDateTime The time to check represented as a local time
     * @param extendedMarket True to use the extended market hours, false for just regular market hours
     * @returns True if the exchange is considered open at the specified time, false otherwise
     */
    public boolean isOpen( LocalDateTime localDateTime, boolean extendedMarket ) {
        final LocalDate localDate = localDateTime.toLocalDate();
        if( holidays.contains( localDate ) )
            return false;

        return getMarketHours( localDateTime.getDayOfWeek() ).isOpen( Extensions.timeOfDay( localDateTime ), extendedMarket );
    }

    /**
     * Determines if the exchange is open at any point in time over the specified interval.
     * @param startLocalDateTime The start of the interval in local time
     * @param endLocalDateTime The end of the interval in local time
     * @param extendedMarket True to use the extended market hours, false for just regular market hours
     * @returns True if the exchange is considered open at the specified time, false otherwise
     */
    public boolean isOpen( LocalDateTime startLocalDateTime, LocalDateTime endLocalDateTime, boolean extendedMarket ) {
        if( startLocalDateTime == endLocalDateTime ) {
            // if we're testing an instantaneous moment, use the other function
            return isOpen( startLocalDateTime, extendedMarket );
        }

        // we must make intra-day requests to LocalMarketHours, so check for a day gap
        LocalDateTime start = startLocalDateTime;
        LocalDateTime next = start.toLocalDate().atStartOfDay().plusDays( 1 ).minusNanos( 100 );
        LocalDateTime end = endLocalDateTime.compareTo( next ) <= 0 ? endLocalDateTime : next;
        do {
            if( !holidays.contains( start.toLocalDate() ) ) {
                // check to see if the market is open
                LocalMarketHours marketHours = getMarketHours( start.getDayOfWeek() );
                if( marketHours.isOpen( Extensions.timeOfDay( start ), Extensions.timeOfDay( end ), extendedMarket ) )
                    return true;
            }

            start = start.plusDays( 1 );
            next = end.plusDays( 1 );
            end = endLocalDateTime.compareTo( next ) <= 0 ? endLocalDateTime : next;
        }
        while( end.isAfter( start ) );

        return false;
    }

    /**
     * Determines if the exchange will be open on the date specified by the local date time
     * @param localDate The date time to check if the day is open
     * @returns True if the exchange will be open on the specified date, false otherwise
     */
    public boolean isDateOpen( LocalDate localDate ) {
        final LocalMarketHours marketHours = getMarketHours( localDate.getDayOfWeek() );
        if( marketHours.isClosedAllDay() ) {
            // if we don't have hours for this day then we're not open
            return false;
        }

        // if we don't have a holiday then we're open
        return !holidays.contains( localDate );
    }

    /**
     * Helper to access the market hours field based on the day of week
     * @param localDateTime The local date time to retrieve market hours for
     */
    public LocalMarketHours getMarketHours( LocalDateTime localDateTime) {
        return getMarketHours( localDateTime.getDayOfWeek() );
    }

    /**
     * Gets the local date time corresponding to the next market open following the specified time
     * @param localDateTime The time to begin searching for market open (non-inclusive)
     * @param extendedMarket True to include extended market hours in the search
     * @returns The next market opening date time following the specified local date time
     */
    public LocalDateTime getNextMarketOpen( LocalDateTime localDateTime, boolean extendedMarket ) {
        LocalDateTime time = localDateTime;
        final LocalDateTime oneWeekLater = localDateTime.toLocalDate().plusDays( 15 ).atStartOfDay();
        do {
            LocalMarketHours marketHours = getMarketHours( time.getDayOfWeek() );
            if( !marketHours.isClosedAllDay() && !holidays.contains( time.toLocalDate() ) ) {
                final Optional<Duration> marketOpenTimeOfDay = marketHours.getMarketOpen( Extensions.timeOfDay( time ), extendedMarket );
                if( marketOpenTimeOfDay.isPresent() ) {
                    final LocalDateTime marketOpen = time.toLocalDate().atStartOfDay().plus( marketOpenTimeOfDay.get() );
                    if( localDateTime.compareTo( marketOpen ) < 0 )
                        return marketOpen;
                }
            }

            time = time.truncatedTo( ChronoUnit.DAYS ).plusDays( 1 );
        }
        while( time.isBefore( oneWeekLater ) );

        throw new IllegalStateException( "Unable to locate next market open within two weeks." );
    }

    /**
     * Gets the local date time corresponding to the next market close following the specified time
     * @param localDateTime The time to begin searching for market close (non-inclusive)
     * @param extendedMarket True to include extended market hours in the search
     * @returns The next market closing date time following the specified local date time
     */
    public LocalDateTime getNextMarketClose( LocalDateTime localDateTime, boolean extendedMarket ) {
        LocalDateTime time = localDateTime;
        final LocalDateTime oneWeekLater = localDateTime.plusDays( 15 );
        do {
            final LocalMarketHours marketHours = getMarketHours().get( time.getDayOfWeek() );
            if( !marketHours.isClosedAllDay() && !holidays.contains( time.toLocalDate() ) ) {
                final Optional<Duration> marketCloseTimeOfDay = marketHours.getMarketClose( Extensions.timeOfDay( time ), extendedMarket);
                if( marketCloseTimeOfDay.isPresent() ) {
                    final LocalDateTime marketClose = time.truncatedTo( ChronoUnit.DAYS ).plus( marketCloseTimeOfDay.get() );
                    if( localDateTime.isBefore( marketClose ) )
                        return marketClose;
                }
            }

            time = time.truncatedTo( ChronoUnit.DAYS ).plusDays( 1 );
        }
        while( time.isBefore( oneWeekLater ) );

        throw new IllegalStateException( "Unable to locate next market close within two weeks." );
    }

    /**
     * Helper to extract market hours from the <see cref="_openHoursByDay"/> dictionary, filling
     * in Closed instantes when not present
     */
    private LocalMarketHours setMarketHoursForDay( DayOfWeek dayOfWeek ) {
        LocalMarketHours localMarketHoursForDay = openHoursByDay.get( dayOfWeek );
        if( localMarketHoursForDay == null ) {
            // assign to our dictionary that we're closed this day, as well as our local field
            openHoursByDay.put( dayOfWeek, localMarketHoursForDay = LocalMarketHours.closedAllDay( dayOfWeek ) );
        }
        
        return localMarketHoursForDay;
    }

    /**
     * Helper to access the market hours field based on the day of week
     */
    private LocalMarketHours getMarketHours( DayOfWeek day ) {
        switch( day ) {
            case SUNDAY:
                return sunday;
            case MONDAY:
                return monday;
            case TUESDAY:
                return tuesday;
            case WEDNESDAY:
                return wednesday;
            case THURSDAY:
                return thursday;
            case FRIDAY:
                return friday;
            case SATURDAY:
                return saturday;
            default:
                throw new IllegalArgumentException( day.toString() );
        }
    }
}
