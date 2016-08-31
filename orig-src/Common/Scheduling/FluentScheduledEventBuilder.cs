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
using System.Linq;
using NodaTime;
using QuantConnect.Securities;

package com.quantconnect.lean.Scheduling
{
    /**
     * Provides a builder class to allow for fluent syntax when constructing new events
    */
     * 
     * This builder follows the following steps for event creation:
     * 
     * 1. Specify an event name (optional)
     * 2. Specify an IDateRule
     * 3. Specify an ITimeRule
     *     a. repeat 3. to define extra time rules (optional)
     * 4. Specify additional where clause (optional)
     * 5. Register event via call to Run
     * 
    public class FluentScheduledEventBuilder : IFluentSchedulingDateSpecifier, IFluentSchedulingRunnable
    {
        private IDateRule _dateRule;
        private ITimeRule _timeRule;
        private Func<DateTime, bool> _predicate;

        private final String _name;
        private final ScheduleManager _schedule;
        private final SecurityManager _securities;

        /**
         * Initializes a new instance of the <see cref="FluentScheduledEventBuilder"/> class
        */
         * @param schedule The schedule to send created events to
         * @param securities The algorithm's security manager
         * @param name A specific name for this event
        public FluentScheduledEventBuilder(ScheduleManager schedule, SecurityManager securities, String name = null ) {
            _name = name;
            _schedule = schedule;
            _securities = securities;
        }

        private FluentScheduledEventBuilder SetTimeRule(ITimeRule rule) {
            // if it's not set, just set it
            if( _timeRule == null ) {
                _timeRule = rule;
                return this;
            }

            // if it's already a composite, open it up and make a new composite
            // prevent nesting composites
            compositeTimeRule = _timeRule as CompositeTimeRule;
            if( compositeTimeRule != null ) {
                rules = compositeTimeRule.Rules;
                _timeRule = new CompositeTimeRule(rules.Concat(new[] { rule }));
                return this;
            }

            // create a composite from the existing rule and the new rules
            _timeRule = new CompositeTimeRule(_timeRule, rule);
            return this;
        }

        #region DateRules and TimeRules delegation

        /**
         * Creates events on each of the specified day of week
        */
        IFluentSchedulingTimeSpecifier IFluentSchedulingDateSpecifier.Every(params DayOfWeek[] days) {
            _dateRule = _schedule.DateRules.Every(days);
            return this;
        }

        /**
         * Creates events on every day of the year
        */
        IFluentSchedulingTimeSpecifier IFluentSchedulingDateSpecifier.EveryDay() {
            _dateRule = _schedule.DateRules.EveryDay();
            return this;
        }

        /**
         * Creates events on every trading day of the year for the symbol
        */
        IFluentSchedulingTimeSpecifier IFluentSchedulingDateSpecifier.EveryDay(Symbol symbol) {
            _dateRule = _schedule.DateRules.EveryDay(symbol);
            return this;
        }

        /**
         * Creates events on the first day of the month
        */
        IFluentSchedulingTimeSpecifier IFluentSchedulingDateSpecifier.MonthStart() {
            _dateRule = _schedule.DateRules.MonthStart();
            return this;
        }

        /**
         * Creates events on the first trading day of the month
        */
        IFluentSchedulingTimeSpecifier IFluentSchedulingDateSpecifier.MonthStart(Symbol symbol) {
            _dateRule = _schedule.DateRules.MonthStart(symbol);
            return this;
        }

        /**
         * Filters the event times using the predicate
        */
        IFluentSchedulingTimeSpecifier IFluentSchedulingDateSpecifier.Where(Func<DateTime, bool> predicate) {
            _predicate = _predicate == null
                ? predicate
                : (time -> _predicate(time) && predicate(time));
            return this;
        }

        /**
         * Creates events that fire at the specific time of day in the algorithm's time zone
        */
        IFluentSchedulingRunnable IFluentSchedulingTimeSpecifier.At(TimeSpan timeOfDay) {
            return SetTimeRule(_schedule.TimeRules.At(timeOfDay));
        }

        /**
         * Creates events that fire a specified number of minutes after market open
        */
        IFluentSchedulingRunnable IFluentSchedulingTimeSpecifier.AfterMarketOpen(Symbol symbol, double minutesAfterOpen, boolean extendedMarketOpen) {
            return SetTimeRule(_schedule.TimeRules.AfterMarketOpen(symbol, minutesAfterOpen, extendedMarketOpen));
        }

        /**
         * Creates events that fire a specified numer of minutes before market close
        */
        IFluentSchedulingRunnable IFluentSchedulingTimeSpecifier.BeforeMarketClose(Symbol symbol, double minuteBeforeClose, boolean extendedMarketClose) {
            return SetTimeRule(_schedule.TimeRules.BeforeMarketClose(symbol, minuteBeforeClose, extendedMarketClose));
        }

        /**
         * Creates events that fire on a period define by the specified interval
        */
        IFluentSchedulingRunnable IFluentSchedulingTimeSpecifier.Every(TimeSpan interval) {
            return SetTimeRule(_schedule.TimeRules.Every(interval));
        }

        /**
         * Filters the event times using the predicate
        */
        IFluentSchedulingTimeSpecifier IFluentSchedulingTimeSpecifier.Where(Func<DateTime, bool> predicate) {
            _predicate = _predicate == null
                ? predicate
                : (time -> _predicate(time) && predicate(time));
            return this;
        }

        /**
         * Register the defined event with the callback
        */
        ScheduledEvent IFluentSchedulingRunnable.Run(Action callback) {
            return ((IFluentSchedulingRunnable)this).Run((name, time) -> callback());
        }

        /**
         * Register the defined event with the callback
        */
        ScheduledEvent IFluentSchedulingRunnable.Run(Action<DateTime> callback) {
            return ((IFluentSchedulingRunnable)this).Run((name, time) -> callback(time));
        }

        /**
         * Register the defined event with the callback
        */
        ScheduledEvent IFluentSchedulingRunnable.Run(Action<String, DateTime> callback) {
            name = _name ?? _dateRule.Name + ": " + _timeRule.Name;
            // back the date up to ensure we get all events, the event scheduler will skip past events that whose time has passed
            dates = _dateRule.GetDates(_securities.UtcTime.Date.AddDays(-1), Time.EndOfTime);
            eventTimes = _timeRule.CreateUtcEventTimes(dates);
            if( _predicate != null ) {
                eventTimes = eventTimes.Where(_predicate);
            }
            scheduledEvent = new ScheduledEvent(name, eventTimes, callback);
            _schedule.Add(scheduledEvent);
            return scheduledEvent;
        }

        /**
         * Filters the event times using the predicate
        */
        IFluentSchedulingRunnable IFluentSchedulingRunnable.Where(Func<DateTime, bool> predicate) {
            _predicate = _predicate == null
                ? predicate
                : (time -> _predicate(time) && predicate(time));
            return this;
        }

        /**
         * Filters the event times to only include times where the symbol's market is considered open
        */
        IFluentSchedulingRunnable IFluentSchedulingRunnable.DuringMarketHours(Symbol symbol, boolean extendedMarket) {
            security = GetSecurity(symbol);
            Func<DateTime, bool> predicate = time =>
            {
                localTime = time Extensions.convertFromUtc(security.Exchange.TimeZone);
                return security.Exchange.IsOpenDuringBar(localTime, localTime, extendedMarket);
            };
            _predicate = _predicate == null
                ? predicate
                : (time -> _predicate(time) && predicate(time));
            return this;
        }

        IFluentSchedulingTimeSpecifier IFluentSchedulingDateSpecifier.On(int year, int month, int day) {
            _dateRule = _schedule.DateRules.On(year, month, day);
            return this;
        }

        IFluentSchedulingTimeSpecifier IFluentSchedulingDateSpecifier.On(params DateTime[] dates) {
            _dateRule = _schedule.DateRules.On(dates);
            return this;
        }

        IFluentSchedulingRunnable IFluentSchedulingTimeSpecifier.At(int hour, int minute, int second) {
            return SetTimeRule(_schedule.TimeRules.At(hour, minute, second));
        }

        IFluentSchedulingRunnable IFluentSchedulingTimeSpecifier.At(int hour, int minute, ZoneId timeZone) {
            return SetTimeRule(_schedule.TimeRules.At(hour, minute, 0, timeZone));
        }

        IFluentSchedulingRunnable IFluentSchedulingTimeSpecifier.At(int hour, int minute, int second, ZoneId timeZone) {
            return SetTimeRule(_schedule.TimeRules.At(hour, minute, second, timeZone));
        }

        IFluentSchedulingRunnable IFluentSchedulingTimeSpecifier.At(TimeSpan timeOfDay, ZoneId timeZone) {
            return SetTimeRule(_schedule.TimeRules.At(timeOfDay, timeZone));
        }

        private Security GetSecurity(Symbol symbol) {
            Security security;
            if( !_securities.TryGetValue(symbol, out security)) {
                throw new Exception(symbol.toString() + " not found in portfolio. Request this data when initializing the algorithm.");
            }
            return security;
        }

        #endregion
    }

    /**
     * Specifies the date rule component of a scheduled event
    */
    public interface IFluentSchedulingDateSpecifier
    {
        /**
         * Filters the event times using the predicate
        */
        IFluentSchedulingTimeSpecifier Where(Func<DateTime, bool> predicate);
        /**
         * Creates events only on the specified date
        */
        IFluentSchedulingTimeSpecifier On(int year, int month, int day);
        /**
         * Creates events only on the specified dates
        */
        IFluentSchedulingTimeSpecifier On(params DateTime[] dates);
        /**
         * Creates events on each of the specified day of week
        */
        IFluentSchedulingTimeSpecifier Every(params DayOfWeek[] days);
        /**
         * Creates events on every day of the year
        */
        IFluentSchedulingTimeSpecifier EveryDay();
        /**
         * Creates events on every trading day of the year for the symbol
        */
        IFluentSchedulingTimeSpecifier EveryDay(Symbol symbol);
        /**
         * Creates events on the first day of the month
        */
        IFluentSchedulingTimeSpecifier MonthStart();
        /**
         * Creates events on the first trading day of the month
        */
        IFluentSchedulingTimeSpecifier MonthStart(Symbol symbol);
    }

    /**
     * Specifies the time rule component of a scheduled event
    */
    public interface IFluentSchedulingTimeSpecifier
    {
        /**
         * Filters the event times using the predicate
        */
        IFluentSchedulingTimeSpecifier Where(Func<DateTime, bool> predicate);
        /**
         * Creates events that fire at the specified time of day in the specified time zone
        */
        IFluentSchedulingRunnable At(int hour, int minute, int second = 0);
        /**
         * Creates events that fire at the specified time of day in the specified time zone
        */
        IFluentSchedulingRunnable At(int hour, int minute, ZoneId timeZone);
        /**
         * Creates events that fire at the specified time of day in the specified time zone
        */
        IFluentSchedulingRunnable At(int hour, int minute, int second, ZoneId timeZone);
        /**
         * Creates events that fire at the specified time of day in the specified time zone
        */
        IFluentSchedulingRunnable At(TimeSpan timeOfDay, ZoneId timeZone);
        /**
         * Creates events that fire at the specific time of day in the algorithm's time zone
        */
        IFluentSchedulingRunnable At(TimeSpan timeOfDay);
        /**
         * Creates events that fire on a period define by the specified interval
        */
        IFluentSchedulingRunnable Every(TimeSpan interval);
        /**
         * Creates events that fire a specified number of minutes after market open
        */
        IFluentSchedulingRunnable AfterMarketOpen(Symbol symbol, double minutesAfterOpen = 0, boolean extendedMarketOpen = false);
        /**
         * Creates events that fire a specified numer of minutes before market close
        */
        IFluentSchedulingRunnable BeforeMarketClose(Symbol symbol, double minuteBeforeClose = 0, boolean extendedMarketClose = false);
    }

    /**
     * Specifies the callback component of a scheduled event, as well as final filters
    */
    public interface IFluentSchedulingRunnable : IFluentSchedulingTimeSpecifier
    {
        /**
         * Filters the event times using the predicate
        */
        new IFluentSchedulingRunnable Where(Func<DateTime, bool> predicate);
        /**
         * Filters the event times to only include times where the symbol's market is considered open
        */
        IFluentSchedulingRunnable DuringMarketHours(Symbol symbol, boolean extendedMarket = false);
        /**
         * Register the defined event with the callback
        */
        ScheduledEvent Run(Action callback);
        /**
         * Register the defined event with the callback
        */
        ScheduledEvent Run(Action<DateTime> callback);
        /**
         * Register the defined event with the callback
        */
        ScheduledEvent Run(Action<String, DateTime> callback);
    }
}