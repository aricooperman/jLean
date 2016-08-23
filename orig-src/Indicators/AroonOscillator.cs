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

using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators
{
    /**
    /// The Aroon Oscillator is the difference between AroonUp and AroonDown. The value of this
    /// indicator fluctuats between -100 and +100. An upward trend bias is present when the oscillator
    /// is positive, and a negative trend bias is present when the oscillator is negative. AroonUp/Down
    /// values over 75 identify strong trends in their respective direction.
    */
    public class AroonOscillator : TradeBarIndicator
    {
        /**
        /// Gets the AroonUp indicator
        */
        public IndicatorBase<IndicatorDataPoint> AroonUp { get; private set; }

        /**
        /// Gets the AroonDown indicator
        */
        public IndicatorBase<IndicatorDataPoint> AroonDown { get; private set; }

        /**
        /// Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return AroonUp.IsReady && AroonDown.IsReady; }
        }

        /**
        /// Creates a new AroonOscillator from the specified up/down periods.
        */
         * @param upPeriod">The lookback period to determine the highest high for the AroonDown
         * @param downPeriod">The lookback period to determine the lowest low for the AroonUp
        public AroonOscillator(int upPeriod, int downPeriod)
            : this( String.format( "AROON(%1$s,%2$s)", upPeriod, downPeriod), upPeriod, downPeriod) {
        }

        /**
        /// Creates a new AroonOscillator from the specified up/down periods.
        */
         * @param name">The name of this indicator
         * @param upPeriod">The lookback period to determine the highest high for the AroonDown
         * @param downPeriod">The lookback period to determine the lowest low for the AroonUp
        public AroonOscillator( String name, int upPeriod, int downPeriod)
            : base(name) {
            max = new Maximum(name + "_Max", upPeriod + 1);
            AroonUp = new FunctionalIndicator<IndicatorDataPoint>(name + "_AroonUp",
                input -> ComputeAroonUp(upPeriod, max, input),
                aroonUp -> max.IsReady,
                () -> max.Reset()
                );

            min = new Minimum(name + "_Min", downPeriod + 1);
            AroonDown = new FunctionalIndicator<IndicatorDataPoint>(name + "_AroonDown",
                input -> ComputeAroonDown(downPeriod, min, input),
                aroonDown -> min.IsReady,
                () -> min.Reset()
                );
        }

        /**
        /// Computes the next value of this indicator from the given state
        */
         * @param input">The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            AroonUp.Update(input.Time, input.High);
            AroonDown.Update(input.Time, input.Low);

            return AroonUp - AroonDown;
        }

        /**
        /// AroonUp = 100 * (period - {periods since max})/period
        */
         * @param upPeriod">The AroonUp period
         * @param max">A Maximum indicator used to compute periods since max
         * @param input">The next input data
        @returns The AroonUp value
        private static BigDecimal ComputeAroonUp(int upPeriod, Maximum max, IndicatorDataPoint input) {
            max.Update(input);
            return 100m * (upPeriod - max.PeriodsSinceMaximum) / upPeriod;
        }

        /**
        /// AroonDown = 100 * (period - {periods since min})/period
        */
         * @param downPeriod">The AroonDown period
         * @param min">A Minimum indicator used to compute periods since min
         * @param input">The next input data
        @returns The AroonDown value
        private static BigDecimal ComputeAroonDown(int downPeriod, Minimum min, IndicatorDataPoint input) {
            min.Update(input);
            return 100m * (downPeriod - min.PeriodsSinceMinimum) / downPeriod;
        }

        /**
        /// Resets this indicator and both sub-indicators (AroonUp and AroonDown)
        */
        public @Override void Reset() {
            AroonUp.Reset();
            AroonDown.Reset();
            base.Reset();
        }
    }
}