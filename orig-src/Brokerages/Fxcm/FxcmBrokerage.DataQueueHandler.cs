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

using System.Collections.Generic;
using System.Linq;
using com.fxcm.fix;
using com.fxcm.fix.pretrade;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using QuantConnect.Packets;

package com.quantconnect.lean.Brokerages.Fxcm
{
    /**
     * FXCM brokerage - implementation of IDataQueueHandler interface
    */
    public partial class FxcmBrokerage
    {
        private final List<Tick> _ticks = new List<Tick>();
        private final HashSet<Symbol> _subscribedSymbols = new HashSet<Symbol>(); 
        
        #region IDataQueueHandler implementation

        /**
         * Get the next ticks from the live trading data queue
        */
        @returns IEnumerable list of ticks since the last update.
        public IEnumerable<BaseData> GetNextTicks() {
            synchronized(_ticks) {
                copy = _ticks.ToArray();
                _ticks.Clear();
                return copy;
            }
        }

        /**
         * Adds the specified symbols to the subscription
        */
         * @param job Job we're subscribing for:
         * @param symbols The symbols to be added keyed by SecurityType
        public void Subscribe(LiveNodePacket job, IEnumerable<Symbol> symbols) {
            symbolsToSubscribe = (from symbol in symbols 
                                      where !_subscribedSymbols.Contains(symbol) 
                                      select symbol).ToList();
            if( symbolsToSubscribe.Count == 0)
                return;

            Log.Trace( "FxcmBrokerage.Subscribe(): %1$s", String.join( ",", symbolsToSubscribe));

            request = new MarketDataRequest();
            foreach (symbol in symbolsToSubscribe) {
                request.addRelatedSymbol(_fxcmInstruments[_symbolMapper.GetBrokerageSymbol(symbol)]);
            }
            request.setSubscriptionRequestType(SubscriptionRequestTypeFactory.SUBSCRIBE);
            request.setMDEntryTypeSet(MarketDataRequest.MDENTRYTYPESET_ALL);

            synchronized(_locker) {
                _gateway.sendMessage(request);
            }

            foreach (symbol in symbolsToSubscribe) {
                _subscribedSymbols.Add(symbol);
            }
        }

        /**
         * Removes the specified symbols to the subscription
        */
         * @param job Job we're processing.
         * @param symbols The symbols to be removed keyed by SecurityType
        public void Unsubscribe(LiveNodePacket job, IEnumerable<Symbol> symbols) {
            symbolsToUnsubscribe = (from symbol in symbols 
                                        where _subscribedSymbols.Contains(symbol) 
                                        select symbol).ToList();
            if( symbolsToUnsubscribe.Count == 0)
                return;

            Log.Trace( "FxcmBrokerage.Unsubscribe(): %1$s", String.join( ",", symbolsToUnsubscribe));

            request = new MarketDataRequest();
            foreach (symbol in symbolsToUnsubscribe) {
                request.addRelatedSymbol(_fxcmInstruments[_symbolMapper.GetBrokerageSymbol(symbol)]);
            }
            request.setSubscriptionRequestType(SubscriptionRequestTypeFactory.UNSUBSCRIBE);
            request.setMDEntryTypeSet(MarketDataRequest.MDENTRYTYPESET_ALL);

            synchronized(_locker) {
                _gateway.sendMessage(request);
            }

            foreach (symbol in symbolsToUnsubscribe) {
                _subscribedSymbols.Remove(symbol);
            }
        }

        #endregion

    }
}
