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
using System.ComponentModel.Composition;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Packets;
using QuantConnect.Securities;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Datafeed interface for creating custom datafeed sources.
    */
    [InheritedExport(typeof(IDataFeed))]
    public interface IDataFeed : IEnumerable<TimeSlice>
    {
        /**
         * Gets all of the current subscriptions this data feed is processing
        */
        IEnumerable<Subscription> Subscriptions
        {
            get;
        }

        /**
         * Public flag indicator that the thread is still busy.
        */
        boolean IsActive
        {
            get;
        }

        /**
         * Initializes the data feed for the specified job and algorithm
        */
        void Initialize(IAlgorithm algorithm, AlgorithmNodePacket job, IResultHandler resultHandler, IMapFileProvider mapFileProvider, IFactorFileProvider factorFileProvider);

        /**
         * Adds a new subscription to provide data for the specified security.
        */
         * @param universe The universe the subscription is to be added to
         * @param security The security to add a subscription for
         * @param config The subscription config to be added
         * @param utcStartTime The start time of the subscription
         * @param utcEndTime The end time of the subscription
        @returns True if the subscription was created and added successfully, false otherwise
        boolean AddSubscription(Universe universe, Security security, SubscriptionDataConfig config, DateTime utcStartTime, DateTime utcEndTime);

        /**
         * Removes the subscription from the data feed, if it exists
        */
         * @param configuration The configuration of the subscription to remove
        @returns True if the subscription was successfully removed, false otherwise
        boolean RemoveSubscription(SubscriptionDataConfig configuration);

        /**
         * Primary entry point.
        */
        void Run();

        /**
         * External controller calls to signal a terminate of the thread.
        */
        void Exit();
    }
}
