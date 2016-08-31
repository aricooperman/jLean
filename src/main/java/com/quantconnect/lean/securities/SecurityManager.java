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

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

import com.quantconnect.lean.securities.Security;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.TimeKeeper;

//using System.Collections.Concurrent;
//using System.Collections.Specialized;
//using System.Linq;
//using NodaTime;

/**
 * Enumerable security management class for grouping security objects into an array and providing any common properties.
 * Implements Map for the index searching of securities by symbol
 */
public class SecurityManager implements Map<Symbol,Security>, INotifyCollectionChanged {
    
    /**
     * Event fired when a security is added or removed from this collection
    */
    public /*event*/ NotifyCollectionChangedEventHandler CollectionChanged;

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
     * Initialise the algorithm security manager with two empty dictionaries
     * @param timeKeeper">
     */
    public SecurityManager( TimeKeeper timeKeeper ) {
        this._timeKeeper = timeKeeper;
        this._securityManager = new ConcurrentHashMap<Symbol,Security>();
    }

    /**
     * Add a new security with this symbol to the collection.
     * Map implementation
     * @param symbol symbol for security we're trading
     * @param security security object
     * <seealso cref="Add(Security)"/>
    */
    public void add( Symbol symbol, Security security ) {
        if( _securityManager.TryAdd( symbol, security ) ) {
            security.SetLocalTimeKeeper(_timeKeeper.GetLocalTimeKeeper(security.Exchange.TimeZone));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, security));
        }
    }

    /**
     * Add a new security with this symbol to the collection.
     * @param security security object
     */
    public void add( Security security ) {
        put( security.Symbol, security );
    }

    /**
     * Add a symbol-security by its key value pair.
     * Map implementation
     * @param pair
     */
    public void add( Entry<Symbol,Security> pair ) {
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
    */
     * @param pair Search key-value pair
     * Map implementation
    @returns Bool true if contains this key-value pair
    public boolean Contains(KeyValuePair<Symbol, Security> pair) {
        return _securityManager.Contains(pair);
    }

    /**
     * Check if this collection contains this symbol.
    */
     * @param symbol Symbol we're checking for.
     * Map implementation
    @returns Bool true if contains this symbol pair
    public boolean ContainsKey(Symbol symbol) {
        return _securityManager.ContainsKey(symbol);
    }

    /**
     * Copy from the internal array to an external array.
    */
     * @param array Array we're outputting to
     * @param number Starting index of array
     * Map implementation
    public void CopyTo(KeyValuePair<Symbol, Security>[] array, int number) {
        ((Map<Symbol, Security>)_securityManager).CopyTo(array, number);
    }

    /**
     * Count of the number of securities in the collection.
    */
     * Map implementation
    public int Count
    {
        get { return _securityManager.Count; }
    }

    /**
     * Flag indicating if the internal arrray is read only.
    */
     * Map implementation
    public boolean IsReadOnly
    {
        get { return false;  }
    }

    /**
     * Remove a key value of of symbol-securities from the collections.
    */
     * Map implementation
     * @param pair Key Value pair of symbol-security to remove
    @returns Boolean true on success
    public boolean Remove(KeyValuePair<Symbol, Security> pair) {
        return Remove(pair.Key);
    }

    /**
     * Remove this symbol security: Dictionary interface implementation.
    */
     * @param symbol Symbol we're searching for
    @returns true success
    public boolean Remove(Symbol symbol) {
        Security security;
        if( _securityManager.TryRemove(symbol, out security)) {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, security));
            return true;
        }
        return false;
    }

    /**
     * List of the symbol-keys in the collection of securities.
    */
     * Map implementation
    public ICollection<Symbol> Keys
    {
        get { return _securityManager.Keys; }
    }

    /**
     * Try and get this security object with matching symbol and return true on success.
    */
     * @param symbol String search symbol
     * @param security Output Security object
     * Map implementation
    @returns True on successfully locating the security object
    public boolean TryGetValue(Symbol symbol, out Security security) {
        return _securityManager.TryGetValue(symbol, out security);
    }

    /**
     * Get a list of the security objects for this collection.
    */
     * Map implementation
    public ICollection<Security> Values
    {
        get { return _securityManager.Values; }
    }

    /**
     * Get the enumerator for this security collection.
    */
     * Map implementation
    @returns Enumerable key value pair
    IEnumerator<KeyValuePair<Symbol, Security>> IEnumerable<KeyValuePair<Symbol, Security>>.GetEnumerator() {
        return _securityManager.GetEnumerator();
    }

    /**
     * Get the enumerator for this securities collection.
    */
     * Map implementation
    @returns Enumerator.
    IEnumerator IEnumerable.GetEnumerator() {
        return _securityManager.GetEnumerator();
    }

    /**
     * Indexer method for the security manager to access the securities objects by their symbol.
    */
     * Map implementation
     * @param symbol Symbol object indexer
    @returns Security
    public Security this[Symbol symbol]
    {
        get 
        {
            if( !_securityManager.ContainsKey(symbol)) {
                throw new Exception( String.format( "This asset symbol (%1$s) was not found in your security list. Please add this security or check it exists before using it with 'Securities.ContainsKey(\"%2$s\")'", symbol, SymbolCache.GetTicker(symbol)));
            } 
            return _securityManager[symbol];
        }
        set
        {
            Security existing;
            if( _securityManager.TryGetValue(symbol, out existing) && existing != value) {
                throw new IllegalArgumentException( "Unable to over write existing Security: " + symbol.toString());
            }

            // no security exists for the specified symbol key, add it now
            if( existing == null ) {
                Add(symbol, value);
            }
        }
    }

    /**
     * Indexer method for the security manager to access the securities objects by their symbol.
    */
     * Map implementation
     * @param ticker string ticker symbol indexer
    @returns Security
    public Security this[string ticker]
    {
        get
        {
            Symbol symbol;
            if( !SymbolCache.TryGetSymbol(ticker, out symbol)) {
                throw new Exception( String.format( "This asset symbol (%1$s) was not found in your security list. Please add this security or check it exists before using it with 'Securities.ContainsKey(\"%1$s\")'", ticker));
            }
            return this[symbol];
        }
        set
        {
            Symbol symbol;
            if( !SymbolCache.TryGetSymbol(ticker, out symbol)) {
                throw new Exception( String.format( "This asset symbol (%1$s) was not found in your security list. Please add this security or check it exists before using it with 'Securities.ContainsKey(\"%1$s\")'", ticker));
            }
            this[symbol] = value;
        }
    }

    /**
     * Event invocator for the <see cref="CollectionChanged"/> event
    */
     * @param changedEventArgs Event arguments for the <see cref="CollectionChanged"/> event
    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs changedEventArgs) {
        handler = CollectionChanged;
        if( handler != null ) handler(this, changedEventArgs);
    }

    /**
     * Creates a security and matching configuration. This applies the default leverage if
     * leverage is less than or equal to zero.
     * This method also add the new symbol mapping to the <see cref="SymbolCache"/>
    */
    public static Security CreateSecurity(Type factoryType,
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
        boolean addToSymbolCache = true,
        boolean isFilteredSubscription = true) {
        // add the symbol to our cache
        if( addToSymbolCache) SymbolCache.Set(symbol.Value, symbol);

        //Add the symbol to Data Manager -- generate unified data streams for algorithm events
        config = subscriptionManager.Add(factoryType, symbol, resolution, dataTimeZone, exchangeHours.TimeZone, isCustomData, fillDataForward,
            extendedMarketHours, isInternalFeed, isFilteredSubscription);

        // verify the cash book is in a ready state
        quoteCurrency = symbolProperties.QuoteCurrency;
        if( !securityPortfolioManager.CashBook.ContainsKey(quoteCurrency)) {
            // since we have none it's safe to say the conversion is zero
            securityPortfolioManager.CashBook.Add(quoteCurrency, 0, 0);
        }
        if( symbol.ID.SecurityType == SecurityType.Forex) {
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
