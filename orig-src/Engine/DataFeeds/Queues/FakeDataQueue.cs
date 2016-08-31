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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using Timer = System.Timers.Timer;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Queues
{
    /**
     * This is an implementation of <see cref="IDataQueueHandler"/> used for testing
    */
    public class FakeDataQueue : IDataQueueHandler
    {
        private int count;
        private final Random _random = new Random();

        private final Timer _timer;
        private final ConcurrentQueue<BaseData> _ticks;
        private final HashSet<Symbol> _symbols;
        private final object _sync = new object();

        /**
         * Initializes a new instance of the <see cref="FakeDataQueue"/> class to randomly emit data for each symbol
        */
        public FakeDataQueue() {
            _ticks = new ConcurrentQueue<BaseData>();
            _symbols = new HashSet<Symbol>();
            
            // load it up to start
            PopulateQueue();
            PopulateQueue();
            PopulateQueue();
            PopulateQueue();
            
            _timer = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 1000,
            };
            
            lastCount = 0;
            lastTime = DateTime.Now;
            _timer.Elapsed += (sender, args) =>
            {
                elapsed = (DateTime.Now - lastTime);
                ticksPerSecond = (count - lastCount)/elapsed.TotalSeconds;
                Console.WriteLine( "TICKS PER SECOND:: " + ticksPerSecond.toString( "000000.0") + " ITEMS IN QUEUE:: " + _ticks.Count);
                lastCount = count;
                lastTime = DateTime.Now;
                PopulateQueue();
            };
        }

        /**
         * Get the next ticks from the live trading data queue
        */
        @returns IEnumerable list of ticks since the last update.
        public IEnumerable<BaseData> GetNextTicks() {
            BaseData tick;
            while (_ticks.TryDequeue(out tick)) {
                yield return tick;
                Interlocked.Increment(ref count);
            }
        }

        /**
         * Adds the specified symbols to the subscription
        */
         * @param job Job we're subscribing for:
         * @param symbols The symbols to be added keyed by SecurityType
        public void Subscribe(LiveNodePacket job, IEnumerable<Symbol> symbols) {
            foreach (symbol in symbols) {
                synchronized(_sync) {
                    _symbols.Add(symbol);
                }
            }
        }

        /**
         * Removes the specified symbols to the subscription
        */
         * @param job Job we're processing.
         * @param symbols The symbols to be removed keyed by SecurityType
        public void Unsubscribe(LiveNodePacket job, IEnumerable<Symbol> symbols) {
            foreach (symbol in symbols) {
                synchronized(_sync) {
                    _symbols.Remove(symbol);
                }
            }
        }

        /**
         * Pumps a bunch of ticks into the queue
        */
        private void PopulateQueue() {
            List<Symbol> symbols;
            synchronized(_sync) {
                symbols = _symbols.ToList();
            }

            foreach (symbol in symbols) {
                // emits 500k per second
                for (int i = 0; i < 500000; i++) {
                    _ticks.Enqueue(new Tick
                    {
                        Time = DateTime.Now,
                        Symbol = symbol,
                        Value = 10 + (decimal)Math.Abs(Math.Sin(DateTime.Now Extensions.timeOfDay(  ).TotalMinutes)),
                        TickType = TickType.Trade,
                        Quantity = _random.Next(10, (int)_timer.Interval)
                    });
                }
            }
        }
    }
}
