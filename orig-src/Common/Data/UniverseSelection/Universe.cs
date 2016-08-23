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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Data.UniverseSelection
{
    /**
    /// Provides a base class for all universes to derive from.
    */
    public abstract class Universe
    {
        /**
        /// Gets a value indicating that no change to the universe should be made
        */
        public static final UnchangedUniverse Unchanged = UnchangedUniverse.Instance;

        private HashSet<Symbol> _previousSelections; 

        private final ConcurrentMap<Symbol, Member> _securities;

        /**
        /// Gets the security type of this universe
        */
        public SecurityType SecurityType
        {
            get { return Configuration.SecurityType; }
        }

        /**
        /// Gets the market of this universe
        */
        public String Market
        {
            get { return Configuration.Market; }
        }

        /**
        /// Gets the settings used for subscriptons added for this universe
        */
        public abstract UniverseSettings UniverseSettings
        {
            get;
        }

        /**
        /// Gets the configuration used to get universe data
        */
        public SubscriptionDataConfig Configuration
        {
            get; private set;
        }

        /**
        /// Gets the instance responsible for initializing newly added securities
        */
        public ISecurityInitializer SecurityInitializer
        {
            get; private set;
        }

        /**
        /// Gets the current listing of members in this universe. Modifications
        /// to this dictionary do not change universe membership.
        */
        public Map<Symbol, Security> Members
        {
            get { return _securities.Select(x -> x.Value.Security).ToDictionary(x -> x.Symbol); }
        }

        /**
        /// Initializes a new instance of the <see cref="Universe"/> class
        */
         * @param config">The configuration used to source data for this universe
         * @param securityInitializer">Initializes securities when they're added to the universe
        protected Universe(SubscriptionDataConfig config, ISecurityInitializer securityInitializer = null ) {
            _previousSelections = new HashSet<Symbol>();
            _securities = new ConcurrentMap<Symbol, Member>();

            Configuration = config;
            SecurityInitializer = securityInitializer ?? Securities.SecurityInitializer.Null;
        }

        /**
        /// Determines whether or not the specified security can be removed from
        /// this universe. This is useful to prevent securities from being taken
        /// out of a universe before the algorithm has had enough time to make
        /// decisions on the security
        */
         * @param utcTime">The current utc time
         * @param security">The security to check if its ok to remove
        @returns True if we can remove the security, false otherwise
        public virtual boolean CanRemoveMember(DateTime utcTime, Security security) {
            Member member;
            if( _securities.TryGetValue(security.Symbol, out member)) {
                timeInUniverse = utcTime - member.Added;
                if( timeInUniverse >= UniverseSettings.MinimumTimeInUniverse) {
                    return true;
                }
            }
            return false;
        }

        /**
        /// Performs universe selection using the data specified
        */
         * @param utcTime">The current utc time
         * @param data">The symbols to remain in the universe
        @returns The data that passes the filter
        public IEnumerable<Symbol> PerformSelection(DateTime utcTime, BaseDataCollection data) {
            result = SelectSymbols(utcTime, data);
            if( ReferenceEquals(result, Unchanged)) {
                return Unchanged;
            }

            selections = result.ToHashSet();
            hasDiffs = _previousSelections.Except(selections).Union(selections.Except(_previousSelections)).Any();
            _previousSelections = selections;
            if( !hasDiffs) {
                return Unchanged;
            }
            return selections;
        }

        /**
        /// Performs universe selection using the data specified
        */
         * @param utcTime">The current utc time
         * @param data">The symbols to remain in the universe
        @returns The data that passes the filter
        public abstract IEnumerable<Symbol> SelectSymbols(DateTime utcTime, BaseDataCollection data);

        /**
        /// Creates and configures a security for the specified symbol
        */
         * @param symbol">The symbol of the security to be created
         * @param algorithm">The algorithm instance
         * @param marketHoursDatabase">The market hours database
         * @param symbolPropertiesDatabase">The symbol properties database
        @returns The newly initialized security object
        public virtual Security CreateSecurity(Symbol symbol, IAlgorithm algorithm, MarketHoursDatabase marketHoursDatabase, SymbolPropertiesDatabase symbolPropertiesDatabase) {
            // by default invoke the create security method to handle security initialization
            return SecurityManager.CreateSecurity(algorithm.Portfolio, algorithm.SubscriptionManager, marketHoursDatabase, symbolPropertiesDatabase,
                SecurityInitializer, symbol, UniverseSettings.Resolution, UniverseSettings.FillForward, UniverseSettings.Leverage,
                UniverseSettings.ExtendedMarketHours, false, false, symbol.ID.SecurityType == SecurityType.Option);
        }

        /**
        /// Gets the subscriptions to be added for the specified security
        */
        /// 
        /// In most cases the default implementaon of returning the security's configuration is
        /// sufficient. It's when we want multiple subscriptions (trade/quote data) that we'll need
        /// to @Override this
        /// 
         * @param security">The security to get subscriptions for
        @returns All subscriptions required by this security
        public virtual IEnumerable<SubscriptionDataConfig> GetSubscriptions(Security security) {
            return security.Subscriptions;
        }

        /**
        /// Determines whether or not the specified symbol is currently a member of this universe
        */
         * @param symbol">The symbol whose membership is to be checked
        @returns True if the specified symbol is part of this universe, false otherwise
        public boolean ContainsMember(Symbol symbol) {
            return _securities.ContainsKey(symbol);
        }

        /**
        /// Adds the specified security to this universe
        */
         * @param utcTime">The current utc date time
         * @param security">The security to be added
        @returns True if the security was successfully added,
        /// false if the security was already in the universe
        internal boolean AddMember(DateTime utcTime, Security security) {
            if( _securities.ContainsKey(security.Symbol)) {
                return false;
            }
            return _securities.TryAdd(security.Symbol, new Member(utcTime, security));
        }

        /**
        /// Tries to remove the specified security from the universe. This
        /// will first check to verify that we can remove the security by
        /// calling the <see cref="CanRemoveMember"/> function.
        */
         * @param utcTime">The current utc time
         * @param security">The security to be removed
        @returns True if the security was successfully removed, false if
        /// we're not allowed to remove or if the security didn't exist
        internal boolean RemoveMember(DateTime utcTime, Security security) {
            if( CanRemoveMember(utcTime, security)) {
                Member member;
                return _securities.TryRemove(security.Symbol, out member);
            }
            return false;
        }

        /**
        /// Provides a value to indicate that no changes should be made to the universe.
        /// This value is intended to be return reference via <see cref="Universe.SelectSymbols"/>
        */
        public sealed class UnchangedUniverse : IEnumerable<String>, IEnumerable<Symbol>
        {
            /**
            /// Read-only instance of the <see cref="UnchangedUniverse"/> value
            */
            public static final UnchangedUniverse Instance = new UnchangedUniverse();
            private UnchangedUniverse() { }
            IEnumerator<Symbol> IEnumerable<Symbol>.GetEnumerator() { yield break; }
            IEnumerator<String> IEnumerable<String>.GetEnumerator() { yield break; }
            IEnumerator IEnumerable.GetEnumerator() { yield break; }
        }

        private sealed class Member
        {
            public final DateTime Added;
            public final Security Security;
            public Member(DateTime added, Security security) {
                Added = added;
                Security = security;
            }
        }
    }
}