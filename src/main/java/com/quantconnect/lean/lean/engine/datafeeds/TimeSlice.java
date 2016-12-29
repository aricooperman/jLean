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

//using NodaTime;

package com.quantconnect.lean.lean.engine.datafeeds;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Map.Entry;
import java.util.Set;

import com.quantconnect.lean.Extensions;
import com.quantconnect.lean.MarketDataType;
import com.quantconnect.lean.SecurityType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.TickType;
import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.Slice;
import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.data.market.Delisting;
import com.quantconnect.lean.data.market.Delistings;
import com.quantconnect.lean.data.market.Dividend;
import com.quantconnect.lean.data.market.Dividends;
import com.quantconnect.lean.data.market.OptionChain;
import com.quantconnect.lean.data.market.OptionChains;
import com.quantconnect.lean.data.market.OptionContract;
import com.quantconnect.lean.data.market.QuoteBar;
import com.quantconnect.lean.data.market.QuoteBars;
import com.quantconnect.lean.data.market.Split;
import com.quantconnect.lean.data.market.Splits;
import com.quantconnect.lean.data.market.SymbolChangedEvent;
import com.quantconnect.lean.data.market.SymbolChangedEvents;
import com.quantconnect.lean.data.market.Tick;
import com.quantconnect.lean.data.market.Ticks;
import com.quantconnect.lean.data.market.TradeBar;
import com.quantconnect.lean.data.market.TradeBars;
import com.quantconnect.lean.data.universeselection.BaseDataCollection;
import com.quantconnect.lean.data.universeselection.OptionChainUniverseDataCollection;
import com.quantconnect.lean.data.universeselection.SecurityChanges;
import com.quantconnect.lean.securities.Cash;
import com.quantconnect.lean.securities.CashBook;
import com.quantconnect.lean.securities.Security;
import com.quantconnect.lean.securities.option.Option;

import javaslang.Lazy;


/**
 * Represents a grouping of data emitted at a certain time.
*/
public class TimeSlice {
   
    private final int DataPointCount;
    private final LocalDateTime Time;
    private final List<DataFeedPacket> Data;
    
    
    /**
     * Gets the count of data points in this <see cref="TimeSlice"/>
     */
    public int getDataPointCount() {
        return DataPointCount;
    }

    /**
     * Gets the time this data was emitted
     */
    public LocalDateTime getTime() {
        return Time;
    }

    /**
     * Gets the data in the time slice
     */
    public List<DataFeedPacket> getData() {
        return Data;
    }

    /**
     * Gets the <see cref="Slice"/> that will be used as input for the algorithm
     */
    public Slice Slice;
//    { get; private set; }
    
    /**
     * Gets the data used to update the cash book
     */
    public List<UpdateData<Cash>> CashBookUpdateData;
//    { get; private set; }
    
    /**
     * Gets the data used to update securities
     */
    public List<UpdateData<Security>> SecuritiesUpdateData;
//    { get; private set; }
    
    /**
     * Gets the data used to update the consolidators
     */
    public List<UpdateData<SubscriptionDataConfig>> ConsolidatorUpdateData;
//    { get; private set; }
    
    /**
     * Gets all the custom data in this <see cref="TimeSlice"/>
     */
    public List<UpdateData<Security>> CustomData;
//    { get; private set; }
    
    /**
     * Gets the changes to the data subscriptions as a result of universe selection
     */
    public SecurityChanges SecurityChanges;
//    { final get; set; }
    
    /**
     * Initializes a new <see cref="TimeSlice"/> containing the specified data
     */
    public TimeSlice(
            final LocalDateTime time,
            final int dataPointCount,
            final Slice slice,
            final List<DataFeedPacket> data,
            final List<UpdateData<Cash>> cashBookUpdateData,
            final List<UpdateData<Security>> securitiesUpdateData,
            final List<UpdateData<SubscriptionDataConfig>> consolidatorUpdateData,
            final List<UpdateData<Security>> customData,
            final SecurityChanges securityChanges) {
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
     * @param utcDateTime The UTC frontier date time
     * @param algorithmTimeZone The algorithm's time zone, required for computing algorithm and slice time
     * @param cashBook The algorithm's cash book, required for generating cash update pairs
     * @param data The data in this <see cref="TimeSlice"/>
     * @param changes The new changes that are seen in this time slice as a result of universe selection
     * @returns A new <see cref="TimeSlice"/> containing the specified data
     */
    @SuppressWarnings("unchecked")
    public static TimeSlice create( final LocalDateTime utcDateTime, final ZoneId algorithmTimeZone, final CashBook cashBook, final List<DataFeedPacket> data, final SecurityChanges changes ) {
        int count = 0;
        final List<UpdateData<Security>> security = new ArrayList<>();
        final List<UpdateData<Security>> custom = new ArrayList<>();
        final List<UpdateData<SubscriptionDataConfig>> consolidator = new ArrayList<>();
        final List<BaseData> allDataForAlgorithm = new ArrayList<>( data.size() );
        final List<UpdateData<Cash>> cash = new ArrayList<>( cashBook.size() );
    
        final Set<Symbol> cashSecurities = new HashSet<>();
        for( final Cash cashItem : cashBook.values() )
            cashSecurities.add( cashItem.getSecuritySymbol() );
    
//        final Split split;
//        final Dividend dividend;
//        final Delisting delisting;
//        final SymbolChangedEvent symbolChange;
    
        // we need to be able to reference the slice being created in order to define the
        // evaluation of option price models, so we define a 'future' that can be referenced
        // in the option price model evaluation delegates for each contract
        final Slice slice;
        final Lazy<Slice> sliceFuture = Lazy.of( () -> slice );
    
        final LocalDateTime algorithmTime = Extensions.convertFromUtc( utcDateTime, algorithmTimeZone );
        final TradeBars tradeBars = new TradeBars();
        final QuoteBars quoteBars = new QuoteBars();
        final Ticks ticks = new Ticks();
        final Splits splits = new Splits();
        final Dividends dividends = new Dividends();
        final Delistings delistings = new Delistings();
        final OptionChains optionChains = new OptionChains();
        final SymbolChangedEvents symbolChanges = new SymbolChangedEvents();
    
        for( final DataFeedPacket packet : data ) {
            final List<BaseData> list = packet.getData();
            final Symbol symbol = packet.getSecurity().getSymbol();
    
            if( list.isEmpty() )
                continue;
            
            // keep count of all data points
            if( list.size() == 1 && list.get( 0 ) instanceof BaseDataCollection ) {
                final int baseDataCollectionCount = ((BaseDataCollection)list.get( 0 )).getData().size();
                if( baseDataCollectionCount == 0 )
                    continue;
                
                count += baseDataCollectionCount;
            }
            else
                count += list.size();
    
            final SubscriptionDataConfig configuration = packet.getConfiguration();
            if( !configuration.isInternalFeed && configuration.isCustomData ) {
                // This is all the custom data
                custom.add( new UpdateData<>( packet.getSecurity(), (Class<? extends Security>)configuration.type, list ) );
            }
    
            final List<BaseData> securityUpdate = new ArrayList<>( list.size() );
            final List<BaseData> consolidatorUpdate = new ArrayList<>( list.size() );
            for( int i = 0; i < list.size(); i++ ) {
                final BaseData baseData = list.get( i );
                if( !configuration.isInternalFeed ) {
                    // this is all the data that goes into the algorithm
                    allDataForAlgorithm.add( baseData );
                }
                // don't add internal feed data to ticks/bars objects
                if( baseData.getDataType() != MarketDataType.Auxiliary) {
                    if( !configuration.isInternalFeed ) {
                        populateDataDictionaries( baseData, ticks, tradeBars, quoteBars, optionChains );
    
                        // special handling of options data to build the option chain
                        if( packet.getSecurity().getType() == SecurityType.Option ) {
                            if( baseData.getDataType() == MarketDataType.OptionChain )
                                optionChains.put( baseData.getSymbol(), (OptionChain)baseData );
                            else if( !handleOptionData( algorithmTime, baseData, optionChains, packet.getSecurity(), sliceFuture ) )
                                continue;
                        }
    
                        // this is data used to update consolidators
                        consolidatorUpdate.add( baseData );
                    }
    
                    // this is the data used set market prices
                    securityUpdate.add( baseData );
                }
                // include checks for various aux types so we don't have to construct the dictionaries in Slice
                else if( baseData instanceof Delisting )
                    delistings.put( symbol, (Delisting)baseData );
                else if( baseData instanceof Dividend )
                    dividends.put( symbol, (Dividend)baseData );
                else if( baseData instanceof Split )
                    splits.put( symbol, (Split)baseData );
                else if( baseData instanceof SymbolChangedEvent ) {
                    // symbol changes is keyed by the requested symbol
                    symbolChanges.put( configuration.getSymbol(), (SymbolChangedEvent)baseData );
                }
            }
    
            if( securityUpdate.size() > 0 ) {
                // check for 'cash securities' if we found valid update data for this symbol
                // and we need this data to update cash conversion rates, long term we should
                // have Cash hold onto it's security, then he can update himself, or rather, just
                // patch through calls to conversion rate to compue it on the fly using Security.Price
                if( cashSecurities.contains( packet.getSecurity().getSymbol() ) ) {
                    for( final Entry<String,Cash> cashKvp : cashBook.entrySet() ) {
                        if( cashKvp.getValue().getSecuritySymbol().equals( packet.getSecurity().getSymbol() ) ) {
                            final List<BaseData> cashUpdates = Arrays.asList( securityUpdate.get( securityUpdate.size() - 1 ) );
                            cash.add( new UpdateData<>( cashKvp.getValue(), (Class<? extends Cash>)configuration.type, cashUpdates ) );
                        }
                    }
                }
    
                security.add( new UpdateData<>( packet.getSecurity(), (Class<? extends Security>)configuration.type, securityUpdate ) );
            }
            
            if( consolidatorUpdate.size() > 0 )
                consolidator.add( new UpdateData<>( configuration, (Class<? extends SubscriptionDataConfig>)configuration.type, consolidatorUpdate ) );
        }
    
        slice = new Slice( algorithmTime, allDataForAlgorithm, tradeBars, quoteBars, ticks, optionChains, splits, dividends, delistings, symbolChanges, allDataForAlgorithm.size() > 0 );
    
        return new TimeSlice( utcDateTime, count, slice, data, cash, security, consolidator, custom, changes);
    }
    
    /**
     * Adds the specified <see cref="BaseData"/> instance to the appropriate <see cref="DataDictionary{T}"/>
     */
    private static void populateDataDictionaries( final BaseData baseData, final Ticks ticks, final TradeBars tradeBars,
            final QuoteBars quoteBars, final OptionChains optionChains) {
        final Symbol symbol = baseData.getSymbol();
    
        // populate data dictionaries
        switch( baseData.getDataType() ) {
            case Tick:
                ticks.put( symbol, Collections.singletonList( (Tick)baseData ) );
                break;
    
            case TradeBar:
                tradeBars.put( symbol, (TradeBar)baseData );
                break;
    
            case QuoteBar:
                quoteBars.put( symbol, (QuoteBar)baseData );
                break;
    
            case OptionChain:
                optionChains.put( symbol, (OptionChain)baseData );
                break;
            default:
                break;
        }
    }

    private static boolean handleOptionData( final LocalDateTime algorithmTime, final BaseData baseData, final OptionChains optionChains,
            final Security security, final Lazy<Slice> sliceFuture ) {
        final Symbol symbol = baseData.getSymbol();
        
        final Symbol canonical = Symbol.create( symbol.getId().getSymbol(), SecurityType.Option, symbol.getId().getMarket() );
        OptionChain chain = optionChains.get( canonical );
        if( chain == null ) {
            chain = new OptionChain( canonical, algorithmTime );
            optionChains.put( canonical, chain );
        }
    
        if( baseData instanceof OptionChainUniverseDataCollection ) {
            final OptionChainUniverseDataCollection universeData = (OptionChainUniverseDataCollection)baseData;
            if( universeData.getUnderlying() != null )
                chain.setUnderlying( universeData.getUnderlying() );
            
            for( final Symbol contractSymbol : universeData.getFilteredContracts() ) {
                chain.getFilteredContracts().add( contractSymbol );
            }
            
            return false;
        }
    
        OptionContract contract = chain.getContracts().get( baseData.getSymbol() );
        if( contract == null ) {
            final Symbol underlyingSymbol = Symbol.create( baseData.getSymbol().getId().getSymbol(), SecurityType.Equity, baseData.getSymbol().getId().getMarket() );
            contract = new OptionContract( baseData.getSymbol(), underlyingSymbol );
            contract.setTime( baseData.getEndTime() );
            contract.setLastPrice( security.getClose() );
            contract.setBidPrice( security.getBidPrice() );
            contract.setBidSize( security.getBidSize() );
            contract.setAskPrice( security.getAskPrice() );
            contract.setAskSize( security.getAskSize() );
            contract.setUnderlyingLastPrice( chain.getUnderlying() != null ? chain.getUnderlying().getPrice() : BigDecimal.ZERO );
            
            chain.getContracts().put( baseData.getSymbol(), contract );
            if( security instanceof Option ) {
                final Option option = (Option)security;
                contract.setOptionPriceModel( () -> option.getPriceModel().evaluate( option, sliceFuture.get(), contract ) );
            }
        }
    
        Tick tick;
        TradeBar tradeBar;
        QuoteBar quote;
        // populate ticks and tradebars dictionaries with no aux data
        switch( baseData.getDataType() ) {
            case Tick:
                tick = (Tick)baseData;
                chain.getTicks().put( tick.getSymbol(), Arrays.asList( tick ) );
                updateContract( contract, tick );
                break;

            case TradeBar:
                tradeBar = (TradeBar)baseData;
                chain.getTradeBars().put( symbol, tradeBar );
                contract.setLastPrice( tradeBar.getClose() );
                break;

            case QuoteBar:
                quote = (QuoteBar)baseData;
                chain.getQuoteBars().put( symbol, quote );
                updateContract( contract, quote );
                break;

            case Base:
                chain.addAuxData( baseData );
                break;
            default:
                break;
        }
        
        return true;
    }

    private static void updateContract( final OptionContract contract, final QuoteBar quote ) {
        if( quote.getAsk() != null && quote.getAsk().getClose().signum() != 0 ) {
            contract.setAskPrice( quote.getAsk().getClose() );
            contract.setAskSize( quote.getLastAskSize() );
        }
        
        if( quote.getBid() != null && quote.getBid().getClose().signum() != 0 ) {
            contract.setBidPrice( quote.getBid().getClose() );
            contract.setBidSize( quote.getLastBidSize() );
        }
    }

    private static void updateContract( final OptionContract contract, final Tick tick ) {
        if( tick.tickType == TickType.Trade )
            contract.setLastPrice( tick.getPrice() );
        else if( tick.tickType == TickType.Quote ) {
            if( tick.askPrice.signum() != 0 ) {
                contract.setAskPrice( tick.askPrice );
                contract.setAskSize( tick.askSize );
            }
            if( tick.bidPrice.signum() != 0 ) {
                contract.setBidPrice( tick.bidPrice );
                contract.setBidSize( tick.bidSize );
            }
        }
    }
}
