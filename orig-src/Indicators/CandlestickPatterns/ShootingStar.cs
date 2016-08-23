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

using System;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators.CandlestickPatterns
{
    /**
    /// Shooting Star candlestick pattern
    */
    /// 
    /// Must have:
    /// - small real body
    /// - long upper shadow
    /// - no, or very short, lower shadow
    /// - gap up from prior real body
    /// The meaning of "short", "very short" and "long" is specified with SetCandleSettings;
    /// The returned value is negative(-1): shooting star is always bearish;
    /// The user should consider that a shooting star must appear in an uptrend, while this function does not consider it
    /// 
    public class ShootingStar : CandlestickPattern
    {
        private final int _bodyShortAveragePeriod;
        private final int _shadowLongAveragePeriod;
        private final int _shadowVeryShortAveragePeriod;

        private BigDecimal _bodyShortPeriodTotal;
        private BigDecimal _shadowLongPeriodTotal;
        private BigDecimal _shadowVeryShortPeriodTotal;

        /**
        /// Initializes a new instance of the <see cref="ShootingStar"/> class using the specified name.
        */
         * @param name">The name of this indicator
        public ShootingStar( String name)
            : base(name, Math.Max(Math.Max(CandleSettings.Get(CandleSettingType.BodyShort).AveragePeriod, CandleSettings.Get(CandleSettingType.ShadowLong).AveragePeriod),
                  CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod) + 1 + 1) {
            _bodyShortAveragePeriod = CandleSettings.Get(CandleSettingType.BodyShort).AveragePeriod;
            _shadowLongAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowLong).AveragePeriod;
            _shadowVeryShortAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod;
        }

        /**
        /// Initializes a new instance of the <see cref="ShootingStar"/> class.
        */
        public ShootingStar()
            : this( "SHOOTINGSTAR") {
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
                if( Samples >= Period - _bodyShortAveragePeriod) {
                    _bodyShortPeriodTotal += GetCandleRange(CandleSettingType.BodyShort, input);
                }

                if( Samples >= Period - _shadowLongAveragePeriod) {
                    _shadowLongPeriodTotal += GetCandleRange(CandleSettingType.ShadowLong, input);
                }

                if( Samples >= Period - _shadowVeryShortAveragePeriod) {
                    _shadowVeryShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowVeryShort, input);
                }

                return 0m;
            }

            BigDecimal value;
            if( 
                // small rb
                GetRealBody(input) < GetCandleAverage(CandleSettingType.BodyShort, _bodyShortPeriodTotal, input) &&
                // long upper shadow
                GetUpperShadow(input) > GetCandleAverage(CandleSettingType.ShadowLong, _shadowLongPeriodTotal, input) &&
                // very short lower shadow
                GetLowerShadow(input) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal, input) &&
                // gap up
                GetRealBodyGapUp(input, window[1])
                )
                value = -1m;
            else
                value = 0m;

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
        /// Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _bodyShortPeriodTotal = 0;
            _shadowLongPeriodTotal = 0;
            _shadowVeryShortPeriodTotal = 0;
            base.Reset();
        }
    }
}
