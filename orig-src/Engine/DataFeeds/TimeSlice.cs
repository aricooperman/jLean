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
using System.Collections.Generic;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Securities;
using QuantConnect.Securities.Option;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Represents a grouping of data emitted at a certain time.
    */
    public class TimeSlice
    {
        /**
         * Gets the count of data points in this <see cref="TimeSlice"/>
        */
        public int DataPointCount { get; private set; }

        /**
         * Gets the time this data was emitted
        */
        public DateTime Time { get; private set; }

        /**
         * Gets the data in the time slice
        */
        public List<DataFeedPacket> Data { get; private set; }

        /**
         * Gets the <see cref="Slice"/> that will be used as input for the algorithm
        */
        public Slice Slice { get; private set; }

        /**
         * Gets the data used to update the cash book
        */
        public List<UpdateData<Cash>> CashBookUpdateData { get; private set; }

        /**
         * Gets the data used to update securities
        */
        public List<UpdateData<Security>> SecuritiesUpdateData { get; private set; }

        /**
         * Gets the data used to update the consolidators
        */
        public List<UpdateData<SubscriptionDataConfig>> ConsolidatorUpdateData { get; private set; }

        /**
         * Gets all the custom data in this <see cref="TimeSlice"/>
        */
        public List<UpdateData<Security>> CustomData { get; private set; }

        /**
         * Gets the changes to the data subscriptions as a result of universe selection
        */
        public SecurityChanges SecurityChanges { get; set; }

        /**
         * Initializes a new <see cref="TimeSlice"/> containing the specified data
        */
        public TimeSlice(DateTime time,
            int dataPointCount,
            Slice slice,
            List<DataFeedPacket> data,
            List<UpdateData<Cash>> cashBookUpdateData,
            List<UpdateData<Security>> securitiesUpdateData,
            List<UpdateData<SubscriptionDataConfig>> consolidatorUpdateData,
            List<UpdateData<Security>> customData,
            SecurityChanges securityChanges) {
            Time = time;
            Data = data;
            Slice = slice;
            CustomData = customData;
            DataPointCount = dataPointCount;
            CashBookUpdateData = cashBookUpdateData;
            SecuritiesUpdateData = securitiesUpdateData;
            ConsolidatorUpdateData = consolidatorUpdateData;
            SecurityChanges = securityChanges;
        }

        /**
         * Creates a new <see cref="TimeSlice"/> for the specified time using the specified data
        */
         * @param utcDateTime The UTC frontier date time
         * @param algorithmTimeZone The algorithm's time zone, required for computing algorithm and slice time
         * @param cashBook The algorithm's cash book, required for generating cash update pairs
         * @param data The data in this <see cref="TimeSlice"/>
         * @param changes The new changes that are seen in this time slice as a result of universe selection
        @returns A new <see cref="TimeSlice"/> containing the specified data
        public static TimeSlice Create(DateTime utcDateTime, ZoneId algorithmTimeZone, CashBook cashBook, List<DataFeedPacket> data, SecurityChanges changes) {
            int count = 0;
            security = new List<UpdateData<Security>>();
            custom = new List<UpdateData<Security>>();
            consolidator = new List<UpdateData<SubscriptionDataConfig>>();
            allDataForAlgorithm = new List<BaseData>(data.Count);
            cash = new List<UpdateData<Cash>>(cashBook.Count);

            cashSecurities = new HashSet<Symbol>();
            foreach (cashItem in cashBook.Values) {
                cashSecurities.Add(cashItem.SecuritySymbol);
            }

            Split split;
            Dividend dividend;
            Delisting delisting;
            SymbolChangedEvent symbolChange;

            // we need to be able to reference the slice being created in order to define the
            // evaluation of option price models, so we define a 'future' that can be referenced
            // in the option price model evaluation delegates for each contract
            Slice slice = null;
            sliceFuture = new Lazy<Slice>(() -> slice);

            algorithmTime = utcDateTime.ConvertFromUtc(algorithmTimeZone);
            tradeBars = new TradeBars(algorithmTime);
            quoteBars = new QuoteBars(algorithmTime);
            ticks = new Ticks(algorithmTime);
            splits = new Splits(algorithmTime);
            dividends = new Dividends(algorithmTime);
            delistings = new Delistings(algorithmTime);
            optionChains = new OptionChains(algorithmTime);
            symbolChanges = new SymbolChangedEvents(algorithmTime);

            foreach (packet in data) {
                list = packet.Data;
                symbol = packet.Security.Symbol;

                if( list.Count == 0) continue;
                
                // keep count of all data points
                if( list.Count == 1 && list[0] is BaseDataCollection) {
                    baseDataCollectionCount = ((BaseDataCollection)list[0]).Data.Count;
                    if( baseDataCollectionCount == 0) {
                        continue;
                    }
                    count += baseDataCollectionCount;
                }
                else
                {
                    count += list.Count;
                }

                if( !packet.Configuration.IsInternalFeed && packet.Configuration.IsCustomData) {
                    // This is all the custom data
                    custom.Add(new UpdateData<Security>(packet.Security, packet.Configuration.Type, list));
                }

                securityUpdate = new List<BaseData>(list.Count);
                consolidatorUpdate = new List<BaseData>(list.Count);
                for (int i = 0; i < list.Count; i++) {
                    baseData = list[i];
                    if( !packet.Configuration.IsInternalFeed) {
                        // this is all the data that goes into the algorithm
                        allDataForAlgorithm.Add(baseData);
                    }
                    // don't add internal feed data to ticks/bars objects
                    if( baseData.DataType != MarketDataType.Auxiliary) {
                        if( !packet.Configuration.IsInternalFeed) {
                            PopulateDataDictionaries(baseData, ticks, tradeBars, quoteBars, optionChains);

                            // special handling of options data to build the option chain
                            if( packet.Security.Type == SecurityType.Option) {
                                if( baseData.DataType == MarketDataType.OptionChain) {
                                    optionChains[baseData.Symbol] = (OptionChain) baseData;
                                }
                                else if( !HandleOptionData(algorithmTime, baseData, optionChains, packet.Security, sliceFuture)) {
                                    continue;
                                }
                            }

                            // this is data used to update consolidators
                            consolidatorUpdate.Add(baseData);
                        }

                        // this is the data used set market prices
                        securityUpdate.Add(baseData);
                    }
                    // include checks for various aux types so we don't have to construct the dictionaries in Slice
                    else if( (delisting = baseData as Delisting) != null ) {
                        delistings[symbol] = delisting;
                    }
                    else if( (dividend = baseData as Dividend) != null ) {
                        dividends[symbol] = dividend;
                    }
                    else if( (split = baseData as Split) != null ) {
                        splits[symbol] = split;
                    }
                    else if( (symbolChange = baseData as SymbolChangedEvent) != null ) {
                        // symbol changes is keyed by the requested symbol
                        symbolChanges[packet.Configuration.Symbol] = symbolChange;
                    }
                }

                if( securityUpdate.Count > 0) {
                    // check for 'cash securities' if we found valid update data for this symbol
                    // and we need this data to update cash conversion rates, long term we should
                    // have Cash hold onto it's security, then he can update himself, or rather, just
                    // patch through calls to conversion rate to compue it on the fly using Security.Price
                    if( cashSecurities.Contains(packet.Security.Symbol)) {
                        foreach (cashKvp in cashBook) {
                            if( cashKvp.Value.SecuritySymbol == packet.Security.Symbol) {
                                cashUpdates = new List<BaseData> {securityUpdate[securityUpdate.Count - 1]};
                                cash.Add(new UpdateData<Cash>(cashKvp.Value, packet.Configuration.Type, cashUpdates));
                            }
                        }
                    }

                    security.Add(new UpdateData<Security>(packet.Security, packet.Configuration.Type, securityUpdate));
                }
                if( consolidatorUpdate.Count > 0) {
                    consolidator.Add(new UpdateData<SubscriptionDataConfig>(packet.Configuration, packet.Configuration.Type, consolidatorUpdate));
                }
            }

            slice = new Slice(algorithmTime, allDataForAlgorithm, tradeBars, quoteBars, ticks, optionChains, splits, dividends, delistings, symbolChanges, allDataForAlgorithm.Count > 0);

            return new TimeSlice(utcDateTime, count, slice, data, cash, security, consolidator, custom, changes);
        }

        /**
         * Adds the specified <see cref="BaseData"/> instance to the appropriate <see cref="DataDictionary{T}"/>
        */
        private static void PopulateDataDictionaries(BaseData baseData, Ticks ticks, TradeBars tradeBars, QuoteBars quoteBars, OptionChains optionChains) {
            symbol = baseData.Symbol;

            // populate data dictionaries
            switch (baseData.DataType) {
                case MarketDataType.Tick:
                    ticks.Add(symbol, (Tick)baseData);
                    break;

                case MarketDataType.TradeBar:
                    tradeBars[symbol] = (TradeBar) baseData;
                    break;

                case MarketDataType.QuoteBar:
                    quoteBars[symbol] = (QuoteBar) baseData;
                    break;

                case MarketDataType.OptionChain:
                    optionChains[symbol] = (OptionChain) baseData;
                    break;
            }
        }

        private static boolean HandleOptionData(DateTime algorithmTime, BaseData baseData, OptionChains optionChains, Security security, Lazy<Slice> sliceFuture) {
            symbol = baseData.Symbol;
            
            OptionChain chain;
            canonical = Symbol.Create(symbol.ID.Symbol, SecurityType.Option, symbol.ID.Market);
            if( !optionChains.TryGetValue(canonical, out chain)) {
                chain = new OptionChain(canonical, algorithmTime);
                optionChains[canonical] = chain;
            }

            universeData = baseData as OptionChainUniverseDataCollection;
            if( universeData != null ) {
                if( universeData.Underlying != null ) {
                    chain.Underlying = universeData.Underlying;
                }
                foreach (contractSymbol in universeData.FilteredContracts) {
                    chain.FilteredContracts.Add(contractSymbol);
                }
                return false;
            }

            OptionContract contract;
            if( !chain.Contracts.TryGetValue(baseData.Symbol, out contract)) {
                underlyingSymbol = Symbol.Create(baseData.Symbol.ID.Symbol, SecurityType.Equity, baseData.Symbol.ID.Market);
                contract = new OptionContract(baseData.Symbol, underlyingSymbol) {
                    Time = baseData.EndTime,
                    LastPrice = security.Close,
                    BidPrice = security.BidPrice,
                    BidSize = security.BidSize,
                    AskPrice = security.AskPrice,
                    AskSize = security.AskSize,
                    UnderlyingLastPrice = chain.Underlying != null ? chain.Underlying.Price : BigDecimal.ZERO
                };
                chain.Contracts[baseData.Symbol] = contract;
                option = security as Option;
                if( option != null ) {
                    contract.SetOptionPriceModel(() -> option.PriceModel.Evaluate(option, sliceFuture.Value, contract));
                }
            }

            // populate ticks and tradebars dictionaries with no aux data
            switch (baseData.DataType) {
                case MarketDataType.Tick:
                    tick = (Tick)baseData;
                    chain.Ticks.Add(tick.Symbol, tick);
                    UpdateContract(contract, tick);
                    break;

                case MarketDataType.TradeBar:
                    tradeBar = (TradeBar)baseData;
                    chain.TradeBars[symbol] = tradeBar;
                    contract.LastPrice = tradeBar.Close;
                    break;

                case MarketDataType.QuoteBar:
                    quote = (QuoteBar)baseData;
                    chain.QuoteBars[symbol] = quote;
                    UpdateContract(contract, quote);
                    break;

                case MarketDataType.Base:
                    chain.AddAuxData(baseData);
                    break;
            }
            return true;
        }

        private static void UpdateContract(OptionContract contract, QuoteBar quote) {
            if( quote.Ask != null && quote.Ask.Close != BigDecimal.ZERO) {
                contract.AskPrice = quote.Ask.Close;
                contract.AskSize = quote.LastAskSize;
            }
            if( quote.Bid != null && quote.Bid.Close != BigDecimal.ZERO) {
                contract.BidPrice = quote.Bid.Close;
                contract.BidSize = quote.LastBidSize;
            }
        }

        private static void UpdateContract(OptionContract contract, Tick tick) {
            if( tick.TickType == TickType.Trade) {
                contract.LastPrice = tick.Price;
            }
            else if( tick.TickType == TickType.Quote) {
                if( tick.AskPrice != BigDecimal.ZERO) {
                    contract.AskPrice = tick.AskPrice;
                    contract.AskSize = tick.AskSize;
                }
                if( tick.BidPrice != BigDecimal.ZERO) {
                    contract.BidPrice = tick.BidPrice;
                    contract.BidSize = tick.BidSize;
                }
            }
        }
    }
}