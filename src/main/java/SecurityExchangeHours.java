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

//using System.Linq;
//using NodaTime;
//using QuantConnect.Util;

/// Represents the schedule of a security exchange. This includes daily regular and extended market hours
/// as well as holidays
/// 
/// This type assumes that IsOpen will be called with increasingly future times, that is, the calls should never back
/// track in time. This assumption is required to prevent time zone conversions on every call.
/// 
public class SecurityExchangeHours {
    private final ZoneId _timeZone;
    private final HashSet<long> _holidays;

    // these are listed individually for speed
    private final LocalMarketHours _sunday;
    private final LocalMarketHours _monday;
    private final LocalMarketHours _tuesday;
    private final LocalMarketHours _wednesday;
    private final LocalMarketHours _thursday;
    private final LocalMarketHours _friday;
    private final LocalMarketHours _saturday;
    private final Map<DayOfWeek, LocalMarketHours> _openHoursByDay;

    /// Gets the time zone this exchange resides in
    public ZoneId getTimeZone {
        return _timeZone;
    }

    /// Gets the holidays for the exchange
    public HashSet<DateTime> Holidays
    {
        get { return _holidays.ToHashSet(x -> new DateTime(x)); }
    }

    /**
     * Gets the market hours for this exchange
     */    
    public IReadOnlyMap<DayOfWeek, LocalMarketHours> MarketHours
    {
        get { return _openHoursByDay; }
    }

    /**
    /// Gets a <see cref="SecurityExchangeHours"/> instance that is always open
    */
    public static SecurityExchangeHours AlwaysOpen(ZoneId timeZone) {
        dayOfWeeks = Enum.GetValues(typeof (DayOfWeek)).OfType<DayOfWeek>();
        return new SecurityExchangeHours(timeZone,
            Enumerable.Empty<DateTime>(),
            dayOfWeeks.Select(LocalMarketHours.OpenAllDay).ToDictionary(x -> x.DayOfWeek)
            );
    }

    /**
    /// Initializes a new instance of the <see cref="SecurityExchangeHours"/> class
    */
     * @param timeZone">The time zone the dates and hours are represented in
     * @param holidayDates">The dates this exchange is closed for holiday
     * @param marketHoursForEachDayOfWeek">The exchange's schedule for each day of the week
    public SecurityExchangeHours(ZoneId timeZone, IEnumerable<DateTime> holidayDates, IReadOnlyMap<DayOfWeek, LocalMarketHours> marketHoursForEachDayOfWeek) {
        _timeZone = timeZone;
        _holidays = holidayDates.Select(x -> x.Date.Ticks).ToHashSet();
        // make a copy of the dictionary for internal use
        _openHoursByDay = new Map<DayOfWeek, LocalMarketHours>(marketHoursForEachDayOfWeek.ToDictionary());

        SetMarketHoursForDay(DayOfWeek.Sunday, out _sunday);
        SetMarketHoursForDay(DayOfWeek.Monday, out _monday);
        SetMarketHoursForDay(DayOfWeek.Tuesday, out _tuesday);
        SetMarketHoursForDay(DayOfWeek.Wednesday, out _wednesday);
        SetMarketHoursForDay(DayOfWeek.Thursday, out _thursday);
        SetMarketHoursForDay(DayOfWeek.Friday, out _friday);
        SetMarketHoursForDay(DayOfWeek.Saturday, out _saturday);
    }

    /**
    /// Determines if the exchange is open at the specified local date time.
    */
     * @param localDateTime">The time to check represented as a local time
     * @param extendedMarket">True to use the extended market hours, false for just regular market hours
    @returns True if the exchange is considered open at the specified time, false otherwise
    public boolean IsOpen(DateTime localDateTime, boolean extendedMarket) {
        if( _holidays.Contains(localDateTime.Date.Ticks)) {
            return false;
        }

        return GetMarketHours(localDateTime.DayOfWeek).IsOpen(localDateTime.TimeOfDay, extendedMarket);
    }

    /**
    /// Determines if the exchange is open at any point in time over the specified interval.
    */
     * @param startLocalDateTime">The start of the interval in local time
     * @param endLocalDateTime">The end of the interval in local time
     * @param extendedMarket">True to use the extended market hours, false for just regular market hours
    @returns True if the exchange is considered open at the specified time, false otherwise
    public boolean IsOpen(DateTime startLocalDateTime, DateTime endLocalDateTime, boolean extendedMarket) {
        if( startLocalDateTime == endLocalDateTime) {
            // if we're testing an instantaneous moment, use the other function
            return IsOpen(startLocalDateTime, extendedMarket);
        }

        // we must make intra-day requests to LocalMarketHours, so check for a day gap
        start = startLocalDateTime;
        end = new DateTime(Math.Min(endLocalDateTime.Ticks, start.Date.Ticks + Time.OneDay.Ticks - 1));
        do
        {
            if( !_holidays.Contains(start.Date.Ticks)) {
                // check to see if the market is open
                marketHours = GetMarketHours(start.DayOfWeek);
                if( marketHours.IsOpen(start.TimeOfDay, end.TimeOfDay, extendedMarket)) {
                    return true;
                }
            }

            start = start.Date.AddDays(1);
            end = new DateTime(Math.Min(endLocalDateTime.Ticks, end.Ticks + Time.OneDay.Ticks));
        }
        while (end > start);

        return false;
    }

    /**
    /// Determines if the exchange will be open on the date specified by the local date time
    */
     * @param localDateTime">The date time to check if the day is open
    @returns True if the exchange will be open on the specified date, false otherwise
    public boolean IsDateOpen(DateTime localDateTime) {
        marketHours = GetMarketHours(localDateTime.DayOfWeek);
        if( marketHours.IsClosedAllDay) {
            // if we don't have hours for this day then we're not open
            return false;
        }

        // if we don't have a holiday then we're open
        return !_holidays.Contains(localDateTime.Date.Ticks);
    }

    /**
    /// Helper to access the market hours field based on the day of week
    */
     * @param localDateTime">The local date time to retrieve market hours for
    public LocalMarketHours GetMarketHours(DateTime localDateTime) {
        return GetMarketHours(localDateTime.DayOfWeek);
    }

    /**
    /// Gets the local date time corresponding to the next market open following the specified time
    */
     * @param localDateTime">The time to begin searching for market open (non-inclusive)
     * @param extendedMarket">True to include extended market hours in the search
    @returns The next market opening date time following the specified local date time
    public DateTime GetNextMarketOpen(DateTime localDateTime, boolean extendedMarket) {
        time = localDateTime;
        oneWeekLater = localDateTime.Date.AddDays(15);
        do
        {
            marketHours = GetMarketHours(time.DayOfWeek);
            if( !marketHours.IsClosedAllDay && !_holidays.Contains(time.Ticks)) {
                marketOpenTimeOfDay = marketHours.GetMarketOpen(time.TimeOfDay, extendedMarket);
                if( marketOpenTimeOfDay.HasValue) {
                    marketOpen = time.Date + marketOpenTimeOfDay.Value;
                    if( localDateTime < marketOpen) {
                        return marketOpen;
                    }
                }
            }

            time = time.Date + Time.OneDay;
        }
        while (time < oneWeekLater);

        throw new Exception( "Unable to locate next market open within two weeks.");
    }

    /**
    /// Gets the local date time corresponding to the next market close following the specified time
    */
     * @param localDateTime">The time to begin searching for market close (non-inclusive)
     * @param extendedMarket">True to include extended market hours in the search
    @returns The next market closing date time following the specified local date time
    public DateTime GetNextMarketClose(DateTime localDateTime, boolean extendedMarket) {
        time = localDateTime;
        oneWeekLater = localDateTime.Date.AddDays(15);
        do
        {
            marketHours = GetMarketHours(time.DayOfWeek);
            if( !marketHours.IsClosedAllDay && !_holidays.Contains(time.Ticks)) {
                marketCloseTimeOfDay = marketHours.GetMarketClose(time.TimeOfDay, extendedMarket);
                if( marketCloseTimeOfDay.HasValue) {
                    marketClose = time.Date + marketCloseTimeOfDay.Value;
                    if( localDateTime < marketClose) {
                        return marketClose;
                    }
                }
            }

            time = time.Date + Time.OneDay;
        }
        while (time < oneWeekLater);

        throw new Exception( "Unable to locate next market close within two weeks.");
    }

    /**
    /// Helper to extract market hours from the <see cref="_openHoursByDay"/> dictionary, filling
    /// in Closed instantes when not present
    */
    private void SetMarketHoursForDay(DayOfWeek dayOfWeek, out LocalMarketHours localMarketHoursForDay) {
        if( !_openHoursByDay.TryGetValue(dayOfWeek, out localMarketHoursForDay)) {
            // assign to our dictionary that we're closed this day, as well as our local field
            _openHoursByDay[dayOfWeek] = localMarketHoursForDay = LocalMarketHours.ClosedAllDay(dayOfWeek);
        }
    }

    /**
    /// Helper to access the market hours field based on the day of week
    */
    private LocalMarketHours GetMarketHours(DayOfWeek day) {
        switch (day) {
            case DayOfWeek.Sunday:
                return _sunday;
            case DayOfWeek.Monday:
                return _monday;
            case DayOfWeek.Tuesday:
                return _tuesday;
            case DayOfWeek.Wednesday:
                return _wednesday;
            case DayOfWeek.Thursday:
                return _thursday;
            case DayOfWeek.Friday:
                return _friday;
            case DayOfWeek.Saturday:
                return _saturday;
            default:
                throw new ArgumentOutOfRangeException( "day", day, null );
        }
    }
}
