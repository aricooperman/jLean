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

package com.quantconnect.lean.util;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.time.LocalDate;
import java.time.temporal.ChronoField;
import java.util.Arrays;
import java.util.stream.Collectors;

import com.quantconnect.lean.Global.DateFormat;
import com.quantconnect.lean.Global.OptionRight;
import com.quantconnect.lean.Global.OptionStyle;
import com.quantconnect.lean.Global.Resolution;
import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.Global.TickType;
import com.quantconnect.lean.Market;
import com.quantconnect.lean.SecurityIdentifier;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.IBaseData;
import com.quantconnect.lean.data.market.IBar;
import com.quantconnect.lean.data.market.QuoteBar;
import com.quantconnect.lean.data.market.Tick;
import com.quantconnect.lean.data.market.TradeBar;


/// Provides methods for generating lean data file content
public class LeanData {
    
    private static final BigDecimal SCALE = BigDecimal.valueOf( 10000 );

    
    private LeanData() { }

    /// Converts the specified base data instance into a lean data file csv line
    public static String generateLine( IBaseData data, SecurityType securityType, Resolution resolution ) {
        final String milliseconds = Long.toString( data.getTime().toLocalTime().getLong( ChronoField.MILLI_OF_DAY ) );
        final String longTime = data.getTime().format( DateFormat.TwelveCharacter );

        Tick tick;
        TradeBar bar;
        TradeBar bigBar;
        switch( securityType ) {
            case Equity:
                switch( resolution ) {
                    case Tick:
                        tick = (Tick)data;
                        return toCsv( milliseconds, scale( tick.getLastPrice() ), tick.quantity, tick.exchange, tick.saleCondition, tick.suspicious ? "1" : "0");

                    case Minute:
                    case Second:
                        bar = (TradeBar)data;
                        return toCsv( milliseconds, scale( bar.getOpen() ), scale( bar.getHigh() ), scale( bar.getLow() ), scale( bar.getClose() ), bar.getVolume() );

                    case Hour:
                    case Daily:
                        bigBar = (TradeBar)data;
                        return toCsv( longTime, scale( bigBar.getOpen() ), scale( bigBar.getHigh() ), scale( bigBar.getLow() ), scale( bigBar.getClose() ), bigBar.getVolume());
                }
                break;

            case Forex:
            case Cfd:
                switch( resolution ) {
                    case Tick:
                        tick = (Tick)data;
                        return toCsv( milliseconds, tick.bidPrice, tick.askPrice );

                    case Second:
                    case Minute:
                        bar = (TradeBar)data;
                        return toCsv( milliseconds, bar.getOpen(), bar.getHigh(), bar.getLow(), bar.getClose() );

                    case Hour:
                    case Daily:
                        bigBar = (TradeBar)data;
                        return toCsv( longTime, bigBar.getOpen(), bigBar.getHigh(), bigBar.getLow(), bigBar.getClose() );
                }
                break;

            case Option:
//                final String putCall = data.getSymbol().getId().getOptionRight() == OptionRight.PUT ? "P" : "C";
                switch( resolution ) {
                    case Tick:
                        tick = (Tick)data;
                        if( tick.tickType == TickType.Trade )
                            return toCsv( milliseconds, scale( tick.getLastPrice() ), tick.quantity, tick.exchange, tick.saleCondition, tick.suspicious ? "1": "0" );
                        else if(tick.tickType == TickType.Quote )
                            return toCsv( milliseconds, scale( tick.bidPrice ), tick.bidSize, scale( tick.askPrice ), tick.askSize, tick.exchange, tick.suspicious ? "1" : "0" );
                        break;

                    case Second:
                    case Minute:
                        // option data can be quote or trade bars
                        if( data instanceof QuoteBar ) {
                            final QuoteBar quoteBar = (QuoteBar)data;
                            return toCsv( milliseconds, toCsv( quoteBar.getBid() ), quoteBar.getLastBidSize(), toCsv( quoteBar.getAsk() ), quoteBar.getLastAskSize() );
                        }
                        if( data instanceof TradeBar ) {
                            final TradeBar tradeBar = (TradeBar)data;
                            return toCsv( milliseconds, scale( tradeBar.getOpen() ), scale( tradeBar.getHigh() ), scale( tradeBar.getLow() ), 
                                    scale( tradeBar.getClose() ), tradeBar.getVolume() );
                        }
                        break;

                    case Hour:
                    case Daily:
                        // option data can be quote or trade bars
                        if( data instanceof QuoteBar ) {
                            final QuoteBar bigQuoteBar = (QuoteBar)data;
                            return toCsv( longTime, toCsv( bigQuoteBar.getBid() ), bigQuoteBar.getLastBidSize(), toCsv( bigQuoteBar.getAsk() ), bigQuoteBar.getLastAskSize() );
                        }
                        if( data instanceof TradeBar ) {
                            final TradeBar bigTradeBar = (TradeBar)data;
                            return toCsv( longTime, toCsv( bigTradeBar ), bigTradeBar.getVolume() );
                        }
                        break;

                    default:
                        throw new UnsupportedOperationException( "Resolution " + resolution + " is out of range" );
                }
                break;
                
            case Base:
            case Commodity:
            case Future:
            default:
                break;
        }

        throw new UnsupportedOperationException( "LeanData.generateLine has not yet been implemented for security type: " + securityType + " at resolution: " + resolution );
    }

    /// Generates the full zip file path rooted in the <paramref name="dataDirectory"/>
    public static Path generateZipFilePath( String dataDirectory, Symbol symbol, LocalDate date, Resolution resolution, TickType tickType ) {
        return Paths.get( dataDirectory ).resolve( generateRelativeZipFilePath( symbol, date, resolution, tickType ) );
    }

    /// Generates the full zip file path rooted in the <paramref name="dataDirectory"/>
    public static Path generateZipFilePath( String dataDirectory, String symbol, SecurityType securityType, String market, LocalDate date, Resolution resolution ) {
        return Paths.get( dataDirectory ).resolve( generateRelativeZipFilePath( symbol, securityType, market, date, resolution ) );
    }

    /// Generates the relative zip directory for the specified symbol/resolution
    public static Path generateRelativeZipFileDirectory( Symbol symbol, Resolution resolution ) {
        final boolean isHourOrDaily = resolution == Resolution.Hour || resolution == Resolution.Daily;
        SecurityType st = symbol.getId().getSecurityType();
        final String securityType = st.toString().toLowerCase();
        final String market = symbol.getId().getMarket().toLowerCase();
        final String res = resolution.toString().toLowerCase();
        final Path directory = Paths.get( securityType, market, res );
        switch( st ) {
            case Base:
            case Equity:
            case Forex:
            case Cfd:
                return !isHourOrDaily ? directory.resolve( symbol.getValue().toLowerCase() ) : directory;

            case Option:
                // options uses the underlying symbol for pathing
                return !isHourOrDaily ? directory.resolve( symbol.getId().getSymbol().toLowerCase() ) : directory;

            case Commodity:
            case Future:
            default:
                throw new UnsupportedOperationException( st + " is unsupported" );
        }
    }

    /// Generates the relative zip file path rooted in the /Data directory
    public static Path generateRelativeZipFilePath( Symbol symbol, LocalDate date, Resolution resolution, TickType tickType ) {
        return generateRelativeZipFileDirectory( symbol, resolution ).resolve( generateZipFileName( symbol, date, resolution, tickType ) );
    }

    /// Generates the relative zip file path rooted in the /Data directory
    public static Path generateRelativeZipFilePath( String symbol, SecurityType securityType, String market, LocalDate date, Resolution resolution ) {
        Path directory = Paths.get( securityType.toString().toLowerCase(), market.toLowerCase(), resolution.toString().toLowerCase() );
        if( resolution != Resolution.Daily && resolution != Resolution.Hour )
            directory = directory.resolve( symbol.toLowerCase() );

        return directory.resolve( generateZipFileName( symbol, securityType, date, resolution ) );
    }

    /// generate's the zip entry name to hold the specified data.
    public static String generateZipEntryName( Symbol symbol, LocalDate date, Resolution resolution, TickType tickType ) {
        final String formattedDate = date.format( DateFormat.EightCharacter );
        final boolean isHourOrDaily = resolution == Resolution.Hour || resolution == Resolution.Daily;

        final SecurityIdentifier secId = symbol.getId();
        switch ( secId.getSecurityType() ) {
            case Base:
            case Equity:
            case Forex:
            case Cfd:
                if( isHourOrDaily )
                    return String.format( "%s.csv", symbol.getValue().toLowerCase() );

                return String.format( "%s_%s_%s_%s.csv", formattedDate, symbol.getValue().toLowerCase(), resolution.toString().toLowerCase(), tickType.toString().toLowerCase() );

            case Option:
                if( isHourOrDaily) {
                    return String.join( "_",
                        secId.getSymbol().toLowerCase(), // underlying
                        tickType.toString().toLowerCase(),
                        secId.getOptionStyle().toString().toLowerCase(),
                        secId.getOptionRight().toString().toLowerCase(),
                        Long.toString( scale( secId.getStrikePrice() ) ),
                        secId.getDate().format( DateFormat.EightCharacter ) 
                        ) + ".csv";
                }

                return String.join( "_", 
                    formattedDate,
                    secId.getSymbol().toLowerCase(), // underlying
                    resolution.toString().toLowerCase(),
                    tickType.toString().toLowerCase(),
                    secId.getOptionStyle().toString().toLowerCase(),
                    secId.getOptionRight().toString().toLowerCase(),
                    Long.toString( scale( secId.getStrikePrice() ) ),
                    secId.getDate().format( DateFormat.EightCharacter )
                    ) + ".csv";

            case Commodity:
            case Future:
            default:
                throw new UnsupportedOperationException();
        }
    }

    /// Creates the entry name for a QC zip data file
    public static String generateZipEntryName( String symbol, SecurityType securityType, LocalDate date, Resolution resolution ) {
        return generateZipEntryName( symbol, securityType, date, resolution, TickType.Trade );
    }
    
    public static String generateZipEntryName( String symbol, SecurityType securityType, LocalDate date, Resolution resolution, TickType dataType ) {
        if( securityType != SecurityType.Base && securityType != SecurityType.Equity && securityType != SecurityType.Forex && securityType != SecurityType.Cfd )
            throw new UnsupportedOperationException( "This method only implements base, equity, forex and cfd security type." );

        symbol = symbol.toLowerCase();

        if( resolution == Resolution.Hour || resolution == Resolution.Daily )
            return symbol + ".csv";

        //All fx is quote data.
        if( securityType == SecurityType.Forex || securityType == SecurityType.Cfd )
            dataType = TickType.Quote;

        return String.format( "%s_%s_%s_%s.csv", date.format( DateFormat.EightCharacter ), symbol, resolution.toString().toLowerCase(), dataType.toString().toLowerCase() );
    }

    /// Generates the zip file name for the specified date of data.
    public static String generateZipFileName( Symbol symbol, LocalDate date, Resolution resolution, TickType tickType ) {
        final String tickTypeString = tickType.toString().toLowerCase();
        final String formattedDate = date.format( DateFormat.EightCharacter );
        final boolean isHourOrDaily = resolution == Resolution.Hour || resolution == Resolution.Daily;

        switch( symbol.getId().getSecurityType() ) {
            case Base:
            case Equity:
            case Forex:
            case Cfd:
                if( isHourOrDaily )
                    return String.format( "%s.zip", symbol.getValue().toLowerCase() );

                return String.format( "%s_%s.zip", formattedDate, tickTypeString );

            case Option:
                if( isHourOrDaily) {
                    return String.format( "%1$s_%2$s_%3$s.zip", 
                        symbol.getId().getSymbol().toLowerCase(), // underlying
                        tickTypeString,
                        symbol.getId().getOptionStyle().toString().toLowerCase() );
                }

                return String.format( "%1$s_%2$s_%3$s.zip", 
                    formattedDate, 
                    tickTypeString,
                    symbol.getId().getOptionStyle().toString().toLowerCase() );

            case Commodity:
            case Future:
            default:
                throw new UnsupportedOperationException();
        }
    }

    /// Creates the zip file name for a QC zip data file
    public static String generateZipFileName( String symbol, SecurityType securityType, LocalDate date, Resolution resolution ) {
        return generateZipFileName( symbol, securityType, date, resolution, null );
    }
    
    public static String generateZipFileName( String symbol, SecurityType securityType, LocalDate date, Resolution resolution, TickType tickType ) {
        if( resolution == Resolution.Hour || resolution == Resolution.Daily)
            return symbol.toLowerCase() + ".zip";

        final String zipFileName = date.format( DateFormat.EightCharacter );
        if( tickType == null )
            tickType = (securityType == SecurityType.Forex || securityType == SecurityType.Cfd) ? TickType.Quote : TickType.Trade;
       
        final String suffix = String.format( "_%s.zip", tickType.toString().toLowerCase() );
        return zipFileName + suffix;
    }

    /// Gets the tick type most commonly associated with the specified security type
    /// <param name="securityType">The security type</param>
    /// <returns>The most common tick type for the specified security type</returns>
    public static TickType getCommonTickType(SecurityType securityType ) {
        if( securityType == SecurityType.Forex || securityType == SecurityType.Cfd )
            return TickType.Quote;

        return TickType.Trade;
    }

    /// Creates a symbol from the specified zip entry name
    /// <param name="securityType">The security type of the output symbol</param>
    /// <param name="resolution">The resolution of the data source producing the zip entry name</param>
    /// <param name="zipEntryName">The zip entry name to be parsed</param>
    /// <returns>A new symbol representing the zip entry name</returns>
    public static Symbol readSymbolFromZipEntry( SecurityType securityType, Resolution resolution, String zipEntryName ) {
        boolean isHourlyOrDaily = resolution == Resolution.Hour || resolution == Resolution.Daily;
        switch( securityType ) {
            case Option:
                final String[] parts = zipEntryName.replace( ".csv", "" ).split( "_" );
                if( isHourlyOrDaily ) {
                    final OptionStyle style = OptionStyle.valueOf( parts[2].toUpperCase() );
                    final OptionRight right = OptionRight.valueOf( parts[3].toUpperCase() );
                    final BigDecimal strike = (new BigDecimal( parts[4] )).divide( SCALE, RoundingMode.HALF_EVEN );
                    final LocalDate expiry = LocalDate.parse( parts[5], DateFormat.EightCharacter );
                    return Symbol.createOption( parts[0], Market.USA, style, right, strike, expiry );
                }
                else {
                    final OptionStyle style = OptionStyle.valueOf( parts[4].toUpperCase() );
                    final OptionRight right = OptionRight.valueOf( parts[5].toUpperCase() );
                    final BigDecimal strike = (new BigDecimal( parts[6] )).divide( SCALE, RoundingMode.HALF_EVEN );
                    final LocalDate expiry = LocalDate.parse( parts[7], DateFormat.EightCharacter );
                    return Symbol.createOption( parts[1], Market.USA, style, right, strike, expiry );
                }

            default:
                throw new UnsupportedOperationException( "readSymbolFromZipEntry is not implemented for " + securityType + " " + resolution );
        }
    }

    /// scale and convert the resulting number to deci-cents int.
    private static long scale( BigDecimal value) {
        return value.multiply( SCALE ).longValue();
    }

    /// Create a csv line from the specified arguments
    private static String toCsv( Object... args ) {
//        // use culture neutral formatting for decimals
//        for( int i = 0; i < args.length; i++ ) {
//            Object value = args[i];
//            if( value instanceof BigDecimal )
//                args[i] = ((BigDecimal)value).toString();
//        }

        return Arrays.stream( args ).map( Object::toString ).collect( Collectors.joining( "," ) );
    }

    /// Creates a csv line for the bar, if null fills in empty strings
    private static String toCsv( IBar bar ) {
        if( bar == null )
            return toCsv( null, null, null, null );

        return toCsv( scale( bar.getOpen() ), scale( bar.getHigh() ), scale( bar.getLow() ), scale( bar.getClose() ) );
    }
}
