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
using Krs.Ats.IBNet;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Brokerages.InteractiveBrokers
{
    /**
     * Factory type for the <see cref="InteractiveBrokersBrokerage"/>
    */
    public class InteractiveBrokersBrokerageFactory : BrokerageFactory
    {
        /**
         * Initializes a new instance of the InteractiveBrokersBrokerageFactory class
        */
        public InteractiveBrokersBrokerageFactory()
            : base(typeof(InteractiveBrokersBrokerage)) {
        }

        /**
         * Gets the brokerage data required to run the IB brokerage from configuration
        */
         * 
         * The implementation of this property will create the brokerage data dictionary required for
         * running live jobs. See <see cref="IJobQueueHandler.NextJob"/>
         * 
        public @Override Map<String,String> BrokerageData
        {
            get
            {
                data = new Map<String,String>();
                data.Add( "ib-account", Config.Get( "ib-account"));
                data.Add( "ib-user-name", Config.Get( "ib-user-name"));
                data.Add( "ib-password", Config.Get( "ib-password"));
                data.Add( "ib-agent-description", Config.Get( "ib-agent-description"));
                return data;
            }
        }

        /**
         * Gets a new instance of the <see cref="InteractiveBrokersBrokerageModel"/>
        */
        public @Override IBrokerageModel BrokerageModel
        {
            get { return new InteractiveBrokersBrokerageModel(); }
        }

        /**
         * Creates a new IBrokerage instance and set ups the environment for the brokerage
        */
         * @param job The job packet to create the brokerage for
         * @param algorithm The algorithm instance
        @returns A new brokerage instance
        public @Override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm) {
            errors = new List<String>();

            // read values from the brokerage datas
            useTws = Config.GetBool( "ib-use-tws");
            port = Config.GetInt( "ib-port", 4001);
            host = Config.Get( "ib-host", "127.0.0.1");
            twsDirectory = Config.Get( "ib-tws-dir", "C:\\Jts");
            ibControllerDirectory = Config.Get( "ib-controller-dir", "C:\\IBController");

            account = Read<String>(job.BrokerageData, "ib-account", errors);
            userID = Read<String>(job.BrokerageData, "ib-user-name", errors);
            password = Read<String>(job.BrokerageData, "ib-password", errors);
            agentDescription = Read<AgentDescription>(job.BrokerageData, "ib-agent-description", errors);

            if( errors.Count != 0) {
                // if we had errors then we can't create the instance
                throw new Exception( String.Join(Environment.NewLine, errors));
            }
            
            // launch the IB gateway
            InteractiveBrokersGatewayRunner.Start(ibControllerDirectory, twsDirectory, userID, password, useTws);

            ib = new InteractiveBrokersBrokerage(algorithm.Transactions, algorithm.Portfolio, account, host, port, agentDescription);
            Composer.Instance.AddPart<IDataQueueHandler>(ib);
            return ib;
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
         * Stops the InteractiveBrokersGatewayRunner
        */
         * <filterpriority>2</filterpriority>
        public @Override void Dispose() {
            InteractiveBrokersGatewayRunner.Stop();
        }
    }
}
