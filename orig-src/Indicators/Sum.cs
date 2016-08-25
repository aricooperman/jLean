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


package com.quantconnect.lean.Indicators {
    /**
     * Represents an indictor capable of tracking the sum for the given period
    */
    public class Sum : WindowIndicator<IndicatorDataPoint> {
        /**The sum for the given period</summary>
        private BigDecimal _sum;

        /**
         *     Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady {
            get { return Samples >= Period; }
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _sum = 0.0m;
            base.Reset();
        }

        /**
         *     Initializes a new instance of the Sum class with the specified name and period
        */
         * @param name The name of this indicator
         * @param period The period of the SMA
        public Sum( String name, int period)
            : base(name, period) {
        }

        /**
         *     Initializes a new instance of the Sum class with the default name and period
        */
         * @param period The period of the SMA
        public Sum(int period)
            : this( "SUM" + period, period) {
        }

        /**
         *     Computes the next value for this indicator from the given state.
        */
         * @param window The window of data held in this indicator
         * @param input The input value to this indicator on this time step
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            _sum += input.Value;
            if( window.IsReady) {
                _sum -= window.MostRecentlyRemoved.Value;
            }
            return _sum;
        }
    }
}
