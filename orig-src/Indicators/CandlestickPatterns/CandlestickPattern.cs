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

package com.quantconnect.lean.Indicators.CandlestickPatterns
{
    /**
    /// Abstract base class for a candlestick pattern indicator
    */
    public abstract class CandlestickPattern : WindowIndicator<TradeBar>
    {
        /**
        /// Creates a new <see cref="CandlestickPattern"/> with the specified name
        */
         * @param name">The name of this indicator
         * @param period">The number of data points to hold in the window
        protected CandlestickPattern( String name, int period) 
            : base(name, period) {
        }

        /**
        /// Returns the candle color of a candle
        */
         * @param tradeBar">The input candle
        protected static CandleColor GetCandleColor(TradeBar tradeBar) {
            return tradeBar.Close >= tradeBar.Open ? CandleColor.White : CandleColor.Black;
        }

        /**
        /// Returns the distance between the close and the open of a candle
        */
         * @param tradeBar">The input candle
        protected static BigDecimal GetRealBody(TradeBar tradeBar) {
            return Math.Abs(tradeBar.Close - tradeBar.Open);
        }

        /**
        /// Returns the full range of the candle
        */
         * @param tradeBar">The input candle
        protected static BigDecimal GetHighLowRange(TradeBar tradeBar) {
            return tradeBar.High - tradeBar.Low;
        }

        /**
        /// Returns the range of a candle
        */
         * @param type">The type of setting to use
         * @param tradeBar">The input candle
        protected static BigDecimal GetCandleRange(CandleSettingType type, TradeBar tradeBar) {
            switch (CandleSettings.Get(type).RangeType) {
                case CandleRangeType.RealBody:
                    return GetRealBody(tradeBar);
                    
                case CandleRangeType.HighLow:
                    return GetHighLowRange(tradeBar);

                case CandleRangeType.Shadows:
                    return GetUpperShadow(tradeBar) + GetLowerShadow(tradeBar);

                default:
                    return 0m;
            }
        }

        /**
        /// Returns true if the candle is higher than the previous one
        */
        protected static boolean GetCandleGapUp(TradeBar tradeBar, TradeBar previousBar) {
            return tradeBar.Low > previousBar.High;
        }

        /**
        /// Returns true if the candle is lower than the previous one
        */
        protected static boolean GetCandleGapDown(TradeBar tradeBar, TradeBar previousBar) {
            return tradeBar.High < previousBar.Low;
        }

        /**
        /// Returns true if the candle is higher than the previous one (with no body overlap)
        */
        protected static boolean GetRealBodyGapUp(TradeBar tradeBar, TradeBar previousBar) {
            return Math.Min(tradeBar.Open, tradeBar.Close) > Math.Max(previousBar.Open, previousBar.Close);
        }

        /**
        /// Returns true if the candle is lower than the previous one (with no body overlap)
        */
        protected static boolean GetRealBodyGapDown(TradeBar tradeBar, TradeBar previousBar) {
            return Math.Max(tradeBar.Open, tradeBar.Close) < Math.Min(previousBar.Open, previousBar.Close);
        }

        /**
        /// Returns the range of the candle's lower shadow
        */
         * @param tradeBar">The input candle
        protected static BigDecimal GetLowerShadow(TradeBar tradeBar) {
            return (tradeBar.Close >= tradeBar.Open ? tradeBar.Open : tradeBar.Close) - tradeBar.Low;
        }

        /**
        /// Returns the range of the candle's upper shadow
        */
         * @param tradeBar">The input candle
        protected static BigDecimal GetUpperShadow(TradeBar tradeBar) {
            return tradeBar.High - (tradeBar.Close >= tradeBar.Open ? tradeBar.Close : tradeBar.Open);
        }

        /**
        /// Returns the average range of the previous candles
        */
         * @param type">The type of setting to use
         * @param sum">The sum of the previous candles ranges
         * @param tradeBar">The input candle
        protected static BigDecimal GetCandleAverage(CandleSettingType type, BigDecimal sum, TradeBar tradeBar) {
            defaultSetting = CandleSettings.Get(type);

            return defaultSetting.Factor *
                (defaultSetting.AveragePeriod != 0 ? sum / defaultSetting.AveragePeriod : GetCandleRange(type, tradeBar)) /
                (defaultSetting.RangeType == CandleRangeType.Shadows ? 2.0m : 1.0m);
        }
    }
}
