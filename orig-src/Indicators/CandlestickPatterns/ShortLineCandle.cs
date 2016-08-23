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
    /// Short Line Candle candlestick pattern indicator
    */
    /// 
    /// Must have:
    /// - short real body
    /// - short upper and lower shadow
    /// The meaning of "short" is specified with SetCandleSettings
    /// The returned value is positive(+1) when white, negative (-1) when black;
    /// it does not mean bullish or bearish
    /// 
    public class ShortLineCandle : CandlestickPattern
    {
        private final int _bodyShortAveragePeriod;
        private final int _shadowShortAveragePeriod;

        private BigDecimal _bodyShortPeriodTotal;
        private BigDecimal _shadowShortPeriodTotal;

        /**
        /// Initializes a new instance of the <see cref="ShortLineCandle"/> class using the specified name.
        */
         * @param name">The name of this indicator
        public ShortLineCandle( String name) 
            : base(name, Math.Max(CandleSettings.Get(CandleSettingType.BodyShort).AveragePeriod, CandleSettings.Get(CandleSettingType.ShadowShort).AveragePeriod) + 1) {
            _bodyShortAveragePeriod = CandleSettings.Get(CandleSettingType.BodyShort).AveragePeriod;
            _shadowShortAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowShort).AveragePeriod;
        }

        /**
        /// Initializes a new instance of the <see cref="ShortLineCandle"/> class.
        */
        public ShortLineCandle()
            : this( "SHORTLINECANDLE") {
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

                if( Samples >= Period - _shadowShortAveragePeriod) {
                    _shadowShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowShort, input);
                }

                return 0m;
            }

            BigDecimal value;
            if( GetRealBody(input) < GetCandleAverage(CandleSettingType.BodyShort, _bodyShortPeriodTotal, input) &&
                GetUpperShadow(input) < GetCandleAverage(CandleSettingType.ShadowShort, _shadowShortPeriodTotal, input) &&
                GetLowerShadow(input) < GetCandleAverage(CandleSettingType.ShadowShort, _shadowShortPeriodTotal, input)
                )
                value = (int)GetCandleColor(input);
            else
                value = 0m;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyShortPeriodTotal += GetCandleRange(CandleSettingType.BodyShort, input) -
                                     GetCandleRange(CandleSettingType.BodyShort, window[_bodyShortAveragePeriod]);

            _shadowShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowShort, input) -
                                       GetCandleRange(CandleSettingType.ShadowShort, window[_shadowShortAveragePeriod]);

            return value;
        }

        /**
        /// Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _bodyShortPeriodTotal = 0m;
            _shadowShortPeriodTotal = 0m;
            base.Reset();
        }
    }
}
