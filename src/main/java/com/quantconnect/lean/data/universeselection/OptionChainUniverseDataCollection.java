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


package com.quantconnect.lean.data.universeselection;

import java.time.LocalDateTime;
import java.util.HashSet;
import java.util.Set;

import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.BaseData;

/**
 * Defines the universe selection data type for <see cref="OptionChainUniverse"/>
*/
public class OptionChainUniverseDataCollection extends BaseDataCollection {

    private BaseData underlying;
    private Set<Symbol> filteredContracts;

    
    /**
     * The option chain's underlying price data
     */
    public BaseData getUnderlying() {
        return underlying;
    }
    
    public void setUnderlying( final BaseData underlying ) {
        this.underlying = underlying;
    }

    /**
     * Gets or sets the contracts selected by the universe
     */
    public final Set<Symbol> getFilteredContracts() {
        return filteredContracts;
    }

    public void setFilteredContracts( final Set<Symbol> filteredContracts ) {
        this.filteredContracts = filteredContracts;
    }

    /**
     * Initializes a new default instance of the <see cref="OptionChainUniverseDataCollection"/> c;ass
     */
    public OptionChainUniverseDataCollection() {
        this( LocalDateTime.MIN, null );
        filteredContracts = new HashSet<>();
    }

    /**
     * Initializes a new instance of the <see cref="OptionChainUniverseDataCollection"/> class
     * @param time The time of this data
     * @param symbol A common identifier for all data in this packet
     * @param data The data to add to this collection
     * @param underlying The option chain's underlying price data
     */
    public OptionChainUniverseDataCollection( final LocalDateTime time, final Symbol symbol ) {
        this( time, symbol, null, null );
    }
    
    /**
     * Initializes a new instance of the <see cref="OptionChainUniverseDataCollection"/> class
     * @param time The time of this data
     * @param symbol A common identifier for all data in this packet
     * @param data The data to add to this collection
     * @param underlying The option chain's underlying price data
     */
    public OptionChainUniverseDataCollection( final LocalDateTime time, final Symbol symbol, final Iterable<BaseData> data, final BaseData underlying ) {
        this( time, time, symbol, data, underlying );
    }

    /**
     * Initializes a new instance of the <see cref="OptionChainUniverseDataCollection"/> class
     * @param time The start time of this data
     * @param endTime The end time of this data
     * @param symbol A common identifier for all data in this packet
     * @param data The data to add to this collection
     * @param underlying The option chain's underlying price data
     */
    public OptionChainUniverseDataCollection( final LocalDateTime time, final LocalDateTime endTime, final Symbol symbol ) {
        this( time, endTime, symbol, null, null );
    }
        
    
    /**
     * Initializes a new instance of the <see cref="OptionChainUniverseDataCollection"/> class
     * @param time The start time of this data
     * @param endTime The end time of this data
     * @param symbol A common identifier for all data in this packet
     * @param data The data to add to this collection
     * @param underlying The option chain's underlying price data
     */
    public OptionChainUniverseDataCollection( final LocalDateTime time, final LocalDateTime endTime, final Symbol symbol, final Iterable<BaseData> data, final BaseData underlying ) {
        super( time, endTime, symbol, data );
        this.underlying = underlying;
    }

    /**
     * Return a new instance clone of this object, used in fill forward
     * This base implementation uses reflection to copy all public fields and properties
     * @returns A clone of the current object
     */
    @Override
    public BaseData clone() {
        final OptionChainUniverseDataCollection clone = new OptionChainUniverseDataCollection();
        clone.setUnderlying( getUnderlying() );
        clone.setSymbol( getSymbol() );
        clone.setTime( getTime() );
        clone.setEndTime( getEndTime() );
        clone.setData( getData() );
        clone.setDataType( getDataType() );
        clone.filteredContracts = filteredContracts;
            
        return clone;
    }
}
