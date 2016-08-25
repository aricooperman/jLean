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

package com.quantconnect.lean.Indicators
{
    /**
     * The AverageTrueRange indicator is a measure of volatility introduced by Welles Wilder in his
     * book: New Concepts in Technical Trading Systems.  This indicator computes the TrueRange and then
     * smoothes the TrueRange over a given period.
     * 
     * TrueRange is defined as the maximum of the following:
     *   High - Low
     *   ABS(High - PreviousClose)
     *   ABS(Low  - PreviousClose)
    */
    public class AverageTrueRange : TradeBarIndicator
    {
        /**This indicator is used to smooth the TrueRange computation</summary>
         * This is not exposed publicly since it is the same value as this indicator, meaning
         * that this '_smoother' computers the ATR directly, so exposing it publicly would be duplication
        private final IndicatorBase<IndicatorDataPoint> _smoother;

        /**
         * Gets the true range which is the more volatile calculation to be smoothed by this indicator
        */
        public IndicatorBase<TradeBar> TrueRange { get; private set; } 

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return _smoother.IsReady; }
        }

        /**
         * Creates a new AverageTrueRange indicator using the specified period and moving average type
        */
         * @param name The name of this indicator
         * @param period The smoothing period used to smooth the true range values
         * @param movingAverageType The type of smoothing used to smooth the true range values
        public AverageTrueRange( String name, int period, MovingAverageType movingAverageType = MovingAverageType.Wilders)
            : base(name) {
            _smoother = movingAverageType.AsIndicator( String.format( "%1$s_%2$s", name, movingAverageType), period);

            TradeBar previous = null;
            TrueRange = new FunctionalIndicator<TradeBar>(name + "_TrueRange", currentBar =>
            {
                // in our ComputeNextValue function we'll just call the ComputeTrueRange
                nextValue = ComputeTrueRange(previous, currentBar);
                previous = currentBar;
                return nextValue;
            }   // in our IsReady function we just need at least one sample
            , trueRangeIndicator -> trueRangeIndicator.Samples >= 1
            );
        }

        /**
         * Creates a new AverageTrueRange indicator using the specified period and moving average type
        */
         * @param period The smoothing period used to smooth the true range values
         * @param movingAverageType The type of smoothing used to smooth the true range values
        public AverageTrueRange(int period, MovingAverageType movingAverageType = MovingAverageType.Wilders)
            : this( "ATR" + period, period, movingAverageType) {
        }

        /**
         * Computes the TrueRange from the current and previous trade bars
         * 
         * TrueRange is defined as the maximum of the following:
         *   High - Low
         *   ABS(High - PreviousClose)
         *   ABS(Low  - PreviousClose)
        */
         * @param previous The previous trade bar
         * @param current The current trade bar
        @returns The true range
        public static BigDecimal ComputeTrueRange(TradeBar previous, TradeBar current) {
            range1 = current.High - current.Low;
            if( previous == null ) {
                return range1;
            }

            range2 = Math.Abs(current.High - previous.Close);
            range3 = Math.Abs(current.Low - previous.Close);

            return Math.Max(range1, Math.Max(range2, range3));
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            // compute the true range and then send it to our smoother
            TrueRange.Update(input);
            _smoother.Update(input.Time, TrueRange);

            return _smoother.Current.Value;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _smoother.Reset();
            TrueRange.Reset();
            base.Reset();
        }
    }
}