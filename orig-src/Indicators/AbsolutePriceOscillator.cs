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
     * This indicator computes the Absolute Price Oscillator (APO)
     * The Absolute Price Oscillator is calculated using the following formula:
     * APO[i] = FastMA[i] - SlowMA[i]
    */
     * 
     * The Absolute Price Oscillator is the same as a MACD with the signal period equal to the slow period.
     * 
    public class AbsolutePriceOscillator : MovingAverageConvergenceDivergence
    {
        /**
         * Initializes a new instance of the <see cref="AbsolutePriceOscillator"/> class using the specified name and parameters.
        */ 
         * @param name The name of this indicator
         * @param fastPeriod The fast moving average period
         * @param slowPeriod The slow moving average period
         * @param movingAverageType The type of moving average to use
        public AbsolutePriceOscillator( String name, int fastPeriod, int slowPeriod, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : base(name, fastPeriod, slowPeriod, slowPeriod, movingAverageType) {
        }

        /**
         * Initializes a new instance of the <see cref="AbsolutePriceOscillator"/> class using the specified parameters.
        */ 
         * @param fastPeriod The fast moving average period
         * @param slowPeriod The slow moving average period
         * @param movingAverageType The type of moving average to use
        public AbsolutePriceOscillator(int fastPeriod, int slowPeriod, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : this( String.format( "APO(%1$s,%2$s)", fastPeriod, slowPeriod), fastPeriod, slowPeriod, movingAverageType) {
        }
    }
}
