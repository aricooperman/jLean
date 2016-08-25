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
*/

using System;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Data.Consolidators
{
    /**
     * Represents the simplest DataConsolidator implementation, one that is defined
     * by a straight pass through of the data. No projection or aggregation is performed.
    */
     * <typeparam name="T The type of data</typeparam>
    public class IdentityDataConsolidator<T> : DataConsolidator<T>
        where T : BaseData
    {
        private static final boolean IsTick = typeof (T) == typeof (Tick);

        private T _last;

        /**
         * Gets a clone of the data being currently consolidated
        */
        public @Override BaseData WorkingData
        {
            get { return _last == null ? null : _last.Clone(); }
        }

        /**
         * Gets the type produced by this consolidator
        */
        public @Override Type OutputType
        {
            get { return typeof (T); }
        }

        /**
         * Updates this consolidator with the specified data
        */
         * @param data The new data for the consolidator
        public @Override void Update(T data) {
            if( IsTick || _last == null || _last.EndTime != data.EndTime) {
                OnDataConsolidated(data);
                _last = data;
            }
        }

        /**
         * Scans this consolidator to see if it should emit a bar due to time passing
        */
         * @param currentLocalTime The current time in the local time zone (same as <see cref="BaseData.Time"/>)
        public @Override void Scan(DateTime currentLocalTime) {
        }
    }
}