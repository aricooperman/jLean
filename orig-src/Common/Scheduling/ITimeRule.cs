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
using System.Collections.Generic;

package com.quantconnect.lean.Scheduling
{
    /**
     * Specifies times times on dates for events, used in conjunction with <see cref="IDateRule"/>
    */
    public interface ITimeRule
    {
        /**
         * Gets a name for this rule
        */
        String Name { get; }

        /**
         * Creates the event times for the specified dates in UTC
        */
         * @param dates The dates to apply times to
        @returns An enumerable of date times that is the result
         * of applying this rule to the specified dates
        IEnumerable<DateTime> CreateUtcEventTimes(IEnumerable<DateTime> dates);
    }
}