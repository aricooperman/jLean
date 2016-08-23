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
using System.Linq;
using System.Timers;
using QuantConnect.Algorithm;
using QuantConnect.AlgorithmFactory;
using QuantConnect.Brokerages;
using QuantConnect.Brokerages.Backtesting;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.RealTime;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Logging;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.Setup
{
    /**
    /// Backtesting setup handler processes the algorithm initialize method and sets up the internal state of the algorithm class.
    */
    public class BacktestingSetupHandler : ISetupHandler
    {
        private Duration _maxRuntime = Duration.ofSeconds(300);
        private BigDecimal _startingCaptial = 0;
        private int _maxOrders = 0;
        private DateTime _startingDate = new DateTime(1998, 01, 01);

        /**
        /// Internal errors list from running the setup proceedures.
        */
        public List<String> Errors
        { 
            get; 
            set; 
        }

        /**
        /// Maximum runtime of the algorithm in seconds.
        */
        /// Maximum runtime is a formula based on the number and resolution of symbols requested, and the days backtesting
        public Duration MaximumRuntime
        {
            get 
            {
                return _maxRuntime;
            }
        }

        /**
        /// Starting capital according to the users initialize routine.
        */
        /// Set from the user code.
        /// <seealso cref="QCAlgorithm.SetCash(decimal)"/>
        public BigDecimal StartingPortfolioValue
        {
            get 
            {
                return _startingCaptial;
            }
        }

        /**
        /// Start date for analysis loops to search for data.
        */
        /// <seealso cref="QCAlgorithm.SetStartDate(DateTime)"/>
        public DateTime StartingDate
        {
            get
            {
                return _startingDate;
            }
        }

        /**
        /// Maximum number of orders for this backtest.
        */
        /// To stop algorithm flooding the backtesting system with hundreds of megabytes of order data we limit it to 100 per day
        public int MaxOrders
        {
            get 
            {
                return _maxOrders;
            }
        }

        /**
        /// Initialize the backtest setup handler.
        */
        public BacktestingSetupHandler() {
            Errors = new List<String>();
        }

        /**
        /// Creates a new algorithm instance. Verified there's only one defined in the assembly and requires
        /// instantiation to take less than 10 seconds
        */
         * @param assemblyPath">Physical location of the assembly.
         * @param language">Language of the DLL
        @returns Algorithm instance.
        public IAlgorithm CreateAlgorithmInstance( String assemblyPath, Language language) {
            String error;
            IAlgorithm algorithm;

            // limit load times to 10 seconds and force the assembly to have exactly one derived type
            loader = new Loader(language, Duration.ofSeconds(15), names -> names.SingleOrDefault());
            complete = loader.TryCreateAlgorithmInstanceWithIsolator(assemblyPath, out algorithm, out error);
            if( !complete) throw new Exception(error + " Try re-building algorithm.");

            return algorithm;
        }

        /**
        /// Creates a new <see cref="BacktestingBrokerage"/> instance
        */
         * @param algorithmNodePacket">Job packet
         * @param uninitializedAlgorithm">The algorithm instance before Initialize has been called
        @returns The brokerage instance, or throws if error creating instance
        public IBrokerage CreateBrokerage(AlgorithmNodePacket algorithmNodePacket, IAlgorithm uninitializedAlgorithm) {
            return new BacktestingBrokerage(uninitializedAlgorithm);
        }

        /**
        /// Setup the algorithm cash, dates and data subscriptions as desired.
        */
         * @param algorithm">Algorithm instance
         * @param brokerage">Brokerage instance
         * @param baseJob">Algorithm job
         * @param resultHandler">The configured result handler
         * @param transactionHandler">The configurated transaction handler
         * @param realTimeHandler">The configured real time handler
        @returns Boolean true on successfully initializing the algorithm
        public boolean Setup(IAlgorithm algorithm, IBrokerage brokerage, AlgorithmNodePacket baseJob, IResultHandler resultHandler, ITransactionHandler transactionHandler, IRealTimeHandler realTimeHandler) {
            job = baseJob as BacktestNodePacket;
            if( job == null ) {
                throw new ArgumentException( "Expected BacktestNodePacket but received " + baseJob.GetType().Name);
            }

            Log.Trace( String.format( "BacktestingSetupHandler.Setup(): Setting up job: Plan: %1$s, UID: %2$s, PID: %3$s, Version: {3}, Source: {4}", job.UserPlan, job.UserId, job.ProjectId, job.Version, job.RequestSource));

            if( algorithm == null ) {
                Errors.Add( "Could not create instance of algorithm");
                return false;
            }

            //Make sure the algorithm start date ok.
            if( job.PeriodStart == default(DateTime)) {
                Errors.Add( "Algorithm start date was never set");
                return false;
            }

            controls = job.Controls;
            isolator = new Isolator();
            initializeComplete = isolator.ExecuteWithTimeLimit(Duration.ofMinutes(5), () =>
            {
                try
                {
                    //Set our parameters
                    algorithm.SetParameters(job.Parameters);
                    //Algorithm is backtesting, not live:
                    algorithm.SetLiveMode(false);
                    //Set the algorithm time before we even initialize:
                    algorithm.SetDateTime(job.PeriodStart.ConvertToUtc(algorithm.TimeZone));
                    //Set the source impl for the event scheduling
                    algorithm.Schedule.SetEventSchedule(realTimeHandler);
                    //Initialise the algorithm, get the required data:
                    algorithm.Initialize();
                }
                catch (Exception err) {
                    Errors.Add( "Failed to initialize algorithm: Initialize(): " + err.Message);
                }
            });

            //Before continuing, detect if this is ready:
            if( !initializeComplete) return false;

            algorithm.Transactions.SetOrderProcessor(transactionHandler);
            algorithm.PostInitialize();

            //Calculate the max runtime for the strategy
            _maxRuntime = GetMaximumRuntime(job.PeriodStart, job.PeriodFinish, algorithm.SubscriptionManager.Count);

            //Get starting capital:
            _startingCaptial = algorithm.Portfolio.Cash;

            //Max Orders: 10k per backtest:
            if( job.UserPlan == UserPlan.Free) {
                _maxOrders = 10000;
            }
            else
            {
                _maxOrders = int.MaxValue;
                _maxRuntime += _maxRuntime;
            }

            //Set back to the algorithm,
            algorithm.SetMaximumOrders(_maxOrders);
            
            //Starting date of the algorithm:
            _startingDate = job.PeriodStart;

            //Put into log for debugging:
            Log.Trace( "SetUp Backtesting: User: " + job.UserId + " ProjectId: " + job.ProjectId + " AlgoId: " + job.AlgorithmId);
            Log.Trace( "Dates: Start: " + job.PeriodStart.ToShortDateString() + " End: " + job.PeriodFinish.ToShortDateString() + " Cash: " + _startingCaptial.toString( "C"));

            if( Errors.Count > 0) {
                initializeComplete = false;
            }
            return initializeComplete;
        }

        /**
        /// Calculate the maximum runtime for this algorithm job.
        */
         * @param start">State date of the algorithm
         * @param finish">End date of the algorithm
         * @param subscriptionCount">Number of data feeds the user has requested
        @returns Timespan maximum run period
        private Duration GetMaximumRuntime(DateTime start, DateTime finish, int subscriptionCount) {
            double maxRunTime = 0;
            jobDays = (finish - start).TotalDays;

            maxRunTime = 10 * subscriptionCount * jobDays;

            //Rationalize:
            if( (maxRunTime / 3600) > 12) {
                //12 hours maximum
                maxRunTime = 3600 * 12;
            }
            else if( maxRunTime < 60) {
                //If less than 60 seconds.
                maxRunTime = 60;
            }

            Log.Trace( "BacktestingSetupHandler.GetMaxRunTime(): Job Days: " + jobDays + " Max Runtime: " + Math.Round(maxRunTime / 60) + " min");

            //Override for windows:
            if( OS.IsWindows) {
                maxRunTime = 24 * 60 * 60;
            }

            return Duration.ofSeconds(maxRunTime);
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        /// <filterpriority>2</filterpriority>
        public void Dispose() {
            // nothing to clean up
        }
    } // End Result Handler Thread:

} // End Namespace
