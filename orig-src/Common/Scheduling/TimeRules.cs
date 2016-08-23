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
 *
*/

using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using QuantConnect.Securities;

package com.quantconnect.lean.Scheduling
{
    /**
    /// Helper class used to provide better syntax when defining time rules
    */
    public class TimeRules
    {
        private ZoneId _timeZone;

        private final SecurityManager _securities;

        /**
        /// Initializes a new instance of the <see cref="TimeRules"/> helper class
        */
         * @param securities">The security manager
         * @param timeZone">The algorithm's default time zone
        public TimeRules(SecurityManager securities, ZoneId timeZone) {
            _securities = securities;
            _timeZone = timeZone;
        }

        /**
        /// Sets the default time zone
        */
         * @param timeZone">The time zone to use for helper methods that can't resolve a time zone
        public void SetDefaultTimeZone(ZoneId timeZone) {
            _timeZone = timeZone;
        }

        /**
        /// Specifies an event should fire at the specified time of day in the algorithm's time zone
        */
         * @param timeOfDay">The time of day in the algorithm's time zone the event should fire
        @returns A time rule that fires at the specified time in the algorithm's time zone
        public ITimeRule At(TimeSpan timeOfDay) {
            return At(timeOfDay, _timeZone);
        }

        /**
        /// Specifies an event should fire at the specified time of day in the algorithm's time zone
        */
         * @param hour">The hour
         * @param minute">The minute
         * @param second">The second
        @returns A time rule that fires at the specified time in the algorithm's time zone
        public ITimeRule At(int hour, int minute, int second = 0) {
            return At(new TimeSpan(hour, minute, second), _timeZone);
        }

        /**
        /// Specifies an event should fire at the specified time of day in the specified time zone
        */
         * @param hour">The hour
         * @param minute">The minute
         * @param timeZone">The time zone the event time is represented in
        @returns A time rule that fires at the specified time in the algorithm's time zone
        public ITimeRule At(int hour, int minute, ZoneId timeZone) {
            return At(new TimeSpan(hour, minute, 0), timeZone);
        }

        /**
        /// Specifies an event should fire at the specified time of day in the specified time zone
        */
         * @param hour">The hour
         * @param minute">The minute
         * @param second">The second
         * @param timeZone">The time zone the event time is represented in
        @returns A time rule that fires at the specified time in the algorithm's time zone
        public ITimeRule At(int hour, int minute, int second, ZoneId timeZone) {
            return At(new TimeSpan(hour, minute, second), timeZone);
        }

        /**
        /// Specifies an event should fire at the specified time of day in the specified time zone
        */
         * @param timeOfDay">The time of day in the algorithm's time zone the event should fire
         * @param timeZone">The time zone the date time is expressed in
        @returns A time rule that fires at the specified time in the algorithm's time zone
        public ITimeRule At(TimeSpan timeOfDay, ZoneId timeZone) {
            name = String.join( ",", timeOfDay.TotalHours.toString( "0.##"));
            Func<IEnumerable<DateTime>, IEnumerable<DateTime>> applicator = dates =>
                from date in dates
                let localEventTime = date + timeOfDay
                let utcEventTime = localEventTime.ConvertToUtc(timeZone)
                select utcEventTime;

            return new FuncTimeRule(name, applicator);
        }

        /**
        /// Specifies an event should fire periodically on the requested interval
        */
         * @param interval">The frequency with which the event should fire
        @returns A time rule that fires after each interval passes
        public ITimeRule Every(TimeSpan interval) {
            name = "Every " + interval.TotalMinutes.toString( "0.##") + " min";
            Func<IEnumerable<DateTime>, IEnumerable<DateTime>> applicator = dates -> EveryIntervalIterator(dates, interval);
            return new FuncTimeRule(name, applicator);
        }

        /**
        /// Specifies an event should fire at market open +- <paramref name="minutesAfterOpen"/>
        */
         * @param symbol">The symbol whose market open we want an event for
         * @param minutesAfterOpen">The time after market open that the event should fire
         * @param extendedMarketOpen">True to use extended market open, false to use regular market open
        @returns A time rule that fires the specified number of minutes after the symbol's market open
        public ITimeRule AfterMarketOpen(Symbol symbol, double minutesAfterOpen = 0, boolean extendedMarketOpen = false) {
            security = GetSecurity(symbol);

            type = extendedMarketOpen ? "ExtendedMarketOpen" : "MarketOpen";
            name = String.format( "%1$s: %2$s min after %3$s", symbol, minutesAfterOpen.toString( "0.##"), type);

            timeAfterOpen = Duration.ofMinutes(minutesAfterOpen);
            Func<IEnumerable<DateTime>, IEnumerable<DateTime>> applicator = dates =>
                from date in dates
                where security.Exchange.DateIsOpen(date)
                let marketOpen = security.Exchange.Hours.GetNextMarketOpen(date, extendedMarketOpen)
                let localEventTime = marketOpen + timeAfterOpen
                let utcEventTime = localEventTime.ConvertToUtc(security.Exchange.TimeZone)
                select utcEventTime;

            return new FuncTimeRule(name, applicator);
        }

        /**
        /// Specifies an event should fire at the market close +- <paramref name="minutesBeforeClose"/>
        */
         * @param symbol">The symbol whose market close we want an event for
         * @param minutesBeforeClose">The time before market close that the event should fire
         * @param extendedMarketClose">True to use extended market close, false to use regular market close
        @returns A time rule that fires the specified number of minutes before the symbol's market close
        public ITimeRule BeforeMarketClose(Symbol symbol, double minutesBeforeClose = 0, boolean extendedMarketClose = false) {
            security = GetSecurity(symbol);

            type = extendedMarketClose ? "ExtendedMarketClose" : "MarketClose";
            name = String.format( "%1$s: %2$s min before %3$s", security.Symbol, minutesBeforeClose.toString( "0.##"), type);

            timeBeforeClose = Duration.ofMinutes(minutesBeforeClose);
            Func<IEnumerable<DateTime>, IEnumerable<DateTime>> applicator = dates =>
                from date in dates
                where security.Exchange.DateIsOpen(date)
                let marketClose = security.Exchange.Hours.GetNextMarketClose(date, extendedMarketClose)
                let localEventTime = marketClose - timeBeforeClose
                let utcEventTime = localEventTime.ConvertToUtc(security.Exchange.TimeZone)
                select utcEventTime;

            return new FuncTimeRule(name, applicator);
        }

        private Security GetSecurity(Symbol symbol) {
            Security security;
            if( !_securities.TryGetValue(symbol, out security)) {
                throw new Exception(symbol.toString() + " not found in portfolio. Request this data when initializing the algorithm.");
            }
            return security;
        }

        private static IEnumerable<DateTime> EveryIntervalIterator(IEnumerable<DateTime> dates, Duration interval) {
            foreach (date in dates) {
                for (time = Duration.ZERO; time < Time.OneDay; time += interval) {
                    yield return date + time;
                }
            }
        }
    }
}