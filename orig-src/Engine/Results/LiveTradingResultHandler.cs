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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.Setup;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Logging;
using QuantConnect.Notifications;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Statistics;
using QuantConnect.Util;
using QuantConnect.Securities.Forex;

package com.quantconnect.lean.Lean.Engine.Results
{
    /**
    /// Live trading result handler implementation passes the messages to the QC live trading interface.
    */
    /// Live trading result handler is quite busy. It sends constant price updates, equity updates and order/holdings updates.
    public class LiveTradingResultHandler : IResultHandler
    {
        private final DateTime _launchTimeUtc = DateTime.UtcNow;

        // Required properties for the cloud app.
        private boolean _isActive;
        private String _compileId;
        private String _deployId;
        private LiveNodePacket _job;
        private ConcurrentMap<String, Chart> _charts;
        private ConcurrentQueue<OrderEvent> _orderEvents; 
        private ConcurrentQueue<Packet> _messages;
        private IAlgorithm _algorithm;
        private volatile boolean _exitTriggered;
        private final DateTime _startTime;
        private final Map<String,String> _runtimeStatistics = new Map<String,String>();

        //Sampling Periods:
        private final Duration _resamplePeriod;
        private final Duration _notificationPeriod;

        //Update loop:
        private DateTime _nextUpdate;
        private DateTime _nextChartsUpdate;
        private DateTime _nextRunningStatus;
        private DateTime _nextLogStoreUpdate;
        private DateTime _nextStatisticsUpdate;
        private int _lastOrderId = 0;
        private final object _chartLock = new Object();
        private final object _runtimeLock = new Object();
        private String _subscription = "Strategy Equity";

        //Log Message Store:
        private final object _logStoreLock = new object();
        private List<LogEntry> _logStore;
        private DateTime _nextSample;
        private IMessagingHandler _messagingHandler;
        private IApi _api;
        private IDataFeed _dataFeed;
        private ISetupHandler _setupHandler;
        private ITransactionHandler _transactionHandler;

        /**
        /// Live packet messaging queue. Queue the messages here and send when the result queue is ready.
        */
        public ConcurrentQueue<Packet> Messages
        {
            get
            {
                return _messages;
            }
            set
            {
                _messages = value;
            }
        }

        /**
        /// Storage for the price and equity charts of the live results.
        */
        /// 
        ///     Potential memory leak when the algorithm has been running for a long time. Infinitely storing the results isn't wise.
        ///     The results should be stored to disk daily, and then the caches reset.
        /// 
        public ConcurrentMap<String, Chart> Charts
        {
            get
            {
                return _charts;
            }
            set
            {
                _charts = value;
            }
        }

        /**
        /// Boolean flag indicating the thread is still active.
        */
        public boolean IsActive
        {
            get
            {
                return _isActive;
            }
        }

        /**
        /// Equity resampling period for the charting.
        */
        /// Live trading can resample at much higher frequencies (every 1-2 seconds)
        public Duration ResamplePeriod
        {
            get
            {
                return _resamplePeriod;
            }
        }

        /**
        /// Notification periods set how frequently we push updates to the browser.
        */
        /// Live trading resamples - sends updates at high frequencies(every 1-2 seconds)
        public Duration NotificationPeriod
        {
            get
            {
                return _notificationPeriod;
            }
        }

        /**
        /// Initialize the live trading result handler
        */
        public LiveTradingResultHandler() {
            _charts = new ConcurrentMap<String, Chart>();
            _orderEvents = new ConcurrentQueue<OrderEvent>();
            _messages = new ConcurrentQueue<Packet>();
            _isActive = true;
            _runtimeStatistics = new Map<String,String>();

            _resamplePeriod = Duration.ofSeconds(1);
            _notificationPeriod = Duration.ofSeconds(1);
            _startTime = DateTime.Now;

            //Store log and debug messages sorted by time.
            _logStore = new List<LogEntry>();
        }

        /**
        /// Initialize the result handler with this result packet.
        */
         * @param job">Algorithm job packet for this result handler
         * @param messagingHandler">
         * @param api">
         * @param dataFeed">
         * @param setupHandler">
         * @param transactionHandler">
        public void Initialize(AlgorithmNodePacket job, IMessagingHandler messagingHandler, IApi api, IDataFeed dataFeed, ISetupHandler setupHandler, ITransactionHandler transactionHandler) {
            _api = api;
            _dataFeed = dataFeed;
            _messagingHandler = messagingHandler;
            _setupHandler = setupHandler;
            _transactionHandler = transactionHandler;
            _job = (LiveNodePacket)job;
            if( _job == null ) throw new Exception( "LiveResultHandler.Constructor(): Submitted Job type invalid."); 
            _deployId = _job.DeployId;
            _compileId = _job.CompileId;
        }
        
        /**
        /// Live trading result handler thread.
        */
        public void Run() {
            // -> 1. Run Primary Sender Loop: Continually process messages from queue as soon as they arrive.
            while (!(_exitTriggered && Messages.Count == 0)) {
                try
                {
                    //1. Process Simple Messages in Queue
                    Packet packet;
                    if( Messages.TryDequeue(out packet)) {
                        _messagingHandler.Send(packet);
                    }

                    //2. Update the packet scanner:
                    Update();

                    if( Messages.Count == 0) {
                        // prevent thread lock/tight loop when there's no work to be done
                        Thread.Sleep(10);
                    }
                }
                catch (Exception err) {
                    Log.Error(err);
                }
            } // While !End.

            Log.Trace( "LiveTradingResultHandler.Run(): Ending Thread...");
            _isActive = false;
        } // End Run();


        /**
        /// Every so often send an update to the browser with the current state of the algorithm.
        */
        public void Update() {
            //Initialize:
            Map<Integer, Order> deltaOrders;

            //Error checks if the algorithm & threads have not loaded yet, or are closing down.
            if( _algorithm == null || _algorithm.Transactions == null || _transactionHandler.Orders == null || !_algorithm.GetLocked()) {
                Log.Error( "LiveTradingResultHandler.Update(): Algorithm not yet initialized.");
                return;
            }

            try
            {
                if( DateTime.Now > _nextUpdate || _exitTriggered) {
                    //Extract the orders created since last update
                    OrderEvent orderEvent;
                    deltaOrders = new Map<Integer, Order>();

                    stopwatch = Stopwatch.StartNew();
                    while (_orderEvents.TryDequeue(out orderEvent) && stopwatch.ElapsedMilliseconds < 15) {
                        order = _algorithm.Transactions.GetOrderById(orderEvent.OrderId);
                        deltaOrders[orderEvent.OrderId] = order.Clone();
                    }

                    //For charting convert to UTC
                    foreach (order in deltaOrders) {
                        order.Value.Price = order.Value.Price.SmartRounding();
                        order.Value.Time = order.Value.Time.ToUniversalTime();
                    }

                    //Reset loop variables:
                    _lastOrderId = (from order in deltaOrders.Values select order.Id).DefaultIfEmpty(_lastOrderId).Max();

                    //Limit length of orders we pass back dynamically to avoid flooding.
                    //if( deltaOrders.Count > 50) deltaOrders.Clear();

                    //Create and send back the changes in chart since the algorithm started.
                    deltaCharts = new Map<String, Chart>();
                    Log.Debug( "LiveTradingResultHandler.Update(): Build delta charts");
                    lock (_chartLock) {
                        //Get the updates since the last chart
                        foreach (chart in _charts) {
                            // remove directory pathing characters from chart names
                            safeName = chart.Value.Name.Replace('/', '-');
                            deltaCharts.Add(safeName, chart.Value.GetUpdates());
                        }
                    }
                    Log.Debug( "LiveTradingResultHandler.Update(): End build delta charts");

                    //Profit loss changes, get the banner statistics, summary information on the performance for the headers.
                    holdings = new Map<String, Holding>();
                    deltaStatistics = new Map<String,String>();
                    runtimeStatistics = new Map<String,String>();
                    serverStatistics = OS.GetServerStatistics();
                    upTime = DateTime.UtcNow - _launchTimeUtc;
                    serverStatistics["Up Time"] = String.format( "%1$sd {1:hh\\:mm\\:ss}", upTime.Days, upTime);

                    // Only send holdings updates when we have changes in orders, except for first time, then we want to send all
                    foreach (asset in _algorithm.Securities.Values.Where(x -> !x.IsInternalFeed()).OrderBy(x -> x.Symbol.Value)) {
                        holdings.Add(asset.Symbol.Value, new Holding(asset));
                    }

                    //Add the algorithm statistics first.
                    Log.Debug( "LiveTradingResultHandler.Update(): Build run time stats");
                    lock (_runtimeLock) {
                        foreach (pair in _runtimeStatistics) {
                            runtimeStatistics.Add(pair.Key, pair.Value);
                        }
                    }
                    Log.Debug( "LiveTradingResultHandler.Update(): End build run time stats");

                    //Some users have $0 in their brokerage account / starting cash of $0. Prevent divide by zero errors
                    netReturn = _setupHandler.StartingPortfolioValue > 0 ?
                                    (_algorithm.Portfolio.TotalPortfolioValue - _setupHandler.StartingPortfolioValue) / _setupHandler.StartingPortfolioValue
                                    : 0;

                    //Add other fixed parameters.
                    runtimeStatistics.Add( "Unrealized:", "$" + _algorithm.Portfolio.TotalUnrealizedProfit.toString( "N2"));
                    runtimeStatistics.Add( "Fees:", "-$" + _algorithm.Portfolio.TotalFees.toString( "N2"));
                    runtimeStatistics.Add( "Net Profit:", "$" + _algorithm.Portfolio.TotalProfit.toString( "N2"));
                    runtimeStatistics.Add( "Return:", netReturn.toString( "P"));
                    runtimeStatistics.Add( "Equity:", "$" + _algorithm.Portfolio.TotalPortfolioValue.toString( "N2"));
                    runtimeStatistics.Add( "Holdings:", "$" + _algorithm.Portfolio.TotalHoldingsValue.toString( "N2"));
                    runtimeStatistics.Add( "Volume:", "$" + _algorithm.Portfolio.TotalSaleVolume.toString( "N2"));

                    // since we're sending multiple packets, let's do it async and forget about it
                    // chart data can get big so let's break them up into groups
                    splitPackets = SplitPackets(deltaCharts, deltaOrders, holdings, deltaStatistics, runtimeStatistics, serverStatistics);

                    foreach (liveResultPacket in splitPackets) {
                        _messagingHandler.Send(liveResultPacket);
                    }

                    //Send full packet to storage.
                    if( DateTime.Now > _nextChartsUpdate || _exitTriggered) {
                        Log.Debug( "LiveTradingResultHandler.Update(): Pre-store result");
                        _nextChartsUpdate = DateTime.Now.AddMinutes(1);
                        chartComplete = new Map<String, Chart>();
                        lock (_chartLock) {
                            foreach (chart in Charts) {
                                // remove directory pathing characters from chart names
                                safeName = chart.Value.Name.Replace('/', '-');
                                chartComplete.Add(safeName, chart.Value);
                            }
                        }
                        orders = new Map<Integer, Order>(_transactionHandler.Orders);
                        complete = new LiveResultPacket(_job, new LiveResult(chartComplete, orders, _algorithm.Transactions.TransactionRecord, holdings, deltaStatistics, runtimeStatistics, serverStatistics));
                        StoreResult(complete);
                        Log.Debug( "LiveTradingResultHandler.Update(): End-store result");
                    }

                    // Upload the logs every 1-2 minutes; this can be a heavy operation depending on amount of live logging and should probably be done asynchronously.
                    if( DateTime.Now > _nextLogStoreUpdate || _exitTriggered) {
                        List<LogEntry> logs;
                        Log.Debug( "LiveTradingResultHandler.Update(): Storing log...");
                        lock (_logStoreLock) {
                            utc = DateTime.UtcNow;
                            logs = (from log in _logStore
                                    where log.Time >= utc.RoundDown(Duration.ofHours(1))
                                    select log).ToList();
                            //Override the log master to delete the old entries and prevent memory creep.
                            _logStore = logs;
                        }
                        StoreLog(logs);
                        _nextLogStoreUpdate = DateTime.Now.AddMinutes(2);
                        Log.Debug( "LiveTradingResultHandler.Update(): Finished storing log");
                    }

                    // Every minute send usage statistics:
                    if( DateTime.Now > _nextStatisticsUpdate || _exitTriggered) {
                        try
                        {
                            _api.SendStatistics(
                                _job.AlgorithmId, 
                                _algorithm.Portfolio.TotalUnrealizedProfit,
                                _algorithm.Portfolio.TotalFees, 
                                _algorithm.Portfolio.TotalProfit,
                                _algorithm.Portfolio.TotalHoldingsValue, 
                                _algorithm.Portfolio.TotalPortfolioValue,
                                netReturn,
                                _algorithm.Portfolio.TotalSaleVolume, 
                                _lastOrderId, 0);
                        }
                        catch (Exception err) {
                            Log.Error(err, "Error sending statistics:");
                        }
                        _nextStatisticsUpdate = DateTime.Now.AddMinutes(1);
                    }


                    Log.Debug( "LiveTradingResultHandler.Update(): Trimming charts");
                    lock (_chartLock) {
                        foreach (chart in Charts) {
                            foreach (series in chart.Value.Series) {
                                // trim data that's older than 2 days
                                series.Value.Values =
                                    (from v in series.Value.Values
                                     where v.x > Time.DateTimeToUnixTimeStamp(DateTime.UtcNow.AddDays(-2))
                                     select v).ToList();
                            }
                        }
                    }
                    Log.Debug( "LiveTradingResultHandler.Update(): Finished trimming charts");


                    //Set the new update time after we've finished processing. 
                    // The processing can takes time depending on how large the packets are.
                    _nextUpdate = DateTime.Now.AddSeconds(2);

                } // End Update Charts:
            }
            catch (Exception err) {
                Log.Error(err, "LiveTradingResultHandler().Update(): ", true);
            }
        }



        /**
        /// Run over all the data and break it into smaller packets to ensure they all arrive at the terminal
        */
        private IEnumerable<LiveResultPacket> SplitPackets(Map<String, Chart> deltaCharts,
            Map<Integer, Order> deltaOrders,
            Map<String, Holding> holdings,
            Map<String,String> deltaStatistics,
            Map<String,String> runtimeStatistics,
            Map<String,String> serverStatistics) {
            // break the charts into groups

            static final int groupSize = 10;
            Map<String, Chart> current = new Map<String, Chart>();
            chartPackets = new List<LiveResultPacket>();

            // we only want to send data for the chart the user is subscribed to, but
            // we still want to let consumers know that these other charts still exists
            foreach (chart in deltaCharts.Values) {
                if( chart.Name != _subscription) {
                    current.Add(chart.Name, new Chart(chart.Name));
                }
            }

            chartPackets.Add(new LiveResultPacket(_job, new LiveResult { Charts = current }));

            // add in our subscription symbol
            Chart subscriptionChart;
            if( _subscription != null && deltaCharts.TryGetValue(_subscription, out subscriptionChart)) {
                scharts = new Map<String,Chart>();
                scharts.Add(_subscription, subscriptionChart);
                chartPackets.Add(new LiveResultPacket(_job, new LiveResult { Charts = scharts }));
            }

            // these are easier to split up, not as big as the chart objects
            packets = new[]
            {
                new LiveResultPacket(_job, new LiveResult {Orders = deltaOrders}),
                new LiveResultPacket(_job, new LiveResult {Holdings = holdings}),
                new LiveResultPacket(_job, new LiveResult
                {
                    Statistics = deltaStatistics,
                    RuntimeStatistics = runtimeStatistics,
                    ServerStatistics = serverStatistics
                })
            };

            // combine all the packets to be sent to through pubnub
            return packets.Concat(chartPackets);
        }


        /**
        /// Send a live trading debug message to the live console.
        */
         * @param message">Message we'd like shown in console.
        /// When there are already 500 messages in the queue it stops adding new messages.
        public void DebugMessage( String message) {
            if( Messages.Count > 500) return; //if too many in the queue already skip the logging.
            Messages.Enqueue(new DebugPacket(_job.ProjectId, _deployId, _compileId, message));
            AddToLogStore(message);
        }

        /**
        /// Log String messages and send them to the console.
        */
         * @param message">String message wed like logged.
        /// When there are already 500 messages in the queue it stops adding new messages.
        public void LogMessage( String message) {
            //Send the logging messages out immediately for live trading:
            if( Messages.Count > 500) return;
            Messages.Enqueue(new LogPacket(_deployId, message));
            AddToLogStore(message);
        }

        /**
        /// Save an algorithm message to the log store. Uses a different timestamped method of adding messaging to interweve debug and logging messages.
        */
         * @param message">String message to send to browser.
        private void AddToLogStore( String message) {
            Log.Debug( "LiveTradingResultHandler.AddToLogStore(): Adding");
            lock (_logStoreLock) {
                _logStore.Add(new LogEntry(DateTime.Now.toString(DateFormat.UI) + " " + message));
            }
            Log.Debug( "LiveTradingResultHandler.AddToLogStore(): Finished adding");
        }

        /**
        /// Send an error message back to the browser console and highlight it read.
        */
         * @param message">Message we'd like shown in console.
         * @param stacktrace">Stacktrace to show in the console.
        public void ErrorMessage( String message, String stacktrace = "") {
            if( Messages.Count > 500) return;
            Messages.Enqueue(new HandledErrorPacket(_deployId, message, stacktrace));
            AddToLogStore(message + (!StringUtils.isEmpty(stacktrace) ? ": StackTrace: " + stacktrace : string.Empty));
        }

        /**
        /// Send a list of secutity types that the algorithm trades to the browser to show the market clock - is this market open or closed!
        */
         * @param types">List of security types
        public void SecurityType(List<SecurityType> types) {
            packet = new SecurityTypesPacket { Types = types };
            Messages.Enqueue(packet);
        }

        /**
        /// Send a runtime error back to the users browser and highlight it red.
        */
         * @param message">Runtime error message
         * @param stacktrace">Associated error stack trace.
        public void RuntimeError( String message, String stacktrace = "") {
            Messages.Enqueue(new RuntimeErrorPacket(_deployId, message, stacktrace));
            AddToLogStore(message + (!StringUtils.isEmpty(stacktrace) ? ": StackTrace: " + stacktrace : string.Empty));
        }

        /**
        /// Add a sample to the chart specified by the chartName, and seriesName.
        */
         * @param chartName">String chart name to place the sample.
         * @param chartType">Type of chart we should create if it doesn't already exist.
         * @param seriesName">Series name for the chart.
         * @param seriesType">Series type for the chart.
         * @param time">Time for the sample
         * @param value">Value for the chart sample.
         * @param unit">Unit for the chart axis
        /// Sample can be used to create new charts or sample equity - daily performance.
        public void Sample( String chartName, String seriesName, int seriesIndex, SeriesType seriesType, DateTime time, BigDecimal value, String unit = "$") {
            Log.Debug( "LiveTradingResultHandler.Sample(): Sampling " + chartName + "." + seriesName);
            lock (_chartLock) {
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
            Log.Debug( "LiveTradingResultHandler.Sample(): Done sampling " + chartName + "." + seriesName);
        }

        /**
        /// Wrapper methond on sample to create the equity chart.
        */
         * @param time">Time of the sample.
         * @param value">Equity value at this moment in time.
        /// <seealso cref="Sample( String,string,int,SeriesType,DateTime,decimal,string)"/>
        public void SampleEquity(DateTime time, BigDecimal value) {
            if( value > 0) {
                Log.Debug( "LiveTradingResultHandler.SampleEquity(): " + time.ToShortTimeString() + " >" + value);
                Sample( "Strategy Equity", "Equity", 0, SeriesType.Candle, time, value);
            }
        }

        /**
        /// Sample the asset prices to generate plots.
        */
         * @param symbol">Symbol we're sampling.
         * @param time">Time of sample
         * @param value">Value of the asset price
        /// <seealso cref="Sample( String,string,int,SeriesType,DateTime,decimal,string)"/>
        public void SampleAssetPrices(Symbol symbol, DateTime time, BigDecimal value) {
            // don't send stockplots for internal feeds
            Security security;
            if( _algorithm.Securities.TryGetValue(symbol, out security) && !security.IsInternalFeed() && value > 0) {
                now = DateTime.UtcNow.ConvertFromUtc(security.Exchange.TimeZone);
                if( security.Exchange.Hours.IsOpen(now, security.IsExtendedMarketHours)) {
                    Sample( "Stockplot: " + symbol.Value, "Stockplot: " + symbol.Value, 0, SeriesType.Line, time, value);
                }
            }
        }

        /**
        /// Sample the current daily performance directly with a time-value pair.
        */
         * @param time">Current backtest date.
         * @param value">Current daily performance value.
        /// <seealso cref="Sample( String,string,int,SeriesType,DateTime,decimal,string)"/>
        public void SamplePerformance(DateTime time, BigDecimal value) {
            //No "daily performance" sampling for live trading yet.
            //Log.Debug( "LiveTradingResultHandler.SamplePerformance(): " + time.ToShortTimeString() + " >" + value);
            //Sample( "Strategy Equity", ChartType.Overlay, "Daily Performance", SeriesType.Line, time, value, "%");
        }

        /**
        /// Sample the current benchmark performance directly with a time-value pair.
        */
         * @param time">Current backtest date.
         * @param value">Current benchmark value.
        /// <seealso cref="IResultHandler.Sample"/>
        public void SampleBenchmark(DateTime time, BigDecimal value) {
            Sample( "Benchmark", "Benchmark", 0, SeriesType.Line, time, value);
        }

        /**
        /// Add a range of samples from the users algorithms to the end of our current list.
        */
         * @param updates">Chart updates since the last request.
        /// <seealso cref="Sample( String,string,int,SeriesType,DateTime,decimal,string)"/>
        public void SampleRange(List<Chart> updates) {
            Log.Debug( "LiveTradingResultHandler.SampleRange(): Begin sampling");
            lock (_chartLock) {
                foreach (update in updates) {
                    //Create the chart if it doesn't exist already:
                    if( !Charts.ContainsKey(update.Name)) {
                        Charts.AddOrUpdate(update.Name, new Chart(update.Name));
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
            Log.Debug( "LiveTradingResultHandler.SampleRange(): Finished sampling");
        }

        /**
        /// Set the algorithm of the result handler after its been initialized.
        */
         * @param algorithm">Algorithm object matching IAlgorithm interface
        public void SetAlgorithm(IAlgorithm algorithm) {
            _algorithm = algorithm;

            types = new List<SecurityType>();
            foreach (security in _algorithm.Securities.Values) {
                if( !types.Contains(security.Type)) types.Add(security.Type);
            }
            SecurityType(types);

            // we need to forward Console.Write messages to the algorithm's Debug function
            debug = new FuncTextWriter(algorithm.Debug);
            error = new FuncTextWriter(algorithm.Error);
            Console.SetOut(debug);
            Console.SetError(error);
        }


        /**
        /// Send a algorithm status update to the user of the algorithms running state.
        */
         * @param status">Status enum of the algorithm.
         * @param message">Optional String message describing reason for status change.
        public void SendStatusUpdate(AlgorithmStatus status, String message = "") {
            msg = status + ( String.IsNullOrEmpty(message) ? string.Empty : message);
            Log.Trace( "LiveTradingResultHandler.SendStatusUpdate(): " + msg);
            packet = new AlgorithmStatusPacket(_job.AlgorithmId, _job.ProjectId, status, message);
            Messages.Enqueue(packet);
        }


        /**
        /// Set a dynamic runtime statistic to show in the (live) algorithm header
        */
         * @param key">Runtime headline statistic name
         * @param value">Runtime headline statistic value
        public void RuntimeStatistic( String key, String value) {
            Log.Debug( "LiveTradingResultHandler.RuntimeStatistic(): Begin setting statistic");
            lock (_runtimeLock) {
                if( !_runtimeStatistics.ContainsKey(key)) {
                    _runtimeStatistics.Add(key, value);
                }
                _runtimeStatistics[key] = value;
            }
            Log.Debug( "LiveTradingResultHandler.RuntimeStatistic(): End setting statistic");
        }

        /**
        /// Send a final analysis result back to the IDE.
        */
         * @param job">Lean AlgorithmJob task
         * @param orders">Collection of orders from the algorithm
         * @param profitLoss">Collection of time-profit values for the algorithm
         * @param holdings">Current holdings state for the algorithm
         * @param statisticsResults">Statistics information for the algorithm (empty if not finished)
         * @param runtime">Runtime statistics banner information
        public void SendFinalResult(AlgorithmNodePacket job, Map<Integer, Order> orders, Map<DateTime, decimal> profitLoss, Map<String, Holding> holdings, StatisticsResults statisticsResults, Map<String,String> runtime) {
            try
            {
                //Convert local dictionary:
                charts = new Map<String, Chart>(Charts);

                //Create a packet:
                result = new LiveResultPacket((LiveNodePacket)job, new LiveResult(charts, orders, profitLoss, holdings, statisticsResults.Summary, runtime));

                //Save the processing time:
                result.ProcessingTime = (DateTime.Now - _startTime).TotalSeconds;

                //Store to S3:
                StoreResult(result, false);

                //Truncate packet to fit within 32kb:
                result.Results = new LiveResult();

                //Send the truncated packet:
                _messagingHandler.Send(result);
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }


        /**
        /// Process the log entries and save it to permanent storage 
        */
         * @param logs">Log list
        public void StoreLog(IEnumerable<LogEntry> logs) {
            try
            {
                //Concatenate and upload the log file:
                joined = String.join( "\r\n", logs.Select(x=>x.Message));
                key = "live/" + _job.UserId + "/" + _job.ProjectId + "/" + _job.DeployId + "-" + DateTime.UtcNow.toString( "yyyy-MM-dd-HH") + "-log.txt";
                _api.Store(joined, key, StoragePermissions.Authenticated);
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
        /// Save the snapshot of the total results to storage.
        */
         * @param packet">Packet to store.
         * @param async">Store the packet asyncronously to speed up the thread.
        /// 
        ///     Async creates crashes in Mono 3.10 if the thread disappears before the upload is complete so it is disabled for now.
        ///     For live trading we're making assumption its a long running task and safe to async save large files.
        /// 
        public void StoreResult(Packet packet, boolean async = true) {
            // this will hold all the serialized data and the keys to be stored
            data_keys = Enumerable.Range(0, 0).Select(x -> new
            {
                Key = ( String)null,
                Serialized = ( String)null
            }).ToList();

            try
            {
                Log.Debug( "LiveTradingResultHandler.StoreResult(): Begin store result sampling");
                lock (_chartLock) {
                    // Make sure this is the right type of packet:
                    if( packet.Type != PacketType.LiveResult) return;

                    // Port to packet format:
                    live = packet as LiveResultPacket;

                    if( live != null ) {
                        // we need to down sample
                        start = DateTime.UtcNow.Date;
                        stop = start.AddDays(1);

                        // truncate to just today, we don't need more than this for anyone
                        Truncate(live.Results, start, stop);

                        highResolutionCharts = new Map<String, Chart>(live.Results.Charts);

                        // minute resoluton data, save today
                        minuteSampler = new SeriesSampler(Duration.ofMinutes(1));
                        minuteCharts = minuteSampler.SampleCharts(live.Results.Charts, start, stop);

                        // swap out our charts with the sampeld data
                        live.Results.Charts = minuteCharts;
                        data_keys.Add(new
                        {
                            Key = CreateKey( "minute"),
                            Serialized = JsonConvert.SerializeObject(live.Results)
                        });

                        // 10 minute resolution data, save today
                        tenminuteSampler = new SeriesSampler(Duration.ofMinutes(10));
                        tenminuteCharts = tenminuteSampler.SampleCharts(live.Results.Charts, start, stop);

                        live.Results.Charts = tenminuteCharts;
                        data_keys.Add(new
                        {
                            Key = CreateKey( "10minute"),
                            Serialized = JsonConvert.SerializeObject(live.Results)
                        });

                        // high resolution data, we only want to save an hour
                        live.Results.Charts = highResolutionCharts;
                        start = DateTime.UtcNow.RoundDown(Duration.ofHours(1));
                        stop = DateTime.UtcNow.RoundUp(Duration.ofHours(1));

                        Truncate(live.Results, start, stop);

                        foreach (name in live.Results.Charts.Keys) {
                            newPacket = new LiveResult();
                            newPacket.Orders = new Map<Integer, Order>(live.Results.Orders);
                            newPacket.Holdings = new Map<String, Holding>(live.Results.Holdings);
                            newPacket.Charts = new Map<String, Chart>();
                            newPacket.Charts.Add(name, live.Results.Charts[name]);

                            data_keys.Add(new
                            {
                                Key = CreateKey( "second_" + Uri.EscapeUriString(name), "yyyy-MM-dd-HH"),
                                Serialized = JsonConvert.SerializeObject(newPacket)
                            });
                        }
                    }
                    else
                    {
                        Log.Error( "LiveResultHandler.StoreResult(): Result Null.");
                    }
                }
                Log.Debug( "LiveTradingResultHandler.StoreResult(): End store result sampling");

                // Upload Results Portion
                foreach (dataKey in data_keys) {
                    _api.Store(dataKey.Serialized, dataKey.Key, StoragePermissions.Authenticated, async);
                }
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
        /// New order event for the algorithm backtest: send event to browser.
        */
         * @param newEvent">New event details
        public void OrderEvent(OrderEvent newEvent) {
            // we'll pull these out for the deltaOrders
            _orderEvents.Enqueue(newEvent);

            //Send the message to frontend as packet:
            Log.Trace( "LiveTradingResultHandler.OrderEvent(): " + newEvent, true);
            Messages.Enqueue(new OrderEventPacket(_deployId, newEvent));

            DebugMessage( String.format( "New Order Event: OrderId:%1$s Symbol:%2$s Quantity:%3$s Status:{3}", newEvent.OrderId, newEvent.Symbol, newEvent.FillQuantity, newEvent.Status));

            //Add the order event message to the log:
            LogMessage( "New Order Event: Id:" + newEvent.OrderId + " Symbol:" + newEvent.Symbol.toString() + " Quantity:" + newEvent.FillQuantity + " Status:" + newEvent.Status);
        }

        /**
        /// Terminate the result thread and apply any required exit proceedures.
        */
        public void Exit() {
            _exitTriggered = true;
            Update();
        }

        /**
        /// Purge/clear any outstanding messages in message queue.
        */
        public void PurgeQueue() {
            Messages.Clear();
        }

        /**
        /// Truncates the chart and order data in the result packet to within the specified time frame
        */
        private static void Truncate(LiveResult result, DateTime start, DateTime stop) {
            unixDateStart = Time.DateTimeToUnixTimeStamp(start);
            unixDateStop = Time.DateTimeToUnixTimeStamp(stop);

            //Log.Trace( "LiveTradingResultHandler.Truncate: Start: " + start.toString( "u") + " Stop : " + stop.toString( "u"));
            //Log.Trace( "LiveTradingResultHandler.Truncate: Truncate Delta: " + (unixDateStop - unixDateStart) + " Incoming Points: " + result.Charts["Strategy Equity"].Series["Equity"].Values.Count);

            charts = new Map<String, Chart>();
            foreach (kvp in result.Charts) {
                chart = kvp.Value;
                newChart = new Chart(chart.Name, chart.ChartType);
                charts.Add(kvp.Key, newChart);
                foreach (series in chart.Series.Values) {
                    newSeries = new Series(series.Name, series.SeriesType);
                    newSeries.Values.AddRange(series.Values.Where(chartPoint -> chartPoint.x >= unixDateStart && chartPoint.x <= unixDateStop));
                    newChart.AddSeries(newSeries);
                }
            }
            result.Charts = charts;
            result.Orders = result.Orders.Values.Where(x -> x.Time >= start && x.Time <= stop).ToDictionary(x -> x.Id);

            //Log.Trace( "LiveTradingResultHandler.Truncate: Truncate Outgoing: " + result.Charts["Strategy Equity"].Series["Equity"].Values.Count);

            //For live charting convert to UTC
            foreach (order in result.Orders) {
                order.Value.Time = order.Value.Time.ToUniversalTime();
            }
        }

        private String CreateKey( String suffix, String dateFormat = "yyyy-MM-dd") {
            return String.format( "live/%1$s/%2$s/%3$s-{3}_{4}.json", _job.UserId, _job.ProjectId, _job.DeployId, DateTime.UtcNow.toString(dateFormat), suffix);
        }


        /**
        /// Set the chart name that we want data from.
        */
        public void SetChartSubscription( String symbol) {
            _subscription = symbol;
        }

        /**
        /// Process the synchronous result events, sampling and message reading. 
        /// This method is triggered from the algorithm manager thread.
        */
        /// Prime candidate for putting into a base class. Is identical across all result handlers.
        public void ProcessSynchronousEvents( boolean forceProcess = false) {
            time = DateTime.Now;

            if( time > _nextSample || forceProcess) {
                Log.Debug( "LiveTradingResultHandler.ProcessSynchronousEvents(): Enter");

                //Set next sample time: 4000 samples per backtest
                _nextSample = time.Add(ResamplePeriod);

                //Update the asset prices to take a real time sample of the market price even though we're using minute bars
                if( _dataFeed != null ) {
                    foreach (subscription in _dataFeed.Subscriptions) {

                        Security security;
                        if( _algorithm.Securities.TryGetValue(subscription.Configuration.Symbol, out security)) {
                            //Sample Portfolio Value:
                            price = subscription.RealtimePrice;

                            last = security.GetLastData();
                            if( last != null ) {
                                last.Value = price;
                                security.SetRealTimePrice(last);

                                // Update CashBook for Forex securities
                                Cash cash;
                                forex = security as Forex;
                                if( forex != null && _algorithm.Portfolio.CashBook.TryGetValue(forex.BaseCurrencySymbol, out cash)) {
                                    cash.Update(last);
                                }
                            }
                            else
                            {
                                // we haven't gotten data yet so just spoof a tick to push through the system to start with
                                security.SetMarketPrice(new Tick(DateTime.Now, subscription.Configuration.Symbol, price, price));
                            }

                            //Sample Asset Pricing:
                            SampleAssetPrices(subscription.Configuration.Symbol, time, price);
                        }
                    }
                }

                //Sample the portfolio value over time for chart.
                SampleEquity(time, Math.Round(_algorithm.Portfolio.TotalPortfolioValue, 4));

                //Also add the user samples / plots to the result handler tracking:
                SampleRange(_algorithm.GetChartUpdates(true));
            }

            // wait until after we're warmed up to start sending running status each minute
            if( !_algorithm.IsWarmingUp && time > _nextRunningStatus) {
                _nextRunningStatus = time.Add(Duration.ofMinutes(1));
                _api.SetAlgorithmStatus(_job.AlgorithmId, AlgorithmStatus.Running);
            }

            //Send out the debug messages:
            debugMessage = _algorithm.DebugMessages.ToList();
            _algorithm.DebugMessages.Clear();
            foreach (source in debugMessage) {
                DebugMessage(source);
            }

            //Send out the error messages:
            errorMessage = _algorithm.ErrorMessages.ToList();
            _algorithm.ErrorMessages.Clear();
            foreach (source in errorMessage) {
                ErrorMessage(source);
            }

            //Send out the log messages:
            logMessage = _algorithm.LogMessages.ToList();
            _algorithm.LogMessages.Clear();
            foreach (source in logMessage) {
                LogMessage(source);
            }

            //Set the running statistics:
            foreach (pair in _algorithm.RuntimeStatistics) {
                RuntimeStatistic(pair.Key, pair.Value);
            }

            //Send all the notification messages but timeout within a second, or if this is a force process, wait till its done.
            start = DateTime.Now;
            while (_algorithm.Notify.Messages.Count > 0 && (DateTime.Now < start.AddSeconds(1) || forceProcess)) {
                Notification message;
                if( _algorithm.Notify.Messages.TryDequeue(out message)) {
                    //Process the notification messages:
                    Log.Trace( "LiveTradingResultHandler.ProcessSynchronousEvents(): Processing Notification...");
                    try
                    {
                        _messagingHandler.SendNotification(message);
                    }
                    catch (Exception err) {
                        Log.Error(err, "Sending notification: " + message.GetType().FullName);
                    }
                }
            }

            Log.Debug( "LiveTradingResultHandler.ProcessSynchronousEvents(): Exit");
        }
    }
}
