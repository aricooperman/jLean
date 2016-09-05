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

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.Collection;
import java.util.Iterator;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.ForkJoinPool;

import com.quantconnect.lean.securities.Security;
import com.quantconnect.lean.Global.Resolution;
import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.SymbolCache;
import com.quantconnect.lean.TimeKeeper;
import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.data.SubscriptionManager;
import com.quantconnect.lean.event.CollectionChangedEvent;
import com.quantconnect.lean.event.CollectionChangedEventListener;
import com.quantconnect.lean.event.CollectionChangedEvent.CollectionChangedAction;

//using System.Collections.Concurrent;
//using System.Collections.Specialized;
//using System.Linq;
//using NodaTime;

/**
 * Enumerable security management class for grouping security objects into an array and providing any common properties.
 * Implements Map for the index searching of securities by symbol
 */
public class SecurityManager implements Map<Symbol,Security> {
    
    /**
     * Event fired when a security is added or removed from this collection
    */
    private CopyOnWriteArrayList<CollectionChangedEventListener> listeners = new CopyOnWriteArrayList<>();

    private final TimeKeeper _timeKeeper;

    //Internal dictionary implementation:
    private final ConcurrentMap<Symbol, Security> _securityManager;

    /**
     * Gets the most recent time this manager was updated
     */
    public LocalDateTime getUtcTime() {
        return _timeKeeper.getUtcTime();
    }

    /**
     * Initialize the algorithm security manager with two empty dictionaries
     * @param timeKeeper">
     */
    public SecurityManager( TimeKeeper timeKeeper ) {
        this._timeKeeper = timeKeeper;
        this._securityManager = new ConcurrentHashMap<Symbol,Security>();
    }

    /**
     * Add a new security with this symbol to the collection.
     * @param security security object
     */
    public void add( Security security ) {
        put( security.getSymbol(), security );
    }

    /**
     * Add a symbol-security by its key value pair.
     * Map implementation
     * @param pair
     */
    public void put( Entry<Symbol,Security> pair ) {
        put( pair.getKey(), pair.getValue() );
    }

    /**
     * Clear the securities array to delete all the portfolio and asset information.
     * Map implementation
     */
    public void clear() {
        _securityManager.clear();
    }

    /**
     * Check if this collection contains this key value pair.
     * @param pair Search key-value pair
     * Map implementation
     * @returns boolean true if contains this key-value pair
     */
    public boolean contains( Entry<Symbol, Security> pair ) {
        return _securityManager.containsKey( pair.getKey() );
    }

    /**
     * Check if this collection contains this symbol.
     * @param symbol Symbol we're checking for.
     * Map implementation
     * @returns boolean true if contains this symbol pair
     */
    public boolean containsKey( Symbol symbol ) {
        return _securityManager.containsKey( symbol );
    }

//    /**
//     * Copy from the internal array to an external array.
//     * @param array Array we're outputting to
//     * @param number Starting index of array
//     * Map implementation
//     */
//    public void copyTo( Entry<Symbol, Security>[] array, int number) {
//        ((Map<Symbol, Security>)_securityManager).copyTo(array, number);
//    }

    /**
     * Count of the number of securities in the collection.
     * Map implementation
     */
    public int size() {
        return _securityManager.size();
    }

    /**
     * Flag indicating if the internal array is read only.
     * Map implementation
     */
    public boolean isReadOnly() {
        return false;
    }

    /**
     * Remove a key value of of symbol-securities from the collections.
     * Map implementation
     * @param pair Key Value pair of symbol-security to remove
     * @returns Boolean true on success
     */
    public Security remove( Entry<Symbol, Security> pair ) {
        return remove( pair.getKey() );
    }

    /**
     * Remove this symbol security: Dictionary interface implementation.
     * @param symbol Symbol we're searching for
     * @returns true success
     */
    public Security remove( Symbol symbol ) {
        final Security security = _securityManager.remove( symbol );
        if( security != null ) {
            onCollectionChanged( new CollectionChangedEvent( CollectionChangedAction.Remove, security ) );
            return security;
        }
        
        return null;
    }

    /**
     * List of the symbol-keys in the collection of securities.
     * Map implementation
     */
    public Set<Symbol> keySet() {
        return _securityManager.keySet();
    }

    /**
     * Try and get this security object with matching symbol and return true on success.
     * @param symbol String search symbol
     * @param security Output Security object
     * Map implementation
     * @returns Security if it exists
     */
    public Security get( Symbol symbol ) {
        if( !_securityManager.containsKey( symbol ) )
            throw new IllegalArgumentException( String.format( "This asset symbol (%1$s) was not found in your security list. Please add this security or check it exists before using it with 'Securities.containsKey(\"%2$s\")'", 
                    symbol, SymbolCache.getTicker( symbol ) ) );
        
        return _securityManager.get( symbol );
    }

    /**
     * Get a list of the security objects for this collection.
     * Map implementation
     */
    public Collection<Security> values() {
        return _securityManager.values();
    }

    /**
     * Get the enumerator for this security collection.
     * Map implementation
     * @returns Enumerable key value pair
     */
    public Set<Entry<Symbol, Security>> entrySet() {
        return _securityManager.entrySet();
    }

    /**
     * Get the enumerator for this securities collection.
     * Map implementation
     * @return 
     * @returns Enumerator.
     */
    public Iterator<Map.Entry<Symbol,Security>> iterator() {
        return _securityManager.entrySet().iterator();
    }

    /**
     * Indexer method for the security manager to access the securities objects by their symbol.
     * Map implementation
     * @param symbol Symbol object indexer
     * @returns Security
     */
    public Security put( Symbol symbol, Security value ) {
        final Security existing = _securityManager.get( symbol );
        if( existing != null && !existing.equals( value ) )
            throw new IllegalArgumentException( "Unable to over write existing Security: " + symbol.toString() );

        // no security exists for the specified symbol key, add it now
        if( existing == null ) {
            _securityManager.put( symbol, value ) );
            value.setLocalTimeKeeper( _timeKeeper.getLocalTimeKeeper( value.getExchange().getTimeZone() ) );
            onCollectionChanged( new CollectionChangedEvent( CollectionChangedAction.Add, value ) );
        }

        return existing;
    }

    /**
     * Indexer method for the security manager to access the securities objects by their symbol.
     * Map implementation
     * @param ticker string ticker symbol indexer
     * @returns Security
     */
    public Security get( String ticker ) {
        final Symbol symbol = SymbolCache.getSymbol( ticker );
        if( symbol == null )
            throw new IllegalArgumentException( String.format( "This asset symbol (%1$s) was not found in your security list. Please add this security or check it exists before using it with 'Securities.ContainsKey(\"%1$s\")'", ticker ) );
        
        return get( symbol );
    }

    public Security put( String ticker, Security value ) {
        Symbol symbol = SymbolCache.getSymbol( ticker );
        if( symbol == null )
            throw new IllegalArgumentException( String.format( "This asset symbol (%1$s) was not found in your security list. Please add this security or check it exists before using it with 'Securities.containsKey(\"%1$s\")'", ticker ) );

        put( symbol, value );
    }

    /**
     * Event invocator for the <see cref="CollectionChanged"/> event
     * @param changedEvent Event arguments for the <see cref="CollectionChanged"/> event
     */
    protected void onCollectionChanged( final CollectionChangedEvent changedEvent ) {
        if( !listeners.isEmpty() )
            ForkJoinPool.commonPool().execute( () -> listeners.stream().forEach( l -> l.onCollectionChanged( changedEvent ) ) );
    }

    /**
     * Creates a security and matching configuration. This applies the default leverage if
     * leverage is less than or equal to zero.
     * This method also add the new symbol mapping to the <see cref="SymbolCache"/>
    */
    public static Security createSecurity( Class<?> factoryType,
        SecurityPortfolioManager securityPortfolioManager,
        SubscriptionManager subscriptionManager,
        SecurityExchangeHours exchangeHours,
        ZoneId dataTimeZone,
        SymbolProperties symbolProperties,
        ISecurityInitializer securityInitializer,
        Symbol symbol,
        Resolution resolution,
        boolean fillDataForward,
        BigDecimal leverage,
        boolean extendedMarketHours,
        boolean isInternalFeed,
        boolean isCustomData ) {
        
        return createSecurity( factoryType, securityPortfolioManager, subscriptionManager, exchangeHours, dataTimeZone, symbolProperties, 
                securityInitializer, symbol, resolution, fillDataForward, leverage, extendedMarketHours, isInternalFeed, isCustomData, true, true );
    }
    
    /**
     * Creates a security and matching configuration. This applies the default leverage if
     * leverage is less than or equal to zero.
     * This method also add the new symbol mapping to the <see cref="SymbolCache"/>
    */
    public static Security createSecurity( Class<? extends BaseData> factoryType,
        SecurityPortfolioManager securityPortfolioManager,
        SubscriptionManager subscriptionManager,
        SecurityExchangeHours exchangeHours,
        ZoneId dataTimeZone,
        SymbolProperties symbolProperties,
        ISecurityInitializer securityInitializer,
        Symbol symbol,
        Resolution resolution,
        boolean fillDataForward,
        BigDecimal leverage,
        boolean extendedMarketHours,
        boolean isInternalFeed,
        boolean isCustomData,
        boolean addToSymbolCache,
        boolean isFilteredSubscription ) {
        
        // add the symbol to our cache
        if( addToSymbolCache ) 
            SymbolCache.set( symbol.getValue(), symbol );

        //Add the symbol to Data Manager -- generate unified data streams for algorithm events
        final SubscriptionDataConfig config = subscriptionManager.add( factoryType, symbol, resolution, dataTimeZone, exchangeHours.getTimeZone(), isCustomData, fillDataForward,
            extendedMarketHours, isInternalFeed, isFilteredSubscription );

        // verify the cash book is in a ready state
        final String quoteCurrency = symbolProperties.getQuoteCurrency();
        if( !securityPortfolioManager.CashBook.containsKey( quoteCurrency ) ) {
            // since we have none it's safe to say the conversion is zero
            securityPortfolioManager.CashBook.add( quoteCurrency, BigDecimal.ZERO, BigDecimal.ZERO );
        }
        
        if( symbol.getId().getSecurityType() == SecurityType.Forex ) {
            // decompose the symbol into each currency pair
            String baseCurrency;
            Forex.Forex.DecomposeCurrencyPair(symbol.Value, out baseCurrency, out quoteCurrency);

            if( !securityPortfolioManager.CashBook.ContainsKey(baseCurrency)) {
                // since we have none it's safe to say the conversion is zero
                securityPortfolioManager.CashBook.Add(baseCurrency, 0, 0);
            }
            if( !securityPortfolioManager.CashBook.ContainsKey(quoteCurrency)) {
                // since we have none it's safe to say the conversion is zero
                securityPortfolioManager.CashBook.Add(quoteCurrency, 0, 0);
            }
        }
        
        quoteCash = securityPortfolioManager.CashBook[symbolProperties.QuoteCurrency];

        Security security;
        switch (config.SecurityType) {
            case SecurityType.Equity:
                security = new Equity.Equity(symbol, exchangeHours, quoteCash, symbolProperties);
                break;

            case SecurityType.Option:
                security = new Option.Option(exchangeHours, config, securityPortfolioManager.CashBook[CashBook.AccountCurrency], symbolProperties);
                break;

            case SecurityType.Forex:
                security = new Forex.Forex(symbol, exchangeHours, quoteCash, symbolProperties);
                break;

            case SecurityType.Cfd:
                security = new Cfd.Cfd(symbol, exchangeHours, quoteCash, symbolProperties);
                break;

            default:
            case SecurityType.Base:
                security = new Security(symbol, exchangeHours, quoteCash, symbolProperties);
                break;
        }

        // if we're just creating this security and it only has an internal
        // feed, mark it as non-tradable since the user didn't request this data
        if( !config.IsInternalFeed) {
            security.IsTradable = true;
        }

        security.AddData(config);

        // invoke the security initializer
        securityInitializer.Initialize(security);

        // if leverage was specified then apply to security after the initializer has run, parameters of this
        // method take precedence over the intializer
        if( leverage > 0) {
            security.SetLeverage(leverage);
        }

        return security;
    }

    /**
     * Creates a security and matching configuration. This applies the default leverage if
     * leverage is less than or equal to zero.
     * This method also add the new symbol mapping to the <see cref="SymbolCache"/>
    */
    public static Security CreateSecurity(SecurityPortfolioManager securityPortfolioManager,
        SubscriptionManager subscriptionManager,
        MarketHoursDatabase marketHoursDatabase,
        SymbolPropertiesDatabase symbolPropertiesDatabase,
        ISecurityInitializer securityInitializer,
        Symbol symbol,
        Resolution resolution,
        boolean fillDataForward,
        BigDecimal leverage,
        boolean extendedMarketHours,
        boolean isInternalFeed,
        boolean isCustomData,
        boolean addToSymbolCache = true) {
        marketHoursDbEntry = marketHoursDatabase.GetEntry(symbol.ID.Market, symbol.Value, symbol.ID.SecurityType);
        exchangeHours = marketHoursDbEntry.ExchangeHours;

        defaultQuoteCurrency = CashBook.AccountCurrency;
        if( symbol.ID.SecurityType == SecurityType.Forex) defaultQuoteCurrency = symbol.Value.Substring(3);
        symbolProperties = symbolPropertiesDatabase.GetSymbolProperties(symbol.ID.Market, symbol.Value, symbol.ID.SecurityType, defaultQuoteCurrency);

        type = resolution == Resolution.Tick ? typeof(Tick) : typeof(TradeBar);
        if( symbol.ID.SecurityType == SecurityType.Option && resolution != Resolution.Tick) {
            type = typeof(QuoteBar);
        }
        return CreateSecurity(type, securityPortfolioManager, subscriptionManager, exchangeHours, marketHoursDbEntry.DataTimeZone, symbolProperties, securityInitializer, symbol, resolution,
            fillDataForward, leverage, extendedMarketHours, isInternalFeed, isCustomData, addToSymbolCache);
    }
}
