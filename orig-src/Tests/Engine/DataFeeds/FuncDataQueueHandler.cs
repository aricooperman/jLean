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
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Packets;

package com.quantconnect.lean.Tests.Engine.DataFeeds
{
    /**
     * Provides an implementation of <see cref="IDataQueueHandler"/> that can be specified
     * via a function
    */
    public class FuncDataQueueHandler : IDataQueueHandler
    {
        private final object _synchronized= new object();
        private final HashSet<Symbol> _subscriptions = new HashSet<Symbol>();
        private final Func<FuncDataQueueHandler, IEnumerable<BaseData>> _getNextTicksFunction;

        /**
         * Gets the subscriptions currently being managed by the queue handler
        */
        public List<Symbol> Subscriptions
        {
            get { synchronized(_lock) return _subscriptions.ToList(); }
        }

        /**
         * Initializes a new instance of the <see cref="FuncDataQueueHandler"/> class
        */
         * @param getNextTicksFunction The functional implementation for the <see cref="GetNextTicks"/> function
        public FuncDataQueueHandler(Func<FuncDataQueueHandler, IEnumerable<BaseData>> getNextTicksFunction) {
            _getNextTicksFunction = getNextTicksFunction;
        }

        /**
         * Get the next ticks from the live trading data queue
        */
        @returns IEnumerable list of ticks since the last update.
        public IEnumerable<BaseData> GetNextTicks() {
            return _getNextTicksFunction(this);
        }

        /**
         * Adds the specified symbols to the subscription
        */
         * @param job Job we're subscribing for:
         * @param symbols The symbols to be added keyed by SecurityType
        public void Subscribe(LiveNodePacket job, IEnumerable<Symbol> symbols) {
            foreach (symbol in symbols) {
                synchronized(_lock) _subscriptions.Add(symbol);
            }
        }

        /**
         * Removes the specified symbols to the subscription
        */
         * @param job Job we're processing.
         * @param symbols The symbols to be removed keyed by SecurityType
        public void Unsubscribe(LiveNodePacket job, IEnumerable<Symbol> symbols) {
            foreach (symbol in symbols) {
                synchronized(_lock) _subscriptions.Remove(symbol);
            }
        }
    }
}