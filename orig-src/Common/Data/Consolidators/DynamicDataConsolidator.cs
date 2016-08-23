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
    /// A data csolidator that can make trade bars from DynamicData derived types. This is useful for
    /// aggregating Quandl and other highly flexible dynamic custom data types.
    */
    public class DynamicDataConsolidator : TradeBarConsolidatorBase<DynamicData>
    {
        /**
        /// Creates a consolidator to produce a new 'TradeBar' representing the period.
        */
         * @param period">The minimum span of time before emitting a consolidated bar
        public DynamicDataConsolidator(TimeSpan period)
            : base(period) {
        }

        /**
        /// Creates a consolidator to produce a new 'TradeBar' representing the last count pieces of data.
        */
         * @param maxCount">The number of pieces to accept before emiting a consolidated bar
        public DynamicDataConsolidator(int maxCount)
            : base(maxCount) {
        }

        /**
        /// Creates a consolidator to produce a new 'TradeBar' representing the last count pieces of data or the period, whichever comes first.
        */
         * @param maxCount">The number of pieces to accept before emiting a consolidated bar
         * @param period">The minimum span of time before emitting a consolidated bar
        public DynamicDataConsolidator(int maxCount, Duration period)
            : base(maxCount, period) {
        }

        /**
        /// Aggregates the new 'data' into the 'workingBar'. The 'workingBar' will be
        /// null following the event firing
        */
         * @param workingBar">The bar we're building, null if the event was just fired and we're starting a new trade bar
         * @param data">The new data
        protected @Override void AggregateBar(ref TradeBar workingBar, DynamicData data) {
            // grab the properties, if they don't exist just use the .Value property
            open = GetNamedPropertyOrValueProperty(data, "Open");
            high = GetNamedPropertyOrValueProperty(data, "High");
            low = GetNamedPropertyOrValueProperty(data, "Low");
            close = GetNamedPropertyOrValueProperty(data, "Close");

            // if we have volume, use it, otherwise just use zero
            volume = data.HasProperty( "Volume")
                ? (long) Convert.ChangeType(data.GetProperty( "Volume"), typeof (long))
                : 0L;
            
            if( workingBar == null ) {
                workingBar = new TradeBar
                {
                    Symbol = data.Symbol,
                    Time = GetRoundedBarTime(data.Time),
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    Volume = volume
                };
            }
            else
            {
                //Aggregate the working bar
                workingBar.Close = close;
                workingBar.Volume += volume;
                if( low < workingBar.Low) workingBar.Low = low;
                if( high > workingBar.High) workingBar.High = high;
            }
        }

        private static BigDecimal GetNamedPropertyOrValueProperty(DynamicData data, String propertyName) {
            if( !data.HasProperty(propertyName)) {
                return data.Value;
            }
            return (decimal) Convert.ChangeType(data.GetProperty(propertyName), typeof (decimal));
        }
    }
}