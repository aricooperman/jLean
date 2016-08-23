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
using System.ComponentModel.Composition;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.Setup;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Statistics;

package com.quantconnect.lean.Lean.Engine.Results
{
    /**
    /// Handle the results of the backtest: where should we send the profit, portfolio updates:
    /// Backtester or the Live trading platform:
    */
    [InheritedExport(typeof(IResultHandler))]
    public interface IResultHandler
    {
        /**
        /// Put messages to process into the queue so they are processed by this thread.
        */
        ConcurrentQueue<Packet> Messages
        {
            get;
            set;
        }

        /**
        /// Charts collection for storing the master copy of user charting data.
        */
        ConcurrentMap<String, Chart> Charts
        {
            get;
            set;
        }

        /**
        /// Sampling period for timespans between resamples of the charting equity.
        */
        /// Specifically critical for backtesting since with such long timeframes the sampled data can get extreme.
        Duration ResamplePeriod
        {
            get;
        }

        /**
        /// How frequently the backtests push messages to the browser.
        */
        /// Update frequency of notification packets
        Duration NotificationPeriod
        {
            get;
        }

        /**
        /// Boolean flag indicating the result hander thread is busy. 
        /// False means it has completely finished and ready to dispose.
        */
        boolean IsActive
        {
            get;
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
        void Initialize(AlgorithmNodePacket job, IMessagingHandler messagingHandler, IApi api, IDataFeed dataFeed, ISetupHandler setupHandler, ITransactionHandler transactionHandler);

        /**
        /// Primary result thread entry point to process the result message queue and send it to whatever endpoint is set.
        */
        void Run();

        /**
        /// Process debug messages with the preconfigured settings.
        */
         * @param message">String debug message
        void DebugMessage( String message);

        /**
        /// Send a list of security types to the browser
        */
         * @param types">Security types list inside algorithm
        void SecurityType(List<SecurityType> types);

        /**
        /// Send a logging message to the log list for storage.
        */
         * @param message">Message we'd in the log.
        void LogMessage( String message);

        /**
        /// Send an error message back to the browser highlighted in red with a stacktrace.
        */
         * @param error">Error message we'd like shown in console.
         * @param stacktrace">Stacktrace information string
        void ErrorMessage( String error, String stacktrace = "");

        /**
        /// Send a runtime error message back to the browser highlighted with in red 
        */
         * @param message">Error message.
         * @param stacktrace">Stacktrace information string
        void RuntimeError( String message, String stacktrace = "");

        /**
        /// Add a sample to the chart specified by the chartName, and seriesName.
        */
         * @param chartName">String chart name to place the sample.
         * @param seriesName">Series name for the chart.
         * @param seriesType">Series type for the chart.
         * @param time">Time for the sample
         * @param value">Value for the chart sample.
         * @param unit">Unit for the sample chart
         * @param seriesIndex">Index of the series we're sampling
        /// Sample can be used to create new charts or sample equity - daily performance.
        void Sample( String chartName, String seriesName, int seriesIndex, SeriesType seriesType, DateTime time, BigDecimal value, String unit = "$");

        /**
        /// Wrapper methond on sample to create the equity chart.
        */
         * @param time">Time of the sample.
         * @param value">Equity value at this moment in time.
        /// <seealso cref="Sample( String,string,int,SeriesType,DateTime,decimal,string)"/>
        void SampleEquity(DateTime time, BigDecimal value);

        /**
        /// Sample the current daily performance directly with a time-value pair.
        */
         * @param time">Current backtest date.
         * @param value">Current daily performance value.
        /// <seealso cref="Sample( String,string,int,SeriesType,DateTime,decimal,string)"/>
        void SamplePerformance(DateTime time, BigDecimal value);

        /**
        /// Sample the current benchmark performance directly with a time-value pair.
        */
         * @param time">Current backtest date.
         * @param value">Current benchmark value.
        /// <seealso cref="Sample( String,string,int,SeriesType,DateTime,decimal,string)"/>
        void SampleBenchmark(DateTime time, BigDecimal value);

        /**
        /// Sample the asset prices to generate plots.
        */
         * @param symbol">Symbol we're sampling.
         * @param time">Time of sample
         * @param value">Value of the asset price
        /// <seealso cref="Sample( String,string,int,SeriesType,DateTime,decimal,string)"/>
        void SampleAssetPrices(Symbol symbol, DateTime time, BigDecimal value);

        /**
        /// Add a range of samples from the users algorithms to the end of our current list.
        */
         * @param samples">Chart updates since the last request.
        /// <seealso cref="Sample( String,string,int,SeriesType,DateTime,decimal,string)"/>
        void SampleRange(List<Chart> samples);

        /**
        /// Set the algorithm of the result handler after its been initialized.
        */
         * @param algorithm">Algorithm object matching IAlgorithm interface
        void SetAlgorithm(IAlgorithm algorithm);

        /**
        /// Save the snapshot of the total results to storage.
        */
         * @param packet">Packet to store.
         * @param async">Store the packet asyncronously to speed up the thread.
        /// Async creates crashes in Mono 3.10 if the thread disappears before the upload is complete so it is disabled for now.
        void StoreResult(Packet packet, boolean async = false);

        /**
        /// Post the final result back to the controller worker if backtesting, or to console if local.
        */
         * @param job">Lean AlgorithmJob task
         * @param orders">Collection of orders from the algorithm
         * @param profitLoss">Collection of time-profit values for the algorithm
         * @param holdings">Current holdings state for the algorithm
         * @param statisticsResults">Statistics information for the algorithm (empty if not finished)
         * @param banner">Runtime statistics banner information
        void SendFinalResult(AlgorithmNodePacket job, Map<Integer, Order> orders, Map<DateTime, decimal> profitLoss, Map<String, Holding> holdings, StatisticsResults statisticsResults, Map<String,String> banner);

        /**
        /// Send a algorithm status update to the user of the algorithms running state.
        */
         * @param status">Status enum of the algorithm.
         * @param message">Optional String message describing reason for status change.
        void SendStatusUpdate(AlgorithmStatus status, String message = "");

        /**
        /// Set the chart name:
        */
         * @param symbol">Symbol of the chart we want.
        void SetChartSubscription( String symbol);

        /**
        /// Set a dynamic runtime statistic to show in the (live) algorithm header
        */
         * @param key">Runtime headline statistic name
         * @param value">Runtime headline statistic value
        void RuntimeStatistic( String key, String value);

        /**
        /// Send a new order event.
        */
         * @param newEvent">Update, processing or cancellation of an order, update the IDE in live mode or ignore in backtesting.
        void OrderEvent(OrderEvent newEvent);

        /**
        /// Terminate the result thread and apply any required exit proceedures.
        */
        void Exit();

        /**
        /// Purge/clear any outstanding messages in message queue.
        */
        void PurgeQueue();

        /**
        /// Process any synchronous events in here that are primarily triggered from the algorithm loop
        */
        void ProcessSynchronousEvents( boolean forceProcess = false);
    }
}
