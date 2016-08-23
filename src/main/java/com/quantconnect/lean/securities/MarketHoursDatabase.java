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

import com.google.common.collect.ImmutableMap;

//using System.IO;
//using System.Linq;
//using Newtonsoft.Json;
//using NodaTime;
//using QuantConnect.Data;
//using QuantConnect.Logging;
//using QuantConnect.Util;

/// Provides access to exchange hours and raw data times zones in various markets
// TODO [JsonConverter(typeof(MarketHoursDatabaseJsonConverter))]
public class MarketHoursDatabase {
    
    private static MarketHoursDatabase _dataFolderMarketHoursDatabase;
    
    private static final Object DataFolderMarketHoursDatabaseLock = new Object();

    private final ImmutableMap<SecurityDatabaseKey, Entry> _entries;

    /**
    /// Gets an instant of <see cref="MarketHoursDatabase"/> that will always return <see cref="SecurityExchangeHours.AlwaysOpen"/>
    /// for each call to <see cref="GetExchangeHours( String, Symbol, SecurityType,ZoneId)"/>
    */
    public static MarketHoursDatabase AlwaysOpen
    {
        get { return new AlwaysOpenMarketHoursDatabase(); }
    }

    /**
    /// Gets all the exchange hours held by this provider
    */
    public List<KeyValuePair<SecurityDatabaseKey,Entry>> ExchangeHoursListing
    {
        get { return _entries.ToList(); }
    }

    /**
    /// Initializes a new instance of the <see cref="MarketHoursDatabase"/> class
    */
     * @param exchangeHours">The full listing of exchange hours by key
    public MarketHoursDatabase(IReadOnlyMap<SecurityDatabaseKey, Entry> exchangeHours) {
        _entries = exchangeHours.ToDictionary();
    }

    private MarketHoursDatabase() {
        // used for the always open implementation
    }

    /**
    /// Performs a lookup using the specified information and returns the exchange hours if found,
    /// if exchange hours are not found, an exception is thrown
    */
     * @param configuration">The subscription data config to get exchange hours for
     * @param @OverrideTimeZone">Specify this time zone to @Override the resolved time zone from the market hours database.
    /// This value will also be used as the time zone for SecurityType.Base with no market hours database entry.
    /// If null is specified, no @Override will be performed. If null is specified, and it's SecurityType.Base, then Utc will be used.
    public SecurityExchangeHours GetExchangeHours(SubscriptionDataConfig configuration, ZoneId @OverrideTimeZone = null ) {
        // we don't expect base security types to be in the market-hours-database, so set @OverrideTimeZone
        if( configuration.SecurityType == SecurityType.Base && @OverrideTimeZone == null ) @OverrideTimeZone = configuration.ExchangeTimeZone;
        return GetExchangeHours(configuration.Market, configuration.Symbol, configuration.SecurityType, @OverrideTimeZone);
    }

    /**
    /// Performs a lookup using the specified information and returns the exchange hours if found,
    /// if exchange hours are not found, an exception is thrown
    */
     * @param market">The market the exchange resides in, i.e, 'usa', 'fxcm', ect...
     * @param symbol">The particular symbol being traded
     * @param securityType">The security type of the symbol
     * @param @OverrideTimeZone">Specify this time zone to @Override the resolved time zone from the market hours database.
    /// This value will also be used as the time zone for SecurityType.Base with no market hours database entry.
    /// If null is specified, no @Override will be performed. If null is specified, and it's SecurityType.Base, then Utc will be used.
    @returns The exchange hours for the specified security
    public SecurityExchangeHours GetExchangeHours( String market, Symbol symbol, SecurityType securityType, ZoneId @OverrideTimeZone = null ) {
        stringSymbol = symbol == null ? string.Empty : symbol.Value;
        return GetEntry(market, stringSymbol, securityType, @OverrideTimeZone).ExchangeHours;
    }

    /**
    /// Performs a lookup using the specified information and returns the data's time zone if found,
    /// if an entry is not found, an exception is thrown
    */
     * @param market">The market the exchange resides in, i.e, 'usa', 'fxcm', ect...
     * @param symbol">The particular symbol being traded
     * @param securityType">The security type of the symbol
    @returns The raw data time zone for the specified security
    public ZoneId GetDataTimeZone( String market, Symbol symbol, SecurityType securityType) {
        stringSymbol = symbol == null ? string.Empty : symbol.Value;
        return GetEntry(market, stringSymbol, securityType).DataTimeZone;
    }

    /**
    /// Gets the instance of the <see cref="MarketHoursDatabase"/> class produced by reading in the market hours
    /// data found in /Data/market-hours/
    */
    @returns A <see cref="MarketHoursDatabase"/> class that represents the data in the market-hours folder
    public static MarketHoursDatabase FromDataFolder() {
        lock (DataFolderMarketHoursDatabaseLock) {
            if( _dataFolderMarketHoursDatabase == null ) {
                path = Path.Combine(Globals.DataFolder, "market-hours", "market-hours-database.json");
                _dataFolderMarketHoursDatabase = FromFile(path);
            }
        }
        return _dataFolderMarketHoursDatabase;
    }

    /**
    /// Reads the specified file as a market hours database instance
    */
     * @param path">The market hours database file path
    @returns A new instance of the <see cref="MarketHoursDatabase"/> class
    public static MarketHoursDatabase FromFile( String path) {
        return JsonConvert.DeserializeObject<MarketHoursDatabase>(File.ReadAllText(path));
    }

    /**
    /// Gets the entry for the specified market/symbol/security-type
    */
     * @param market">The market the exchange resides in, i.e, 'usa', 'fxcm', ect...
     * @param symbol">The particular symbol being traded
     * @param securityType">The security type of the symbol
     * @param @OverrideTimeZone">Specify this time zone to @Override the resolved time zone from the market hours database.
    /// This value will also be used as the time zone for SecurityType.Base with no market hours database entry.
    /// If null is specified, no @Override will be performed. If null is specified, and it's SecurityType.Base, then Utc will be used.
    @returns The entry matching the specified market/symbol/security-type
    public virtual Entry GetEntry( String market, String symbol, SecurityType securityType, ZoneId @OverrideTimeZone = null ) {
        Entry entry;
        key = new SecurityDatabaseKey(market, symbol, securityType);
        if( !_entries.TryGetValue(key, out entry)) {
            // now check with null symbol key
            if( !_entries.TryGetValue(new SecurityDatabaseKey(market, null, securityType), out entry)) {
                if( securityType == SecurityType.Base) {
                    if( @OverrideTimeZone == null ) {
                        @OverrideTimeZone = TimeZones.Utc;
                        Log.Error( "MarketHoursDatabase.GetExchangeHours(): Custom data no time zone specified, default to UTC. " + key);
                    }
                    // base securities are always open by default and have equal data time zone and exchange time zones
                    return new Entry(@OverrideTimeZone, SecurityExchangeHours.AlwaysOpen(@OverrideTimeZone));
                }

                Log.Error( String.format( "MarketHoursDatabase.GetExchangeHours(): Unable to locate exchange hours for %1$s." + "Available keys: %2$s", key, String.join( ", ", _entries.Keys)));

                // there was nothing that really matched exactly... what should we do here?
                throw new ArgumentException( "Unable to locate exchange hours for " + key);
            }

            // perform time zone @Override if requested, we'll use the same exact local hours
            // and holidays, but we'll express them in a different time zone
            if( @OverrideTimeZone != null && !entry.ExchangeHours.TimeZone.Equals(@OverrideTimeZone)) {
                return new Entry(@OverrideTimeZone, new SecurityExchangeHours(@OverrideTimeZone, entry.ExchangeHours.Holidays, entry.ExchangeHours.MarketHours));
            }
        }

        return entry;
    }

    /// Represents a single entry in the <see cref="MarketHoursDatabase"/>
    public class Entry {
        
        /// Gets the raw data time zone for this entry
        public final ZoneId DataTimeZone;

        /// Gets the exchange hours for this entry
        public final SecurityExchangeHours ExchangeHours;

        /// Initializes a new instance of the <see cref="Entry"/> class
         * @param dataTimeZone">The raw data time zone
         * @param exchangeHours">The security exchange hours for this entry
        public Entry( ZoneId dataTimeZone, SecurityExchangeHours exchangeHours ) {
            this.DataTimeZone = dataTimeZone;
            this.ExchangeHours = exchangeHours;
        }
    }

    class AlwaysOpenMarketHoursDatabase extends MarketHoursDatabase {
        @Override
        public Entry GetEntry( String market, String symbol, SecurityType securityType, ZoneId overrideTimeZone = null ) {
            tz = overrideTimeZone ?? TimeZones.Utc;
            return new Entry(tz, SecurityExchangeHours.AlwaysOpen(tz));
        }
    }
}
