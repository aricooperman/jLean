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
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Data;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Enumerators
{
    /**
    /// Provides an implementation of <see cref="IEnumerator{BaseData}"/> that will not emit
    /// data ahead of the frontier as specified by an instance of <see cref="ITimeProvider"/>.
    /// An instance of <see cref="TimeZoneOffsetProvider"/> is used to convert between UTC
    /// and the data's native time zone
    */
    public class FrontierAwareEnumerator : IEnumerator<BaseData>
    {
        private BaseData _current;
        private boolean _needsMoveNext = true;

        private final ITimeProvider _timeProvider;
        private final IEnumerator<BaseData> _enumerator;
        private final TimeZoneOffsetProvider _offsetProvider;

        private BaseData _lastEmittedValue;

        /**
        /// Initializes a new instance of the <see cref="FrontierAwareEnumerator"/> class
        */
         * @param enumerator">The underlying enumerator to make frontier aware
         * @param timeProvider">The time provider used for resolving the current frontier time
         * @param offsetProvider">An offset provider used for converting the frontier UTC time into the data's native time zone
        public FrontierAwareEnumerator(IEnumerator<BaseData> enumerator, ITimeProvider timeProvider, TimeZoneOffsetProvider offsetProvider) {
            _enumerator = enumerator;
            _timeProvider = timeProvider;
            _offsetProvider = offsetProvider;
        }

        /**
        /// Advances the enumerator to the next element of the collection.
        */
        @returns 
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// 
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public boolean MoveNext() {
            underlyingCurrent = _enumerator.Current;
            frontier = _timeProvider.GetUtcNow();
            localFrontier = new DateTime(frontier.Ticks + _offsetProvider.GetOffsetTicks(frontier));

            // if we moved next, but didn't emit, check to see if it's time to emit yet
            if( !_needsMoveNext && underlyingCurrent != null ) {
                if( underlyingCurrent.EndTime <= localFrontier) {
                    // we can now emit the underlyingCurrent as part of this time slice
                    _current = underlyingCurrent;
                    _needsMoveNext = true;
                    _lastEmittedValue = _current;
                }
                else
                {
                    // it's still not time to emit the underlyingCurrent, keep waiting for time to advance
                    _current = null;
                    _needsMoveNext = false;
                }
                return true;
            }

            // we've exhausted the underlying enumerator, iteration completed
            if( _needsMoveNext && !_enumerator.MoveNext()) {
                _needsMoveNext = true;
                _current = null;
                return false;
            }

            underlyingCurrent = _enumerator.Current;

            if( underlyingCurrent != null && underlyingCurrent.EndTime <= localFrontier) {
                // only emit if new data available (skip custom data if same time/value)
                if( _lastEmittedValue != null && 
                    _lastEmittedValue.DataType == MarketDataType.Base && 
                    _lastEmittedValue.EndTime == underlyingCurrent.EndTime &&
                    _lastEmittedValue.Value == underlyingCurrent.Value) {
                    _current = null;
                }
                else
                {
                    _needsMoveNext = true;
                    _current = underlyingCurrent;
                    _lastEmittedValue = _current;
                }
            }
            else
            {
                _current = null;
                _needsMoveNext = underlyingCurrent == null;
            }

            // technically we still need to return true since the iteration is not completed,
            // however, Current may be null follow a true result here
            return true;
        }

        /**
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        */
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public void Reset() {
            _enumerator.Reset();
        }

        /**
        /// Gets the element in the collection at the current position of the enumerator.
        */
        @returns 
        /// The element in the collection at the current position of the enumerator.
        /// 
        public BaseData Current
        {
            get { return _current; }
        }

        /**
        /// Gets the current element in the collection.
        */
        @returns 
        /// The current element in the collection.
        /// 
        /// <filterpriority>2</filterpriority>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        /// <filterpriority>2</filterpriority>
        public void Dispose() {
            _enumerator.Dispose();
        }
    }
}