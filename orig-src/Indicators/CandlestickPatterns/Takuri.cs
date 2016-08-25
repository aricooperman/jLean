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

using System;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators.CandlestickPatterns
{
    /**
     * Takuri (Dragonfly Doji with very long lower shadow) candlestick pattern indicator
    */
     * 
     * Must have:
     * - doji body
     * - open and close at the high of the day = no or very short upper shadow
     * - very long lower shadow
     * The meaning of "doji", "very short" and "very long" is specified with SetCandleSettings
     * The returned value is always positive(+1) but this does not mean it is bullish: takuri must be considered
     * relatively to the trend
     * 
    public class Takuri : CandlestickPattern
    {
        private final int _bodyDojiAveragePeriod;
        private final int _shadowVeryShortAveragePeriod;
        private final int _shadowVeryLongAveragePeriod;

        private BigDecimal _bodyDojiPeriodTotal;
        private BigDecimal _shadowVeryShortPeriodTotal;
        private BigDecimal _shadowVeryLongPeriodTotal;

        /**
         * Initializes a new instance of the <see cref="Takuri"/> class using the specified name.
        */
         * @param name The name of this indicator
        public Takuri( String name) 
            : base(name, Math.Max(Math.Max(CandleSettings.Get(CandleSettingType.BodyDoji).AveragePeriod, CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod),
                CandleSettings.Get(CandleSettingType.ShadowVeryLong).AveragePeriod) + 1) {
            _bodyDojiAveragePeriod = CandleSettings.Get(CandleSettingType.BodyDoji).AveragePeriod;
            _shadowVeryShortAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod;
            _shadowVeryLongAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowVeryLong).AveragePeriod;
        }

        /**
         * Initializes a new instance of the <see cref="Takuri"/> class.
        */
        public Takuri()
            : this( "TAKURI") {
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= Period; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param window The window of data held in this indicator
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<TradeBar> window, TradeBar input) {
            if( !IsReady) {
                if( Samples >= Period - _bodyDojiAveragePeriod) {
                    _bodyDojiPeriodTotal += GetCandleRange(CandleSettingType.BodyDoji, input);
                }

                if( Samples >= Period - _shadowVeryShortAveragePeriod) {
                    _shadowVeryShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryShort, input);
                }

                if( Samples >= Period - _shadowVeryLongAveragePeriod) {
                    _shadowVeryLongPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryLong, input);
                }

                return BigDecimal.ZERO;
            }

            BigDecimal value;
            if( GetRealBody(input) <= GetCandleAverage(CandleSettingType.BodyDoji, _bodyDojiPeriodTotal, input) &&
                GetUpperShadow(input) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal, input) &&
                GetLowerShadow(input) > GetCandleAverage(CandleSettingType.ShadowVeryLong, _shadowVeryLongPeriodTotal, input)
              )
                value = 1m;
            else
                value = BigDecimal.ZERO;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyDojiPeriodTotal += GetCandleRange(CandleSettingType.BodyDoji, input) -
                                    GetCandleRange(CandleSettingType.BodyDoji, window[_bodyDojiAveragePeriod]);

            _shadowVeryShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryShort, input) -
                                           GetCandleRange(CandleSettingType.ShadowVeryShort, window[_shadowVeryShortAveragePeriod]);

            _shadowVeryLongPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryLong, input) -
                                          GetCandleRange(CandleSettingType.ShadowVeryLong, window[_shadowVeryLongAveragePeriod]);

            return value;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _bodyDojiPeriodTotal = BigDecimal.ZERO;
            _shadowVeryShortPeriodTotal = BigDecimal.ZERO;
            _shadowVeryLongPeriodTotal = BigDecimal.ZERO;
            base.Reset();
        }
    }
}
