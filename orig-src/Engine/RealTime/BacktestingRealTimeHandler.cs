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
using System.Collections.Concurrent;
using System.Linq;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Logging;
using QuantConnect.Packets;
using QuantConnect.Scheduling;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.RealTime
{
    /**
     * Psuedo realtime event processing for backtesting to simulate realtime events in fast forward.
    */
    public class BacktestingRealTimeHandler : IRealTimeHandler
    {
        private IAlgorithm _algorithm;
        private IResultHandler _resultHandler;
        // initialize this immediately since the Initialzie method gets called after IAlgorithm.Initialize,
        // so we want to be ready to accept events as soon as possible
        private final ConcurrentMap<String, ScheduledEvent> _scheduledEvents = new ConcurrentMap<String, ScheduledEvent>();

        /**
         * Flag indicating the hander thread is completely finished and ready to dispose.
        */
        public boolean IsActive
        {
            // this doesn't run as its own thread
            get { return false; }
        }

        /**
         * Intializes the real time handler for the specified algorithm and job
        */
        public void Setup(IAlgorithm algorithm, AlgorithmNodePacket job, IResultHandler resultHandler, IApi api) {
            //Initialize:
            _algorithm = algorithm;
            _resultHandler =  resultHandler;

            // create events for algorithm's end of tradeable dates
            Add(ScheduledEventFactory.EveryAlgorithmEndOfDay(_algorithm, _resultHandler, _algorithm.StartDate, _algorithm.EndDate, ScheduledEvent.AlgorithmEndOfDayDelta));

            // set up the events for each security to fire every tradeable date before market close
            foreach (security in _algorithm.Securities.Values.Where(x -> x.IsInternalFeed())) {
                Add(ScheduledEventFactory.EverySecurityEndOfDay(_algorithm, _resultHandler, security, algorithm.StartDate, _algorithm.EndDate, ScheduledEvent.SecurityEndOfDayDelta));
            }

            foreach (scheduledEvent in _scheduledEvents) {
                // zoom past old events
                scheduledEvent.Value.SkipEventsUntil(algorithm.UtcTime);
                // set logging accordingly
                scheduledEvent.Value.IsLoggingEnabled = Log.DebuggingEnabled;
            }
        }
        
        /**
         * Normally this would run the realtime event monitoring. Backtesting is in fastforward so the realtime is linked to the backtest clock.
         * This thread does nothing. Wait until the job is over.
        */
        public void Run() {
        }

        /**
         * Adds the specified event to the schedule
        */
         * @param scheduledEvent The event to be scheduled, including the date/times the event fires and the callback
        public void Add(ScheduledEvent scheduledEvent) {
            if( _algorithm != null ) {
                scheduledEvent.SkipEventsUntil(_algorithm.UtcTime);
            }

            _scheduledEvents[scheduledEvent.Name] = scheduledEvent;
            if( Log.DebuggingEnabled) {
                scheduledEvent.IsLoggingEnabled = true;
            }
        }

        /**
         * Removes the specified event from the schedule
        */
         * @param name The name of the event to remove
        public void Remove( String name) {
            ScheduledEvent scheduledEvent;
            _scheduledEvents.TryRemove(name, out scheduledEvent);
        }

        /**
         * Set the time for the realtime event handler.
        */
         * @param time Current time.
        public void SetTime(DateTime time) {
            // poke each event to see if it has fired, be sure to invoke these in time order
            foreach (scheduledEvent in _scheduledEvents)//.OrderBy(x -> x.Value.NextEventUtcTime)) {
                scheduledEvent.Value.Scan(time);
            }
        }

        /**
         * Stop the real time thread
        */
        public void Exit() {
            // this doesn't run as it's own thread, so nothing to exit
        }
    }
}