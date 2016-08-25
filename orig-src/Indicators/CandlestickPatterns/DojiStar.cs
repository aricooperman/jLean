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
     * Doji Star candlestick pattern indicator
    */
     * 
     * Must have:
     * - first candle: long real body
     * - second candle: star(open gapping up in an uptrend or down in a downtrend) with a doji
     * The meaning of "doji" and "long" is specified with SetCandleSettings
     * The returned value is positive(+1) when bullish or negative(-1) when bearish;
     * it's defined bullish when the long candle is white and the star gaps up, bearish when the long candle 
     * is black and the star gaps down; the user should consider that a doji star is bullish when it appears
     * in an uptrend and it's bearish when it appears in a downtrend, so to determine the bullishness or 
     * bearishness of the pattern the trend must be analyzed
     * 
    public class DojiStar : CandlestickPattern
    {
        private final int _bodyLongAveragePeriod;
        private final int _bodyDojiAveragePeriod;

        private BigDecimal _bodyLongPeriodTotal;
        private BigDecimal _bodyDojiPeriodTotal;

        /**
         * Initializes a new instance of the <see cref="DojiStar"/> class using the specified name.
        */
         * @param name The name of this indicator
        public DojiStar( String name) 
            : base(name, Math.Max(CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod, CandleSettings.Get(CandleSettingType.BodyDoji).AveragePeriod) + 1 + 1) {
            _bodyLongAveragePeriod = CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod;
            _bodyDojiAveragePeriod = CandleSettings.Get(CandleSettingType.BodyDoji).AveragePeriod;
        }

        /**
         * Initializes a new instance of the <see cref="DojiStar"/> class.
        */
        public DojiStar()
            : this( "DOJISTAR") {
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
                if( Samples >= Period - _bodyLongAveragePeriod - 1 && Samples < Period - 1) {
                    _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, input);
                }

                if( Samples >= Period - _bodyDojiAveragePeriod) {
                    _bodyDojiPeriodTotal += GetCandleRange(CandleSettingType.BodyDoji, input);
                }

                return BigDecimal.ZERO;
            }

            BigDecimal value;
            if( 
                // 1st: long real body
                GetRealBody(window[1]) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal, window[1]) &&
                // 2nd: doji
                GetRealBody(input) <= GetCandleAverage(CandleSettingType.BodyDoji, _bodyDojiPeriodTotal, input) &&
                //      that gaps up if 1st is white
                ((GetCandleColor(window[1]) == CandleColor.White && GetRealBodyGapUp(input, window[1]))
                    ||
                    //      or down if 1st is black
                    (GetCandleColor(window[1]) == CandleColor.Black && GetRealBodyGapDown(input, window[1]))
                ))
                value = -(int)GetCandleColor(window[1]);
            else
                value = BigDecimal.ZERO;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, window[1]) -
                                    GetCandleRange(CandleSettingType.BodyLong, window[_bodyLongAveragePeriod + 1]);

            _bodyDojiPeriodTotal += GetCandleRange(CandleSettingType.BodyDoji, input) -
                                    GetCandleRange(CandleSettingType.BodyDoji, window[_bodyDojiAveragePeriod]);

            return value;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _bodyLongPeriodTotal = BigDecimal.ZERO;
            _bodyDojiPeriodTotal = BigDecimal.ZERO;
            base.Reset();
        }
    }
}
