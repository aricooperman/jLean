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
using System.Linq;

package com.quantconnect.lean.Indicators
{
    /**
    /// This indicator computes the n-period mean absolute deviation.
    */
    public class MeanAbsoluteDeviation : WindowIndicator<IndicatorDataPoint>
    {
        /**
        /// Gets the mean used to compute the deviation
        */
        public IndicatorBase<IndicatorDataPoint> Mean { get; private set; }

        /**
        /// Initializes a new instance of the MeanAbsoluteDeviation class with the specified period.
        ///
        /// Evaluates the mean absolute deviation of samples in the lookback period.
        */
         * @param period">The sample size of the standard deviation
        public MeanAbsoluteDeviation(int period)
            : this( "MAD" + period, period) {
        }

        /**
        /// Initializes a new instance of the MeanAbsoluteDeviation class with the specified period.
        ///
        /// Evaluates the mean absolute deviation of samples in the lookback period.
        */
         * @param name">The name of this indicator
         * @param period">The sample size of the mean absoluate deviation
        public MeanAbsoluteDeviation( String name, int period)
            : base(name, period) {
            Mean = MovingAverageType.Simple.AsIndicator( String.format( "%1$s_%2$s", name, "Mean"), period);
        }

        /**
        /// Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= Period; }
        }

        /**
        /// Computes the next value of this indicator from the given state
        */
         * @param input">The input given to the indicator
         * @param window">The window for the input history
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            Mean.Update(input);
            if( Samples < 2) {
                return 0m;
            }
            return window.Average(v -> Math.Abs(v - Mean.Current.Value));
        }

        /**
        /// Resets this indicator and its sub-indicator Mean to their initial state
        */
        public @Override void Reset() {
            Mean.Reset();
            base.Reset();
        }
    }
}
