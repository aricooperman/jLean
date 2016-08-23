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

using System.Collections.Generic;
using System.Linq;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Data.UniverseSelection
{
    /**
    /// Defines the additions and subtractions to the algorithm's security subscriptions
    */
    public class SecurityChanges
    {
        /**
        /// Gets an instance that represents no changes have been made
        */
        public static final SecurityChanges None = new SecurityChanges(new List<Security>(), new List<Security>());

        private final HashSet<Security> _addedSecurities;
        private final HashSet<Security> _removedSecurities;

        /**
        /// Gets the symbols that were added by universe selection
        */
        public IReadOnlyList<Security> AddedSecurities
        {
            get { return _addedSecurities.OrderBy(x -> x.Symbol.Value).ToList(); }
        }

        /**
        /// Gets the symbols that were removed by universe selection. This list may
        /// include symbols that were removed, but are still receiving data due to
        /// existing holdings or open orders
        */
        public IReadOnlyList<Security> RemovedSecurities
        {
            get { return _removedSecurities.OrderBy(x -> x.Symbol.Value).ToList(); }
        }

        /**
        /// Initializes a new instance of the <see cref="SecurityChanges"/> class
        */
         * @param addedSecurities">Added symbols list
         * @param removedSecurities">Removed symbols list
        public SecurityChanges(IEnumerable<Security> addedSecurities, IEnumerable<Security> removedSecurities) {
            _addedSecurities = addedSecurities.ToHashSet();
            _removedSecurities = removedSecurities.ToHashSet();
        }

        /**
        /// Returns a new instance of <see cref="SecurityChanges"/> with the specified securities marked as added
        */
         * @param securities">The added securities
        @returns A new security changes instance with the specified securities marked as added
        public static SecurityChanges Added(params Security[] securities) {
            if( securities == null || securities.Length == 0) return None;
            return new SecurityChanges(securities.ToList(), new List<Security>());
        }

        /**
        /// Returns a new instance of <see cref="SecurityChanges"/> with the specified securities marked as removed
        */
         * @param securities">The removed securities
        @returns A new security changes instance with the specified securities marked as removed
        public static SecurityChanges Removed(params Security[] securities) {
            if( securities == null || securities.Length == 0) return None;
            return new SecurityChanges(new List<Security>(), securities.ToList());
        }

        /**
        /// Combines the results of two <see cref="SecurityChanges"/>
        */
         * @param left">The left side of the operand
         * @param right">The right side of the operand
        @returns Adds the additions together and removes any removals found in the additions, that is, additions take precendence
        public static SecurityChanges operator +(SecurityChanges left, SecurityChanges right) {
            // common case is adding something to nothing, shortcut these to prevent linqness
            if( left == None) return right;
            if( right == None) return left;

            additions = left.AddedSecurities.Union(right.AddedSecurities).ToList();
            removals = left.RemovedSecurities.Union(right.RemovedSecurities).Where(x -> !additions.Contains(x)).ToList();
            return new SecurityChanges(additions, removals);
        }

        #region Overrides of Object

        /**
        /// Returns a String that represents the current object.
        */
        @returns 
        /// A String that represents the current object.
        /// 
        /// <filterpriority>2</filterpriority>
        public @Override String toString() {
            if( AddedSecurities.Count == 0 && RemovedSecurities.Count == 0) {
                return "SecurityChanges: None";
            }
            added = string.Empty;
            if( AddedSecurities.Count != 0) {
                added = " Added: " + String.join( ",", AddedSecurities.Select(x -> x.Symbol.ID));
            }
            removed = string.Empty;
            if( RemovedSecurities.Count != 0) {
                removed = " Removed: " + String.join( ",", RemovedSecurities.Select(x -> x.Symbol.ID));
            }

            return "SecurityChanges: " + added + removed;
        }

        #endregion
    }
}