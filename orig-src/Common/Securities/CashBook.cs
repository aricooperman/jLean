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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuantConnect.Data;

package com.quantconnect.lean.Securities
{
    /**
    /// Provides a means of keeping track of the different cash holdings of an algorithm
    */
    public class CashBook : Map<String, Cash>
    {

        /**
        /// Gets the base currency used
        */
        public static final String AccountCurrency = "USD";

        private final Map<String, Cash> _currencies;

        /**
        /// Gets the total value of the cash book in units of the base currency
        */
        public BigDecimal TotalValueInAccountCurrency
        {
            get { return _currencies.Values.Sum(x -> x.ValueInAccountCurrency); }
        }

        /**
        /// Initializes a new instance of the <see cref="CashBook"/> class.
        */
        public CashBook() {
            _currencies = new Map<String, Cash>();
            _currencies.Add(AccountCurrency, new Cash(AccountCurrency, 0, 1.0m));
        }

        /**
        /// Adds a new cash of the specified symbol and quantity
        */
         * @param symbol">The symbol used to reference the new cash
         * @param quantity">The amount of new cash to start
         * @param conversionRate">The conversion rate used to determine the initial
        /// portfolio value/starting capital impact caused by this currency position.
        public void Add( String symbol, BigDecimal quantity, BigDecimal conversionRate) {
            cash = new Cash(symbol, quantity, conversionRate);
            _currencies.Add(symbol, cash);
        }

        /**
        /// Checks the current subscriptions and adds necessary currency pair feeds to provide real time conversion data
        */
         * @param securities">The SecurityManager for the algorithm
         * @param subscriptions">The SubscriptionManager for the algorithm
         * @param marketHoursDatabase">A security exchange hours provider instance used to resolve exchange hours for new subscriptions
         * @param symbolPropertiesDatabase">A symbol properties database instance
         * @param marketMap">The market map that decides which market the new security should be in
        @returns Returns a list of added currency securities
        public List<Security> EnsureCurrencyDataFeeds(SecurityManager securities, SubscriptionManager subscriptions, MarketHoursDatabase marketHoursDatabase, SymbolPropertiesDatabase symbolPropertiesDatabase, IReadOnlyMap<SecurityType,String> marketMap) {
            addedSecurities = new List<Security>();
            foreach (cash in _currencies.Values) {
                security = cash.EnsureCurrencyDataFeed(securities, subscriptions, marketHoursDatabase, symbolPropertiesDatabase, marketMap, this);
                if( security != null ) {
                    addedSecurities.Add(security);
                }
            }
            return addedSecurities;
        }

        /**
        /// Converts a quantity of source currency units into the specified destination currency
        */
         * @param sourceQuantity">The quantity of source currency to be converted
         * @param sourceCurrency">The source currency symbol
         * @param destinationCurrency">The destination currency symbol
        @returns The converted value
        public BigDecimal Convert( BigDecimal sourceQuantity, String sourceCurrency, String destinationCurrency) {
            source = this[sourceCurrency];
            destination = this[destinationCurrency];
            conversionRate = source.ConversionRate*destination.ConversionRate;
            return sourceQuantity*conversionRate;
        }

        /**
        /// Converts a quantity of source currency units into the account currency
        */
         * @param sourceQuantity">The quantity of source currency to be converted
         * @param sourceCurrency">The source currency symbol
        @returns The converted value
        public BigDecimal ConvertToAccountCurrency( BigDecimal sourceQuantity, String sourceCurrency) {
            return Convert(sourceQuantity, sourceCurrency, AccountCurrency);
        }

        /**
        /// Returns a String that represents the current object.
        */
        @returns 
        /// A String that represents the current object.
        /// 
        /// <filterpriority>2</filterpriority>
        public @Override String toString() {
            sb = new StringBuilder();
            sb.AppendLine( String.format( "%1$s {1,13}    {2,10} = {3}", "Symbol", "Quantity", "Conversion", "Value in " + AccountCurrency));
            foreach (value in Values) {
                sb.AppendLine(value.toString());
            }
            sb.AppendLine( "-------------------------------------------------");
            sb.AppendLine( String.format( "CashBook Total Value:                %1$s%2$s", 
                Currencies.CurrencySymbols[AccountCurrency], 
                Math.Round(TotalValueInAccountCurrency, 2))
                );

            return sb.toString();
        }

        #region IDictionary Implementation

        /**
        /// Gets the count of Cash items in this CashBook.
        */
        /// <value>The count.</value>
        public int Count
        {
            get { return _currencies.Count; }
        }

        /**
        /// Gets a value indicating whether this instance is read only.
        */
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public boolean IsReadOnly
        {
            get { return ((Map<String, Cash>) _currencies).IsReadOnly; }
        }

        /**
        /// Add the specified item to this CashBook.
        */
         * @param item">KeyValuePair of symbol -> Cash item
        public void Add(KeyValuePair<String, Cash> item) {
            _currencies.Add(item.Key, item.Value);
        }

        /**
        /// Add the specified key and value.
        */
         * @param symbol">The symbol of the Cash value.
         * @param value">Value.
        public void Add( String symbol, Cash value) {
            _currencies.Add(symbol, value);
        }

        /**
        /// Clear this instance of all Cash entries.
        */
        public void Clear() {
            _currencies.Clear();
        }

        /**
        /// Remove the Cash item corresponding to the specified symbol
        */
         * @param symbol">The symbolto be removed
        public boolean Remove( String symbol) {
            return _currencies.Remove (symbol);
        }

        /**
        /// Remove the specified item.
        */
         * @param item">Item.
        public boolean Remove(KeyValuePair<String, Cash> item) {
            return _currencies.Remove(item.Key);
        }

        /**
        /// Determines whether the current instance contains an entry with the specified symbol.
        */
        @returns <c>true</c>, if key was contained, <c>false</c> otherwise.
         * @param symbol">Key.
        public boolean ContainsKey( String symbol) {
            return _currencies.ContainsKey(symbol);
        }

        /**
        /// Try to get the value.
        */
        /// To be added.
        @returns <c>true</c>, if get value was tryed, <c>false</c> otherwise.
         * @param symbol">The symbol.
         * @param value">Value.
        public boolean TryGetValue( String symbol, out Cash value) {
            return _currencies.TryGetValue(symbol, out value);
        }

        /**
        /// Determines whether the current collection contains the specified value.
        */
         * @param item">Item.
        public boolean Contains(KeyValuePair<String, Cash> item) {
            return _currencies.Contains(item);
        }

        /**
        /// Copies to the specified array.
        */
         * @param array">Array.
         * @param arrayIndex">Array index.
        public void CopyTo(KeyValuePair<String, Cash>[] array, int arrayIndex) {
            ((Map<String, Cash>) _currencies).CopyTo(array, arrayIndex);
        }

        /**
        /// Gets or sets the <see cref="QuantConnect.Securities.Cash"/> with the specified symbol.
        */
         * @param symbol">Symbol.
        public Cash this[string symbol]
        {
            get
            {
                Cash cash;
                if( !_currencies.TryGetValue(symbol, out cash)) {
                    throw new Exception( "This cash symbol ( " + symbol + ") was not found in your cash book.");
                }
                return cash;
            }
            set { _currencies[symbol] = value; }
        }

        /**
        /// Gets the keys.
        */
        /// <value>The keys.</value>
        public ICollection<String> Keys
        {
            get { return _currencies.Keys; }
        }

        /**
        /// Gets the values.
        */
        /// <value>The values.</value>
        public ICollection<Cash> Values
        {
            get { return _currencies.Values; }
        }

        /**
        /// Gets the enumerator.
        */
        @returns The enumerator.
        public IEnumerator<KeyValuePair<String, Cash>> GetEnumerator() {
            return _currencies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable) _currencies).GetEnumerator();
        }

        #endregion
    }
}