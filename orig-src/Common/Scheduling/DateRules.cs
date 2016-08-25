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
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Scheduling
{
    /**
     * Helper class used to provide better syntax when defining date rules
    */
    public class DateRules
    {
        private final SecurityManager _securities;

        /**
         * Initializes a new instance of the <see cref="DateRules"/> helper class
        */
         * @param securities The security manager
        public DateRules(SecurityManager securities) {
            _securities = securities;
        }

        /**
         * Specifies an event should fire only on the specified day
        */
         * @param year The year
         * @param month The month
         * @param day The day
        @returns 
        public IDateRule On(int year, int month, int day) {
            // make sure they're date objects
            dates = new[] {new DateTime(year, month, day)};
            return new FuncDateRule( String.Join( ",", dates.Select(x -> x.ToShortDateString())), (start, end) -> dates);
        }

        /**
         * Specifies an event should fire only on the specified days
        */
         * @param dates The dates the event should fire
        @returns 
        public IDateRule On(params DateTime[] dates) {
            // make sure they're date objects
            dates = dates.Select(x -> x.Date).ToArray();
            return new FuncDateRule( String.Join( ",", dates.Select(x -> x.ToShortDateString())), (start, end) -> dates);
        }

        /**
         * Specifies an event should fire on each of the specified days of week
        */
         * @param days The days the event shouls fire
        @returns A date rule that fires on every specified day of week
        public IDateRule Every(params DayOfWeek[] days) {
            hash = days.ToHashSet();
            return new FuncDateRule( String.Join( ",", days), (start, end) -> Time.EachDay(start, end).Where(date -> hash.Contains(date.DayOfWeek)));
        }

        /**
         * Specifies an event should fire every day
        */
        @returns A date rule that fires every day
        public IDateRule EveryDay() {
            return new FuncDateRule( "EveryDay", Time.EachDay);
        }

        /**
         * Specifies an event should fire every day the symbol is trading
        */
         * @param symbol The symbol whose exchange is used to determine tradeable dates
        @returns A date rule that fires every day the specified symbol trades
        public IDateRule EveryDay(Symbol symbol) {
            security = GetSecurity(symbol);
            return new FuncDateRule(symbol.toString() + ": EveryDay", (start, end) -> Time.EachTradeableDay(security, start, end));
        }

        /**
         * Specifies an event should fire on the first of each month
        */
        @returns A date rule that fires on the first of each month
        public IDateRule MonthStart() {
            return new FuncDateRule( "MonthStart", (start, end) -> MonthStartIterator(null, start, end));
        }

        /**
         * Specifies an event should fire on the first tradeable date for the specified
         * symbol of each month
        */
         * @param symbol The symbol whose exchange is used to determine the first 
         * tradeable date of the month
        @returns A date rule that fires on the first tradeable date for the specified security each month
        public IDateRule MonthStart(Symbol symbol) {
            return new FuncDateRule(symbol.toString() + ": MonthStart", (start, end) -> MonthStartIterator(GetSecurity(symbol), start, end));
        }

        /**
         * Gets the security with the specified symbol, or throws an exception if the symbol is not found
        */
         * @param symbol The security's symbol to search for
        @returns The security object matching the given symbol
        private Security GetSecurity(Symbol symbol) {
            Security security;
            if( !_securities.TryGetValue(symbol, out security)) {
                throw new Exception(symbol.toString() + " not found in portfolio. Request this data when initializing the algorithm.");
            }
            return security;
        }

        private static IEnumerable<DateTime> MonthStartIterator(Security security, DateTime start, DateTime end) {
            if( security == null ) {
                foreach (date in Time.EachDay(start, end)) {
                    // fire on the first of each month
                    if( date.Day == 1) yield return date;
                }
                yield break;
            }

            // start a month back so we can properly resolve the first event (we may have passed it)
            aMonthBeforeStart = start.AddMonths(-1);
            int lastMonth = aMonthBeforeStart.Month;
            foreach (date in Time.EachTradeableDay(security, aMonthBeforeStart, end)) {
                if( date.Month != lastMonth) {
                    if( date >= start) {
                        // only emit if the date is on or after the start
                        // the date may be before here because we backed up a month
                        // to properly resolve the first tradeable date
                        yield return date;
                    }
                    lastMonth = date.Month;
                }
            }
        }
    }
}
