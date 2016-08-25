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
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Securities;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Provides the ability to synchronize subscriptions into time slices
    */
    public class SubscriptionSynchronizer
    {
        private final UniverseSelection _universeSelection;

        /**
         * Event fired when a subscription is finished
        */
        public event EventHandler<Subscription> SubscriptionFinished;

        /**
         * Initializes a new instance of the <see cref="SubscriptionSynchronizer"/> class
        */
         * @param universeSelection The universe selection instance used to handle universe
         * selection subscription output
        public SubscriptionSynchronizer(UniverseSelection universeSelection) {
            _universeSelection = universeSelection;
        }

        /**
         * Syncs the specifies subscriptions at the frontier time
        */
         * @param frontier The time used for syncing, data in the future won't be included in this time slice
         * @param subscriptions The subscriptions to sync
         * @param sliceTimeZone The time zone of the created slice object
         * @param cashBook The cash book, used for creating the cash book updates
         * @param nextFrontier The next frontier time as determined by the first piece of data in the future ahead of the frontier.
         * This value will equal DateTime.MaxValue when the subscriptions are all finished
        @returns A time slice for the specified frontier time
        public TimeSlice Sync(DateTime frontier, IEnumerable<Subscription> subscriptions, ZoneId sliceTimeZone, CashBook cashBook, out DateTime nextFrontier) {
            changes = SecurityChanges.None;
            nextFrontier = DateTime.MaxValue;
            earlyBirdTicks = nextFrontier.Ticks;
            data = new List<DataFeedPacket>();

            SecurityChanges newChanges;
            do
            {
                newChanges = SecurityChanges.None;
                foreach (subscription in subscriptions) {
                    if( subscription.EndOfStream) {
                        OnSubscriptionFinished(subscription);
                        continue;
                    }

                    // prime if needed
                    if( subscription.Current == null ) {
                        if( !subscription.MoveNext()) {
                            OnSubscriptionFinished(subscription);
                            continue;
                        }
                    }

                    packet = new DataFeedPacket(subscription.Security, subscription.Configuration);
                    data.Add(packet);

                    configuration = subscription.Configuration;
                    offsetProvider = subscription.OffsetProvider;
                    currentOffsetTicks = offsetProvider.GetOffsetTicks(frontier);
                    while (subscription.Current.EndTime.Ticks - currentOffsetTicks <= frontier.Ticks) {
                        // we want bars rounded using their subscription times, we make a clone
                        // so we don't interfere with the enumerator's internal logic
                        clone = subscription.Current.Clone(subscription.Current.IsFillForward);
                        clone.Time = clone.Time.ExchangeRoundDown(configuration.Increment, subscription.Security.Exchange.Hours, configuration.ExtendedMarketHours);
                        packet.Add(clone);
                        if( !subscription.MoveNext()) {
                            OnSubscriptionFinished(subscription);
                            break;
                        }
                    }

                    // we have new universe data to select based on
                    if( subscription.IsUniverseSelectionSubscription && packet.Count > 0) {
                        // assume that if the first item is a base data collection then the enumerator handled the aggregation,
                        // otherwise, load all the the data into a new collection instance
                        collection = packet.Data[0] as BaseDataCollection ?? new BaseDataCollection(frontier, subscription.Configuration.Symbol, packet.Data);
                        newChanges += _universeSelection.ApplyUniverseSelection(subscription.Universe, frontier, collection);
                    }

                    if( subscription.Current != null ) {
                        // take the earliest between the next piece of data or the next tz discontinuity
                        earlyBirdTicks = Math.Min(earlyBirdTicks, Math.Min(subscription.Current.EndTime.Ticks - currentOffsetTicks, offsetProvider.GetNextDiscontinuity()));
                    }
                }

                changes += newChanges;
            }
            while (newChanges != SecurityChanges.None);

            nextFrontier = new DateTime(Math.Max(earlyBirdTicks, frontier.Ticks), DateTimeKind.Utc);

            return TimeSlice.Create(frontier, sliceTimeZone, cashBook, data, changes);
        }

        /**
         * Event invocator for the <see cref="SubscriptionFinished"/> event
        */
        protected void OnSubscriptionFinished(Subscription subscription) {
            handler = SubscriptionFinished;
            if( handler != null ) handler(this, subscription);
        }
    }
}