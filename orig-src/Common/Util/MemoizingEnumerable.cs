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

package com.quantconnect.lean.Util
{
    /**
    /// Defines an enumerable that can be enumerated many times while
    /// only performing a single enumeration of the root enumerable
    */
    /// <typeparam name="T"></typeparam>
    public class MemoizingEnumerable<T> : IEnumerable<T>
    {
        private boolean _finished;

        private final List<T> _buffer;
        private final IEnumerator<T> _enumerator;

        private final object _lock = new object();

        /**
        /// Initializes a new instance of the <see cref="MemoizingEnumerable{T}"/> class
        */
         * @param enumerable">The source enumerable to be memoized
        public MemoizingEnumerable(IEnumerable<T> enumerable)
            : this(enumerable.GetEnumerator()) {
        }

        /**
        /// Initializes a new instance of the <see cref="MemoizingEnumerable{T}"/> class
        */
         * @param enumerator">The source enumerator to be memoized
        public MemoizingEnumerable(IEnumerator<T> enumerator) {
            _buffer = new List<T>();
            _enumerator = enumerator;
        }

        /**
        /// Returns an enumerator that iterates through the collection.
        */
        @returns 
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// 
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator() {
            int i = 0;
            while (true) {
                boolean hasValue;
                
                // sync for multiple threads access to _enumerator and _buffer
                lock (_lock) {
                    // check to see if we need to move next
                    if( !_finished && i >= _buffer.Count) {
                        hasValue = _enumerator.MoveNext();
                        if( hasValue) {
                            _buffer.Add(_enumerator.Current);
                        }
                        else
                        {
                            _finished = true;
                        }
                    }
                    else
                    {
                        // we have a value if it's in the buffer
                        hasValue = _buffer.Count > i;
                    }
                }

                // yield the i'th element if we have it, otherwise stop enumeration
                if( hasValue) {
                    yield return _buffer[i];
                }
                else
                {
                    yield break;
                }

                // increment for next time
                i++;
            }
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
