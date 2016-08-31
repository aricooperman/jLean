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
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Securities.Option;
using QuantConnect.Util;

package com.quantconnect.lean.Data.UniverseSelection
{
    /**
     * Defines a universe for a single option chain
    */
    public class OptionChainUniverse : Universe
    {
        private static final IReadOnlyList<TickType> QuotesAndTrades = new[] { TickType.Quote, TickType.Trade };

        private BaseData _underlying;
        private final Option _option;
        private final UniverseSettings _universeSettings;

        /**
         * Initializes a new instance of the <see cref="OptionChainUniverse"/> class
        */
         * @param option The canonical option chain security
         * @param universeSettings The universe settings to be used for new subscriptions
         * @param securityInitializer The security initializer to use on newly created securities
        public OptionChainUniverse(Option option, UniverseSettings universeSettings, ISecurityInitializer securityInitializer = null )
            : base(option.SubscriptionDataConfig, securityInitializer) {
            _option = option;
            _universeSettings = universeSettings;
        }

        /**
         * Gets the settings used for subscriptons added for this universe
        */
        public @Override UniverseSettings UniverseSettings
        {
            get { return _universeSettings; }
        }

        /**
         * Performs universe selection using the data specified
        */
         * @param utcTime The current utc time
         * @param data The symbols to remain in the universe
        @returns The data that passes the filter
        public @Override IEnumerable<Symbol> SelectSymbols(DateTime utcTime, BaseDataCollection data) {
            optionsUniverseDataCollection = data as OptionChainUniverseDataCollection;
            if( optionsUniverseDataCollection == null ) {
                throw new IllegalArgumentException( String.format( "Expected data of type '%1$s'", typeof (OptionChainUniverseDataCollection).Name));
            }

            _underlying = optionsUniverseDataCollection.Underlying ?? _underlying;
            optionsUniverseDataCollection.Underlying = _underlying;

            if( _underlying == null || data.Data.Count == 0) {
                return Unchanged;
            }

            availableContracts = optionsUniverseDataCollection.Data.Select(x -> x.Symbol);
            results = _option.ContractFilter.Filter(availableContracts, _underlying).ToHashSet();

            // we save off the filtered results to the universe data collection for later
            // population into the OptionChain. This is non-ideal and could be remedied by
            // the universe subscription emitting a special type after selection that could
            // be checked for in TimeSlice.Create, but for now this will do
            optionsUniverseDataCollection.FilteredContracts = results;

            return results;
        }

        /**
         * Gets the subscriptions to be added for the specified security
        */
         * 
         * In most cases the default implementaon of returning the security's configuration is
         * sufficient. It's when we want multiple subscriptions (trade/quote data) that we'll need
         * to @Override this
         * 
         * @param security The security to get subscriptions for
        @returns All subscriptions required by this security
        public @Override IEnumerable<SubscriptionDataConfig> GetSubscriptions(Security security) {
            config = security.SubscriptionDataConfig;

            // canonical also needs underlying price data
            if( security.Symbol == _option.Symbol) {
                underlying = Symbol.Create(config.Symbol.ID.Symbol, SecurityType.Equity, config.Market);
                resolution = config.Resolution == Resolution.Tick ? Resolution.Second : config.Resolution;
                return new[]
                {
                    // rewrite the primary to be non-tick and fill forward
                    new SubscriptionDataConfig(config, resolution: resolution, fillForward: true), 
                    // add underlying trade data
                    new SubscriptionDataConfig(config, resolution: resolution, fillForward: true, symbol: underlying, objectType: typeof(TradeBar), tickType: TickType.Trade), 
                };
            }

            // we want to return both quote and trade subscriptions
            return QuotesAndTrades.Select(x -> new SubscriptionDataConfig(config,
                tickType: x,
                objectType: GetDataType(config.Resolution, x),
                isFilteredSubscription: true
                ));
        }

        /**
         * Creates and configures a security for the specified symbol
        */
         * @param symbol The symbol of the security to be created
         * @param algorithm The algorithm instance
         * @param marketHoursDatabase The market hours database
         * @param symbolPropertiesDatabase The symbol properties database
        @returns The newly initialized security object
        public @Override Security CreateSecurity(Symbol symbol, IAlgorithm algorithm, MarketHoursDatabase marketHoursDatabase, SymbolPropertiesDatabase symbolPropertiesDatabase) {
            // set the underlying security and pricing model from the canonical security
            option = (Option)base.CreateSecurity(symbol, algorithm, marketHoursDatabase, symbolPropertiesDatabase);
            option.Underlying = _option.Underlying;
            option.PriceModel = _option.PriceModel;
            return option;
        }

        /**
         * Determines whether or not the specified security can be removed from
         * this universe. This is useful to prevent securities from being taken
         * out of a universe before the algorithm has had enough time to make
         * decisions on the security
        */
         * @param utcTime The current utc time
         * @param security The security to check if its ok to remove
        @returns True if we can remove the security, false otherwise
        public @Override boolean CanRemoveMember(DateTime utcTime, Security security) {
            // if we haven't begun receiving data for this security then it's safe to remove
            lastData = security.Cache.GetData();
            if( lastData == null ) {
                return true;
            }

            // only remove members on day changes, this prevents us from needing to
            // fast forward contracts continuously as price moves and out filtered
            // contracts change thoughout the day
            localTime = utcTime Extensions.convertFromUtc(security.Exchange.TimeZone);
            if( localTime.Date != lastData.Time.Date) {
                return true;
            }
            return false;
        }

        /**
         * Gets the data type required for the specified combination of resolution and tick type
        */
        private static Type GetDataType(Resolution resolution, TickType tickType) {
            if( resolution == Resolution.Tick) return typeof(Tick);
            if( tickType == TickType.Quote) return typeof(QuoteBar);
            return typeof(TradeBar);
        }
    }
}
