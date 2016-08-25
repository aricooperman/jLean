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

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using QuantConnect.Data;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Provides a collection for holding subscriptions.
    */
    public class SubscriptionCollection : IEnumerable<Subscription>
    {
        private final ConcurrentMap<Symbol, ConcurrentMap<SubscriptionDataConfig, Subscription>> _subscriptions;

        /**
         * Initializes a new instance of the <see cref="SubscriptionCollection"/> class
        */
        public SubscriptionCollection() {
            _subscriptions = new ConcurrentMap<Symbol, ConcurrentMap<SubscriptionDataConfig, Subscription>>();
        }

        /**
         * Checks the collection for the specified subscription configuration
        */
         * @param configuration The subscription configuration to check for
        @returns True if a subscription with the specified configuration is found in this collection, false otherwise
        public boolean Contains(SubscriptionDataConfig configuration) {
            ConcurrentMap<SubscriptionDataConfig, Subscription> dictionary;
            if( !_subscriptions.TryGetValue(configuration.Symbol, out dictionary)) {
                return false;
            }

            return dictionary.ContainsKey(configuration);
        }

        /**
         * Checks the collection for any subscriptions with the specified symbol
        */
         * @param symbol The symbol to check
        @returns True if any subscriptions are found with the specified symbol
        public boolean ContainsAny(Symbol symbol) {
            return _subscriptions.ContainsKey(symbol);
        }

        /**
         * Attempts to add the specified subscription to the collection. If another subscription
         * exists with the same configuration then it won't be added.
        */
         * @param subscription The subscription to add
        @returns True if the subscription is successfully added, false otherwise
        public boolean TryAdd(Subscription subscription) {
            ConcurrentMap<SubscriptionDataConfig, Subscription> dictionary;
            if( !_subscriptions.TryGetValue(subscription.Configuration.Symbol, out dictionary)) {
                dictionary = new ConcurrentMap<SubscriptionDataConfig, Subscription>();
                _subscriptions[subscription.Configuration.Symbol] = dictionary;
            }

            return dictionary.TryAdd(subscription.Configuration, subscription);
        }

        /**
         * Attempts to retrieve the subscription with the specified configuration
        */
         * @param configuration The subscription's configuration
         * @param subscription The subscription matching the configuration, null if not found
        @returns True if the subscription is successfully retrieved, false otherwise
        public boolean TryGetValue(SubscriptionDataConfig configuration, out Subscription subscription) {
            ConcurrentMap<SubscriptionDataConfig, Subscription> dictionary;
            if( !_subscriptions.TryGetValue(configuration.Symbol, out dictionary)) {
                subscription = null;
                return false;
            }

            return dictionary.TryGetValue(configuration, out subscription);
        }
        
        /**
         * Attempts to retrieve the subscription with the specified configuration
        */
         * @param symbol The symbol of the subscription's configuration
         * @param subscriptions The subscriptions matching the symbol, null if not found
        @returns True if the subscriptions are successfully retrieved, false otherwise
        public boolean TryGetAll(Symbol symbol, out ICollection<Subscription> subscriptions) {
            ConcurrentMap<SubscriptionDataConfig, Subscription> dictionary;
            if( !_subscriptions.TryGetValue(symbol, out dictionary)) {
                subscriptions = null;
                return false;
            }
            
            subscriptions = dictionary.Values;
            return true;
        }

        /**
         * Attempts to remove the subscription with the specified configuraton from the collection.
        */
         * @param configuration The configuration of the subscription to remove
         * @param subscription The removed subscription, null if not found.
        @returns True if the subscription is successfully removed, false otherwise
        public boolean TryRemove(SubscriptionDataConfig configuration, out Subscription subscription) {
            ConcurrentMap<SubscriptionDataConfig, Subscription> dictionary;
            if( !_subscriptions.TryRemove(configuration.Symbol, out dictionary)) {
                subscription = null;
                return false;
            }

            return dictionary.TryRemove(configuration, out subscription);
        }

        /**
         * Attempts to remove all subscriptons for the specified symbol
        */
         * @param symbol The symbol of the subscriptions to remove
         * @param subscriptions The removed subscriptions
        @returns 
        public boolean TryRemoveAll(Symbol symbol, out ICollection<Subscription> subscriptions) {
            ConcurrentMap<SubscriptionDataConfig, Subscription> dictionary;
            if( !_subscriptions.TryRemove(symbol, out dictionary)) {
                subscriptions = null;
                return false;
            }

            subscriptions = dictionary.Values;
            return true;
        }

        /**
         * Returns an enumerator that iterates through the collection.
        */
        @returns 
         * An enumerator that can be used to iterate through the collection.
         * 
        public IEnumerator<Subscription> GetEnumerator() {
            foreach (subscriptionsBySymbol in _subscriptions) {
                subscriptionsByConfig = subscriptionsBySymbol.Value;
                foreach (kvp in subscriptionsByConfig) {
                    subscription = kvp.Value;
                    yield return subscription;
                }
            }
        }

        /**
         * Returns an enumerator that iterates through a collection.
        */
        @returns 
         * An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
         * 
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
