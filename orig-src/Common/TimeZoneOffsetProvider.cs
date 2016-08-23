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
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using NodaTime.TimeZones;

package com.quantconnect.lean
{
    /**
    /// Represents the discontinuties in a single time zone and provides offsets to UTC.
    /// This type assumes that times will be asked in a forward marching manner.
    /// This type is not thread safe.
    */
    public class TimeZoneOffsetProvider
    {
        private static final long DateTimeMaxValueTicks = DateTime.MaxValue.Ticks;

        private long _nextDiscontinuity;
        private long _currentOffsetTicks;
        private final ZoneId _timeZone;
        private final Queue<long> _discontinuities;

        /**
        /// Gets the time zone this instances provides offsets for
        */
        public ZoneId TimeZone
        {
            get { return _timeZone; }
        }

        /**
        /// Initializes a new instance of the <see cref="TimeZoneOffsetProvider"/> class
        */
         * @param timeZone">The time zone to provide offsets for
         * @param utcStartTime">The start of the range of offsets
         * @param utcEndTime">The en of the range of offsets
        public TimeZoneOffsetProvider(ZoneId timeZone, DateTime utcStartTime, DateTime utcEndTime) {
            _timeZone = timeZone;

            // pad the end so we get the correct zone interval
            utcEndTime += Duration.ofDays(2*365);

            start = ZoneId.Utc.AtLeniently(LocalDateTime.FromDateTime(utcStartTime));
            end = ZoneId.Utc.AtLeniently(LocalDateTime.FromDateTime(utcEndTime));
            zoneIntervals = _timeZone.GetZoneIntervals(start.ToInstant(), end.ToInstant()).ToList();
            
            // short circuit time zones with no discontinuities
            if( zoneIntervals.Count == 1 && zoneIntervals[0].Start == Instant.MinValue && zoneIntervals[0].End == Instant.MaxValue) {
                // end of discontinuities
                _discontinuities = new Queue<long>();
                _nextDiscontinuity = DateTime.MaxValue.Ticks;
                _currentOffsetTicks = _timeZone.GetUtcOffset(Instant.FromDateTimeUtc(DateTime.UtcNow)).Ticks;
            }
            else
            {
                // get the offset just before the next discontinuity to initialize
                _discontinuities = new Queue<long>(zoneIntervals.Select(GetDateTimeUtcTicks));
                _nextDiscontinuity = _discontinuities.Dequeue();
                _currentOffsetTicks = _timeZone.GetUtcOffset(Instant.FromDateTimeUtc(new DateTime(_nextDiscontinuity - 1, DateTimeKind.Utc))).Ticks;
            }
        }

        /**
        /// Gets the offset in ticks from this time zone to UTC, such that UTC time + offset = local time
        */
         * @param utcTime">The time in UTC to get an offset to local
        @returns The offset in ticks between UTC and the local time zone
        public long GetOffsetTicks(DateTime utcTime) {
            // keep advancing our discontinuity until the requested time, don't recompute if already at max value
            while (utcTime.Ticks >= _nextDiscontinuity && _nextDiscontinuity != DateTimeMaxValueTicks) {
                // grab the next discontinuity
                _nextDiscontinuity = _discontinuities.Count == 0 
                    ? DateTime.MaxValue.Ticks
                    : _discontinuities.Dequeue();

                // get the offset just before the next discontinuity
                offset = _timeZone.GetUtcOffset(Instant.FromDateTimeUtc(new DateTime(_nextDiscontinuity - 1, DateTimeKind.Utc)));
                _currentOffsetTicks = offset.Ticks;
            }

            return _currentOffsetTicks;
        }

        /**
        /// Gets this offset provider's next discontinuity
        */
        @returns The next discontinuity in UTC ticks
        public long GetNextDiscontinuity() {
            return _nextDiscontinuity;
        }

        /**
        /// Converts the specified <paramref name="utcTime"/> using the offset resolved from
        /// a call to <see cref="GetOffsetTicks"/>
        */
         * @param utcTime">The time to convert from utc
        @returns The same instant in time represented in the <see cref="TimeZone"/>
        public DateTime ConvertFromUtc(DateTime utcTime) {
            return new DateTime(utcTime.Ticks + GetOffsetTicks(utcTime));
        }

        /**
        /// Gets the zone interval's start time in DateTimeKind.Utc ticks
        */
        private static long GetDateTimeUtcTicks(ZoneInterval zoneInterval) {
            // can't convert these values directly to date times, so just shortcut these here
            // we set the min value to one since the logic in the ctor will decrement this value to
            // determine the last instant BEFORE the discontinuity
            if( zoneInterval.Start == Instant.MinValue) return 1;
            if( zoneInterval.Start == Instant.MaxValue) return DateTime.MaxValue.Ticks;

            return zoneInterval.Start.ToDateTimeUtc().Ticks;
        }
    }
}
