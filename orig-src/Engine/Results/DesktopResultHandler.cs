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
using System.Threading;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.Setup;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Statistics;

package com.quantconnect.lean.Lean.Engine.Results
{
    /**
     * Desktop Result Handler - Desktop GUI Result Handler for Piping Results to WinForms:
    */
    public class DesktopResultHandler : IResultHandler
    {
        private boolean _isActive;
        private boolean _exitTriggered;
        private IAlgorithm _algorithm;
        private final object _chartLock;
        private AlgorithmNodePacket _job;

        //Sampling Periods:
        private DateTime _nextSample;
        private final Duration _resamplePeriod;
        private final Duration _notificationPeriod;

        /**
         * A dictionary containing summary statistics
        */
        public Map<String,String> FinalStatistics { get; private set; } 

        /**
         * Messaging to store notification messages for processing.
        */
        public ConcurrentQueue<Packet> Messages 
        {
            get;
            set;
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
            get;
            set;
        }

        /**
         * Boolean flag indicating the result hander thread is busy. 
         * False means it has completely finished and ready to dispose.
        */
        public boolean IsActive {
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
         * Desktop default constructor
        */
        public DesktopResultHandler() {
            FinalStatistics = new Map<String,String>();
            Messages = new ConcurrentQueue<Packet>();
            Charts = new ConcurrentMap<String, Chart>();

            _chartLock = new Object();
            _isActive = true;
            _resamplePeriod = Duration.ofSeconds(2);
            _notificationPeriod = Duration.ofSeconds(2);
        }

        /**
         * Initialize the result handler with this result packet.
        */
         * @param job Algorithm job packet for this result handler
         * @param messagingHandler">
         * @param api">
         * @param dataFeed">
         * @param setupHandler">
         * @param transactionHandler">
        public void Initialize(AlgorithmNodePacket job, IMessagingHandler messagingHandler, IApi api, IDataFeed dataFeed, ISetupHandler setupHandler, ITransactionHandler transactionHandler) {
            //Redirect the log messages here:
            _job = job;
            desktopLogging = new FunctionalLogHandler(DebugMessage, DebugMessage, ErrorMessage);
            Log.LogHandler = new CompositeLogHandler(new[] { desktopLogging, Log.LogHandler });
        }

        /**
         * Entry point for console result handler thread.
        */
        public void Run() {
            while ( !_exitTriggered || Messages.Count > 0 ) {
                Thread.Sleep(100);
            }
            DebugMessage( "DesktopResultHandler: Ending Thread...");
            _isActive = false;
        }

        /**
         * Send a debug message back to the browser console.
        */
         * @param message Message we'd like shown in console.
        public void DebugMessage( String message) {
            Messages.Enqueue(new DebugPacket(0, "", "", message));
        }

        /**
         * Send a logging message to the log list for storage.
        */
         * @param message Message we'd in the log.
        public void LogMessage( String message) {
            Messages.Enqueue(new LogPacket( "", message));
        }

        /**
         * Send a runtime error message back to the browser highlighted with in red 
        */
         * @param message Error message.
         * @param stacktrace Stacktrace information string
        public void RuntimeError( String message, String stacktrace = "") {
            Messages.Enqueue(new RuntimeErrorPacket( "", message, stacktrace));
        }

        /**
         * Send an error message back to the console highlighted in red with a stacktrace.
        */
         * @param message Error message we'd like shown in console.
        public void ErrorMessage( String message) {
            Messages.Enqueue(new HandledErrorPacket( "", message, ""));
        }

        /**
         * Send an error message back to the console highlighted in red with a stacktrace.
        */
         * @param message Error message we'd like shown in console.
         * @param stacktrace Stacktrace information string
        public void ErrorMessage( String message, String stacktrace = "") {
            Messages.Enqueue(new HandledErrorPacket( "", message, stacktrace));
        }

        /**
         * Add a sample to the chart specified by the chartName, and seriesName.
        */
         * @param chartName String chart name to place the sample.
         * @param seriesIndex Class of chart we should create if it doesn't already exist.
         * @param seriesName Series name for the chart.
         * @param seriesType Series type for the chart.
         * @param time Time for the sample
         * @param value Value for the chart sample.
         * @param unit Unit for the sample axis
         * Sample can be used to create new charts or sample equity - daily performance.
        public void Sample( String chartName, String seriesName, int seriesIndex, SeriesType seriesType, DateTime time, BigDecimal value, String unit = "$") {
            synchronized(_chartLock) {
                //Add a copy locally:
                if( !Charts.ContainsKey(chartName)) {
                    Charts.AddOrUpdate<String, Chart>(chartName, new Chart(chartName));
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
         * Sample the strategy equity at this moment in time.
        */
         * @param time Current time
         * @param value Current equity value
        public void SampleEquity(DateTime time, BigDecimal value) {
            Sample( "Strategy Equity", "Equity", 0, SeriesType.Candle, time, value, "$");
        }

        /**
         * Sample today's algorithm daily performance value.
        */
         * @param time Current time.
         * @param value Value of the daily performance.
        public void SamplePerformance(DateTime time, BigDecimal value) {
            Sample( "Strategy Equity", "Daily Performance", 0, SeriesType.Line, time, value, "%");
        }

        /**
         * Sample the current benchmark performance directly with a time-value pair.
        */
         * @param time Current backtest date.
         * @param value Current benchmark value.
         * <seealso cref="IResultHandler.Sample"/>
        public void SampleBenchmark(DateTime time, BigDecimal value) {
            Sample( "Benchmark", "Benchmark", 0, SeriesType.Line, time, value);
        }

        /**
         * Analyse the algorithm and determine its security types.
        */
         * @param types List of security types in the algorithm
        public void SecurityType(List<SecurityType> types) {
            //NOP
        }

        /**
         * Send an algorithm status update to the browser.
        */
         * @param status Status enum value.
         * @param message Additional optional status message.
         * In backtesting we do not send the algorithm status updates.
        public void SendStatusUpdate(AlgorithmStatus status, String message = "") {
            DebugMessage( "DesktopResultHandler.SendStatusUpdate(): Algorithm Status: " + status + " : " + message);
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
         * Add a range of samples to the store.
        */
         * @param updates Charting updates since the last sample request.
        public void SampleRange(List<Chart> updates) {
            synchronized(_chartLock) {
                foreach (update in updates) {
                    //Create the chart if it doesn't exist already:
                    if( !Charts.ContainsKey(update.Name)) {
                        Charts.AddOrUpdate(update.Name, new Chart(update.Name, update.ChartType));
                    }

                    //Add these samples to this chart.
                    foreach (series in update.Series.Values) {
                        //If we don't already have this record, its the first packet
                        if( !Charts[update.Name].Series.ContainsKey(series.Name)) {
                            Charts[update.Name].Series.Add(series.Name, new Series(series.Name, series.SeriesType));
                        }

                        //We already have this record, so just the new samples to the end:
                        Charts[update.Name].Series[series.Name].Values.AddRange(series.Values);
                    }
                }
            }
        }

        
        /**
         * Algorithm final analysis results dumped to the console.
        */
         * @param job Lean AlgorithmJob task
         * @param orders Collection of orders from the algorithm
         * @param profitLoss Collection of time-profit values for the algorithm
         * @param holdings Current holdings state for the algorithm
         * @param statisticsResults Statistics information for the algorithm (empty if not finished)
         * @param banner Runtime statistics banner information
        public void SendFinalResult(AlgorithmNodePacket job, Map<Integer, Order> orders, Map<DateTime, decimal> profitLoss, Map<String, Holding> holdings, StatisticsResults statisticsResults, Map<String,String> banner) {
            // uncomment these code traces to help write regression tests
            //Log.Trace( "statistics = new Map<String,String>();");
            
            // Bleh. Nicely format statistical analysis on your algorithm results. Save to file etc.
            foreach (pair in statisticsResults.Summary) {
                DebugMessage( "STATISTICS:: " + pair.Key + " " + pair.Value);
            }

            FinalStatistics = statisticsResults.Summary;
        }

        /**
         * Set the Algorithm instance for ths result.
        */
         * @param algorithm Algorithm we're working on.
         * While setting the algorithm the backtest result handler.
        public void SetAlgorithm(IAlgorithm algorithm) {
            _algorithm = algorithm;
        }

        /**
         * Terminate the result thread and apply any required exit proceedures.
        */
        public void Exit() {
            _exitTriggered = true;
        }

        /**
         * Send a new order event to the browser.
        */
         * In backtesting the order events are not sent because it would generate a high load of messaging.
         * @param newEvent New order event details
        public void OrderEvent(OrderEvent newEvent) {
            DebugMessage( "DesktopResultHandler.OrderEvent(): id:" + newEvent.OrderId + " >> Status:" + newEvent.Status + " >> Fill Price: " + newEvent.FillPrice.toString( "C") + " >> Fill Quantity: " + newEvent.FillQuantity);
        }


        /**
         * Set the current runtime statistics of the algorithm
        */
         * @param key Runtime headline statistic name
         * @param value Runtime headline statistic value
        public void RuntimeStatistic( String key, String value) {
            DebugMessage( "DesktopResultHandler.RuntimeStatistic(): " + key + " : " + value);
        }


        /**
         * Clear the outstanding message queue to exit the thread.
        */
        public void PurgeQueue() {
            Messages.Clear();
        }

        /**
         * Store result on desktop.
        */
         * @param packet Packet of data to store.
         * @param async Store the packet asyncronously to speed up the thread.
         * Async creates crashes in Mono 3.10 if the thread disappears before the upload is complete so it is disabled for now.
        public void StoreResult(Packet packet, boolean async = false) {
            // Do nothing.
        }

        /**
         * Not used
        */
        public void SetChartSubscription( String symbol) {
            //
        }

        /**
         * Process the synchronous result events, sampling and message reading. 
         * This method is triggered from the algorithm manager thread.
        */
         * Prime candidate for putting into a base class. Is identical across all result handlers.
        public void ProcessSynchronousEvents( boolean forceProcess = false) {
            time = _algorithm.Time;

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

    } // End Result Handler Thread:

} // End Namespace
