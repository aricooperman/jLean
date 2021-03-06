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
using QuantConnect.Data.Market;

package com.quantconnect.lean.Data.Consolidators
{    
    /**
     * A data consolidator that can make bigger bars from ticks over a given
     * time span or a count of pieces of data.
    */
    public class TickConsolidator : TradeBarConsolidatorBase<Tick>
    {
        /**
         * Creates a consolidator to produce a new 'TradeBar' representing the period
        */
         * @param period The minimum span of time before emitting a consolidated bar
        public TickConsolidator(TimeSpan period)
            : base(period) {
        }

        /**
         * Creates a consolidator to produce a new 'TradeBar' representing the last count pieces of data
        */
         * @param maxCount The number of pieces to accept before emiting a consolidated bar
        public TickConsolidator(int maxCount)
            : base(maxCount) {
        }

        /**
         * Creates a consolidator to produce a new 'TradeBar' representing the last count pieces of data or the period, whichever comes first
        */
         * @param maxCount The number of pieces to accept before emiting a consolidated bar
         * @param period The minimum span of time before emitting a consolidated bar
        public TickConsolidator(int maxCount, Duration period)
            : base(maxCount, period) {
        }

        /**
         * Aggregates the new 'data' into the 'workingBar'. The 'workingBar' will be
         * null following the event firing
        */
         * @param workingBar The bar we're building
         * @param data The new data
        protected @Override void AggregateBar(ref TradeBar workingBar, Tick data) {
            if( workingBar == null ) {
                workingBar = new TradeBar
                {
                    Symbol = data.Symbol,
                    Time = GetRoundedBarTime(data.Time),
                    Close = data.Value,
                    High = data.Value,
                    Low = data.Value,
                    Open = data.Value,
                    DataType = data.DataType,
                    Value = data.Value,
                    Volume = data.Quantity
                };
            }
            else
            {
                //Aggregate the working bar
                workingBar.Close = data.Value;
                workingBar.Volume += data.Quantity;
                if( data.Value < workingBar.Low) workingBar.Low = data.Value;
                if( data.Value > workingBar.High) workingBar.High = data.Value;
            }
        }
    }
}