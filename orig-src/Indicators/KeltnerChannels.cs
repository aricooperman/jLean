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
     * This indicator creates a moving average (middle band) with an upper band and lower band
     * fixed at k average true range multiples away from the middle band.  
    */
    public class KeltnerChannels : TradeBarIndicator
    {
        private final BigDecimal _k;

        /**
         * Gets the middle band of the channel
        */
        public IndicatorBase<IndicatorDataPoint> MiddleBand
        {
            get; private set;
        }

        /**
         * Gets the upper band of the channel
        */
        public IndicatorBase<TradeBar> UpperBand
        {
            get; private set;
        }

        /**
         * Gets the lower band of the channel
        */
        public IndicatorBase<TradeBar> LowerBand
        {
            get; private set;
        }

        /**
         * Gets the average true range
        */
        public IndicatorBase<TradeBar> AverageTrueRange
        {
            get; private set;
        }


        /**
         * Initializes a new instance of the KeltnerChannels class
        */
         * @param period The period of the average true range and moving average (middle band)
         * @param k The number of multiplies specifying the distance between the middle band and upper or lower bands
         * @param movingAverageType The type of moving average to be used
        public KeltnerChannels(int period, BigDecimal k, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : this( String.format( "KC(%1$s,%2$s)", period, k), period, k, movingAverageType) {
        }

        /**
         * Initializes a new instance of the KeltnerChannels class
        */
         * @param name The name of this indicator
         * @param period The period of the average true range and moving average (middle band)
         * @param k The number of multiples specifying the distance between the middle band and upper or lower bands
         * @param movingAverageType The type of moving average to be used
        public KeltnerChannels( String name, int period, BigDecimal k, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : base(name) {
            _k = k;

            //Initialise ATR and SMA
            AverageTrueRange = new AverageTrueRange(name + "_AverageTrueRange", period, MovingAverageType.Simple);
            MiddleBand = movingAverageType.AsIndicator(name + "_MiddleBand", period);

            //Compute Lower Band
            LowerBand = new FunctionalIndicator<TradeBar>(name + "_LowerBand",
                input -> ComputeLowerBand(),
                lowerBand -> MiddleBand.IsReady,
                () -> MiddleBand.Reset()
                );

            //Compute Upper Band
            UpperBand = new FunctionalIndicator<TradeBar>(name + "_UpperBand",
                input -> ComputeUpperBand(),
                upperBand -> MiddleBand.IsReady,
                () -> MiddleBand.Reset()
                );
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return MiddleBand.IsReady && UpperBand.IsReady && LowerBand.IsReady && AverageTrueRange.IsReady; }
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            AverageTrueRange.Reset();
            MiddleBand.Reset();
            UpperBand.Reset();
            LowerBand.Reset();
            base.Reset();
        }

        /**
         * Computes the next value for this indicator from the given state.
        */
         * @param input The TradeBar to this indicator on this time step
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            AverageTrueRange.Update(input);

            typicalPrice = (input.High + input.Low + input.Close)/3m;
            MiddleBand.Update(input.Time, typicalPrice);

            // poke the upper/lower bands, they actually don't use the input, they compute
            // based on the ATR and the middle band
            LowerBand.Update(input);
            UpperBand.Update(input);
            return MiddleBand;
        }

        /**
         * Calculates the lower band
        */
        private BigDecimal ComputeLowerBand() {
            return MiddleBand.IsReady ? MiddleBand - AverageTrueRange*_k : new decimal(0.0);
        }

        /**
         * Calculates the upper band
        */
        private BigDecimal ComputeUpperBand() {
            return MiddleBand.IsReady ? MiddleBand + AverageTrueRange*_k : new decimal(0.0);
        }
    }
}
