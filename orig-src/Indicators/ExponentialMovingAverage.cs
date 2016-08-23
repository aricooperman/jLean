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
    ///     Represents the traditional exponential moving average indicator (EMA)
    */
    public class ExponentialMovingAverage : Indicator
    {
        private final BigDecimal _k;
        private final int _period;

        /**Initializes a new instance of the ExponentialMovingAverage class with the specified name and period
        */
         * @param name">The name of this indicator
         * @param period">The period of the EMA
        public ExponentialMovingAverage( String name, int period)
            : base(name) {
            _period = period;
            _k = ExponentialMovingAverage.SmoothingFactorDefault(period);
        }

        /**Initializes a new instance of the ExponentialMovingAverage class with the specified name and period
        */
         * @param name">The name of this indicator
         * @param period">The period of the EMA
         * @param smoothingFactor">The percentage of data from the previous value to be carried into the next value
        public ExponentialMovingAverage( String name, int period, BigDecimal smoothingFactor)
            : base(name) {
            _period = period;
            _k = smoothingFactor;
        }

        /**
        ///     Initializes a new instance of the ExponentialMovingAverage class with the default name and period
        */
         * @param period">The period of the EMA
        public ExponentialMovingAverage(int period)
            : this( "EMA" + period, period) {
        }

        /**Initializes a new instance of the ExponentialMovingAverage class with the default name and period
        */
         * @param period">The period of the EMA
         * @param smoothingFactor">The percentage of data from the previous value to be carried into the next value
        public ExponentialMovingAverage(int period, BigDecimal smoothingFactor)
            : this( "EMA" + period, period, smoothingFactor) {
        }

        /**Calculates the default smoothing factor for an ExponentialMovingAverage indicator
        */
         * @param period">The period of the EMA
        @returns The default smoothing factor
        public static BigDecimal SmoothingFactorDefault(int period) {
            return 2.0m / ((decimal) period + 1.0m);
        }

        /**
        ///     Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= _period; }
        }

        /**
        ///     Computes the next value of this indicator from the given state
        */
         * @param input">The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IndicatorDataPoint input) {
            // our first data point just return identity
            if( Samples == 1) {
                return input;
            }
            return input*_k + Current*(1 - _k);
        }
    }
}