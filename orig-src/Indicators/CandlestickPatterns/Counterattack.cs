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
     * Counterattack candlestick pattern
    */
     * 
     * Must have:
     * - first candle: long black (white)
     * - second candle: long white(black) with close equal to the prior close
     * The meaning of "equal" and "long" is specified with SetCandleSettings
     * The returned value is positive(+1) when bullish or negative(-1) when bearish;
     * The user should consider that counterattack is significant in a trend, while this function does not consider it
     * 
    public class Counterattack : CandlestickPattern
    {
        private final int _equalAveragePeriod;
        private final int _bodyLongAveragePeriod;

        private BigDecimal _equalPeriodTotal;
        private decimal[] _bodyLongPeriodTotal = new decimal[2];

        /**
         * Initializes a new instance of the <see cref="Counterattack"/> class using the specified name.
        */
         * @param name The name of this indicator
        public Counterattack( String name) 
            : base(name, Math.Max(CandleSettings.Get(CandleSettingType.Equal).AveragePeriod, CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod) + 1 + 1) {
            _equalAveragePeriod = CandleSettings.Get(CandleSettingType.Equal).AveragePeriod;
            _bodyLongAveragePeriod = CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod;
        }

        /**
         * Initializes a new instance of the <see cref="Counterattack"/> class.
        */
        public Counterattack()
            : this( "COUNTERATTACK") {
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
                if( Samples >= Period - _equalAveragePeriod) {
                    _equalPeriodTotal += GetCandleRange(CandleSettingType.Equal, window[1]);
                }

                if( Samples >= Period - _bodyLongAveragePeriod) {
                    _bodyLongPeriodTotal[1] += GetCandleRange(CandleSettingType.BodyLong, window[1]);
                    _bodyLongPeriodTotal[0] += GetCandleRange(CandleSettingType.BodyLong, input);
                }

                return BigDecimal.ZERO;
            }

            BigDecimal value;
            if( 
                // opposite candles
                (int)GetCandleColor(window[1]) == -(int)GetCandleColor(input) &&
                // 1st long
                GetRealBody(window[1]) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal[1], window[1]) &&
                // 2nd long
                GetRealBody(input) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal[0], input) &&
                // equal closes
                input.Close <= window[1].Close + GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal, window[1]) &&
                input.Close >= window[1].Close - GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal, window[1])
              )
                value = (int)GetCandleColor(input);
            else
                value = BigDecimal.ZERO;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _equalPeriodTotal += GetCandleRange(CandleSettingType.Equal, input) -
                                 GetCandleRange(CandleSettingType.Equal, window[_equalAveragePeriod + 1]);

            for (i = 1; i >= 0; i--) {
                _bodyLongPeriodTotal[i] += GetCandleRange(CandleSettingType.BodyLong, window[i]) -
                                           GetCandleRange(CandleSettingType.BodyLong, window[i + _bodyLongAveragePeriod]);
            }

            return value;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _equalPeriodTotal = 0;
            _bodyLongPeriodTotal = new decimal[2];
            base.Reset();
        }
    }
}
