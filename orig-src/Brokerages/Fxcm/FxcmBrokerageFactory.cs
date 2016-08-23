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

package com.quantconnect.lean.Brokerages.Fxcm
{
    /**
    /// Provides an implementation of <see cref="IBrokerageFactory"/> that produces a <see cref="FxcmBrokerage"/>
    */
    public class FxcmBrokerageFactory : BrokerageFactory
    {
        private static final String DefaultServer = "http://www.fxcorporate.com/Hosts.jsp";
        private static final String DefaultTerminal = "Demo";

        /**
        /// Initializes a new instance of the <see cref="FxcmBrokerageFactory"/> class
        */
        public FxcmBrokerageFactory()
            : base(typeof(FxcmBrokerage)) {
        }

        /**
        /// Gets the brokerage data required to run the brokerage from configuration/disk
        */
        /// 
        /// The implementation of this property will create the brokerage data dictionary required for
        /// running live jobs. See <see cref="IJobQueueHandler.NextJob"/>
        /// 
        public @Override Map<String,String> BrokerageData
        {
            get
            {
                return new Map<String,String>
                {
                    { "fxcm-server", Config.Get( "fxcm-server", DefaultServer) },
                    { "fxcm-terminal", Config.Get( "fxcm-terminal", DefaultTerminal) },
                    { "fxcm-user-name", Config.Get( "fxcm-user-name") },
                    { "fxcm-password", Config.Get( "fxcm-password") },
                    { "fxcm-account-id", Config.Get( "fxcm-account-id") }
                };
            }
        }

        /**
        /// Gets a new instance of the <see cref="FxcmBrokerageModel"/>
        */
        public @Override IBrokerageModel BrokerageModel
        {
            get { return new FxcmBrokerageModel(); }
        }

        /**
        /// Creates a new <see cref="IBrokerage"/> instance
        */
         * @param job">The job packet to create the brokerage for
         * @param algorithm">The algorithm instance
        @returns A new brokerage instance
        public @Override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm) {
            errors = new List<String>();

            // read values from the brokerage data
            server = Read<String>(job.BrokerageData, "fxcm-server", errors);
            terminal = Read<String>(job.BrokerageData, "fxcm-terminal", errors);
            userName = Read<String>(job.BrokerageData, "fxcm-user-name", errors);
            password = Read<String>(job.BrokerageData, "fxcm-password", errors);
            accountId = Read<String>(job.BrokerageData, "fxcm-account-id", errors);

            if( errors.Count != 0) {
                // if we had errors then we can't create the instance
                throw new Exception( String.Join(Environment.NewLine, errors));
            }

            brokerage = new FxcmBrokerage(algorithm.Transactions, algorithm.Portfolio, server, terminal, userName, password, accountId);
            Composer.Instance.AddPart<IDataQueueHandler>(brokerage);

            return brokerage;
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        /// <filterpriority>2</filterpriority>
        public @Override void Dispose() {
        }

    }
}
