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

package com.quantconnect.lean.Indicators
{
    /**
     * This indicator computes the n-period population variance.
    */
    public class Variance : WindowIndicator<IndicatorDataPoint>
    {
        private BigDecimal _rollingSum;
        private BigDecimal _rollingSumOfSquares;

        /**
         * Initializes a new instance of the <see cref="Variance"/> class using the specified period.
        */ 
         * @param period The period of the indicator
        public Variance(int period)
            : this( "VAR" + period, period) {
        }

        /**
         * Initializes a new instance of the <see cref="Variance"/> class using the specified name and period.
        */ 
         * @param name The name of this indicator
         * @param period The period of the indicator
        public Variance( String name, int period)
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
            _rollingSum += input.Value;
            _rollingSumOfSquares += input.Value * input.Value;

            if( Samples < 2)
                return BigDecimal.ZERO;

            n = Period;
            if( Samples < n)
                n = (int)Samples;

            meanValue1 = _rollingSum / n;
            meanValue2 = _rollingSumOfSquares / n;

            if( n == Period) {
                removedValue = window[Period - 1];
                _rollingSum -= removedValue;
                _rollingSumOfSquares -= removedValue * removedValue;
            }

            return meanValue2 - meanValue1 * meanValue1;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _rollingSum = 0;
            _rollingSumOfSquares = 0;
            base.Reset();
        }
    }
}
