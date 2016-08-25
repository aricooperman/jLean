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
     * This indicator computes the Average Directional Movement Index Rating (ADXR). 
     * The Average Directional Movement Index Rating is calculated with the following formula:
     * ADXR[i] = (ADX[i] + ADX[i - period + 1]) / 2
    */
    public class AverageDirectionalMovementIndexRating : TradeBarIndicator
    {
        private final int _period;
        private final AverageDirectionalIndex _adx;
        private final RollingWindow<decimal> _adxHistory;

        /**
         * Initializes a new instance of the <see cref="AverageDirectionalMovementIndexRating"/> class using the specified name and period.
        */ 
         * @param name The name of this indicator
         * @param period The period of the ADXR
        public AverageDirectionalMovementIndexRating( String name, int period) 
            : base(name) {
            _period = period;
            _adx = new AverageDirectionalIndex(name + "_ADX", period);
            _adxHistory = new RollingWindow<decimal>(period);
        }

        /**
         * Initializes a new instance of the <see cref="AverageDirectionalMovementIndexRating"/> class using the specified period.
        */ 
         * @param period The period of the ADXR
        public AverageDirectionalMovementIndexRating(int period)
            : this( "ADXR" + period, period) {
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= _period; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            _adx.Update(input);
            _adxHistory.Add(_adx);

            return (_adx + _adxHistory[Math.Min(_adxHistory.Count - 1, _period - 1)]) / 2;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _adx.Reset();
            _adxHistory.Reset();
            base.Reset();
        }
    }
}
