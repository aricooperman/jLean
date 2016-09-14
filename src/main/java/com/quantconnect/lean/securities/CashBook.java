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

package com.quantconnect.lean.securities;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

import com.google.common.collect.ImmutableMap;
import com.quantconnect.lean.Currencies;
import com.quantconnect.lean.SecurityType;
import com.quantconnect.lean.data.SubscriptionManager;


/**
 * Provides a means of keeping track of the different cash holdings of an algorithm
 */
public class CashBook implements Map<String,Cash> {
    
    /**
     * Gets the base currency used
     */
    public static final String ACCOUNT_CURRENCY = "USD";

    private final Map<String,Cash> currencies;

    /**
     * Gets the total value of the cash book in units of the base currency
     */
    public BigDecimal getTotalValueInAccountCurrency() {
        return currencies.values().stream()
                .map( x -> x.getValueInAccountCurrency() )
                .reduce( BigDecimal.ZERO, BigDecimal::add );
    }

    /**
     * Initializes a new instance of the <see cref="CashBook"/> class.
     */
    public CashBook() {
        currencies = new HashMap<String,Cash>();
        currencies.put( ACCOUNT_CURRENCY, new Cash( ACCOUNT_CURRENCY, BigDecimal.ZERO, BigDecimal.ONE ) );
    }

    /**
     * Adds a new cash of the specified symbol and quantity
     * @param symbol The symbol used to reference the new cash
     * @param quantity The amount of new cash to start
     * @param conversionRate The conversion rate used to determine the initial
     *        portfolio value/starting capital impact caused by this currency position.
     */
    public void add( String symbol, BigDecimal quantity, BigDecimal conversionRate ) {
        currencies.put( symbol, new Cash( symbol, quantity, conversionRate ) );
    }

    /**
     * Checks the current subscriptions and adds necessary currency pair feeds to provide real time conversion data
     * @param securities The SecurityManager for the algorithm
     * @param subscriptions The SubscriptionManager for the algorithm
     * @param marketHoursDatabase A security exchange hours provider instance used to resolve exchange hours for new subscriptions
     * @param symbolPropertiesDatabase A symbol properties database instance
     * @param marketMap The market map that decides which market the new security should be in
     * @returns Returns a list of added currency securities
     */
    public List<Security> ensureCurrencyDataFeeds( SecurityManager securities, SubscriptionManager subscriptions, 
            MarketHoursDatabase marketHoursDatabase, SymbolPropertiesDatabase symbolPropertiesDatabase, ImmutableMap<SecurityType,String> marketMap ) {
        final List<Security> addedSecurities = new ArrayList<Security>();
        for( Cash cash : currencies.values() ) {
            final Security security = cash.ensureCurrencyDataFeed( securities, subscriptions, marketHoursDatabase, symbolPropertiesDatabase, marketMap, this );
            if( security != null )
                addedSecurities.add( security );
        }
        
        return addedSecurities;
    }

    /**
     * Converts a quantity of source currency units into the specified destination currency
     * @param sourceQuantity The quantity of source currency to be converted
     * @param sourceCurrency The source currency symbol
     * @param destinationCurrency The destination currency symbol
     * @returns The converted value
     */
    public BigDecimal convert( BigDecimal sourceQuantity, String sourceCurrency, String destinationCurrency ) {
        final Cash source = get( sourceCurrency );
        final Cash destination = get( destinationCurrency );
        final BigDecimal conversionRate = source.getConversionRate().multiply( destination.getConversionRate() );
        return sourceQuantity.multiply( conversionRate );
    }

    /**
     * Converts a quantity of source currency units into the account currency
     * @param sourceQuantity The quantity of source currency to be converted
     * @param sourceCurrency The source currency symbol
     * @returns The converted value
     */
    public BigDecimal convertToAccountCurrency( BigDecimal sourceQuantity, String sourceCurrency ) {
        return convert( sourceQuantity, sourceCurrency, ACCOUNT_CURRENCY );
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        final StringBuilder sb = new StringBuilder();
        sb.append( String.format( "%1$s {1,13}    {2,10} = %4$s", "Symbol", "Quantity", "Conversion", "Value in " + ACCOUNT_CURRENCY ) ).append( '\n' );
        for( Cash value : values() )
            sb.append( value.toString() ).append( '\n' );

        sb.append( "-------------------------------------------------\n");
        sb.append( String.format( "CashBook Total Value:                %1$s%2$s", 
            Currencies.CURRENCY_SYMBOLS.get( ACCOUNT_CURRENCY ), 
            getTotalValueInAccountCurrency().setScale( 2, RoundingMode.HALF_UP ) ) ).append( '\n' );

        return sb.toString();
    }

    /* Map Implementation */

    /**
     * Gets the count of Cash items in this CashBook.
     * @returns The count.
    */
    public int size() {
        return currencies.size();
    }

    /**
     * Gets a value indicating whether this instance is read only.
     * @returns <c>true</c> if this instance is read only; otherwise, <c>false</c>.
    */
    public boolean isReadOnly() {
        return (currencies instanceof ImmutableMap);
    }

    /**
     * Add the specified item to this CashBook.
     * @param item KeyValuePair of symbol -> Cash item
     */
    public void add( Entry<String,Cash> item ) {
        currencies.put( item.getKey(), item.getValue() );
    }

    /**
     * Add the specified key and value.
     * @param symbol The symbol of the Cash value.
     * @param value Value.
     * @return 
     */
    public Cash put( String symbol, Cash value ) {
        return currencies.put( symbol, value );
    }

    /**
     * Clear this instance of all Cash entries.
     */
    public void clear() {
        currencies.clear();
    }

    /**
     * Remove the Cash item corresponding to the specified symbol
     * @param symbol The symbol to be removed
     */
    public Cash remove( Object symbol ) {
        return currencies.remove( symbol );
    }

    /**
     * Remove the specified item.
     * @param item Item.
     */
    public boolean remove( Entry<String,Cash> item ) {
        return currencies.remove( item.getKey() ) != null;
    }

    /**
     * Determines whether the current instance contains an entry with the specified symbol.
     * @param symbol Key.
     * @returns <c>true</c>, if key was contained, <c>false</c> otherwise.
     */
    public boolean containsKey( Object symbol ) {
        return currencies.containsKey( symbol );
    }

    /**
     * Determines whether the current collection contains the specified value.
     * @param item Item.
     */
    public boolean contains( Entry<String,Cash> item ) {
        final Cash cash = currencies.get( item.getKey() );
        return cash != null && cash.equals( item.getValue() );
    }

    @Override
    public boolean containsValue( Object value ) {
        return currencies.containsValue( value );
    }
    
//    /**
//     * Copies to the specified array.
//     * @param array Array.
//     * @param arrayIndex Array index.
//     */
//    public void copyTo( KeyValuePair<String, Cash>[] array, int arrayIndex) {
//        ((Map<String, Cash>) _currencies).CopyTo(array, arrayIndex);
//    }

    /**
     * Gets the keys.
     * @returns The keys.
     */
    public Set<String> keySet() {
        return currencies.keySet();
    }

    /**
     * Gets the values.
     * @returns The values.
     */
    public Collection<Cash> values() {
        return currencies.values();
    }

    @Override
    public boolean isEmpty() {
        return currencies.isEmpty();
    }

    @Override
    public Cash get( Object symbol ) {
        final Cash cash = currencies.get( symbol );
        if( cash == null )
            throw new RuntimeException( "This cash symbol ( " + symbol + ") was not found in your cash book.");

        return cash;
    }

    @Override
    public void putAll( Map<? extends String,? extends Cash> m ) {
        currencies.putAll( m );
    }

    @Override
    public Set<Entry<String,Cash>> entrySet() {
        return currencies.entrySet();
    }
}
