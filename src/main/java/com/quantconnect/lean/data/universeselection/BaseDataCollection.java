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
import java.util.ArrayList;
import java.util.List;

import com.google.common.collect.Lists;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.BaseData;

/**
 * This type exists for transport of data as a single packet
*/
public class BaseDataCollection extends BaseData {
    
    private LocalDateTime endTime;
    private List<BaseData> data;

    /**
     * Gets the data list
     */
    public final List<BaseData> getData() {
        return data;
    }

    public void setData( final List<BaseData> data ) {
        this.data = data;
    }

    /**
     * Gets or sets the end time of this data
     */
    @Override
    public LocalDateTime getEndTime() {
        return endTime;
    }
    
    @Override
    public void setEndTime( final LocalDateTime endTime ) {
        this.endTime = endTime;
    }
    
    /**
     * Initializes a new default instance of the <see cref="BaseDataCollection"/> c;ass
     */
    public BaseDataCollection() {
        this( LocalDateTime.MIN, null );
    }

    /**
     * Initializes a new instance of the <see cref="BaseDataCollection"/> class
     * @param time The time of this data
     * @param symbol A common identifier for all data in this packet
     * @param data The data to add to this collection
     */
    public BaseDataCollection( final LocalDateTime time, final Symbol symbol ) {
        this( time, symbol, null );
    }
    
    
    /**
     * Initializes a new instance of the <see cref="BaseDataCollection"/> class
     * @param time The time of this data
     * @param symbol A common identifier for all data in this packet
     * @param data The data to add to this collection
     */
    public BaseDataCollection( final LocalDateTime time, final Symbol symbol, final Iterable<BaseData> data ) {
        this( time, time, symbol, data );
    }

    /**
     * Initializes a new instance of the <see cref="BaseDataCollection"/> class
     * @param time The start time of this data
     * @param endTime The end time of this data
     * @param symbol A common identifier for all data in this packet
     * @param data The data to add to this collection
     */
    public BaseDataCollection( final LocalDateTime time, final LocalDateTime endTime, final Symbol symbol ) {
        this( time, endTime, symbol, null );
    }
    
    /**
     * Initializes a new instance of the <see cref="BaseDataCollection"/> class
     * @param time The start time of this data
     * @param endTime The end time of this data
     * @param symbol A common identifier for all data in this packet
     * @param data The data to add to this collection
     */
    public BaseDataCollection( final LocalDateTime time, final LocalDateTime endTime, final Symbol symbol, final Iterable<BaseData> data ) {
        setSymbol( symbol );
        setTime( time );
        this.endTime = endTime;
        this.data = data != null ? Lists.newArrayList( data ) : new ArrayList<>();
    }

    /**
     * Return a new instance clone of this object, used in fill forward
     * This base implementation uses reflection to copy all public fields and properties
     * @returns A clone of the current object
     */
    @Override
    public BaseData clone() {
        return new BaseDataCollection( getTime(), endTime, getSymbol(), data );
    }
}
