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
using QuantConnect.Data;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Provides a factory method for creating <see cref="ISubscriptionDataSourceReader"/> instances
    */
    public static class SubscriptionDataSourceReader
    {
        /**
         * Creates a new <see cref="ISubscriptionDataSourceReader"/> capable of handling the specified <paramref name="source"/>
        */
         * @param source The subscription data source to create a factory for
         * @param config The configuration of the subscription
         * @param date The date to be processed
         * @param isLiveMode True for live mode, false otherwise
        @returns A new <see cref="ISubscriptionDataSourceReader"/> that can read the specified <paramref name="source"/>
        public static ISubscriptionDataSourceReader ForSource(SubscriptionDataSource source, SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            switch (source.Format) {
                case FileFormat.Csv:
                    return new TextSubscriptionDataSourceReader(config, date, isLiveMode);

                case FileFormat.Collection:
                    return new CollectionSubscriptionDataSourceReader(config, date, isLiveMode);

                case FileFormat.ZipEntryName:
                    return new ZipEntryNameSubscriptionDataSourceReader(config, date, isLiveMode);

                default:
                    throw new NotImplementedException( "SubscriptionFactory.ForSource( " + source + ") has not been implemented yet.");
            }
        }
    }

}
