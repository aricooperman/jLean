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
import java.math.RoundingMode;

import com.google.common.collect.ImmutableMap;
import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.SubscriptionManager;

/**
 * Represents a holding of a currency in cash.
 */
public class Cash {
    private boolean _isBaseCurrency;
    private boolean _invertRealTimePrice;

    private Symbol SecuritySymbol;
    private String Symbol;
    private BigDecimal Amount;

    protected BigDecimal ConversionRate;

    private final Object _locker = new Object();

    /**
     * Gets the symbol of the security required to provide conversion rates.
     */
    public Symbol getSecuritySymbol() { 
        return SecuritySymbol;
    }
    
    /**
     * Gets the symbol used to represent this cash
     */
    public String getSymbol() { 
        return Symbol;
    }
    
    /**
     * Gets or sets the amount of cash held
     */
    public BigDecimal getAmount() { 
        return Amount;
    }
    
    /**
     * Gets the conversion rate into account currency
     */
    public BigDecimal getConversionRate() { 
        return ConversionRate;
    }

    /**
     * Gets the value of this cash in the accout currency
     */
    public BigDecimal getValueInAccountCurrency() {
        return Amount.multiply( ConversionRate );
    }

    /**
     * Initializes a new instance of the <see cref="Cash"/> class
     * @param symbol The symbol used to represent this cash
     * @param amount The amount of this currency held
     * @param conversionRate The initial conversion rate of this currency into the <see cref="CashBook.AccountCurrency"/>
     */
    public Cash( String symbol, BigDecimal amount, BigDecimal conversionRate ) {
        if( symbol == null || symbol.length() != 3 )
            throw new IllegalArgumentException( "Cash symbols must be exactly 3 characters." );

        this.Amount = amount;
        this.ConversionRate = conversionRate;
        this.Symbol = symbol.toUpperCase();
    }

    /**
     * Updates this cash object with the specified data
     * @param data The new data for this cash object
     */
    public void update( BaseData data ) {
        if( _isBaseCurrency ) 
            return;
        
        BigDecimal rate = data.getValue();
        if( _invertRealTimePrice )
            rate = BigDecimal.ONE.divide( rate, RoundingMode.HALF_EVEN );
        
        ConversionRate = rate;
    }

    /**
     * Adds the specified amount of currency to this Cash instance and returns the new total.
     * This operation is thread-safe
     * @param amount The amount of currency to be added
     * @returns The amount of currency directly after the addition
     */
    public BigDecimal addAmount( BigDecimal amt ) {
        synchronized( _locker ) {
            Amount = Amount.add( amt );
            return Amount;
        }
    }

    /**
     * Sets the Quantity to the specified amount
     * @param amount The amount to set the quantity to
     */
    public void setAmount( BigDecimal amount ) {
        synchronized( _locker ) {
            this.Amount = amount;
        }
    }

    /**
     * Ensures that we have a data feed to convert this currency into the base currency.
     * This will add a subscription at the lowest resolution if one is not found.
     * @param securities The security manager
     * @param subscriptions The subscription manager used for searching and adding subscriptions
     * @param marketHoursDatabase A security exchange hours provider instance used to resolve exchange hours for new subscriptions
     * @param symbolPropertiesDatabase A symbol properties database instance
     * @param marketMap The market map that decides which market the new security should be in
     * @param cashBook The cash book used for resolving quote currencies for created conversion securities
     * @returns Returns the added currency security if needed, otherwise null
     */
    public Security ensureCurrencyDataFeed( SecurityManager securities, SubscriptionManager subscriptions, MarketHoursDatabase marketHoursDatabase, 
            SymbolPropertiesDatabase symbolPropertiesDatabase, ImmutableMap<SecurityType,String> marketMap, CashBook cashBook ) {
        if( Symbol == CashBook.AccountCurrency ) {
            SecuritySymbol = QuantConnect.Symbol.Empty;
            _isBaseCurrency = true;
            ConversionRate = BigDecimal.ONE;
            return null;
        }

        if( subscriptions.Count == 0)
            throw new UnsupportedOperationException( "Unable to add cash when no subscriptions are present. Please add subscriptions in the Initialize() method." );

        // we require a subscription that converts this into the base currency
        String normal = Symbol + CashBook.AccountCurrency;
        String invert = CashBook.AccountCurrency + Symbol;
        foreach (config in subscriptions.Subscriptions.Where(config -> config.SecurityType == SecurityType.Forex || config.SecurityType == SecurityType.Cfd)) {
            if( config.Symbol.Value == normal) {
                SecuritySymbol = config.Symbol;
                return null;
            }
            if( config.Symbol.Value == invert) {
                SecuritySymbol = config.Symbol;
                _invertRealTimePrice = true;
                return null;
            }
        }
        // if we've made it here we didn't find a subscription, so we'll need to add one
        currencyPairs = Currencies.CurrencyPairs.Select( x -> {
            // allow XAU or XAG to be used as quote currencies, but pairs including them are CFDs
            securityType = Symbol.StartsWith( "X") ? SecurityType.Cfd : SecurityType.Forex;
            market = marketMap[securityType];
            return QuantConnect.Symbol.Create(x, securityType, market);
        } );
        minimumResolution = subscriptions.Subscriptions.Select(x -> x.Resolution).DefaultIfEmpty(Resolution.Minute).Min();
        objectType = minimumResolution == Resolution.Tick ? typeof (Tick) : typeof (TradeBar);
        for( symbol : currencyPairs ) {
            if( symbol.Value == normal || symbol.Value == invert ) {
                _invertRealTimePrice = symbol.Value == invert;
                securityType = symbol.ID.SecurityType;
                symbolProperties = symbolPropertiesDatabase.GetSymbolProperties(symbol.ID.Market, symbol.Value, securityType, Symbol);
                Cash quoteCash;
                if( !cashBook.TryGetValue(symbolProperties.QuoteCurrency, out quoteCash))
                    throw new Exception( "Unable to resolve quote cash: " + symbolProperties.QuoteCurrency + ". This is required to add conversion feed: " + symbol.toString());

                marketHoursDbEntry = marketHoursDatabase.GetEntry(symbol.ID.Market, symbol.Value, symbol.ID.SecurityType);
                exchangeHours = marketHoursDbEntry.ExchangeHours;
                // set this as an internal feed so that the data doesn't get sent into the algorithm's OnData events
                config = subscriptions.Add(objectType, symbol, minimumResolution, marketHoursDbEntry.DataTimeZone, exchangeHours.TimeZone, false, true, false, true);
                SecuritySymbol = config.Symbol;

                Security security;
                if( securityType == SecurityType.Cfd)
                    security = new Cfd.Cfd(exchangeHours, quoteCash, config, symbolProperties);
                else
                    security = new Forex.Forex(exchangeHours, quoteCash, config, symbolProperties);
                securities.Add(config.Symbol, security);
                Log.Trace( "Cash.EnsureCurrencyDataFeed(): Adding " + symbol.Value + " for cash " + Symbol + " currency feed");
                return security;
            }
        }

        // if this still hasn't been set then it's an error condition
        throw new IllegalArgumentException( String.format( "In order to maintain cash in %1$s you are required to add a subscription for Forex pair %1$s%2$s or %2$s%1$s", Symbol, CashBook.AccountCurrency));
    }
    
    @Override
    public int hashCode() {
        final int prime = 31;
        int result = 1;
        result = prime * result + ((Amount == null) ? 0 : Amount.hashCode());
        result = prime * result + ((ConversionRate == null) ? 0 : ConversionRate.hashCode());
        result = prime * result + ((SecuritySymbol == null) ? 0 : SecuritySymbol.hashCode());
        result = prime * result + ((Symbol == null) ? 0 : Symbol.hashCode());
        result = prime * result + (_invertRealTimePrice ? 1231 : 1237);
        result = prime * result + (_isBaseCurrency ? 1231 : 1237);
        return result;
    }

    @Override
    public boolean equals( Object obj ) {
        if( this == obj ) return true;
        if( obj == null ) return false;
        if( !(obj instanceof Cash) ) return false;
        Cash other = (Cash)obj;
        if( Amount == null ) {
            if( other.Amount != null ) return false;
        }
        else if( !Amount.equals( other.Amount ) ) return false;
        if( ConversionRate == null ) {
            if( other.ConversionRate != null ) return false;
        }
        else if( !ConversionRate.equals( other.ConversionRate ) ) return false;
        if( SecuritySymbol == null ) {
            if( other.SecuritySymbol != null ) return false;
        }
        else if( !SecuritySymbol.equals( other.SecuritySymbol ) ) return false;
        if( Symbol == null ) {
            if( other.Symbol != null ) return false;
        }
        else if( !Symbol.equals( other.Symbol ) ) return false;
        if( _invertRealTimePrice != other._invertRealTimePrice ) return false;
        if( _isBaseCurrency != other._isBaseCurrency ) return false;
        return true;
    }

    /**
     * Returns a <see cref="System.String"/> that represents the current <see cref="QuantConnect.Securities.Cash"/>.
     * @returns A <see cref="System.String"/> that represents the current <see cref="QuantConnect.Securities.Cash"/>.
     */
    public String toString() {
        // round the conversion rate for output
        BigDecimal rate = ConversionRate;
        rate = rate < 1000 ? rate.setScale( 5, RoundingMode.HALF_UP ) : Math.round( rate, 2 );
        return String.format( "%1$s: {1,15} @ ${2,10} = %4$s%5$s", 
            Symbol, 
            Amount.toString( "0.00" ), 
            rate.toString( "0.00####"), 
            Currencies.CurrencySymbols[Symbol], 
            Math.Round( ValueInAccountCurrency, 2 )
            );
    }
}
