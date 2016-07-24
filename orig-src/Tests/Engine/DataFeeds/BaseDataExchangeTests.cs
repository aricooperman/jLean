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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Securities;

namespace QuantConnect.Tests.Engine.DataFeeds
{
    [TestFixture]
    public class BaseDataExchangeTests
    {
        [Test]
        public void FiresCorrectHandlerBySymbol()
        {
            dataQueue = new ConcurrentQueue<BaseData>();
            exchange = CreateExchange(dataQueue);
            exchange.SleepInterval = 1;

            firedHandler = false;
            firedWrongHandler = false;
            exchange.SetDataHandler(Symbols.SPY, spy =>
            {
                firedHandler = true;
            });
            exchange.SetDataHandler(Symbols.EURUSD, eurusd =>
            {
                firedWrongHandler = true;
            });

            dataQueue.Enqueue(new Tick{Symbol = Symbols.SPY});

            Task.Run(() => exchange.Start());

            Thread.Sleep(50);

            Assert.IsTrue(firedHandler);
            Assert.IsFalse(firedWrongHandler);
        }

        [Test]
        public void RemovesHandlerBySymbol()
        {
            dataQueue = new ConcurrentQueue<BaseData>();
            exchange = CreateExchange(dataQueue);

            firedHandler = false;
            exchange.SetDataHandler(Symbols.SPY, spy =>
            {
                firedHandler = true;
            });
            exchange.RemoveDataHandler(Symbols.SPY);

            dataQueue.Enqueue(new Tick {Symbol = Symbols.SPY});

            Task.Run(() => exchange.Start());

            Thread.Sleep(10);

            Assert.IsFalse(firedHandler);
        }

        [Test, Category("TravisExclude")]
        public void EndsQueueConsumption()
        {
            dataQueue = new ConcurrentQueue<BaseData>();
            exchange = CreateExchange(dataQueue);

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1);
                    dataQueue.Enqueue(new Tick {Symbol = Symbols.SPY, Time = DateTime.UtcNow});
                }
            });

            BaseData last = null;
            exchange.SetDataHandler(Symbols.SPY, spy =>
            {
                last = spy;
            });

            Task.Run(() => exchange.Start());

            Thread.Sleep(1);

            Thread.Sleep(25);

            exchange.Stop();

            endTime = DateTime.UtcNow;

            Assert.IsNotNull(last);
            Assert.IsTrue(last.Time <= endTime);
        }

        [Test]
        public void DefaultErrorHandlerDoesNotStopQueueConsumption()
        {
            dataQueue = new ConcurrentQueue<BaseData>();
            exchange = CreateExchange(dataQueue);

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1);
                    dataQueue.Enqueue(new Tick { Symbol = Symbols.SPY, Time = DateTime.UtcNow });
                }
            });

            first = true;
            BaseData last = null;
            exchange.SetDataHandler(Symbols.SPY, spy =>
            {
                if (first)
                {
                    first = false;
                    throw new Exception("This exception should be swalloed by the exchange!");
                }
                last = spy;
            });

            Task.Run(() => exchange.Start());

            Thread.Sleep(50);

            exchange.Stop();

            Assert.IsNotNull(last);
        }

        [Test]
        public void SetErrorHandlerExitsOnTrueReturn()
        {
            dataQueue = new ConcurrentQueue<BaseData>();
            exchange = CreateExchange(dataQueue);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1);
                    dataQueue.Enqueue(new Tick { Symbol = Symbols.SPY, Time = DateTime.UtcNow });
                }
            });

            first = true;
            BaseData last = null;
            exchange.SetDataHandler(Symbols.SPY, spy =>
            {
                if (first)
                {
                    first = false;
                    throw new Exception();
                }
                last = spy;
            });

            exchange.SetErrorHandler(error => true);

            Task.Run(() => exchange.Start());

            Thread.Sleep(25);

            exchange.Stop();

            Assert.IsNull(last);
        }

        [Test]
        public void RespectsShouldMoveNext()
        {
            exchange = new BaseDataExchange("test");
            exchange.SetErrorHandler(exception => true);
            exchange.AddEnumerator(Symbol.Empty, new List<BaseData> {new Tick()}.GetEnumerator(), () => false);

            isFaultedEvent = new ManualResetEvent(false);
            isCompletedEvent = new ManualResetEvent(false);
            Task.Run(() => exchange.Start(new CancellationTokenSource(50).Token)).ContinueWith(task =>
            {
                if (task.IsFaulted) isFaultedEvent.Set();
                isCompletedEvent.Set();
            });

            isCompletedEvent.WaitOne();
            Assert.IsFalse(isFaultedEvent.WaitOne(0));
        }

        [Test]
        public void FiresOnEnumeratorFinishedEvents()
        {
            exchange = new BaseDataExchange("test");
            IEnumerator<BaseData> enumerator = new List<BaseData>().GetEnumerator();

            isCompletedEvent = new ManualResetEvent(false);
            exchange.AddEnumerator(Symbol.Empty, enumerator, () => true, handler => isCompletedEvent.Set());
            Task.Run(() => exchange.Start(new CancellationTokenSource(50).Token));

            isCompletedEvent.WaitOne();
        }

        [Test]
        public void RemovesBySymbol()
        {
            exchange = new BaseDataExchange("test");
            enumerator = new List<BaseData> {new Tick {Symbol = Symbols.SPY}}.GetEnumerator();
            exchange.AddEnumerator(Symbols.SPY, enumerator);
            removed = exchange.RemoveEnumerator(Symbols.AAPL);
            Assert.IsNull(removed);
            removed = exchange.RemoveEnumerator(Symbols.SPY);
            Assert.AreEqual(Symbols.SPY, removed.Symbol);
        }

        private sealed class ExceptionEnumerator<T> : IEnumerator<T>
        {
            public void Reset() { }
            public void Dispose() { }
            public T Current { get; private set; }
            object IEnumerator.Current { get { return Current; } }
            public boolean MoveNext() { throw new Exception("ExceptionEnumerator.MoveNext always throws exceptions!"); }
        }

        private static BaseDataExchange CreateExchange(ConcurrentQueue<BaseData> dataQueue)
        {
            dataQueueHandler = new FuncDataQueueHandler(q =>
            {
                BaseData data;
                int count = 0;
                list = new List<BaseData>();
                while (++count < 10 && dataQueue.TryDequeue(out data)) list.Add(data);
                return list;
            });
            exchange = new BaseDataExchange("test");
            IEnumerator<BaseData> enumerator = GetNextTicksEnumerator(dataQueueHandler);
            sym = Symbol.Create("data-queue-handler-symbol", SecurityType.Base, Market.USA);
            exchange.AddEnumerator(sym, enumerator, null, null);
            return exchange;
        }

        private static IEnumerator<BaseData> GetNextTicksEnumerator(IDataQueueHandler dataQueueHandler)
        {
            while (true)
            {
                int ticks = 0;
                foreach (data in dataQueueHandler.GetNextTicks())
                {
                    ticks++;
                    yield return data;
                }
                if (ticks == 0) Thread.Sleep(1);
            }
        }
    }
}
