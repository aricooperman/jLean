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
     * Identical Three Crows candlestick pattern
    */
     * 
     * Must have:
     * - three consecutive and declining black candlesticks
     * - each candle must have no or very short lower shadow
     * - each candle after the first must open at or very close to the prior candle's close
     * The meaning of "very short" is specified with SetCandleSettings;
     * the meaning of "very close" is specified with SetCandleSettings(Equal);
     * The returned value is negative(-1): identical three crows is always bearish;
     * The user should consider that identical 3 crows is significant when it appears after a mature advance or at high levels,
     * while this function does not consider it
     * 
    public class IdenticalThreeCrows : CandlestickPattern
    {
        private final int _shadowVeryShortAveragePeriod;
        private final int _equalAveragePeriod;

        private decimal[] _shadowVeryShortPeriodTotal = new decimal[3];
        private decimal[] _equalPeriodTotal = new decimal[3];

        /**
         * Initializes a new instance of the <see cref="IdenticalThreeCrows"/> class using the specified name.
        */
         * @param name The name of this indicator
        public IdenticalThreeCrows( String name) 
            : base(name, Math.Max(CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod, CandleSettings.Get(CandleSettingType.Equal).AveragePeriod) + 2 + 1) {
            _shadowVeryShortAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowVeryShort).AveragePeriod;
            _equalAveragePeriod = CandleSettings.Get(CandleSettingType.Equal).AveragePeriod;
        }

        /**
         * Initializes a new instance of the <see cref="IdenticalThreeCrows"/> class.
        */
        public IdenticalThreeCrows()
            : this( "IDENTICALTHREECROWS") {
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
                if( Samples >= Period - _shadowVeryShortAveragePeriod) {
                    _shadowVeryShortPeriodTotal[2] += GetCandleRange(CandleSettingType.ShadowVeryShort, window[2]);
                    _shadowVeryShortPeriodTotal[1] += GetCandleRange(CandleSettingType.ShadowVeryShort, window[1]);
                    _shadowVeryShortPeriodTotal[0] += GetCandleRange(CandleSettingType.ShadowVeryShort, input);
                }

                if( Samples >= Period - _equalAveragePeriod) {
                    _equalPeriodTotal[2] += GetCandleRange(CandleSettingType.Near, window[2]);
                    _equalPeriodTotal[1] += GetCandleRange(CandleSettingType.Near, window[1]);
                }

                return BigDecimal.ZERO;
            }

            BigDecimal value;
            if( 
                // 1st black
                GetCandleColor(window[2]) == CandleColor.Black &&
                // very short lower shadow
                GetLowerShadow(window[2]) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal[2], window[2]) &&
                // 2nd black
                GetCandleColor(window[1]) == CandleColor.Black &&
                // very short lower shadow
                GetLowerShadow(window[1]) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal[1], window[1]) &&
                // 3rd black
                GetCandleColor(input) == CandleColor.Black &&
                // very short lower shadow
                GetLowerShadow(input) < GetCandleAverage(CandleSettingType.ShadowVeryShort, _shadowVeryShortPeriodTotal[0], input) &&
                // three declining
                window[2].Close > window[1].Close &&
                window[1].Close > input.Close &&
                // 2nd black opens very close to 1st close
                window[1].Open <= window[2].Close + GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal[2], window[2]) &&
                window[1].Open >= window[2].Close - GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal[2], window[2]) &&
                // 3rd black opens very close to 2nd close 
                input.Open <= window[1].Close + GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal[1], window[1]) &&
                input.Open >= window[1].Close - GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal[1], window[1])
              )
                value = -1m;
            else
                value = BigDecimal.ZERO;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            for (i = 2; i >= 0; i--) {
                _shadowVeryShortPeriodTotal[i] += GetCandleRange(CandleSettingType.ShadowVeryShort, window[i]) -
                                                  GetCandleRange(CandleSettingType.ShadowVeryShort, window[i + _shadowVeryShortAveragePeriod]);
            }

            for (i = 2; i >= 1; i--) {
                _equalPeriodTotal[i] += GetCandleRange(CandleSettingType.Equal, window[i]) -
                                       GetCandleRange(CandleSettingType.Equal, window[i + _equalAveragePeriod]);
            }

            return value;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _shadowVeryShortPeriodTotal = new decimal[3];
            _equalPeriodTotal = new decimal[3];
            base.Reset();
        }
    }
}
