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
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Util;

package com.quantconnect.lean.Brokerages.Oanda
{
    /**
     * Provides an implementations of <see cref="IBrokerageFactory"/> that produces a <see cref="OandaBrokerage"/>
    */
    public class OandaBrokerageFactory: BrokerageFactory
    {
        /**
         * Initializes a new instance of the <see cref="OandaBrokerageFactory"/> class.
        */
        public OandaBrokerageFactory() 
            : base(typeof(OandaBrokerage)) {
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        public @Override void Dispose() {
        }

        /**
         * Gets the brokerage data required to run the brokerage from configuration/disk
        */
         * 
         * The implementation of this property will create the brokerage data dictionary required for
         * running live jobs. See <see cref="IJobQueueHandler.NextJob"/>
         * 
        public @Override Map<String,String> BrokerageData
        {
            get
            {
                return new Map<String,String>
                {
                    { "oanda-environment", Config.Get( "oanda-environment") },
                    { "oanda-access-token", Config.Get( "oanda-access-token") },
                    { "oanda-account-id", Config.Get( "oanda-account-id") }
                };
            }
        }

        /**
         * Gets a new instance of the <see cref="OandaBrokerageModel"/>
        */
        public @Override IBrokerageModel BrokerageModel
        {
            get { return new OandaBrokerageModel(); }
        }

        /**
         * Creates a new <see cref="IBrokerage"/> instance
        */
         * @param job The job packet to create the brokerage for
         * @param algorithm The algorithm instance
        @returns A new brokerage instance
        public @Override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm) {
            errors = new List<String>();

            // read values from the brokerage data
            environment = Read<Environment>(job.BrokerageData, "oanda-environment", errors);
            accessToken = Read<String>(job.BrokerageData, "oanda-access-token", errors);
            accountId = Read<Integer>(job.BrokerageData, "oanda-account-id", errors);

            if( errors.Count != 0) {
                // if we had errors then we can't create the instance
                throw new Exception( String.Join(System.Environment.NewLine, errors));
            }

            brokerage = new OandaBrokerage(algorithm.Transactions, algorithm.Portfolio, environment, accessToken, accountId);
            Composer.Instance.AddPart<IDataQueueHandler>(brokerage);

            return brokerage;
        }

    }
}
