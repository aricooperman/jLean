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

import java.math.BigDecimal;
import java.time.ZoneId;

import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.fasterxml.jackson.datatype.guava.GuavaModule;
import com.fasterxml.jackson.datatype.jdk8.Jdk8Module;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

public class Global {
    
    public static final ZoneId UTC_ZONE_TZ_ID = ZoneId.of( "UTC" );
    public static final ZoneId NEW_YORK_TZ_ID = ZoneId.of( "America/New_York" );
    
//    public static final EventBus APP_EVENT_BUS = new EventBus( "Main Bus" );
    
    public static final ObjectMapper OBJECT_MAPPER = new ObjectMapper()
            .registerModule( new GuavaModule() )
            .registerModule( new Jdk8Module() )
            .registerModule( new JavaTimeModule() )
            .configure( DeserializationFeature.READ_ENUMS_USING_TO_STRING, true )
            .configure( SerializationFeature.WRITE_ENUMS_USING_TO_STRING, true );

    
    public static final BigDecimal ONE_THOUSAND = BigDecimal.valueOf( 1000 );
    public static final BigDecimal TWO = BigDecimal.valueOf( 2 );
    
    

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
//        /// <param name="security The security instance
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
//            if( holding.Type == SecurityType.Forex || holding.Type == SecurityType.Cfd)
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
//        public @Override String toString()
//        {
//            if( ConversionRate == 1.0m)
//            {
//                return String.format( "%1$s: %2$s @ %3$s{3} - Market: %3$s{4}", Symbol, Quantity, CurrencySymbol, AveragePrice, MarketPrice);
//            }
//            return String.format( "%1$s: %2$s @ %3$s{3} - Market: %3$s{4} - Conversion: {5}", Symbol, Quantity, CurrencySymbol, AveragePrice, MarketPrice, ConversionRate);
//        }
//    }

    

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

}
