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

package com.quantconnect.lean.Scheduling
{
    /**
    /// Uses a function to define an enumerable of dates over a requested start/end period
    */
    public class FuncDateRule : IDateRule
    {
        private final Func<DateTime, DateTime, IEnumerable<DateTime>> _getDatesFuntion;

        /**
        /// Initializes a new instance of the <see cref="FuncDateRule"/> class
        */
         * @param name">The name of this rule
         * @param getDatesFuntion">The time applicator function
        public FuncDateRule( String name, Func<DateTime, DateTime, IEnumerable<DateTime>> getDatesFuntion) {
            Name = name;
            _getDatesFuntion = getDatesFuntion;
        }

        /**
        /// Gets a name for this rule
        */
        public String Name
        {
            get; private set;
        }

        /**
        /// Gets the dates produced by this date rule between the specified times
        */
         * @param start">The start of the interval to produce dates for
         * @param end">The end of the interval to produce dates for
        @returns All dates in the interval matching this date rule
        public IEnumerable<DateTime> GetDates(DateTime start, DateTime end) {
            return _getDatesFuntion(start, end);
        }
    }
}