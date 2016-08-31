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
using System.Collections;
using System.Collections.Generic;
using NodaTime;
using QuantConnect.Data;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Enumerators
{
    /**
     * Provides the ability to fast forward an enumerator based on the age of the data
    */
    public class FastForwardEnumerator : IEnumerator<BaseData>
    {
        private BaseData _current;

        private final ZoneId _timeZone;
        private final Duration _maximumDataAge;
        private final ITimeProvider _timeProvider;
        private final IEnumerator<BaseData> _enumerator;

        /**
         * Initializes a new instance of the <see cref="FastForwardEnumerator"/> class
        */
         * @param enumerator The source enumerator
         * @param timeProvider A time provider used to determine age of data
         * @param timeZone The data's time zone
         * @param maximumDataAge The maximum age of data allowed
        public FastForwardEnumerator(IEnumerator<BaseData> enumerator, ITimeProvider timeProvider, ZoneId timeZone, Duration maximumDataAge) {
            _enumerator = enumerator;
            _timeProvider = timeProvider;
            _timeZone = timeZone;
            _maximumDataAge = maximumDataAge;
        }

        /**
         * Advances the enumerator to the next element of the collection.
        */
        @returns 
         * true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
         * 
        public boolean MoveNext() {
            // keep churning until recent data or null
            while (_enumerator.MoveNext()) {
                // we can't fast forward nulls or bad times
                if( _enumerator.Current == null || _enumerator.Current.Time == DateTime.MinValue) {
                    _current = null;
                    return true;
                }

                // make sure we never emit past data
                if( _current != null && _current.EndTime > _enumerator.Current.EndTime) {
                    continue;
                }

                // comute the age of the data, if within limits we're done
                age = _timeProvider.GetUtcNow() Extensions.convertFromUtc(_timeZone) - _enumerator.Current.EndTime;
                if( age <= _maximumDataAge) {
                    _current = _enumerator.Current;
                    return true;
                }
            }

            // we've exhausted the underlying enumerator, iterator completed
            _current = _enumerator.Current;
            return true;
        }

        /**
         * Sets the enumerator to its initial position, which is before the first element in the collection.
        */
         * <exception cref="T:System.InvalidOperationException The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public void Reset() {
            _enumerator.Reset();
        }

        /**
         * Gets the element in the collection at the current position of the enumerator.
        */
        @returns 
         * The element in the collection at the current position of the enumerator.
         * 
        public BaseData Current
        {
            get { return _current; }
        }

        /**
         * Gets the current element in the collection.
        */
        @returns 
         * The current element in the collection.
         * 
         * <filterpriority>2</filterpriority>
        object IEnumerator.Current
        {
            get { return _current; }
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
         * <filterpriority>2</filterpriority>
        public void Dispose() {
            _enumerator.Dispose();
        }

    }
}