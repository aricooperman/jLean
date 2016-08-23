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

using System.Collections;
using System.Collections.Generic;

package com.quantconnect.lean.Util
{
    /**
    /// Provides an implementation of an add-only fixed length, unique queue system
    */
    public class FixedSizeHashQueue<T> : IEnumerable<T>
    {
        private final int _size;
        private final Queue<T> _queue; 
        private final HashSet<T> _hash; 

        /**
        /// Initializes a new instance of the <see cref="FixedSizeHashQueue{T}"/> class
        */
         * @param size">The maximum number of items to hold
        public FixedSizeHashQueue(int size) {
            _size = size;
            _queue = new Queue<T>(size);
            _hash = new HashSet<T>();
        }

        /**
        /// Returns true if the item was added and didn't already exists
        */
        public boolean Add(T item) {
            if( _hash.Add(item)) {
                _queue.Enqueue(item);
                if( _queue.Count > _size) {
                    // remove the item from both
                    _hash.Remove(_queue.Dequeue());
                }
                return true;
            }
            return false;
        }

        /**
        /// Tries to inspect the first item in the queue
        */
        public boolean TryPeek(out T item) {
            if( _queue.Count > 0) {
                item = _queue.Peek();
                return true;
            }
            item = default(T);
            return false;
        }

        /**
        /// Dequeues and returns the next item in the queue
        */
        public T Dequeue() {
            item = _queue.Dequeue();
            _hash.Remove(item);
            return item;
        }

        /**
        /// Returns true if the specified item exists in the collection
        */
        public boolean Contains(T item) {
            return _hash.Contains(item);
        }

        /**
        /// Returns an enumerator that iterates through the collection.
        */
        @returns 
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// 
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator() {
            return _queue.GetEnumerator();
        }

        /**
        /// Returns an enumerator that iterates through a collection.
        */
        @returns 
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// 
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}