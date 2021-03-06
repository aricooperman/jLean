﻿/*
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Logging;
using QuantConnect.Packets;
using QuantConnect.Scheduling;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.RealTime
{
    /**
     * Live trading realtime event processing.
    */
    public class LiveTradingRealTimeHandler : IRealTimeHandler
    {
        private boolean _isActive = true;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        // initialize this immediately since the Initialzie method gets called after IAlgorithm.Initialize,
        // so we want to be ready to accept events as soon as possible
        private final ConcurrentMap<String, ScheduledEvent> _scheduledEvents = new ConcurrentMap<String, ScheduledEvent>();

        //Algorithm and Handlers:
        private IApi _api;
        private IAlgorithm _algorithm;
        private IResultHandler _resultHandler;

        /**
         * Boolean flag indicating thread state.
        */
        public boolean IsActive
        {
            get { return _isActive; }
        }

        /**
         * Intializes the real time handler for the specified algorithm and job
        */
        public void Setup(IAlgorithm algorithm, AlgorithmNodePacket job, IResultHandler resultHandler, IApi api) {
            //Initialize:
            _api = api;
            _algorithm = algorithm;
            _resultHandler = resultHandler;
            _cancellationTokenSource = new CancellationTokenSource();

            todayInAlgorithmTimeZone = DateTime.UtcNow Extensions.convertFromUtc(_algorithm.TimeZone).Date;

            // refresh the market hours for today explicitly, and then set up an event to refresh them each day at midnight
            RefreshMarketHoursToday(todayInAlgorithmTimeZone);

            // every day at midnight from tomorrow until the end of time
            times =
                from date in Time.EachDay(todayInAlgorithmTimeZone.AddDays(1), Time.EndOfTime)
                select date Extensions.convertToUtc(_algorithm.TimeZone);

            Add(new ScheduledEvent( "RefreshMarketHours", times, (name, triggerTime) =>
            {
                // refresh market hours from api every day
                RefreshMarketHoursToday(triggerTime Extensions.convertFromUtc(_algorithm.TimeZone).Date);
            }));

            // add end of day events for each tradeable day
            Add(ScheduledEventFactory.EveryAlgorithmEndOfDay(_algorithm, _resultHandler, todayInAlgorithmTimeZone, Time.EndOfTime, ScheduledEvent.AlgorithmEndOfDayDelta, DateTime.UtcNow));

            // add end of trading day events for each security
            foreach (security in _algorithm.Securities.Values.Where(x -> x.IsInternalFeed())) {
                // assumes security.Exchange has been updated with today's hours via RefreshMarketHoursToday
                Add(ScheduledEventFactory.EverySecurityEndOfDay(_algorithm, _resultHandler, security, todayInAlgorithmTimeZone, Time.EndOfTime, ScheduledEvent.SecurityEndOfDayDelta, DateTime.UtcNow));
            }

            foreach (scheduledEvent in _scheduledEvents) {
                // zoom past old events
                scheduledEvent.Value.SkipEventsUntil(algorithm.UtcTime);
                // set logging accordingly
                scheduledEvent.Value.IsLoggingEnabled = Log.DebuggingEnabled;
            }
        }

        /**
         * Execute the live realtime event thread montioring. 
         * It scans every second monitoring for an event trigger.
        */
        public void Run() {
            _isActive = true;

            // continue thread until cancellation is requested
            while (!_cancellationTokenSource.IsCancellationRequested) {
                try
                {
                    time = DateTime.UtcNow;

                    // pause until the next second
                    nextSecond = time.RoundUp(Duration.ofSeconds(1));
                    delay =  Integer.parseInt( (nextSecond - time).TotalMilliseconds);
                    Thread.Sleep(delay < 0 ? 1 : delay);

                    // poke each event to see if it should fire
                    foreach (scheduledEvent in _scheduledEvents) {
                        scheduledEvent.Value.Scan(time);
                    }
                }
                catch (Exception err) {
                    Log.Error(err);
                }
            }

            _isActive = false;
            Log.Trace( "LiveTradingRealTimeHandler.Run(): Exiting thread... Exit triggered: " + _cancellationTokenSource.IsCancellationRequested);
        }

        /**
         * Refresh the Today variable holding the market hours information
        */
        private void RefreshMarketHoursToday(DateTime date) {
            date = date.Date;

            // update market hours for each security
            foreach (security in _algorithm.Securities.Values) {
                marketHours = _api.MarketToday(date, security.Symbol);
                security.Exchange.SetMarketHours(marketHours, date.DayOfWeek);
                localMarketHours = security.Exchange.Hours.MarketHours[date.DayOfWeek];
                Log.Trace( String.format( "LiveTradingRealTimeHandler.SetupEvents(%1$s): Market hours set: Symbol: %2$s %3$s",
                        security.Type, security.Symbol, localMarketHours));
            }
        }

        /**
         * Adds the specified event to the schedule
        */
         * @param scheduledEvent The event to be scheduled, including the date/times the event fires and the callback
        public void Add(ScheduledEvent scheduledEvent) {
            if( _algorithm != null ) {
                scheduledEvent.SkipEventsUntil(_algorithm.UtcTime);
            }

            _scheduledEvents.AddOrUpdate(scheduledEvent.Name, scheduledEvent);
        }

        /**
         * Removes the specified event from the schedule
        */
         * @param name">
        public void Remove( String name) {
            ScheduledEvent scheduledEvent;
            _scheduledEvents.TryRemove(name, out scheduledEvent);
        }

        /**
         * Set the current time. If the date changes re-start the realtime event setup routines.
        */
         * @param time">
        public void SetTime(DateTime time) {
            // in live mode we use current time for our time keeping
            // this method is used by backtesting to set time based on the data
        }

        /**
         * Stop the real time thread
        */
        public void Exit() {
            _cancellationTokenSource.Cancel();
        }
    }
}