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
using System.Collections.Generic;

package com.quantconnect.lean.Statistics
{
    /**
     * The <see cref="AlgorithmPerformance"/> class is a wrapper for <see cref="TradeStatistics"/> and <see cref="PortfolioStatistics"/>
    */
    public class AlgorithmPerformance
    {
        /**
         * The algorithm statistics on closed trades
        */
        public TradeStatistics TradeStatistics { get; private set; }

        /**
         * The algorithm statistics on portfolio
        */
        public PortfolioStatistics PortfolioStatistics { get; private set; }

        /**
         * The list of closed trades
        */
        public List<Trade> ClosedTrades { get; private set; }

        /**
         * Initializes a new instance of the <see cref="AlgorithmPerformance"/> class
        */
         * @param trades The list of closed trades
         * @param profitLoss Trade record of profits and losses
         * @param equity The list of daily equity values
         * @param listPerformance The list of algorithm performance values
         * @param listBenchmark The list of benchmark values
         * @param startingCapital The algorithm starting capital
        public AlgorithmPerformance(
            List<Trade> trades,
            SortedMap<DateTime, decimal> profitLoss,
            SortedMap<DateTime, decimal> equity,
            List<double> listPerformance,
            List<double> listBenchmark, 
            BigDecimal startingCapital) {
            TradeStatistics = new TradeStatistics(trades);
            PortfolioStatistics = new PortfolioStatistics(profitLoss, equity, listPerformance, listBenchmark, startingCapital);
            ClosedTrades = trades;
        }

        /**
         * Initializes a new instance of the <see cref="AlgorithmPerformance"/> class
        */
        public AlgorithmPerformance() {
            TradeStatistics = new TradeStatistics();
            PortfolioStatistics = new PortfolioStatistics();
            ClosedTrades = new List<Trade>();
        }

    }
}
