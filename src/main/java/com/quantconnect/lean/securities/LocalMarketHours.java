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

/**
 * Represents the market hours under normal conditions for an exchange and a specific day of the week in terms of local time
 */
public class LocalMarketHours {
    private final boolean _hasPreMarket;
    private final boolean _hasPostMarket;
    private final boolean _isOpenAllDay;
    private final boolean _isClosedAllDay;
    private final DayOfWeek _dayOfWeek;
    private final MarketHoursSegment[] _segments;

    /**
     * Gets whether or not this exchange is closed all day
     */
    public boolean isClosedAllDay() {
        return _isClosedAllDay;
    }

    /**
     * Gets whether or not this exchange is closed all day
     */
    public boolean isOpenAllDay() {
        return _isOpenAllDay;
    }

    /**
     * Gets the day of week these hours apply to
     */
    public DayOfWeek getDayOfWeek() {
        return _dayOfWeek;
    }

    /**
     * Gets the individual market hours segments that define the hours of operation for this day
     */
    public IEnumerable<MarketHoursSegment> Segments
    {
        get { return _segments; }
    }

    /**
    /// Initializes a new instance of the <see cref="LocalMarketHours"/> class
    */
     * @param day">The day of the week these hours are applicable
     * @param segments">The open/close segments defining the market hours for one day
    public LocalMarketHours(DayOfWeek day, params MarketHoursSegment[] segments)
        : this(day, (IEnumerable<MarketHoursSegment>) segments) {
    }

    /**
    /// Initializes a new instance of the <see cref="LocalMarketHours"/> class
    */
     * @param day">The day of the week these hours are applicable
     * @param segments">The open/close segments defining the market hours for one day
    public LocalMarketHours(DayOfWeek day, IEnumerable<MarketHoursSegment> segments) {
        _dayOfWeek = day;
        // filter out the closed states, we'll assume closed if no segment exists
        _segments = (segments ?? Enumerable.Empty<MarketHoursSegment>()).Where(x -> x.State != MarketHoursState.Closed).ToArray();
        _isClosedAllDay = _segments.Length == 0;
        _isOpenAllDay = _segments.Length == 1 
            && _segments[0].Start == Duration.ZERO 
            && _segments[0].End == Time.OneDay
            && _segments[0].State == MarketHoursState.Market;

        foreach (segment in _segments) {
            if( segment.State == MarketHoursState.PreMarket) {
                _hasPreMarket = true;
            }
            if( segment.State == MarketHoursState.PostMarket) {
                _hasPostMarket = true;
            }
        }
    }

    /**
    /// Initializes a new instance of the <see cref="LocalMarketHours"/> class from the specified open/close times
    */
     * @param day">The day of week these hours apply to
     * @param extendedMarketOpen">The extended market open time
     * @param marketOpen">The regular market open time, must be greater than or equal to the extended market open time
     * @param marketClose">The regular market close time, must be greater than the regular market open time
     * @param extendedMarketClose">The extended market close time, must be greater than or equal to the regular market close time
    public LocalMarketHours(DayOfWeek day, Duration extendedMarketOpen, Duration marketOpen, Duration marketClose, Duration extendedMarketClose) {
        _dayOfWeek = day;

        segments = new List<MarketHoursSegment>();

        if( extendedMarketOpen != marketOpen) {
            _hasPreMarket = true;
            segments.Add(new MarketHoursSegment(MarketHoursState.PreMarket, extendedMarketOpen, marketOpen));
        }

        if( marketOpen != Duration.ZERO || marketClose != Duration.ZERO) {
            segments.Add(new MarketHoursSegment(MarketHoursState.Market, marketOpen, marketClose));
        }

        if( marketClose != extendedMarketClose) {
            _hasPostMarket = true;
            segments.Add(new MarketHoursSegment(MarketHoursState.PostMarket, marketClose, extendedMarketClose));
        }

        _segments = segments.ToArray();
        _isClosedAllDay = _segments.Length == 0;

        // perform some sanity checks
        if( marketOpen < extendedMarketOpen) {
            throw new ArgumentException( "Extended market open time must be less than or equal to market open time.");
        }
        if( marketClose < marketOpen) {
            throw new ArgumentException( "Market close time must be after market open time.");
        }
        if( extendedMarketClose < marketClose) {
            throw new ArgumentException( "Extended market close time must be greater than or equal to market close time.");
        }
    }

    /**
    /// Initializes a new instance of the <see cref="LocalMarketHours"/> class from the specified open/close times
    /// using the market open as the extended market open and the market close as the extended market close, effectively
    /// removing any 'extended' session from these exchange hours
    */
     * @param day">The day of week these hours apply to
     * @param marketOpen">The regular market open time
     * @param marketClose">The regular market close time, must be greater than the regular market open time
    public LocalMarketHours(DayOfWeek day, Duration marketOpen, Duration marketClose)
        : this(day, marketOpen, marketOpen, marketClose, marketClose) {
    }

    /**
    /// Gets the market opening time of day
    */
     * @param time">The reference time, the open returned will be the first open after the specified time if there are multiple market open segments
     * @param extendedMarket">True to include extended market hours, false for regular market hours
    @returns The market's opening time of day
    public TimeSpan? GetMarketOpen(TimeSpan time, boolean extendedMarket) {
        for (int i = 0; i < _segments.Length; i++) {
            if( _segments[i].State == MarketHoursState.Closed || _segments[i].End <= time) {
                continue;
            }

            if( extendedMarket && _hasPreMarket) {
                if( _segments[i].State == MarketHoursState.PreMarket) {
                    return _segments[i].Start;
                }
            }
            else if( _segments[i].State == MarketHoursState.Market) {
                return _segments[i].Start;
            }
        }

        // we couldn't locate an open segment after the specified time
        return null;
    }

    /**
    /// Gets the market closing time of day
    */
     * @param time">The reference time, the close returned will be the first close after the specified time if there are multiple market open segments
     * @param extendedMarket">True to include extended market hours, false for regular market hours
    @returns The market's closing time of day
    public TimeSpan? GetMarketClose(TimeSpan time, boolean extendedMarket) {
        for (int i = 0; i < _segments.Length; i++) {
            if( _segments[i].State == MarketHoursState.Closed || _segments[i].End <= time) {
                continue;
            }

            if( extendedMarket && _hasPostMarket) {
                if( _segments[i].State == MarketHoursState.PostMarket) {
                    return _segments[i].End;
                }
            }
            else if( _segments[i].State == MarketHoursState.Market) {
                return _segments[i].End;
            }
        }
        
        // we couldn't locate an open segment after the specified time
        return null;
    }

    /**
    /// Determines if the exchange is open at the specified time
    */
     * @param time">The time of day to check
     * @param extendedMarket">True to check exended market hours, false to check regular market hours
    @returns True if the exchange is considered open, false otherwise
    public boolean IsOpen(TimeSpan time, boolean extendedMarket) {
        for (int i = 0; i < _segments.Length; i++) {
            if( _segments[i].State == MarketHoursState.Closed) {
                continue;
            }

            if( _segments[i].Contains(time)) {
                return extendedMarket || _segments[i].State == MarketHoursState.Market;
            }
        }

        // if we didn't find a segment then we're closed
        return false;
    }

    /**
    /// Determines if the exchange is open during the specified interval
    */
     * @param start">The start time of the interval
     * @param end">The end time of the interval
     * @param extendedMarket">True to check exended market hours, false to check regular market hours
    @returns True if the exchange is considered open, false otherwise
    public boolean IsOpen(TimeSpan start, Duration end, boolean extendedMarket) {
        if( start == end) {
            return IsOpen(start, extendedMarket);
        }
        
        for (int i = 0; i < _segments.Length; i++) {
            if( _segments[i].State == MarketHoursState.Closed) {
                continue;
            }

            if( extendedMarket || _segments[i].State == MarketHoursState.Market) {
                if( _segments[i].Overlaps(start, end)) {
                    return true;
                }
            }
        }

        // if we didn't find a segment then we're closed
        return false;
    }

    /**
    /// Gets a <see cref="LocalMarketHours"/> instance that is always closed
    */
     * @param dayOfWeek">The day of week
    @returns A <see cref="LocalMarketHours"/> instance that is always closed
    public static LocalMarketHours ClosedAllDay(DayOfWeek dayOfWeek) {
        return new LocalMarketHours(dayOfWeek);
    }

    /**
    /// Gets a <see cref="LocalMarketHours"/> instance that is always open
    */
     * @param dayOfWeek">The day of week
    @returns A <see cref="LocalMarketHours"/> instance that is always open
    public static LocalMarketHours OpenAllDay(DayOfWeek dayOfWeek) {
        return new LocalMarketHours(dayOfWeek, new MarketHoursSegment(MarketHoursState.Market, Duration.ZERO, Time.OneDay));
    }

    /**
    /// Returns a String that represents the current object.
    */
    @returns 
    /// A String that represents the current object.
    /// 
    /// <filterpriority>2</filterpriority>
    public @Override String toString() {
        if( IsClosedAllDay) {
            return "Closed All Day";
        }
        if( IsOpenAllDay) {
            return "Open All Day";
        }
        return DayOfWeek + ": " + String.join( " | ", (IEnumerable<MarketHoursSegment>) _segments);
    }
}