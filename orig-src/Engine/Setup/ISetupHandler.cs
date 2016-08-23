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
using System.ComponentModel.Composition;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.RealTime;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Packets;

package com.quantconnect.lean.Lean.Engine.Setup
{
    /**
    /// Interface to setup the algorithm. Pass in a raw algorithm, return one with portfolio, cash, etc already preset.
    */
    [InheritedExport(typeof(ISetupHandler))]
    public interface ISetupHandler : IDisposable
    {
        /**
        /// Any errors from the initialization stored here:
        */
        List<String> Errors 
        { 
            get; 
            set; 
        }

        /**
        /// Get the maximum runtime for this algorithm job.
        */
        Duration MaximumRuntime
        {
            get;
        }

        /**
        /// Algorithm starting capital for statistics calculations
        */
        BigDecimal StartingPortfolioValue
        {
            get;
        }

        /**
        /// Start date for analysis loops to search for data.
        */
        DateTime StartingDate
        {
            get;
        }

        /**
        /// Maximum number of orders for the algorithm run -- applicable for backtests only.
        */
        int MaxOrders
        {
            get;
        }

        /**
        /// Create a new instance of an algorithm from a physical dll path.
        */
         * @param assemblyPath">The path to the assembly's location
         * @param language">Language of the assembly.
        @returns A new instance of IAlgorithm, or throws an exception if there was an error
        IAlgorithm CreateAlgorithmInstance( String assemblyPath, Language language);

        /**
        /// Creates the brokerage as specified by the job packet
        */
         * @param algorithmNodePacket">Job packet
         * @param uninitializedAlgorithm">The algorithm instance before Initialize has been called
        @returns The brokerage instance, or throws if error creating instance
        IBrokerage CreateBrokerage(AlgorithmNodePacket algorithmNodePacket, IAlgorithm uninitializedAlgorithm);

        /**
        /// Primary entry point to setup a new algorithm
        */
         * @param algorithm">Algorithm instance
         * @param brokerage">New brokerage output instance
         * @param job">Algorithm job task
         * @param resultHandler">The configured result handler
         * @param transactionHandler">The configurated transaction handler
         * @param realTimeHandler">The configured real time handler
        @returns True on successfully setting up the algorithm state, or false on error.
        boolean Setup(IAlgorithm algorithm, IBrokerage brokerage, AlgorithmNodePacket job, IResultHandler resultHandler, ITransactionHandler transactionHandler, IRealTimeHandler realTimeHandler);
    }
}
