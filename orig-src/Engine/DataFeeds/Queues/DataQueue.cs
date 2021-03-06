﻿/*
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
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Packets;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Queues
{
    /**
     * Live Data Queue is the cut out implementation of how to bind a custom live data source 
    */
    public class LiveDataQueue : IDataQueueHandler
    {
        /**
         * Desktop/Local doesn't support live data from this handler
        */
        @returns Tick
        public IEnumerable<BaseData> GetNextTicks() {
            throw new UnsupportedOperationException( "QuantConnect.Queues.LiveDataQueue has not implemented live data.");
        }

        /**
         * Desktop/Local doesn't support live data from this handler
        */
        public void Subscribe(LiveNodePacket job, IEnumerable<Symbol> symbols) {
            throw new UnsupportedOperationException( "QuantConnect.Queues.LiveDataQueue has not implemented live data.");
        }

        /**
         * Desktop/Local doesn't support live data from this handler
        */
        public void Unsubscribe(LiveNodePacket job, IEnumerable<Symbol> symbols) {
            throw new UnsupportedOperationException( "QuantConnect.Queues.LiveDataQueue has not implemented live data.");
        }
    }
}
