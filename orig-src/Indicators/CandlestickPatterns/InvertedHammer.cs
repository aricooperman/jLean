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
     * Inverted Hammer candlestick pattern indicator
    */
     * 
     * Must have:
     * - small real body
     * - long upper shadow
     * - no, or very short, lower shadow
     * - gap down
     * The meaning of "short", "very short" and "long" is specified with SetCandleSettings;
     * The returned value is positive(+1): inverted hammer is always bullish;
     * The user should consider that an inverted hammer must appear in a downtrend, while this function does not consider it
     * 
    public class InvertedHammer : CandlestickPattern
    {
        private final int _bodyShortAveragePeriod;
        private final int _shadowLongAveragePeriod;
        private final int _shadowVeryShortAveragePeriod;

        private BigDecimal _bodyShortPeriodTotal;
        private BigDecimal _shadowLongPeriodTotal;
        private BigDecimal _shadowVeryShortPeriodTotal;

        /**
         * Initializes a new instance of the <see cref="InvertedHammer"/> class using the specified name.
        */
         * @param name The name of this indicator
        public InvertedHammer( String name) 
            : base(name, Math.Max(Math.Max(CandleSettings.Get(CandleSettingType.BodyShort).AveragePeriod, CandleSettings.Get(CandleSettingType.ShadowLong).AveragePeriod),
                CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod) + 1 + 1) {
            _bodyShortAveragePeriod = CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod;
            _shadowLongAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowLong).AveragePeriod;
            _shadowVeryShortAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod;
        }

        /**
         * Initializes a new instance of the <see cref="InvertedHammer"/> class.
        */
        public InvertedHammer()
            : this( "INVERTEDHAMMER") {
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
                if( Samples >= Period - _bodyShortAveragePeriod) {
                    _bodyShortPeriodTotal += GetCandleRange(CandleSettingType.BodyShort, input);
                }

                if( Samples >= Period - _shadowLongAveragePeriod) {
                    _shadowLongPeriodTotal += GetCandleRange(CandleSettingType.ShadowLong, input);
                }

                if( Samples >= Period - _shadowVeryShortAveragePeriod) {
                    _shadowVeryShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryShort, input);
                }

                return BigDecimal.ZERO;
            }

            BigDecimal value;
            if( 
                // small rb
                GetRealBody(input) < GetCandleAverage(CandleSettingType.BodyShort, _bodyShortPeriodTotal, input) &&
                // long upper shadow
                GetUpperShadow(input) > GetCandleAverage(CandleSettingType.ShadowLong, _shadowLongPeriodTotal, input) &&
                // very short lower shadow
                GetLowerShadow(input) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal, input) &&
                // gap down
                GetRealBodyGapDown(input, window[1])
                )
                value = 1m;
            else
                value = BigDecimal.ZERO;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyShortPeriodTotal += GetCandleRange(CandleSettingType.BodyShort, input) -
                                    GetCandleRange(CandleSettingType.BodyShort, window[_bodyShortAveragePeriod]);

            _shadowLongPeriodTotal += GetCandleRange(CandleSettingType.ShadowLong, input) -
                                      GetCandleRange(CandleSettingType.ShadowLong, window[_shadowLongAveragePeriod]);

            _shadowVeryShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryShort, input) -
                                           GetCandleRange(CandleSettingType.ShadowVeryShort, window[_shadowVeryShortAveragePeriod]);

            return value;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _bodyShortPeriodTotal = BigDecimal.ZERO;
            _shadowLongPeriodTotal = BigDecimal.ZERO;
            _shadowVeryShortPeriodTotal = BigDecimal.ZERO;
            base.Reset();
        }
    }
}
