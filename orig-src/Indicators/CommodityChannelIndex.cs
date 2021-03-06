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

using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators
{
    /**
     *     Represents the traditional commodity channel index (CCI)
     *     
     *     CCI = (Typical Price - 20-period SMA of TP) / (.015 * Mean Deviation)
     *     Typical Price (TP) = (High + Low + Close)/3
     *     Constant = 0.015
    ///
     *     There are four steps to calculating the Mean Deviation, first, subtract
     *     the most recent 20-period average of the typical price from each period's
     *     typical price. Second, take the absolute values of these numbers. Third,
     *     sum the absolute values. Fourth, divide by the total number of periods (20).
    */
    public class CommodityChannelIndex : TradeBarIndicator
    {
        /**This constant is used to ensure that CCI values fall between +100 and -100, 70% to 80% of the time</summary>
        private static final BigDecimal _k = 0.015m;

        /**
         * Gets the type of moving average
        */
        public MovingAverageType MovingAverageType { get; private set; }

        /**
         * Keep track of the simple moving average of the typical price
        */
        public IndicatorBase<IndicatorDataPoint> TypicalPriceAverage { get; private set; }

        /**
         * Keep track of the mean absolute deviation of the typical price
        */
        public IndicatorBase<IndicatorDataPoint> TypicalPriceMeanDeviation { get; private set; }

        /**
         * Initializes a new instance of the CommodityChannelIndex class
        */
         * @param period The period of the standard deviation and moving average (middle band)
         * @param movingAverageType The type of moving average to be used
        public CommodityChannelIndex(int period, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : this( "CCI" + period, period, movingAverageType) {
        }

        /**
         * Initializes a new instance of the CommodityChannelIndex class
        */
         * @param name The name of this indicator
         * @param period The period of the standard deviation and moving average (middle band)
         * @param movingAverageType The type of moving average to be used
        public CommodityChannelIndex( String name, int period, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : base(name) {
            MovingAverageType = movingAverageType;
            TypicalPriceAverage = movingAverageType.AsIndicator(name + "_TypicalPriceAvg", period);
            TypicalPriceMeanDeviation = new MeanAbsoluteDeviation(name + "_TypicalPriceMAD", period);
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return TypicalPriceAverage.IsReady && TypicalPriceMeanDeviation.IsReady; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            BigDecimal typicalPrice = (input.High + input.Low + input.Close)/3.0m;

            TypicalPriceAverage.Update(input.Time, typicalPrice);
            TypicalPriceMeanDeviation.Update(input.Time, typicalPrice);

            // compare this to zero, since if the mean deviation is very small we can get
            // precision errors due to non-floating point math
            weightedMeanDeviation = _k * TypicalPriceMeanDeviation.Current;
            if( weightedMeanDeviation == 0.0m) {
                return 0.0m;
            }

            return (typicalPrice - TypicalPriceAverage.Current)/weightedMeanDeviation;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            TypicalPriceAverage.Reset();
            TypicalPriceMeanDeviation.Reset();
            base.Reset();
        }
    }
}
