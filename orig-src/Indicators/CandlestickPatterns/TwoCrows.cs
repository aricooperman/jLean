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

using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators.CandlestickPatterns
{
    /**
     * Two Crows candlestick pattern indicator
    */
     * 
     * Must have:
     * - first candle: long white candle
     * - second candle: black real body
     * - gap between the first and the second candle's real bodies
     * - third candle: black candle that opens within the second real body and closes within the first real body
     * The meaning of "long" is specified with SetCandleSettings
     * The returned value is negative (-1): two crows is always bearish;
     * The user should consider that two crows is significant when it appears in an uptrend, while this function
     * does not consider the trend.
     * 
    public class TwoCrows : CandlestickPattern
    {
        private final int _bodyLongAveragePeriod;

        private BigDecimal _bodyLongPeriodTotal;

        /**
         * Initializes a new instance of the <see cref="TwoCrows"/> class using the specified name.
        */
         * @param name The name of this indicator
        public TwoCrows( String name) 
            : base(name, CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod + 2 + 1) {
            _bodyLongAveragePeriod = CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod;
        }

        /**
         * Initializes a new instance of the <see cref="TwoCrows"/> class.
        */
        public TwoCrows()
            : this( "TWOCROWS") {
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
                if( Samples >= Period - _bodyLongAveragePeriod - 2 && Samples < Period - 2) {
                    _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, input);
                }
                return BigDecimal.ZERO;
            }

            BigDecimal value;
            if( 
                // 1st: white
                GetCandleColor(window[2]) == CandleColor.White &&
                //      long
                GetRealBody(window[2]) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal, window[2]) &&
                // 2nd: black
                GetCandleColor(window[1]) == CandleColor.Black &&
                //      gapping up
                GetRealBodyGapUp(window[1], window[2]) &&
                // 3rd: black
                GetCandleColor(input) == CandleColor.Black &&
                //      opening within 2nd rb
                input.Open < window[1].Open && input.Open > window[1].Close &&
                //      closing within 1st rb
                input.Close > window[2].Open && input.Close < window[2].Close
              )
                value = -1m;
            else
                value = BigDecimal.ZERO;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, window[2]) -
                                    GetCandleRange(CandleSettingType.BodyLong, window[2 + _bodyLongAveragePeriod]);

            return value;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _bodyLongPeriodTotal = BigDecimal.ZERO;
            base.Reset();
        }
    }
}
