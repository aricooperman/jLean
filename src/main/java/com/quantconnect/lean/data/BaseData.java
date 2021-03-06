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
*/

package com.quantconnect.lean.data;

import java.math.BigDecimal;
import java.net.InetAddress;
import java.net.URI;
import java.net.UnknownHostException;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.time.LocalDate;
import java.time.LocalDateTime;

import com.quantconnect.lean.DataFeedEndpoint;
import com.quantconnect.lean.MarketDataType;
import com.quantconnect.lean.SubscriptionTransportMedium;
import com.quantconnect.lean.Symbol;


/// Abstract base data class of QuantConnect. It is intended to be extended to define 
/// generic user customizable data types while at the same time implementing the basics of data where possible
public abstract class BaseData implements IBaseData {
    
    private MarketDataType dataType = MarketDataType.Base;
    private LocalDateTime time;
    private Symbol symbol = Symbol.EMPTY;
    private BigDecimal value;
    private boolean isFillForward;

    /// Market Data Class of this data - does it come in individual price packets or is it grouped into OHLC.
    /// <remarks>Data is classed into two categories - streams of instantaneous prices and groups of OHLC data.</remarks>
    public MarketDataType getDataType() {
        return dataType;
    }
    
    public void setDataType( MarketDataType value ) {
        dataType = value;
    }

    /// True if this is a fill forward piece of data
    public boolean isFillForward() {
        return isFillForward;
    }

    /// Current time marker of this data packet.
    /// <remarks>All data is timeseries based.</remarks>
    public LocalDateTime getTime() {
        return time;
    }
    
    public void setTime( LocalDateTime value ) {
        time = value;
    }

    /// The end time of this data. Some data covers spans (trade bars) and as such we want
    /// to know the entire time span covered
    public LocalDateTime getEndTime() {
        return time;
    }
    
    public void setEndTime( LocalDateTime value ) {
        time = value;
    }
    
    /// Symbol representation for underlying Security
    public Symbol getSymbol() {
        return symbol;
    }
    
    public void setSymbol( Symbol value ) {
        symbol = value;
    }

    /// Value representation of this data packet. All data requires a representative value for this moment in time.
    /// For streams of data this is the price now, for OHLC packets this is the closing price.
    public BigDecimal getValue() {
        return value;
    }
    
    public void setValue( BigDecimal value ) {
        this.value = value;
    }

    /// As this is a backtesting platform we'll provide an alias of value as price.
    public BigDecimal getPrice() {
        return value;
    }

    /// Constructor for initialising the dase data class
    public BaseData() { 
        //Empty constructor required for fast-reflection initialization
    }


    /// <summary>
    /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
    /// each time it is called. The returned object is assumed to be time stamped in the config.ExchangeTimeZone.
    /// </summary>
    /// <param name="config">Subscription data config setup object
    /// <param name="line">Line of the source document
    /// <param name="date">Date of the requested data
    /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode
    /// <returns>Instance of the T:BaseData object generated by this line of the CSV</returns>
    public BaseData reader( SubscriptionDataConfig config, String line, LocalDate date, boolean isLiveMode ) {
        // stub implementation to prevent compile errors in user algorithms
        final DataFeedEndpoint dataFeed = isLiveMode ? DataFeedEndpoint.LiveTrading : DataFeedEndpoint.Backtesting;
        return reader( config, line, date, dataFeed );
    }

    /**
     *  Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
    /// each time it is called. 
    /// </summary>
    /// <remarks>OBSOLETE:: This implementation is added for backward/forward compatibility purposes. This function is no longer called by the LEAN engine.</remarks>
    /// <param name="config">Subscription data config setup object
    /// <param name="line">Line of the source document
    /// <param name="date">Date of the requested data
    /// <param name="datafeed">Type of datafeed we're requesting - a live or backtest feed.
    /// <returns>Instance of the T:BaseData object generated by this line of the CSV</returns>
    @Deprecated("Reader(SubscriptionDataConfig, string, DateTime, DataFeedEndpoint) method has been made obsolete, use Reader(SubscriptionDataConfig, string, DateTime, bool) instead.")]
     */
    public BaseData reader( SubscriptionDataConfig config, String line, LocalDate date, DataFeedEndpoint datafeed ) {
        throw new UnsupportedOperationException( "Please implement Reader(SubscriptionDataConfig, string, DateTime, bool) on your custom data type: " + getClass().getName() );
    }

    /// <summary>
    /// Return the URL string source of the file. This will be converted to a stream 
    /// </summary>
    /// <param name="config">Configuration object
    /// <param name="date">Date of this source file
    /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode
    /// <returns>String URL of source file.</returns>
    public SubscriptionDataSource getSource(SubscriptionDataConfig config, LocalDate date, boolean isLiveMode ) {
        // stub implementation to prevent compile errors in user algorithms
        final DataFeedEndpoint dataFeed = isLiveMode ? DataFeedEndpoint.LiveTrading : DataFeedEndpoint.Backtesting;
        final String source = getSource( config, date, dataFeed );

        if( isLiveMode ) {
            // live trading by default always gets a rest endpoint
            return new SubscriptionDataSource( Paths.get( source ), SubscriptionTransportMedium.Rest );
        }
        
        // construct a uri to determine if we have a local or remote file
        final URI uri = URI.create( source );
        try {
            if( uri.isAbsolute() && !InetAddress.getByName( uri.getHost() ).isLoopbackAddress() )
                return new SubscriptionDataSource( Paths.get( source ), SubscriptionTransportMedium.RemoteFile );
        }
        catch( UnknownHostException e ) { }
            
        return new SubscriptionDataSource( Paths.get( source ), SubscriptionTransportMedium.LocalFile );
    }
    

    /**
     *  Return the URL string source of the file. This will be converted to a stream 
    /// <remarks>OBSOLETE:: This implementation is added for backward/forward compatibility purposes. This function is no longer called by the LEAN engine.</remarks>
    /// <param name="config">Configuration object
    /// <param name="date">Date of this source file
    /// <param name="datafeed">Type of datafeed we're reqesting - backtest or live
    /// <returns>String URL of source file.</returns>
    @deprecated[Obsolete("GetSource(SubscriptionDataConfig, DateTime, DataFeedEndpoint) method has been made obsolete, use GetSource(SubscriptionDataConfig, DateTime, bool) instead.")]
     */
    public String getSource( SubscriptionDataConfig config, LocalDate date, DataFeedEndpoint datafeed ) {
        throw new UnsupportedOperationException( "Please implement GetSource(SubscriptionDataConfig, DateTime, bool) on your custom data type: " + getClass().getName() );
    }

    /// Return the URL String source of the file. This will be converted to a stream 
    /// <param name="config Configuration object
    /// <param name="date Date of this source file
    /// <param name="isLiveMode true if we're in live mode, false for backtesting mode
    /// <returns>String URL of source file.</returns>
    protected SubscriptionDataSource createSubscriptionDataSource( Path source, boolean isLiveMode ) {
        if( isLiveMode )
            // live trading by default always gets a rest endpoint
            return new SubscriptionDataSource( source, SubscriptionTransportMedium.Rest );
        
        // construct a uri to determine if we have a local or remote file
        try {
            final URI uri = source.toUri(); //, UriKind.RelativeOrAbsolute);
            if( uri.isAbsolute() && !InetAddress.getByName( uri.getHost() ).isLoopbackAddress() )
                return new SubscriptionDataSource( source, SubscriptionTransportMedium.RemoteFile );
        }
        catch( Exception e ) { }
            
        return new SubscriptionDataSource( source, SubscriptionTransportMedium.LocalFile );
    }

    /// Updates this base data with a new trade
    /// <param name="lastTrade The price of the last trade
    /// <param name="tradeSize The quantity traded
    public void updateTrade( BigDecimal lastTrade, long tradeSize ) {
        update( lastTrade, BigDecimal.ZERO, BigDecimal.ZERO, BigDecimal.valueOf( tradeSize ), BigDecimal.ZERO, BigDecimal.ZERO );
    }

    /// Updates this base data with new quote information
    /// <param name="bidPrice The current bid price
    /// <param name="bidSize The current bid size
    /// <param name="askPrice The current ask price
    /// <param name="askSize The current ask size
    public void updateQuote( BigDecimal bidPrice, long bidSize, BigDecimal askPrice, long askSize ) {
        update( BigDecimal.ZERO, bidPrice, askPrice, BigDecimal.ZERO, BigDecimal.valueOf( bidSize ), BigDecimal.valueOf( askSize ) );
    }

    /// Updates this base data with the new quote bid information
    /// <param name="bidPrice The current bid price
    /// <param name="bidSize The current bid size
    public void updateBid( BigDecimal bidPrice, long bidSize ) {
        update( BigDecimal.ZERO, bidPrice, BigDecimal.ZERO, BigDecimal.ZERO, BigDecimal.valueOf( bidSize ), BigDecimal.ZERO );
    }

    /// Updates this base data with the new quote ask information
    /// <param name="askPrice The current ask price
    /// <param name="askSize The current ask size
    public void updateAsk( BigDecimal askPrice, long askSize ) {
        update( BigDecimal.ZERO, BigDecimal.ZERO, askPrice, BigDecimal.ZERO, BigDecimal.ZERO, BigDecimal.valueOf( askSize ) );
    }

    /// Update routine to build a bar/tick from a data update. 
    /// <param name="lastTrade The last trade price
    /// <param name="bidPrice Current bid price
    /// <param name="askPrice Current asking price
    /// <param name="volume Volume of this trade
    /// <param name="bidSize The size of the current bid, if available
    /// <param name="askSize The size of the current ask, if available
    public void update( BigDecimal lastTrade, BigDecimal bidPrice, BigDecimal askPrice, BigDecimal volume, BigDecimal bidSize, BigDecimal askSize ) {
        value = lastTrade;
    }

    /// Return a new instance clone of this object, used in fill forward
    /// <remarks>
    /// This base implementation uses reflection to copy all public fields and properties
    /// </remarks>
    /// <param name="fillForward True if this is a fill forward clone
    /// <returns>A clone of the current object</returns>
    public BaseData clone( boolean fillForward ) {
        final BaseData clone = clone();
        clone.isFillForward = fillForward;
        return clone;
    }

    /// Return a new instance clone of this object, used in fill forward
    /// <remarks>
    /// This base implementation uses reflection to copy all public fields and properties
    /// </remarks>
    /// <returns>A clone of the current object</returns>
    public BaseData clone() {
        try {
            return (BaseData) super.clone();
        }
        catch( CloneNotSupportedException e ) { 
            throw new RuntimeException( e );
        }
    }

    /// Formats a String with the symbol and value.
    /// <returns>string - a String formatted as SPY: 167.753</returns>
    public String toString() {
        return String.format( "%s: %s", symbol, value );
    }
}
