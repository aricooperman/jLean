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
     * Williams %R, or just %R, is the current closing price in relation to the high and low of
     * the past N days (for a given N). The value of this indicator fluctuats between -100 and 0. 
     * The symbol is said to be oversold when the oscillator is below -80%,
     * and overbought when the oscillator is above -20%. 
    */
    public class WilliamsPercentR : TradeBarIndicator
    {
        /**
         * Gets the Maximum indicator
        */
        public Maximum Maximum { get; private set; }

        /**
         * Gets the Minimum indicator
        */
        public Minimum Minimum { get; private set; }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Maximum.IsReady && Minimum.IsReady; }
        }

        /**
         * Creates a new Williams %R.
        */
         * @param period The lookback period to determine the highest high for the AroonDown
        public WilliamsPercentR(int period)
            : this( "WILR"+period, period) {
        }

        /**
         * Creates a new Williams %R.
        */
         * @param name The name of this indicator
         * @param period The lookback period to determine the highest high for the AroonDown
        public WilliamsPercentR( String name, int period)
            : base(name) {
            Maximum = new Maximum(name + "_Max", period);
            Minimum = new Minimum(name + "_Min", period);
        }

        /**
         * Resets this indicator and both sub-indicators (Max and Min)
        */
        public @Override void Reset() {
            Maximum.Reset();
            Minimum.Reset();
            base.Reset();
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            Minimum.Update(input.Time, input.Low);
            Maximum.Update(input.Time, input.High);

            if( !this.IsReady) return 0;
           
            range = (Maximum.Current.Value - Minimum.Current.Value);

            return range == 0 ? 0 : -100m*(Maximum.Current.Value - input.Close)/range;
        }
    }
}