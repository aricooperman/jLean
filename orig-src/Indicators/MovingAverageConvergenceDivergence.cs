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
     * This indicator creates two moving averages defined on a base indicator and produces the difference
     * between the fast and slow averages.
    */
    public class MovingAverageConvergenceDivergence : Indicator
    {
        /**
         * Gets the fast average indicator
        */
        public IndicatorBase<IndicatorDataPoint> Fast { get; private set; }

        /**
         * Gets the slow average indicator
        */
        public IndicatorBase<IndicatorDataPoint> Slow { get; private set; }

        /**
         * Gets the signal of the MACD
        */
        public IndicatorBase<IndicatorDataPoint> Signal { get; private set; }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Signal.IsReady; }
        }

        /**
         * Creates a new MACD with the specified parameters
        */
         * @param fastPeriod The fast moving average period
         * @param slowPeriod The slow moving average period
         * @param signalPeriod The signal period
         * @param type The type of moving averages to use
        public MovingAverageConvergenceDivergence(int fastPeriod, int slowPeriod, int signalPeriod, MovingAverageType type = MovingAverageType.Simple)
            : this( String.format( "MACD(%1$s,%2$s)", fastPeriod, slowPeriod), fastPeriod, slowPeriod, signalPeriod, type) {
        }

        /**
         * Creates a new MACD with the specified parameters
        */
         * @param name The name of this indicator
         * @param fastPeriod The fast moving average period
         * @param slowPeriod The slow moving average period
         * @param signalPeriod The signal period
         * @param type The type of moving averages to use
        public MovingAverageConvergenceDivergence( String name, int fastPeriod, int slowPeriod, int signalPeriod, MovingAverageType type = MovingAverageType.Simple)
            : base(name) {
            Fast = type.AsIndicator(name + "_Fast", fastPeriod);
            Slow = type.AsIndicator(name + "_Slow", slowPeriod);
            Signal = type.AsIndicator(name + "_Signal", signalPeriod);
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IndicatorDataPoint input) {
            Fast.Update(input);
            Slow.Update(input);

            macd = Fast - Slow;
            if( Fast.IsReady && Slow.IsReady) {
                Signal.Update(input.Time, macd);
            }

            return macd;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            Fast.Reset();
            Slow.Reset();
            Signal.Reset();

            base.Reset();
        }
    }
}