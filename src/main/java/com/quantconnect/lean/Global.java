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

package com.quantconnect.lean;

import java.time.LocalDate;
import java.util.Arrays;
import java.util.Map;
import java.util.Set;
import java.util.function.Function;
import java.util.stream.Collectors;

import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.fasterxml.jackson.datatype.guava.GuavaModule;
import com.fasterxml.jackson.datatype.jdk8.Jdk8Module;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;
import com.google.common.collect.ImmutableMap;
import com.google.common.collect.ImmutableSet;

public class Global {
    
    public static final ObjectMapper OBJECT_MAPPER = new ObjectMapper()
            .registerModule( new GuavaModule() )
            .registerModule( new Jdk8Module() )
            .registerModule( new JavaTimeModule() )
            .configure( DeserializationFeature.READ_ENUMS_USING_TO_STRING, true )
            .configure( SerializationFeature.WRITE_ENUMS_USING_TO_STRING, true );
    
    /// Shortcut date format strings
    public static class DateFormat {
        /// Year-Month-Date 6 Character Date Representation
        public static final String SixCharacter = "yyMMdd";
        /// YYYY-MM-DD Eight Character Date Representation
        public static final String EightCharacter = "yyyyMMdd";
        /// Daily and hourly time format
        public static final String TwelveCharacter = "yyyyMMdd HH:mm";
        /// JSON Format Date Representation
        public static String JsonFormat = "yyyy-MM-ddThh:mm:ss";
        /// MySQL Format Date Representation
        public static final String DB = "yyyy-MM-dd HH:mm:ss";
        /// QuantConnect UX Date Representation
        public static final String UI = "yyyy-MM-dd HH:mm:ss";
        /// en-US format
        public static final String US = "M/d/yyyy h:mm:ss tt";
        /// Date format of QC forex data
        public static final String Forex = "yyyyMMdd HH:mm:ss.ffff";
    }

    /// Singular holding of assets from backend live nodes:
//    [JsonObject]
//    public class Holding {
//        /// Symbol of the Holding:
//        public Symbol Symbol = Symbol.Empty;
//
//        /// Type of the security
//        public SecurityType Type;
//
//        /// The currency symbol of the holding, such as $
//        public String CurrencySymbol;
//
//        /// Average Price of our Holding in the currency the symbol is traded in
//        public BigDecimal AveragePrice;
//
//        /// Quantity of Symbol We Hold.
//        public BigDecimal Quantity;
//
//        /// Current Market Price of the Asset in the currency the symbol is traded in
//        public BigDecimal MarketPrice;
//
//        /// Current market conversion rate into the account currency
//        public BigDecimal ConversionRate;
//
//        /// Create a new default holding:
//        public Holding()
//        {
//            CurrencySymbol = "$";
//            ConversionRate = 1m;
//        }
//
//        /// <summary>
//        /// Create a simple JSON holdings from a Security holding class.
//        /// </summary>
//        /// <param name="security">The security instance</param>
//        public Holding(Security security)
//             : this()
//        {
//            holding = security.Holdings;
//
//            Symbol = holding.Symbol;
//            Type = holding.Type;
//            Quantity = holding.Quantity;
//            CurrencySymbol = Currencies.CurrencySymbols[security.QuoteCurrency.Symbol];
//            ConversionRate = security.QuoteCurrency.ConversionRate;
//
//            rounding = 2;
//            if (holding.Type == SecurityType.Forex || holding.Type == SecurityType.Cfd)
//            {
//                rounding = 5;
//            }
//
//            AveragePrice = Math.Round(holding.AveragePrice, rounding);
//            MarketPrice = Math.Round(holding.Price, rounding);
//        }
//
//        /// <summary>
//        /// Clones this instance
//        /// </summary>
//        /// <returns>A new Holding object with the same values as this one</returns>
//        public Holding Clone()
//        {
//            return new Holding
//            {
//                AveragePrice = AveragePrice,
//                Symbol = Symbol,
//                Type = Type,
//                Quantity = Quantity,
//                MarketPrice = MarketPrice,
//                ConversionRate  = ConversionRate,
//                CurrencySymbol = CurrencySymbol
//            };
//        }
//
//        /// <summary>
//        /// Writes out the properties of this instance to string
//        /// </summary>
//        public override String ToString()
//        {
//            if (ConversionRate == 1.0m)
//            {
//                return string.Format("{0}: {1} @ {2}{3} - Market: {2}{4}", Symbol, Quantity, CurrencySymbol, AveragePrice, MarketPrice);
//            }
//            return string.Format("{0}: {1} @ {2}{3} - Market: {2}{4} - Conversion: {5}", Symbol, Quantity, CurrencySymbol, AveragePrice, MarketPrice, ConversionRate);
//        }
//    }

    /// Multilanguage support enum: which language is this project for the interop bridge converter.
//    [JsonConverter(typeof(StringEnumConverter))]
    public enum Language {
        /// C# Language Project
//        [EnumMember(Value = "C#")]
        CSharp,

        /// FSharp Project
//        [EnumMember(Value = "F#")]
        FSharp,

        /// Visual Basic Project
//        [EnumMember(Value = "VB")]
        VisualBasic,

        /// Java Language Project
//        [EnumMember(Value = "Ja")]
        Java,

        /// Python Language Project
//        [EnumMember(Value = "Py")]
        Python
    }


    /// User / Algorithm Job Subscription Level
    public enum UserPlan {
        /// Free User (Backtesting).
        Free,

        /// Hobbyist User with Included 512mb Server.
        Hobbyist
    }


    /// Live server types available through the web IDE. / QC deployment.
    public enum ServerType {
        /// Additional server
        Server512,

        /// Upgraded server
        Server1024,

        /// Server with 2048 MB Ram.
        Server2048
    }


    /// Type of tradable security / underlying asset
    public enum SecurityType {
        /// Base class for all security types:
        Base,

        /// US Equity Security
        Equity,

        /// Option Security Type
        Option,

        /// Commodity Security Type
        Commodity,

        /// FOREX Security
        Forex,

        /// Future Security Type
        Future,

        /// Contract For a Difference Security Type.
        Cfd;
        
        private static final ImmutableMap<Integer,SecurityType> ordinalToTypeMap = ImmutableMap.<Integer,SecurityType>builder()
                .putAll( Arrays.stream( SecurityType.values() ).collect( Collectors.toMap( st -> st.ordinal(), Function.identity() ) ) )
                .build();

        public static SecurityType fromOrdinal( int ord ) {
            return ordinalToTypeMap.get( ord );
        }
    }

    /// Account type: margin or cash
    public enum AccountType {
        /// Margin account type
        Margin,

        /// Cash account type
        Cash
    }

    /// Market data style: is the market data a summary (OHLC style) bar, or is it a time-price value.
    public enum MarketDataType {
        /// Base market data type
        Base,
        /// TradeBar market data type (OHLC summary bar)
        TradeBar,
        /// Tick market data type (price-time pair)
        Tick,
        /// Data associated with an instrument
        Auxiliary,
        /// QuoteBar market data type [Bid(OHLC), Ask(OHLC) and Mid(OHLC) summary bar]
        QuoteBar,
        /// Option chain data
        OptionChain
    }

    /// Datafeed enum options for selecting the source of the datafeed.
    public enum DataFeedEndpoint {
        /// Backtesting Datafeed Endpoint
        Backtesting,
        /// Loading files off the local system
        FileSystem,
        /// Getting datafeed from a QC-Live-Cloud
        LiveTrading,
        /// Database
        Database
    }

    /// Cloud storage permission options.
    public enum StoragePermissions {
        /// Public Storage Permissions
        Public,

        /// Authenticated Read Storage Permissions
        Authenticated
    }

    /// Types of tick data - trades or quote ticks.
    /// <remarks>QuantConnect currently only has trade tick data but can handle quote tick data with the same data structures.</remarks>
    public enum TickType {
        /// Trade type tick object.
        Trade,
        /// Quote type tick object.
        Quote
    }

    /// Specifies the type of <see cref="QuantConnect.Data.Market.Delisting"/> data
    public enum DelistingType {
        /// Specifies a warning of an imminent delisting
        Warning/* = 0*/,

        /// Specifies the symbol has been delisted
        Delisted/* = 1*/
    }

    /// Resolution of data requested.
    /// <remarks>Always sort the enum from the smallest to largest resolution</remarks>
    public enum Resolution {
        /// Tick Resolution (1)
        Tick,
        /// Second Resolution (2)
        Second,
        /// Minute Resolution (3)
        Minute,
        /// Hour Resolution (4)
        Hour,
        /// Daily Resolution (5)
        Daily
    }

    /// Specifies the different types of options
    public enum OptionRight {
        /// A call option, the right to buy at the strike price
        Call,

        /// A put option, the right to sell at the strike price
        Put;

        private static final ImmutableMap<Integer,OptionRight> ordinalToTypeMap = ImmutableMap.<Integer,OptionRight>builder()
                .putAll( Arrays.stream( OptionRight.values() ).collect( Collectors.toMap( or -> or.ordinal(), Function.identity() ) ) )
                .build();

        public static OptionRight fromOrdinal( int ord ) {
            return ordinalToTypeMap.get( ord );
        }
    }

    /// Specifies the style of an option
    public enum OptionStyle {
        /// American style options are able to be exercised at any time on or before the expiration date
        American,

        /// European style options are able to be exercised on the expiration date only.
        European;

        private static final ImmutableMap<Integer,OptionStyle> ordinalToTypeMap = ImmutableMap.<Integer,OptionStyle>builder()
                .putAll( Arrays.stream( OptionStyle.values() ).collect( Collectors.toMap( os -> os.ordinal(), Function.identity() ) ) )
                .build();

        public static OptionStyle fromOrdinal( int ord ) {
            return ordinalToTypeMap.get( ord );
        }
    }

    /// Wrapper for algorithm status enum to include the charting subscription.
    public class AlgorithmControl {
        /// Default initializer for algorithm control class.
        public AlgorithmControl() {
            // default to true, API can override
            hasSubscribers = true;
            status = AlgorithmStatus.Running;
            chartSubscription = "Strategy Equity";
        }

        /// Current run status of the algorithm id.
        public AlgorithmStatus status;

        /// Currently requested chart.
        public String chartSubscription;

        /// True if there's subscribers on the channel
        public boolean hasSubscribers;
    }

    /// States of a live deployment.
    public enum AlgorithmStatus {
        /// Error compiling algorithm at start
        DeployError,    //1
        /// Waiting for a server
        InQueue,        //2
        /// Running algorithm
        Running,        //3
        /// Stopped algorithm or exited with runtime errors
        Stopped,        //4
        /// Liquidated algorithm
        Liquidated,     //5
        /// Algorithm has been deleted
        Deleted,        //6
        /// Algorithm completed running
        Completed,      //7
        /// Runtime Error Stoped Algorithm
        RuntimeError,    //8
        /// Error in the algorithm id (not used).
        Invalid,
        /// The algorithm is logging into the brokerage
        LoggingIn,
        /// The algorithm is initializing
        Initializing,
        /// History status update
        History
    }

    /// Specifies where a subscription's data comes from
    public enum SubscriptionTransportMedium {
        /// The subscription's data comes from disk
        LocalFile,

        /// The subscription's data is downloaded from a remote source
        RemoteFile,

        /// The subscription's data comes from a rest call that is polled and returns a single line/data point of information
        Rest
    }

//    /// enum Period - Enum of all the analysis periods, AS integers. Reference "Period" Array to access the values
//    public enum Period {
//        /// Period Short Codes - 10
//        TenSeconds = 10,
//        /// Period Short Codes - 30 Second
//        ThirtySeconds = 30,
//        /// Period Short Codes - 60 Second
//        OneMinute = 60,
//        /// Period Short Codes - 120 Second
//        TwoMinutes = 120,
//        /// Period Short Codes - 180 Second
//        ThreeMinutes = 180,
//        /// Period Short Codes - 300 Second
//        FiveMinutes = 300,
//        /// Period Short Codes - 600 Second
//        TenMinutes = 600,
//        /// Period Short Codes - 900 Second
//        FifteenMinutes = 900,
//        /// Period Short Codes - 1200 Second
//        TwentyMinutes = 1200,
//        /// Period Short Codes - 1800 Second
//        ThirtyMinutes = 1800,
//        /// Period Short Codes - 3600 Second
//        OneHour = 3600,
//        /// Period Short Codes - 7200 Second
//        TwoHours = 7200,
//        /// Period Short Codes - 14400 Second
//        FourHours = 14400,
//        /// Period Short Codes - 21600 Second
//        SixHours = 21600
//    }

    /// Specifies how data is normalized before being sent into an algorithm
    public enum DataNormalizationMode {
        /// The raw price with dividends added to cash book
        Raw,
        /// The adjusted prices with splits and dividendends factored in
        Adjusted,
        /// The adjusted prices with only splits factored in, dividends paid out to the cash book
        SplitAdjusted,
        /// The split adjusted price plus dividends
        TotalReturn
    }

    /// Global Market Short Codes and their full versions: (used in tick objects)
    public static class MarketCodes {
        /// US Market Codes
        public static final Map<String,String> US = ImmutableMap.<String,String>builder()
                .put( "A", "American Stock Exchange" )
                .put( "B", "Boston Stock Exchange" )
                .put( "C", "National Stock Exchange" )
                .put( "D", "FINRA ADF" )
                .put( "I", "Int.put( rnational Securities Exchange" )
                .put( "J", "Direct Edge A" )
                .put( "K", "Direct Edge X" )
                .put( "M", "Chicago Stock Exchange" )
                .put( "N", "New York Stock Exchange" )
                .put( "P", "Nyse Arca Exchange" )
                .put( "Q", "NASDAQ OMX" )
                .put( "T", "NASDAQ OMX" )
                .put( "U", "OTC Bulletin Board" )
                .put( "u", "Over-the-Counter trade in Non-NASDAQ issue" )
                .put( "W", "Chicago Board Options Exchange" )
                .put( "X", "Philadelphia Stock Exchange" )
                .put( "Y", "BATS Y-Exchange, Inc" )
                .put( "Z", "BATS Exchange, Inc" )
                .build();

        /// Canada Market Short Codes:
        public static final Map<String,String> Canada = ImmutableMap.<String,String>builder()
                .put( "T", "Toronto" )
                .put( "V", "Venture" )
                .build();
    }

    /// Defines the different channel status values
    public static class ChannelStatus {
        /// The channel is empty
        public static final String Vacated = "channel_vacated";
        /// The channel has subscribers
        public static final String Occupied = "channel_occupied";
    }

    /// US Public Holidays - Not Tradeable:
    public static class USHoliday {

        /// Public Holidays
        public static final Set<LocalDate> DATES = ImmutableSet.<LocalDate>builder()
            /* New Years Day*/
                .add( LocalDate.of( 1998, 1, 1 ) )
                .add( LocalDate.of( 1999, 1, 1 ) )
                .add( LocalDate.of( 2001, 1, 1 ) )
                .add( LocalDate.of( 2002, 1, 1 ) )
                .add( LocalDate.of( 2003, 1, 1 ) )
                .add( LocalDate.of( 2004, 1, 1 ) )
                .add( LocalDate.of( 2006, 1, 2 ) )
                .add( LocalDate.of( 2007, 1, 1 ) )
                .add( LocalDate.of( 2008, 1, 1 ) )
                .add( LocalDate.of( 2009, 1, 1 ) )
                .add( LocalDate.of( 2010, 1, 1 ) )
                .add( LocalDate.of( 2011, 1, 1 ) )
                .add( LocalDate.of( 2012, 1, 2 ) )
                .add( LocalDate.of( 2013, 1, 1 ) )
                .add( LocalDate.of( 2014, 1, 1 ) )
                .add( LocalDate.of( 2015, 1, 1 ) )
                .add( LocalDate.of( 2016, 1, 1 ) )
    
                /* Day of Mouring */
                .add( LocalDate.of( 2007, 1, 2 ) )
    
                /* World Trade Center */
                .add( LocalDate.of( 2001, 9, 11 ) )
                .add( LocalDate.of( 2001, 9, 12 ) )
                .add( LocalDate.of( 2001, 9, 13 ) )
                .add( LocalDate.of( 2001, 9, 14 ) )
    
                /* Regan Funeral */
                .add( LocalDate.of( 2004, 6, 11 ) )
    
                /* Hurricane Sandy */
                .add( LocalDate.of( 2012, 10, 29 ) )
                .add( LocalDate.of( 2012, 10, 30 ) )
    
                /* Martin Luther King Jnr Day*/
                .add( LocalDate.of( 1998, 1, 19 ) )
                .add( LocalDate.of( 1999, 1, 18 ) )
                .add( LocalDate.of( 2000, 1, 17 ) )
                .add( LocalDate.of( 2001, 1, 15 ) )
                .add( LocalDate.of( 2002, 1, 21 ) )
                .add( LocalDate.of( 2003, 1, 20 ) )
                .add( LocalDate.of( 2004, 1, 19 ) )
                .add( LocalDate.of( 2005, 1, 17 ) )
                .add( LocalDate.of( 2006, 1, 16 ) )
                .add( LocalDate.of( 2007, 1, 15 ) )
                .add( LocalDate.of( 2008, 1, 21 ) )
                .add( LocalDate.of( 2009, 1, 19 ) )
                .add( LocalDate.of( 2010, 1, 18 ) )
                .add( LocalDate.of( 2011, 1, 17 ) )
                .add( LocalDate.of( 2012, 1, 16 ) )
                .add( LocalDate.of( 2013, 1, 21 ) )
                .add( LocalDate.of( 2014, 1, 20 ) )
                .add( LocalDate.of( 2015, 1, 19 ) )
                .add( LocalDate.of( 2016, 1, 18 ) )
    
                /* Washington / Presidents Day */
                .add( LocalDate.of( 1998, 2, 16 ) )
                .add( LocalDate.of( 1999, 2, 15 ) )
                .add( LocalDate.of( 2000, 2, 21 ) )
                .add( LocalDate.of( 2001, 2, 19 ) )
                .add( LocalDate.of( 2002, 2, 18 ) )
                .add( LocalDate.of( 2003, 2, 17 ) )
                .add( LocalDate.of( 2004, 2, 16 ) )
                .add( LocalDate.of( 2005, 2, 21 ) )
                .add( LocalDate.of( 2006, 2, 20 ) )
                .add( LocalDate.of( 2007, 2, 19 ) )
                .add( LocalDate.of( 2008, 2, 18 ) )
                .add( LocalDate.of( 2009, 2, 16 ) )
                .add( LocalDate.of( 2010, 2, 15 ) )
                .add( LocalDate.of( 2011, 2, 21 ) )
                .add( LocalDate.of( 2012, 2, 20 ) )
                .add( LocalDate.of( 2013, 2, 18 ) )
                .add( LocalDate.of( 2014, 2, 17 ) )
                .add( LocalDate.of( 2015, 2, 16 ) )
                .add( LocalDate.of( 2016, 2, 15 ) )
    
                /* Good Friday */
                .add( LocalDate.of( 1998, 4, 10 ) )
                .add( LocalDate.of( 1999, 4, 2 ) )
                .add( LocalDate.of( 2000, 4, 21 ) )
                .add( LocalDate.of( 2001, 4, 13 ) )
                .add( LocalDate.of( 2002, 3, 29 ) )
                .add( LocalDate.of( 2003, 4, 18 ) )
                .add( LocalDate.of( 2004, 4, 9 ) )
                .add( LocalDate.of( 2005, 3, 25 ) )
                .add( LocalDate.of( 2006, 4, 14 ) )
                .add( LocalDate.of( 2007, 4, 6 ) )
                .add( LocalDate.of( 2008, 3, 21 ) )
                .add( LocalDate.of( 2009, 4, 10 ) )
                .add( LocalDate.of( 2010, 4, 2 ) )
                .add( LocalDate.of( 2011, 4, 22 ) )
                .add( LocalDate.of( 2012, 4, 6 ) )
                .add( LocalDate.of( 2013, 3, 29 ) )
                .add( LocalDate.of( 2014, 4, 18 ) )
                .add( LocalDate.of( 2015, 4, 3 ) )
                .add( LocalDate.of( 2016, 3, 25 ) )
    
                /* Memorial Day */
                .add( LocalDate.of( 1998, 5, 25 ) )
                .add( LocalDate.of( 1999, 5, 31 ) )
                .add( LocalDate.of( 2000, 5, 29 ) )
                .add( LocalDate.of( 2001, 5, 28 ) )
                .add( LocalDate.of( 2002, 5, 27 ) )
                .add( LocalDate.of( 2003, 5, 26 ) )
                .add( LocalDate.of( 2004, 5, 31 ) )
                .add( LocalDate.of( 2005, 5, 30 ) )
                .add( LocalDate.of( 2006, 5, 29 ) )
                .add( LocalDate.of( 2007, 5, 28 ) )
                .add( LocalDate.of( 2008, 5, 26 ) )
                .add( LocalDate.of( 2009, 5, 25 ) )
                .add( LocalDate.of( 2010, 5, 31 ) )
                .add( LocalDate.of( 2011, 5, 30 ) )
                .add( LocalDate.of( 2012, 5, 28 ) )
                .add( LocalDate.of( 2013, 5, 27 ) )
                .add( LocalDate.of( 2014, 5, 26 ) )
                .add( LocalDate.of( 2015, 5, 25 ) )
                .add( LocalDate.of( 2016, 5, 30 ) )
    
                /* Independence Day */
                .add( LocalDate.of( 1998, 7, 3 ) )
                .add( LocalDate.of( 1999, 7, 5 ) )
                .add( LocalDate.of( 2000, 7, 4 ) )
                .add( LocalDate.of( 2001, 7, 4 ) )
                .add( LocalDate.of( 2002, 7, 4 ) )
                .add( LocalDate.of( 2003, 7, 4 ) )
                .add( LocalDate.of( 2004, 7, 5 ) )
                .add( LocalDate.of( 2005, 7, 4 ) )
                .add( LocalDate.of( 2006, 7, 4 ) )
                .add( LocalDate.of( 2007, 7, 4 ) )
                .add( LocalDate.of( 2008, 7, 4 ) )
                .add( LocalDate.of( 2009, 7, 3 ) )
                .add( LocalDate.of( 2010, 7, 5 ) )
                .add( LocalDate.of( 2011, 7, 4 ) )
                .add( LocalDate.of( 2012, 7, 4 ) )
                .add( LocalDate.of( 2013, 7, 4 ) )
                .add( LocalDate.of( 2014, 7, 4 ) )
                .add( LocalDate.of( 2014, 7, 4 ) )
                .add( LocalDate.of( 2015, 7, 3 ) )
                .add( LocalDate.of( 2016, 7, 4 ) )
    
                /* Labor Day */
                .add( LocalDate.of( 1998, 9, 7 ) )
                .add( LocalDate.of( 1999, 9, 6 ) )
                .add( LocalDate.of( 2000, 9, 4 ) )
                .add( LocalDate.of( 2001, 9, 3 ) )
                .add( LocalDate.of( 2002, 9, 2 ) )
                .add( LocalDate.of( 2003, 9, 1 ) )
                .add( LocalDate.of( 2004, 9, 6 ) )
                .add( LocalDate.of( 2005, 9, 5 ) )
                .add( LocalDate.of( 2006, 9, 4 ) )
                .add( LocalDate.of( 2007, 9, 3 ) )
                .add( LocalDate.of( 2008, 9, 1 ) )
                .add( LocalDate.of( 2009, 9, 7 ) )
                .add( LocalDate.of( 2010, 9, 6 ) )
                .add( LocalDate.of( 2011, 9, 5 ) )
                .add( LocalDate.of( 2012, 9, 3 ) )
                .add( LocalDate.of( 2013, 9, 2 ) )
                .add( LocalDate.of( 2014, 9, 1 ) )
                .add( LocalDate.of( 2015, 9, 7 ) )
                .add( LocalDate.of( 2016, 9, 5 ) )
    
                /* Thanksgiving Day */
                .add( LocalDate.of( 1998, 11, 26 ) )
                .add( LocalDate.of( 1999, 11, 25 ) )
                .add( LocalDate.of( 2000, 11, 23 ) )
                .add( LocalDate.of( 2001, 11, 22 ) )
                .add( LocalDate.of( 2002, 11, 28 ) )
                .add( LocalDate.of( 2003, 11, 27 ) )
                .add( LocalDate.of( 2004, 11, 25 ) )
                .add( LocalDate.of( 2005, 11, 24 ) )
                .add( LocalDate.of( 2006, 11, 23 ) )
                .add( LocalDate.of( 2007, 11, 22 ) )
                .add( LocalDate.of( 2008, 11, 27 ) )
                .add( LocalDate.of( 2009, 11, 26 ) )
                .add( LocalDate.of( 2010, 11, 25 ) )
                .add( LocalDate.of( 2011, 11, 24 ) )
                .add( LocalDate.of( 2012, 11, 22 ) )
                .add( LocalDate.of( 2013, 11, 28 ) )
                .add( LocalDate.of( 2014, 11, 27 ) )
                .add( LocalDate.of( 2015, 11, 26 ) )
                .add( LocalDate.of( 2016, 11, 24 ) )
    
                /* Christmas */
                .add( LocalDate.of( 1998, 12, 25 ) )
                .add( LocalDate.of( 1999, 12, 24 ) )
                .add( LocalDate.of( 2000, 12, 25 ) )
                .add( LocalDate.of( 2001, 12, 25 ) )
                .add( LocalDate.of( 2002, 12, 25 ) )
                .add( LocalDate.of( 2003, 12, 25 ) )
                .add( LocalDate.of( 2004, 12, 24 ) )
                .add( LocalDate.of( 2005, 12, 26 ) )
                .add( LocalDate.of( 2006, 12, 25 ) )
                .add( LocalDate.of( 2007, 12, 25 ) )
                .add( LocalDate.of( 2008, 12, 25 ) )
                .add( LocalDate.of( 2009, 12, 25 ) )
                .add( LocalDate.of( 2010, 12, 24 ) )
                .add( LocalDate.of( 2011, 12, 26 ) )
                .add( LocalDate.of( 2012, 12, 25 ) )
                .add( LocalDate.of( 2013, 12, 25 ) )
                .add( LocalDate.of( 2014, 12, 25 ) )
                .add( LocalDate.of( 2015, 12, 25 ) )
                .add( LocalDate.of( 2016, 12, 25 ) )
                .build();
    }
}

/*

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuantConnect.Securities;
using QuantConnect.Securities.Cfd;
using QuantConnect.Securities.Forex;

package com.quantconnect.lean
{
    /// <summary>
    /// Shortcut date format strings
    /// </summary>
    public static class DateFormat
    {
        /// Year-Month-Date 6 Character Date Representation
        public static final String SixCharacter = "yyMMdd";
        /// YYYY-MM-DD Eight Character Date Representation
        public static final String EightCharacter = "yyyyMMdd";
        /// Daily and hourly time format
        public static final String TwelveCharacter = "yyyyMMdd HH:mm";
        /// JSON Format Date Representation
        public static String JsonFormat = "yyyy-MM-ddThh:mm:ss";
        /// MySQL Format Date Representation
        public static final String DB = "yyyy-MM-dd HH:mm:ss";
        /// QuantConnect UX Date Representation
        public static final String UI = "yyyy-MM-dd HH:mm:ss";
        /// en-US format
        public static final String US = "M/d/yyyy h:mm:ss tt";
        /// Date format of QC forex data
        public static final String Forex = "yyyyMMdd HH:mm:ss.ffff";
    }

    /// <summary>
    /// Singular holding of assets from backend live nodes:
    /// </summary>
    [JsonObject]
    public class Holding
    {
        /// Symbol of the Holding:
        public Symbol Symbol = Symbol.Empty;

        /// Type of the security
        public SecurityType Type;

        /// The currency symbol of the holding, such as $
        public String CurrencySymbol;

        /// Average Price of our Holding in the currency the symbol is traded in
        public BigDecimal AveragePrice;

        /// Quantity of Symbol We Hold.
        public BigDecimal Quantity;

        /// Current Market Price of the Asset in the currency the symbol is traded in
        public BigDecimal MarketPrice;

        /// Current market conversion rate into the account currency
        public BigDecimal ConversionRate;

        /// Create a new default holding:
        public Holding()
        {
            CurrencySymbol = "$";
            ConversionRate = 1m;
        }

        /// <summary>
        /// Create a simple JSON holdings from a Security holding class.
        /// </summary>
        /// <param name="security">The security instance</param>
        public Holding(Security security)
             : this()
        {
            holding = security.Holdings;

            Symbol = holding.Symbol;
            Type = holding.Type;
            Quantity = holding.Quantity;
            CurrencySymbol = Currencies.CurrencySymbols[security.QuoteCurrency.Symbol];
            ConversionRate = security.QuoteCurrency.ConversionRate;

            rounding = 2;
            if (holding.Type == SecurityType.Forex || holding.Type == SecurityType.Cfd)
            {
                rounding = 5;
            }

            AveragePrice = Math.Round(holding.AveragePrice, rounding);
            MarketPrice = Math.Round(holding.Price, rounding);
        }

        /// <summary>
        /// Clones this instance
        /// </summary>
        /// <returns>A new Holding object with the same values as this one</returns>
        public Holding Clone()
        {
            return new Holding
            {
                AveragePrice = AveragePrice,
                Symbol = Symbol,
                Type = Type,
                Quantity = Quantity,
                MarketPrice = MarketPrice,
                ConversionRate  = ConversionRate,
                CurrencySymbol = CurrencySymbol
            };
        }

        /// <summary>
        /// Writes out the properties of this instance to string
        /// </summary>
        public override String ToString()
        {
            if (ConversionRate == 1.0m)
            {
                return string.Format("{0}: {1} @ {2}{3} - Market: {2}{4}", Symbol, Quantity, CurrencySymbol, AveragePrice, MarketPrice);
            }
            return string.Format("{0}: {1} @ {2}{3} - Market: {2}{4} - Conversion: {5}", Symbol, Quantity, CurrencySymbol, AveragePrice, MarketPrice, ConversionRate);
        }
    }

    /// <summary>
    /// Processing runmode of the backtest.
    /// </summary>
    /// <obsolete>The runmode enum is now obsolete and all tasks are run in series mode. This was done to ensure algorithms have memory of the day before.</obsolete>
    public enum RunMode
    {
        /// Automatically detect the runmode of the algorithm: series for minute data, parallel for second-tick
        Automatic,
        /// Series runmode for the algorithm
        Series,
        /// Parallel runmode for the algorithm
        Parallel
    }


    /// <summary>
    /// Multilanguage support enum: which language is this project for the interop bridge converter.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Language
    {
        /// <summary>
        /// C# Language Project
        /// </summary>
        [EnumMember(Value = "C#")]
        CSharp,

        /// <summary>
        /// FSharp Project
        /// </summary>
        [EnumMember(Value = "F#")]
        FSharp,

        /// <summary>
        /// Visual Basic Project
        /// </summary>
        [EnumMember(Value = "VB")]
        VisualBasic,

        /// <summary>
        /// Java Language Project
        /// </summary>
        [EnumMember(Value = "Ja")]
        Java,

        /// <summary>
        /// Python Language Project
        /// </summary>
        [EnumMember(Value = "Py")]
        Python
    }


    /// <summary>
    /// User / Algorithm Job Subscription Level
    /// </summary>
    public enum UserPlan
    {
        /// <summary>
        /// Free User (Backtesting).
        /// </summary>
        Free,

        /// <summary>
        /// Hobbyist User with Included 512mb Server.
        /// </summary>
        Hobbyist
    }


    /// <summary>
    /// Live server types available through the web IDE. / QC deployment.
    /// </summary>
    public enum ServerType
    {
        /// <summary>
        /// Additional server
        /// </summary>
        Server512,

        /// <summary>
        /// Upgraded server
        /// </summary>
        Server1024,

        /// <summary>
        /// Server with 2048 MB Ram.
        /// </summary>
        Server2048
    }


    /// <summary>
    /// Type of tradable security / underlying asset
    /// </summary>
    public enum SecurityType
    {
        /// <summary>
        /// Base class for all security types:
        /// </summary>
        Base,

        /// <summary>
        /// US Equity Security
        /// </summary>
        Equity,

        /// <summary>
        /// Option Security Type
        /// </summary>
        Option,

        /// <summary>
        /// Commodity Security Type
        /// </summary>
        Commodity,

        /// <summary>
        /// FOREX Security
        /// </summary>
        Forex,

        /// <summary>
        /// Future Security Type
        /// </summary>
        Future,

        /// <summary>
        /// Contract For a Difference Security Type.
        /// </summary>
        Cfd
    }

    /// <summary>
    /// Account type: margin or cash
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// Margin account type
        /// </summary>
        Margin,

        /// <summary>
        /// Cash account type
        /// </summary>
        Cash
    }

    /// <summary>
    /// Market data style: is the market data a summary (OHLC style) bar, or is it a time-price value.
    /// </summary>
    public enum MarketDataType
    {
        /// Base market data type
        Base,
        /// TradeBar market data type (OHLC summary bar)
        TradeBar,
        /// Tick market data type (price-time pair)
        Tick,
        /// Data associated with an instrument
        Auxiliary,
        /// QuoteBar market data type [Bid(OHLC), Ask(OHLC) and Mid(OHLC) summary bar]
        QuoteBar,
        /// Option chain data
        OptionChain
    }

    /// <summary>
    /// Datafeed enum options for selecting the source of the datafeed.
    /// </summary>
    public enum DataFeedEndpoint
    {
        /// Backtesting Datafeed Endpoint
        Backtesting,
        /// Loading files off the local system
        FileSystem,
        /// Getting datafeed from a QC-Live-Cloud
        LiveTrading,
        /// Database
        Database
    }

    /// <summary>
    /// Cloud storage permission options.
    /// </summary>
    public enum StoragePermissions
    {
        /// Public Storage Permissions
        Public,

        /// Authenticated Read Storage Permissions
        Authenticated
    }

    /// <summary>
    /// Types of tick data - trades or quote ticks.
    /// </summary>
    /// <remarks>QuantConnect currently only has trade tick data but can handle quote tick data with the same data structures.</remarks>
    public enum TickType
    {
        /// Trade type tick object.
        Trade,
        /// Quote type tick object.
        Quote
    }

    /// <summary>
    /// Specifies the type of <see cref="QuantConnect.Data.Market.Delisting"/> data
    /// </summary>
    public enum DelistingType
    {
        /// <summary>
        /// Specifies a warning of an imminent delisting
        /// </summary>
        Warning = 0,

        /// <summary>
        /// Specifies the symbol has been delisted
        /// </summary>
        Delisted = 1
    }

    /// <summary>
    /// Resolution of data requested.
    /// </summary>
    /// <remarks>Always sort the enum from the smallest to largest resolution</remarks>
    public enum Resolution
    {
        /// Tick Resolution (1)
        Tick,
        /// Second Resolution (2)
        Second,
        /// Minute Resolution (3)
        Minute,
        /// Hour Resolution (4)
        Hour,
        /// Daily Resolution (5)
        Daily
    }

    /// <summary>
    /// Specifies the different types of options
    /// </summary>
    public enum OptionRight
    {
        /// <summary>
        /// A call option, the right to buy at the strike price
        /// </summary>
        Call,

        /// <summary>
        /// A put option, the right to sell at the strike price
        /// </summary>
        Put
    }

    /// <summary>
    /// Specifies the style of an option
    /// </summary>
    public enum OptionStyle
    {
        /// <summary>
        /// American style options are able to be exercised at any time on or before the expiration date
        /// </summary>
        American,

        /// <summary>
        /// European style options are able to be exercised on the expiration date only.
        /// </summary>
        European
    }

    /// <summary>
    /// Wrapper for algorithm status enum to include the charting subscription.
    /// </summary>
    public class AlgorithmControl
    {
        /// <summary>
        /// Default initializer for algorithm control class.
        /// </summary>
        public AlgorithmControl()
        {
            // default to true, API can override
            HasSubscribers = true;
            Status = AlgorithmStatus.Running;
            ChartSubscription = "Strategy Equity";
        }

        /// <summary>
        /// Current run status of the algorithm id.
        /// </summary>
        public AlgorithmStatus Status;

        /// <summary>
        /// Currently requested chart.
        /// </summary>
        public String ChartSubscription;

        /// <summary>
        /// True if there's subscribers on the channel
        /// </summary>
        public boolean HasSubscribers;
    }

    /// <summary>
    /// States of a live deployment.
    /// </summary>
    public enum AlgorithmStatus
    {
        /// Error compiling algorithm at start
        DeployError,    //1
        /// Waiting for a server
        InQueue,        //2
        /// Running algorithm
        Running,        //3
        /// Stopped algorithm or exited with runtime errors
        Stopped,        //4
        /// Liquidated algorithm
        Liquidated,     //5
        /// Algorithm has been deleted
        Deleted,        //6
        /// Algorithm completed running
        Completed,      //7
        /// Runtime Error Stoped Algorithm
        RuntimeError,    //8
        /// Error in the algorithm id (not used).
        Invalid,
        /// The algorithm is logging into the brokerage
        LoggingIn,
        /// The algorithm is initializing
        Initializing,
        /// History status update
        History
    }

    /// <summary>
    /// Specifies where a subscription's data comes from
    /// </summary>
    public enum SubscriptionTransportMedium
    {
        /// <summary>
        /// The subscription's data comes from disk
        /// </summary>
        LocalFile,

        /// <summary>
        /// The subscription's data is downloaded from a remote source
        /// </summary>
        RemoteFile,

        /// <summary>
        /// The subscription's data comes from a rest call that is polled and returns a single line/data point of information
        /// </summary>
        Rest
    }

    /// <summary>
    /// enum Period - Enum of all the analysis periods, AS integers. Reference "Period" Array to access the values
    /// </summary>
    public enum Period
    {
        /// Period Short Codes - 10
        TenSeconds = 10,
        /// Period Short Codes - 30 Second
        ThirtySeconds = 30,
        /// Period Short Codes - 60 Second
        OneMinute = 60,
        /// Period Short Codes - 120 Second
        TwoMinutes = 120,
        /// Period Short Codes - 180 Second
        ThreeMinutes = 180,
        /// Period Short Codes - 300 Second
        FiveMinutes = 300,
        /// Period Short Codes - 600 Second
        TenMinutes = 600,
        /// Period Short Codes - 900 Second
        FifteenMinutes = 900,
        /// Period Short Codes - 1200 Second
        TwentyMinutes = 1200,
        /// Period Short Codes - 1800 Second
        ThirtyMinutes = 1800,
        /// Period Short Codes - 3600 Second
        OneHour = 3600,
        /// Period Short Codes - 7200 Second
        TwoHours = 7200,
        /// Period Short Codes - 14400 Second
        FourHours = 14400,
        /// Period Short Codes - 21600 Second
        SixHours = 21600
    }

    /// <summary>
    /// Specifies how data is normalized before being sent into an algorithm
    /// </summary>
    public enum DataNormalizationMode
    {
        /// <summary>
        /// The raw price with dividends added to cash book
        /// </summary>
        Raw,
        /// <summary>
        /// The adjusted prices with splits and dividendends factored in
        /// </summary>
        Adjusted,
        /// <summary>
        /// The adjusted prices with only splits factored in, dividends paid out to the cash book
        /// </summary>
        SplitAdjusted,
        /// <summary>
        /// The split adjusted price plus dividends
        /// </summary>
        TotalReturn
    }

    /// <summary>
    /// Global Market Short Codes and their full versions: (used in tick objects)
    /// </summary>
    public static class MarketCodes
    {
        /// US Market Codes
        public static Map<String,String> US = new Map<String,String>()
        {
            {"A", "American Stock Exchange"},
            {"B", "Boston Stock Exchange"},
            {"C", "National Stock Exchange"},
            {"D", "FINRA ADF"},
            {"I", "International Securities Exchange"},
            {"J", "Direct Edge A"},
            {"K", "Direct Edge X"},
            {"M", "Chicago Stock Exchange"},
            {"N", "New York Stock Exchange"},
            {"P", "Nyse Arca Exchange"},
            {"Q", "NASDAQ OMX"},
            {"T", "NASDAQ OMX"},
            {"U", "OTC Bulletin Board"},
            {"u", "Over-the-Counter trade in Non-NASDAQ issue"},
            {"W", "Chicago Board Options Exchange"},
            {"X", "Philadelphia Stock Exchange"},
            {"Y", "BATS Y-Exchange, Inc"},
            {"Z", "BATS Exchange, Inc"}
        };

        /// Canada Market Short Codes:
        public static Map<String,String> Canada = new Map<String,String>()
        {
            {"T", "Toronto"},
            {"V", "Venture"}
        };
    }

    /// <summary>
    /// Defines the different channel status values
    /// </summary>
    public static class ChannelStatus
    {
        /// <summary>
        /// The channel is empty
        /// </summary>
        public static final String Vacated = "channel_vacated";
        /// <summary>
        /// The channel has subscribers
        /// </summary>
        public static final String Occupied = "channel_occupied";
    }

    /// <summary>
    /// US Public Holidays - Not Tradeable:
    /// </summary>
    public static class USHoliday
    {
        /// <summary>
        /// Public Holidays
        /// </summary>
        public static readonly HashSet<DateTime> Dates = new HashSet<DateTime>
        {
            /* New Years Day
            new DateTime(1998, 01, 01),
            new DateTime(1999, 01, 01),
            new DateTime(2001, 01, 01),
            new DateTime(2002, 01, 01),
            new DateTime(2003, 01, 01),
            new DateTime(2004, 01, 01),
            new DateTime(2006, 01, 02),
            new DateTime(2007, 01, 01),
            new DateTime(2008, 01, 01),
            new DateTime(2009, 01, 01),
            new DateTime(2010, 01, 01),
            new DateTime(2011, 01, 01),
            new DateTime(2012, 01, 02),
            new DateTime(2013, 01, 01),
            new DateTime(2014, 01, 01),
            new DateTime(2015, 01, 01),
            new DateTime(2016, 01, 01),

            /* Day of Mouring 
            new DateTime(2007, 01, 02),

            /* World Trade Center 
            new DateTime(2001, 09, 11),
            new DateTime(2001, 09, 12),
            new DateTime(2001, 09, 13),
            new DateTime(2001, 09, 14),

            /* Regan Funeral 
            new DateTime(2004, 06, 11),

            /* Hurricane Sandy 
            new DateTime(2012, 10, 29),
            new DateTime(2012, 10, 30),

            /* Martin Luther King Jnr Day
            new DateTime(1998, 01, 19),
            new DateTime(1999, 01, 18),
            new DateTime(2000, 01, 17),
            new DateTime(2001, 01, 15),
            new DateTime(2002, 01, 21),
            new DateTime(2003, 01, 20),
            new DateTime(2004, 01, 19),
            new DateTime(2005, 01, 17),
            new DateTime(2006, 01, 16),
            new DateTime(2007, 01, 15),
            new DateTime(2008, 01, 21),
            new DateTime(2009, 01, 19),
            new DateTime(2010, 01, 18),
            new DateTime(2011, 01, 17),
            new DateTime(2012, 01, 16),
            new DateTime(2013, 01, 21),
            new DateTime(2014, 01, 20),
            new DateTime(2015, 01, 19),
            new DateTime(2016, 01, 18),

            /* Washington / Presidents Day 
            new DateTime(1998, 02, 16),
            new DateTime(1999, 02, 15),
            new DateTime(2000, 02, 21),
            new DateTime(2001, 02, 19),
            new DateTime(2002, 02, 18),
            new DateTime(2003, 02, 17),
            new DateTime(2004, 02, 16),
            new DateTime(2005, 02, 21),
            new DateTime(2006, 02, 20),
            new DateTime(2007, 02, 19),
            new DateTime(2008, 02, 18),
            new DateTime(2009, 02, 16),
            new DateTime(2010, 02, 15),
            new DateTime(2011, 02, 21),
            new DateTime(2012, 02, 20),
            new DateTime(2013, 02, 18),
            new DateTime(2014, 02, 17),
            new DateTime(2015, 02, 16),
            new DateTime(2016, 02, 15),

            /* Good Friday 
            new DateTime(1998, 04, 10),
            new DateTime(1999, 04, 02),
            new DateTime(2000, 04, 21),
            new DateTime(2001, 04, 13),
            new DateTime(2002, 03, 29),
            new DateTime(2003, 04, 18),
            new DateTime(2004, 04, 09),
            new DateTime(2005, 03, 25),
            new DateTime(2006, 04, 14),
            new DateTime(2007, 04, 06),
            new DateTime(2008, 03, 21),
            new DateTime(2009, 04, 10),
            new DateTime(2010, 04, 02),
            new DateTime(2011, 04, 22),
            new DateTime(2012, 04, 06),
            new DateTime(2013, 03, 29),
            new DateTime(2014, 04, 18),
            new DateTime(2015, 04, 03),
            new DateTime(2016, 03, 25),

            /* Memorial Day 
            new DateTime(1998, 05, 25),
            new DateTime(1999, 05, 31),
            new DateTime(2000, 05, 29),
            new DateTime(2001, 05, 28),
            new DateTime(2002, 05, 27),
            new DateTime(2003, 05, 26),
            new DateTime(2004, 05, 31),
            new DateTime(2005, 05, 30),
            new DateTime(2006, 05, 29),
            new DateTime(2007, 05, 28),
            new DateTime(2008, 05, 26),
            new DateTime(2009, 05, 25),
            new DateTime(2010, 05, 31),
            new DateTime(2011, 05, 30),
            new DateTime(2012, 05, 28),
            new DateTime(2013, 05, 27),
            new DateTime(2014, 05, 26),
            new DateTime(2015, 05, 25),
            new DateTime(2016, 05, 30),

            /* Independence Day 
            new DateTime(1998, 07, 03),
            new DateTime(1999, 07, 05),
            new DateTime(2000, 07, 04),
            new DateTime(2001, 07, 04),
            new DateTime(2002, 07, 04),
            new DateTime(2003, 07, 04),
            new DateTime(2004, 07, 05),
            new DateTime(2005, 07, 04),
            new DateTime(2006, 07, 04),
            new DateTime(2007, 07, 04),
            new DateTime(2008, 07, 04),
            new DateTime(2009, 07, 03),
            new DateTime(2010, 07, 05),
            new DateTime(2011, 07, 04),
            new DateTime(2012, 07, 04),
            new DateTime(2013, 07, 04),
            new DateTime(2014, 07, 04),
            new DateTime(2014, 07, 04),
            new DateTime(2015, 07, 03),
            new DateTime(2016, 07, 04),

            /* Labor Day 
            new DateTime(1998, 09, 07),
            new DateTime(1999, 09, 06),
            new DateTime(2000, 09, 04),
            new DateTime(2001, 09, 03),
            new DateTime(2002, 09, 02),
            new DateTime(2003, 09, 01),
            new DateTime(2004, 09, 06),
            new DateTime(2005, 09, 05),
            new DateTime(2006, 09, 04),
            new DateTime(2007, 09, 03),
            new DateTime(2008, 09, 01),
            new DateTime(2009, 09, 07),
            new DateTime(2010, 09, 06),
            new DateTime(2011, 09, 05),
            new DateTime(2012, 09, 03),
            new DateTime(2013, 09, 02),
            new DateTime(2014, 09, 01),
            new DateTime(2015, 09, 07),
            new DateTime(2016, 09, 05),

            /* Thanksgiving Day 
            new DateTime(1998, 11, 26),
            new DateTime(1999, 11, 25),
            new DateTime(2000, 11, 23),
            new DateTime(2001, 11, 22),
            new DateTime(2002, 11, 28),
            new DateTime(2003, 11, 27),
            new DateTime(2004, 11, 25),
            new DateTime(2005, 11, 24),
            new DateTime(2006, 11, 23),
            new DateTime(2007, 11, 22),
            new DateTime(2008, 11, 27),
            new DateTime(2009, 11, 26),
            new DateTime(2010, 11, 25),
            new DateTime(2011, 11, 24),
            new DateTime(2012, 11, 22),
            new DateTime(2013, 11, 28),
            new DateTime(2014, 11, 27),
            new DateTime(2015, 11, 26),
            new DateTime(2016, 11, 24),

            /* Christmas 
            new DateTime(1998, 12, 25),
            new DateTime(1999, 12, 24),
            new DateTime(2000, 12, 25),
            new DateTime(2001, 12, 25),
            new DateTime(2002, 12, 25),
            new DateTime(2003, 12, 25),
            new DateTime(2004, 12, 24),
            new DateTime(2005, 12, 26),
            new DateTime(2006, 12, 25),
            new DateTime(2007, 12, 25),
            new DateTime(2008, 12, 25),
            new DateTime(2009, 12, 25),
            new DateTime(2010, 12, 24),
            new DateTime(2011, 12, 26),
            new DateTime(2012, 12, 25),
            new DateTime(2013, 12, 25),
            new DateTime(2014, 12, 25),
            new DateTime(2015, 12, 25),
            new DateTime(2016, 12, 25)
        };
    }
}
*/