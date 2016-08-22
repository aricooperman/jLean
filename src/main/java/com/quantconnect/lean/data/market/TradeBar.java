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

import java.math.BigDecimal;
import java.nio.file.Path;
import java.time.Duration;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.temporal.ChronoUnit;
import java.util.concurrent.atomic.AtomicBoolean;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

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
import com.quantconnect.lean.util.LeanData;


/// TradeBar class for second and minute resolution data: 
/// An OHLC implementation of the QuantConnect BaseData class with parameters for candles.
public class TradeBar extends BaseData implements IBar {
    // scale factor used in QC equity/forex data files
    private static final BigDecimal _scaleFactor = BigDecimal.ONE.divide( BigDecimal.valueOf( 10000 ) );

    private final Logger log = LoggerFactory.getLogger( getClass() );

    private final AtomicBoolean initialized;
    private BigDecimal open;
    private BigDecimal high;
    private BigDecimal low;

    /// Volume:
    private long volume;
    
    /// The period of this trade bar, (second, minute, daily, ect...)
    private Duration period;
    
    public long getVolume() {
        return volume;
    }
    
    public void setVolume( long v ) {
        volume = v;
    }

    /// Opening price of the bar: Defined as the price at the start of the time period.
    public BigDecimal getOpen() {
        return open;
    }
    
    public void setOpen( BigDecimal value ) {
        initialize( value );
        open = value;
    }

    /// High price of the TradeBar during the time period.
    public BigDecimal getHigh() {
        return high;
    }
    
    public void setHigh( BigDecimal value ) {
        initialize( value );
        high = value;
    }

    /// Low price of the TradeBar during the time period.
    public BigDecimal getLow() {
        return low; 
    }
    
    public void setLow( BigDecimal value ) {
        initialize( value );
        low = value;
    }

    /// Closing price of the TradeBar. Defined as the price at Start Time + TimeSpan.
    public BigDecimal getClose() {
        return getValue();
    }
     
    public void setClose( BigDecimal value ) {
        initialize( value );
        setValue( value );
    }

    /// The closing time of this bar, computed via the Time and Period
    public LocalDateTime getEndTime() {
        return getTime().plus( period );
    }
    
    public void setEndTime( LocalDateTime value ) {
        period = Duration.between( getTime(), value ); 
    }

    
    public Duration getPeriod() {
        return period;
    }
    
    public void setPeriod( Duration value ) {
        this.period = value;
    }

    //In Base Class: Alias of Closing:
    //public BigDecimal Price;

    //Symbol of Asset.
    //In Base Class: public Symbol Symbol;

    //In Base Class: DateTime Of this TradeBar
    //public DateTime Time;

    /// Default initializer to setup an empty tradebar.
    public TradeBar() {
        setSymbol( Symbol.EMPTY );
        setDataType( MarketDataType.TradeBar );
        period = Duration.ofMinutes( 1 );
        initialized = new AtomicBoolean( false );
    }

    /// Cloner constructor for implementing fill forward. 
    /// Return a new instance with the same values as this original.
    /// <param name="original">Original tradebar object we seek to clone</param>
    public TradeBar( TradeBar original ) {
        setDataType( MarketDataType.TradeBar );
        setTime( original.getTime() );
        setSymbol( original.getSymbol() );
        setValue( original.getClose() );
        setClose( original.getClose() );
        this.open = original.open;
        this.high = original.high;
        this.low = original.low;
        this.volume = original.volume;
        this.period = original.period;
        this.initialized = new AtomicBoolean( true );
    }

    /// Initialize Trade Bar with OHLC Values:
    /// <param name="time">DateTime Timestamp of the bar</param>
    /// <param name="symbol">Market MarketType Symbol</param>
    /// <param name="open">Decimal Opening Price</param>
    /// <param name="high">Decimal High Price of this bar</param>
    /// <param name="low">Decimal Low Price of this bar</param>
    /// <param name="close">Decimal Close price of this bar</param>
    /// <param name="volume">Volume sum over day</param>
    /// <param name="period">The period of this bar, specify null for default of 1 minute</param>
    public TradeBar( LocalDateTime time, Symbol symbol, BigDecimal open, BigDecimal high, BigDecimal low, BigDecimal close, long volume ) {
        this( time, symbol, open, high, low, close, volume, null );
    }
    
    public TradeBar( LocalDateTime time, Symbol symbol, BigDecimal open, BigDecimal high, BigDecimal low, BigDecimal close, long volume, Duration period ) {
        setTime( time );
        setSymbol( symbol );
        setValue( close );
        setDataType( MarketDataType.TradeBar );
        setClose( close );
        this.open = open;
        this.high = high;
        this.low = low;
        this.volume = volume;
        this.period = period != null ? period : Duration.ofMinutes( 1 );
        this.initialized = new AtomicBoolean( true );
    }

    /// TradeBar Reader: Fetch the data from the QC storage and feed it line by line into the engine.
    /// <param name="config">Symbols, Resolution, DataType, </param>
    /// <param name="line">Line from the data file requested</param>
    /// <param name="date">Date of this reader request</param>
    /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
    /// <returns>Enumerable iterator for returning each line of the required data.</returns>
    @Override
    public BaseData reader( SubscriptionDataConfig config, String line, LocalDate date, boolean isLiveMode ) {
        //Handle end of file:
        if( line == null )
            return null;

        if( isLiveMode )
            return new TradeBar();

        try {
            switch( config.securityType ) {
                //Equity File Data Format:
                case Equity:
                    return parseEquity( config, line, date, TradeBar.class );
                //FOREX has a different data file format:
                case Forex:
                    return parseForex( config, line, date, TradeBar.class );
                case Cfd:
                    return parseCfd( config, line, date, TradeBar.class );
                case Option:
                    return parseOption( config, line, date, TradeBar.class );
                default:
                    break;
            }
        }
        catch( Exception err ) {
            log .error( "SecurityType: " + config.securityType + " Line: " + line, err );
        }

        // if we couldn't parse it above return a default instance
        final TradeBar tradeBar = new TradeBar();
        tradeBar.setSymbol( config.getSymbol() );
        tradeBar.period = config.increment;
        return tradeBar;
    }

    /// Parses the trade bar data line assuming QC data formats
    public static TradeBar parse( SubscriptionDataConfig config, String line, LocalDate baseDate ) {
        switch( config.securityType ) {
            case Equity:
                return parseEquity( config, line, baseDate );
            case Forex:
                return parseForex( config, line, baseDate );
            case Cfd:
                return parseCfd( config, line, baseDate );
            default:
                break;
        }

        return null;
    }

    /// Parses equity trade bar data into the specified tradebar type, useful for custom types with OHLCV data deriving from TradeBar
    /// <typeparam name="T">The requested output type, must derive from TradeBar</typeparam>
    /// <param name="config">Symbols, Resolution, DataType, </param>
    /// <param name="line">Line from the data file requested</param>
    /// <param name="date">Date of this reader request</param>
    /// <returns></returns>
    public static <T extends TradeBar> T parseEquity( SubscriptionDataConfig config, String line, LocalDate date, Class<T> clazz ) {
        T tradeBar;
        try {
            tradeBar = clazz.newInstance();
        }
        catch( InstantiationException | IllegalAccessException e ) {
            throw new RuntimeException( e );
        }

        tradeBar.setSymbol( config.getSymbol() );
        tradeBar.setPeriod( config.increment );

        final String[] csv = Extensions.toCsv( line, 6 );
        if( config.resolution == Resolution.Daily || config.resolution == Resolution.Hour )
            // hourly and daily have different time format, and can use slow, robust c# parser.
            tradeBar.setTime( Extensions.convertTo( LocalDateTime.parse( csv[0], DateFormat.TwelveCharacter ), config.dataTimeZone, config.exchangeTimeZone ) );
        else
            // Using custom "ToDecimal" conversion for speed on high resolution data.
            tradeBar.setTime( Extensions.convertTo( date.atStartOfDay().plus( Integer.parseInt( csv[0] ), ChronoUnit.MILLIS ), config.dataTimeZone, config.exchangeTimeZone ) );

        tradeBar.setOpen( config.getNormalizedPrice( _scaleFactor.multiply( new BigDecimal( csv[1] ) ) ) );
        tradeBar.setHigh( config.getNormalizedPrice( _scaleFactor.multiply( new BigDecimal( csv[2] ) ) ) );
        tradeBar.setLow( config.getNormalizedPrice( _scaleFactor.multiply( new BigDecimal( csv[3] ) ) ) );
        tradeBar.setClose( config.getNormalizedPrice( _scaleFactor.multiply( new BigDecimal( csv[4] ) ) ) );
        tradeBar.setVolume( Long.parseLong( csv[5] ) );

        return tradeBar;
    }

    /// Parses equity trade bar data into the specified tradebar type, useful for custom types with OHLCV data deriving from TradeBar
    /// <param name="config">Symbols, Resolution, DataType, </param>
    /// <param name="line">Line from the data file requested</param>
    /// <param name="date">Date of this reader request</param>
    /// <returns></returns>
    public static TradeBar parseEquity( SubscriptionDataConfig config, String line, LocalDate date ) {
        return parseEquity( config, line, date, TradeBar.class );
    }

    /// Parses forex trade bar data into the specified tradebar type, useful for custom types with OHLCV data deriving from TradeBar
    /// <typeparam name="T">The requested output type, must derive from TradeBar</typeparam>
    /// <param name="config">Symbols, Resolution, DataType, </param>
    /// <param name="line">Line from the data file requested</param>
    /// <param name="date">The base data used to compute the time of the bar since the line specifies a milliseconds since midnight</param>
    /// <returns></returns>
    public static <T extends TradeBar> T parseForex( SubscriptionDataConfig config, String line, LocalDate date, Class<T> clazz ) {
        T tradeBar;
        try {
            tradeBar = clazz.newInstance();
        }
        catch( InstantiationException | IllegalAccessException e ) {
            throw new RuntimeException( e );
        }

        tradeBar.setSymbol( config.getSymbol() );
        tradeBar.setPeriod( config.increment );

        final String[] csv = Extensions.toCsv( line, 5 );
        if( config.resolution == Resolution.Daily || config.resolution == Resolution.Hour )
            // hourly and daily have different time format, and can use slow, robust c# parser.
            tradeBar.setTime( Extensions.convertTo( LocalDateTime.parse( csv[0], DateFormat.TwelveCharacter ), config.dataTimeZone, config.exchangeTimeZone ) );
        else
            //Fast BigDecimal conversion
            tradeBar.setTime( Extensions.convertTo( date.atStartOfDay().plus( Integer.parseInt( csv[0] ), ChronoUnit.MILLIS ), 
                    config.dataTimeZone, config.exchangeTimeZone ) );

        tradeBar.setOpen( new BigDecimal( csv[1] ) );
        tradeBar.setHigh( new BigDecimal( csv[2] ) );
        tradeBar.setLow( new BigDecimal( csv[3] ) );
        tradeBar.setClose( new BigDecimal( csv[4] ) );

        return tradeBar;
    }

    /// Parses forex trade bar data into the specified tradebar type, useful for custom types with OHLCV data deriving from TradeBar
    /// <param name="config">Symbols, Resolution, DataType, </param>
    /// <param name="line">Line from the data file requested</param>
    /// <param name="date">The base data used to compute the time of the bar since the line specifies a milliseconds since midnight</param>
    /// <returns></returns>
    public static TradeBar parseForex( SubscriptionDataConfig config, String line, LocalDate date ) {
        return parseForex( config, line, date, TradeBar.class );
    }

    /// Parses CFD trade bar data into the specified tradebar type, useful for custom types with OHLCV data deriving from TradeBar
    /// <typeparam name="T">The requested output type, must derive from TradeBar</typeparam>
    /// <param name="config">Symbols, Resolution, DataType, </param>
    /// <param name="line">Line from the data file requested</param>
    /// <param name="date">The base data used to compute the time of the bar since the line specifies a milliseconds since midnight</param>
    /// <returns></returns>
    public static <T extends TradeBar> T parseCfd( SubscriptionDataConfig config, String line, LocalDate date, Class<T> clazz ) {
        // CFD has the same data format as Forex
        return parseForex( config, line, date, clazz );
    }

    /// <summary>
    /// Parses CFD trade bar data into the specified tradebar type, useful for custom types with OHLCV data deriving from TradeBar
    /// </summary>
    /// <param name="config">Symbols, Resolution, DataType, </param>
    /// <param name="line">Line from the data file requested</param>
    /// <param name="date">The base data used to compute the time of the bar since the line specifies a milliseconds since midnight</param>
    /// <returns></returns>
    public static TradeBar parseCfd( SubscriptionDataConfig config, String line, LocalDate date ) {
        return parseCfd( config, line, date, TradeBar.class );
    }

    /// Parses CFD trade bar data into the specified tradebar type, useful for custom types with OHLCV data deriving from TradeBar
    /// <typeparam name="T">The requested output type, must derive from TradeBar</typeparam>
    /// <param name="config">Symbols, Resolution, DataType, </param>
    /// <param name="line">Line from the data file requested</param>
    /// <param name="date">The base data used to compute the time of the bar since the line specifies a milliseconds since midnight</param>
    /// <returns></returns>
    public static <T extends TradeBar> T parseOption( SubscriptionDataConfig config, String line, LocalDate date, Class<T> clazz ) {
        T tradeBar;
        try {
            tradeBar = clazz.newInstance();
        }
        catch( InstantiationException | IllegalAccessException e ) {
            throw new RuntimeException( e );
        }
        
        tradeBar.setPeriod( config.increment );
        tradeBar.setSymbol( config.getSymbol() );

        final String[] csv = Extensions.toCsv( line, 6 );
        if( config.resolution == Resolution.Daily || config.resolution == Resolution.Hour )
            // hourly and daily have different time format, and can use slow, robust c# parser.
            tradeBar.setTime( Extensions.convertTo( LocalDateTime.parse( csv[0], DateFormat.TwelveCharacter ), config.dataTimeZone, config.exchangeTimeZone ) );
        else
            // Using custom "ToDecimal" conversion for speed on high resolution data.
            tradeBar.setTime( Extensions.convertTo( date.atStartOfDay().plus( Integer.parseInt( csv[0] ), ChronoUnit.MILLIS ), config.dataTimeZone, config.exchangeTimeZone ) );

        tradeBar.setOpen( config.getNormalizedPrice( _scaleFactor.multiply( new BigDecimal( csv[1] ) ) ) );
        tradeBar.setHigh( config.getNormalizedPrice( _scaleFactor.multiply( new BigDecimal( csv[2] ) ) ) );
        tradeBar.setLow( config.getNormalizedPrice( _scaleFactor.multiply( new BigDecimal( csv[3] ) ) ) );
        tradeBar.setClose( config.getNormalizedPrice( _scaleFactor.multiply( new BigDecimal( csv[4] ) ) ) );
        tradeBar.setVolume( Long.parseLong( csv[5] ) );

        return tradeBar;
    }

    /// Parses CFD trade bar data into the specified tradebar type, useful for custom types with OHLCV data deriving from TradeBar
    /// <param name="config">Symbols, Resolution, DataType, </param>
    /// <param name="line">Line from the data file requested</param>
    /// <param name="date">The base data used to compute the time of the bar since the line specifies a milliseconds since midnight</param>
    /// <returns></returns>
    public static TradeBar parseOption( SubscriptionDataConfig config, String line, LocalDate date ) {
        return parseOption( config, line, date, TradeBar.class );
    }

    /// Update the tradebar - build the bar from this pricing information:
    /// <param name="lastTrade">This trade price</param>
    /// <param name="bidPrice">Current bid price (not used) </param>
    /// <param name="askPrice">Current asking price (not used) </param>
    /// <param name="volume">Volume of this trade</param>
    /// <param name="bidSize">The size of the current bid, if available</param>
    /// <param name="askSize">The size of the current ask, if available</param>
    @Override
    public void update( BigDecimal lastTrade, BigDecimal bidPrice, BigDecimal askPrice, BigDecimal volume, BigDecimal bidSize, BigDecimal askSize ) {
        initialize( lastTrade );
        if( lastTrade.compareTo( high ) > 0 ) 
            high = lastTrade;
        
        if( lastTrade.compareTo( low ) < 0 ) 
            low = lastTrade;
        
        //Volume is the total summed volume of trades in this bar:
        this.volume += volume.intValue();
        //Always set the closing price;
        setClose( lastTrade );
    }

    /// <summary>
    /// Get Source for Custom Data File
    /// >> What source file location would you prefer for each type of usage:
    /// </summary>
    /// <param name="config">Configuration object</param>
    /// <param name="date">Date of this source request if source spread across multiple files</param>
    /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
    /// <returns>String source location of the file</returns>
    @Override
    public SubscriptionDataSource getSource( SubscriptionDataConfig config, LocalDate date, boolean isLiveMode ) {
        if( isLiveMode)
            return new SubscriptionDataSource( null, SubscriptionTransportMedium.LocalFile );

        Path source = LeanData.generateZipFilePath( Globals.getDataFolder(), config.getSymbol(), date, config.resolution, config.tickType );
        if( config.securityType == SecurityType.Option )
            source = source.resolve( "#" + LeanData.generateZipEntryName( config.getSymbol(), date, config.resolution, config.tickType ) );

        return new SubscriptionDataSource( source, SubscriptionTransportMedium.LocalFile, FileFormat.Csv );
    }

    /// Return a new instance clone of this object
    public BaseData clone() {
        return (BaseData)super.clone();
    }

    /// Initializes this bar with a first data point
    /// <param name="value">The seed value for this bar</param>
    private void initialize( BigDecimal value ) {
        if( initialized.compareAndSet( false, true ) ) {
            open = value;
            low = value;
            high = value;
        }
    }
}
