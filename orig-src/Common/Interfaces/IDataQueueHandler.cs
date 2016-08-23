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
using System.ComponentModel.Composition;
using QuantConnect.Data;
using QuantConnect.Packets;

package com.quantconnect.lean.Interfaces
{
    /**
    /// Task requestor interface with cloud system
    */
    [InheritedExport(typeof(IDataQueueHandler))]
    public interface IDataQueueHandler
    {
        /**
        /// Get the next ticks from the live trading data queue
        */
        @returns IEnumerable list of ticks since the last update.
        IEnumerable<BaseData> GetNextTicks();

        /**
        /// Adds the specified symbols to the subscription
        */
         * @param job">Job we're subscribing for:
         * @param symbols">The symbols to be added keyed by SecurityType
        void Subscribe(LiveNodePacket job, IEnumerable<Symbol> symbols);

        /**
        /// Removes the specified symbols to the subscription
        */
         * @param job">Job we're processing.
         * @param symbols">The symbols to be removed keyed by SecurityType
        void Unsubscribe(LiveNodePacket job, IEnumerable<Symbol> symbols);
    }
}
