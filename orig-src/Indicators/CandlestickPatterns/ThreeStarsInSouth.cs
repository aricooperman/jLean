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
 *
*/

using System;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators.CandlestickPatterns
{
    /**
     * Three Stars In The South candlestick pattern
    */
     * 
     * Must have:
     * - first candle: long black candle with long lower shadow
     * - second candle: smaller black candle that opens higher than prior close but within prior candle's range 
     * and trades lower than prior close but not lower than prior low and closes off of its low(it has a shadow)
     * - third candle: small black marubozu(or candle with very short shadows) engulfed by prior candle's range
     * The meanings of "long body", "short body", "very short shadow" are specified with SetCandleSettings;
     * The returned value is positive (+1): 3 stars in the south is always bullish;
     * The user should consider that 3 stars in the south is significant when it appears in downtrend, while this function
     * does not consider it
     * 
    public class ThreeStarsInSouth : CandlestickPattern
    {
        private final int _bodyLongAveragePeriod;
        private final int _shadowLongAveragePeriod;
        private final int _shadowVeryShortAveragePeriod;
        private final int _bodyShortAveragePeriod;

        private BigDecimal _bodyLongPeriodTotal;
        private BigDecimal _shadowLongPeriodTotal;
        private decimal[] _shadowVeryShortPeriodTotal = new decimal[2];
        private BigDecimal _bodyShortPeriodTotal;

        /**
         * Initializes a new instance of the <see cref="ThreeStarsInSouth"/> class using the specified name.
        */
         * @param name The name of this indicator
        public ThreeStarsInSouth( String name) 
            : base(name, Math.Max(Math.Max(CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod, CandleSettings.Get(CandleSettingType.ShadowLong).AveragePeriod),
                  Math.Max(CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod, CandleSettings.Get(CandleSettingType.BodyShort).AveragePeriod)) + 2 + 1) {
            _bodyLongAveragePeriod = CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod;
            _shadowLongAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowLong).AveragePeriod;
            _shadowVeryShortAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod;
            _bodyShortAveragePeriod = CandleSettings.Get(CandleSettingType.BodyShort).AveragePeriod;
        }

        /**
         * Initializes a new instance of the <see cref="ThreeStarsInSouth"/> class.
        */
        public ThreeStarsInSouth()
            : this( "THREESTARSINSOUTH") {
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
                if( Samples >= Period - _bodyLongAveragePeriod) {
                    _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, window[2]);
                }

                if( Samples >= Period - _shadowLongAveragePeriod) {
                    _shadowLongPeriodTotal += GetCandleRange(CandleSettingType.ShadowLong, window[2]);
                }

                if( Samples >= Period - _shadowVeryShortAveragePeriod) {
                    _shadowVeryShortPeriodTotal[1] += GetCandleRange(CandleSettingType.ShadowVeryShort, window[1]);
                    _shadowVeryShortPeriodTotal[0] += GetCandleRange(CandleSettingType.ShadowVeryShort, input);
                }

                if( Samples >= Period - _bodyShortAveragePeriod) {
                    _bodyShortPeriodTotal += GetCandleRange(CandleSettingType.BodyShort, input);
                }

                return BigDecimal.ZERO;
            }

            BigDecimal value;
            if( 
                // 1st black
                GetCandleColor(window[2]) == CandleColor.Black &&
                // 2nd black
                GetCandleColor(window[1]) == CandleColor.Black &&
                // 3rd black
                GetCandleColor(input) == CandleColor.Black &&
                // 1st: long
                GetRealBody(window[2]) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal, window[2]) &&
                //      with long lower shadow
                GetLowerShadow(window[2]) > GetCandleAverage(CandleSettingType.ShadowLong, _shadowLongPeriodTotal, window[2]) &&
                // 2nd: smaller candle
                GetRealBody(window[1]) < GetRealBody(window[2]) &&
                //      that opens higher but within 1st range
                window[1].Open > window[2].Close && window[1].Open <= window[2].High &&
                //      and trades lower than 1st close
                window[1].Low < window[2].Close &&
                //      but not lower than 1st low
                window[1].Low >= window[2].Low &&
                //      and has a lower shadow
                GetLowerShadow(window[1]) > GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal[1], window[1]) &&
                // 3rd: small marubozu
                GetRealBody(input) < GetCandleAverage(CandleSettingType.BodyShort, _bodyShortPeriodTotal, input) &&
                GetLowerShadow(input) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal[0], input) &&
                GetUpperShadow(input) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal[0], input) &&
                //      engulfed by prior candle's range
                input.Low > window[1].Low && input.High < window[1].High
              )
                value = 1m;
            else
                value = BigDecimal.ZERO;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, window[2]) -
                                    GetCandleRange(CandleSettingType.BodyLong, window[2 + _bodyLongAveragePeriod]);

            _shadowLongPeriodTotal += GetCandleRange(CandleSettingType.ShadowLong, window[2]) -
                                      GetCandleRange(CandleSettingType.ShadowLong, window[2 + _shadowLongAveragePeriod]);

            for (i = 1; i >= 0; i--) {
                _shadowVeryShortPeriodTotal[i] += GetCandleRange(CandleSettingType.ShadowVeryShort, window[i]) -
                                                  GetCandleRange(CandleSettingType.ShadowVeryShort, window[i + _shadowVeryShortAveragePeriod]);
            }

            _bodyShortPeriodTotal += GetCandleRange(CandleSettingType.BodyShort, input) -
                                     GetCandleRange(CandleSettingType.BodyShort, window[_bodyShortAveragePeriod]);

            return value;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _bodyLongPeriodTotal = 0;
            _shadowLongPeriodTotal = 0;
            _shadowVeryShortPeriodTotal = new decimal[2];
            _bodyShortPeriodTotal = 0;
            base.Reset();
        }
    }
}
