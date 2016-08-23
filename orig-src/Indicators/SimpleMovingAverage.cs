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
    ///     Represents the traditional simple moving average indicator (SMA)
    */
    public class SimpleMovingAverage : WindowIndicator<IndicatorDataPoint>
    {
        /**A rolling sum for computing the average for the given period</summary>
        public IndicatorBase<IndicatorDataPoint> RollingSum { get; private set; }

        /**
        ///     Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady{
            get { return RollingSum.IsReady; }
        }

        /**
        /// Resets this indicator to its initial state
        */
        public @Override void Reset() {
            RollingSum.Reset();
            base.Reset();
        }

        /**
        ///     Initializes a new instance of the SimpleMovingAverage class with the specified name and period
        */
         * @param name">The name of this indicator
         * @param period">The period of the SMA
        public SimpleMovingAverage( String name, int period)
            : base(name, period) {
            RollingSum = new Sum(name + "_Sum", period);
        }

        /**
        ///     Initializes a new instance of the SimpleMovingAverage class with the default name and period
        */
         * @param period">The period of the SMA
        public SimpleMovingAverage(int period)
            : this( "SMA" + period, period) {
        }

        /**
        ///     Computes the next value for this indicator from the given state.
        */
         * @param window">The window of data held in this indicator
         * @param input">The input value to this indicator on this time step
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            RollingSum.Update(input.Time, input.Value);
            return RollingSum.Current.Value / window.Count;
        }
    }
}