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
using QuantConnect.Data.Market;

package com.quantconnect.lean.Data.Consolidators
{
    /**
     * Consolidates ticks into quote bars. This consolidator ignores trade ticks
    */
    public class TickQuoteBarConsolidator : PeriodCountConsolidatorBase<Tick, QuoteBar>
    {
        /**
         * Initializes a new instance of the <see cref="TickQuoteBarConsolidator"/> class
        */
         * @param period The minimum span of time before emitting a consolidated bar
        public TickQuoteBarConsolidator(TimeSpan period)
            : base(period) {
        }

        /**
         * Initializes a new instance of the <see cref="TickQuoteBarConsolidator"/> class
        */
         * @param maxCount The number of pieces to accept before emiting a consolidated bar
        public TickQuoteBarConsolidator(int maxCount)
            : base(maxCount) {
        }

        /**
         * Initializes a new instance of the <see cref="TickQuoteBarConsolidator"/> class
        */
         * @param maxCount The number of pieces to accept before emiting a consolidated bar
         * @param period The minimum span of time before emitting a consolidated bar
        public TickQuoteBarConsolidator(int maxCount, Duration period)
            : base(maxCount, period) {
        }
        
        /**
         * Determines whether or not the specified data should be processd
        */
         * @param data The data to check
        @returns True if the consolidator should process this data, false otherwise
        protected @Override boolean ShouldProcess(Tick data) {
            return data.TickType == TickType.Quote;
        }

        /**
         * Aggregates the new 'data' into the 'workingBar'. The 'workingBar' will be
         * null following the event firing
        */
         * @param workingBar The bar we're building, null if the event was just fired and we're starting a new consolidated bar
         * @param data The new data
        protected @Override void AggregateBar(ref QuoteBar workingBar, Tick data) {
            if( workingBar == null ) {
                workingBar = new QuoteBar
                {
                    Symbol = data.Symbol,
                    Time = GetRoundedBarTime(data.Time),
                    Bid = null,
                    Ask = null
                };
            }

            // update the bid and ask
            workingBar.Update(0, data.BidPrice, data.AskPrice, 0, data.BidSize, data.AskSize);
        }
    }
}