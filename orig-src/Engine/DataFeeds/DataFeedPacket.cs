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

using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Securities;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Defines a container type to hold data produced by a data feed subscription
    */
    public class DataFeedPacket
    {
        private final List<BaseData> _data;

        /**
         * The security
        */
        public Security Security
        {
            get; private set;
        }

        /**
         * The subscription configuration that produced this data
        */
        public SubscriptionDataConfig Configuration
        {
            get; private set;
        }

        /**
         * Gets the number of data points held within this packet
        */
        public int Count
        {
            get { return _data.Count; }
        }

        /**
         * The data for the security
        */
        public List<BaseData> Data
        {
            get { return _data; }
        }

        /**
         * Initializes a new instance of the <see cref="DataFeedPacket"/> class
        */
         * @param security The security whose data is held in this packet
         * @param configuration The subscription configuration that produced this data
        public DataFeedPacket(Security security, SubscriptionDataConfig configuration) {
            Security = security;
            Configuration = configuration;
            _data = new List<BaseData>();
        }

        /**
         * Initializes a new instance of the <see cref="DataFeedPacket"/> class
        */
         * @param security The security whose data is held in this packet
         * @param configuration The subscription configuration that produced this data
         * @param data The data to add to this packet. The list reference is reused
         * internally and NOT copied.
        public DataFeedPacket(Security security, SubscriptionDataConfig configuration, List<BaseData> data) {
            Security = security;
            Configuration = configuration;
            _data = data;
        }

        /**
         * Adds the specified data to this packet
        */
         * @param data The data to be added to this packet
        public void Add(BaseData data) {
            _data.Add(data);
        }
    }
}