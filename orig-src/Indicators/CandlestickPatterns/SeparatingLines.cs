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
    /// Separating Lines candlestick pattern indicator
    */
    /// 
    /// Must have:
    /// - first candle: black (white) candle
    /// - second candle: bullish(bearish) belt hold with the same open as the prior candle
    /// The meaning of "long body" and "very short shadow" of the belt hold is specified with SetCandleSettings
    /// The returned value is positive(+1) when bullish or negative(-1) when bearish;
    /// The user should consider that separating lines is significant when coming in a trend and the belt hold has
    /// the same direction of the trend, while this function does not consider it
    /// 
    public class SeparatingLines : CandlestickPattern
    {
        private final int _shadowVeryShortAveragePeriod;
        private final int _bodyLongAveragePeriod;
        private final int _equalAveragePeriod;

        private BigDecimal _shadowVeryShortPeriodTotal;
        private BigDecimal _bodyLongPeriodTotal;
        private BigDecimal _equalPeriodTotal;

        /**
        /// Initializes a new instance of the <see cref="SeparatingLines"/> class using the specified name.
        */
         * @param name">The name of this indicator
        public SeparatingLines( String name) 
            : base(name, Math.Max(Math.Max(CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod, CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod),
                CandleSettings.Get(CandleSettingType.Equal).AveragePeriod) + 1 + 1) {
            _shadowVeryShortAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod;
            _bodyLongAveragePeriod = CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod;
            _equalAveragePeriod = CandleSettings.Get(CandleSettingType.Equal).AveragePeriod;
        }

        /**
        /// Initializes a new instance of the <see cref="SeparatingLines"/> class.
        */
        public SeparatingLines()
            : this( "SEPARATINGLINES") {
        }

        /**
        /// Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= Period; }
        }

        /**
        /// Computes the next value of this indicator from the given state
        */
         * @param window">The window of data held in this indicator
         * @param input">The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<TradeBar> window, TradeBar input) {
            if( !IsReady) {
                if( Samples >= Period - _shadowVeryShortAveragePeriod) {
                    _shadowVeryShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryShort, input);
                }

                if( Samples >= Period - _bodyLongAveragePeriod) {
                    _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, input);
                }

                if( Samples >= Period - _equalAveragePeriod) {
                    _equalPeriodTotal += GetCandleRange(CandleSettingType.Equal, window[1]);
                }

                return 0m;
            }

            BigDecimal value;
            if( 
                // opposite candles
                (int)GetCandleColor(window[1]) == -(int)GetCandleColor(input) &&
                // same open
                input.Open <= window[1].Open + GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal, window[1]) &&
                input.Open >= window[1].Open - GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal, window[1]) &&
                // belt hold: long body
                GetRealBody(input) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal, input) &&
                (
                  // with no lower shadow if bullish
                  (GetCandleColor(input) == CandleColor.White &&
                    GetLowerShadow(input) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal, input)
                  )
                  ||
                  // with no upper shadow if bearish
                  (GetCandleColor(input) == CandleColor.Black &&
                    GetUpperShadow(input) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal, input)
                  )
                )
              )
                value = (int)GetCandleColor(input);
            else
                value = 0m;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _shadowVeryShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryShort, input) -
                                           GetCandleRange(CandleSettingType.ShadowVeryShort, window[_shadowVeryShortAveragePeriod]);

            _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, input) -
                                    GetCandleRange(CandleSettingType.BodyLong, window[_bodyLongAveragePeriod]);

            _equalPeriodTotal += GetCandleRange(CandleSettingType.Equal, window[1]) -
                                 GetCandleRange(CandleSettingType.Equal, window[_equalAveragePeriod + 1]);

            return value;
        }

        /**
        /// Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _shadowVeryShortPeriodTotal = 0m;
            _bodyLongPeriodTotal = 0m;
            _equalPeriodTotal = 0m;
            base.Reset();
        }
    }
}
