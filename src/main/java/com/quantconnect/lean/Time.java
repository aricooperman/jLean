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

package com.quantconnect.lean;

import java.time.Duration;
import java.time.Instant;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.time.format.DateTimeParseException;
import java.time.temporal.ChronoUnit;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.quantconnect.lean.securities.Security;
import com.quantconnect.lean.securities.SecurityExchangeHours;

//using NodaTime;

/**
 * Time helper class collection for working with trading dates
 */
public class Time {
    
    private static Logger LOG = LoggerFactory.getLogger( Time.class );

    private Time() { }
    
//    /**
//     * Provides a value far enough in the future the current computer hardware will have decayed :)
//     * {@value new LocalDate(2050, 12, 31)
//     */
//    public static final LocalDate EndOfTime = LocalDate.MAX; //( 2050, 12, 31 );

//    /**
//     * Provides a value far enough in the past that can be used as a lower bound on dates
//     * {@value DateTime.FromOADate(0) }
//     */
//    public static final LocalDateTime BeginningOfTime = LocalDateTime.MIN;

    /**
     * Provides a value large enough that we won't hit the limit, while small enough
     * we can still do math against it without checking everywhere for {@link Duration.MaxValue"/>
     */
    public static final Duration MAX_TIME_SPAN = Duration.ofDays( 1000 * 365 );

    /**
     * One Day Duration Period Constant
     */
    public static final Duration ONE_DAY = Duration.ofDays( 1 );

    /**
     * One Hour Duration Period Constant
     */
    public static final Duration ONE_HOUR = Duration.ofHours( 1 );
    
    /**
     * One Minute Duration Period Constant
     */
    public static final Duration ONE_MINUTE = Duration.ofMinutes( 1 );

    /**
     * One Second Duration Period Constant
     */
    public static final Duration ONE_SECOND = Duration.ofSeconds( 1 );

    /**
     * One Millisecond Duration Period Constant
     */
    public static final Duration ONE_MILLISECOND = Duration.ofMillis( 1 );
    
    private static final LocalDateTime EPOCH_TIME = LocalDateTime.from( Instant.EPOCH ); //( 1970, 1, 1, 0, 0, 0, 0 );

    /**
     * Live charting is sensitive to timezone so need to convert the local system time to a UTC and display in browser as UTC.
     */
    public static class DateTimeWithZone {
        private final LocalDateTime utcDateTime;
        private final ZoneId timeZone;

        /**
         * Initializes a new instance of the DateTimeWithZone class.
         * @param dateTime Date time.
         * @param timeZone Zone Id.
         */
        public DateTimeWithZone( LocalDateTime dateTime, ZoneId timeZone ) {
            this.utcDateTime = Extensions.convertToUtc( dateTime, timeZone );
            this.timeZone = timeZone;
        }

        /**
         * Gets the universal time.
         * @return The universal time.
         */
        public LocalDateTime getUniversalTime() { 
            return utcDateTime;
        }

        /**
         * Gets the time zone.
         * @return The time zone.
         */
        public ZoneId getTimeZone() { 
            return timeZone;
        }

        /**
         * Gets the local time.
         * @return The local time
         */
        public LocalDateTime getLocalTime() {
            return Extensions.convertFromUtc( utcDateTime, timeZone );
        }
    }

    /**
     * Create a C# DateTime from a UnixTimestamp
     * @param unixTimeStamp Double unix timestamp (Time since Midnight Jan 1 1970)
     * @return C# date timeobject
     */
    public static LocalDateTime unixTimeStampToDateTime( long unixTimeStamp ) {
        try {
            // Unix timestamp is seconds past epoch
            return EPOCH_TIME.plus( unixTimeStamp, ChronoUnit.MILLIS );
        }
        catch( Exception err ) {
            LOG.error( "UnixTimeStamp: " + unixTimeStamp, err );
            return LocalDateTime.now();
        }
    }

    /**
     * Convert a LocalDateTime to Unix Timestamp
     * @param time LocalDateTime object
     * @returns long unix timestamp
     */
    public static long dateTimeToUnixTimeStamp( LocalDateTime time ) {
        try {
            return time.atZone( ZoneId.systemDefault() ).toInstant().toEpochMilli();
        } 
        catch (Exception err) {
            LOG.error( time.toString(), err );
            return 0L;
        }
    }

    /**
     * Get the current time as a unix timestamp
     * @returns long value of the unix as UTC timestamp
     */
    public static long timeStamp() {
        return dateTimeToUnixTimeStamp( LocalDateTime.now() );
    }
    
    /**
     * Returns the timespan with the larger value
     */
    public static Duration max( Duration one, Duration two ) {
        return one.compareTo( two ) >= 0 ? one : two;
    }
    
    /**
     * Returns the Duration with the smaller value
     */
    public static Duration min( Duration one, Duration two ) {
        return one.compareTo( two ) <= 0 ? one : two;
    }

    /**
     * Parse a standard YY MM DD date into a DateTime. Attempt common date formats 
     * @param dateToParse String date time to parse
     * @returns Date time
     */
    public static LocalDate parseDate( String dateToParse) {
        try { 
            //First try the exact options:
            return LocalDate.parse( dateToParse, DateFormat.SixCharacter );
        } catch( DateTimeParseException e ) { }
        
        try { return LocalDate.parse( dateToParse, DateFormat.EightCharacter ); } catch( DateTimeParseException e ) { }
        
        try { return LocalDate.parse( dateToParse.substring( 0, 19 ), DateFormat.JsonFormat ); } catch( DateTimeParseException e ) { }
        
        try { return LocalDate.parse( dateToParse, DateFormat.US ); } catch( DateTimeParseException e ) { }
            
        try { return LocalDate.parse( dateToParse ); } catch( DateTimeParseException e ) { }
        
        return LocalDate.now();
    }


    /**
     * Define an enumerable date range and return each date as a LocalDate object in the date range
     * @param from DateTime start date
     * @param thru DateTime end date
     * @returns Iterable of date range
     */
    public static Iterable<LocalDate> eachDay( LocalDate from, LocalDate thru ) {
        final List<LocalDate> dates = new ArrayList<>();
        for( LocalDate day = from; !day.isAfter( thru ); day = day.plusDays( 1 ) )
            dates.add( day );
        
        return dates;
    }


    /**
     * Define an enumerable date range of tradeable dates - skip the holidays and weekends when securities in this algorithm don't trade.
     * @param securities Securities we have in portfolio
     * @param from Start date
     * @param thru End date
     * @returns Iterable of date range
     */
    public static Iterable<LocalDate> eachTradeableDay( Collection<Security> securities, LocalDate from, LocalDate thru ) {
        final List<LocalDate> dates = new ArrayList<>();
        for( LocalDate day = from; !day.isAfter( thru ); day = day.plusDays( 1 ) ) {
            if( tradableDate( securities, day ) )
                dates.add( day );
        }
        
        return dates;
    }


    /**
     * Define an enumerable date range of tradeable dates - skip the holidays and weekends when securities in this algorithm don't trade.
     * @param security The security to get tradeable dates for
     * @param from Start date
     * @param thru End date
     * @returns Iterable of date range
     */
    public static Iterable<LocalDate> eachTradeableDay( Security security, LocalDate from, LocalDate thru ) {
        return eachTradeableDay( security.getExchange().getHours(), from, thru );
    }


    /**
     * Define an enumerable date range of tradeable dates - skip the holidays and weekends when securities in this algorithm don't trade.
     * @param exchange The security to get tradeable dates for
     * @param from Start date
     * @param thru End date
     * @returns Enumerable date range
    */
    public static Iterable<LocalDate> eachTradeableDay( SecurityExchangeHours exchange, LocalDate from, LocalDate thru ) {
        final List<LocalDate> dates = new ArrayList<>();
        for( LocalDate day = from; !day.isAfter( thru ); day = day.plusDays( 1 ) ) {
            if( exchange.isDateOpen( day ) )
                dates.add( day );
        }
        
        return dates;
    }

    /**
     * Define an enumerable date range of tradeable dates but expressed in a different time zone.
     * 
     * This is mainly used to bridge the gap between exchange time zone and data time zone for file written to disk. The returned
     * enumerable of dates is gauranteed to be the same size or longer than those generated via <see cref="EachTradeableDay(ICollection{Security},DateTime,DateTime)"/>
     * 
     * @param exchange The exchange hours
     * @param from The start time in the exchange time zone
     * @param thru The end time in the exchange time zone (inclusive of the final day)
     * @param timeZone The timezone to project the dates into (inclusive of the final day)
     * @returns 
     */
    public static Iterable<LocalDateTime> eachTradeableDayInTimeZone( SecurityExchangeHours exchange, LocalDateTime from, LocalDateTime thru, ZoneId timeZone ) {
        return eachTradeableDayInTimeZone( exchange, from, thru, timeZone, true );
    }
    
    /**
     * Define an stream date range of tradeable dates but expressed in a different time zone.
     * 
     * This is mainly used to bridge the gap between exchange time zone and data time zone for file written to disk. The returned
     * enumerable of dates is guaranteed to be the same size or longer than those generated via <see cref="EachTradeableDay(ICollection{Security},DateTime,DateTime)"/>
     * 
     * @param exchange The exchange hours
     * @param from The start time in the exchange time zone
     * @param thru The end time in the exchange time zone (inclusive of the final day)
     * @param timeZone The timezone to project the dates into (inclusive of the final day)
     * @param includeExtendedMarketHours True to include extended market hours trading in the search, false otherwise
     * @returns 
     */
    public static Iterable<LocalDateTime> eachTradeableDayInTimeZone( SecurityExchangeHours exchange, LocalDateTime from, LocalDateTime thru, ZoneId timeZone, 
            boolean includeExtendedMarketHours ) {
        LocalDateTime currentExchangeTime = from;
        thru = thru.plusDays( 1 ); // we want to include the full thru date
        final List<LocalDateTime> dates = new ArrayList<>();
        
        while( currentExchangeTime.isBefore( thru ) ) {
            // take steps of max size of one day in the data time zone
            final LocalDateTime currentInTimeZone = Extensions.convertTo( currentExchangeTime, exchange.getTimeZone(), timeZone );
            LocalDateTime currentInTimeZoneEod = currentInTimeZone.toLocalDate().plusDays( 1 ).atStartOfDay();

            // don't pass the end
            if( Extensions.convertTo( currentInTimeZoneEod, timeZone, exchange.getTimeZone() ).isAfter( thru ) )
                currentInTimeZoneEod = Extensions.convertTo( thru, exchange.getTimeZone(), timeZone );

            // perform market open checks in the exchange time zone
            final LocalDateTime currentExchangeTimeEod = Extensions.convertTo( currentInTimeZoneEod, timeZone, exchange.getTimeZone() );
            if( exchange.isOpen( currentExchangeTime, currentExchangeTimeEod, includeExtendedMarketHours ) )
                dates.add( currentInTimeZone );

            currentExchangeTime = Extensions.convertTo( currentInTimeZoneEod, timeZone, exchange.getTimeZone() );
        }
        
        return dates;
    } 

    /**
     * Make sure this date is not a holiday, or weekend for the securities in this algorithm.
     * @param securities Security manager from the algorithm
     * @param day DateTime to check if trade-able.
     * @returns True if tradeable date
     */
    public static boolean tradableDate( Iterable<Security> securities, LocalDate day ) {
        try {
            for( Security security : securities ) {
                if( security.getExchange().isOpenDuringBar( day.atStartOfDay(), day.plusDays( 1 ).atStartOfDay(), security.isExtendedMarketHours() ) ) 
                    return true;
            }
        }
        catch( Exception err ) {
            LOG.error( err.getMessage(), err );
        }
        
        return false;
    }


    /**
     * Could of the number of tradeable dates within this period.
     * @param securities Securities we're trading
     * @param start Start of Date Loop
     * @param finish End of Date Loop
     * @returns Number of dates
     */
    public static int tradeableDates( Collection<Security> securities, LocalDate start, LocalDate finish ) {
        int count = 0;
        LOG.trace( "Time.TradeableDates(): Security Count: {}", securities.size() );
        try {
            for( LocalDate day : eachDay( start, finish ) ) {
                if( tradableDate( securities, day ) )
                    count++;
            }
        } 
        catch( Exception err ) {
            LOG.error( err.getMessage(), err );
        }
        
        return count;
    }

    /**
     * Determines the start time required to produce the requested number of bars and the given size
     * @param exchange The exchange used to test for market open hours
     * @param end The end time of the last bar over the requested period
     * @param barSize The length of each bar
     * @param barCount The number of bars requested
     * @param extendedMarketHours True to allow extended market hours bars, otherwise false for only normal market hours
     * @returns The start time that would provide the specified number of bars ending at the specified end time, rounded down by the requested bar size
     */
    public static LocalDateTime getStartTimeForTradeBars( SecurityExchangeHours exchange, LocalDateTime end, Duration barSize, int barCount, boolean extendedMarketHours ) {
        if( barSize.compareTo( Duration.ZERO ) <= 0 )
            throw new IllegalArgumentException( "barSize must be greater than Duration.ZERO" );

        LocalDateTime current = Extensions.roundDown( end, barSize);
        for (int i = 0; i < barCount;) {
            LocalDateTime previous = current;
            current = current.minus( barSize );
            if( exchange.isOpen( current, previous, extendedMarketHours ) )
                i++;
        }
        
        return current;
    }
}
