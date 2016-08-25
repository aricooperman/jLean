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
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Securities;

package com.quantconnect.lean.Algorithm
{
    public partial class QCAlgorithm
    {
        /**
         * Gets universe manager which holds universes keyed by their symbol
        */
        public UniverseManager UniverseManager
        {
            get;
            private set;
        }

        /**
         * Gets the universe settings to be used when adding securities via universe selection
        */
        public UniverseSettings UniverseSettings
        {
            get;
            private set;
        }

        /**
         * Gets a helper that provides pre-defined universe defintions, such as top dollar volume
        */
        public UniverseDefinitions Universe
        {
            get; 
            private set;
        }

        /**
         * Adds the universe to the algorithm
        */
         * @param universe The universe to be added
        public void AddUniverse(Universe universe) {
            UniverseManager.Add(universe.Configuration.Symbol, universe);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This will use the default universe settings
         * specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
         * of SecurityType.Equity, Resolution.Daily, Market.USA, and UniverseSettings
        */
         * <typeparam name="T The data type</typeparam>
         * @param name A unique name for this universe
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>( String name, Func<IEnumerable<T>, IEnumerable<Symbol>> selector) {
            AddUniverse(SecurityType.Equity, name, Resolution.Daily, Market.USA, UniverseSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This will use the default universe settings
         * specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
         * of SecurityType.Equity, Resolution.Daily, Market.USA, and UniverseSettings
        */
         * <typeparam name="T The data type</typeparam>
         * @param name A unique name for this universe
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>( String name, Func<IEnumerable<T>, IEnumerable<String>> selector) {
            AddUniverse(SecurityType.Equity, name, Resolution.Daily, Market.USA, UniverseSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This will use the default universe settings
         * specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
         * of SecurityType.Equity, Resolution.Daily, and Market.USA
        */
         * <typeparam name="T The data type</typeparam>
         * @param name A unique name for this universe
         * @param universeSettings The settings used for securities added by this universe
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>( String name, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<Symbol>> selector) {
            AddUniverse(SecurityType.Equity, name, Resolution.Daily, Market.USA, universeSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This will use the default universe settings
         * specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
         * of SecurityType.Equity, Resolution.Daily, and Market.USA
        */
         * <typeparam name="T The data type</typeparam>
         * @param name A unique name for this universe
         * @param universeSettings The settings used for securities added by this universe
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>( String name, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<String>> selector) {
            AddUniverse(SecurityType.Equity, name, Resolution.Daily, Market.USA, universeSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This will use the default universe settings
         * specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
         * of SecurityType.Equity, Market.USA and UniverseSettings
        */
         * <typeparam name="T The data type</typeparam>
         * @param name A unique name for this universe
         * @param resolution The epected resolution of the universe data
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>( String name, Resolution resolution, Func<IEnumerable<T>, IEnumerable<Symbol>> selector) {
            AddUniverse(SecurityType.Equity, name, resolution, Market.USA, UniverseSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This will use the default universe settings
         * specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
         * of SecurityType.Equity, Market.USA and UniverseSettings
        */
         * <typeparam name="T The data type</typeparam>
         * @param name A unique name for this universe
         * @param resolution The epected resolution of the universe data
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>( String name, Resolution resolution, Func<IEnumerable<T>, IEnumerable<String>> selector) {
            AddUniverse(SecurityType.Equity, name, resolution, Market.USA, UniverseSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This will use the default universe settings
         * specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
         * of SecurityType.Equity, and Market.USA
        */
         * <typeparam name="T The data type</typeparam>
         * @param name A unique name for this universe
         * @param resolution The epected resolution of the universe data
         * @param universeSettings The settings used for securities added by this universe
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>( String name, Resolution resolution, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<Symbol>> selector) {
            AddUniverse(SecurityType.Equity, name, resolution, Market.USA, universeSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This will use the default universe settings
         * specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
         * of SecurityType.Equity, and Market.USA
        */
         * <typeparam name="T The data type</typeparam>
         * @param name A unique name for this universe
         * @param resolution The epected resolution of the universe data
         * @param universeSettings The settings used for securities added by this universe
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>( String name, Resolution resolution, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<String>> selector) {
            AddUniverse(SecurityType.Equity, name, resolution, Market.USA, universeSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This will use the default universe settings
         * specified via the <see cref="UniverseSettings"/> property.
        */
         * <typeparam name="T The data type</typeparam>
         * @param securityType The security type the universe produces
         * @param name A unique name for this universe
         * @param resolution The epected resolution of the universe data
         * @param market The market for selected symbols
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>(SecurityType securityType, String name, Resolution resolution, String market, Func<IEnumerable<T>, IEnumerable<Symbol>> selector) {
            AddUniverse(securityType, name, resolution, market, UniverseSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This will use the default universe settings
         * specified via the <see cref="UniverseSettings"/> property.
        */
         * <typeparam name="T The data type</typeparam>
         * @param securityType The security type the universe produces
         * @param name A unique name for this universe
         * @param resolution The epected resolution of the universe data
         * @param market The market for selected symbols
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>(SecurityType securityType, String name, Resolution resolution, String market, Func<IEnumerable<T>, IEnumerable<String>> selector) {
            AddUniverse(securityType, name, resolution, market, UniverseSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm
        */
         * <typeparam name="T The data type</typeparam>
         * @param securityType The security type the universe produces
         * @param name A unique name for this universe
         * @param resolution The epected resolution of the universe data
         * @param market The market for selected symbols
         * @param universeSettings The subscription settings to use for newly created subscriptions
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>(SecurityType securityType, String name, Resolution resolution, String market, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<Symbol>> selector) {
            marketHoursDbEntry = _marketHoursDatabase.GetEntry(market, name, securityType);
            dataTimeZone = marketHoursDbEntry.DataTimeZone;
            exchangeTimeZone = marketHoursDbEntry.ExchangeHours.TimeZone;
            symbol = QuantConnect.Symbol.Create(name, securityType, market);
            config = new SubscriptionDataConfig(typeof(T), symbol, resolution, dataTimeZone, exchangeTimeZone, false, false, true, true, isFilteredSubscription: false);
            AddUniverse(new FuncUniverse(config, universeSettings, SecurityInitializer, d -> selector(d.OfType<T>())));
        }

        /**
         * Creates a new universe and adds it to the algorithm
        */
         * <typeparam name="T The data type</typeparam>
         * @param securityType The security type the universe produces
         * @param name A unique name for this universe
         * @param resolution The epected resolution of the universe data
         * @param market The market for selected symbols
         * @param universeSettings The subscription settings to use for newly created subscriptions
         * @param selector Function delegate that performs selection on the universe data
        public void AddUniverse<T>(SecurityType securityType, String name, Resolution resolution, String market, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<String>> selector) {
            marketHoursDbEntry = _marketHoursDatabase.GetEntry(market, name, securityType);
            dataTimeZone = marketHoursDbEntry.DataTimeZone;
            exchangeTimeZone = marketHoursDbEntry.ExchangeHours.TimeZone;
            symbol = QuantConnect.Symbol.Create(name, securityType, market);
            config = new SubscriptionDataConfig(typeof(T), symbol, resolution, dataTimeZone, exchangeTimeZone, false, false, true, true, isFilteredSubscription: false);
            AddUniverse(new FuncUniverse(config, universeSettings, SecurityInitializer, d -> selector(d.OfType<T>()).Select(x -> QuantConnect.Symbol.Create(x, securityType, market))));
        }

        /**
         * Creates a new univese and adds it to the algorithm. This is for coarse fundamntal US Equity data and
         * will be executed on day changes in the NewYork time zone (<see cref="TimeZones.NewYork"/>
        */
         * @param selector Defines an initial coarse selection
        public void AddUniverse(Func<IEnumerable<CoarseFundamental>, IEnumerable<Symbol>> selector) {
            symbol = CoarseFundamental.CreateUniverseSymbol(Market.USA);
            config = new SubscriptionDataConfig(typeof(CoarseFundamental), symbol, Resolution.Daily, TimeZones.NewYork, TimeZones.NewYork, false, false, true, isFilteredSubscription: false);
            AddUniverse(new FuncUniverse(config, UniverseSettings, SecurityInitializer, selectionData -> selector(selectionData.OfType<CoarseFundamental>())));
        }

        /**
         * Creates a new universe and adds it to the algorithm. This can be used to return a list of string
         * symbols retrieved from anywhere and will loads those symbols under the US Equity market.
        */
         * @param name A unique name for this universe
         * @param selector Function delegate that accepts a DateTime and returns a collection of String symbols
        public void AddUniverse( String name, Func<DateTime, IEnumerable<String>> selector) {
            AddUniverse(SecurityType.Equity, name, Resolution.Daily, Market.USA, UniverseSettings, selector);
        }

        /**
         * Creates a new universe and adds it to the algorithm. This can be used to return a list of string
         * symbols retrieved from anywhere and will loads those symbols under the US Equity market.
        */
         * @param name A unique name for this universe
         * @param resolution The resolution this universe should be triggered on
         * @param selector Function delegate that accepts a DateTime and returns a collection of String symbols
        public void AddUniverse( String name, Resolution resolution, Func<DateTime, IEnumerable<String>> selector) {
            AddUniverse(SecurityType.Equity, name, resolution, Market.USA, UniverseSettings, selector);
        }

        /**
         * Creates a new user defined universe that will fire on the requested resolution during market hours.
        */
         * @param securityType The security type of the universe
         * @param name A unique name for this universe
         * @param resolution The resolution this universe should be triggered on
         * @param market The market of the universe
         * @param universeSettings The subscription settings used for securities added from this universe
         * @param selector Function delegate that accepts a DateTime and returns a collection of String symbols
        public void AddUniverse(SecurityType securityType, String name, Resolution resolution, String market, UniverseSettings universeSettings, Func<DateTime, IEnumerable<String>> selector) {
            marketHoursDbEntry = _marketHoursDatabase.GetEntry(market, name, securityType);
            dataTimeZone = marketHoursDbEntry.DataTimeZone;
            exchangeTimeZone = marketHoursDbEntry.ExchangeHours.TimeZone;
            symbol = QuantConnect.Symbol.Create(name, securityType, market);
            config = new SubscriptionDataConfig(typeof(CoarseFundamental), symbol, resolution, dataTimeZone, exchangeTimeZone, false, false, true, isFilteredSubscription: false);
            AddUniverse(new UserDefinedUniverse(config, universeSettings, SecurityInitializer, resolution.ToTimeSpan(), selector));
        }

        /**
         * Adds the security to the user defined universe for the specified 
        */
        private void AddToUserDefinedUniverse(Security security) {
            Securities.Add(security);

            // add this security to the user defined universe
            Universe universe;
            subscription = security.Subscriptions.First();
            universeSymbol = UserDefinedUniverse.CreateSymbol(subscription.SecurityType, subscription.Market);
            if( !UniverseManager.TryGetValue(universeSymbol, out universe)) {
                // create a new universe, these subscription settings don't currently get used
                // since universe selection proper is never invoked on this type of universe
                uconfig = new SubscriptionDataConfig(subscription, symbol: universeSymbol, isInternalFeed: true, fillForward: false);
                universe = new UserDefinedUniverse(uconfig,
                    new UniverseSettings(security.Resolution, security.Leverage, security.IsFillDataForward, security.IsExtendedMarketHours, Duration.ZERO),
                    SecurityInitializer,
                    QuantConnect.Time.OneDay,
                    new List<Symbol> { security.Symbol }
                    );
                UniverseManager.Add(universeSymbol, universe);
            }
            
            userDefinedUniverse = universe as UserDefinedUniverse;
            if( userDefinedUniverse != null ) {
                userDefinedUniverse.Add(security.Symbol);
            }
            else
            {
                // should never happen, someone would need to add a non-user defined universe with this symbol
                throw new Exception( "Expected universe with symbol '" + universeSymbol.Value + "' to be of type UserDefinedUniverse.");
            }
        }
    }
}
