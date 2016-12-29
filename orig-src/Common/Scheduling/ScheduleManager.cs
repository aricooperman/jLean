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
using NodaTime;
using QuantConnect.Securities;

package com.quantconnect.lean.Scheduling
{
    /**
     * Provides access to the real time handler's event scheduling feature
    */
    public class ScheduleManager : IEventSchedule
    {
        private IEventSchedule _eventSchedule;

        private final SecurityManager _securities;
        private final object _eventScheduleLock = new object();
        private final List<ScheduledEvent> _preInitializedEvents;

        /**
         * Gets the date rules helper object to make specifying dates for events easier
        */
        public DateRules DateRules { get; private set; }

        /**
         * Gets the time rules helper object to make specifying times for events easier
        */
        public TimeRules TimeRules { get; private set; }

        /**
         * Initializes a new instance of the <see cref="ScheduleManager"/> class
        */
         * @param securities Securities manager containing the algorithm's securities
         * @param timeZone The algorithm's time zone
        public ScheduleManager(SecurityManager securities, ZoneId timeZone) {
            _securities = securities;
            DateRules = new DateRules(securities);
            TimeRules = new TimeRules(securities, timeZone);

            // used for storing any events before the event schedule is set
            _preInitializedEvents = new List<ScheduledEvent>();
        }

        /**
         * Sets the <see cref="IEventSchedule"/> implementation
        */
         * @param eventSchedule The event schedule implementation to be used. This is the IRealTimeHandler
        internal void SetEventSchedule(IEventSchedule eventSchedule) {
            if( eventSchedule == null ) {
                throw new NullPointerException( "eventSchedule");
            }

            synchronized(_eventScheduleLock) {
                _eventSchedule = eventSchedule;

                // load up any events that were added before we were ready to send them to the scheduler
                foreach (scheduledEvent in _preInitializedEvents) {
                    _eventSchedule.Add(scheduledEvent);
                }
            }
        }

        /**
         * Adds the specified event to the schedule using the <see cref="ScheduledEvent.Name"/> as a key.
        */
         * @param scheduledEvent The event to be scheduled, including the date/times the event fires and the callback
        public void Add(ScheduledEvent scheduledEvent) {
            synchronized(_eventScheduleLock) {
                if( _eventSchedule != null ) {
                    _eventSchedule.Add(scheduledEvent);
                }
                else
                {
                    _preInitializedEvents.Add(scheduledEvent);
                }
            }
        }

        /**
         * Removes the event with the specified name from the schedule
        */
         * @param name The name of the event to be removed
        public void Remove( String name) {
            synchronized(_eventScheduleLock) {
                if( _eventSchedule != null ) {
                    _eventSchedule.Remove(name);
                }
                else
                {
                    _preInitializedEvents.RemoveAll(se -> se.Name == name);
                }
            }
        }

        /**
         * Schedules the callback to run using the specified date and time rules
        */
         * @param dateRule Specifies what dates the event should run
         * @param timeRule Specifies the times on those dates the event should run
         * @param callback The callback to be invoked
        public ScheduledEvent On(IDateRule dateRule, ITimeRule timeRule, Action callback) {
            return On(dateRule, timeRule, (name, time) -> callback());
        }

        /**
         * Schedules the callback to run using the specified date and time rules
        */
         * @param dateRule Specifies what dates the event should run
         * @param timeRule Specifies the times on those dates the event should run
         * @param callback The callback to be invoked
        public ScheduledEvent On(IDateRule dateRule, ITimeRule timeRule, Action<String, DateTime> callback) {
            name = dateRule.Name + ": " + timeRule.Name;
            return On(name, dateRule, timeRule, callback);
        }

        /**
         * Schedules the callback to run using the specified date and time rules
        */
         * @param name The event's unique name
         * @param dateRule Specifies what dates the event should run
         * @param timeRule Specifies the times on those dates the event should run
         * @param callback The callback to be invoked
        public ScheduledEvent On( String name, IDateRule dateRule, ITimeRule timeRule, Action callback) {
            return On(name, dateRule, timeRule, (n, d) -> callback());
        }

        /**
         * Schedules the callback to run using the specified date and time rules
        */
         * @param name The event's unique name
         * @param dateRule Specifies what dates the event should run
         * @param timeRule Specifies the times on those dates the event should run
         * @param callback The callback to be invoked
        public ScheduledEvent On( String name, IDateRule dateRule, ITimeRule timeRule, Action<String, DateTime> callback) {
            // back the date up to ensure we get all events, the event scheduler will skip past events that whose time has passed
            dates = dateRule.GetDates(_securities.UtcTime.Date.AddDays(-1), Time.EndOfTime);
            eventTimes = timeRule.CreateUtcEventTimes(dates);
            scheduledEvent = new ScheduledEvent(name, eventTimes, callback);
            Add(scheduledEvent);
            return scheduledEvent;
        }

        #region Fluent Scheduling

        /**
         * Entry point for the fluent scheduled event builder
        */
        public IFluentSchedulingDateSpecifier Event() {
            return new FluentScheduledEventBuilder(this, _securities);
        }

        /**
         * Entry point for the fluent scheduled event builder
        */
        public IFluentSchedulingDateSpecifier Event( String name) {
            return new FluentScheduledEventBuilder(this, _securities, name);
        }

        #endregion
    }
}
