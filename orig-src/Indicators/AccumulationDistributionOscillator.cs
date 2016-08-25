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
     * This indicator computes the Accumulation/Distribution Oscillator (ADOSC)
     * The Accumulation/Distribution Oscillator is calculated using the following formula:
     * ADOSC = EMA(fast,AD) - EMA(slow,AD)
    */
    public class AccumulationDistributionOscillator : TradeBarIndicator
    {
        private final int _period;
        private final AccumulationDistribution _ad;
        private final ExponentialMovingAverage _emaFast;
        private final ExponentialMovingAverage _emaSlow;

        /**
         * Initializes a new instance of the <see cref="AccumulationDistributionOscillator"/> class using the specified parameters
        */ 
         * @param fastPeriod The fast moving average period
         * @param slowPeriod The slow moving average period
        public AccumulationDistributionOscillator(int fastPeriod, int slowPeriod)
            : this( String.format( "ADOSC(%1$s,%2$s)", fastPeriod, slowPeriod), fastPeriod, slowPeriod) {
        }

        /**
         * Initializes a new instance of the <see cref="AccumulationDistributionOscillator"/> class using the specified parameters
        */ 
         * @param name The name of this indicator
         * @param fastPeriod The fast moving average period
         * @param slowPeriod The slow moving average period
        public AccumulationDistributionOscillator( String name, int fastPeriod, int slowPeriod)
            : base(name) {
            _period = Math.Max(fastPeriod, slowPeriod);
            _ad = new AccumulationDistribution(name + "_AD");
            _emaFast = new ExponentialMovingAverage(name + "_Fast", fastPeriod);
            _emaSlow = new ExponentialMovingAverage(name + "_Slow", slowPeriod);
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
            _ad.Update(input);
            _emaFast.Update(_ad.Current);
            _emaSlow.Update(_ad.Current);

            return IsReady ? _emaFast.Current.Value - _emaSlow.Current.Value : BigDecimal.ZERO;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _ad.Reset();
            _emaFast.Reset();
            _emaSlow.Reset();
            base.Reset();
        }
    }
}
