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
    /// This indicator computes the Triple Exponential Moving Average (TEMA). 
    /// The Triple Exponential Moving Average is calculated with the following formula:
    /// EMA1 = EMA(t,period)
    /// EMA2 = EMA(EMA(t,period),period)
    /// EMA3 = EMA(EMA(EMA(t,period),period),period)
    /// TEMA = 3 * EMA1 - 3 * EMA2 + EMA3
    */
    public class TripleExponentialMovingAverage : IndicatorBase<IndicatorDataPoint>
    {
        private final int _period;
        private final ExponentialMovingAverage _ema1;
        private final ExponentialMovingAverage _ema2;
        private final ExponentialMovingAverage _ema3;

        /**
        /// Initializes a new instance of the <see cref="TripleExponentialMovingAverage"/> class using the specified name and period.
        */ 
         * @param name">The name of this indicator
         * @param period">The period of the TEMA
        public TripleExponentialMovingAverage( String name, int period)
            : base(name) {
            _period = period;
            _ema1 = new ExponentialMovingAverage(name + "_1", period);
            _ema2 = new ExponentialMovingAverage(name + "_2", period);
            _ema3 = new ExponentialMovingAverage(name + "_3", period);
        }

        /**
        /// Initializes a new instance of the <see cref="TripleExponentialMovingAverage"/> class using the specified period.
        */ 
         * @param period">The period of the TEMA
        public TripleExponentialMovingAverage(int period)
            : this( "TEMA" + period, period) {
        }

        /**
        /// Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples > 3 * (_period - 1); }
        }

        /**
        /// Computes the next value of this indicator from the given state
        */
         * @param input">The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IndicatorDataPoint input) {
            _ema1.Update(input);

            if( Samples > _period - 1)
                _ema2.Update(_ema1.Current);

            if( Samples > 2 * (_period - 1))
                _ema3.Update(_ema2.Current);

            return IsReady ? 3m * _ema1 - 3m * _ema2 + _ema3 : 0m;
        }

        /**
        /// Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _ema1.Reset();
            _ema2.Reset();
            _ema3.Reset();
            base.Reset();
        }
    }
}
