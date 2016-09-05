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
using NodaTime;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Provides an implementation of <see cref="ITimeProvider"/> that can be
     * manually advanced through time
    */
    public class ManualTimeProvider : ITimeProvider
    {
        private DateTime _currentTime;
        private final ZoneId _setCurrentTimeTimeZone;

        /**
         * Initializes a new instance of the <see cref="ManualTimeProvider"/>
        */
         * @param setCurrentTimeTimeZone Specify to use this time zone when calling <see cref="SetCurrentTime"/>,
         * leave null for the deault of <see cref="Global.UTC_ZONE_ID"/>
        public ManualTimeProvider(ZoneId setCurrentTimeTimeZone = null ) {
            _setCurrentTimeTimeZone = setCurrentTimeTimeZone ?? Global.UTC_ZONE_ID;
        }

        /**
         * Initializes a new instance of the <see cref="ManualTimeProvider"/> class
        */
         * @param currentTime The current time in the specified time zone, if the time zone is
         * null then the time is interpreted as being in <see cref="Global.UTC_ZONE_ID"/>
         * @param setCurrentTimeTimeZone Specify to use this time zone when calling <see cref="SetCurrentTime"/>,
         * leave null for the deault of <see cref="Global.UTC_ZONE_ID"/>
        public ManualTimeProvider(DateTime currentTime, ZoneId setCurrentTimeTimeZone = null ) {
            _setCurrentTimeTimeZone = setCurrentTimeTimeZone ?? Global.UTC_ZONE_ID;
            _currentTime = currentTime Extensions.convertToUtc(_setCurrentTimeTimeZone);
        }

        /**
         * Gets the current time in UTC
        */
        @returns The current time in UTC
        public DateTime GetUtcNow() {
            return _currentTime;
        }

        /**
         * Sets the current time interpreting the specified time as a UTC time
        */
         * @param time The current time in UTC
        public void SetCurrentTimeUtc(DateTime time) {
            _currentTime = time;
        }

        /**
         * Sets the current time interpeting the specified time as a local time
         * using the time zone used at instatiation.
        */
         * @param time The local time to set the current time time, will be
         * converted into UTC
        public void SetCurrentTime(DateTime time) {
            _currentTime = time Extensions.convertToUtc(_setCurrentTimeTimeZone);
        }

        /**
         * Advances the current time by the specified span
        */
         * @param span The amount of time to advance the current time by
        public void Advance(TimeSpan span) {
            _currentTime += span;
        }

        /**
         * Advances the current time by the specified number of seconds
        */
         * @param seconds The number of seconds to advance the current time by
        public void AdvanceSeconds(double seconds) {
            Advance(Duration.ofSeconds(seconds));
        }
    }
}