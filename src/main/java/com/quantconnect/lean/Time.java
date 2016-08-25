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
import java.util.ArrayList;
import java.util.List;
import java.util.Spliterators;
import java.util.stream.IntStream;
import java.util.stream.StreamSupport;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.quantconnect.lean.securities.Security;

//using NodaTime;
//using QuantConnect.Logging;
//using QuantConnect.Securities;

/**
 * Time helper class collection for working with trading dates
 */
public class Time {
    
    private static Logger LOG = LoggerFactory.getLogger( Time.class );

    private Time() { }
    
    /**
     * Provides a value far enough in the future the current computer hardware will have decayed :)
     * {@value new LocalDate(2050, 12, 31)
     */
    public static final LocalDate EndOfTime = LocalDate.MAX; //( 2050, 12, 31 );

    /**
     * Provides a value far enough in the past that can be used as a lower bound on dates
     * {@value DateTime.FromOADate(0) }
     */
    public static final LocalDateTime BeginningOfTime = LocalDateTime.MIN;

    /**
     * Provides a value large enough that we won't hit the limit, while small enough
     * we can still do math against it without checking everywhere for {@link Duration.MaxValue"/>
     */
    public static final Duration MaxTimeSpan = Duration.ofDays( 1000 * 365 );

    /**
     * One Day Duration Period Constant
     */
    public static final Duration OneDay = Duration.ofDays( 1 );

    /**
     * One Hour Duration Period Constant
     */
    public static final Duration OneHour = Duration.ofHours( 1 );
    
    /**
     * One Minute Duration Period Constant
     */
    public static final Duration OneMinute = Duration.ofMinutes( 1 );

    /**
     * One Second Duration Period Constant
     */
    public static final Duration OneSecond = Duration.ofSeconds( 1 );

    /**
     * One Millisecond Duration Period Constant
     */
    public static final Duration OneMillisecond = Duration.ofMillis( 1 );
    
    private static final LocalDateTime EpochTime = LocalDateTime.from( Instant.EPOCH ); //( 1970, 1, 1, 0, 0, 0, 0 );

    /**
     * Live charting is sensitive to timezone so need to convert the local system time to a UTC and display in browser as UTC.
     */
    public class DateTimeWithZone {
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
        LocalDateTime time;
        try {
            // Unix timestamp is seconds past epoch
            time = EpochTime.plusSeconds( unixTimeStamp );
        }
        catch( Exception err ) {
            LOG .Error(err, "UnixTimeStamp: " + unixTimeStamp);
            time = DateTime.Now;
        }
        return time;
    }

    /**
     * Convert a Datetime to Unix Timestamp
     * @param time LocalDateTime object
     * @returns long unix timestamp
     */
    public static long dateTimeToUnixTimeStamp( LocalDateTime time ) {
        double timestamp = 0;
        try {
            timestamp = (time - new DateTime( 1970, 1, 1, 0, 0, 0, 0 ) ).TotalSeconds;
        } 
        catch (Exception err) {
            LOG.error( time.toString( "o" ), err );
        }
        return timestamp;
    }

    /**
     * Get the current time as a unix timestamp
     * @returns long value of the unix as UTC timestamp
     */
    public static double timeStamp() {
        return dateTimeToUnixTimeStamp( DateTime.UtcNow );
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
    public static DateTime ParseDate( String dateToParse) {
        try
        {
            //First try the exact options:
            DateTime date;
            if( DateTime.TryParseExact(dateToParse, DateFormat.SixCharacter, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
                return date;
            }
            if( DateTime.TryParseExact(dateToParse, DateFormat.EightCharacter, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
                return date;
            }
            if( DateTime.TryParseExact(dateToParse.Substring(0, 19), DateFormat.JsonFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
                return date;
            }
            if( DateTime.TryParseExact(dateToParse, DateFormat.US, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
                return date;
            }
            if( DateTime.TryParse(dateToParse, out date)) {
                return date;
            }
        }
        catch (Exception err) {
            Log.Error(err);
        }
        
        return DateTime.Now;
    }


    /**
     * Define an enumerable date range and return each date as a LocalDate object in the date range
     * @param from DateTime start date
     * @param thru DateTime end date
     * @returns Stream date range
     */
    public static Stream<LocalDate> eachDay( LocalDate from, LocalDate thru ) {
        final List<LocalDate> dates = new ArrayList<>();
        for( day = from; !day.isAfter( thru ); day = day.plusDays( 1 ) )
            dates.add( day );
        
        return dates.stream();
    }


    /**
     * Define an enumerable date range of tradeable dates - skip the holidays and weekends when securities in this algorithm don't trade.
     * @param securities Securities we have in portfolio
     * @param from Start date
     * @param thru End date
     * @returns Stream date range
    */
    public static Stream<LocalDate> eachTradeableDay( Collection<Security> securities, LocalDate from, LocalDate thru ) {
        final List<LocalDate> dates = new ArrayList<>();
        for( day = from; !day.isAfter( thru ); day = day.plusDays( 1 ) ) {
            if( tradableDate( securities, day ) )
                dates.add( day );
        }
        
        return dates.stream();
    }


    /**
     * Define an enumerable date range of tradeable dates - skip the holidays and weekends when securities in this algorithm don't trade.
     * @param security The security to get tradeable dates for
     * @param from Start date
     * @param thru End date
     * @returns Stream date range
    */
    public static Stream<LocalDate> eachTradeableDay( Security security, LocalDate from, LocalDate thru ) {
        return eachTradeableDay( security.Exchange.Hours, from, thru );
    }


    /**
     * Define an enumerable date range of tradeable dates - skip the holidays and weekends when securities in this algorithm don't trade.
     * @param exchange The security to get tradeable dates for
     * @param from Start date
     * @param thru End date
     * @returns Enumerable date range
    */
    public static IEnumerable<DateTime> EachTradeableDay(SecurityExchangeHours exchange, DateTime from, DateTime thru) {
        for (day = from.Date; day.Date <= thru.Date; day = day.AddDays(1)) {
            if( exchange.IsDateOpen(day)) {
                yield return day;
            }
        }
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
     * @param includeExtendedMarketHours True to include extended market hours trading in the search, false otherwise
     * @returns 
    */
    public static IEnumerable<DateTime> EachTradeableDayInTimeZone(SecurityExchangeHours exchange, DateTime from, DateTime thru, ZoneId timeZone, boolean includeExtendedMarketHours = true) {
        currentExchangeTime = from;
        thru = thru.Date.AddDays(1); // we want to include the full thru date
        while (currentExchangeTime < thru) {
            // take steps of max size of one day in the data time zone
            currentInTimeZone = currentExchangeTime Extensions.convertTo(  )exchange.TimeZone, timeZone);
            currentInTimeZoneEod = currentInTimeZone.Date.AddDays(1);

            // don't pass the end
            if( currentInTimeZoneEod Extensions.convertTo(  )timeZone, exchange.TimeZone) > thru) {
                currentInTimeZoneEod = thru Extensions.convertTo(  )exchange.TimeZone, timeZone);
            }

            // perform market open checks in the exchange time zone
            currentExchangeTimeEod = currentInTimeZoneEod Extensions.convertTo(  )timeZone, exchange.TimeZone);
            if( exchange.IsOpen(currentExchangeTime, currentExchangeTimeEod, includeExtendedMarketHours)) {
                yield return currentInTimeZone.Date;
            }

            currentExchangeTime = currentInTimeZoneEod Extensions.convertTo(  )timeZone, exchange.TimeZone);
        }
    } 

    /**
     * Make sure this date is not a holiday, or weekend for the securities in this algorithm.
    */
     * @param securities Security manager from the algorithm
     * @param day DateTime to check if trade-able.
    @returns True if tradeable date
    public static boolean TradableDate(IEnumerable<Security> securities, DateTime day) {
        try
        {
            foreach (security in securities) {
                if( security.Exchange.IsOpenDuringBar(day.Date, day.Date.AddDays(1), security.IsExtendedMarketHours)) return true;
            }
        }
        catch (Exception err) {
            Log.Error(err);
        }
        return false;
    }


    /**
     * Could of the number of tradeable dates within this period.
    */
     * @param securities Securities we're trading
     * @param start Start of Date Loop
     * @param finish End of Date Loop
    @returns Number of dates
    public static int TradeableDates(ICollection<Security> securities, DateTime start, DateTime finish) {
        count = 0;
        Log.Trace( "Time.TradeableDates(): Security Count: " + securities.Count);
        try 
        {
            foreach (day in EachDay(start, finish)) {
                if( TradableDate(securities, day)) {
                    count++;
                }
            }
        } 
        catch (Exception err) {
            Log.Error(err);
        }
        return count;
    }

    /**
     * Determines the start time required to produce the requested number of bars and the given size
    */
     * @param exchange The exchange used to test for market open hours
     * @param end The end time of the last bar over the requested period
     * @param barSize The length of each bar
     * @param barCount The number of bars requested
     * @param extendedMarketHours True to allow extended market hours bars, otherwise false for only normal market hours
    @returns The start time that would provide the specified number of bars ending at the specified end time, rounded down by the requested bar size
    public static DateTime GetStartTimeForTradeBars(SecurityExchangeHours exchange, DateTime end, Duration barSize, int barCount, boolean extendedMarketHours) {
        if( barSize <= Duration.ZERO) {
            throw new IllegalArgumentException( "barSize must be greater than Duration.ZERO", "barSize");
        }

        current = end.RoundDown(barSize);
        for (int i = 0; i < barCount;) {
            previous = current;
            current = current - barSize;
            if( exchange.IsOpen(current, previous, extendedMarketHours)) {
                i++;
            }
        }
        return current;
    }
}
