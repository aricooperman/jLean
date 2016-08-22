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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Lean.Engine.Setup;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Statistics;

package com.quantconnect.lean.Tests.Engine
{
    /// <summary>
    /// Provides a result handler implementation that handles result packets via
    /// a constructor defined function. Also, this implementation does not require
    /// the Run method to be called at all, a task is launched via ctor to process
    /// the packets
    /// </summary>
    public class TestResultHandler : IResultHandler
    {
        private AlgorithmNodePacket _job = new BacktestNodePacket();

        private readonly Action<Packet> _packetHandler;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public ConcurrentQueue<Packet> Messages { get; set; }
        public ConcurrentMap<String, Chart> Charts { get; set; }
        public TimeSpan ResamplePeriod { get; private set; }
        public TimeSpan NotificationPeriod { get; private set; }
        public boolean IsActive { get; private set; }

        public TestResultHandler(Action<Packet> packetHandler = null ) {
            _packetHandler = packetHandler ?? (packet => { });
            Messages = new ConcurrentQueue<Packet>();
            Task.Run(() =>
            {
                try
                {
                    IsActive = true;
                    while (!_cancellationTokenSource.IsCancellationRequested) {
                        Packet packet;
                        if( Messages.TryDequeue(out packet)) {
                            _packetHandler(packet);
                        }

                        Thread.Sleep(1);
                    }
                }
                finally
                {
                    IsActive = false;
                }
            });
        }

        public void Initialize(AlgorithmNodePacket job,
            IMessagingHandler messagingHandler,
            IApi api,
            IDataFeed dataFeed,
            ISetupHandler setupHandler,
            ITransactionHandler transactionHandler) {
            _job = job;
        }

        public void Run() {
        }

        public void DebugMessage( String message) {
            Messages.Enqueue(new DebugPacket(_job.ProjectId, _job.AlgorithmId, _job.CompileId, message));
        }

        public void SecurityType(List<SecurityType> types) {
        }

        public void LogMessage( String message) {
            Messages.Enqueue(new LogPacket(_job.AlgorithmId, message));
        }

        public void ErrorMessage( String error, String stacktrace = "") {
            Messages.Enqueue(new HandledErrorPacket(_job.AlgorithmId, error, stacktrace));
        }

        public void RuntimeError( String message, String stacktrace = "") {
            Messages.Enqueue(new RuntimeErrorPacket(_job.AlgorithmId, message, stacktrace));
        }

        public void Sample( String chartName, String seriesName, int seriesIndex, SeriesType seriesType, DateTime time, BigDecimal value, String unit = "$") {
            //Add a copy locally:
            if( !Charts.ContainsKey(chartName)) {
                Charts.AddOrUpdate(chartName, new Chart(chartName));
            }

            //Add the sample to our chart:
            if( !Charts[chartName].Series.ContainsKey(seriesName)) {
                Charts[chartName].Series.Add(seriesName, new Series(seriesName, seriesType, seriesIndex, unit));
            }

            //Add our value:
            Charts[chartName].Series[seriesName].Values.Add(new ChartPoint(time, value));
        }

        public void SampleEquity(DateTime time, BigDecimal value) {
            Sample( "Strategy Equity", "Equity", 0, SeriesType.Candle, time, value);
        }

        public void SamplePerformance(DateTime time, BigDecimal value) {
            Sample( "Strategy Equity", "Daily Performance", 1, SeriesType.Line, time, value, "%");
        }

        public void SampleBenchmark(DateTime time, BigDecimal value) {
            Sample( "Benchmark", "Benchmark", 0, SeriesType.Line, time, value);
        }

        public void SampleAssetPrices(Symbol symbol, DateTime time, BigDecimal value) {
            Sample( "Stockplot: " + symbol.Value, "Stockplot: " + symbol.Value, 0, SeriesType.Line, time, value);
        }

        public void SampleRange(List<Chart> updates) {
            foreach (update in updates) {
                //Create the chart if it doesn't exist already:
                if( !Charts.ContainsKey(update.Name)) {
                    Charts.AddOrUpdate(update.Name, new Chart(update.Name, update.ChartType));
                }

                //Add these samples to this chart.
                foreach (series in update.Series.Values) {
                    //If we don't already have this record, its the first packet
                    if( !Charts[update.Name].Series.ContainsKey(series.Name)) {
                        Charts[update.Name].Series.Add(series.Name, new Series(series.Name, series.SeriesType, series.Index, series.Unit));
                    }

                    //We already have this record, so just the new samples to the end:
                    Charts[update.Name].Series[series.Name].Values.AddRange(series.Values);
                }
            }
        }

        public void SetAlgorithm(IAlgorithm algorithm) {
        }

        public void StoreResult(Packet packet, boolean async = false) {
        }

        public void SendFinalResult(AlgorithmNodePacket job,
            Map<Integer, Order> orders,
            Map<DateTime, decimal> profitLoss,
            Map<String, Holding> holdings,
            StatisticsResults statisticsResults,
            Map<String,String> banner) {
        }

        public void SendStatusUpdate(AlgorithmStatus status, String message = "") {
        }

        public void SetChartSubscription( String symbol) {
        }

        public void RuntimeStatistic( String key, String value) {
        }

        public void OrderEvent(OrderEvent newEvent) {
        }

        public void Exit() {
            _cancellationTokenSource.Cancel();
        }

        public void PurgeQueue() {
            Messages.Clear();
        }

        public void ProcessSynchronousEvents( boolean forceProcess = false) {
        }
    }
}