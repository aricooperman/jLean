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
    /// Uses a function to define a time rule as a projection of date times to date times
    */
    public class FuncTimeRule : ITimeRule
    {
        private final Func<IEnumerable<DateTime>, IEnumerable<DateTime>> _createUtcEventTimesFunction;

        /**
        /// Initializes a new instance of the <see cref="FuncTimeRule"/> class
        */
         * @param name">The name of the time rule
         * @param createUtcEventTimesFunction">Function used to transform dates into event date times
        public FuncTimeRule( String name, Func<IEnumerable<DateTime>, IEnumerable<DateTime>> createUtcEventTimesFunction) {
            Name = name;
            _createUtcEventTimesFunction = createUtcEventTimesFunction;
        }

        /**
        /// Gets a name for this rule
        */
        public String Name
        {
            get; private set;
        }

        /**
        /// Creates the event times for the specified dates in UTC
        */
         * @param dates">The dates to apply times to
        @returns An enumerable of date times that is the result
        /// of applying this rule to the specified dates
        public IEnumerable<DateTime> CreateUtcEventTimes(IEnumerable<DateTime> dates) {
            return _createUtcEventTimesFunction(dates);
        }
    }
}