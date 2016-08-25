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
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Securities;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Represents the data required for a data feed to process a single subsciption
    */
    public class Subscription : IEnumerator<BaseData>
    {
        private final IEnumerator<BaseData> _enumerator;

        /**
         * Gets the universe for this subscription
        */
        public Universe Universe
        {
            get;
            private set;
        }

        /**
         * Gets the security this subscription points to
        */
        public final Security Security;

        /**
         * Gets the configuration for this subscritions
        */
        public final SubscriptionDataConfig Configuration;

        /**
         * Gets the data time zone associated with this subscription
        */
        public ZoneId TimeZone
        {
            get { return Security.Exchange.TimeZone; }
        }

        /**
         * Gets the offset provider for time zone conversions to and from the data's local time
        */
        public final TimeZoneOffsetProvider OffsetProvider;

        /**
         * Gets the most current value from the subscription source
        */
        public BigDecimal RealtimePrice { get; set; }

        /**
         * Gets true if this subscription is finished, false otherwise
        */
        public boolean EndOfStream { get; private set; }

        /**
         * Gets true if this subscription is used in universe selection
        */
        public boolean IsUniverseSelectionSubscription { get; private set; }

        /**
         * Gets the start time of this subscription in UTC
        */
        public DateTime UtcStartTime { get; private set; }

        /**
         * Gets the end time of this subscription in UTC
        */
        public DateTime UtcEndTime { get; private set; }

        /**
         * Initializes a new instance of the <see cref="Subscription"/> class with a universe
        */
         * @param universe Specified for universe subscriptions
         * @param security The security this subscription is for
         * @param configuration The subscription configuration that was used to generate the enumerator
         * @param enumerator The subscription's data source
         * @param timeZoneOffsetProvider The offset provider used to convert data local times to utc
         * @param utcStartTime The start time of the subscription
         * @param utcEndTime The end time of the subscription
         * @param isUniverseSelectionSubscription True if this is a subscription for universe selection,
         * that is, the configuration is used to produce the used to perform universe selection, false for a
         * normal data subscription, i.e, SPY
        public Subscription(Universe universe,
            Security security,
            SubscriptionDataConfig configuration,
            IEnumerator<BaseData> enumerator,
            TimeZoneOffsetProvider timeZoneOffsetProvider,
            DateTime utcStartTime,
            DateTime utcEndTime,
            boolean isUniverseSelectionSubscription) {
            Universe = universe;
            Security = security;
            _enumerator = enumerator;
            IsUniverseSelectionSubscription = isUniverseSelectionSubscription;
            Configuration = configuration;
            OffsetProvider = timeZoneOffsetProvider;

            UtcStartTime = utcStartTime;
            UtcEndTime = utcEndTime;
        }

        /**
         * Advances the enumerator to the next element of the collection.
        */
        @returns 
         * true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
         * 
         * <exception cref="T:System.InvalidOperationException The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public boolean MoveNext() {
            if( EndOfStream) {
                return false;
            }

            moveNext = _enumerator.MoveNext();
            EndOfStream = !moveNext;
            Current = _enumerator.Current;
            return moveNext;
        }

        /**
         * Sets the enumerator to its initial position, which is before the first element in the collection.
        */
         * <exception cref="T:System.InvalidOperationException The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public void Reset() {
            _enumerator.Reset();
        }

        /**
         * Gets the element in the collection at the current position of the enumerator.
        */
        @returns 
         * The element in the collection at the current position of the enumerator.
         * 
        public BaseData Current { get; private set; }

        /**
         * Gets the current element in the collection.
        */
        @returns 
         * The current element in the collection.
         * 
         * <filterpriority>2</filterpriority>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
         * <filterpriority>2</filterpriority>
        public void Dispose() {
            EndOfStream = true;
            _enumerator.Dispose();
        }

        /**
         * Serves as a hash function for a particular type. 
        */
        @returns 
         * A hash code for the current <see cref="T:System.Object"/>.
         * 
         * <filterpriority>2</filterpriority>
        public @Override int hashCode() {
            return Configuration.Symbol.hashCode();
        }
    }
}