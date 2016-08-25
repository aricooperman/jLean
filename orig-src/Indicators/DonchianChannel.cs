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
     * This indicator computes the upper and lower band of the Donchian Channel.
     * The upper band is computed by finding the highest high over the given period.
     * The lower band is computed by finding the lowest low over the given period.
     * The primary output value of the indicator is the mean of the upper and lower band for 
     * the given timeframe.
    */
    public class DonchianChannel : TradeBarIndicator
    {
        private TradeBar _previousInput;
        /**
         * Gets the upper band of the Donchian Channel.
        */
        public IndicatorBase<IndicatorDataPoint> UpperBand { get; private set; }

        /**
         * Gets the lower band of the Donchian Channel.
        */
        public IndicatorBase<IndicatorDataPoint> LowerBand { get; private set; }

        /**
         * Initializes a new instance of the <see cref="DonchianChannel"/> class.
        */
         * @param name The name.
         * @param period The period for both the upper and lower channels.
        public DonchianChannel( String name, int period)
            : this(name, period, period) {
        }

        /**
         * Initializes a new instance of the <see cref="DonchianChannel"/> class.
        */
         * @param name The name.
         * @param upperPeriod The period for the upper channel.
         * @param lowerPeriod The period for the lower channel
        public DonchianChannel( String name, int upperPeriod, int lowerPeriod)
            : base(name) {
            UpperBand = new Maximum(name + "_UpperBand", upperPeriod);
            LowerBand = new Minimum(name + "_LowerBand", lowerPeriod);
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return UpperBand.IsReady && LowerBand.IsReady; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator, which by convention is the mean value of the upper band and lower band.
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            if( _previousInput != null ) {
                UpperBand.Update(new IndicatorDataPoint(_previousInput.Time, _previousInput.High));
                LowerBand.Update(new IndicatorDataPoint(_previousInput.Time, _previousInput.Low));
            }

            _previousInput = input;
            return (UpperBand.Current.Value + LowerBand.Current.Value) / 2;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            base.Reset();
            UpperBand.Reset();
            LowerBand.Reset();
        }
    }
}
