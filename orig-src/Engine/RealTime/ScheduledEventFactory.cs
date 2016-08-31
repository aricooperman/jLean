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
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Logging;
using QuantConnect.Scheduling;
using QuantConnect.Securities;

package com.quantconnect.lean.Lean.Engine.RealTime
{
    /**
     * Provides methods for creating common scheduled events
    */
    public static class ScheduledEventFactory
    {
        /**
         * Creates a new <see cref="ScheduledEvent"/> that will fire at the specified <paramref name="timeOfDay"/> for every day in
         * <paramref name="dates"/>
        */
         * @param name An identifier for this event
         * @param dates The dates to set events for at the specified time. These act as a base time to which
         * the <paramref name="timeOfDay"/> is added to, that is, the implementation does not use .Date before
         * the addition
         * @param timeOfDay The time each tradeable date to fire the event
         * @param callback The delegate to call when an event fires
         * @param currentUtcTime Specfies the current time in UTC, before which, no events will be scheduled. Specify null to skip this filter.
        @returns A new <see cref="ScheduledEvent"/> instance that fires events each tradeable day from the start to the finish at the specified time
        public static ScheduledEvent EveryDayAt( String name, IEnumerable<DateTime> dates, Duration timeOfDay, Action<String, DateTime> callback, DateTime? currentUtcTime = null ) {
            eventTimes = dates.Select(x -> x.Date + timeOfDay);
            if( currentUtcTime.HasValue) {
                eventTimes = eventTimes.Where(x -> x < currentUtcTime.Value);
            }
            return new ScheduledEvent(name, eventTimes, callback);
        }

        /**
         * Creates a new <see cref="ScheduledEvent"/> that will fire before market close by the specified time 
        */
         * @param algorithm The algorithm instance the event is fo
         * @param resultHandler The result handler, used to communicate run time errors
         * @param start The date to start the events
         * @param end The date to end the events
         * @param endOfDayDelta The time difference between the market close and the event, positive time will fire before market close
         * @param currentUtcTime Specfies the current time in UTC, before which, no events will be scheduled. Specify null to skip this filter.
        @returns The new <see cref="ScheduledEvent"/> that will fire near market close each tradeable dat
        public static ScheduledEvent EveryAlgorithmEndOfDay(IAlgorithm algorithm, IResultHandler resultHandler, DateTime start, DateTime end, Duration endOfDayDelta, DateTime? currentUtcTime = null ) {
            if( endOfDayDelta >= Duration.ofDays( 1 )) {
                throw new IllegalArgumentException( "Delta must be less than a day", "endOfDayDelta");
            }

            // set up an event to fire every tradeable date for the algorithm as a whole
            eodEventTime = Duration.ofDays( 1 ).Subtract(endOfDayDelta);

            // create enumerable of end of day in algorithm's time zone
            times =
                // for every date any exchange is open in the algorithm
                from date in Time.EachTradeableDay(algorithm.Securities.Values, start, end)
                // define the time of day we want the event to fire, a little before midnight
                let eventTime = date + eodEventTime
                // convert the event time into UTC
                let eventUtcTime = eventTime Extensions.convertToUtc(algorithm.TimeZone)
                // perform filter to verify it's not before the current time
                where !currentUtcTime.HasValue || eventUtcTime > currentUtcTime.Value
                select eventUtcTime;

            return new ScheduledEvent(CreateEventName( "Algorithm", "EndOfDay"), times, (name, triggerTime) =>
            {
                try
                {
                    algorithm.OnEndOfDay();
                }
                catch (Exception err) {
                    resultHandler.RuntimeError(String.format( "Runtime error in %1$s event: %2$s", name, err.Message), err.StackTrace);
                    Log.Error(err, String.format( "ScheduledEvent.%1$s:", name));
                }
            });
        }

        /**
         * Creates a new <see cref="ScheduledEvent"/> that will fire before market close by the specified time 
        */
         * @param algorithm The algorithm instance the event is fo
         * @param resultHandler The result handler, used to communicate run time errors
         * @param security The security used for defining tradeable dates
         * @param start The first date for the events
         * @param end The date to end the events
         * @param endOfDayDelta The time difference between the market close and the event, positive time will fire before market close
         * @param currentUtcTime Specfies the current time in UTC, before which, no events will be scheduled. Specify null to skip this filter.
        @returns The new <see cref="ScheduledEvent"/> that will fire near market close each tradeable dat
        public static ScheduledEvent EverySecurityEndOfDay(IAlgorithm algorithm, IResultHandler resultHandler, Security security, DateTime start, DateTime end, Duration endOfDayDelta, DateTime? currentUtcTime = null ) {
            if( endOfDayDelta >= Duration.ofDays( 1 )) {
                throw new IllegalArgumentException( "Delta must be less than a day", "endOfDayDelta");
            }

            // define all the times we want this event to be fired, every tradeable day for the securtiy
            // at the delta time before market close expressed in UTC
            times =
                // for every date the exchange is open for this security
                from date in Time.EachTradeableDay(security, start, end)
                // get the next market close for the specified date
                let marketClose = security.Exchange.Hours.GetNextMarketClose(date, security.IsExtendedMarketHours)
                // define the time of day we want the event to fire before marketclose
                let eventTime = marketClose.Subtract(endOfDayDelta)
                // convert the event time into UTC
                let eventUtcTime = eventTime Extensions.convertToUtc(security.Exchange.TimeZone)
                // perform filter to verify it's not before the current time
                where !currentUtcTime.HasValue || eventUtcTime > currentUtcTime
                select eventUtcTime;

            return new ScheduledEvent(CreateEventName(security.Symbol.toString(), "EndOfDay"), times, (name, triggerTime) =>
            {
                try
                {
                    algorithm.OnEndOfDay(security.Symbol);
                }
                catch (Exception err) {
                    resultHandler.RuntimeError(String.format( "Runtime error in %1$s event: %2$s", name, err.Message), err.StackTrace);
                    Log.Error(err, String.format( "ScheduledEvent.%1$s:", name));
                }
            });
        }

        /**
         * Defines the format of event names generated by this system.
        */
         * @param scope The scope of the event, example, 'Algorithm' or 'Security'
         * @param name A name for this specified event in this scope, example, 'EndOfDay'
        @returns A String representing a fully scoped event name
        public static String CreateEventName( String scope, String name) {
            return String.format( "%1$s.%2$s", scope, name);
        }
    }
}