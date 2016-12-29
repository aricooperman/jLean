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
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.quantconnect.lean.Currencies;
import com.quantconnect.lean.Global;
import com.quantconnect.lean.Resolution;
import com.quantconnect.lean.SecurityType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.data.SubscriptionManager;
import com.quantconnect.lean.data.market.Tick;
import com.quantconnect.lean.data.market.TradeBar;
import com.quantconnect.lean.securities.MarketHoursDatabase.DatabaseEntry;
import com.quantconnect.lean.securities.cfd.Cfd;
import com.quantconnect.lean.securities.forex.Forex;


/**
 * Represents a holding of a currency in cash.
 */
public class Cash {
    private final Logger log = LoggerFactory.getLogger( getClass() );
    private final Object locker = new Object();

    private boolean isBaseCurrency;
    private boolean invertRealTimePrice;
    private Symbol securitySymbol;
    private String symbol;
    private BigDecimal amount;

    protected BigDecimal conversionRate;


    /**
     * Gets the symbol of the security required to provide conversion rates.
     */
    public Symbol getSecuritySymbol() { 
        return securitySymbol;
    }
    
    /**
     * Gets the symbol used to represent this cash
     */
    public String getSymbol() { 
        return symbol;
    }
    
    /**
     * Gets or sets the amount of cash held
     */
    public BigDecimal getAmount() { 
        return amount;
    }
    
    /**
     * Gets the conversion rate into account currency
     */
    public BigDecimal getConversionRate() { 
        return conversionRate;
    }

    /**
     * Gets the value of this cash in the accout currency
     */
    public BigDecimal getValueInAccountCurrency() {
        return amount.multiply( conversionRate );
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

        this.amount = amount;
        this.conversionRate = conversionRate;
        this.symbol = symbol.toUpperCase();
    }

    /**
     * Updates this cash object with the specified data
     * @param data The new data for this cash object
     */
    public void update( BaseData data ) {
        if( isBaseCurrency ) 
            return;
        
        BigDecimal rate = data.getValue();
        if( invertRealTimePrice )
            rate = BigDecimal.ONE.divide( rate, RoundingMode.HALF_EVEN );
        
        conversionRate = rate;
    }

    /**
     * Adds the specified amount of currency to this Cash instance and returns the new total.
     * This operation is thread-safe
     * @param amount The amount of currency to be added
     * @returns The amount of currency directly after the addition
     */
    public BigDecimal addAmount( BigDecimal amt ) {
        synchronized( locker ) {
            amount = amount.add( amt );
            return amount;
        }
    }

    /**
     * Sets the Quantity to the specified amount
     * @param amount The amount to set the quantity to
     */
    public void setAmount( BigDecimal amount ) {
        synchronized( locker ) {
            this.amount = amount;
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
            SymbolPropertiesDatabase symbolPropertiesDatabase, Map<SecurityType,String> marketMap, CashBook cashBook ) {
        
        if( symbol.equals( CashBook.ACCOUNT_CURRENCY ) ) {
            this.securitySymbol = null;
            this.isBaseCurrency = true;
            this.conversionRate = BigDecimal.ONE;
            return null;
        }

        if( subscriptions.getCount() == 0 )
            throw new UnsupportedOperationException( "Unable to add cash when no subscriptions are present. Please add subscriptions in the Initialize() method." );

        // we require a subscription that converts this into the base currency
        final String normal = symbol + CashBook.ACCOUNT_CURRENCY;
        final String invert = CashBook.ACCOUNT_CURRENCY + symbol;
        for( SubscriptionDataConfig config : subscriptions.getSubscriptions().stream()
                .filter( config -> config.securityType == SecurityType.Forex || config.securityType == SecurityType.Cfd )
                .collect( Collectors.toList() ) ) {
            
            final Symbol symbol = config.getSymbol();
            final String symbolValue = symbol.getValue();
            if( symbolValue.equals( normal ) ) {
                this.securitySymbol = symbol;
                return null;
            }
            
            if( symbolValue.equals( invert ) ) {
                this.securitySymbol = symbol;
                this.invertRealTimePrice = true;
                return null;
            }
        }
        
        // if we've made it here we didn't find a subscription, so we'll need to add one
        final List<Symbol> currencyPairs = Currencies.CURRENCY_PAIRS.stream().map( x -> {
            // allow XAU or XAG to be used as quote currencies, but pairs including them are CFDs
            final SecurityType securityType = symbol.startsWith( "X" ) ? SecurityType.Cfd : SecurityType.Forex;
            String market = marketMap.get( securityType );
            return Symbol.create( x, securityType, market );
        } ).collect( Collectors.toList() );
        
        final Resolution minimumResolution = subscriptions.getSubscriptions().stream().map( x -> x.resolution ).min( Comparator.naturalOrder() ).orElse( Resolution.Minute );
        final Class<? extends BaseData> objectType = minimumResolution == Resolution.Tick ? Tick.class : TradeBar.class;
        
        for( Symbol symbol : currencyPairs ) {
            if( symbol.getValue().equals( normal ) || symbol.getValue().equals( invert ) ) {
                invertRealTimePrice = symbol.getValue().equals( invert );
                final SecurityType securityType = symbol.getId().getSecurityType();
                final SymbolProperties symbolProperties = symbolPropertiesDatabase.getSymbolProperties( symbol.getId().getMarket(), symbol.getValue(), securityType, symbol.toString() );
                Cash quoteCash = cashBook.get( symbolProperties.getQuoteCurrency() );
                if( quoteCash == null )
                    throw new RuntimeException( "Unable to resolve quote cash: " + symbolProperties.getQuoteCurrency() + ". This is required to add conversion feed: " + symbol.toString() );

                final DatabaseEntry marketHoursDbEntry = marketHoursDatabase.getEntry( symbol.getId().getMarket(), symbol.getValue(), symbol.getId().getSecurityType() );
                SecurityExchangeHours exchangeHours = marketHoursDbEntry.exchangeHours;
                // set this as an internal feed so that the data doesn't get sent into the algorithm's OnData events
                final SubscriptionDataConfig config = subscriptions.add( objectType, symbol, minimumResolution, marketHoursDbEntry.dataTimeZone, exchangeHours.getTimeZone(), false, true, false, true, true );
                securitySymbol = config.getSymbol();

                Security security;
                if( securityType == SecurityType.Cfd )
                    security = new Cfd( exchangeHours, quoteCash, config, symbolProperties );
                else
                    security = new Forex( exchangeHours, quoteCash, config, symbolProperties );
                securities.put( config.getSymbol(), security );
                log .trace( "Cash.EnsureCurrencyDataFeed(): Adding " + symbol.getValue() + " for cash " + symbol + " currency feed" );
                return security;
            }
        }

        // if this still hasn't been set then it's an error condition
        throw new IllegalArgumentException( String.format( "In order to maintain cash in %1$s you are required to add a subscription for Forex pair %1$s%2$s or %2$s%1$s", symbol, CashBook.ACCOUNT_CURRENCY ) );
    }
    
    @Override
    public int hashCode() {
        final int prime = 31;
        int result = 1;
        result = prime * result + ((amount == null) ? 0 : amount.hashCode());
        result = prime * result + ((conversionRate == null) ? 0 : conversionRate.hashCode());
        result = prime * result + ((securitySymbol == null) ? 0 : securitySymbol.hashCode());
        result = prime * result + ((symbol == null) ? 0 : symbol.hashCode());
        result = prime * result + (invertRealTimePrice ? 1231 : 1237);
        result = prime * result + (isBaseCurrency ? 1231 : 1237);
        return result;
    }

    @Override
    public boolean equals( Object obj ) {
        if( this == obj ) return true;
        if( obj == null ) return false;
        if( !(obj instanceof Cash) ) return false;
        
        final Cash other = (Cash)obj;
        if( amount == null ) {
            if( other.amount != null ) return false;
        }
        else if( amount.compareTo( other.amount ) != 0 ) return false;
        if( conversionRate == null ) {
            if( other.conversionRate != null ) return false;
        }
        else if( conversionRate.compareTo( other.conversionRate ) != 0 ) return false;
        if( securitySymbol == null ) {
            if( other.securitySymbol != null ) return false;
        }
        else if( !securitySymbol.equals( other.securitySymbol ) ) return false;
        if( symbol == null ) {
            if( other.symbol != null ) return false;
        }
        else if( !symbol.equals( other.symbol ) ) return false;
        if( invertRealTimePrice != other.invertRealTimePrice ) return false;
        if( isBaseCurrency != other.isBaseCurrency ) return false;
        return true;
    }

    /**
     * Returns a <see cref="System.String"/> that represents the current <see cref="QuantConnect.Securities.Cash"/>.
     * @returns A <see cref="System.String"/> that represents the current <see cref="QuantConnect.Securities.Cash"/>.
     */
    public String toString() {
        // round the conversion rate for output
        BigDecimal rate = conversionRate;
        rate = rate.compareTo( Global.ONE_THOUSAND ) < 0 ? rate.setScale( 5, RoundingMode.HALF_UP ) : rate.setScale( 2, RoundingMode.HALF_UP );
        return String.format( "%1$s: %2$15.2f @ %3$10.6f = %4$s %5$s", symbol, amount, rate, Currencies.CURRENCY_SYMBOLS.get( symbol ), getValueInAccountCurrency().setScale( 2 ) );
    }
}
