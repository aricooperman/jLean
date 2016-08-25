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
using QuantConnect.Logging;

package com.quantconnect.lean.Scheduling
{
    /**
     * Real time self scheduling event
    */
    public class ScheduledEvent : IDisposable
    {
        /**
         * Gets the default time before market close end of trading day events will fire
        */
        public static final Duration SecurityEndOfDayDelta = Duration.ofMinutes(10);

        /**
         * Gets the default time before midnight end of day events will fire
        */
        public static final Duration AlgorithmEndOfDayDelta = Duration.ofMinutes(2);

        private boolean _needsMoveNext;
        private boolean _endOfScheduledEvents;

        private final String _name;
        private final Action<String, DateTime> _callback;
        private final IEnumerator<DateTime> _orderedEventUtcTimes;

        /**
         * Event that fires each time this scheduled event happens
        */
        public event Action<String, DateTime> EventFired;

        /**
         * Gets or sets whether this event is enabled
        */
        public boolean Enabled
        {
            get; set;
        }

        /**
         * Gets or sets whether this event will log each time it fires
        */
        internal boolean IsLoggingEnabled
        {
            get; set;
        }

        /**
         * Gets the next time this scheduled event will fire in UTC
        */
        public DateTime NextEventUtcTime
        {
            get { return _endOfScheduledEvents ? DateTime.MaxValue : _orderedEventUtcTimes.Current; }
        }

        /**
         * Gets an identifier for this event
        */
        public String Name
        {
            get { return _name; }
        }

        /**
         * Initalizes a new instance of the <see cref="ScheduledEvent"/> class
        */
         * @param name An identifier for this event
         * @param eventUtcTime The date time the event should fire
         * @param callback Delegate to be called when the event time passes
        public ScheduledEvent( String name, DateTime eventUtcTime, Action<String, DateTime> callback = null )
            : this(name, new[] { eventUtcTime }.AsEnumerable().GetEnumerator(), callback) {
        }

        /**
         * Initalizes a new instance of the <see cref="ScheduledEvent"/> class
        */
         * @param name An identifier for this event
         * @param orderedEventUtcTimes An enumerable that emits event times
         * @param callback Delegate to be called each time an event passes
        public ScheduledEvent( String name, IEnumerable<DateTime> orderedEventUtcTimes, Action<String, DateTime> callback = null )
            : this(name, orderedEventUtcTimes.GetEnumerator(), callback) {
        }

        /**
         * Initalizes a new instance of the <see cref="ScheduledEvent"/> class
        */
         * @param name An identifier for this event
         * @param orderedEventUtcTimes An enumerator that emits event times
         * @param callback Delegate to be called each time an event passes
        public ScheduledEvent( String name, IEnumerator<DateTime> orderedEventUtcTimes, Action<String, DateTime> callback = null ) {
            _name = name;
            _callback = callback;
            _orderedEventUtcTimes = orderedEventUtcTimes;

            // prime the pump
            _endOfScheduledEvents = !_orderedEventUtcTimes.MoveNext();

            Enabled = true;
        }

        /**
         * Scans this event and fires the callback if an event happened
        */
         * @param utcTime The current time in UTC
        internal void Scan(DateTime utcTime) {
            if( _endOfScheduledEvents) {
                return;
            }

            do
            {
                if( _needsMoveNext) {
                    // if we've passed an event or are just priming the pump, we need to move next
                    if( !_orderedEventUtcTimes.MoveNext()) {
                        if( IsLoggingEnabled) {
                            Log.Trace( String.format( "ScheduledEvent.%1$s: Completed scheduled events.", Name));
                        }
                        _endOfScheduledEvents = true;
                        return;
                    }
                    if( IsLoggingEnabled) {
                        Log.Trace( String.format( "ScheduledEvent.%1$s: Next event: %2$s UTC", Name, _orderedEventUtcTimes.Current.toString(DateFormat.UI)));
                    }
                }

                // if time has passed our event
                if( utcTime >= _orderedEventUtcTimes.Current) {
                    if( IsLoggingEnabled) {
                        Log.Trace( String.format( "ScheduledEvent.%1$s: Firing at %2$s UTC Scheduled at %3$s UTC", Name,
                            utcTime.toString(DateFormat.UI),
                            _orderedEventUtcTimes.Current.toString(DateFormat.UI))
                            );
                    }
                    // fire the event
                    OnEventFired(_orderedEventUtcTimes.Current);
                    _needsMoveNext = true;
                }
                else
                {
                    // we haven't passed the event time yet, so keep waiting on this Current
                    _needsMoveNext = false;
                }
            }
            // keep checking events until we pass the current time, this will fire
            // all 'skipped' events back to back in order, perhaps this should be handled
            // in the real time handler
            while (_needsMoveNext);
        }

        /**
         * Fast forwards this schedule to the specified time without invoking the events
        */
         * @param utcTime Frontier time
        internal void SkipEventsUntil(DateTime utcTime) {
            // check if our next event is in the past
            if( utcTime < _orderedEventUtcTimes.Current) return;

            while (_orderedEventUtcTimes.MoveNext()) {
                // zoom through the enumerator until we get to the desired time
                if( utcTime <= _orderedEventUtcTimes.Current) {
                    // pump is primed and ready to go
                    _needsMoveNext = false;

                    if( IsLoggingEnabled) {
                        Log.Trace( String.format( "ScheduledEvent.%1$s: Skipped events before %2$s. Next event: %3$s", Name,
                            utcTime.toString(DateFormat.UI),
                            _orderedEventUtcTimes.Current.toString(DateFormat.UI)
                            ));
                    }
                    return;
                }
            }
            if( IsLoggingEnabled) {
                Log.Trace( String.format( "ScheduledEvent.%1$s: Exhausted event stream during skip until %2$s", Name,
                    utcTime.toString(DateFormat.UI)
                    ));
            }
            _endOfScheduledEvents = true;
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
         * <filterpriority>2</filterpriority>
        void IDisposable.Dispose() {
            _orderedEventUtcTimes.Dispose();
        }

        /**
         * Event invocator for the <see cref="EventFired"/> event
        */
         * @param triggerTime The event's time in UTC
        protected void OnEventFired(DateTime triggerTime) {
            // don't fire the event if we're turned off
            if( !Enabled) return;

            if( _callback != null ) {
                _callback(_name, _orderedEventUtcTimes.Current);
            }
            handler = EventFired;
            if( handler != null ) handler(_name, triggerTime);
        }
    }
}
