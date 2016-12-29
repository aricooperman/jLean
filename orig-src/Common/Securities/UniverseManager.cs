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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using QuantConnect.Data.UniverseSelection;

package com.quantconnect.lean.Securities
{
    /**
     * Manages the algorithm's collection of universes
    */
    public class UniverseManager : Map<Symbol, Universe>, INotifyCollectionChanged
    {
        private final ConcurrentMap<Symbol, Universe> _universes;

        /**
         * Event fired when a universe is added or removed
        */
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /**
         * Initializes a new instance of the <see cref="UniverseManager"/> class
        */
        public UniverseManager() {
            _universes = new ConcurrentMap<Symbol, Universe>();
        }

        #region IDictionary implementation

        /**
         * Returns an enumerator that iterates through the collection.
        */
        @returns 
         * A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
         * 
         * <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<Symbol, Universe>> GetEnumerator() {
            return _universes.GetEnumerator();
        }

        /**
         * Returns an enumerator that iterates through a collection.
        */
        @returns 
         * An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
         * 
         * <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_universes).GetEnumerator();
        }

        /**
         * Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        */
         * @param item The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.<exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(KeyValuePair<Symbol, Universe> item) {
            Add(item.Key, item.Value);
        }

        /**
         * Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        */
         * <exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear() {
            _universes.Clear();
        }

        /**
         * Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        */
        @returns 
         * true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
         * 
         * @param item The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        public boolean Contains(KeyValuePair<Symbol, Universe> item) {
            return _universes.Contains(item);
        }

        /**
         * Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        */
         * @param array The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.<param name="arrayIndex The zero-based index in <paramref name="array"/> at which copying begins.<exception cref="T:System.NullPointerException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(KeyValuePair<Symbol, Universe>[] array, int arrayIndex) {
            ((Map<Symbol, Universe>)_universes).CopyTo(array, arrayIndex);
        }

        /**
         * Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        */
        @returns 
         * true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
         * 
         * @param item The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.<exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public boolean Remove(KeyValuePair<Symbol, Universe> item) {
            Universe universe;
            return _universes.TryRemove(item.Key, out universe);
        }

        /**
         * Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        */
        @returns 
         * The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
         * 
        public int Count
        {
            get { return _universes.Count; }
        }

        /**
         * Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        */
        @returns 
         * true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
         * 
        public boolean IsReadOnly
        {
            get { return false; }
        }

        /**
         * Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        */
        @returns 
         * true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
         * 
         * @param key The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.<exception cref="T:System.NullPointerException"><paramref name="key"/> is null.</exception>
        public boolean ContainsKey(Symbol key) {
            return _universes.ContainsKey(key);
        }

        /**
         * Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        */
         * @param key The object to use as the key of the element to add.<param name="universe The object to use as the value of the element to add.<exception cref="T:System.NullPointerException"><paramref name="key"/> is null.</exception><exception cref="T:System.ArgumentException An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(Symbol key, Universe universe) {
            if( _universes.TryAdd(key, universe)) {
                userDefinedUniverse = universe as UserDefinedUniverse;
                if( userDefinedUniverse != null ) {
                    // wire up user defined universes to trigger
                    userDefinedUniverse.CollectionChanged += (sender, args) -> 
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, universe));
                }

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, universe));
            }
        }

        /**
         * Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        */
        @returns 
         * true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
         * 
         * @param key The key of the element to remove.<exception cref="T:System.NullPointerException"><paramref name="key"/> is null.</exception><exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public boolean Remove(Symbol key) {
            Universe universe;
            if( _universes.TryRemove(key, out universe)) {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, universe));
                return true;
            }
            return false;
        }

        /**
         * Gets the value associated with the specified key.
        */
        @returns 
         * true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
         * 
         * @param key The key whose value to get.<param name="value When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.<exception cref="T:System.NullPointerException"><paramref name="key"/> is null.</exception>
        public boolean TryGetValue(Symbol key, out Universe value) {
            return _universes.TryGetValue(key, out value);
        }

        /**
         * Gets or sets the element with the specified key.
        */
        @returns 
         * The element with the specified key.
         * 
         * @param symbol The key of the element to get or set.<exception cref="T:System.NullPointerException"><paramref name="symbol"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException The property is retrieved and <paramref name="symbol"/> is not found.</exception><exception cref="T:System.NotSupportedException The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public Universe this[Symbol symbol]
        {
            get
            {
                if( !_universes.ContainsKey(symbol)) {
                    throw new Exception( String.format( "This universe symbol (%1$s) was not found in your universe list. Please add this security or check it exists before using it with 'Universes.ContainsKey(\"%2$s\")'", symbol, SymbolCache.GetTicker(symbol)));
                }
                return _universes[symbol];
            }
            set
            {
                Universe existing;
                if( _universes.TryGetValue(symbol, out existing) && existing != value) {
                    throw new IllegalArgumentException( "Unable to over write existing Universe: " + symbol.toString());
                }

                // no security exists for the specified symbol key, add it now
                if( existing == null ) {
                    Add(symbol, value);
                }
            }
        }

        /**
         * Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        */
        @returns 
         * An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
         * 
        public ICollection<Symbol> Keys
        {
            get { return _universes.Keys; }
        }

        /**
         * Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        */
        @returns 
         * An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
         * 
        public ICollection<Universe> Values
        {
            get { return _universes.Values; }
        }

        #endregion

        /**
         * Event invocator for the <see cref="CollectionChanged"/> event
        */
         * @param e">
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            handler = CollectionChanged;
            if( handler != null ) handler(this, e);
        }
    }
}