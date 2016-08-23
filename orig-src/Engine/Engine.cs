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
using QuantConnect.Brokerages;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Statistics;

package com.quantconnect.lean.Lean.Engine 
{
    /**
    /// LEAN ALGORITHMIC TRADING ENGINE: ENTRY POINT.
    /// 
    /// The engine loads new tasks, create the algorithms and threads, and sends them 
    /// to Algorithm Manager to be executed. It is the primary operating loop.
    */
    public class Engine
    {
        private final boolean _liveMode;
        private final LeanEngineSystemHandlers _systemHandlers;
        private final LeanEngineAlgorithmHandlers _algorithmHandlers;

        /**
        /// Gets the configured system handlers for this engine instance
        */
        public LeanEngineSystemHandlers SystemHandlers
        {
            get { return _systemHandlers; }
        }

        /**
        /// Gets the configured algorithm handlers for this engine instance
        */
        public LeanEngineAlgorithmHandlers AlgorithmHandlers
        {
            get { return _algorithmHandlers;}
        }

        /**
        /// Initializes a new instance of the <see cref="Engine"/> class using the specified handlers
        */
         * @param systemHandlers">The system handlers for controlling acquisition of jobs, messaging, and api calls
         * @param algorithmHandlers">The algorithm handlers for managing algorithm initialization, data, results, transaction, and real time events
         * @param liveMode">True when running in live mode, false otherwises
        public Engine(LeanEngineSystemHandlers systemHandlers, LeanEngineAlgorithmHandlers algorithmHandlers, boolean liveMode) {
            _liveMode = liveMode;
            _systemHandlers = systemHandlers;
            _algorithmHandlers = algorithmHandlers;
        }

        /**
        /// Runs a single backtest/live job from the job queue
        */
         * @param job">The algorithm job to be processed
         * @param assemblyPath">The path to the algorithm's assembly
        public void Run(AlgorithmNodePacket job, String assemblyPath) {
            algorithm = default(IAlgorithm);
            algorithmManager = new AlgorithmManager(_liveMode);

            //Start monitoring the backtest active status:
            statusPing = new StateCheck.Ping(algorithmManager, _systemHandlers.Api, _algorithmHandlers.Results, _systemHandlers.Notify, job);
            statusPingThread = new Thread(statusPing.Run);
            statusPingThread.Start();

            try
            {
                //Reset thread holders.
                initializeComplete = false;
                Thread threadFeed = null;
                Thread threadTransactions = null;
                Thread threadResults = null;
                Thread threadRealTime = null;

                //-> Initialize messaging system
                _systemHandlers.Notify.SetAuthentication(job);

                //-> Set the result handler type for this algorithm job, and launch the associated result thread.
                _algorithmHandlers.Results.Initialize(job, _systemHandlers.Notify, _systemHandlers.Api, _algorithmHandlers.DataFeed, _algorithmHandlers.Setup, _algorithmHandlers.Transactions);

                threadResults = new Thread(_algorithmHandlers.Results.Run, 0) {Name = "Result Thread"};
                threadResults.Start();

                IBrokerage brokerage = null;
                try
                {
                    // Save algorithm to cache, load algorithm instance:
                    algorithm = _algorithmHandlers.Setup.CreateAlgorithmInstance(assemblyPath, job.Language);

                    // Initialize the brokerage
                    brokerage = _algorithmHandlers.Setup.CreateBrokerage(job, algorithm);

                    // Initialize the data feed before we initialize so he can intercept added securities/universes via events
                    _algorithmHandlers.DataFeed.Initialize(algorithm, job, _algorithmHandlers.Results, _algorithmHandlers.MapFileProvider, _algorithmHandlers.FactorFileProvider);

                    // initialize command queue system
                    _algorithmHandlers.CommandQueue.Initialize(job, algorithm);

                    // set the history provider before setting up the algorithm
                    _algorithmHandlers.HistoryProvider.Initialize(job, _algorithmHandlers.MapFileProvider, _algorithmHandlers.FactorFileProvider, progress =>
                    {
                        // send progress updates to the result handler only during initialization
                        if( !algorithm.GetLocked() || algorithm.IsWarmingUp) {
                            _algorithmHandlers.Results.SendStatusUpdate(AlgorithmStatus.History, 
                                String.format( "Processing history %1$s%...", progress));
                        }
                    });
                    algorithm.HistoryProvider = _algorithmHandlers.HistoryProvider;

                    // initialize the default brokerage message handler
                    algorithm.BrokerageMessageHandler = new DefaultBrokerageMessageHandler(algorithm, job, _algorithmHandlers.Results, _systemHandlers.Api);

                    //Initialize the internal state of algorithm and job: executes the algorithm.Initialize() method.
                    initializeComplete = _algorithmHandlers.Setup.Setup(algorithm, brokerage, job, _algorithmHandlers.Results, _algorithmHandlers.Transactions, _algorithmHandlers.RealTime);

                    // set this again now that we've actually added securities
                    _algorithmHandlers.Results.SetAlgorithm(algorithm);

                    //If there are any reasons it failed, pass these back to the IDE.
                    if( !initializeComplete || algorithm.ErrorMessages.Count > 0 || _algorithmHandlers.Setup.Errors.Count > 0) {
                        initializeComplete = false;
                        //Get all the error messages: internal in algorithm and external in setup handler.
                        errorMessage = String.Join( ",", algorithm.ErrorMessages);
                        errorMessage += String.Join( ",", _algorithmHandlers.Setup.Errors);
                        Log.Error( "Engine.Run(): " + errorMessage);
                        _algorithmHandlers.Results.RuntimeError(errorMessage);
                        _systemHandlers.Api.SetAlgorithmStatus(job.AlgorithmId, AlgorithmStatus.RuntimeError, errorMessage);
                    }
                }
                catch (Exception err) {
                    Log.Error(err);
                    runtimeMessage = "Algorithm.Initialize() Error: " + err.Message + " Stack Trace: " + err.StackTrace;
                    _algorithmHandlers.Results.RuntimeError(runtimeMessage, err.StackTrace);
                    _systemHandlers.Api.SetAlgorithmStatus(job.AlgorithmId, AlgorithmStatus.RuntimeError, runtimeMessage);
                }

                //-> Using the job + initialization: load the designated handlers:
                if( initializeComplete) {
                    //-> Reset the backtest stopwatch; we're now running the algorithm.
                    startTime = DateTime.Now;

                    //Set algorithm as locked; set it to live mode if we're trading live, and set it to locked for no further updates.
                    algorithm.SetAlgorithmId(job.AlgorithmId);
                    algorithm.SetLocked();

                    //Load the associated handlers for transaction and realtime events:
                    _algorithmHandlers.Transactions.Initialize(algorithm, brokerage, _algorithmHandlers.Results);
                    _algorithmHandlers.RealTime.Setup(algorithm, job, _algorithmHandlers.Results, _systemHandlers.Api);

                    // wire up the brokerage message handler
                    brokerage.Message += (sender, message) =>
                    {
                        algorithm.BrokerageMessageHandler.Handle(message);

                        // fire brokerage message events
                        algorithm.OnBrokerageMessage(message);
                        switch (message.Type) {
                            case BrokerageMessageType.Disconnect:
                                algorithm.OnBrokerageDisconnect();
                                break;
                            case BrokerageMessageType.Reconnect:
                                algorithm.OnBrokerageReconnect();
                                break;
                        }
                    };

                    //Send status to user the algorithm is now executing.
                    _algorithmHandlers.Results.SendStatusUpdate(AlgorithmStatus.Running);

                    //Launch the data, transaction and realtime handlers into dedicated threads
                    threadFeed = new Thread(_algorithmHandlers.DataFeed.Run) {Name = "DataFeed Thread"};
                    threadTransactions = new Thread(_algorithmHandlers.Transactions.Run) {Name = "Transaction Thread"};
                    threadRealTime = new Thread(_algorithmHandlers.RealTime.Run) {Name = "RealTime Thread"};

                    //Launch the data feed, result sending, and transaction models/handlers in separate threads.
                    threadFeed.Start(); // Data feed pushing data packets into thread bridge; 
                    threadTransactions.Start(); // Transaction modeller scanning new order requests
                    threadRealTime.Start(); // RealTime scan time for time based events:

                    // Result manager scanning message queue: (started earlier)
                    _algorithmHandlers.Results.DebugMessage( String.format( "Launching analysis for %1$s with LEAN Engine v%2$s", job.AlgorithmId, Globals.Version));

                    try
                    {
                        //Create a new engine isolator class 
                        isolator = new Isolator();

                        // Execute the Algorithm Code:
                        complete = isolator.ExecuteWithTimeLimit(_algorithmHandlers.Setup.MaximumRuntime, algorithmManager.TimeLoopWithinLimits, () =>
                        {
                            try
                            {
                                //Run Algorithm Job:
                                // -> Using this Data Feed, 
                                // -> Send Orders to this TransactionHandler, 
                                // -> Send Results to ResultHandler.
                                algorithmManager.Run(job, algorithm, _algorithmHandlers.DataFeed, _algorithmHandlers.Transactions, _algorithmHandlers.Results, _algorithmHandlers.RealTime, _algorithmHandlers.CommandQueue, isolator.CancellationToken);
                            }
                            catch (Exception err) {
                                //Debugging at this level is difficult, stack trace needed.
                                Log.Error(err);
                                algorithm.RunTimeError = err;
                                algorithmManager.SetStatus(AlgorithmStatus.RuntimeError);
                                return;
                            }

                            Log.Trace( "Engine.Run(): Exiting Algorithm Manager");
                        }, job.RamAllocation);

                        if( !complete) {
                            Log.Error( "Engine.Main(): Failed to complete in time: " + _algorithmHandlers.Setup.MaximumRuntime.toString( "F"));
                            throw new Exception( "Failed to complete algorithm within " + _algorithmHandlers.Setup.MaximumRuntime.toString( "F")
                                + " seconds. Please make it run faster.");
                        }

                        // Algorithm runtime error:
                        if( algorithm.RunTimeError != null ) {
                            throw algorithm.RunTimeError;
                        }
                    }
                    catch (Exception err) {
                        //Error running the user algorithm: purge datafeed, send error messages, set algorithm status to failed.
                        Log.Error(err, "Breaking out of parent try catch:");
                        if( _algorithmHandlers.DataFeed != null ) _algorithmHandlers.DataFeed.Exit();
                        if( _algorithmHandlers.Results != null ) {
                            message = "Runtime Error: " + err.Message;
                            Log.Trace( "Engine.Run(): Sending runtime error to user...");
                            _algorithmHandlers.Results.LogMessage(message);
                            _algorithmHandlers.Results.RuntimeError(message, err.StackTrace);
                            _systemHandlers.Api.SetAlgorithmStatus(job.AlgorithmId, AlgorithmStatus.RuntimeError, message + " Stack Trace: " + err.StackTrace);
                        }
                    }

                    try
                    {
                        trades = algorithm.TradeBuilder.ClosedTrades;
                        charts = new Map<String, Chart>(_algorithmHandlers.Results.Charts);
                        orders = new Map<Integer, Order>(_algorithmHandlers.Transactions.Orders);
                        holdings = new Map<String, Holding>();
                        banner = new Map<String,String>();
                        statisticsResults = new StatisticsResults();

                        try
                        {
                            //Generates error when things don't exist (no charting logged, runtime errors in main algo execution)
                            static final String strategyEquityKey = "Strategy Equity";
                            static final String equityKey = "Equity";
                            static final String dailyPerformanceKey = "Daily Performance";
                            static final String benchmarkKey = "Benchmark";

                            // make sure we've taken samples for these series before just blindly requesting them
                            if( charts.ContainsKey(strategyEquityKey) &&
                                charts[strategyEquityKey].Series.ContainsKey(equityKey) &&
                                charts[strategyEquityKey].Series.ContainsKey(dailyPerformanceKey)) {
                                equity = charts[strategyEquityKey].Series[equityKey].Values;
                                performance = charts[strategyEquityKey].Series[dailyPerformanceKey].Values;
                                profitLoss = new SortedMap<DateTime, decimal>(algorithm.Transactions.TransactionRecord);
                                totalTransactions = algorithm.Transactions.GetOrders(x -> x.Status.IsFill()).Count();
                                benchmark = charts[benchmarkKey].Series[benchmarkKey].Values;

                                statisticsResults = StatisticsBuilder.Generate(trades, profitLoss, equity, performance, benchmark,
                                    _algorithmHandlers.Setup.StartingPortfolioValue, algorithm.Portfolio.TotalFees, totalTransactions);

                                //Some users have $0 in their brokerage account / starting cash of $0. Prevent divide by zero errors
                                netReturn = _algorithmHandlers.Setup.StartingPortfolioValue > 0 ?
                                                (algorithm.Portfolio.TotalPortfolioValue - _algorithmHandlers.Setup.StartingPortfolioValue) / _algorithmHandlers.Setup.StartingPortfolioValue
                                                : 0;

                                //Add other fixed parameters.
                                banner.Add( "Unrealized", "$" + algorithm.Portfolio.TotalUnrealizedProfit.toString( "N2"));
                                banner.Add( "Fees", "-$" + algorithm.Portfolio.TotalFees.toString( "N2"));
                                banner.Add( "Net Profit", "$" + algorithm.Portfolio.TotalProfit.toString( "N2"));
                                banner.Add( "Return", netReturn.toString( "P"));
                                banner.Add( "Equity", "$" + algorithm.Portfolio.TotalPortfolioValue.toString( "N2"));
                            }
                        }
                        catch (Exception err) {
                            Log.Error(err, "Error generating statistics packet");
                        }

                        //Diagnostics Completed, Send Result Packet:
                        totalSeconds = (DateTime.Now - startTime).TotalSeconds;
                        dataPoints = algorithmManager.DataPoints + _algorithmHandlers.HistoryProvider.DataPointCount;
                        _algorithmHandlers.Results.DebugMessage(
                            String.format( "Algorithm Id:(%1$s) completed in %2$s seconds at %3$sk data points per second. Processing total of {3} data points.",
                                job.AlgorithmId, totalSeconds.toString( "F2"), ((dataPoints/(double) 1000)/totalSeconds).toString( "F0"),
                                dataPoints.toString( "N0")));

                        _algorithmHandlers.Results.SendFinalResult(job, orders, algorithm.Transactions.TransactionRecord, holdings, statisticsResults, banner);
                    }
                    catch (Exception err) {
                        Log.Error(err, "Error sending analysis results");
                    }

                    //Before we return, send terminate commands to close up the threads
                    _algorithmHandlers.Transactions.Exit();
                    _algorithmHandlers.DataFeed.Exit();
                    _algorithmHandlers.RealTime.Exit();
                }

                //Close result handler:
                _algorithmHandlers.Results.Exit();
                statusPing.Exit();

                //Wait for the threads to complete:
                ts = Stopwatch.StartNew();
                while ((_algorithmHandlers.Results.IsActive 
                    || (_algorithmHandlers.Transactions != null && _algorithmHandlers.Transactions.IsActive) 
                    || (_algorithmHandlers.DataFeed != null && _algorithmHandlers.DataFeed.IsActive)
                    || (_algorithmHandlers.RealTime != null && _algorithmHandlers.RealTime.IsActive))
                    && ts.ElapsedMilliseconds < 30*1000) {
                    Thread.Sleep(100);
                    Log.Trace( "Waiting for threads to exit...");
                }

                //Terminate threads still in active state.
                if( threadFeed != null && threadFeed.IsAlive) threadFeed.Abort();
                if( threadTransactions != null && threadTransactions.IsAlive) threadTransactions.Abort();
                if( threadResults != null && threadResults.IsAlive) threadResults.Abort();
                if( statusPingThread != null && statusPingThread.IsAlive) statusPingThread.Abort();

                if( brokerage != null ) {
                    Log.Trace( "Engine.Run(): Disconnecting from brokerage...");
                    brokerage.Disconnect();
                }
                if( _algorithmHandlers.Setup != null ) {
                    Log.Trace( "Engine.Run(): Disposing of setup handler...");
                    _algorithmHandlers.Setup.Dispose();
                }
                Log.Trace( "Engine.Main(): Analysis Completed and Results Posted.");
            }
            catch (Exception err) {
                Log.Error(err, "Error running algorithm");
            }
            finally
            {
                //No matter what for live mode; make sure we've set algorithm status in the API for "not running" conditions:
                if( _liveMode && algorithmManager.State != AlgorithmStatus.Running && algorithmManager.State != AlgorithmStatus.RuntimeError)
                    _systemHandlers.Api.SetAlgorithmStatus(job.AlgorithmId, algorithmManager.State);

                _algorithmHandlers.Results.Exit();
                _algorithmHandlers.DataFeed.Exit();
                _algorithmHandlers.Transactions.Exit();
                _algorithmHandlers.RealTime.Exit();
            }
        }
    } // End Algorithm Node Core Thread
} // End Namespace
