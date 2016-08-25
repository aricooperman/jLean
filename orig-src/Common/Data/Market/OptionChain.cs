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
using QuantConnect.Util;

package com.quantconnect.lean.Data.Market
{
    /**
     * Represents an entire chain of option contracts for a single underying security.
     * This type is <see cref="IEnumerable{OptionContract}"/>
    */
    public class OptionChain : BaseData, IEnumerable<OptionContract>
    {
        private final Map<Type, Map<Symbol, List<BaseData>>> _auxiliaryData = new Map<Type, Map<Symbol, List<BaseData>>>();

        /**
         * Gets the most recent trade information for the underlying. This may
         * be a <see cref="Tick"/> or a <see cref="TradeBar"/>
        */
        public BaseData Underlying
        {
            get; internal set;
        }

        /**
         * Gets all ticks for every option contract in this chain, keyed by option symbol
        */
        public Ticks Ticks
        {
            get; private set;
        }

        /**
         * Gets all trade bars for every option contract in this chain, keyed by option symbol
        */
        public TradeBars TradeBars
        {
            get; private set;
        }

        /**
         * Gets all quote bars for every option contract in this chain, keyed by option symbol
        */
        public QuoteBars QuoteBars
        {
            get; private set;
        }

        /**
         * Gets all contracts in the chain, keyed by option symbol
        */
        public OptionContracts Contracts
        {
            get; private set;
        }

        /**
         * Gets the set of symbols that passed the <see cref="Option.ContractFilter"/>
        */
        public HashSet<Symbol> FilteredContracts
        {
            get; private set;
        }

        /**
         * Initializes a new default instance of the <see cref="OptionChain"/> class
        */
        private OptionChain() {
            DataType = MarketDataType.OptionChain;
        }

        /**
         * Initializes a new instance of the <see cref="OptionChain"/> class
        */
         * @param canonicalOptionSymbol The symbol for this chain.
         * @param time The time of this chain
        public OptionChain(Symbol canonicalOptionSymbol, DateTime time) {
            Time = time;
            Symbol = canonicalOptionSymbol;
            DataType = MarketDataType.OptionChain;
            Ticks = new Ticks(time);
            TradeBars = new TradeBars(time);
            QuoteBars = new QuoteBars(time);
            Contracts = new OptionContracts(time);
            FilteredContracts = new HashSet<Symbol>();
        }

        /**
         * Initializes a new instance of the <see cref="OptionChain"/> class
        */
         * @param canonicalOptionSymbol The symbol for this chain.
         * @param time The time of this chain
         * @param underlying The most recent underlying trade data
         * @param trades All trade data for the entire option chain
         * @param quotes All quote data for the entire option chain
         * @param contracts All contrains for this option chain
        public OptionChain(Symbol canonicalOptionSymbol, DateTime time, BaseData underlying, IEnumerable<BaseData> trades, IEnumerable<BaseData> quotes, IEnumerable<OptionContract> contracts, IEnumerable<Symbol> filteredContracts) {
            Time = time;
            Underlying = underlying;
            Symbol = canonicalOptionSymbol;
            DataType = MarketDataType.OptionChain;
            FilteredContracts = filteredContracts.ToHashSet();

            Ticks = new Ticks(time);
            TradeBars = new TradeBars(time);
            QuoteBars = new QuoteBars(time);
            Contracts = new OptionContracts(time);

            foreach (trade in trades) {
                tick = trade as Tick;
                if( tick != null ) {
                    List<Tick> ticks;
                    if( !Ticks.TryGetValue(tick.Symbol, out ticks)) {
                        ticks = new List<Tick>();
                        Ticks[tick.Symbol] = ticks;
                    }
                    ticks.Add(tick);
                    continue;
                }
                bar = trade as TradeBar;
                if( bar != null ) {
                    TradeBars[trade.Symbol] = bar;
                }
            }

            foreach (quote in quotes) {
                tick = quote as Tick;
                if( tick != null ) {
                    List<Tick> ticks;
                    if( !Ticks.TryGetValue(tick.Symbol, out ticks)) {
                        ticks = new List<Tick>();
                        Ticks[tick.Symbol] = ticks;
                    }
                    ticks.Add(tick);
                    continue;
                }
                bar = quote as QuoteBar;
                if( bar != null ) {
                    QuoteBars[quote.Symbol] = bar;
                }
            }

            foreach (contract in contracts) {
                Contracts[contract.Symbol] = contract;
            }
        }

        /**
         * Gets the auxiliary data with the specified type and symbol
        */
         * <typeparam name="T The type of auxiliary data</typeparam>
         * @param symbol The symbol of the auxiliary data
        @returns The last auxiliary data with the specified type and symbol
        public T GetAux<T>(Symbol symbol) {
            List<BaseData> list;
            Map<Symbol, List<BaseData>> dictionary;
            if( !_auxiliaryData.TryGetValue(typeof(T), out dictionary) || !dictionary.TryGetValue(symbol, out list)) {
                return default(T);
            }
            return list.OfType<T>().LastOrDefault();
        }

        /**
         * Gets all auxiliary data of the specified type as a dictionary keyed by symbol
        */
         * <typeparam name="T The type of auxiliary data</typeparam>
        @returns A dictionary containing all auxiliary data of the specified type
        public DataMap<T> GetAux<T>() {
            Map<Symbol, List<BaseData>> d;
            if( !_auxiliaryData.TryGetValue(typeof(T), out d)) {
                return new DataMap<T>();
            }
            dictionary = new DataMap<T>();
            foreach (kvp in d) {
                item = kvp.Value.OfType<T>().LastOrDefault();
                if( item != null ) {
                    dictionary.Add(kvp.Key, item);
                }
            }
            return dictionary;
        }

        /**
         * Gets all auxiliary data of the specified type as a dictionary keyed by symbol
        */
         * <typeparam name="T The type of auxiliary data</typeparam>
        @returns A dictionary containing all auxiliary data of the specified type
        public Map<Symbol, List<BaseData>> GetAuxList<T>() {
            Map<Symbol, List<BaseData>> dictionary;
            if( !_auxiliaryData.TryGetValue(typeof(T), out dictionary)) {
                return new Map<Symbol, List<BaseData>>();
            }
            return dictionary;
        }

        /**
         * Gets a list of auxiliary data with the specified type and symbol
        */
         * <typeparam name="T The type of auxiliary data</typeparam>
         * @param symbol The symbol of the auxiliary data
        @returns The list of auxiliary data with the specified type and symbol
        public List<T> GetAuxList<T>(Symbol symbol) {
            List<BaseData> list;
            Map<Symbol, List<BaseData>> dictionary;
            if( !_auxiliaryData.TryGetValue(typeof(T), out dictionary) || !dictionary.TryGetValue(symbol, out list)) {
                return new List<T>();
            }
            return list.OfType<T>().ToList();
        }

        /**
         * Returns an enumerator that iterates through the collection.
        */
        @returns 
         * An enumerator that can be used to iterate through the collection.
         * 
        public IEnumerator<OptionContract> GetEnumerator() {
            return Contracts.Values.GetEnumerator();
        }

        /**
         * Returns an enumerator that iterates through a collection.
        */
        @returns 
         * An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
         * 
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /**
         * Return a new instance clone of this object, used in fill forward
        */
        @returns A clone of the current object
        public @Override BaseData Clone() {
            return new OptionChain
            {
                Underlying = Underlying,
                Ticks = Ticks,
                Contracts = Contracts,
                QuoteBars = QuoteBars,
                TradeBars = TradeBars,
                FilteredContracts = FilteredContracts,
                Symbol = Symbol,
                Time = Time,
                DataType = DataType,
                Value = Value
            };
        }

        /**
         * Adds the specified auxiliary data to this option chain
        */
         * @param baseData The auxiliary data to be added
        internal void AddAuxData(BaseData baseData) {
            type = baseData.GetType();
            Map<Symbol, List<BaseData>> dictionary;
            if( !_auxiliaryData.TryGetValue(type, out dictionary)) {
                dictionary = new Map<Symbol, List<BaseData>>();
                _auxiliaryData[type] = dictionary;
            }

            List<BaseData> list;
            if( !dictionary.TryGetValue(baseData.Symbol, out list)) {
                list = new List<BaseData>();
                dictionary[baseData.Symbol] = list;
            }
            list.Add(baseData);
        }
    }
}