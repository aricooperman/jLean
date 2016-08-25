﻿/*
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
using System.Threading;

package com.quantconnect.lean.Indicators
{
    /**
     *     This is a window that allows for list access semantics,
     *     where this[0] refers to the most recent item in the
     *     window and this[Count-1] refers to the last item in the window
    */
     * <typeparam name="T The type of data in the window</typeparam>
    public class RollingWindow<T> : IReadOnlyWindow<T>
    {
        // the backing list object used to hold the data
        private final List<T> _list;
        // read-write synchronizedused for controlling access to the underlying list data structure
        private final ReaderWriterLockSlim _listLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        // the most recently removed item from the window (fell off the back)
        private T _mostRecentlyRemoved;
        // the total number of samples taken by this indicator
        private BigDecimal _samples;
        // used to locate the last item in the window as an indexer into the _list
        private int _tail;

        /**
         *     Initializes a new instance of the RollwingWindow class with the specified window size.
        */
         * @param size The number of items to hold in the window
        public RollingWindow(int size) {
            if( size < 1) {
                throw new IllegalArgumentException( "RollingWindow must have size of at least 1.", "size");
            }
            _list = new List<T>(size);
        }

        /**
         *     Gets the size of this window
        */
        public int Size
        {
            get
            { 
                try
                {
                    _listLock.EnterReadLock();
                    return _list.Capacity;
                }
                finally
                {
                    _listLock.ExitReadLock();
                }
            }
        }

        /**
         *     Gets the current number of elements in this window
        */
        public int Count
        {
            get
            { 
                try
                {
                    _listLock.EnterReadLock();
                    return _list.Count;
                }
                finally
                {
                    _listLock.ExitReadLock();
                }
            }
        }

        /**
         *     Gets the number of samples that have been added to this window over its lifetime
        */
        public BigDecimal Samples
        {
            get
            { 
                try
                {
                    _listLock.EnterReadLock();
                    return _samples;
                }
                finally
                {
                    _listLock.ExitReadLock();
                }
            }
        }

        /**
         *     Gets the most recently removed item from the window. This is the
         *     piece of data that just 'fell off' as a result of the most recent
         *     add. If no items have been removed, this will throw an exception.
        */
        public T MostRecentlyRemoved
        {
            get
            {
                try
                {
                    _listLock.EnterReadLock();

                    if( !IsReady) {
                        throw new InvalidOperationException( "No items have been removed yet!");
                    }
                    return _mostRecentlyRemoved;
                }
                finally
                {
                    _listLock.ExitReadLock();
                }

            }
        }

        /**
         *     Indexes into this window, where index 0 is the most recently
         *     entered value
        */
         * @param i the index, i
        @returns the ith most recent entry
        public T this [int i]
        {
            get
            {
                try
                {
                    _listLock.EnterReadLock();

                    if( i >= Count) {
                        throw new ArgumentOutOfRangeException( "i", i, String.format( "Must be between 0 and Count %1$s", Count));
                    }
                    return _list[(Count + _tail - i - 1) % Count];
                }
                finally
                {
                    _listLock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _listLock.EnterWriteLock();

                    if( i >= Count) {
                        throw new ArgumentOutOfRangeException( "i", i, String.format( "Must be between 0 and Count %1$s", Count));
                    }
                    _list[(Count + _tail - i - 1) % Count] = value;
                }
                finally
                {
                    _listLock.ExitWriteLock();
                }
            }
        }

        /**
         *     Gets a value indicating whether or not this window is ready, i.e,
         *     it has been filled to its capacity and one has fallen off the back
        */
        public boolean IsReady
        {
            get
            { 
                try
                {
                    _listLock.EnterReadLock();
                    return Samples > Size;
                }
                finally
                {
                    _listLock.ExitReadLock();
                } 
            }
        }

        /**
         *     Returns an enumerator that iterates through the collection.
        */
        @returns 
         *     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
         * 
         * <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator() {
            // we make a copy on purpose so the enumerator isn't tied 
            // to a mutable object, well it is still mutable but out of scope
            temp = new List<T>(Count);
            try
            {
                _listLock.EnterReadLock();

                for (int i = 0; i < Count; i++) {
                    temp.Add(this[i]);
                }
                return temp.GetEnumerator();
            }
            finally
            {
                _listLock.ExitReadLock();
            }

        }

        /**
         *     Returns an enumerator that iterates through a collection.
        */
        @returns 
         *     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
         * 
         * <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /**
         *     Adds an item to this window and shifts all other elements
        */
         * @param item The item to be added
        public void Add(T item) {
            try
            {
                _listLock.EnterWriteLock();

                _samples++;
                if( Size == Count) {
                    // keep track of what's the last element
                    // so we can reindex on this[ int ]
                    _mostRecentlyRemoved = _list[_tail];
                    _list[_tail] = item;
                    _tail = (_tail + 1) % Size;
                }
                else
                {
                    _list.Add(item);
                }
            }
            finally
            {
                _listLock.ExitWriteLock();
            }
        }

        /**
         *     Clears this window of all data
        */
        public void Reset() {
            try
            {
                _listLock.EnterWriteLock();

                _samples = 0;
                _list.Clear();
                _tail = 0;
            }
            finally
            {
                _listLock.ExitWriteLock();
            }
        }
    }
}