﻿/*
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
     * This indicator computes the Chande Momentum Oscillator (CMO). 
     * CMO calculation is mostly identical to RSI.
     * The only difference is in the last step of calculation:
     * RSI = gain / (gain+loss)
     * CMO = (gain-loss) / (gain+loss)
    */
    public class ChandeMomentumOscillator : WindowIndicator<IndicatorDataPoint>
    {
        private BigDecimal _prevValue;
        private BigDecimal _prevGain;
        private BigDecimal _prevLoss;

        /**
         * Initializes a new instance of the <see cref="ChandeMomentumOscillator"/> class using the specified period.
        */ 
         * @param period The period of the indicator
        public ChandeMomentumOscillator(int period)
            : this( "CMO" + period, period) {
        }

        /**
         * Initializes a new instance of the <see cref="ChandeMomentumOscillator"/> class using the specified name and period.
        */ 
         * @param name The name of this indicator
         * @param period The period of the indicator
        public ChandeMomentumOscillator( String name, int period)
            : base(name, period) {
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples > Period; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
         * @param window The window for the input history
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            if( Samples == 1) {
                _prevValue = input;
                return BigDecimal.ZERO;
            }

            difference = input.Value - _prevValue;

            _prevValue = input.Value;

            if( Samples > Period + 1) {
                _prevLoss *= (Period - 1);
                _prevGain *= (Period - 1);
            }

            if( difference < 0)
                _prevLoss -= difference;
            else
                _prevGain += difference;

            if( !IsReady)
                return BigDecimal.ZERO;

            _prevLoss /= Period;
            _prevGain /= Period;

            sum = _prevGain + _prevLoss;
            return sum != 0 ? 100m * ((_prevGain - _prevLoss) / sum) : BigDecimal.ZERO;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _prevValue = 0;
            _prevGain = 0;
            _prevLoss = 0;
            base.Reset();
        }
    }
}
