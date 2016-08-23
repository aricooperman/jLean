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
using QuantConnect.Data.UniverseSelection;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Enumerators
{
    /**
    /// Provides an implementation of <see cref="IEnumerator{BaseDataCollection}"/>
    /// that aggregates an underlying <see cref="IEnumerator{BaseData}"/> into a single
    /// data packet
    */
    public class BaseDataCollectionAggregatorEnumerator<T> : IEnumerator<T>
        where T : BaseDataCollection, new() {
        private boolean _endOfStream;
        private boolean _needsMoveNext;
        private final Symbol _symbol;
        private final IEnumerator<BaseData> _enumerator;

        /**
        /// Initializes a new instance of the <see cref="BaseDataCollectionAggregatorEnumerator"/> class
        /// This will aggregate instances emitted from the underlying enumerator and tag them with the
        /// specified symbol
        */
         * @param enumerator">The underlying enumerator to aggregate
         * @param symbol">The symbol to place on the aggregated collection
        public BaseDataCollectionAggregatorEnumerator(IEnumerator<BaseData> enumerator, Symbol symbol) {
            _symbol = symbol;
            _enumerator = enumerator;

            _needsMoveNext = true;
        }

        /**
        /// Advances the enumerator to the next element of the collection.
        */
        @returns 
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// 
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public boolean MoveNext() {
            if( _endOfStream) {
                return false;
            }

            T collection = null;
            while (true) {
                if( _needsMoveNext) {
                    // move next if we dequeued the last item last time we were invoked
                    if( !_enumerator.MoveNext()) {
                        _endOfStream = true;
                        break;
                    }
                }

                if( _enumerator.Current == null ) {
                    // the underlying returned null, stop here and start again on the next call
                    _needsMoveNext = true;
                    break;
                }

                if( collection == null ) {
                    // we have new data, set the collection's symbol/times
                    current = _enumerator.Current;
                    collection = CreateCollection(_symbol, current.Time, current.EndTime);
                }

                if( collection.EndTime != _enumerator.Current.EndTime) {
                    // the data from the underlying is at a different time, stop here
                    _needsMoveNext = false;
                    break;
                }

                // this data belongs in this collection, keep going until null or bad time
                Add(collection, _enumerator.Current);
                _needsMoveNext = true;
            }

            Current = collection;
            return collection != null;
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
        public T Current
        {
            get; private set;
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

        /**
        /// Creates a new, empty <see cref="BaseDataCollection"/>.
        */
         * @param symbol">The base data collection symbol
         * @param time">The start time of the collection
         * @param endTime">The end time of the collection
        @returns A new, empty <see cref="BaseDataCollection"/>
        protected virtual T CreateCollection(Symbol symbol, DateTime time, DateTime endTime) {
            return new T
            {
                Symbol = symbol,
                Time = time,
                EndTime = endTime
            };
        }

        /**
        /// Adds the specified instance of <see cref="BaseData"/> to the current collection
        */
         * @param collection">The collection to be added to
         * @param current">The data to be added
        protected virtual void Add(T collection, BaseData current) {
            collection.Data.Add(current);
        }
    }

    /**
    /// Provides a non-generic implementation of <see cref="BaseDataCollectionAggregatorEnumerator{T}"/>
    */
    public class BaseDataCollectionAggregatorEnumerator : BaseDataCollectionAggregatorEnumerator<BaseDataCollection>
    {
        /**
        /// Initializes a new instance of the <see cref="BaseDataCollectionAggregatorEnumerator"/> class
        */
         * @param enumerator">The enumerator to aggregate
         * @param symbol">The output data's symbol
        public BaseDataCollectionAggregatorEnumerator(IEnumerator<BaseData> enumerator, Symbol symbol)
            : base(enumerator, symbol) {
        }
    }
}
