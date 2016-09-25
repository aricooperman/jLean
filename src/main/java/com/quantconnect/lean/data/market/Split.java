﻿/*
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

package com.quantconnect.lean.data.market;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;

import com.quantconnect.lean.MarketDataType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.data.SubscriptionDataSource;

/**
 * Split event from a security
 */
public class Split extends BaseData {
 
    private BigDecimal splitFactor;
    private BigDecimal referencePrice;
    
    /**
     * Initializes a new instance of the Split class
     */
    public Split() {
        setDataType( MarketDataType.Auxiliary );
    }

    /**
     * Initializes a new instance of the Split class
     * @param symbol The symbol
     * @param date The date
     * @param price The price at the time of the split
     * @param splitFactor The split factor to be applied to current holdings
     */
    public Split( Symbol symbol, LocalDateTime date, BigDecimal price, BigDecimal splitFactor ) {
        this();
        setSymbol( symbol );
        setTime( date );
        this.referencePrice = price;
        this.splitFactor = splitFactor;
    }

    /**
     * Gets the split factor
     */
    public BigDecimal getSplitFactor() {
        return splitFactor;
    }

    /**
     * Gets the price at which the split occurred
     */
    public BigDecimal getReferencePrice() {
        return referencePrice;
    }
    
    public void setReferencePrice( BigDecimal value ) {
        referencePrice = value;
    }

    /**
     * Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
     * each time it is called. 
     * @param config Subscription data config setup object
     * @param line Line of the source document
     * @param date Date of the requested data
     * @param isLiveMode true if we're in live mode, false for backtesting mode
     * @returns Instance of the T:BaseData object generated by this line of the CSV
     */
    @Override
    public BaseData reader( SubscriptionDataConfig config, String line, LocalDate date, boolean isLiveMode ) {
        // this is implemented in the SubscriptionDataReader.CheckForSplit
        throw new UnsupportedOperationException( "This method is not supposed to be called on the Split type.");
    }

    /**
     * Return the URL String source of the file. This will be converted to a stream 
     * @param config Configuration object
     * @param date Date of this source file
     * @param isLiveMode true if we're in live mode, false for backtesting mode
     * @returns String URL of source file.
     */
    @Override
    public SubscriptionDataSource getSource( SubscriptionDataConfig config, LocalDate date, boolean isLiveMode ) {
        // this data is derived from map files and factor files in backtesting
        throw new UnsupportedOperationException( "This method is not supposed to be called on the Split type.");
    }

    /**
     * Returns a <see cref="System.String"/> that represents the current <see cref="QuantConnect.Data.Market.split"/>.
     * @returns A <see cref="System.String"/> that represents the current <see cref="QuantConnect.Data.Market.split"/>.
     */
    @Override
    public String toString() {
        return String.format( "%1$s: %2$s", getSymbol(), splitFactor );
    }

    /**
     * Return a new instance clone of this object, used in fill forward
     * 
     * This base implementation uses reflection to copy all public fields and properties
     * 
     * @returns A clone of the current object
     */
    @Override
    public BaseData clone() {
        return new Split( getSymbol(), getTime(), getPrice(), splitFactor );
    }
}
