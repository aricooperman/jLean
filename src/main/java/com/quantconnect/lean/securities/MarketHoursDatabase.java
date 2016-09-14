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

package com.quantconnect.lean.securities;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.time.ZoneId;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.stream.Collectors;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.common.collect.ImmutableMap;
import com.quantconnect.lean.Global;
import com.quantconnect.lean.Globals;
import com.quantconnect.lean.SecurityType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.SubscriptionDataConfig;

/**
 * Provides access to exchange hours and raw data times zones in various markets
 */
// TODO [JsonConverter(typeof(MarketHoursDatabaseJsonConverter))]
public class MarketHoursDatabase {
    
    private static MarketHoursDatabase _dataFolderMarketHoursDatabase;
    
    private static final Object DataFolderMarketHoursDatabaseLock = new Object();

    private final ImmutableMap<SecurityDatabaseKey,DatabaseEntry> _entries;
    private final Logger log = LoggerFactory.getLogger( getClass() );

    /**
     * Gets an instant of <see cref="MarketHoursDatabase"/> that will always return <see cref="SecurityExchangeHours.AlwaysOpen"/>
     * for each call to <see cref="GetExchangeHours( String, Symbol, SecurityType,ZoneId)"/>
     */
    public static MarketHoursDatabase getAlwaysOpen() {
        return new AlwaysOpenMarketHoursDatabase();
    }

    /**
     * Gets all the exchange hours held by this provider
     */
    public List<Entry<SecurityDatabaseKey,DatabaseEntry>> getExchangeHoursListing() {
        return _entries.entrySet().stream().collect( Collectors.toList() );
    }

    /**
     * Initializes a new instance of the <see cref="MarketHoursDatabase"/> class
     * @param exchangeHours The full listing of exchange hours by key
     */
    public MarketHoursDatabase( Map<SecurityDatabaseKey,DatabaseEntry> exchangeHours ) {
        _entries = ImmutableMap.copyOf( exchangeHours );
    }

    // used for the always open implementation
    private MarketHoursDatabase() {
        this( Collections.emptyMap() );
    }
    
    /**
     * Performs a lookup using the specified information and returns the exchange hours if found,
     * if exchange hours are not found, an exception is thrown
     * @param configuration The subscription data config to get exchange hours for
     * This value will also be used as the time zone for SecurityType.Base with no market hours database entry.
     * If null is specified, no @Override will be performed. If null is specified, and it's SecurityType.Base, then Utc will be used.
     */
    public SecurityExchangeHours getExchangeHours( SubscriptionDataConfig configuration ) {
        return getExchangeHours( configuration, null );
    }

    /**
     * Performs a lookup using the specified information and returns the exchange hours if found,
     * if exchange hours are not found, an exception is thrown
     * @param configuration The subscription data config to get exchange hours for
     * @param overrideTimeZone Specify this time zone to @Override the resolved time zone from the market hours database.
     * This value will also be used as the time zone for SecurityType.Base with no market hours database entry.
     * If null is specified, no @Override will be performed. If null is specified, and it's SecurityType.Base, then Utc will be used.
     */
    public SecurityExchangeHours getExchangeHours( SubscriptionDataConfig configuration, ZoneId overrideTimeZone ) {
        // we don't expect base security types to be in the market-hours-database, so set @OverrideTimeZone
        if( configuration.securityType == SecurityType.Base && overrideTimeZone == null ) 
            overrideTimeZone = configuration.exchangeTimeZone;
        return getExchangeHours( configuration.market, configuration.getSymbol(), configuration.securityType, overrideTimeZone );
    }

    /**
     * Performs a lookup using the specified information and returns the exchange hours if found,
     * if exchange hours are not found, an exception is thrown
     * @param market The market the exchange resides in, i.e, 'usa', 'fxcm', ect...
     * @param symbol The particular symbol being traded
     * @param securityType The security type of the symbol
     * This value will also be used as the time zone for SecurityType.Base with no market hours database entry.
     * If null is specified, no @Override will be performed. If null is specified, and it's SecurityType.Base, then Utc will be used.
     * @returns The exchange hours for the specified security
     */
    public SecurityExchangeHours getExchangeHours( String market, Symbol symbol, SecurityType securityType ) {
        return getExchangeHours( market, symbol, securityType, null );
    }
    
    /**
     * Performs a lookup using the specified information and returns the exchange hours if found,
     * if exchange hours are not found, an exception is thrown
     * @param market The market the exchange resides in, i.e, 'usa', 'fxcm', ect...
     * @param symbol The particular symbol being traded
     * @param securityType The security type of the symbol
     * @param @OverrideTimeZone Specify this time zone to @Override the resolved time zone from the market hours database.
     * This value will also be used as the time zone for SecurityType.Base with no market hours database entry.
     * If null is specified, no @Override will be performed. If null is specified, and it's SecurityType.Base, then Utc will be used.
     * @returns The exchange hours for the specified security
     */
    public SecurityExchangeHours getExchangeHours( String market, Symbol symbol, SecurityType securityType, ZoneId overrideTimeZone ) {
        String stringSymbol = symbol == null ? null : symbol.getValue();
        return getEntry( market, stringSymbol, securityType, overrideTimeZone ).exchangeHours;
    }

    /**
     * Performs a lookup using the specified information and returns the data's time zone if found,
     * if an entry is not found, an exception is thrown
     * @param market The market the exchange resides in, i.e, 'usa', 'fxcm', ect...
     * @param symbol The particular symbol being traded
     * @param securityType The security type of the symbol
     * @returns The raw data time zone for the specified security
     */
    public ZoneId getDataTimeZone( String market, Symbol symbol, SecurityType securityType ) {
        final String stringSymbol = symbol == null ? null : symbol.getValue();
        return getEntry( market, stringSymbol, securityType ).dataTimeZone;
    }

    /**
     * Gets the instance of the <see cref="MarketHoursDatabase"/> class produced by reading in the market hours
     * data found in /Data/market-hours/
     * @returns A <see cref="MarketHoursDatabase"/> class that represents the data in the market-hours folder
     */
    public static MarketHoursDatabase fromDataFolder() {
        synchronized( DataFolderMarketHoursDatabaseLock ) {
            if( _dataFolderMarketHoursDatabase == null ) {
                final Path path = Paths.get( Globals.getDataFolder(), "market-hours", "market-hours-database.json" );
                _dataFolderMarketHoursDatabase = fromFile( path );
            }
        }
        
        return _dataFolderMarketHoursDatabase;
    }

    /**
     * Reads the specified file as a market hours database instance
     * @param Files. The markeeof the <see cref="MarketHoursDatabase"/> class
     */
    public static MarketHoursDatabase fromFile( Path path ) {
        try {
            return Global.OBJECT_MAPPER.readValue( Files.newInputStream( path ), MarketHoursDatabase.class );
        }
        catch( IOException e ) {
            throw new RuntimeException( e );
        }
    }

    /**
     * Gets the entry for the specified market/symbol/security-type
     * @param market The market the exchange resides in, i.e, 'usa', 'fxcm', ect...
     * @param symbol The particular symbol being traded
     * @param securityType The security type of the symbol
     * @param @OverrideTimeZone Specify this time zone to @Override the resolved time zone from the market hours database.
     * This value will also be used as the time zone for SecurityType.Base with no market hours database entry.
     * If null is specified, no @Override will be performed. If null is specified, and it's SecurityType.Base, then Utc will be used.
     * @returns The entry matching the specified market/symbol/security-type
     */
    public DatabaseEntry getEntry( String market, String symbol, SecurityType securityType ) {
        return getEntry( market, symbol, securityType, null );
    }
    
    /**
     * Gets the entry for the specified market/symbol/security-type
     * @param market The market the exchange resides in, i.e, 'usa', 'fxcm', ect...
     * @param symbol The particular symbol being traded
     * @param securityType The security type of the symbol
     * @param @OverrideTimeZone Specify this time zone to @Override the resolved time zone from the market hours database.
     * This value will also be used as the time zone for SecurityType.Base with no market hours database entry.
     * If null is specified, no @Override will be performed. If null is specified, and it's SecurityType.Base, then Utc will be used.
     * @returns The entry matching the specified market/symbol/security-type
     */
    public DatabaseEntry getEntry( String market, String symbol, SecurityType securityType, ZoneId overrideTimeZone ) {
        final SecurityDatabaseKey key = new SecurityDatabaseKey( market, symbol, securityType );
        
        DatabaseEntry entry = _entries.get( key );
        if( entry == null) {
            // now check with null symbol key
            entry = _entries.get( new SecurityDatabaseKey( market, null, securityType ) );
            if( entry == null ) {
                if( securityType == SecurityType.Base ) {
                    if( overrideTimeZone == null ) {
                        overrideTimeZone = Global.UTC_ZONE_TZ_ID;
                        log.error( "MarketHoursDatabase.getExchangeHours(): Custom data no time zone specified, default to UTC. " + key );
                    }
                    // base securities are always open by default and have equal data time zone and exchange time zones
                    return new DatabaseEntry( overrideTimeZone, SecurityExchangeHours.alwaysOpen( overrideTimeZone ) );
                }

                log.error( String.format( "MarketHoursDatabase.getExchangeHours(): Unable to locate exchange hours for %1$s. Available keys: %2$s", 
                        key, _entries.keySet().stream().map( SecurityDatabaseKey::toString ).collect( Collectors.joining( ", " ) ) ) );

                // there was nothing that really matched exactly... what should we do here?
                throw new IllegalArgumentException( "Unable to locate exchange hours for " + key);
            }

            // perform time zone override if requested, we'll use the same exact local hours
            // and holidays, but we'll express them in a different time zone
            if( overrideTimeZone != null && !entry.exchangeHours.getTimeZone().equals( overrideTimeZone ) )
                return new DatabaseEntry( overrideTimeZone, new SecurityExchangeHours( overrideTimeZone, entry.exchangeHours.getHolidays(), entry.exchangeHours.getMarketHours() ) );
        }

        return entry;
    }

    /**
     * Represents a single entry in the <see cref="MarketHoursDatabase"/>
     */ 
    public class DatabaseEntry {
        /**
         * Gets the raw data time zone for this entry
         */
        public final ZoneId dataTimeZone;

        /**
         * Gets the exchange hours for this entry
         */
        public final SecurityExchangeHours exchangeHours;

        /**
         * Initializes a new instance of the <see cref="Entry"/> class
         * @param dataTimeZone The raw data time zone
         * @param exchangeHours The security exchange hours for this entry
         */
        public DatabaseEntry( ZoneId dataTimeZone, SecurityExchangeHours exchangeHours ) {
            this.dataTimeZone = dataTimeZone;
            this.exchangeHours = exchangeHours;
        }
    }

    static class AlwaysOpenMarketHoursDatabase extends MarketHoursDatabase {
        @Override
        public DatabaseEntry getEntry( String market, String symbol, SecurityType securityType, ZoneId overrideTimeZone ) {
            final ZoneId tz = overrideTimeZone != null ? overrideTimeZone : Global.UTC_ZONE_TZ_ID;
            return new DatabaseEntry(tz, SecurityExchangeHours.alwaysOpen( tz ) );
        }
    }
}
