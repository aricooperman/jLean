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
     * This indicator computes the Normalized Average True Range (NATR).
     * The Normalized Average True Range is calculated with the following formula:
     * NATR = (ATR(period) / Close) * 100
    */
    public class NormalizedAverageTrueRange : TradeBarIndicator
    {
        private final int _period;
        private final TrueRange _tr;
        private final AverageTrueRange _atr;
        private BigDecimal _lastAtrValue;

        /**
         * Initializes a new instance of the <see cref="NormalizedAverageTrueRange"/> class using the specified name and period.
        */ 
         * @param name The name of this indicator
         * @param period The period of the NATR
        public NormalizedAverageTrueRange( String name, int period) : 
            base(name) {
            _period = period;
            _tr = new TrueRange(name + "_TR");
            _atr = new AverageTrueRange(name + "_ATR", period, MovingAverageType.Simple);
        }

        /**
         * Initializes a new instance of the <see cref="NormalizedAverageTrueRange"/> class using the specified period.
        */ 
         * @param period The period of the NATR
        public NormalizedAverageTrueRange(int period)
            : this( "NATR" + period, period) {
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples > _period; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            _tr.Update(input);

            if( !IsReady) {
                _atr.Update(input);
                return input.Close != 0 ? _atr / input.Close * 100 : BigDecimal.ZERO;
            }

            if( Samples == _period + 1) {
                // first output value is SMA of TrueRange
                _atr.Update(input);
                _lastAtrValue = _atr;
            }
            else
            {
                // next TrueRange values are smoothed using Wilder's approach
                _lastAtrValue = (_lastAtrValue * (_period - 1) + _tr) / _period;
            }

            return input.Close != 0 ? _lastAtrValue / input.Close * 100 : BigDecimal.ZERO;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _tr.Reset();
            _atr.Reset();
            base.Reset();
        }
    }
}
