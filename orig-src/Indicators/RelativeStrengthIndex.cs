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
     * Represents the  Relative Strength Index (RSI) developed by K. Welles Wilder.
     * You can optionally specified a different moving average type to be used in the computation
    */
    public class RelativeStrengthIndex : Indicator
    {
        private IndicatorDataPoint previousInput;

        /**
         * Gets the type of indicator used to compute AverageGain and AverageLoss
        */
        public MovingAverageType MovingAverageType { get; private set; }

        /**
         * Gets the EMA for the down days
        */
        public IndicatorBase<IndicatorDataPoint> AverageLoss { get; private set; }

        /**
         * Gets the indicator for average gain
        */
        public IndicatorBase<IndicatorDataPoint> AverageGain { get; private set; }

        /**
         * Initializes a new instance of the RelativeStrengthIndex class with the specified name and period
        */
         * @param period The period used for up and down days
         * @param movingAverageType The type of moving average to be used for computing the average gain/loss values
        public RelativeStrengthIndex(int period, MovingAverageType movingAverageType = MovingAverageType.Wilders)
            : this( "RSI" + period, period, movingAverageType) {
        }

        /**
         * Initializes a new instance of the RelativeStrengthIndex class with the specified name and period
        */
         * @param name The name of this indicator
         * @param period The period used for up and down days
         * @param movingAverageType The type of moving average to be used for computing the average gain/loss values
        public RelativeStrengthIndex( String name, int period, MovingAverageType movingAverageType = MovingAverageType.Wilders)
            : base(name) {
            MovingAverageType = movingAverageType;
            AverageGain = movingAverageType.AsIndicator(name + "Up", period);
            AverageLoss = movingAverageType.AsIndicator(name + "Down", period);
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return AverageGain.IsReady && AverageLoss.IsReady; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IndicatorDataPoint input) {
            if( previousInput != null && input.Value >= previousInput.Value) {
                AverageGain.Update(input.Time, input.Value - previousInput.Value);
                AverageLoss.Update(input.Time, BigDecimal.ZERO);
            }
            else if( previousInput != null && input.Value < previousInput.Value) {
                AverageGain.Update(input.Time, BigDecimal.ZERO);
                AverageLoss.Update(input.Time, previousInput.Value - input.Value);
            }

            previousInput = input;
            if( AverageLoss == BigDecimal.ZERO) {
                // all up days is 100
                return 100m;
            }

            rs = AverageGain / AverageLoss;
            return 100m - (100m / (1 + rs));
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            AverageGain.Reset();
            AverageLoss.Reset();
            base.Reset();
        }
    }
}