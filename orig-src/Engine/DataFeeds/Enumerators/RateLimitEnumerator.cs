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
using QuantConnect.Data;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Enumerators
{
    /**
    /// Provides augmentation of how often an enumerator can be called. Time is measured using
    /// an <see cref="ITimeProvider"/> instance and calls to the underlying enumerator are limited
    /// to a minimum time between each call.
    */
    public class RateLimitEnumerator : IEnumerator<BaseData>
    {
        private BaseData _current;
        private DateTime _lastCallTime;

        private final ITimeProvider _timeProvider;
        private final IEnumerator<BaseData> _enumerator;
        private final Duration _minimumTimeBetweenCalls;

        /**
        /// Initializes a new instance of the <see cref="RateLimitEnumerator"/> class
        */
         * @param enumerator">The underlying enumerator to place rate limits on
         * @param timeProvider">Time provider used for determing the time between calls
         * @param minimumTimeBetweenCalls">The minimum time allowed between calls to the underlying enumerator
        public RateLimitEnumerator(IEnumerator<BaseData> enumerator, ITimeProvider timeProvider, Duration minimumTimeBetweenCalls) {
            _enumerator = enumerator;
            _timeProvider = timeProvider;
            _minimumTimeBetweenCalls = minimumTimeBetweenCalls;
        }

        /**
        /// Advances the enumerator to the next element of the collection.
        */
        @returns 
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// 
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public boolean MoveNext() {
            // determine time since last successful call, do this on units of the minimum time
            // this will give us nice round emit times
            currentTime = _timeProvider.GetUtcNow().RoundDown(_minimumTimeBetweenCalls);
            timeBetweenCalls = currentTime - _lastCallTime;

            // if within limits, patch it through to move next
            if( timeBetweenCalls >= _minimumTimeBetweenCalls) {
                if( !_enumerator.MoveNext()) {
                    // our underlying is finished
                    _current = null;
                    return false;
                }

                // only update last call time on non rate limited requests
                _lastCallTime = currentTime;
                _current = _enumerator.Current;
            }
            else
            {
                // we've been rate limitted
                _current = null;
            }

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
            get { return _current; }
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