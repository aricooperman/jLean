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
using System.Globalization;
using QuantConnect.Brokerages.Backtesting;
using QuantConnect.Interfaces;
using QuantConnect.Packets;

package com.quantconnect.lean.Brokerages.Paper
{
    /**
    /// The factory type for the <see cref="PaperBrokerage"/>
    */
    public class PaperBrokerageFactory : IBrokerageFactory
    {
        /**
        /// Gets the type of brokerage produced by this factory
        */
        public Type BrokerageType
        {
            get { return typeof(PaperBrokerage); }
        }

        /**
        /// Gets the brokerage data required to run the IB brokerage from configuration
        */
        /// 
        /// The implementation of this property will create the brokerage data dictionary required for
        /// running live jobs. See <see cref="IJobQueueHandler.NextJob"/>
        /// 
        public Map<String,String> BrokerageData
        {
            get { return new Map<String,String>(); }
        }

        /**
        /// Gets a new instance of the <see cref="InteractiveBrokersBrokerageModel"/>
        */
        public IBrokerageModel BrokerageModel
        {
            get { return new DefaultBrokerageModel(); }
        }

        /**
        /// Creates a new IBrokerage instance
        */
         * @param job">The job packet to create the brokerage for
         * @param algorithm">The algorithm instance
        @returns A new brokerage instance
        public IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm) {
            return new PaperBrokerage(algorithm, job);
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        /// <filterpriority>2</filterpriority>
        public void Dispose() {
            // NOP
        }
    }
}