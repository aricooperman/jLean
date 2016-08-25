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

package com.quantconnect.lean.Lean.Engine.DataFeeds.Enumerators
{
    /**
     * Provides an implementation of <see cref="IEnumerator{T}"/> that will
     * always return true via MoveNext.
    */
     * <typeparam name="T"></typeparam>
    public class RefreshEnumerator<T> : IEnumerator<T>
    {
        private T _current;
        private IEnumerator<T> _enumerator;
        private final Func<IEnumerator<T>> _enumeratorFactory;

        /**
         * Initializes a new instance of the <see cref="RefreshEnumerator{T}"/> class
        */
         * @param enumeratorFactory Enumerator factory used to regenerate the underlying
         * enumerator when it ends
        public RefreshEnumerator(Func<IEnumerator<T>> enumeratorFactory) {
            _enumeratorFactory = enumeratorFactory;
        }

        /**
         * Advances the enumerator to the next element of the collection.
        */
        @returns 
         * true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
         * 
         * <exception cref="T:System.InvalidOperationException The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public boolean MoveNext() {
            _enumerator = _enumeratorFactory.Invoke();

            moveNext = _enumerator.MoveNext();
            if( moveNext) {
                _current = _enumerator.Current;
            }
            else
            {
                _enumerator = null;
                _current = default(T);
            }

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
        public T Current
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
            get { return Current; }
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
