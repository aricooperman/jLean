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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Enumerators
{
    /**
    /// An implementation of <see cref="IEnumerator{T}"/> that relies on the
    /// <see cref="Enqueue"/> method being called and only ends when <see cref="Stop"/>
    /// is called
    */
    /// <typeparam name="T">The item type yielded by the enumerator</typeparam>
    public class EnqueueableEnumerator<T> : IEnumerator<T>
    {
        private T _current;
        private T _lastEnqueued;
        private volatile boolean _end;
        private volatile boolean _disposed;

        private final int _timeout;
        private final object _lock = new object();
        private final BlockingCollection<T> _blockingCollection;

        /**
        /// Gets the current number of items held in the internal queue
        */
        public int Count
        {
            get
            {
                lock (_lock) {
                    if( _end) return 0;
                    return _blockingCollection.Count;
                }
            }
        }

        /**
        /// Gets the last item that was enqueued
        */
        public T LastEnqueued
        {
            get { return _lastEnqueued; }
        }

        /**
        /// Initializes a new instance of the <see cref="EnqueueableEnumerator{T}"/> class
        */
         * @param blocking">Specifies whether or not to use the blocking behavior
        public EnqueueableEnumerator( boolean blocking = false) {
            _blockingCollection = new BlockingCollection<T>();
            _timeout = blocking ? Timeout.Infinite : 0;
        }

        /**
        /// Enqueues the new data into this enumerator
        */
         * @param data">The data to be enqueued
        public void Enqueue(T data) {
            lock (_lock) {
                if( _end) return;
                _blockingCollection.Add(data);
                _lastEnqueued = data;
            }
        }

        /**
        /// Signals the enumerator to stop enumerating when the items currently
        /// held inside are gone. No more items will be added to this enumerator.
        */
        public void Stop() {
            lock (_lock) {
                if( _end) return;
                // no more items can be added, so no need to wait anymore
                _blockingCollection.CompleteAdding();
                _end = true;
            }
        }

        /**
        /// Advances the enumerator to the next element of the collection.
        */
        @returns 
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// 
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public boolean MoveNext() {
            T current;
            if( !_blockingCollection.TryTake(out current, _timeout)) {
                _current = default(T);
                return !_end;
            }

            _current = current;

            // even if we don't have data to return, we haven't technically
            // passed the end of the collection, so always return true until
            // the enumerator is explicitly disposed or ended
            return true;
        }

        /**
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        */
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public void Reset() {
            throw new NotImplementedException( "EnqueableEnumerator.Reset() has not been implemented yet.");
        }

        /**
        /// Gets the element in the collection at the current position of the enumerator.
        */
        @returns 
        /// The element in the collection at the current position of the enumerator.
        /// 
        public T Current
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
            lock (_lock) {
                if( _disposed) return;
                Stop();
                if( _blockingCollection != null ) _blockingCollection.Dispose();
                _disposed = true;
            }
        }
    }
}