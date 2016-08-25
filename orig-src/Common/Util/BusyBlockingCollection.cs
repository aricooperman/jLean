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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

package com.quantconnect.lean.Util
{
    /**
     * A small wrapper around <see cref="BlockingCollection{T}"/> used to communicate busy state of the items
     * being processed
    */
     * <typeparam name="T The item type being processed</typeparam>
    public class BusyBlockingCollection<T> : IDisposable
    {
        private final BlockingCollection<T> _collection;
        private final ManualResetEventSlim _processingCompletedEvent;
        private final object _synchronized= new object();

        /**
         * Gets a wait handle that can be used to wait until this instance is done
         * processing all of it's item
        */
        public WaitHandle WaitHandle
        {
            get { return _processingCompletedEvent.WaitHandle; }
        }

        /**
         * Gets the number of items held within this collection
        */
        public int Count
        {
            get { return _collection.Count; }
        }

        /**
         * Returns true if processing, false otherwise
        */
        public boolean IsBusy
        {
            get
            {
                synchronized(_lock) {
                    return _collection.Count > 0 || !_processingCompletedEvent.IsSet;
                }
            }
        }

        /**
         * Initializes a new instance of the <see cref="BusyBlockingCollection{T}"/> class
         * with a bounded capacity of <see cref="int.MaxValue"/>
        */
        public BusyBlockingCollection()
            : this(int.MaxValue) {
        }

        /**
         * Initializes a new instance of the <see cref="BusyBlockingCollection{T}"/> class
         * with the specified <paramref name="boundedCapacity"/>
        */
         * @param boundedCapacity The maximum number of items allowed in the collection
        public BusyBlockingCollection(int boundedCapacity) {
            _collection = new BlockingCollection<T>(boundedCapacity);

            // initialize as not busy
            _processingCompletedEvent = new ManualResetEventSlim(true);
        }

        /**
         * Adds the items to this collection
        */
         * @param item The item to be added
        public void Add(T item) {
            Add(item, CancellationToken.None);
        }

        /**
         * Adds the items to this collection
        */
         * @param item The item to be added
         * @param cancellationToken A cancellation token to observer
        public void Add(T item, CancellationToken cancellationToken) {
            boolean added;
            synchronized(_lock) {
                // we're adding work to be done, mark us as busy
                _processingCompletedEvent.Reset();
                added = _collection.TryAdd(item, 0, cancellationToken);
            }

            if( !added) {
                _collection.Add(item, cancellationToken);
            }
        }

        /**
         * Marks the <see cref="BusyBlockingCollection{T}"/> as not accepting any more additions
        */
        public void CompleteAdding() {
            _collection.CompleteAdding();
        }

        /**
         * Provides a consuming enumerable for items in this collection.
        */
        @returns An enumerable that removes and returns items from the collection
        public IEnumerable<T> GetConsumingEnumerable() {
            return GetConsumingEnumerable(CancellationToken.None);
        }

        /**
         * Provides a consuming enumerable for items in this collection.
        */
         * @param cancellationToken A cancellation token to observer
        @returns An enumerable that removes and returns items from the collection
        public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken) {
            while (!_collection.IsCompleted) {
                T item;

                // check to see if something is immediately available
                boolean tookItem;

                try
                {
                    tookItem = _collection.TryTake(out item, 0, cancellationToken);
                }
                catch (OperationCanceledException) {
                    // if the operation was canceled, just bail on the enumeration
                    yield break;
                }

                if( tookItem) {
                    // something was immediately available, emit it
                    yield return item;
                    continue;
                }


                // we need to synchronizedthis with the Add method since we need to model the act of
                // taking/flipping the switch and adding/flipping the switch as one operation
                synchronized(_lock) {
                    // double check that there's nothing in the collection within a lock, it's possible
                    // that between the TryTake above and this statement, the Add method was called, so we
                    // don't want to flip the switch if there's something in the collection
                    if( _collection.Count == 0) {
                        // nothing was immediately available, mark us as idle
                        _processingCompletedEvent.Set();
                    }
                }

                try
                {
                    // now bsynchronizeduntil something is available
                    tookItem = _collection.TryTake(out item, Timeout.Infinite, cancellationToken);
                }
                catch (OperationCanceledException) {
                    // if the operation was canceled, just bail on the enumeration
                    yield break;
                }

                if( tookItem) {
                    // emit the item we found
                    yield return item;
                }
            }

            // no more items to process
            _processingCompletedEvent.Set();
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
         * <filterpriority>2</filterpriority>
        public void Dispose() {
            _collection.Dispose();
            _processingCompletedEvent.Dispose();
        }
    }
}
