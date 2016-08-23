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
using System.Linq;
using QuantConnect.Data.Custom;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Data
{
    /**
    /// Provides a data structure for all of an algorithm's data at a single time step
    */
    public class Slice : IEnumerable<KeyValuePair<Symbol, BaseData>>
    {
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
        private final Lazy<DataMap<SymbolData>> _data;
        // Quandl -> DataDictonary<Quandl>
        private final Map<Type, Lazy<object>> _dataByType;

        /**
        /// Gets the timestamp for this slice of data
        */
        public DateTime Time
        {
            get; private set;
        }

        /**
        /// Gets whether or not this slice has data
        */
        public boolean HasData
        {
            get; private set;
        }

        /**
        /// Gets the <see cref="TradeBars"/> for this slice of data
        */
        public TradeBars Bars
        {
            get { return _bars; }
        }

        /**
        /// Gets the <see cref="QuoteBars"/> for this slice of data
        */
        public QuoteBars QuoteBars
        {
            get { return _quoteBars; }
        }

        /**
        /// Gets the <see cref="Ticks"/> for this slice of data
        */
        public Ticks Ticks
        {
            get { return _ticks; }
        }

        /**
        /// Gets the <see cref="OptionChains"/> for this slice of data
        */
        public OptionChains OptionChains
        {
            get { return _optionChains; }
        }

        /**
        /// Gets the <see cref="Splits"/> for this slice of data
        */
        public Splits Splits
        {
            get { return _splits; }
        }

        /**
        /// Gets the <see cref="Dividends"/> for this slice of data
        */
        public Dividends Dividends
        {
            get { return _dividends; }
        }

        /**
        /// Gets the <see cref="Delistings"/> for this slice of data
        */
        public Delistings Delistings
        {
            get { return _delistings; }
        }

        /**
        /// Gets the <see cref="QuantConnect.Data.Market.SymbolChangedEvents"/> for this slice of data
        */
        public SymbolChangedEvents SymbolChangedEvents
        {
            get { return _symbolChangedEvents; }
        }

        /**
        /// Gets the number of symbols held in this slice
        */
        public int Count
        {
            get { return _data.Value.Count; }
        }

        /**
        /// Gets all the symbols in this slice
        */
        public IReadOnlyList<Symbol> Keys
        {
            get { return new List<Symbol>(_data.Value.Keys); }
        }

        /**
        /// Gets a list of all the data in this slice
        */
        public IReadOnlyList<BaseData> Values
        {
            get { return GetKeyValuePairEnumerable().Select(x -> x.Value).ToList(); }
        }

        /**
        /// Initializes a new instance of the <see cref="Slice"/> class, lazily
        /// instantiating the <see cref="Slice.Bars"/> and <see cref="Slice.Ticks"/>
        /// collections on demand
        */
         * @param time">The timestamp for this slice of data
         * @param data">The raw data in this slice
        public Slice(DateTime time, IEnumerable<BaseData> data)
            : this(time, data, null, null, null, null, null, null, null, null ) {
        }

        /**
        /// Initializes a new instance of the <see cref="Slice"/> class
        */
         * @param time">The timestamp for this slice of data
         * @param data">The raw data in this slice
         * @param tradeBars">The trade bars for this slice
         * @param quoteBars">The quote bars for this slice
         * @param ticks">This ticks for this slice
         * @param optionChains">The option chains for this slice
         * @param splits">The splits for this slice
         * @param dividends">The dividends for this slice
         * @param delistings">The delistings for this slice
         * @param symbolChanges">The symbol changed events for this slice
         * @param hasData">true if this slice contains data
        public Slice(DateTime time, IEnumerable<BaseData> data, TradeBars tradeBars, QuoteBars quoteBars, Ticks ticks, OptionChains optionChains, Splits splits, Dividends dividends, Delistings delistings, SymbolChangedEvents symbolChanges, bool? hasData = null ) {
            Time = time;

            _dataByType = new Map<Type, Lazy<object>>();

            // market data
            _data = new Lazy<DataMap<SymbolData>>(() -> CreateDynamicDataDictionary(data));

            HasData = hasData ?? _data.Value.Count > 0;

            _ticks = CreateTicksCollection(ticks);
            _bars = CreateCollection<TradeBars, TradeBar>(tradeBars);
            _quoteBars = CreateCollection<QuoteBars, QuoteBar>(quoteBars);
            _optionChains = CreateCollection<OptionChains, OptionChain>(optionChains);

            // auxiliary data
            _splits = CreateCollection<Splits, Split>(splits);
            _dividends = CreateCollection<Dividends, Dividend>(dividends);
            _delistings = CreateCollection<Delistings, Delisting>(delistings);
            _symbolChangedEvents = CreateCollection<SymbolChangedEvents, SymbolChangedEvent>(symbolChanges);
        }

        /**
        /// Gets the data corresponding to the specified symbol. If the requested data
        /// is of <see cref="MarketDataType.Tick"/>, then a <see cref="List{Tick}"/> will
        /// be returned, otherwise, it will be the subscribed type, for example, <see cref="TradeBar"/>
        /// or event <see cref="Quandl"/> for custom data.
        */
         * @param symbol">The data's symbols
        @returns The data for the specified symbol
        public dynamic this[Symbol symbol]
        {
            get
            {
                SymbolData value;
                if( _data.Value.TryGetValue(symbol, out value)) {
                    return value.GetData();
                }
                throw new KeyNotFoundException( String.format( "'%1$s' wasn't found in the Slice object, likely because there was no-data at this moment in time and it wasn't possible to fillforward historical data. Please check the data exists before accessing it with data.ContainsKey(\"%1$s\")", symbol));
            }
        }

        /**
        /// Gets the <see cref="DataDictionary{T}"/> for all data of the specified type
        */
        /// <typeparam name="T">The type of data we want, for example, <see cref="TradeBar"/> or <see cref="Quandl"/>, ect...</typeparam>
        @returns The <see cref="DataDictionary{T}"/> containing the data of the specified type
        public DataMap<T> Get<T>()
            where T : BaseData
        {
            Lazy<object> dictionary;
            if( !_dataByType.TryGetValue(typeof(T), out dictionary)) {
                if( typeof(T) == typeof(Tick)) {
                    dictionary = new Lazy<object>(() -> new DataMap<T>(_data.Value.Values.SelectMany<dynamic, dynamic>(x -> x.GetData()).OfType<T>(), x -> x.Symbol));
                }
                else
                {
                    dictionary = new Lazy<object>(() -> new DataMap<T>(_data.Value.Values.Select(x -> x.GetData()).OfType<T>(), x -> x.Symbol));
                }

                _dataByType[typeof(T)] = dictionary;
            }
            return (DataMap<T>)dictionary.Value;
        }

        /**
        /// Gets the data of the specified symbol and type.
        */
        /// <typeparam name="T">The type of data we seek</typeparam>
         * @param symbol">The specific symbol was seek
        @returns The data for the requested symbol
        public T Get<T>(Symbol symbol)
            where T : BaseData
        {
            return Get<T>()[symbol];
        }

        /**
        /// Determines whether this instance contains data for the specified symbol
        */
         * @param symbol">The symbol we seek data for
        @returns True if this instance contains data for the symbol, false otherwise
        public boolean ContainsKey(Symbol symbol) {
            return _data.Value.ContainsKey(symbol);
        }

        /**
        /// Gets the data associated with the specified symbol
        */
         * @param symbol">The symbol we want data for
         * @param data">The data for the specifed symbol, or null if no data was found
        @returns True if data was found, false otherwise
        public boolean TryGetValue(Symbol symbol, out dynamic data) {
            data = null;
            SymbolData symbolData;
            if( _data.Value.TryGetValue(symbol, out symbolData)) {
                data = symbolData.GetData();
                return data != null;
            }
            return false;
        }

        /**
        /// Produces the dynamic data dictionary from the input data
        */
        private static DataMap<SymbolData> CreateDynamicDataDictionary(IEnumerable<BaseData> data) {
            allData = new DataMap<SymbolData>();
            foreach (datum in data) {
                SymbolData symbolData;
                if( !allData.TryGetValue(datum.Symbol, out symbolData)) {
                    symbolData = new SymbolData(datum.Symbol);
                    allData[datum.Symbol] = symbolData;
                }

                switch (datum.DataType) {
                    case MarketDataType.Base:
                        symbolData.Type = SubscriptionType.Custom;
                        symbolData.Custom = datum;
                        break;

                    case MarketDataType.TradeBar:
                        symbolData.Type = SubscriptionType.TradeBar;
                        symbolData.TradeBar = (TradeBar)datum;
                        break;

                    case MarketDataType.Tick:
                        symbolData.Type = SubscriptionType.Tick;
                        symbolData.Ticks.Add((Tick)datum);
                        break;

                    case MarketDataType.Auxiliary:
                        symbolData.AuxilliaryData.Add(datum);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return allData;
        }

        /**
        /// Returns the input ticks if non-null, otherwise produces one fom the dynamic data dictionary
        */
        private Ticks CreateTicksCollection(Ticks ticks) {
            if( ticks != null ) return ticks;
            ticks = new Ticks(Time);
            foreach (listTicks in _data.Value.Values.Select(x -> x.GetData()).OfType<List<Tick>>().Where(x -> x.Count != 0)) {
                ticks[listTicks[0].Symbol] = listTicks;
            }
            return ticks;
        }

        /**
        /// Returns the input collection if onon-null, otherwise produces one from the dynamic data dictionary
        */
        /// <typeparam name="T">The data dictionary type</typeparam>
        /// <typeparam name="TItem">The item type of the data Map</typeparam>
         * @param collection">The input collection, if non-null, returned immediately
        @returns The data dictionary of <typeparamref name="TItem"/> containing all the data of that type in this slice
        private T CreateCollection<T, TItem>(T collection)
            where T : DataMap<TItem>, new()
            where TItem : BaseData
        {
            if( collection != null ) return collection;
            collection = new T();
#pragma warning disable 618 // This assignment is left here until the Time property is removed.
            collection.Time = Time;
#pragma warning restore 618
            foreach (item in _data.Value.Values.Select(x -> x.GetData()).OfType<TItem>()) {
                collection[item.Symbol] = item;
            }
            return collection;
        }

        /**
        /// Returns an enumerator that iterates through the collection.
        */
        @returns 
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// 
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<Symbol, BaseData>> GetEnumerator() {
            return GetKeyValuePairEnumerable().GetEnumerator();
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

        private IEnumerable<KeyValuePair<Symbol, BaseData>> GetKeyValuePairEnumerable() {
            // this will not enumerate auxilliary data!
            return _data.Value.Select(kvp -> new KeyValuePair<Symbol, BaseData>(kvp.Key, kvp.Value.GetData()));
        }

        private enum SubscriptionType { TradeBar, Tick, Custom };
        private class SymbolData
        {
            public SubscriptionType Type;
            public final Symbol Symbol;

            // data
            public BaseData Custom;
            public TradeBar TradeBar;
            public final List<Tick> Ticks;
            public final List<BaseData> AuxilliaryData;

            public SymbolData(Symbol symbol) {
                Symbol = symbol;
                Ticks = new List<Tick>();
                AuxilliaryData = new List<BaseData>();
            }

            public dynamic GetData() {
                switch (Type) {
                    case SubscriptionType.TradeBar:
                        return TradeBar;
                    case SubscriptionType.Tick:
                        return Ticks;
                    case SubscriptionType.Custom:
                        return Custom;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

    }
}
