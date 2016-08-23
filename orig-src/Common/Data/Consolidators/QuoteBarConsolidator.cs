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
    /// Consolidates quotebars into larger quotebars
    */
    public class QuoteBarConsolidator : PeriodCountConsolidatorBase<QuoteBar, QuoteBar>
    {
        /**
        /// Initializes a new instance of the <see cref="TickQuoteBarConsolidator"/> class
        */
         * @param period">The minimum span of time before emitting a consolidated bar
        public QuoteBarConsolidator(TimeSpan period)
            : base(period) {
        }

        /**
        /// Initializes a new instance of the <see cref="TickQuoteBarConsolidator"/> class
        */
         * @param maxCount">The number of pieces to accept before emiting a consolidated bar
        public QuoteBarConsolidator(int maxCount)
            : base(maxCount) {
        }

        /**
        /// Initializes a new instance of the <see cref="TickQuoteBarConsolidator"/> class
        */
         * @param maxCount">The number of pieces to accept before emiting a consolidated bar
         * @param period">The minimum span of time before emitting a consolidated bar
        public QuoteBarConsolidator(int maxCount, Duration period)
            : base(maxCount, period) {
        }

        /**
        /// Aggregates the new 'data' into the 'workingBar'. The 'workingBar' will be
        /// null following the event firing
        */
         * @param workingBar">The bar we're building, null if the event was just fired and we're starting a new consolidated bar
         * @param data">The new data
        protected @Override void AggregateBar(ref QuoteBar workingBar, QuoteBar data) {
            bid = data.Bid;
            ask = data.Ask;

            if( workingBar == null ) {
                workingBar = new QuoteBar
                {
                    Symbol = data.Symbol,
                    Time = GetRoundedBarTime(data.Time),
                    Bid = bid == null ? null : bid.Clone(),
                    Ask = ask == null ? null : ask.Clone()
                };
            }

            // update the bid and ask
            if( bid != null ) {
                workingBar.LastBidSize = data.LastBidSize;
                if( workingBar.Bid == null ) {
                    workingBar.Bid = new Bar(bid.Open, bid.High, bid.Low, bid.Close);
                }
                else
                {
                    workingBar.Bid.Close = bid.Close;
                    if( workingBar.Bid.High < bid.High) workingBar.Bid.High = bid.High;
                    if( workingBar.Bid.Low > bid.Low) workingBar.Bid.Low = bid.Low;
                }
            }
            if( ask != null ) {
                workingBar.LastAskSize = data.LastAskSize;
                if( workingBar.Ask == null ) {
                    workingBar.Ask = new Bar(ask.Open, ask.High, ask.Low, ask.Close);
                }
                else
                {
                    workingBar.Ask.Close = ask.Close;
                    if( workingBar.Ask.High < ask.High) workingBar.Ask.High = ask.High;
                    if( workingBar.Ask.Low > ask.Low) workingBar.Ask.Low = ask.Low;
                }
            }
        }
    }
}