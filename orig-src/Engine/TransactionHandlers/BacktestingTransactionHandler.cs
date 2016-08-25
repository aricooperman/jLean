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
using QuantConnect.Brokerages.Backtesting;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.Results;

package com.quantconnect.lean.Lean.Engine.TransactionHandlers
{
    /**
     * This transaction handler is used for processing transactions during backtests
    */
    public class BacktestingTransactionHandler : BrokerageTransactionHandler
    {
        // save off a strongly typed version of the brokerage
        private BacktestingBrokerage _brokerage;

        /**
         * Creates a new BacktestingTransactionHandler using the BacktestingBrokerage
        */
         * @param algorithm The algorithm instance
         * @param brokerage The BacktestingBrokerage
         * @param resultHandler">
        public @Override void Initialize(IAlgorithm algorithm, IBrokerage brokerage, IResultHandler resultHandler) {
            if( !(brokerage is BacktestingBrokerage)) {
                throw new IllegalArgumentException( "Brokerage must be of type BacktestingBrokerage for use wth the BacktestingTransactionHandler");
            }
            
            _brokerage = (BacktestingBrokerage) brokerage;

            base.Initialize(algorithm, brokerage, resultHandler);
        }

        /**
         * Processes all synchronous events that must take place before the next time loop for the algorithm
        */
        public @Override void ProcessSynchronousEvents() {
            base.ProcessSynchronousEvents();

            _brokerage.Scan();
        }

        /**
         * Processes asynchronous events on the transaction handler's thread
        */
        public @Override void ProcessAsynchronousEvents() {
            base.ProcessAsynchronousEvents();

            _brokerage.Scan();
        }
    }
}