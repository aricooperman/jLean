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
     * Up/Down-gap side-by-side white lines candlestick pattern
    */
     * 
     * Must have:
     * - upside or downside gap (between the bodies)
     * - first candle after the window: white candlestick
     * - second candle after the window: white candlestick with similar size(near the same) and about the same
     * open(equal) of the previous candle
     * - the second candle does not close the window
     * The meaning of "near" and "equal" is specified with SetCandleSettings
     * The returned value is positive(+1) or negative(-1): the user should consider that upside
     * or downside gap side-by-side white lines is significant when it appears in a trend, while this function
     * does not consider the trend
     * 
    public class GapSideBySideWhite : CandlestickPattern
    {
        private final int _nearAveragePeriod;
        private final int _equalAveragePeriod;

        private BigDecimal _nearPeriodTotal;
        private BigDecimal _equalPeriodTotal;

        /**
         * Initializes a new instance of the <see cref="GapSideBySideWhite"/> class using the specified name.
        */
         * @param name The name of this indicator
        public GapSideBySideWhite( String name) 
            : base(name, Math.Max(CandleSettings.Get(CandleSettingType.Near).AveragePeriod, CandleSettings.Get(CandleSettingType.Equal).AveragePeriod) + 2 + 1) {
            _nearAveragePeriod = CandleSettings.Get(CandleSettingType.Near).AveragePeriod;
            _equalAveragePeriod = CandleSettings.Get(CandleSettingType.Equal).AveragePeriod;
        }

        /**
         * Initializes a new instance of the <see cref="GapSideBySideWhite"/> class.
        */
        public GapSideBySideWhite()
            : this( "GAPSIDEBYSIDEWHITE") {
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
                if( Samples >= Period - _nearAveragePeriod) {
                    _nearPeriodTotal += GetCandleRange(CandleSettingType.Near, window[1]);
                }

                if( Samples >= Period - _equalAveragePeriod) {
                    _equalPeriodTotal += GetCandleRange(CandleSettingType.Equal, window[1]);
                }

                return BigDecimal.ZERO;
            }

            BigDecimal value;
            if( 
                ( // upside or downside gap between the 1st candle and both the next 2 candles
                  (GetRealBodyGapUp(window[1], window[2]) && GetRealBodyGapUp(input, window[2]))
                  ||
                  (GetRealBodyGapDown(window[1], window[2]) && GetRealBodyGapDown(input, window[2]))
                ) &&
                // 2nd: white
                GetCandleColor(window[1]) == CandleColor.White &&
                // 3rd: white
                GetCandleColor(input) == CandleColor.White &&
                // same size 2 and 3
                GetRealBody(input) >= GetRealBody(window[1]) - GetCandleAverage(CandleSettingType.Near, _nearPeriodTotal, window[1]) &&
                GetRealBody(input) <= GetRealBody(window[1]) + GetCandleAverage(CandleSettingType.Near, _nearPeriodTotal, window[1]) &&
                // same open 2 and 3
                input.Open >= window[1].Open - GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal, window[1]) &&
                input.Open <= window[1].Open + GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal, window[1])
              )
                value = GetRealBodyGapUp(window[1], window[2]) ? 1m : -1m;
            else
                value = BigDecimal.ZERO;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _nearPeriodTotal += GetCandleRange(CandleSettingType.Near, window[1]) -
                                GetCandleRange(CandleSettingType.Near, window[1 + _nearAveragePeriod]);

            _equalPeriodTotal += GetCandleRange(CandleSettingType.Equal, window[1]) -
                                 GetCandleRange(CandleSettingType.Equal, window[1 + _equalAveragePeriod]);

            return value;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _nearPeriodTotal = 0;
            _equalPeriodTotal = 0;
            base.Reset();
        }
    }
}
