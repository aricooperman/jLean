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
using System.Collections.Generic;

package com.quantconnect.lean.Util
{
    /**
     * A never ending queue that will dequeue and reenqueue the same item
    */
    public class CircularQueue<T>
    {
        private final T _head;
        private final Queue<T> _queue;

        /**
         * Fired when we do a full circle
        */
        public event EventHandler CircleCompleted;

        /**
         * Initializes a new instance of the <see cref="CircularQueue{T}"/> class
        */
         * @param items The items in the queue
        public CircularQueue(params T[] items)
            : this((IEnumerable<T>)items) {
        }

        /**
         * Initializes a new instance of the <see cref="CircularQueue{T}"/> class
        */
         * @param items The items in the queue
        public CircularQueue(IEnumerable<T> items) {
            _queue = new Queue<T>();

            first = true;
            foreach (item in items) {
                if( first) {
                    first = false;
                    _head = item;
                }
                _queue.Enqueue(item);
            }
        }

        /**
         * Dequeues the next item
        */
        @returns The next item
        public T Dequeue() {
            item = _queue.Dequeue();
            if( item.Equals(_head)) {
                OnCircleCompleted();
            }
            _queue.Enqueue(item);
            return item;
        }

        /**
         * Event invocator for the <see cref="CircleCompleted"/> evet
        */
        protected void OnCircleCompleted() {
            handler = CircleCompleted;
            if( handler != null ) handler(this, EventArgs.Empty);
        }
    }
}