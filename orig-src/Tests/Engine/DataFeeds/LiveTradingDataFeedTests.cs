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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Auxiliary;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Tests.Engine.DataFeeds
{
    [TestFixture, Ignore( "These tests depend on a remote server")]
    public class LiveTradingDataFeedTests
    {
        [Test]
        public void EmitsData() {
            algorithm = new AlgorithmStub(forex: new List<String> {"EURUSD"});

            // job is used to send into DataQueueHandler
            job = new LiveNodePacket();
            // result handler is used due to dependency in SubscriptionDataReader
            resultHandler = new BacktestingResultHandler();

            lastTime = DateTime.MinValue;
            timeProvider = new RealTimeProvider();
            dataQueueHandler = new FuncDataQueueHandler(fdqh =>
            {
                time = timeProvider.GetUtcNow() Extensions.convertFromUtc(TimeZones.EasternStandard);
                if( time == lastTime) return Enumerable.Empty<BaseData>();
                lastTime = time;
                 return Enumerable.Range(0, 9).Select(x -> new Tick(time.AddMilliseconds(x*100), Symbols.EURUSD, 1.3m, 1.2m, 1.3m));
            });

            feed = new TestableLiveTradingDataFeed(dataQueueHandler, timeProvider);
            mapFileProvider = new LocalDiskMapFileProvider();
            feed.Initialize(algorithm, job, resultHandler, mapFileProvider, new LocalDiskFactorFileProvider(mapFileProvider));

            feedThreadStarted = new ManualResetEvent(false);
            Task.Factory.StartNew(() =>
            {
                feedThreadStarted.Set();
                feed.Run();
            });

            // wait for feed.Run to actually begin
            feedThreadStarted.WaitOne();

            emittedData = false;
            ConsumeBridge(feed, Duration.ofSeconds(10), true, ts =>
            {
                if( ts.Slice.Count != 0) {
                    emittedData = true;
                    Console.WriteLine( "HasData: " + ts.Slice.Bars[Symbols.EURUSD].EndTime);
                    Console.WriteLine();
                }
            });

            Assert.IsTrue(emittedData);
        }

        [Test]
        public void HandlesMultipleSecurities() {
            algorithm = new AlgorithmStub(
                equities: new List<String> {"SPY", "IBM", "AAPL", "GOOG", "MSFT", "BAC", "GS"},
                forex: new List<String> {"EURUSD", "USDJPY", "GBPJPY", "AUDUSD", "NZDUSD"}
                );
            feed = RunDataFeed(algorithm);

            ConsumeBridge(feed, Duration.ofSeconds(5), ts =>
            {
                delta = (DateTime.UtcNow - ts.Time).TotalMilliseconds;
                Console.WriteLine(((decimal)delta).SmartRounding() + "ms : " + String.join( ",", ts.Slice.Keys.Select(x -> x.Value)));
            });
        }

        [Test]
        public void PerformanceBenchmark() {
            symbolCount = 600;
            algorithm = new AlgorithmStub(Resolution.Tick,
                equities: Enumerable.Range(0, symbolCount).Select(x -> "E"+x.toString()).ToList()
                );

            securitiesCount = algorithm.Securities.Count;
            expected = algorithm.Securities.Keys.ToHashSet();
            Console.WriteLine( "Securities.Count: " + securitiesCount);

            FuncDataQueueHandler queue;
            count = new Count();
            stopwatch = Stopwatch.StartNew();
            feed = RunDataFeed(algorithm, out queue, null, fdqh -> ProduceBenchmarkTicks(fdqh, count));
             
            ConsumeBridge(feed, Duration.ofSeconds(5), ts =>
            {
                Console.WriteLine( "Count: " + ts.Slice.Keys.Count + " " + DateTime.UtcNow.toString( "o"));
                if( ts.Slice.Keys.Count != securitiesCount) {
                    included = ts.Slice.Keys.ToHashSet();
                    expected.ExceptWith(included);
                    Console.WriteLine( "Missing: " + String.join( ",", expected.OrderBy(x -> x.Value)));
                }
            });
            stopwatch.Stop();

            Console.WriteLine( "Total ticks: " + count.Value);
            Console.WriteLine( "Elapsed time: " + stopwatch.Elapsed);
            Console.WriteLine( "Ticks/sec: " + (count.Value/stopwatch.Elapsed.TotalSeconds));
            Console.WriteLine( "Ticks/sec/symbol: " + (count.Value/stopwatch.Elapsed.TotalSeconds)/symbolCount);
        }

        class Count
        {
            public int Value;
        }

        private static IEnumerable<BaseData> ProduceBenchmarkTicks(FuncDataQueueHandler fdqh, Count count) {
            for (int i = 0; i < 10000; i++) {
                foreach (symbol in fdqh.Subscriptions) {
                    count.Value++;
                    yield return new Tick{Symbol = symbol};
                }
            }
        }

        [Test]
        public void DoesNotSubscribeToCustomData() {
            // Current implementation only sends equity/forex subscriptions to the queue handler,
            // new impl sends all, the restriction shouldn't live in the feed, but rather in the
            // queue handler impl

            algorithm = new AlgorithmStub(equities: new List<String> { "SPY" }, forex: new List<String> { "EURUSD" });
            algorithm.AddData<RemoteFileBaseData>( "RemoteFile");
            remoteFile = SymbolCache.GetSymbol( "RemoteFile");
            FuncDataQueueHandler dataQueueHandler;
            RunDataFeed(algorithm, out dataQueueHandler);

            Assert.IsTrue(dataQueueHandler.Subscriptions.Contains(Symbols.SPY));
            Assert.IsTrue(dataQueueHandler.Subscriptions.Contains(Symbols.EURUSD));
            Assert.IsFalse(dataQueueHandler.Subscriptions.Contains(remoteFile));
            Assert.AreEqual(2, dataQueueHandler.Subscriptions.Count);
        }

        [Test]
        public void Unsubscribes() {
            algorithm = new AlgorithmStub(equities: new List<String> { "SPY" }, forex: new List<String> { "EURUSD" });
            algorithm.AddData<RemoteFileBaseData>( "RemoteFile");
            remoteFile = SymbolCache.GetSymbol( "RemoteFile");
            FuncDataQueueHandler dataQueueHandler;
            feed = RunDataFeed(algorithm, out dataQueueHandler);

            feed.RemoveSubscription(feed.Subscriptions.Single(sub -> sub.Configuration.Symbol == Symbols.SPY).Configuration);

            Assert.AreEqual(1, dataQueueHandler.Subscriptions.Count);
            Assert.IsFalse(dataQueueHandler.Subscriptions.Contains(Symbols.SPY));
            Assert.IsFalse(dataQueueHandler.Subscriptions.Contains(remoteFile));
            Assert.IsTrue(dataQueueHandler.Subscriptions.Contains(Symbols.EURUSD));
        }

        [Test]
        public void HandlesAtLeast10kTicksPerSecondWithTwentySymbols() {
            // this ran at ~25k ticks/per symbol for 20 symbols
            algorithm = new AlgorithmStub(Resolution.Tick, Enumerable.Range(0, 20).Select(x -> x.toString()).ToList());
            t = Enumerable.Range(0, 20).Select(x -> new Tick {Symbol = SymbolCache.GetSymbol(x.toString())}).ToList();
            feed = RunDataFeed(algorithm, handler -> t);
            flag = false;
            int ticks = 0;
            averages = new List<decimal>();
            timer = new Timer(state =>
            {
                avg = ticks/20m;
                Interlocked.Exchange(ref ticks, 0);
                Console.WriteLine( "Average ticks per symbol: " + avg.SmartRounding());
                if( flag) flag = false;
                averages.Add(avg);
            }, null, Time.OneSecond, Time.OneSecond);
            ConsumeBridge(feed, Duration.ofSeconds(5), false, ts =>
            {
                Interlocked.Add(ref ticks, ts.Slice.Ticks.Sum(x -> x.Value.Count));
            }, true);


            average = averages.Average();
            Console.WriteLine( "\r\nAverage ticks per symbol per second: " + average);
            Assert.That(average, Is.GreaterThan(10000));
        }

        [Test]
        public void EmitsForexDataWithRoundedUtcTimes() {
            algorithm = new AlgorithmStub(forex: new List<String> { "EURUSD" });

            feed = RunDataFeed(algorithm);

            emittedData = false;
            lastTime = DateTime.UtcNow;
            ConsumeBridge(feed, Duration.ofSeconds(5), ts =>
            {
                if( !emittedData) {
                    emittedData = true;
                    lastTime = ts.Time;
                    return;
                }
                delta = (DateTime.UtcNow - ts.Time).TotalMilliseconds;
                Console.WriteLine(((decimal)delta).SmartRounding() + "ms : " + String.join( ", ", ts.Slice.Keys.Select(x -> x.Value + ": " + ts.Slice[x].Volume)));
                Assert.AreEqual(lastTime.Add(Time.OneSecond), ts.Time);
                Assert.AreEqual(1, ts.Slice.Bars.Count);
                lastTime = ts.Time;
            });

            Assert.IsTrue(emittedData);
        }

        [Test]
        public void HandlesManyCustomDataSubscriptions() {
            resolution = Resolution.Second;
            algorithm = new AlgorithmStub();
            for (int i = 0; i < 5; i++) {
                algorithm.AddData<RemoteFileBaseData>((100+ i).toString(), resolution, fillDataForward: false);
            }

            feed = RunDataFeed(algorithm);

            int count = 0;
            boolean receivedData = false;
            stopwatch = Stopwatch.StartNew();
            Console.WriteLine( "start: " + DateTime.UtcNow.toString( "o"));
            ConsumeBridge(feed, Duration.ofSeconds(5), ts =>
            {
                // because this is a remote file we may skip data points while the newest
                // version of the file is downloading [internet speed] and also we decide
                // not to emit old data

                stopwatch.Stop();
                if( ts.Slice.Count == 0) return;

                count++;
                receivedData = true;
                time = ts.Slice.Min(x -> x.Value.EndTime) Extensions.convertToUtc(TimeZones.NewYork);
                // make sure within 2 seconds
                delta = DateTime.UtcNow.Subtract(time);
                //Assert.IsTrue(delta <= Duration.ofSeconds(2), delta.toString());
                Console.WriteLine( "Count: " + ts.Slice.Count + "Data time: " + time Extensions.convertFromUtc(TimeZones.NewYork) + " Delta (ms): "
                    + ((decimal) delta.TotalMilliseconds).SmartRounding() + Environment.NewLine);
            });

            Console.WriteLine( "end: " + DateTime.UtcNow.toString( "o"));
            Console.WriteLine( "Spool up time: " + stopwatch.Elapsed);

            // even though we're doing 20 seconds, give a little
            // leeway for slow internet traffic
            //Assert.That(count, Is.GreaterThan(17));
            //Assert.IsTrue(receivedData);
        }

        [Test]
        public void HandlesRestApi() {
            resolution = Resolution.Second;
            algorithm = new AlgorithmStub();
            algorithm.AddData<RestApiBaseData>( "RestApi", resolution);
            symbol = SymbolCache.GetSymbol( "RestApi");
            FuncDataQueueHandler dqgh;
            timeProvider = new ManualTimeProvider(new DateTime(2015, 10, 10, 16, 36, 0));
            feed = RunDataFeed(algorithm, out dqgh, null );

            count = 0;
            receivedData = false;
            timeZone = algorithm.Securities[symbol].Exchange.TimeZone;
            RestApiBaseData last = null;

            timeout = new CancellationTokenSource(Duration.ofSeconds(5));
            foreach (ts in feed) {
                //timeProvider.AdvanceSeconds(0.5);

                if( !ts.Slice.ContainsKey(symbol)) return;

                count++;
                receivedData = true;
                data = (RestApiBaseData)ts.Slice[symbol];
                time = data.EndTime Extensions.convertToUtc(timeZone);
                Console.WriteLine(DateTime.UtcNow + ": Data time: " + time Extensions.convertFromUtc(TimeZones.NewYork) + Environment.NewLine);
                if( last != null ) {
                    Assert.AreEqual(last.EndTime, data.EndTime.Subtract(resolution.ToTimeSpan()));
                }
                last = data;
            }

            // even though we're doing 10 seconds, give a little
            // leeway for slow internet traffic
            Assert.That(count, Is.GreaterThanOrEqualTo(8));
            Assert.IsTrue(receivedData);
            Assert.That(RestApiBaseData.ReaderCount, Is.LessThanOrEqualTo(30)); // we poll at 10x frequency

            Console.WriteLine( "Count: " + count + " ReaderCount: " + RestApiBaseData.ReaderCount);
        }

        [Test]
        public void HandlesCoarseFundamentalData() {
            algorithm = new AlgorithmStub();
            Symbol symbol = CoarseFundamental.CreateUniverseSymbol(Market.USA);
            algorithm.AddUniverse(new FuncUniverse(
                new SubscriptionDataConfig(typeof(CoarseFundamental), symbol, Resolution.Daily, TimeZones.NewYork, TimeZones.NewYork, false, false, false),
                new UniverseSettings(Resolution.Second, 1, true, false, Duration.ZERO), SecurityInitializer.Null,
                coarse -> coarse.Take(10).Select(x -> x.Symbol) 
                ));

            lck = new object();
            BaseDataCollection list = null;
            static final int coarseDataPointCount = 100000;
            timer = new Timer(state =>
            {
                currentTime = DateTime.UtcNow Extensions.convertFromUtc(TimeZones.NewYork);
                Console.WriteLine(currentTime + ": timer.Elapsed");

                synchronized(state) {
                    list = new BaseDataCollection {Symbol = symbol};
                    list.Data.AddRange(Enumerable.Range(0, coarseDataPointCount).Select(x -> new CoarseFundamental
                    {
                        Symbol = SymbolCache.GetSymbol(x.toString()),
                        Time = currentTime - Duration.ofDays( 1 ), // hard-coded coarse period of one day
                    }));
                }
            }, lck, Duration.ofSeconds(3), Duration.ofSeconds(500));

            boolean yieldedUniverseData = false;
            feed = RunDataFeed(algorithm, fdqh =>
            {
                synchronized(lck) {
                    if( list != null )
                        try
                        {
                            tmp = list;
                            return new List<BaseData> { tmp };
                        }
                        finally
                        {
                            list = null;
                            yieldedUniverseData = true;
                        }
                }
                return Enumerable.Empty<BaseData>();
            });

            Assert.IsTrue(feed.Subscriptions.Any(x -> x.IsUniverseSelectionSubscription));

            universeSelectionHadAllData = false;


            ConsumeBridge(feed, Duration.ofSeconds(5), ts =>
            {
            });

            Assert.IsTrue(yieldedUniverseData);
            Assert.IsTrue(universeSelectionHadAllData);
        }



        private IDataFeed RunDataFeed(IAlgorithm algorithm, Func<FuncDataQueueHandler, IEnumerable<BaseData>> getNextTicksFunction = null ) {
            FuncDataQueueHandler dataQueueHandler;
            return RunDataFeed(algorithm, out dataQueueHandler, null, getNextTicksFunction);
        }

        private IDataFeed RunDataFeed(IAlgorithm algorithm, out FuncDataQueueHandler dataQueueHandler, ITimeProvider timeProvider = null, Func<FuncDataQueueHandler, IEnumerable<BaseData>> getNextTicksFunction = null ) {
            getNextTicksFunction = getNextTicksFunction ?? (fdqh -> fdqh.Subscriptions.Select(symbol -> new Tick(DateTime.Now, symbol, 1, 2){Quantity = 1}));

            // job is used to send into DataQueueHandler
            job = new LiveNodePacket();
            // result handler is used due to dependency in SubscriptionDataReader
            resultHandler = new BacktestingResultHandler(); // new ResultHandlerStub();

            dataQueueHandler = new FuncDataQueueHandler(getNextTicksFunction);

            feed = new TestableLiveTradingDataFeed(dataQueueHandler, timeProvider);
            mapFileProvider = new LocalDiskMapFileProvider();
            feed.Initialize(algorithm, job, resultHandler, mapFileProvider, new LocalDiskFactorFileProvider(mapFileProvider));

            feedThreadStarted = new ManualResetEvent(false);
            Task.Factory.StartNew(() =>
            {
                feedThreadStarted.Set();
                feed.Run();
            });

            // wait for feed.Run to actually begin
            feedThreadStarted.WaitOne();

            return feed;
        }

        private static void ConsumeBridge(IDataFeed feed, Action<TimeSlice> handler) {
            ConsumeBridge(feed, Duration.ofSeconds(10), handler);
        }

        private static void ConsumeBridge(IDataFeed feed, Duration timeout, Action<TimeSlice> handler) {
            ConsumeBridge(feed, timeout, false, handler);
        }

        private static void ConsumeBridge(IDataFeed feed, Duration timeout, boolean alwaysInvoke, Action<TimeSlice> handler, boolean noOutput = false) {
            Task.Delay(timeout).ContinueWith(_ -> feed.Exit());
            boolean startedReceivingata = false;
            foreach (timeSlice in feed) {
                if( !noOutput) {
                    Console.WriteLine( "\r\n" + "Now (EDT): %1$s TimeSlice.Time (EDT): %2$s",
                        DateTime.UtcNow Extensions.convertFromUtc(TimeZones.NewYork).toString( "o"),
                        timeSlice.Time Extensions.convertFromUtc(TimeZones.NewYork).toString( "o")
                        );
                }

                if( !startedReceivingata && timeSlice.Slice.Count != 0) {
                    startedReceivingata = true;
                }
                if( startedReceivingata || alwaysInvoke) {
                    handler(timeSlice);
                }
            }
        }

    }

    public class TestableLiveTradingDataFeed : LiveTradingDataFeed
    {
        private final ITimeProvider _timeProvider;
        private final IDataQueueHandler _dataQueueHandler;

        public TestableLiveTradingDataFeed(IDataQueueHandler dataQueueHandler, ITimeProvider timeProvider = null ) {
            _dataQueueHandler = dataQueueHandler;
            _timeProvider = timeProvider ?? new RealTimeProvider();
        }

        protected @Override IDataQueueHandler GetDataQueueHandler() {
            return _dataQueueHandler;
        }

        protected @Override ITimeProvider GetTimeProvider() {
            return _timeProvider;
        }
    }
}
