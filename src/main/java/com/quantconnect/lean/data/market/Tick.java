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
import java.math.RoundingMode;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.temporal.ChronoUnit;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.quantconnect.lean.Extensions;
import com.quantconnect.lean.Global.DateFormat;
import com.quantconnect.lean.Global.MarketDataType;
import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.Global.SubscriptionTransportMedium;
import com.quantconnect.lean.Global.TickType;
import com.quantconnect.lean.Globals;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.FileFormat;
import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.data.SubscriptionDataSource;
import com.quantconnect.lean.util.LeanData;

/// Tick class is the base representation for tick data. It is grouped into a Ticks object
/// which implements IDictionary and passed into an OnData event handler.
public class Tick extends BaseData {

    private static final BigDecimal SCALE_FACTOR = BigDecimal.valueOf( 10000 );
    
    private final Logger log = LoggerFactory.getLogger( getClass() );

    /// Type of the Tick: Trade or Quote.
    public TickType tickType = TickType.Trade;

    /// Quantity exchanged in a trade.
    public int quantity = 0;

    /// exchange we are executing on. String short code expanded in the MarketCodes.US global dictionary
    public String exchange = "";

    /// Sale condition for the tick.
    public String saleCondition = "";

    /// boolean whether this is a suspicious tick
    public boolean suspicious = false;

    /// Bid Price for Tick
    /// <remarks>QuantConnect does not currently have quote data but was designed to handle ticks and quotes</remarks>
    public BigDecimal bidPrice = BigDecimal.ZERO;

    /// Asking price for the Tick quote.
    /// <remarks>QuantConnect does not currently have quote data but was designed to handle ticks and quotes</remarks>
    public BigDecimal askPrice = BigDecimal.ZERO;

    /// Alias for "Value" - the last sale for this asset.
    public BigDecimal getLastPrice() {
        return getValue();
    }

    /// Size of bid quote.
    public long bidSize = 0;

    /// Size of ask quote.
    public long askSize = 0;

    //In Base Class: Alias of Closing:
    //public BigDecimal Price;

    //Symbol of Asset.
    //In Base Class: public Symbol Symbol;

    //In Base Class: DateTime Of this TradeBar
    //public DateTime Time;

    /// Initialize tick class with a default constructor.
    public Tick() {
        setValue( BigDecimal.ZERO );
        setTime( LocalDateTime.now() );
        setDataType( MarketDataType.Tick );
        setSymbol( Symbol.EMPTY );
        this.tickType = TickType.Trade;
        this.quantity = 0;
        this.exchange = "";
        this.saleCondition = "";
        this.suspicious = false;
        this.bidSize = 0;
        this.askSize = 0;
    }

    /// Cloner constructor for fill forward engine implementation. Clone the original tick into this new tick:
    /// <param name="original">Original tick we're cloning</param>
    public Tick( Tick original )  {
        setSymbol( original.getSymbol() );
        setTime( original.getTime() );
        setValue( original.getValue() );
        setDataType( MarketDataType.Tick );
        bidPrice = original.bidPrice;
        askPrice = original.askPrice;
        exchange = original.exchange;
        saleCondition = original.saleCondition;
        quantity = original.quantity;
        suspicious = original.suspicious;
        tickType = original.tickType;
        bidSize = original.bidSize;
        askSize = original.askSize;
    }

    /// Constructor for a FOREX tick where there is no last sale price. The volume in FX is so high its rare to find FX trade data.
    /// To fake this the tick contains bid-ask prices and the last price is the midpoint.
    /// <param name="time">Full date and time</param>
    /// <param name="symbol">Underlying currency pair we're trading</param>
    /// <param name="bid">FX tick bid value</param>
    /// <param name="ask">FX tick ask value</param>
    public Tick( LocalDateTime time, Symbol symbol, BigDecimal bid, BigDecimal ask ) {
        setDataType( MarketDataType.Tick );
        setTime( time );
        setSymbol( symbol );
        setValue( Extensions.midPrice( bid, ask ) );
        tickType = TickType.Quote;
        bidPrice = bid;
        askPrice = ask;
    }


    /// Initializer for a last-trade equity tick with bid or ask prices. 
    /// <param name="time">Full date and time</param>
    /// <param name="symbol">Underlying equity security symbol</param>
    /// <param name="bid">Bid value</param>
    /// <param name="ask">Ask value</param>
    /// <param name="last">Last trade price</param>
    public Tick( LocalDateTime time, Symbol symbol, BigDecimal last, BigDecimal bid, BigDecimal ask ) {
        setDataType( MarketDataType.Tick );
        setTime( time );
        setSymbol( symbol );
        setValue( last );
        tickType = TickType.Quote;
        bidPrice = bid;
        askPrice = ask;
    }

    /// Constructor for QuantConnect FXCM Data source:
    /// <param name="symbol">Symbol for underlying asset</param>
    /// <param name="line">CSV line of data from FXCM</param>
    public Tick( Symbol symbol, String line ) {
        final String[] csv = line.split( "," );
        setDataType( MarketDataType.Tick );
        setSymbol( symbol );
        setTime( LocalDateTime.parse( csv[0], DateFormat.Forex ) );
        setValue( Extensions.midPrice( bidPrice, askPrice ) );
        tickType = TickType.Quote;
        bidPrice = new BigDecimal( csv[1] );
        askPrice = new BigDecimal( csv[2] );
    }

    /// Constructor for QuantConnect tick data
    /// <param name="symbol">Symbol for underlying asset</param>
    /// <param name="line">CSV line of data from QC tick csv</param>
    /// <param name="baseDate">The base date of the tick</param>
    public Tick( Symbol symbol, String line, LocalDateTime baseDate ) {
        final String[] csv = line.split( "," );
        setDataType( MarketDataType.Tick );
        setSymbol( symbol );
        setTime( baseDate.truncatedTo( ChronoUnit.DAYS ).plus( Integer.parseInt( csv[0] ), ChronoUnit.MILLIS ) );
        setValue( (new BigDecimal( csv[1] )).divide( SCALE_FACTOR, RoundingMode.HALF_UP ) );
        tickType = TickType.Trade;
        quantity = Integer.parseInt( csv[2] );
        exchange = csv[3].trim();
        saleCondition = csv[4];
        suspicious = Integer.parseInt( csv[5] ) == 1;
    }

    /// Parse a tick data line from quantconnect zip source files.
    /// <param name="line">CSV source line of the compressed source</param>
    /// <param name="date">Base date for the tick (ticks date is stored as int milliseconds since midnight)</param>
    /// <param name="config">Subscription configuration object</param>
    public Tick( SubscriptionDataConfig config, String line, LocalDateTime date ) {
        try {
            setDataType( MarketDataType.Tick );

            // Which security type is this data feed:
            String[] csv;
            switch( config.securityType ) {
                case Equity:
                    csv = Extensions.toCsv( line, 6 );
                    setSymbol( config.getSymbol() );
                    setTime( Extensions.convertTo( date.truncatedTo( ChronoUnit.DAYS ).plus( Long.parseLong( csv[0] ), ChronoUnit.MILLIS ), config.dataTimeZone, config.exchangeTimeZone ) );
                    setValue( config.getNormalizedPrice( (new BigDecimal( csv[1] )).divide( SCALE_FACTOR, RoundingMode.HALF_UP ) ) );
                    tickType = TickType.Trade;
                    quantity = Integer.parseInt( csv[2] );
                    if( csv.length > 3 ) {
                        exchange = csv[3];
                        saleCondition = csv[4];
                        suspicious = (csv[5] == "1");
                    }
                    break;

                case Forex:
                case Cfd:
                    csv = Extensions.toCsv( line, 3 );
                    setSymbol( config.getSymbol() );
                    tickType = TickType.Quote;
                    setTime( Extensions.convertTo( date.truncatedTo( ChronoUnit.DAYS ).plus( Long.parseLong( csv[0] ), ChronoUnit.MILLIS ), config.dataTimeZone, config.exchangeTimeZone ) );
                    bidPrice = new BigDecimal( csv[1] );
                    askPrice = new BigDecimal( csv[2] );
                    setValue( Extensions.midPrice( bidPrice, askPrice ) );
                    break;

                case Option:
                    csv = Extensions.toCsv( line, 7 );
                    tickType = config.tickType;
                    setTime( Extensions.convertTo( date.truncatedTo( ChronoUnit.DAYS ).plus( Long.parseLong( csv[0] ), ChronoUnit.MILLIS ), config.dataTimeZone, config.exchangeTimeZone ) );
                    setSymbol( config.getSymbol() );

                    if( tickType == TickType.Trade ) {
                        setValue( config.getNormalizedPrice( scale( new BigDecimal( csv[1] ) ) ) );
                        quantity = Integer.parseInt( csv[2] );
                        exchange = csv[3];
                        saleCondition = csv[4];
                        suspicious = csv[5].equals( "1" );
                    }
                    else {
                        if( csv[1].length() != 0 ) {
                            bidPrice = config.getNormalizedPrice( scale( new BigDecimal( csv[1] ) ) );
                            bidSize = Integer.parseInt( csv[2] );
                        }
                        if( csv[3].length() != 0 ) {
                            askPrice = config.getNormalizedPrice( scale( new BigDecimal( csv[3] ) ) );
                            askSize = Integer.parseInt( csv[4] );
                        }
                        exchange = csv[5];
                        suspicious = csv[6] == "1";

                        if( bidPrice.signum() != 0 ) {
                            if( askPrice.signum() != 0 )
                                setValue( Extensions.midPrice( bidPrice, askPrice ) );
                            else
                                setValue( bidPrice );
                        }
                        else
                            setValue( askPrice );
                    }

                    break;
                
                case Base:
                case Commodity:
                case Future:
                default:
                    throw new UnsupportedOperationException( config.securityType + " is not supported" );
            }
        }
        catch( Exception err ) {
            log.error( "Unable to construct", err );
        }
    }

    private BigDecimal scale( BigDecimal value ) {
        return value.divide( SCALE_FACTOR, RoundingMode.HALF_UP );
    }

    /// Tick implementation of reader method: read a line of data from the source and convert it to a tick object.
    /// <param name="config">Subscription configuration object for algorithm</param>
    /// <param name="line">Line from the datafeed source</param>
    /// <param name="date">Date of this reader request</param>
    /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
    /// <returns>New Initialized tick</returns>
    @Override
    public BaseData reader( SubscriptionDataConfig config, String line, LocalDate date, boolean isLiveMode ) {
        if( isLiveMode )
            // currently ticks don't come through the reader function
            return new Tick();
        
        return new Tick( config, line, date );
    }
    
    /// Get source for tick data feed - not used with QuantConnect data sources implementation.
    /// <param name="config">Configuration object</param>
    /// <param name="date">Date of this source request if source spread across multiple files</param>
    /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
    /// <returns>String source location of the file to be opened with a stream</returns>
    @Override
    public SubscriptionDataSource getSource( SubscriptionDataConfig config, LocalDate date, boolean isLiveMode ) {
        if( isLiveMode )
            // Currently ticks aren't sourced through GetSource in live mode
            return new SubscriptionDataSource( null, SubscriptionTransportMedium.LocalFile );

        String source = LeanData.generateZipFilePath( Globals.getDataFolder(), config.getSymbol(), date, config.resolution, config.tickType );
        if( config.securityType == SecurityType.Option )
            source += "#" + LeanData.generateZipEntryName( config.getSymbol(), date, config.resolution, config.tickType );

        return new SubscriptionDataSource( source, SubscriptionTransportMedium.LocalFile, FileFormat.Csv );
    }


    /// Update the tick price information - not used.
    /// <param name="lastTrade">This trade price</param>
    /// <param name="bidPrice">Current bid price</param>
    /// <param name="askPrice">Current asking price</param>
    /// <param name="volume">Volume of this trade</param>
    /// <param name="bidSize">The size of the current bid, if available</param>
    /// <param name="askSize">The size of the current ask, if available</param>
   @Override
    public void update( BigDecimal lastTrade, BigDecimal bidPrice, BigDecimal askPrice, BigDecimal volume, BigDecimal bidSize, BigDecimal askSize ) {
        setValue( lastTrade );
        this.bidPrice = bidPrice;
        this.askPrice = askPrice;
        this.bidSize = bidSize.longValue();
        this.askSize = askSize.longValue();
        this.quantity = volume.intValue();
    }

    /// Check if tick contains valid data (either a trade, or a bid or ask)
    public boolean isValid() {
        return (tickType == TickType.Trade && getLastPrice().signum() > 0 && quantity > 0) ||
               (tickType == TickType.Quote && askPrice.signum() > 0 && askSize > 0) ||
               (tickType == TickType.Quote && bidPrice.signum() > 0 && bidSize > 0);
    }

    /// Clone implementation for tick class:
    /// <returns>New tick object clone of the current class values.</returns>
    @Override
    public BaseData clone() {
        return new Tick( this );
    }
} // End Tick Class:
