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

package com.quantconnect.lean.data;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.function.Function;
import java.util.stream.Collectors;
import java.util.stream.Stream;

import com.google.common.collect.ImmutableList;
import com.google.common.collect.ImmutableSet;
import com.google.common.collect.Iterables;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.market.DataDictionary;
import com.quantconnect.lean.data.market.Delistings;
import com.quantconnect.lean.data.market.Dividends;
import com.quantconnect.lean.data.market.OptionChains;
import com.quantconnect.lean.data.market.QuoteBars;
import com.quantconnect.lean.data.market.Splits;
import com.quantconnect.lean.data.market.SymbolChangedEvents;
import com.quantconnect.lean.data.market.Tick;
import com.quantconnect.lean.data.market.Ticks;
import com.quantconnect.lean.data.market.TradeBar;
import com.quantconnect.lean.data.market.TradeBars;
import com.quantconnect.lean.util.ExpressionBuilder;

import javaslang.Lazy;

/**
 * Provides a data structure for all of an algorithm's data at a single time step
 */
public class Slice implements Iterable<Entry<Symbol,BaseData>> {

    private final Ticks _ticks;
    private final TradeBars _bars;
    private final QuoteBars _quoteBars;
    private final OptionChains _optionChains;

    // aux data
    private final Splits _splits;
    private final Dividends _dividends;
    private final Delistings _delistings;
    private final SymbolChangedEvents _symbolChangedEvents;

    // String -> data   for non-tick data
    // String -> list{data} for tick data
    private final Lazy<DataDictionary<SymbolData>> _data;
    // Quandl -> DataDictonary<Quandl>
    private final Map<Class<?>,Lazy<Object>> _dataByType;

    private final LocalDateTime Time;
    private final boolean hasData;

    /**
     * Gets the timestamp for this slice of data
     */
    public LocalDateTime getTime() {
        return Time;
    }

    /**
     * Gets whether or not this slice has data
     */
    public boolean hasData() {
        return hasData;
    }

    /**
     * Gets the <see cref="TradeBars"/> for this slice of data
     */
    public TradeBars getBars() {
        return _bars;
    }

    /**
     * Gets the <see cref="QuoteBars"/> for this slice of data
     */
    public QuoteBars getQuoteBars() {
        return _quoteBars;
    }

    /**
     * Gets the <see cref="Ticks"/> for this slice of data
     */
    public Ticks getTicks() {
        return _ticks;
    }

    /**
     * Gets the <see cref="OptionChains"/> for this slice of data
     */
    public OptionChains getOptionChains() {
        return _optionChains;
    }

    /**
     * Gets the <see cref="Splits"/> for this slice of data
     */
    public Splits getSplits() {
        return _splits;
    }

    /**
     * Gets the <see cref="Dividends"/> for this slice of data
     */
    public Dividends getDividends() {
        return _dividends;
    }

    /**
     * Gets the <see cref="Delistings"/> for this slice of data
     */
    public Delistings getDelistings() {
        return _delistings;
    }

    /**
     * Gets the <see cref="QuantConnect.Data.Market.SymbolChangedEvents"/> for this slice of data
     */
    public SymbolChangedEvents getSymbolChangedEvents() {
        return _symbolChangedEvents;
    }

    /**
     * Gets the number of symbols held in this slice
     */
    public int getCount() {
        return _data.get().size();
    }

    /**
     * Gets all the symbols in this slice
     */
    public ImmutableSet<Symbol> keySet() {
        return ImmutableSet.copyOf( _data.get().keySet() );
    }

    /**
     * Gets a list of all the data in this slice
     */
    public ImmutableList<BaseData> values() {
        return ImmutableList.copyOf( Iterables.transform( getKeyValuePairIterable(), x -> x.getValue() ) );
    }

    /**
     * Initializes a new instance of the <see cref="Slice"/> class, lazily
     * instantiating the <see cref="Slice.Bars"/> and <see cref="Slice.Ticks"/>
     * collections on demand
     * @param time The timestamp for this slice of data
     * @param data The raw data in this slice
     */
    public Slice( final LocalDateTime time, final Iterable<BaseData> data ) {
        this( time, data, null, null, null, null, null, null, null, null );
    }

    /**
     * Initializes a new instance of the <see cref="Slice"/> class
     * @param time The timestamp for this slice of data
     * @param data The raw data in this slice
     * @param tradeBars The trade bars for this slice
     * @param quoteBars The quote bars for this slice
     * @param ticks This ticks for this slice
     * @param optionChains The option chains for this slice
     * @param splits The splits for this slice
     * @param dividends The dividends for this slice
     * @param delistings The delistings for this slice
     * @param symbolChanges The symbol changed events for this slice
     */
    public Slice( final LocalDateTime time, final Iterable<BaseData> data, final TradeBars tradeBars, final QuoteBars quoteBars, final Ticks ticks,
            final OptionChains optionChains, final Splits splits, final Dividends dividends, final Delistings delistings, final SymbolChangedEvents symbolChanges ) {
        this( time, data, tradeBars, quoteBars, ticks, optionChains, splits, dividends, delistings, symbolChanges, null );
    }

    /**
     * Initializes a new instance of the <see cref="Slice"/> class
     * @param time The timestamp for this slice of data
     * @param data The raw data in this slice
     * @param tradeBars The trade bars for this slice
     * @param quoteBars The quote bars for this slice
     * @param ticks This ticks for this slice
     * @param optionChains The option chains for this slice
     * @param splits The splits for this slice
     * @param dividends The dividends for this slice
     * @param delistings The delistings for this slice
     * @param symbolChanges The symbol changed events for this slice
     * @param hasData true if this slice contains data
     */
    public Slice( final LocalDateTime time, final Iterable<BaseData> data, final TradeBars tradeBars, final QuoteBars quoteBars, final Ticks ticks,
            final OptionChains optionChains, final Splits splits, final Dividends dividends, final Delistings delistings, final SymbolChangedEvents symbolChanges, final Boolean hasData ) {
        Time = time;

        _dataByType = new HashMap<>();

        // market data
        _data = Lazy.of( () -> createDynamicDataDictionary( data ) );

        this.hasData = hasData != null ? hasData : _data.get().size() > 0;

        _ticks = createTicksCollection( ticks );
        _bars = createCollection( tradeBars, TradeBars.class );
        _quoteBars = createCollection( quoteBars, QuoteBars.class );
        _optionChains = createCollection( optionChains, OptionChains.class );

        // auxiliary data
        _splits = createCollection( splits, Splits.class );
        _dividends = createCollection( dividends, Dividends.class );
        _delistings = createCollection( delistings, Delistings.class );
        _symbolChangedEvents = createCollection( symbolChanges, SymbolChangedEvents.class );
    }

    /**
     * Gets the <see cref="DataDictionary{T}"/> for all data of the specified type
     * <typeparam name="T The type of data we want, for example, <see cref="TradeBar"/> or <see cref="Quandl"/>, ect...</typeparam>
     * @returns The <see cref="DataDictionary{T}"/> containing the data of the specified type
     */
    @SuppressWarnings("unchecked")
    public <T extends BaseData> DataDictionary<T> get( final Class<? extends T> type ) {
        Lazy<Object> dictionary = _dataByType.get( type );
        if( dictionary == null ) {
            if( type.equals( Tick.class ) )
                dictionary = Lazy.of( () -> new DataDictionary<>( _data.get().values().stream().flatMap( x -> ((List<T>)x.getData()).stream() ).collect( Collectors.toList() ), x -> x.getSymbol() ) );
            else
                dictionary = Lazy.of( () -> new DataDictionary<>( _data.get().values().stream().map( x -> (T)x.getData() ).collect( Collectors.toList() ), x -> x.getSymbol() ) );

            _dataByType.put( type, dictionary );
        }

        return (DataDictionary<T>)dictionary.get();
    }

    /**
     * Gets the data of the specified symbol and type.
     * <typeparam name="T The type of data we seek</typeparam>
     * @param symbol The specific symbol was seek
     * @returns The data for the requested symbol
     */
    @SuppressWarnings("unchecked")
    public <T extends BaseData> T get( final Symbol symbol ) {
        final SymbolData value = _data.get().get( symbol );
        if( value != null )
            return (T)value.getData();

        throw new IllegalStateException(
                String.format( "'%1$s' wasn't found in the Slice object, likely because there was no-data at this moment in time and it wasn't possible to fillforward historical data. " +
                        "Please check the data exists before accessing it with data.containsKey(\"%1$s\")", symbol ) );
    }

    /**
     * Determines whether this instance contains data for the specified symbol
     * @param symbol The symbol we seek data for
     * @returns True if this instance contains data for the symbol, false otherwise
     */
    public boolean containsKey( final Symbol symbol ) {
        return _data.get().containsKey( symbol );
    }

    /**
     * Produces the dynamic data dictionary from the input data
     */
    private DataDictionary<SymbolData> createDynamicDataDictionary( final Iterable<BaseData> data ) {
        final DataDictionary<SymbolData> allData = new DataDictionary<>();
        for( final BaseData datum : data) {
            final SymbolData symbolData = allData.computeIfAbsent( datum.getSymbol(), s -> new SymbolData( s ) );

            switch( datum.getDataType() ) {
                case Base:
                    symbolData.type = SubscriptionType.Custom;
                    symbolData.custom = datum;
                    break;

                case TradeBar:
                    symbolData.type = SubscriptionType.TradeBar;
                    symbolData.tradeBar = (TradeBar)datum;
                    break;

                case Tick:
                    symbolData.type = SubscriptionType.Tick;
                    symbolData.ticks.add( (Tick)datum );
                    break;

                case Auxiliary:
                    symbolData.auxilliaryData.add( datum );
                    break;

                default:
                    throw new IllegalArgumentException();
            }
        }
        return allData;
    }

    /**
     * Returns the input ticks if non-null, otherwise produces one fom the dynamic data dictionary
     */
    @SuppressWarnings("unchecked")
    private Ticks createTicksCollection( final Ticks ticks ) {
        if( ticks != null )
            return ticks;

        final Ticks newTicks = new Ticks();
        _data.get().values().stream()
            .map( x -> (List<Tick>)x.getData() )
            .filter( x -> x.size() != 0 )
            .forEach( x -> newTicks.put( x.get( 0 ).getSymbol(), x ) );

        return ticks;
    }

    /**
     * Returns the input collection if onon-null, otherwise produces one from the dynamic data dictionary
     * <typeparam name="T The data dictionary type</typeparam>
     * <typeparam name="TItem The item type of the data Map</typeparam>
     * @param collection The input collection, if non-null, returned immediately
     * @returns The data dictionary of <typeparamref name="TItem"/> containing all the data of that type in this slice
     */
    @SuppressWarnings("unchecked")
    private <T extends DataDictionary<TItem>,TItem extends BaseData> T createCollection( T collection, final Class<T> type ) {
        if( collection != null )
            return collection;
        
        try {
            collection = type.newInstance();
        }
        catch( InstantiationException | IllegalAccessException e ) {
            //TODO
            e.printStackTrace();
            return null;
        }
        
        for( final TItem item : _data.get().values().stream().map( x -> (TItem)x.getData() ).collect( Collectors.toList() ) )
            collection.put( item.getSymbol(), item );

        return collection;
    }

    /**
     * Returns an enumerator that iterates through the collection.
     * @returns A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
     */
    @Override
    public Iterator<Entry<Symbol,BaseData>> iterator() {
        return getKeyValuePairIterable().iterator();
    }

    private Iterable<Entry<Symbol, BaseData>> getKeyValuePairIterable() {
        // TODO this will not enumerate auxiliary data!
        return _data.get().entrySet().stream().collect( Collectors.toMap( Entry::getKey, kvp -> (BaseData)kvp.getValue().getData() ) ).entrySet();
    }

    private enum SubscriptionType { TradeBar, Tick, Custom };

    private class SymbolData {
        public SubscriptionType type;
//        public final Symbol symbol;

        // data
        public BaseData custom;
        public TradeBar tradeBar;
        public final List<Tick> ticks;
        public final List<BaseData> auxilliaryData;

        public SymbolData( final Symbol symbol ) {
//            this.symbol = symbol;
            ticks = new ArrayList<>();
            auxilliaryData = new ArrayList<>();
        }

        public Object getData() {
            switch( type ) {
                case TradeBar:
                    return tradeBar;
                case Tick:
                    return ticks;
                case Custom:
                    return custom;
                default:
                    throw new IllegalArgumentException();
            }
        }
    }

    
    /*
     * Provides extension methods to slice enumerables
     */

    /**
     * Selects into the slice and returns the TradeBars that have data in order
     * @param slices The enumerable of slice
     * @returns An enumerable of TradeBars
     */
    public static Stream<TradeBars> tradeBars( final Stream<Slice> slices ) {
        return slices.map( x -> x.getBars() ).filter( x -> x.size() > 0 );
    }

    /**
     * Selects into the slice and returns the Ticks that have data in order
     * @param slices The enumerable of slice
     * @returns An enumerable of Ticks
     */
    public static Stream<Ticks> ticks( final Stream<Slice> slices ) {
        return slices.map( x -> x.getTicks() ).filter( x -> x.size() > 0 );
    }

    /**
     * Gets an enumerable of TradeBar for the given symbol. This method does not verify
     * that the specified symbol points to a TradeBar
     * @param slices The enumerable of slice
     * @param symbol The symbol to retrieve
     * @returns An enumerable of TradeBar for the matching symbol, of no TradeBar found for symbol, empty enumerable is returned
     */
    public static Stream<TradeBar> get( final Stream<Slice> slices, final Symbol symbol ) {
        return tradeBars( slices ).filter( x -> x.containsKey( symbol ) ).map( x -> x.get( symbol ) );
    }

    /**
     * Gets an enumerable of T for the given symbol. This method does not verify that the specified symbol points to a T
     * @param dataDictionaries The data dictionary enumerable to access
     * @param symbol The symbol to retrieve
     * @returns An enumerable of T for the matching symbol, if no T is found for symbol, empty enumerable is returned
     */
    public static <T extends BaseData> Stream<T> getFromDataDictionaries( final Stream<DataDictionary<T>> dataDictionaries, final Symbol symbol ) {
        return dataDictionaries.filter( x -> x.containsKey( symbol ) ).map( x -> x.get( symbol ) );
    }

    /**
     * Gets an enumerable of decimals by accessing the specified field on data for the symbol
     * <typeparam name="T The data type</typeparam>
     * @param dataDictionaries An enumerable of data dictionaries
     * @param symbol The symbol to retrieve
     * @param field The field to access
     * @returns An enumerable of decimals
     */
    @SuppressWarnings("unchecked")
    public static <T> Stream<BigDecimal> get( final Stream<DataDictionary<T>> dataDictionaries, final Symbol symbol, final Class<? extends T> type, final String field ) {
        final Function<T,BigDecimal> selector;
        if( DynamicData.class.isAssignableFrom( type ) ) {
            selector = data -> {
                final DynamicData dyn = (DynamicData)data;
                return (BigDecimal)dyn.getProperty( field );
            };
        }
        else if( List.class.isAssignableFrom( type ) ) {
            // perform the selection on the last tick
            // NOTE: This is a known bug, should be updated to perform the selection on each item in the list
            final Function<Tick,BigDecimal> dataSelector = ExpressionBuilder.makePropertyOrFieldSelector( Tick.class, field );
            selector = ticks -> dataSelector.apply( Iterables.getLast( (List<Tick>)ticks ) );
        }
        else
            selector = ExpressionBuilder.<T,BigDecimal>makePropertyOrFieldSelector( type, field );

        final List<BigDecimal> values = new ArrayList<>();
        dataDictionaries.forEach( dataDictionary -> {
            final T item = dataDictionary.get( symbol );
            if( item != null )
                values.add( selector.apply( item ) );
        } );

        return values.stream();
    }

    /**
     * Gets the data dictionaries of the requested type in each slice
     * <typeparam name="T The data type</typeparam>
     * @param slices The enumerable of slice
     * @returns An enumerable of data dictionary of the requested type
     */
    public static <T extends BaseData> Stream<DataDictionary<T>> get( final Stream<Slice> slices, final Class<T> type ) {
        return slices.map( x -> x.get( type ) ).filter( x -> x.size() > 0 );
    }

    /**
     * Gets an enumerable of T by accessing the slices for the requested symbol
     * <typeparam name="T The data type</typeparam>
     * @param slices The enumerable of slice
     * @param symbol The symbol to retrieve
     * @returns An enumerable of T by accessing each slice for the requested symbol
     */
    public static <T extends BaseData> Stream<T> get( final Stream<Slice> slices, final Symbol symbol, final Class<T> type ) {
        return slices.map(x -> x.get( type ) ).filter( x -> x.containsKey( symbol ) ).map( x -> x.get( symbol ) );
    }

    /**
     * Gets an enumerable of BigDecimal by accessing the slice for the symbol and then retrieving the specified
     * field on each piece of data
     * @param slices The enumerable of slice
     * @param symbol The symbol to retrieve
     * @param field The field selector used to access the dats
     * @returns An enumerable of decimal
     */
    @SuppressWarnings("unchecked")
    public static Stream<BigDecimal> get( final Stream<Slice> slices, final Symbol symbol, final Function<BaseData,BigDecimal> field ) {
        final Stream<BaseData> map = slices.map( slice -> slice.get( symbol ) );
        final Stream<BaseData> filter = map.filter( i -> i != null );
        final Stream<BigDecimal> map2 = filter.map( i -> i instanceof List ? field.apply( Iterables.getLast( (List<BaseData>)i ) ) : field.apply( i ) );
        return map2;
    }

    /**
     * Converts the specified enumerable of decimals into a double array
     * @param decimals The enumerable of decimal
     * @returns Double array representing the enumerable of decimal
     */
    public static double[] toDoubleArray( final Stream<BigDecimal> decimals ) {
        return decimals.mapToDouble( x -> x.doubleValue() ).toArray();
    }
}
