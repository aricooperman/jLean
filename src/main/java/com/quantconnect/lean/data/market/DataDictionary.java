package com.quantconnect.lean.data.market;

import java.util.Collection;
import java.util.HashMap;
import java.util.Map;
import java.util.NoSuchElementException;
import java.util.Set;
import java.util.function.Function;

import com.quantconnect.lean.Extensions;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.SymbolCache;
import com.quantconnect.lean.data.BaseData;

/**
 * Provides a base class for types holding base data instances keyed by symbol
 */
public class DataDictionary<T> implements Map<Symbol,T> {
    
    // storage for the data
    private final Map<Symbol,T> data = new HashMap<>();

    
    public DataDictionary() {
        super();
    }

    /**
     * Initializes a new instance of the <see cref="QuantConnect.Data.Market.DataDictionary{T}"/> class
     * using the specified <paramref name="data"/> as a data source
     * @param data The data source for this data Map
     * @param keySelector Delegate used to select a key from the value
     */
    public DataDictionary( final Iterable<T> data, final Function<T,Symbol> keySelector ) {
        for( final T datum : data )
            put( keySelector.apply( datum ), datum );
    }

    /**
     * Returns an enumerator that iterates through the collection.
     * @returns A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
     */
    @Override
    public Set<Entry<Symbol,T>> entrySet() {
        return data.entrySet();
    }

//    /**
//     * Returns an enumerator that iterates through a collection.
//     * @returns An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
//     */
//    IEnumerator IEnumerable.GetEnumerator() {
//        return ((IEnumerable) _data).GetEnumerator();
//    }

    /**
     * Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
     * <exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
     */
    @Override
    public void clear() {
        data.clear();
    }

    /**
     * Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
     * @returns The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
     */
    @Override
    public int size() {
        return data.size();
    }

    /**
     * Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
     * @param key The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
     * @returns true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
     */
    @Override
    public boolean containsKey( final Object key ) {
        return data.containsKey( key );
    }

    /**
     * Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
     * @param key The object to use as the key of the element to add.<param name="value The object to use as the value of the element to add.
     * <exception cref="T:System.NullPointerException"><paramref name="key"/> is null.</exception>
     * <exception cref="T:System.ArgumentException An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
     * </exception><exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
     * @return
     */
    @Override
    public T put( final Symbol key, final T data ) {
        return this.data.put( key, data );
    }

    /**
     * Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
     * @param key The key of the element to remove.
     * <exception cref="T:System.NullPointerException"><paramref name="key"/> is null.</exception>
     * <exception cref="T:System.NotSupportedException The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
     * @returns true if the element is successfully removed; otherwise, false.  This method also returns false if
     * <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
     * 
     */
    @Override
    public T remove( final Object key ) {
        return data.remove( key );
    }

    /**
     * Gets the value associated with the specified key.
     * @returns true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
     * @param key The key whose value to get.<param name="value When this method returns, the value associated with the specified key,
     * if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.
     * <exception cref="T:System.NullPointerException"><paramref name="key"/> is null.</exception>
     */
    @Override
    public T get( final Object key ) {
        return data.get( key );
    }

    /**
     * Gets or sets the element with the specified key.
     * @param ticker The key of the element to get or set.
     * @returns The element with the specified key.
     * <exception cref="T:System.NullPointerException"><paramref name="ticker"/> is null.</exception>
     * <exception cref="T:System.Collections.Generic.KeyNotFoundException The property is retrieved and <paramref name="ticker"/> is not found.</exception>
     * <exception cref="T:System.NotSupportedException The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
     */
    public T get( final String ticker ) {
        final Symbol symbol = SymbolCache.getSymbol( ticker );
        if( symbol == null )
            throw new NoSuchElementException( String.format( "'%1$s' wasn't found in the %2$s object, likely because there was no-data at this moment in time and it wasn't possible to fillforward historical data. " +
                    "Please check the data exists before accessing it with data.ContainsKey(\"%1$s\")", ticker, Extensions.getBetterTypeName( getClass() ) ) );
        
        return get( symbol );
    }
     
    
    public void put( final String ticker, final T value ) {
        final Symbol symbol = SymbolCache.getSymbol( ticker );
        if( symbol == null )
            throw new NoSuchElementException( String.format( "'%1$s' wasn't found in the %2$s object, likely because there was no-data at this moment in time and it wasn't possible to fillforward historical data. " +
                    "Please check the data exists before accessing it with data.ContainsKey(\"%1$s\")", ticker, Extensions.getBetterTypeName( getClass() ) ) );
        
        put( symbol,  value );
    }

    /**
     * Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
     * @returns An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
     */
    @Override
    public Set<Symbol> keySet() {
        return data.keySet();
    }

    /**
     * Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
     * @returns An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
     */
    @Override
    public Collection<T> values() {
        return data.values();
    }
     
    /**
     * Provides a convenience method for adding a base data instance to our data dictionary
     */
    @SuppressWarnings("unchecked")
    public void add( final BaseData data ) {
        put( data.getSymbol(), (T)data );
    }

    @Override
    public boolean isEmpty() {
        return data.isEmpty();
    }

    @Override
    public boolean containsValue( final Object value ) {
        return data.containsValue( value );
    }


    @Override
    public void putAll( final Map<? extends Symbol,? extends T> m ) {
        data.putAll( m );
    }
}