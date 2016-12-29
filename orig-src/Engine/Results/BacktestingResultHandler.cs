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
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.Setup;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Statistics;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.Results
{
    /**
     * Backtesting result handler passes messages back from the Lean to the User.
    */
    public class BacktestingResultHandler : IResultHandler
    {
        private boolean _exitTriggered = false;
        private BacktestNodePacket _job;
        private int _jobDays = 0;
        private String _compileId = "";
        private String _backtestId = "";
        private DateTime _nextUpdate = new DateTime();
        private DateTime _nextS3Update = new DateTime();
        DateTime _lastUpdate = new DateTime();
        private String _debugMessage = "";
        private List<String> _log = new List<String>();
        private String _errorMessage = "";
        private IAlgorithm _algorithm;
        private ConcurrentQueue<Packet> _messages;
        private ConcurrentMap<String, Chart> _charts;
        private boolean _isActive = true;
        private object _chartLock = new Object();
        private object _runtimeLock = new Object();
        private final Map<String,String> _runtimeStatistics = new Map<String,String>();
        private double _daysProcessed = 0;
        private double _lastDaysProcessed = 1;
        private boolean _processingFinalPacket = false;

        //Debug variables:
        private int _debugMessageCount = 0;
        private int _debugMessageMin = 100;
        private int _debugMessageMax = 10;
        private int _debugMessageLength = 200;
        private String _debugMessagePeriod = "day";

        //Sampling Periods:
        private Duration _resamplePeriod = Duration.ofMinutes(4);
        private Duration _notificationPeriod = Duration.ofSeconds(2);

        //Processing Time:
        private DateTime _startTime;
        private DateTime _nextSample;
        private IMessagingHandler _messagingHandler;
        private IApi _api;
        private ITransactionHandler _transactionHandler;
        private ISetupHandler _setupHandler;

        private static final double _samples = 4000;
        private static final double _minimumSamplePeriod = 4;

        /**
         * Packeting message queue to temporarily store packets and then pull for processing.
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
         * Local object access to the algorithm for the underlying Debug and Error messaging.
        */
        public IAlgorithm Algorithm 
        {
            get
            {
                return _algorithm;
            }
            set
            {
                _algorithm = value;
            }
        }

        /**
         * Charts collection for storing the master copy of user charting data.
        */
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
         * Boolean flag indicating the result hander thread is completely finished and ready to dispose.
        */
        public boolean IsActive 
        { 
            get
            {
                return _isActive;
            }
        }



        /**
         * Sampling period for timespans between resamples of the charting equity.
        */
         * Specifically critical for backtesting since with such long timeframes the sampled data can get extreme.
        public Duration ResamplePeriod
        {
            get 
            {
                return _resamplePeriod;
            }
        }

        /**
         * How frequently the backtests push messages to the browser.
        */
         * Update frequency of notification packets
        public Duration NotificationPeriod
        {
            get 
            {
                return _notificationPeriod;
            }
        }

        /**
         * A dictionary containing summary statistics
        */
        public Map<String,String> FinalStatistics { get; private set; }

        /**
         * Default initializer for 
        */
        public BacktestingResultHandler() {
            //Initialize Properties:
            _messages = new ConcurrentQueue<Packet>();
            _charts = new ConcurrentMap<String, Chart>();
            _chartLock = new Object();
            _isActive = true;

            //Notification Period for Browser Pushes:
            _notificationPeriod = Duration.ofSeconds(2);
            _exitTriggered = false;

            //Set the start time for the algorithm
            _startTime = DateTime.Now;

            //Default charts:
            Charts.AddOrUpdate( "Strategy Equity", new Chart( "Strategy Equity"));
            Charts["Strategy Equity"].Series.Add( "Equity", new Series( "Equity", SeriesType.Candle, 0, "$"));
            Charts["Strategy Equity"].Series.Add( "Daily Performance", new Series( "Daily Performance", SeriesType.Bar, 1, "%"));
        }

        /**
         * Initialize the result handler with this result packet.
        */
         * @param job Algorithm job packet for this result handler
         * @param messagingHandler The handler responsible for communicating messages to listeners
         * @param api The api instance used for handling logs
         * @param dataFeed">
         * @param setupHandler">
         * @param transactionHandler">
        public void Initialize(AlgorithmNodePacket job, IMessagingHandler messagingHandler, IApi api, IDataFeed dataFeed, ISetupHandler setupHandler, ITransactionHandler transactionHandler) {
            _api = api;
            _messagingHandler = messagingHandler;
            _transactionHandler = transactionHandler;
            _setupHandler = setupHandler;
            _job = (BacktestNodePacket)job;
            if( _job == null ) throw new Exception( "BacktestingResultHandler.Constructor(): Submitted Job type invalid.");
            _compileId = _job.CompileId;
            _backtestId = _job.BacktestId;
        }
        
        /**
         * The main processing method steps through the messaging queue and processes the messages one by one.
        */
        public void Run() {
            //Initialize:
            lastMessage = "";
            _lastDaysProcessed = 5;

            //Setup minimum result arrays:
            //SampleEquity(job.periodStart, job.startingCapital);
            //SamplePerformance(job.periodStart, 0);

            try
            {
                while (!(_exitTriggered && Messages.Count == 0)) {
                    //While there's no work to do, go back to the algorithm:
                    if( Messages.Count == 0) {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        //1. Process Simple Messages in Queue
                        Packet packet;
                        if( Messages.TryDequeue(out packet)) {
                            _messagingHandler.Send(packet);
                        }
                    }

                    //2. Update the packet scanner:
                    Update();

                } // While !End.
            }
            catch (Exception err) {
                // unexpected error, we need to close down shop
                Log.Error(err);
                // quit the algorithm due to error
                _algorithm.RunTimeError = err;
            }

            Log.Trace( "BacktestingResultHandler.Run(): Ending Thread...");
            _isActive = false;
        } // End Run();

        /**
         * Send a backtest update to the browser taking a latest snapshot of the charting data.
        */
        public void Update() {
            try
            {
                //Sometimes don't run the update, if not ready or we're ending.
                if( Algorithm == null || Algorithm.Transactions == null || _processingFinalPacket) {
                    return;
                }

                if( DateTime.Now <= _nextUpdate || !(_daysProcessed > (_lastDaysProcessed + 1))) return;

                //Extract the orders since last update
                deltaOrders = new Map<Integer, Order>();

                try
                {
                    deltaOrders = (from order in _transactionHandler.Orders
                        where order.Value.Time.Date >= _lastUpdate && order.Value.Status == OrderStatus.Filled
                        select order).ToDictionary(t -> t.Key, t -> t.Value);
                }
                catch (Exception err) {
                    Log.Error(err, "Transactions");
                }

                //Limit length of orders we pass back dynamically to avoid flooding.
                if( deltaOrders.Count > 50) deltaOrders.Clear();

                //Reset loop variables:
                try
                {
                    _lastUpdate = Algorithm.Time.Date;
                    _lastDaysProcessed = _daysProcessed;
                    _nextUpdate = DateTime.Now.AddSeconds(0.5);
                }
                catch (Exception err) {
                    Log.Error(err, "Can't update variables");
                }

                deltaCharts = new Map<String, Chart>();
                synchronized(_chartLock) {
                    //Get the updates since the last chart
                    foreach (chart in Charts.Values) {
                        deltaCharts.Add(chart.Name, chart.GetUpdates());
                    }
                }

                //Get the runtime statistics from the user algorithm:
                runtimeStatistics = new Map<String,String>();
                synchronized(_runtimeLock) {
                    foreach (pair in _runtimeStatistics) {
                        runtimeStatistics.Add(pair.Key, pair.Value);
                    }
                }
                runtimeStatistics.Add( "Unrealized", "$" + _algorithm.Portfolio.TotalUnrealizedProfit.toString( "N2"));
                runtimeStatistics.Add( "Fees", "-$" + _algorithm.Portfolio.TotalFees.toString( "N2"));
                runtimeStatistics.Add( "Net Profit", "$" + _algorithm.Portfolio.TotalProfit.toString( "N2"));
                runtimeStatistics.Add( "Return", ((_algorithm.Portfolio.TotalPortfolioValue - _setupHandler.StartingPortfolioValue) / _setupHandler.StartingPortfolioValue).toString( "P"));
                runtimeStatistics.Add( "Equity", "$" + _algorithm.Portfolio.TotalPortfolioValue.toString( "N2"));

                //Profit Loss Changes:
                progress = new BigDecimal( _daysProcessed / _jobDays);
                if( progress > 0.999m) progress = 0.999m;

                //1. Cloud Upload -> Upload the whole packet to S3  Immediately:
                completeResult = new BacktestResult(Charts, _transactionHandler.Orders, Algorithm.Transactions.TransactionRecord, new Map<String,String>(), runtimeStatistics, new Map<String, AlgorithmPerformance>());
                complete = new BacktestResultPacket(_job, completeResult, progress);

                if( DateTime.Now > _nextS3Update) {
                    _nextS3Update = DateTime.Now.AddSeconds(30);
                    StoreResult(complete, false);
                }

                //2. Backtest Update -> Send the truncated packet to the backtester:
                splitPackets = SplitPackets(deltaCharts, deltaOrders, runtimeStatistics, progress);

                foreach (backtestingPacket in splitPackets) {
                    _messagingHandler.Send(backtestingPacket);
                }
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
         * Run over all the data and break it into smaller packets to ensure they all arrive at the terminal
        */
        public IEnumerable<BacktestResultPacket> SplitPackets(Map<String, Chart> deltaCharts, Map<Integer, Order> deltaOrders, Map<String,string> runtimeStatistics, BigDecimal progress) {
            // break the charts into groups
            splitPackets = new List<BacktestResultPacket>();
            foreach (chart in deltaCharts.Values) {
                //Don't add packet if the series is empty:
                if( chart.Series.Values.Sum(x -> x.Values.Count) == 0) continue;

                splitPackets.Add(new BacktestResultPacket(_job, new BacktestResult { Charts = new Map<String, Chart>() {
                    {chart.Name,chart}
                }  }, progress));
            }

            // Add the orders into the charting packet:
            splitPackets.Add(new BacktestResultPacket(_job, new BacktestResult { Orders = deltaOrders }, progress));

            //Add any user runtime statistics into the backtest.
            splitPackets.Add(new BacktestResultPacket(_job, new BacktestResult { RuntimeStatistics = runtimeStatistics }, progress));

            return splitPackets;
        }

        /**
         * Save the snapshot of the total results to storage.
        */
         * @param packet Packet to store.
         * @param async Store the packet asyncronously to speed up the thread.
         * Async creates crashes in Mono 3.10 if the thread disappears before the upload is complete so it is disabled for now.
        public void StoreResult(Packet packet, boolean async = false) {
            //Initialize:
            serialized = "";
            key = "";

            try
            {
                synchronized(_chartLock) {
                    //1. Make sure this is the right type of packet:
                    if( packet.Type != PacketType.BacktestResult) return;

                    //2. Port to packet format:
                    result = packet as BacktestResultPacket;

                    if( result != null ) {
                        //3. Get Storage Location:
                        key = "backtests/" + _job.UserId + "/" + _job.ProjectId + "/" + _job.BacktestId + ".json";

                        //4. Serialize to JSON:
                        serialized = JsonConvert.SerializeObject(result.Results);
                    }
                    else 
                    {
                        Log.Error( "BacktestingResultHandler.StoreResult(): Result Null.");
                    }

                    //Upload Results Portion
                    _api.Store(serialized, key, StoragePermissions.Authenticated, async);
                }
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
         * Send a final analysis result back to the IDE.
        */
         * @param job Lean AlgorithmJob task
         * @param orders Collection of orders from the algorithm
         * @param profitLoss Collection of time-profit values for the algorithm
         * @param holdings Current holdings state for the algorithm
         * @param statisticsResults Statistics information for the algorithm (empty if not finished)
         * @param banner Runtime statistics banner information
        public void SendFinalResult(AlgorithmNodePacket job, Map<Integer, Order> orders, Map<DateTime, decimal> profitLoss, Map<String, Holding> holdings, StatisticsResults statisticsResults, Map<String,String> banner) { 
            try
            {
                FinalStatistics = statisticsResults.Summary;

                //Convert local dictionary:
                charts = new Map<String, Chart>(Charts);
                _processingFinalPacket = true;

                // clear the trades collection before placing inside the backtest result
                foreach (ap in statisticsResults.RollingPerformances.Values) {
                    ap.ClosedTrades.Clear();
                }

                //Create a result packet to send to the browser.
                result = new BacktestResultPacket((BacktestNodePacket) job,
                    new BacktestResult(charts, orders, profitLoss, statisticsResults.Summary, banner, statisticsResults.RollingPerformances, statisticsResults.TotalPerformance)
                    , 1m) {
                    ProcessingTime = (DateTime.Now - _startTime).TotalSeconds,
                    DateFinished = DateTime.Now,
                    Progress = 1
                };

                //Place result into storage.
                StoreResult(result);

                //Second, send the truncated packet:
                _messagingHandler.Send(result);

                Log.Trace( "BacktestingResultHandler.SendAnalysisResult(): Processed final packet"); 
            } 
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
         * Set the Algorithm instance for ths result.
        */
         * @param algorithm Algorithm we're working on.
         * While setting the algorithm the backtest result handler.
        public void SetAlgorithm(IAlgorithm algorithm) {
            _algorithm = algorithm;

            //Get the resample period:
            totalMinutes = (_job.PeriodFinish - _job.PeriodStart).TotalMinutes;
            resampleMinutes = (totalMinutes < (_minimumSamplePeriod * _samples)) ? _minimumSamplePeriod : (totalMinutes / _samples); // Space out the sampling every 
            _resamplePeriod = Duration.ofMinutes(resampleMinutes);
            Log.Trace( "BacktestingResultHandler(): Sample Period Set: " + resampleMinutes.toString( "00.00"));
            
            //Setup the sampling periods:
            _jobDays = Time.TradeableDates(Algorithm.Securities.Values, _job.PeriodStart, _job.PeriodFinish);

            //Setup Debug Messaging:
            _debugMessageMax =  Integer.parseInt( 10 * _jobDays);
            //Minimum 100 messages per backtest:
            if( _debugMessageMax < _debugMessageMin) _debugMessageMax = _debugMessageMin;
            //Messaging for the log messages:
            _debugMessagePeriod = "backtest";

            //Set the security / market types.
            types = new List<SecurityType>();
            foreach (security in _algorithm.Securities.Values) {
                if( !types.Contains(security.Type)) types.Add(security.Type);
            }
            SecurityType(types);

            if( Config.GetBool( "forward-console-messages", true)) {
                // we need to forward Console.Write messages to the algorithm's Debug function
                debug = new FuncTextWriter(algorithm.Debug);
                error = new FuncTextWriter(algorithm.Error);
                Console.SetOut(debug);
                Console.SetError(error);
            }
        }

        /**
         * Send a debug message back to the browser console.
        */
         * @param message Message we'd like shown in console.
        public void DebugMessage( String message) {
            if( Messages.Count > 500) return;
            Messages.Enqueue(new DebugPacket(_job.ProjectId, _backtestId, _compileId, message));

            //Save last message sent:
            _log.Add(_algorithm.Time.toString(DateFormat.UI) + " " + message);
            _debugMessage = message;
        }

        /**
         * Send a logging message to the log list for storage.
        */
         * @param message Message we'd in the log.
        public void LogMessage( String message) {
            Messages.Enqueue(new LogPacket(_backtestId, message)); 

            _log.Add(_algorithm.Time.toString(DateFormat.UI) + " " + message);
        }

        /**
         * Send list of security asset types the algortihm uses to browser.
        */
        public void SecurityType(List<SecurityType> types) {
            packet = new SecurityTypesPacket
            {
                Types = types
            };
            Messages.Enqueue(packet);
        }

        /**
         * Send an error message back to the browser highlighted in red with a stacktrace.
        */
         * @param message Error message we'd like shown in console.
         * @param stacktrace Stacktrace information string
        public void ErrorMessage( String message, String stacktrace = "") {
            if( message == _errorMessage) return;
            if( Messages.Count > 500) return;
            Messages.Enqueue(new HandledErrorPacket(_backtestId, message, stacktrace));
            _errorMessage = message;
        }

        /**
         * Send a runtime error message back to the browser highlighted with in red 
        */
         * @param message Error message.
         * @param stacktrace Stacktrace information string
        public void RuntimeError( String message, String stacktrace = "") {
            PurgeQueue();
            Messages.Enqueue(new RuntimeErrorPacket(_backtestId, message, stacktrace));
            _errorMessage = message;
        }

        /**
         * Add a sample to the chart specified by the chartName, and seriesName.
        */
         * @param chartName String chart name to place the sample.
         * @param seriesIndex Class of chart we should create if it doesn't already exist.
         * @param seriesName Series name for the chart.
         * @param seriesType Series type for the chart.
         * @param time Time for the sample
         * @param unit Unit of the sample
         * @param value Value for the chart sample.
        public void Sample( String chartName, String seriesName, int seriesIndex, SeriesType seriesType, DateTime time, BigDecimal value, String unit = "$") {
            synchronized(_chartLock) {
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
        }

        /**
         * Sample the current equity of the strategy directly with time-value pair.
        */
         * @param time Current backtest time.
         * @param value Current equity value.
        public void SampleEquity(DateTime time, BigDecimal value) {
            //Sample the Equity Value:
            Sample( "Strategy Equity", "Equity", 0, SeriesType.Candle, time, value, "$");

            //Recalculate the days processed:
            _daysProcessed = (time - _job.PeriodStart).TotalDays;
        }

        /**
         * Sample the current daily performance directly with a time-value pair.
        */
         * @param time Current backtest date.
         * @param value Current daily performance value.
        public void SamplePerformance(DateTime time, BigDecimal value) {
            //Added a second chart to equity plot - daily perforamnce:
            Sample( "Strategy Equity", "Daily Performance", 1, SeriesType.Bar, time, value, "%");
        }

        /**
         * Sample the current benchmark performance directly with a time-value pair.
        */
         * @param time Current backtest date.
         * @param value Current benchmark value.
         * <seealso cref="IResultHandler.Sample"/>
        public void SampleBenchmark(DateTime time, BigDecimal value) {
            Sample( "Benchmark", "Benchmark", 0, SeriesType.Line, time, value, "$");
        }

        /**
         * Add a range of samples from the users algorithms to the end of our current list.
        */
         * @param updates Chart updates since the last request.
        public void SampleRange(List<Chart> updates) {
            synchronized(_chartLock) {
                foreach (update in updates) {
                    //Create the chart if it doesn't exist already:
                    if( !Charts.ContainsKey(update.Name)) {
                        Charts.AddOrUpdate(update.Name, new Chart(update.Name));
                    }

                    //Add these samples to this chart.
                    foreach (series in update.Series.Values) {
                        //If we don't already have this record, its the first packet
                        if( !Charts[update.Name].Series.ContainsKey(series.Name)) {
                            Charts[update.Name].Series.Add(series.Name, new Series(series.Name, series.SeriesType, series.Index, series.Unit) {
                                Color = series.Color, ScatterMarkerSymbol = series.ScatterMarkerSymbol
                            });
                        }

                        //We already have this record, so just the new samples to the end:
                        Charts[update.Name].Series[series.Name].Values.AddRange(series.Values);
                    }
                }
            }
        }

        /**
         * Terminate the result thread and apply any required exit proceedures.
        */
        public void Exit() {
            //Process all the log messages and send them to the S3:
            logURL = ProcessLogMessages(_job);
            if( logURL != "") DebugMessage( "Your log was successfully created and can be downloaded from: " + logURL);

            //Set exit flag, and wait for the messages to send:
            _exitTriggered = true;
        }

        /**
         * Send a new order event to the browser.
        */
         * In backtesting the order events are not sent because it would generate a high load of messaging.
         * @param newEvent New order event details
        public void OrderEvent(OrderEvent newEvent) { 
            // NOP. Don't do any order event processing for results in backtest mode.
        }


        /**
         * Send an algorithm status update to the browser.
        */
         * @param status Status enum value.
         * @param message Additional optional status message.
         * In backtesting we do not send the algorithm status updates.
        public void SendStatusUpdate(AlgorithmStatus status, String message = "") { 
            //NOP. Don't send status for backtests
        }

        /**
         * Sample the asset prices to generate plots.
        */
         * @param symbol Symbol we're sampling.
         * @param time Time of sample
         * @param value Value of the asset price
        public void SampleAssetPrices(Symbol symbol, DateTime time, BigDecimal value) {
            //NOP. Don't sample asset prices in console.
        }

        /**
         * Purge/clear any outstanding messages in message queue.
        */
        public void PurgeQueue() {
            Messages.Clear();
        }

        /**
         * Set the current runtime statistics of the algorithm. 
         * These are banner/title statistics which show at the top of the live trading results.
        */
         * @param key Runtime headline statistic name
         * @param value Runtime headline statistic value
        public void RuntimeStatistic( String key, String value) {
            synchronized(_runtimeLock) {
                _runtimeStatistics[key] = value;
            }
        }

        /**
         * Process log messages to ensure the meet the user caps and send them to storage.
        */
         * @param job Algorithm job/task packet
        @returns String URL of log
        private String ProcessLogMessages(AlgorithmNodePacket job) {
            remoteUrl = @"http://data.quantconnect.com/";
            logLength = 0;

            try
            {
                //Return nothing if there's no log messages to procesS:
                if( !_log.Any()) return "";

                //Get the max length allowed for the algorithm:
                allowance = _api.ReadLogAllowance(job.UserId, job.Channel);
                logBacktestMax = allowance[0];
                logDailyMax = allowance[1];
                logRemaining = Math.Min(logBacktestMax, allowance[2]); //Minimum of maxium backtest or remaining allowance.
                hitLimit = false;
                serialized = new StringBuilder();

                key = "backtests/" + job.UserId + "/" + job.ProjectId + "/" + job.AlgorithmId + "-log.txt";
                remoteUrl += key;

                foreach (line in _log) {
                    if( (logLength + line.Length) < logRemaining) {
                        serialized.Append(line + "\r\n");
                        logLength += line.Length;
                    }
                    else
                    {
                        btMax = Math.Round((double)logBacktestMax / 1024, 0) + "kb";
                        dyMax = Math.Round((double)logDailyMax / 1024, 0) + "kb";

                        //Same cap notice for both free & subscribers
                        requestMore = "";
                        capNotice = "You currently have a maximum of " + btMax + " of log data per backtest, and " + dyMax + " total max per day.";
                        DebugMessage( "You currently have a maximum of " + btMax + " of log data per backtest remaining, and " + dyMax + " total max per day.");
                        
                        //Data providers set max log limits and require email requests for extensions
                        if( job.UserPlan == UserPlan.Free) {
                            requestMore ="Please upgrade your account and contact us to request more allocation here: https://www.quantconnect.com/contact"; 
                        }
                        else
                        {
                            requestMore = "If you require more please briefly explain request for more allocation here: https://www.quantconnect.com/contact";
                        }
                        DebugMessage(requestMore);
                        serialized.Append(capNotice);
                        serialized.Append(requestMore);
                        hitLimit = true;
                        break;
                    }
                }

                //Save the log: Upload this file to S3:
                _api.Store(serialized.toString(), key, StoragePermissions.Public);
                //Record the data usage:
                _api.UpdateDailyLogUsed(job.UserId, job.AlgorithmId, remoteUrl, logLength, job.Channel, hitLimit);
            }
            catch (Exception err) {
                Log.Error(err);
            }
            Log.Trace( "BacktestingResultHandler.ProcessLogMessages(): Ready: " + remoteUrl);
            return remoteUrl;
        }

        /**
         * Set the chart subscription we want data for. Not used in backtesting.
        */
        public void SetChartSubscription( String symbol) {
            //NOP.
        }

        /**
         * Process the synchronous result events, sampling and message reading. 
         * This method is triggered from the algorithm manager thread.
        */
         * Prime candidate for putting into a base class. Is identical across all result handlers.
        public void ProcessSynchronousEvents( boolean forceProcess = false) {
            time = _algorithm.UtcTime;

            if( time > _nextSample || forceProcess) {
                //Set next sample time: 4000 samples per backtest
                _nextSample = time.Add(ResamplePeriod);

                //Sample the portfolio value over time for chart.
                SampleEquity(time, Math.Round(_algorithm.Portfolio.TotalPortfolioValue, 4));

                //Also add the user samples / plots to the result handler tracking:
                SampleRange(_algorithm.GetChartUpdates());

                //Sample the asset pricing:
                foreach (security in _algorithm.Securities.Values) {
                    SampleAssetPrices(security.Symbol, time, security.Price);
                }
            }

            //Send out the debug messages:
            _algorithm.DebugMessages.ForEach(x -> DebugMessage(x));
            _algorithm.DebugMessages.Clear();

            //Send out the error messages:
            _algorithm.ErrorMessages.ForEach(x -> ErrorMessage(x));
            _algorithm.ErrorMessages.Clear();

            //Send out the log messages:
            _algorithm.LogMessages.ForEach(x -> LogMessage(x));
            _algorithm.LogMessages.Clear();

            //Set the running statistics:
            foreach (pair in _algorithm.RuntimeStatistics) {
                RuntimeStatistic(pair.Key, pair.Value);
            }
        }
    }
}
