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
using QuantConnect.Interfaces;
using QuantConnect.Packets;

package com.quantconnect.lean.Brokerages
{
    /**
     * Provides a base implementation of IBrokerageFactory that provides a helper for reading data from a job's brokerage data dictionary
    */
    public abstract class BrokerageFactory : IBrokerageFactory
    {
        private final Class _brokerageType;

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
         * <filterpriority>2</filterpriority>
        public abstract void Dispose();

        /**
         * Gets the type of brokerage produced by this factory
        */
        public Class BrokerageType
        {
            get { return _brokerageType; }
        }

        /**
         * Gets the brokerage data required to run the brokerage from configuration/disk
        */
         * 
         * The implementation of this property will create the brokerage data dictionary required for
         * running live jobs. See <see cref="IJobQueueHandler.NextJob"/>
         * 
        public abstract Map<String,String> BrokerageData { get; }

        /**
         * Gets a brokerage model that can be used to model this brokerage's unique
         * behaviors
        */
        public abstract IBrokerageModel BrokerageModel { get; }

        /**
         * Creates a new IBrokerage instance
        */
         * @param job The job packet to create the brokerage for
         * @param algorithm The algorithm instance
        @returns A new brokerage instance
        public abstract IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm);

        /**
         * Initializes a new instance of the <see cref="BrokerageFactory"/> class for the specified <paramref name="brokerageType"/>
        */
         * @param brokerageType The type of brokerage created by this factory
        protected BrokerageFactory(Type brokerageType) {
            _brokerageType = brokerageType;
        }
        
        /**
         * Reads a value from the brokerage data, adding an error if the key is not found
        */
        protected static T Read<T>(ImmutableMap<String,String> brokerageData, String key, ICollection<String> errors) 
            where T : IConvertible
        {
            String value;
            if( !brokerageData.TryGetValue(key, out value)) {
                errors.Add( "BrokerageFactory.CreateBrokerage(): Missing key: " + key);
                return default(T);
            }

            try
            {
                return value.ConvertTo<T>();
            }
            catch (Exception err) {
                errors.Add( String.format( "BrokerageFactory.CreateBrokerage(): Error converting key '%1$s' with value '%2$s'. %3$s", key, value, err.Message));
                return default(T);
            }
        }
    }
}