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

package com.quantconnect.lean.Indicators
{
    /**
     *     Represents the LogReturn indicator (LOGR)
     *      - log returns are useful for identifying price convergence/divergence in a given period
     *      - logr = log (current price / last price in period)
    */
    public class LogReturn : WindowIndicator<IndicatorDataPoint>
    {
        /**
         *     Initializes a new instance of the LogReturn class with the specified name and period
        */
         * @param name The name of this indicator
         * @param period The period of the LOGR
        public LogReturn( String name, int period)
            : base(name, period) {
        }

        /**
         *     Initializes a new instance of the LogReturn class with the default name and period
        */
         * @param period The period of the SMA
        public LogReturn(int period)
            : base( "LOGR" + period, period) {
        }

        /**
         *     Computes the next value for this indicator from the given state.
         *      - logr = log (current price / last price in period)
        */
         * @param window The window of data held in this indicator
         * @param input The input value to this indicator on this time step
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            BigDecimal valuef = input;

            BigDecimal value0 = !window.IsReady
                ? window[window.Count - 1]
                : window.MostRecentlyRemoved;

            return (decimal)Math.Log((double)(valuef / value0));
        }
    }
}
