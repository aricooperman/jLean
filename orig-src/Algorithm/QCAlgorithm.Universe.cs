﻿/*
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
        /// <summary>
        /// Gets universe manager which holds universes keyed by their symbol
        /// </summary>
        public UniverseManager UniverseManager
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the universe settings to be used when adding securities via universe selection
        /// </summary>
        public UniverseSettings UniverseSettings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a helper that provides pre-defined universe defintions, such as top dollar volume
        /// </summary>
        public UniverseDefinitions Universe
        {
            get; 
            private set;
        }

        /// <summary>
        /// Adds the universe to the algorithm
        /// </summary>
        /// <param name="universe">The universe to be added</param>
        public void AddUniverse(Universe universe)
        {
            UniverseManager.Add(universe.Configuration.Symbol, universe);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This will use the default universe settings
        /// specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
        /// of SecurityType.Equity, Resolution.Daily, Market.USA, and UniverseSettings
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>( String name, Func<IEnumerable<T>, IEnumerable<Symbol>> selector)
        {
            AddUniverse(SecurityType.Equity, name, Resolution.Daily, Market.USA, UniverseSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This will use the default universe settings
        /// specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
        /// of SecurityType.Equity, Resolution.Daily, Market.USA, and UniverseSettings
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>( String name, Func<IEnumerable<T>, IEnumerable<String>> selector)
        {
            AddUniverse(SecurityType.Equity, name, Resolution.Daily, Market.USA, UniverseSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This will use the default universe settings
        /// specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
        /// of SecurityType.Equity, Resolution.Daily, and Market.USA
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="universeSettings">The settings used for securities added by this universe</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>( String name, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<Symbol>> selector)
        {
            AddUniverse(SecurityType.Equity, name, Resolution.Daily, Market.USA, universeSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This will use the default universe settings
        /// specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
        /// of SecurityType.Equity, Resolution.Daily, and Market.USA
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="universeSettings">The settings used for securities added by this universe</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>( String name, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<String>> selector)
        {
            AddUniverse(SecurityType.Equity, name, Resolution.Daily, Market.USA, universeSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This will use the default universe settings
        /// specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
        /// of SecurityType.Equity, Market.USA and UniverseSettings
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="resolution">The epected resolution of the universe data</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>( String name, Resolution resolution, Func<IEnumerable<T>, IEnumerable<Symbol>> selector)
        {
            AddUniverse(SecurityType.Equity, name, resolution, Market.USA, UniverseSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This will use the default universe settings
        /// specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
        /// of SecurityType.Equity, Market.USA and UniverseSettings
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="resolution">The epected resolution of the universe data</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>( String name, Resolution resolution, Func<IEnumerable<T>, IEnumerable<String>> selector)
        {
            AddUniverse(SecurityType.Equity, name, resolution, Market.USA, UniverseSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This will use the default universe settings
        /// specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
        /// of SecurityType.Equity, and Market.USA
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="resolution">The epected resolution of the universe data</param>
        /// <param name="universeSettings">The settings used for securities added by this universe</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>( String name, Resolution resolution, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<Symbol>> selector)
        {
            AddUniverse(SecurityType.Equity, name, resolution, Market.USA, universeSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This will use the default universe settings
        /// specified via the <see cref="UniverseSettings"/> property. This universe will use the defaults
        /// of SecurityType.Equity, and Market.USA
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="resolution">The epected resolution of the universe data</param>
        /// <param name="universeSettings">The settings used for securities added by this universe</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>( String name, Resolution resolution, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<String>> selector)
        {
            AddUniverse(SecurityType.Equity, name, resolution, Market.USA, universeSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This will use the default universe settings
        /// specified via the <see cref="UniverseSettings"/> property.
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="securityType">The security type the universe produces</param>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="resolution">The epected resolution of the universe data</param>
        /// <param name="market">The market for selected symbols</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>(SecurityType securityType, String name, Resolution resolution, String market, Func<IEnumerable<T>, IEnumerable<Symbol>> selector)
        {
            AddUniverse(securityType, name, resolution, market, UniverseSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This will use the default universe settings
        /// specified via the <see cref="UniverseSettings"/> property.
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="securityType">The security type the universe produces</param>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="resolution">The epected resolution of the universe data</param>
        /// <param name="market">The market for selected symbols</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>(SecurityType securityType, String name, Resolution resolution, String market, Func<IEnumerable<T>, IEnumerable<String>> selector)
        {
            AddUniverse(securityType, name, resolution, market, UniverseSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="securityType">The security type the universe produces</param>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="resolution">The epected resolution of the universe data</param>
        /// <param name="market">The market for selected symbols</param>
        /// <param name="universeSettings">The subscription settings to use for newly created subscriptions</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>(SecurityType securityType, String name, Resolution resolution, String market, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<Symbol>> selector)
        {
            marketHoursDbEntry = _marketHoursDatabase.GetEntry(market, name, securityType);
            dataTimeZone = marketHoursDbEntry.DataTimeZone;
            exchangeTimeZone = marketHoursDbEntry.ExchangeHours.TimeZone;
            symbol = QuantConnect.Symbol.Create(name, securityType, market);
            config = new SubscriptionDataConfig(typeof(T), symbol, resolution, dataTimeZone, exchangeTimeZone, false, false, true, true, isFilteredSubscription: false);
            AddUniverse(new FuncUniverse(config, universeSettings, SecurityInitializer, d => selector(d.OfType<T>())));
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="securityType">The security type the universe produces</param>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="resolution">The epected resolution of the universe data</param>
        /// <param name="market">The market for selected symbols</param>
        /// <param name="universeSettings">The subscription settings to use for newly created subscriptions</param>
        /// <param name="selector">Function delegate that performs selection on the universe data</param>
        public void AddUniverse<T>(SecurityType securityType, String name, Resolution resolution, String market, UniverseSettings universeSettings, Func<IEnumerable<T>, IEnumerable<String>> selector)
        {
            marketHoursDbEntry = _marketHoursDatabase.GetEntry(market, name, securityType);
            dataTimeZone = marketHoursDbEntry.DataTimeZone;
            exchangeTimeZone = marketHoursDbEntry.ExchangeHours.TimeZone;
            symbol = QuantConnect.Symbol.Create(name, securityType, market);
            config = new SubscriptionDataConfig(typeof(T), symbol, resolution, dataTimeZone, exchangeTimeZone, false, false, true, true, isFilteredSubscription: false);
            AddUniverse(new FuncUniverse(config, universeSettings, SecurityInitializer, d => selector(d.OfType<T>()).Select(x => QuantConnect.Symbol.Create(x, securityType, market))));
        }

        /// <summary>
        /// Creates a new univese and adds it to the algorithm. This is for coarse fundamntal US Equity data and
        /// will be executed on day changes in the NewYork time zone (<see cref="TimeZones.NewYork"/>
        /// </summary>
        /// <param name="selector">Defines an initial coarse selection</param>
        public void AddUniverse(Func<IEnumerable<CoarseFundamental>, IEnumerable<Symbol>> selector)
        {
            symbol = CoarseFundamental.CreateUniverseSymbol(Market.USA);
            config = new SubscriptionDataConfig(typeof(CoarseFundamental), symbol, Resolution.Daily, TimeZones.NewYork, TimeZones.NewYork, false, false, true, isFilteredSubscription: false);
            AddUniverse(new FuncUniverse(config, UniverseSettings, SecurityInitializer, selectionData => selector(selectionData.OfType<CoarseFundamental>())));
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This can be used to return a list of string
        /// symbols retrieved from anywhere and will loads those symbols under the US Equity market.
        /// </summary>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="selector">Function delegate that accepts a DateTime and returns a collection of String symbols</param>
        public void AddUniverse( String name, Func<DateTime, IEnumerable<String>> selector)
        {
            AddUniverse(SecurityType.Equity, name, Resolution.Daily, Market.USA, UniverseSettings, selector);
        }

        /// <summary>
        /// Creates a new universe and adds it to the algorithm. This can be used to return a list of string
        /// symbols retrieved from anywhere and will loads those symbols under the US Equity market.
        /// </summary>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="resolution">The resolution this universe should be triggered on</param>
        /// <param name="selector">Function delegate that accepts a DateTime and returns a collection of String symbols</param>
        public void AddUniverse( String name, Resolution resolution, Func<DateTime, IEnumerable<String>> selector)
        {
            AddUniverse(SecurityType.Equity, name, resolution, Market.USA, UniverseSettings, selector);
        }

        /// <summary>
        /// Creates a new user defined universe that will fire on the requested resolution during market hours.
        /// </summary>
        /// <param name="securityType">The security type of the universe</param>
        /// <param name="name">A unique name for this universe</param>
        /// <param name="resolution">The resolution this universe should be triggered on</param>
        /// <param name="market">The market of the universe</param>
        /// <param name="universeSettings">The subscription settings used for securities added from this universe</param>
        /// <param name="selector">Function delegate that accepts a DateTime and returns a collection of String symbols</param>
        public void AddUniverse(SecurityType securityType, String name, Resolution resolution, String market, UniverseSettings universeSettings, Func<DateTime, IEnumerable<String>> selector)
        {
            marketHoursDbEntry = _marketHoursDatabase.GetEntry(market, name, securityType);
            dataTimeZone = marketHoursDbEntry.DataTimeZone;
            exchangeTimeZone = marketHoursDbEntry.ExchangeHours.TimeZone;
            symbol = QuantConnect.Symbol.Create(name, securityType, market);
            config = new SubscriptionDataConfig(typeof(CoarseFundamental), symbol, resolution, dataTimeZone, exchangeTimeZone, false, false, true, isFilteredSubscription: false);
            AddUniverse(new UserDefinedUniverse(config, universeSettings, SecurityInitializer, resolution.ToTimeSpan(), selector));
        }

        /// <summary>
        /// Adds the security to the user defined universe for the specified 
        /// </summary>
        private void AddToUserDefinedUniverse(Security security)
        {
            Securities.Add(security);

            // add this security to the user defined universe
            Universe universe;
            subscription = security.Subscriptions.First();
            universeSymbol = UserDefinedUniverse.CreateSymbol(subscription.SecurityType, subscription.Market);
            if (!UniverseManager.TryGetValue(universeSymbol, out universe))
            {
                // create a new universe, these subscription settings don't currently get used
                // since universe selection proper is never invoked on this type of universe
                uconfig = new SubscriptionDataConfig(subscription, symbol: universeSymbol, isInternalFeed: true, fillForward: false);
                universe = new UserDefinedUniverse(uconfig,
                    new UniverseSettings(security.Resolution, security.Leverage, security.IsFillDataForward, security.IsExtendedMarketHours, TimeSpan.Zero),
                    SecurityInitializer,
                    QuantConnect.Time.OneDay,
                    new List<Symbol> { security.Symbol }
                    );
                UniverseManager.Add(universeSymbol, universe);
            }
            
            userDefinedUniverse = universe as UserDefinedUniverse;
            if (userDefinedUniverse != null)
            {
                userDefinedUniverse.Add(security.Symbol);
            }
            else
            {
                // should never happen, someone would need to add a non-user defined universe with this symbol
                throw new Exception("Expected universe with symbol '" + universeSymbol.Value + "' to be of type UserDefinedUniverse.");
            }
        }
    }
}
