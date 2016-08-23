﻿/*
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

package com.quantconnect.lean.Indicators
{
    /**
    /// This indicator computes the n-period change in a value using the following:
    /// value_0 - value_n
    */
    public class Momentum : WindowIndicator<IndicatorDataPoint>
    {
        /**
        /// Creates a new Momentum indicator with the specified period
        */
         * @param period">The period over which to perform to computation
        public Momentum(int period)
            : base( "MOM" + period, period) {
        }

        /**
        /// Creates a new Momentum indicator with the specified period
        */
         * @param name">The name of this indicator
         * @param period">The period over which to perform to computation
        public Momentum( String name, int period)
            : base(name, period) {
        }

        /**
        /// Computes the next value for this indicator from the given state.
        */
         * @param window">The window of data held in this indicator
         * @param input">The input value to this indicator on this time step
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            if( !window.IsReady) {
                // keep returning the delta from the first item put in there to init
                return input - window[window.Count - 1];
            }

            return input - window.MostRecentlyRemoved;
        }
    }
}