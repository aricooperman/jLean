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
 *
*/
using QuantConnect.Data.Market;
using System;

package com.quantconnect.lean.Indicators
{
    /**
     * Volume Weighted Average Price (VWAP) Indicator:
     * It is calculated by adding up the dollars traded for every transaction (price multiplied
     * by number of shares traded) and then dividing by the total shares traded for the day.
    */
    public class VolumeWeightedAveragePriceIndicator : TradeBarIndicator
    {
        /**
         * In this VWAP calculation, typical price is defined by (O + H + L + C) / 4
        */
        private Identity _price;
        private Identity _volume;
        private CompositeIndicator<IndicatorDataPoint> _vwap;

        /**
         * Initializes a new instance of the VWAP class with the default name and period
        */
         * @param period The period of the VWAP
        public VolumeWeightedAveragePriceIndicator(int period)
            : this( "VWAP_" + period, period) {
        }

        /**
         * Initializes a new instance of the VWAP class with a given name and period
        */
         * @param name string - the name of the indicator
         * @param period The period of the VWAP
        public VolumeWeightedAveragePriceIndicator( String name, int period)
            : base(name) {
            _price = new Identity( "Price");
            _volume = new Identity( "Volume");
            
            // This class will be using WeightedBy indicator extension 
            _vwap = _price.WeightedBy(_volume, period);
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return _vwap.IsReady; }
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _price.Reset();
            _volume.Reset();
            _vwap.Reset();
            base.Reset();
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            _price.Update(input.EndTime, (input.Open + input.High + input.Low + input.Value) / 4);
            _volume.Update(input.EndTime, input.Volume);
            return _vwap.Current.Value;
        }
    }
}