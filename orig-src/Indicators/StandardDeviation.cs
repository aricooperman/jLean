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
     * This indicator computes the n-period population standard deviation.
    */
    public class StandardDeviation : Variance
    {
        /**
         * Initializes a new instance of the StandardDeviation class with the specified period.
         * 
         * Evaluates the standard deviation of samples in the lookback period. 
         * On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        */
         * @param period The sample size of the standard deviation
        public StandardDeviation(int period)
            : this( "STD" + period, period) {
        }

        /**
         * Initializes a new instance of the StandardDeviation class with the specified name and period.
         * 
         * Evaluates the standard deviation of samples in the lookback period. 
         * On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        */
         * @param name The name of this indicator
         * @param period The sample size of the standard deviation
        public StandardDeviation( String name, int period)
            : base(name, period) {
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= Period; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
         * @param window The window for the input history
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            return (decimal)Math.Sqrt((double)base.ComputeNextValue(window, input));
        }
    }
}
