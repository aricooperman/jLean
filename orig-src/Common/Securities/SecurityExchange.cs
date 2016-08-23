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
using QuantConnect.Util;

package com.quantconnect.lean.Securities
{
    /**
    /// Base exchange class providing information and helper tools for reading the current exchange situation
    */
    public class SecurityExchange 
    {
        private DateTime _localFrontier;
        private SecurityExchangeHours _exchangeHours;

        /**
        /// Gets the <see cref="SecurityExchangeHours"/> for this exchange
        */
        public SecurityExchangeHours Hours
        {
            get { return _exchangeHours; }
        }

        /**
        /// Gets the time zone for this exchange
        */
        public ZoneId TimeZone 
        {
            get { return _exchangeHours.TimeZone; }
        }

        /**
        /// Number of trading days per year for this security. By default the market is open 365 days per year.
        */
        /// Used for performance statistics to calculate sharpe ratio accurately
        public virtual int TradingDaysPerYear
        {
            get { return 365; }
        }

        /**
        /// Time from the most recent data
        */
        public DateTime LocalTime
        {
            get { return _localFrontier; }
        }

        /**
        /// Boolean property for quickly testing if the exchange is open.
        */
        public boolean ExchangeOpen
        {
            get { return _exchangeHours.IsOpen(_localFrontier, false); }
        }

        /**
        /// Initializes a new instance of the <see cref="SecurityExchange"/> class using the specified
        /// exchange hours to determine open/close times
        */
         * @param exchangeHours">Contains the weekly exchange schedule plus holidays
        public SecurityExchange(SecurityExchangeHours exchangeHours) {
            _exchangeHours = exchangeHours;
        }

        /**
        /// Set the current datetime in terms of the exchange's local time zone
        */
         * @param newLocalTime">Most recent data tick
        public void SetLocalDateTimeFrontier(DateTime newLocalTime) {
            _localFrontier = newLocalTime;
        }

        /**
        /// Check if the *date* is open.
        */
        /// This is useful for first checking the date list, and then the market hours to save CPU cycles
         * @param dateToCheck">Date to check
        @returns Return true if the exchange is open for this date
        public boolean DateIsOpen(DateTime dateToCheck) {
            return _exchangeHours.IsDateOpen(dateToCheck);
        }

        /**
        /// Check if this DateTime is open.
        */
         * @param dateTime">DateTime to check
        @returns Boolean true if the market is open
        public boolean DateTimeIsOpen(DateTime dateTime) {
            return _exchangeHours.IsOpen(dateTime, false);
        }

        /**
        /// Determines if the exchange was open at any time between start and stop
        */
        public boolean IsOpenDuringBar(DateTime barStartTime, DateTime barEndTime, boolean isExtendedMarketHours) {
            return _exchangeHours.IsOpen(barStartTime, barEndTime, isExtendedMarketHours);
        }

        /**
        /// Sets the regular market hours for the specified days If no days are specified then
        /// all days will be updated.
        */
         * @param marketHoursSegments">Specifies each segment of the market hours, such as premarket/market/postmark
         * @param days">The days of the week to set these times for
        public void SetMarketHours(IEnumerable<MarketHoursSegment> marketHoursSegments, params DayOfWeek[] days) {
            if( days.IsNullOrEmpty()) days = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().ToArray();
            
            marketHours = _exchangeHours.MarketHours.ToDictionary();
            marketHoursSegments = marketHoursSegments as IList<MarketHoursSegment> ?? marketHoursSegments.ToList();
            foreach (day in days) {
                marketHours[day] = new LocalMarketHours(day, marketHoursSegments);
            }

            // create a new exchange hours instance for the new hours
            _exchangeHours = new SecurityExchangeHours(_exchangeHours.TimeZone, _exchangeHours.Holidays, marketHours);
        }
    }
}