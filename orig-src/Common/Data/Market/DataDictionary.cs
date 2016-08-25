using System;
using System.Collections;
using System.Collections.Generic;

package com.quantconnect.lean.Data.Market
{
    /**
     * Provides a base class for types holding base data instances keyed by symbol
    */
    public class DataMap<T> : Map<Symbol, T>
    {
        // storage for the data
        private final Map<Symbol, T> _data = new Map<Symbol, T>();

       /**
        * Initializes a new instance of the <see cref="QuantConnect.Data.Market.DataDictionary{T}"/> class.
       */
        public DataDictionary() {
        }

        /**
         * Initializes a new instance of the <see cref="QuantConnect.Data.Market.DataDictionary{T}"/> class
         * using the specified <paramref name="data"/> as a data source
        */
         * @param data The data source for this data Map
         * @param keySelector Delegate used to select a key from the value
        public DataDictionary(IEnumerable<T> data, Func<T, Symbol> keySelector) {
            foreach (datum in data) {
                this[keySelector(datum)] = datum;
            }
        }

        /**
         * Initializes a new instance of the <see cref="QuantConnect.Data.Market.DataDictionary{T}"/> class.
        */
         * @param time The time this data was emitted.
        public DataDictionary(DateTime time) {
#pragma warning disable 618 // This assignment is left here until the Time property is removed.
            Time = time;
#pragma warning restore 618
        }

        /**
         * Gets or sets the time associated with this collection of data
        */
        [Obsolete( "The DataMap<T> Time property is now obsolete. All algorithms should use algorithm.Time instead.")]
        public DateTime Time { get; set; }

        /**
         * Returns an enumerator that iterates through the collection.
        */
        @returns 
         * A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
         * 
         * <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<Symbol, T>> GetEnumerator() {
            return _data.GetEnumerator();
        }

        /**
         * Returns an enumerator that iterates through a collection.
        */
        @returns 
         * An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
         * 
         * <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable) _data).GetEnumerator();
        }

        /**
         * Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        */
         * @param item The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.<exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(KeyValuePair<Symbol, T> item) {
            _data.Add(item);
        }

        /**
         * Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        */
         * <exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear() {
            _data.Clear();
        }

        /**
         * Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        */
        @returns 
         * true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
         * 
         * @param item The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        public boolean Contains(KeyValuePair<Symbol, T> item) {
            return _data.Contains(item);
        }

        /**
         * Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        */
         * @param array The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.<param name="arrayIndex The zero-based index in <paramref name="array"/> at which copying begins.<exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(KeyValuePair<Symbol, T>[] array, int arrayIndex) {
            _data.CopyTo(array, arrayIndex);
        }

        /**
         * Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        */
        @returns 
         * true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
         * 
         * @param item The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.<exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public boolean Remove(KeyValuePair<Symbol, T> item) {
            return _data.Remove(item);
        }

        /**
         * Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        */
        @returns 
         * The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
         * 
        public int Count
        {
            get { return _data.Count; }
        }

        /**
         * Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        */
        @returns 
         * true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
         * 
        public boolean IsReadOnly
        {
            get { return _data.IsReadOnly; }
        }

        /**
         * Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        */
        @returns 
         * true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
         * 
         * @param key The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.<exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public boolean ContainsKey(Symbol key) {
            return _data.ContainsKey(key);
        }

        /**
         * Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        */
         * @param key The object to use as the key of the element to add.<param name="value The object to use as the value of the element to add.<exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.ArgumentException An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(Symbol key, T value) {
            _data.Add(key, value);
        }

        /**
         * Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        */
        @returns 
         * true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
         * 
         * @param key The key of the element to remove.<exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public boolean Remove(Symbol key) {
            return _data.Remove(key);
        }

        /**
         * Gets the value associated with the specified key.
        */
        @returns 
         * true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
         * 
         * @param key The key whose value to get.<param name="value When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.<exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public boolean TryGetValue(Symbol key, out T value) {
            return _data.TryGetValue(key, out value);
        }

        /**
         * Gets or sets the element with the specified key.
        */
        @returns 
         * The element with the specified key.
         * 
         * @param symbol The key of the element to get or set.
         * <exception cref="T:System.ArgumentNullException"><paramref name="symbol"/> is null.</exception>
         * <exception cref="T:System.Collections.Generic.KeyNotFoundException The property is retrieved and <paramref name="symbol"/> is not found.</exception>
         * <exception cref="T:System.NotSupportedException The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public T this[Symbol symbol]
        {
            get
            {
                T data;
                if( TryGetValue(symbol, out data)) {
                    return data;
                }
                throw new KeyNotFoundException( String.format( "'%1$s' wasn't found in the %2$s object, likely because there was no-data at this moment in time and it wasn't possible to fillforward historical data. Please check the data exists before accessing it with data.ContainsKey(\"%1$s\")", symbol, GetType().GetBetterTypeName()));
            }
            set
            {
                _data[symbol] = value;
            }
        }

        /**
         * Gets or sets the element with the specified key.
        */
        @returns 
         * The element with the specified key.
         * 
         * @param ticker The key of the element to get or set.
         * <exception cref="T:System.ArgumentNullException"><paramref name="ticker"/> is null.</exception>
         * <exception cref="T:System.Collections.Generic.KeyNotFoundException The property is retrieved and <paramref name="ticker"/> is not found.</exception>
         * <exception cref="T:System.NotSupportedException The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public T this[string ticker]
        {
            get
            {
                Symbol symbol;
                if( !SymbolCache.TryGetSymbol(ticker, out symbol)) {
                    throw new KeyNotFoundException( String.format( "'%1$s' wasn't found in the %2$s object, likely because there was no-data at this moment in time and it wasn't possible to fillforward historical data. Please check the data exists before accessing it with data.ContainsKey(\"%1$s\")", ticker, GetType().GetBetterTypeName()));
                }
                return this[symbol];
            }
            set
            {
                Symbol symbol;
                if( !SymbolCache.TryGetSymbol(ticker, out symbol)) {
                    throw new KeyNotFoundException( String.format( "'%1$s' wasn't found in the %2$s object, likely because there was no-data at this moment in time and it wasn't possible to fillforward historical data. Please check the data exists before accessing it with data.ContainsKey(\"%1$s\")", ticker, GetType().GetBetterTypeName()));
                }
                this[symbol] = value;
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
            get { return _data.Keys; }
        }

        /**
         * Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        */
        @returns 
         * An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
         * 
        public ICollection<T> Values
        {
            get { return _data.Values; }
        }
    }

    /**
     * Provides extension methods for the DataDictionary class
    */
    public static class DataDictionaryExtensions
    {
        /**
         * Provides a convenience method for adding a base data instance to our data dictionary
        */
        public static void Add<T>(this DataMap<T> dictionary, T data)
            where T : BaseData
        {
            dictionary.Add(data.Symbol, data);
        }
    }
}