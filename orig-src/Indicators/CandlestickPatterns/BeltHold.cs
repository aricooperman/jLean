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
    /// Belt-hold candlestick pattern indicator
    */
    /// 
    /// Must have:
    /// - long white(black) real body
    /// - no or very short lower(upper) shadow
    /// The meaning of "long" and "very short" is specified with SetCandleSettings
    /// The returned value is positive(+1) when white(bullish), negative(-1) when black(bearish)
    /// 
    public class BeltHold : CandlestickPattern
    {
        private final int _bodyLongAveragePeriod;
        private final int _shadowVeryShortAveragePeriod;

        private BigDecimal _bodyLongPeriodTotal;
        private BigDecimal _shadowVeryShortPeriodTotal;

        /**
        /// Initializes a new instance of the <see cref="BeltHold"/> class using the specified name.
        */
         * @param name">The name of this indicator
        public BeltHold( String name) 
            : base(name, Math.Max(CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod, CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod) + 1) {
            _bodyLongAveragePeriod = CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod;
            _shadowVeryShortAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod;
        }

        /**
        /// Initializes a new instance of the <see cref="BeltHold"/> class.
        */
        public BeltHold()
            : this( "BELTHOLD") {
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
                if( Samples >= Period - _bodyLongAveragePeriod) {
                    _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, input);
                }

                if( Samples >= Period - _shadowVeryShortAveragePeriod) {
                    _shadowVeryShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryShort, input);
                }

                return 0m;
            }

            BigDecimal value;
            if( 
                // long body
                GetRealBody(input) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal, input) &&             
                (
                  ( 
                    // white body and very short lower shadow
                    GetCandleColor(input) == CandleColor.White &&
                    GetLowerShadow(input) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal, input)
                  ) ||
                  ( 
                    // black body and very short upper shadow
                    GetCandleColor(input) == CandleColor.Black &&
                    GetUpperShadow(input) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal, input)
                  )
                ))
                value = (int)GetCandleColor(input);
            else
                value = 0m;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, input) -
                                    GetCandleRange(CandleSettingType.BodyLong, window[_bodyLongAveragePeriod]);

            _shadowVeryShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryShort, input) -
                                           GetCandleRange(CandleSettingType.ShadowVeryShort, window[_shadowVeryShortAveragePeriod]);

            return value;
        }

        /**
        /// Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _bodyLongPeriodTotal = 0m;
            _shadowVeryShortPeriodTotal = 0m;
            base.Reset();
        }
    }
}
