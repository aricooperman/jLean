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

import com.quantconnect.lean.data.market.IBar;
import com.quantconnect.lean.util.LeanData;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.Duration;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.temporal.ChronoUnit;

import com.quantconnect.lean.Extensions;
import com.quantconnect.lean.Global.DateFormat;
import com.quantconnect.lean.Global.MarketDataType;
import com.quantconnect.lean.Global.Resolution;
import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.Global.SubscriptionTransportMedium;
import com.quantconnect.lean.Globals;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.FileFormat;
import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.data.SubscriptionDataSource;

/// QuoteBar class for second and minute resolution data: 
/// An OHLC implementation of the QuantConnect BaseData class with parameters for candles.
public class QuoteBar extends BaseData implements IBar {
    
    // scale factor used in QC equity/forex data files
    private static final BigDecimal SCALE_FACTOR = BigDecimal.ONE.divide( BigDecimal.valueOf( 10000 ), RoundingMode.HALF_EVEN );

    /// Average bid size
    private long lastBidSize;
    
    /// Average ask size
    private long lastAskSize;

    /// Bid OHLC
    private Bar bid;

    /// Ask OHLC
    private Bar ask;
    
    /// The period of this quote bar, (second, minute, daily, ect...)
    private Duration period;
    
    public long getLastBidSize() {
        return lastBidSize;
    }

    public void setLastBidSize( long lastBidSize ) {
        this.lastBidSize = lastBidSize;
    }

    public long getLastAskSize() {
        return lastAskSize;
    }

    public void setLastAskSize( long lastAskSize ) {
        this.lastAskSize = lastAskSize;
    }

    public Bar getBid() {
        return bid;
    }

    public void setBid( Bar bid ) {
        this.bid = bid;
    }

    public Bar getAsk() {
        return ask;
    }

    public void setAsk( Bar ask ) {
        this.ask = ask;
    }

    public Duration getPeriod() {
        return period;
    }

    public void setPeriod( Duration period ) {
        this.period = period;
    }

    /// Opening price of the bar: Defined as the price at the start of the time period.
    public BigDecimal getOpen() {
        if( bid != null && ask != null )
            return Extensions.midPrice( bid.getOpen(), ask.getOpen() );
        if( bid != null )
            return bid.getOpen();
        if( ask != null )
            return ask.getOpen();

        return BigDecimal.ZERO;
    }

    /// High price of the QuoteBar during the time period.
    public BigDecimal getHigh() {
        if( bid != null && ask != null )
            return Extensions.midPrice( bid.getHigh(), ask.getHigh() );
        if( bid != null )
            return bid.getHigh();
        if( ask != null )
            return ask.getHigh();
        
        return BigDecimal.ZERO;
    }

    /// Low price of the QuoteBar during the time period.
    public BigDecimal getLow() {
        if( bid != null && ask != null )
            return Extensions.midPrice( bid.getLow(), ask.getLow() );
        if( bid != null )
            return bid.getLow();
        if( ask != null )
            return ask.getLow();
        
        return BigDecimal.ZERO;
    }

    /// Closing price of the QuoteBar. Defined as the price at Start Time + TimeSpan.
    public BigDecimal getClose() {
        if( bid != null && ask != null )
            return Extensions.midPrice( bid.getClose(), ask.getClose() );
        if( bid != null )
            return bid.getClose();
        if( ask != null )
            return ask.getClose();
        
        return getValue();
    }

    /// The closing time of this bar, computed via the Time and Period
    @Override
    public LocalDateTime getEndTime() {
        return getTime().plus( period );
    }
    
    public void setEndTime( LocalDateTime value ) {
        period = Duration.between( getTime(), value );
    }

    /// Default initializer to setup an empty quotebar.
    public QuoteBar() {
        setSymbol( Symbol.EMPTY );
        setTime( LocalDateTime.now() );
        setValue( BigDecimal.ZERO );
        setDataType( MarketDataType.QuoteBar );
        bid = new Bar();
        ask = new Bar();
        period = Duration.ofMinutes( 1 );
    }

    /// Initialize Quote Bar with Bid(OHLC) and Ask(OHLC) Values:
    /// <param name="time">DateTime Timestamp of the bar</param>
    /// <param name="symbol">Market MarketType Symbol</param>
    /// <param name="bid">Bid OLHC bar</param>
    /// <param name="lastBidSize">Average bid size over period</param>
    /// <param name="ask">Ask OLHC bar</param>
    /// <param name="lastAskSize">Average ask size over period</param>
    /// <param name="period">The period of this bar, specify null for default of 1 minute</param>
    public QuoteBar( LocalDateTime time, Symbol symbol, IBar bid, long lastBidSize, IBar ask, long lastAskSize ) {
        this( time, symbol, bid, lastBidSize, ask, lastAskSize, null );
    }
    
    public QuoteBar( LocalDateTime time, Symbol symbol, IBar bid, long lastBidSize, IBar ask, long lastAskSize, Duration period ) {
        setSymbol( symbol );
        setTime( time );
        bid = bid == null ? null : new Bar( bid.getOpen(), bid.getHigh(), bid.getLow(), bid.getClose() );
        ask = ask == null ? null : new Bar( ask.getOpen(), ask.getHigh(), ask.getLow(), ask.getClose() );
        if( bid != null ) 
            this.lastBidSize = lastBidSize;
        if( ask != null ) 
            this.lastAskSize = lastAskSize;
        setValue( getClose() );
        period = period != null ? period : Duration.ofMinutes( 1 );
        setDataType( MarketDataType.QuoteBar );
    }

    /// Update the quotebar - build the bar from this pricing information:
    /// <param name="lastTrade">The last trade price</param>
    /// <param name="bidPrice">Current bid price</param>
    /// <param name="askPrice">Current asking price</param>
    /// <param name="volume">Volume of this trade</param>
    /// <param name="bidSize">The size of the current bid, if available, if not, pass 0</param>
    /// <param name="askSize">The size of the current ask, if available, if not, pass 0</param>
    @Override
    public void update( BigDecimal lastTrade, BigDecimal bidPrice, BigDecimal askPrice, BigDecimal volume, BigDecimal bidSize, BigDecimal askSize ) {
        // update our bid and ask bars - handle null values, this is to give good values for midpoint OHLC
        if( bid == null && bidPrice.signum() != 0 ) 
            bid = new Bar();
        if( bid != null ) 
            bid.update( bidPrice );

        if( ask == null && askPrice.signum() != 0 ) 
            ask = new Bar();
        if( ask != null ) 
            ask.update( askPrice );

        if( bidSize.signum() > 0 ) 
            lastBidSize = bidSize.longValue();
        
        if( askSize.signum() > 0 )
            lastAskSize = askSize.longValue();

        // be prepared for updates without trades
        if( lastTrade.signum() != 0 ) 
            setValue( lastTrade );
        else if( askPrice.signum() != 0 ) 
            setValue( askPrice );
        else if( bidPrice.signum() != 0 ) 
            setValue( bidPrice );
    }

    /// QuoteBar Reader: Fetch the data from the QC storage and feed it line by line into the engine.
    /// <param name="config">Symbols, Resolution, DataType, </param>
    /// <param name="line">Line from the data file requested</param>
    /// <param name="date">Date of this reader request</param>
    /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
    /// <returns>Enumerable iterator for returning each line of the required data.</returns>
    @Override
    public BaseData reader( SubscriptionDataConfig config, String line, LocalDate date, boolean isLiveMode ) {
        final QuoteBar quoteBar = new QuoteBar();
        quoteBar.period = config.increment;
        quoteBar.setSymbol( config.getSymbol() );

        final String[] csv = Extensions.toCsv( line, 10 );
        if( config.resolution == Resolution.Daily || config.resolution == Resolution.Hour )
            // hourly and daily have different time format, and can use slow, robust c# parser.
            quoteBar.setTime( Extensions.convertTo( LocalDateTime.parse( csv[0], DateFormat.TwelveCharacter ), config.dataTimeZone, config.exchangeTimeZone ) );
        else
            // Using custom "ToDecimal" conversion for speed on high resolution data.
            quoteBar.setTime( Extensions.convertTo( date.truncatedTo( ChronoUnit.DAYS ).plus( Integer.parseInt( csv[0] ), ChronoUnit.MILLIS ), config.dataTimeZone, config.exchangeTimeZone ) );

        // only create the bid if it exists in the file
        if( csv[1].length() != 0 || csv[2].length() != 0 || csv[3].length() != 0 || csv[4].length() != 0 ) {
            quoteBar.bid = new Bar();
            quoteBar.bid.setOpen( config.getNormalizedPrice( SCALE_FACTOR.multiply( new BigDecimal( csv[1] ) ) ) );
            quoteBar.bid.setHigh( config.getNormalizedPrice( SCALE_FACTOR.multiply( new BigDecimal( csv[2] ) ) ) );
            quoteBar.bid.setLow( config.getNormalizedPrice( SCALE_FACTOR.multiply( new BigDecimal( csv[3] ) ) ) );
            quoteBar.bid.setClose( config.getNormalizedPrice( SCALE_FACTOR.multiply( new BigDecimal( csv[4] ) ) ) );
            quoteBar.lastBidSize = Long.parseLong( csv[5] );
        }
        else
            quoteBar.bid = null;

        // only create the ask if it exists in the file
        if( csv[6].length() != 0 || csv[7].length() != 0 || csv[8].length() != 0 || csv[9].length() != 0 ) {
            quoteBar.ask = new Bar();
            quoteBar.bid.setOpen( config.getNormalizedPrice( SCALE_FACTOR.multiply( new BigDecimal( csv[6] ) ) ) );
            quoteBar.bid.setHigh( config.getNormalizedPrice( SCALE_FACTOR.multiply( new BigDecimal( csv[7] ) ) ) );
            quoteBar.bid.setLow( config.getNormalizedPrice( SCALE_FACTOR.multiply( new BigDecimal( csv[8] ) ) ) );
            quoteBar.bid.setClose( config.getNormalizedPrice( SCALE_FACTOR.multiply( new BigDecimal( csv[9] ) ) ) );
            quoteBar.lastAskSize = Long.parseLong( csv[10] );
        }
        else
            quoteBar.ask = null;

        quoteBar.setValue( quoteBar.getClose() );

        return quoteBar;
    }

    /// Get Source for Custom Data File
    /// >> What source file location would you prefer for each type of usage:
    /// <param name="config">Configuration object</param>
    /// <param name="date">Date of this source request if source spread across multiple files</param>
    /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
    /// <returns>String source location of the file</returns>
    @Override
    public SubscriptionDataSource getSource( SubscriptionDataConfig config, LocalDate date, boolean isLiveMode ) {
        if( isLiveMode)
            return new SubscriptionDataSource( null, SubscriptionTransportMedium.LocalFile );

        String source = LeanData.generateZipFilePath( Globals.getDataFolder(), config.getSymbol(), date, config.resolution, config.tickType );
        if( config.securityType == SecurityType.Option )
            source += "#" + LeanData.generateZipEntryName( config.getSymbol(), date, config.resolution, config.tickType );

        return new SubscriptionDataSource(source, SubscriptionTransportMedium.LocalFile, FileFormat.Csv );
    }

    /// Return a new instance clone of this quote bar, used in fill forward
    /// <returns>A clone of the current quote bar</returns>
    @Override
    public BaseData clone() {
        final QuoteBar quoteBar = new QuoteBar();
        quoteBar.ask = ask == null ? null : ask.clone();
        quoteBar.bid = bid == null ? null : bid.clone();
        quoteBar.lastAskSize = lastAskSize;
        quoteBar.lastBidSize = lastBidSize;
        quoteBar.setSymbol( getSymbol() );
        quoteBar.setTime( getTime() );
        quoteBar.period = period;
        quoteBar.setValue( getValue() );
        quoteBar.setDataType( getDataType() );
        
        return quoteBar;    
    }
}
