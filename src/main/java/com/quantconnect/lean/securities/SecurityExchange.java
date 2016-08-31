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
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.Collection;
import java.util.Map;

/**
 * Base exchange class providing information and helper tools for reading the current exchange situation
 */
public class SecurityExchange {
    
    private LocalDateTime localFrontier;
    private SecurityExchangeHours exchangeHours;

    /**
     * Gets the <see cref="SecurityExchangeHours"/> for this exchange
     */
    public SecurityExchangeHours getHours() {
        return exchangeHours;
    }

    /**
     * Gets the time zone for this exchange
    */
    public ZoneId getTimeZone() {
        return exchangeHours.getTimeZone();
    }

    /**
     * Number of trading days per year for this security. By default the market is open 365 days per year.
     * Used for performance statistics to calculate sharpe ratio accurately
     */
    public int getTradingDaysPerYear() {
        return 365;
    }

    /**
     * Time from the most recent data
     */
    public LocalDateTime getLocalTime() {
        return localFrontier;
    }

    /**
     * Boolean property for quickly testing if the exchange is open.
    */
    public boolean getExchangeOpen() {
        return exchangeHours.isOpen( localFrontier, false );
    }

    /**
     * Initializes a new instance of the <see cref="SecurityExchange"/> class using the specified
     * exchange hours to determine open/close times
     * @param exchangeHours Contains the weekly exchange schedule plus holidays
     */
    public SecurityExchange( SecurityExchangeHours exchangeHours ) {
        this.exchangeHours = exchangeHours;
    }

    /**
     * Set the current datetime in terms of the exchange's local time zone
     * @param newLocalTime Most recent data tick
     */
    public void setLocalDateTimeFrontier( LocalDateTime newLocalTime ) {
        localFrontier = newLocalTime;
    }

    /**
     * Check if the *date* is open.
     * This is useful for first checking the date list, and then the market hours to save CPU cycles
     * @param dateToCheck Date to check
     * @returns Return true if the exchange is open for this date
     */
    public boolean dateIsOpen( LocalDate dateToCheck ) {
        return exchangeHours.isDateOpen( dateToCheck );
    }

    /**
     * Check if this DateTime is open.
     * @param dateTime DateTime to check
     * @returns Boolean true if the market is open
     */
    public boolean dateTimeIsOpen( LocalDateTime dateTime ) {
        return exchangeHours.isOpen( dateTime, false );
    }

    /**
     * Determines if the exchange was open at any time between start and stop
    */
    public boolean isOpenDuringBar( LocalDateTime barStartTime, LocalDateTime barEndTime, boolean isExtendedMarketHours ) {
        return exchangeHours.isOpen( barStartTime, barEndTime, isExtendedMarketHours );
    }

    /**
     * Sets the regular market hours for the specified days If no days are specified then
     * all days will be updated.
     * @param marketHoursSegments Specifies each segment of the market hours, such as premarket/market/postmark
     * @param days The days of the week to set these times for
     */
    public void setMarketHours( Collection<MarketHoursSegment> marketHoursSegments, DayOfWeek... days ) {
        if( days == null || days.length == 0 ) 
            days = DayOfWeek.values();
        
        final Map<DayOfWeek,LocalMarketHours> marketHours = exchangeHours.getMarketHours();
        for( DayOfWeek day : days )
            marketHours.put( day, new LocalMarketHours( day, marketHoursSegments ) );

        // create a new exchange hours instance for the new hours
        exchangeHours = new SecurityExchangeHours( exchangeHours.getTimeZone(), exchangeHours.getHolidays(), marketHours );
    }
}
