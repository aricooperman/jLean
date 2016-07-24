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

using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;

namespace QuantConnect.Securities
{
    /// <summary>
    /// Represents a holding of a currency in cash.
    /// </summary>
    public class Cash
    {
        private boolean _isBaseCurrency;
        private boolean _invertRealTimePrice;

        private readonly object _locker = new object();

        /// <summary>
        /// Gets the symbol of the security required to provide conversion rates.
        /// </summary>
        public Symbol SecuritySymbol { get; private set; }

        /// <summary>
        /// Gets the symbol used to represent this cash
        /// </summary>
        public String Symbol { get; private set; }

        /// <summary>
        /// Gets or sets the amount of cash held
        /// </summary>
        public BigDecimal Amount { get; private set; }

        /// <summary>
        /// Gets the conversion rate into account currency
        /// </summary>
        public BigDecimal ConversionRate { get; internal set; }

        /// <summary>
        /// Gets the value of this cash in the accout currency
        /// </summary>
        public BigDecimal ValueInAccountCurrency
        {
            get { return Amount*ConversionRate; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cash"/> class
        /// </summary>
        /// <param name="symbol">The symbol used to represent this cash</param>
        /// <param name="amount">The amount of this currency held</param>
        /// <param name="conversionRate">The initial conversion rate of this currency into the <see cref="CashBook.AccountCurrency"/></param>
        public Cash( String symbol, BigDecimal amount, BigDecimal conversionRate)
        {
            if (symbol == null || symbol.Length != 3)
            {
                throw new ArgumentException("Cash symbols must be exactly 3 characters.");
            }
            Amount = amount;
            ConversionRate = conversionRate;
            Symbol = symbol.ToUpper();
        }

        /// <summary>
        /// Updates this cash object with the specified data
        /// </summary>
        /// <param name="data">The new data for this cash object</param>
        public void Update(BaseData data)
        {
            if (_isBaseCurrency) return;
            
            rate = data.Value;
            if (_invertRealTimePrice)
            {
                rate = 1/rate;
            }
            ConversionRate = rate;
        }

        /// <summary>
        /// Adds the specified amount of currency to this Cash instance and returns the new total.
        /// This operation is thread-safe
        /// </summary>
        /// <param name="amount">The amount of currency to be added</param>
        /// <returns>The amount of currency directly after the addition</returns>
        public BigDecimal AddAmount(decimal amount)
        {
            lock (_locker)
            {
                Amount += amount;
                return Amount;
            }
        }

        /// <summary>
        /// Sets the Quantity to the specified amount
        /// </summary>
        /// <param name="amount">The amount to set the quantity to</param>
        public void SetAmount(decimal amount)
        {
            lock (_locker)
            {
                Amount = amount;
            }
        }

        /// <summary>
        /// Ensures that we have a data feed to convert this currency into the base currency.
        /// This will add a subscription at the lowest resolution if one is not found.
        /// </summary>
        /// <param name="securities">The security manager</param>
        /// <param name="subscriptions">The subscription manager used for searching and adding subscriptions</param>
        /// <param name="marketHoursDatabase">A security exchange hours provider instance used to resolve exchange hours for new subscriptions</param>
        /// <param name="symbolPropertiesDatabase">A symbol properties database instance</param>
        /// <param name="marketMap">The market map that decides which market the new security should be in</param>
        /// <param name="cashBook">The cash book - used for resolving quote currencies for created conversion securities</param>
        /// <returns>Returns the added currency security if needed, otherwise null</returns>
        public Security EnsureCurrencyDataFeed(SecurityManager securities, SubscriptionManager subscriptions, MarketHoursDatabase marketHoursDatabase, SymbolPropertiesDatabase symbolPropertiesDatabase, IReadOnlyDictionary<SecurityType, string> marketMap, CashBook cashBook)
        {
            if (Symbol == CashBook.AccountCurrency)
            {
                SecuritySymbol = QuantConnect.Symbol.Empty;
                _isBaseCurrency = true;
                ConversionRate = 1.0m;
                return null;
            }

            if (subscriptions.Count == 0)
            {
                throw new InvalidOperationException("Unable to add cash when no subscriptions are present. Please add subscriptions in the Initialize() method.");
            }

            // we require a subscription that converts this into the base currency
            String normal = Symbol + CashBook.AccountCurrency;
            String invert = CashBook.AccountCurrency + Symbol;
            foreach (config in subscriptions.Subscriptions.Where(config => config.SecurityType == SecurityType.Forex || config.SecurityType == SecurityType.Cfd))
            {
                if (config.Symbol.Value == normal)
                {
                    SecuritySymbol = config.Symbol;
                    return null;
                }
                if (config.Symbol.Value == invert)
                {
                    SecuritySymbol = config.Symbol;
                    _invertRealTimePrice = true;
                    return null;
                }
            }
            // if we've made it here we didn't find a subscription, so we'll need to add one
            currencyPairs = Currencies.CurrencyPairs.Select(x =>
            {
                // allow XAU or XAG to be used as quote currencies, but pairs including them are CFDs
                securityType = Symbol.StartsWith("X") ? SecurityType.Cfd : SecurityType.Forex;
                market = marketMap[securityType];
                return QuantConnect.Symbol.Create(x, securityType, market);
            });
            minimumResolution = subscriptions.Subscriptions.Select(x => x.Resolution).DefaultIfEmpty(Resolution.Minute).Min();
            objectType = minimumResolution == Resolution.Tick ? typeof (Tick) : typeof (TradeBar);
            foreach (symbol in currencyPairs)
            {
                if (symbol.Value == normal || symbol.Value == invert)
                {
                    _invertRealTimePrice = symbol.Value == invert;
                    securityType = symbol.ID.SecurityType;
                    symbolProperties = symbolPropertiesDatabase.GetSymbolProperties(symbol.ID.Market, symbol.Value, securityType, Symbol);
                    Cash quoteCash;
                    if (!cashBook.TryGetValue(symbolProperties.QuoteCurrency, out quoteCash))
                    {
                        throw new Exception("Unable to resolve quote cash: " + symbolProperties.QuoteCurrency + ". This is required to add conversion feed: " + symbol.ToString());
                    }
                    marketHoursDbEntry = marketHoursDatabase.GetEntry(symbol.ID.Market, symbol.Value, symbol.ID.SecurityType);
                    exchangeHours = marketHoursDbEntry.ExchangeHours;
                    // set this as an internal feed so that the data doesn't get sent into the algorithm's OnData events
                    config = subscriptions.Add(objectType, symbol, minimumResolution, marketHoursDbEntry.DataTimeZone, exchangeHours.TimeZone, false, true, false, true);
                    SecuritySymbol = config.Symbol;

                    Security security;
                    if (securityType == SecurityType.Cfd)
                    {
                        security = new Cfd.Cfd(exchangeHours, quoteCash, config, symbolProperties);
                    }
                    else
                    {
                        security = new Forex.Forex(exchangeHours, quoteCash, config, symbolProperties);
                    }
                    securities.Add(config.Symbol, security);
                    Log.Trace("Cash.EnsureCurrencyDataFeed(): Adding " + symbol.Value + " for cash " + Symbol + " currency feed");
                    return security;
                }
            }

            // if this still hasn't been set then it's an error condition
            throw new ArgumentException( String.Format("In order to maintain cash in {0} you are required to add a subscription for Forex pair {0}{1} or {1}{0}", Symbol, CashBook.AccountCurrency));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="QuantConnect.Securities.Cash"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="QuantConnect.Securities.Cash"/>.</returns>
        public override String ToString()
        {
            // round the conversion rate for output
            BigDecimal rate = ConversionRate;
            rate = rate < 1000 ? rate.RoundToSignificantDigits(5) : Math.Round(rate, 2);
            return string.Format("{0}: {1,15} @ ${2,10} = {3}{4}", 
                Symbol, 
                Amount.ToString("0.00"), 
                rate.ToString("0.00####"), 
                Currencies.CurrencySymbols[Symbol], 
                Math.Round(ValueInAccountCurrency, 2)
                );
        }
    }
}