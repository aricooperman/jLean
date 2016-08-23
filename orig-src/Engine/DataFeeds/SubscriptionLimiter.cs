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

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
    /// Provides logic to prevent an algorithm from adding too many data subscriptions
    */
    public class SubscriptionLimiter
    {
        private static final int MinuteMemory = 2;
        private static final int SecondMemory = 10;
        private static final int TickMemory = 34;

        private final int _tickLimit;
        private final int _secondLimit;
        private final int _minuteLimit;
        private final BigDecimal _maxRamEstimate;
        private final Func<IEnumerable<Subscription>> _subscriptionsProvider;

        /**
        /// Initializes a new instance of the <see cref="SubscriptionLimiter"/> class
        */
         * @param subscriptionsProvider">Delegate used to provide access to the current subscriptions
         * @param tickLimit">The maximum number of tick symbols
         * @param secondLimit">The maximum number of second symbols
         * @param minuteLimit">The maximum number of minute symbol
        public SubscriptionLimiter(Func<IEnumerable<Subscription>> subscriptionsProvider, int tickLimit, int secondLimit, int minuteLimit) {
            _subscriptionsProvider = subscriptionsProvider;
            _tickLimit = tickLimit;
            _secondLimit = secondLimit;
            _minuteLimit = minuteLimit;
            _maxRamEstimate = GetRamEstimate(minuteLimit, secondLimit, tickLimit);
        }

        /**
        /// Get the number of securities that have this resolution.
        */
         * @param resolution">Search resolution value.
        @returns Count of the securities
        public int GetResolutionCount(Resolution resolution) {
            return (from subscription in _subscriptionsProvider()
                    let security = subscription.Security
                    where security.Resolution == resolution
                    // don't count feeds we auto add
                    where !subscription.Configuration.IsInternalFeed
                    select security.Resolution).Count();
        }

        /**
        /// Gets the number of available slots for the specifed resolution
        */
         * @param resolution">The resolution we want to add subscriptions at
        @returns The number of subscriptions we can safely add without maxing out the count (ram usage depends on other factors)
        public int GetRemaining(Resolution resolution) {
            return GetResolutionLimit(resolution) - GetResolutionCount(resolution);
        }

        /**
        /// Determines if we can add a subscription for the specified resolution
        */
         * @param resolution">The new subscription resolution to check
         * @param reason">When this function returns false, this is the reason we are unable to add the subscription
        @returns True if we can add a subscription for the specified resolution while
        /// remaining within our limits, false if this will put us over our limits
        public boolean CanAddSubscription(Resolution resolution, out String reason) {
            reason = null;
            limit = GetResolutionLimit(resolution);

            // we increment the resolution since we're about to add one
            count = GetResolutionCount(resolution) + 1;
            
            // check max counts of symbols
            if( count >= limit) {
                reason = GetCountLimitReason(resolution);
                return false;
            }
            
            // check ram usage
            ramEstimate = GetRamEstimate(
                GetResolutionCount(Resolution.Minute), 
                GetResolutionCount(Resolution.Second), 
                GetResolutionCount(Resolution.Tick)
                );

            // finally, check current estimate against the precomputed maximum
            if( ramEstimate > _maxRamEstimate) {
                reason = GetMaxRamReason(ramEstimate);
                return false;
            }
            return true;
        }

        /**
        /// Gets the max number of symbols for the specified resolution
        */
         * @param resolution">The resolution whose limit we seek
        @returns The specified resolution's limit
        private int GetResolutionLimit(Resolution resolution) {
            switch (resolution) {
                case Resolution.Tick:
                    return _tickLimit;

                case Resolution.Second:
                    return _secondLimit;

                case Resolution.Minute:
                    return _minuteLimit;

                case Resolution.Hour:
                case Resolution.Daily:
                    return int.MaxValue;

                default:
                    throw new ArgumentOutOfRangeException( "resolution", resolution, null );
            }
        }

        /**
        /// Estimated ram usage with this symbol combination:
        */
        @returns Decimal estimate of the number of MB ram the requested assets would consume
        private BigDecimal GetRamEstimate(int minute, int second, int tick) {
            return MinuteMemory * minute + SecondMemory * second + TickMemory * tick;
        }

        /**
        /// Gets reason String for having a larger count than the limits
        */
        private String GetCountLimitReason(Resolution resolution) {
            limit = GetResolutionLimit(resolution);
            return String.format( "We currently only support %1$s %2$s at a time due to physical memory limitations", limit, resolution.toString().toLowerCase());
        }

        /**
        /// Gets reason String for having a larger estimated ram usage than the limits
        */
        private String GetMaxRamReason( BigDecimal currentEstimatedRam) {
            return String.format( "We estimate you will run out of memory (%1$smb of %2$smb physically available). " +
                "Please reduce the number of symbols you're analysing or if in live trading upgrade your server to allow more memory.",
                currentEstimatedRam, _maxRamEstimate
                );
        }
    }
}