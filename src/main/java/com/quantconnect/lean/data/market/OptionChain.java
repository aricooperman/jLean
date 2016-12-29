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

package com.quantconnect.lean.data.market;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;
import java.util.stream.Collectors;
import java.util.stream.StreamSupport;

import com.quantconnect.lean.MarketDataType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.BaseData;


/**
 * Represents an entire chain of option contracts for a single underying security.
 * This type is <see cref="IEnumerable{OptionContract}"/>
 */
public class OptionChain extends BaseData implements Iterable<OptionContract> {
    
    private final Map<Class<?>,Map<Symbol,List<BaseData>>> auxiliaryData = new HashMap<>();

    private BaseData underlying;
    private Ticks ticks;
    private TradeBars tradeBars;
    private QuoteBars quoteBars;
    private OptionContracts contracts;
    private Set<Symbol> filteredContracts;

    
    /**
     * Gets the most recent trade information for the underlying. This may
     * be a <see cref="Tick"/> or a <see cref="TradeBar"/>
    */
    public BaseData getUnderlying() {
        return underlying;
    }

    public void setUnderlying( final BaseData underlying ) {
        this.underlying = underlying;
    }

    /**
     * Gets all ticks for every option contract in this chain, keyed by option symbol
     */
    public Ticks getTicks() {
        return ticks;
    }

    /**
     * Gets all trade bars for every option contract in this chain, keyed by option symbol
     */
    public TradeBars getTradeBars() {
        return tradeBars;
    }

    /**
     * Gets all quote bars for every option contract in this chain, keyed by option symbol
     */
    public QuoteBars getQuoteBars() {
        return quoteBars;
    }

    /**
     * Gets all contracts in the chain, keyed by option symbol
     */
    public OptionContracts getContracts() {
        return contracts;
    }

    /**
     * Gets the set of symbols that passed the <see cref="Option.ContractFilter"/>
     */
    public Set<Symbol> getFilteredContracts() {
        return filteredContracts;
    }

    /**
     * Initializes a new default instance of the <see cref="OptionChain"/> class
     */
    private OptionChain() {
        setDataType( MarketDataType.OptionChain );
    }

    /**
     * Initializes a new instance of the <see cref="OptionChain"/> class
     * @param canonicalOptionSymbol The symbol for this chain.
     * @param time The time of this chain
     */
    public OptionChain( final Symbol canonicalOptionSymbol, final LocalDateTime time ) {
        setTime( time );
        setSymbol( canonicalOptionSymbol );
        setDataType( MarketDataType.OptionChain );
        ticks = new Ticks();
        tradeBars = new TradeBars();
        quoteBars = new QuoteBars();
        contracts = new OptionContracts();
        filteredContracts = new HashSet<>();
    }

    /**
     * Initializes a new instance of the <see cref="OptionChain"/> class
     * @param canonicalOptionSymbol The symbol for this chain.
     * @param time The time of this chain
     * @param underlying The most recent underlying trade data
     * @param trades All trade data for the entire option chain
     * @param quotes All quote data for the entire option chain
     * @param contracts All contrains for this option chain
     */
    public OptionChain( final Symbol canonicalOptionSymbol, final LocalDateTime time, final BaseData underlying, final Iterable<BaseData> trades, final Iterable<BaseData> quotes,
            final Iterable<OptionContract> contracts, final Iterable<Symbol> filteredContracts ) {
        setTime( time );
        setSymbol( canonicalOptionSymbol );
        setDataType( MarketDataType.OptionChain );
        this.underlying = underlying;
        this.filteredContracts = StreamSupport.stream( filteredContracts.spliterator(), false ).collect( Collectors.toCollection( HashSet::new ) );

        ticks = new Ticks();
        tradeBars = new TradeBars();
        quoteBars = new QuoteBars();
        this.contracts = new OptionContracts();

        for( final BaseData trade : trades ) {
            if( trade instanceof Tick ) {
                final Tick tick = (Tick)trade;
                ticks.computeIfAbsent( tick.getSymbol(), s -> new ArrayList<>() ).add( tick );
                continue;
            }
            
            if( trade instanceof TradeBar ) {
                final TradeBar bar = (TradeBar)trade;
                tradeBars.put( trade.getSymbol(), bar );
            }
        }

        for( final BaseData quote : quotes ) {
            if( quote instanceof Tick ) {
                final Tick tick = (Tick)quote;
                ticks.computeIfAbsent( tick.getSymbol(), s -> new ArrayList<>() ).add( tick );
                continue;
            }
            
            if( quote instanceof QuoteBar ) {
                final QuoteBar bar = (QuoteBar)quote;
                quoteBars.put( quote.getSymbol(), bar );
            }
        }

        for( final OptionContract contract : contracts )
            this.contracts.put( contract.getSymbol(), contract );
    }

    /**
     * Gets the auxiliary data with the specified type and symbol
     * <typeparam name="T The type of auxiliary data</typeparam>
     * @param symbol The symbol of the auxiliary data
     * @returns The last auxiliary data with the specified type and symbol
     */
    @SuppressWarnings("unchecked")
    public <T> T getAux( final Symbol symbol, final Class<? extends T> type ) {
        List<BaseData> list;
        Map<Symbol,List<BaseData>> dictionary;
        if( (dictionary = auxiliaryData.get( type )) == null || (list = dictionary.get( symbol )) == null )
            return null;
        
        final int count = list.size();
        return count > 0 ? (T)list.get( count-1 ) : null;
    }

    /**
     * Gets all auxiliary data of the specified type as a dictionary keyed by symbol
     * <typeparam name="T The type of auxiliary data</typeparam>
     * @returns A dictionary containing all auxiliary data of the specified type
     */
    public <T> DataDictionary<T> getAux( final Class<? extends T> type ) {
        final Map<Symbol,List<BaseData>> d = auxiliaryData.get( type );
        if( d == null )
            return new DataDictionary<>();
        
        final DataDictionary<T> dictionary = new DataDictionary<>();
        for( final Entry<Symbol,List<BaseData>> kvp : d.entrySet() ) {
            final List<BaseData> list = kvp.getValue();
            final int count = list.size();
            @SuppressWarnings("unchecked")
            final T item = count > 0 ? (T)list.get( count-1 ) : null;
            if( item != null )
                dictionary.put( kvp.getKey(), item );
        }
        return dictionary;
    }

    /**
     * Gets all auxiliary data of the specified type as a dictionary keyed by symbol
     * <typeparam name="T The type of auxiliary data</typeparam>
     * @returns A dictionary containing all auxiliary data of the specified type
     */
    public <T> Map<Symbol,List<BaseData>> getAuxList( final Class<? extends T> type ) {
        final Map<Symbol, List<BaseData>> dictionary = auxiliaryData.get( type );
        if( dictionary == null )
            return new HashMap<>();
        
        return dictionary;
    }

    /**
     * Gets a list of auxiliary data with the specified type and symbol
     * <typeparam name="T The type of auxiliary data</typeparam>
     * @param symbol The symbol of the auxiliary data
     * @returns The list of auxiliary data with the specified type and symbol
     */
    @SuppressWarnings("unchecked")
    public <T> List<T> getAuxList( final Symbol symbol, final Class<? extends T> type ) {
        List<BaseData> list;
        Map<Symbol, List<BaseData>> dictionary = null;
        if( (dictionary = auxiliaryData.get( type )) == null || (list = dictionary.get( symbol )) == null )
            return new ArrayList<>();
        
        return (List<T>)list;
    }

    /**
     * Returns an enumerator that iterates through the collection.
     * @returns An enumerator that can be used to iterate through the collection.
     */
    @Override
    public Iterator<OptionContract> iterator() {
        return contracts.values().iterator();
    }

    /**
     * Return a new instance clone of this object, used in fill forward
     * @returns A clone of the current object
     */
    public @Override BaseData clone() {
        final OptionChain optionChain = new OptionChain();
        optionChain.underlying = underlying;
        optionChain.ticks = ticks;
        optionChain.contracts = contracts;
        optionChain.quoteBars = quoteBars;
        optionChain.tradeBars = tradeBars;
        optionChain.filteredContracts = filteredContracts;
        optionChain.setSymbol( getSymbol() );
        optionChain.setTime( getTime() );
        optionChain.setDataType( getDataType() );
        optionChain.setValue( getValue() );
        return optionChain;
    }

    /**
     * Adds the specified auxiliary data to this option chain
     * @param baseData The auxiliary data to be added
     */
    public void addAuxData( final BaseData baseData ) {
        final Class<? extends BaseData> type = baseData.getClass();
        final Map<Symbol,List<BaseData>> dictionary = auxiliaryData.computeIfAbsent( type, c -> new HashMap<>() );
        final List<BaseData> list = dictionary.computeIfAbsent( baseData.getSymbol(), s -> new ArrayList<>() );
        list.add( baseData );
    }
}
