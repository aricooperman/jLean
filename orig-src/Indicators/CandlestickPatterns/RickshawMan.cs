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
     * Rickshaw Man candlestick pattern
    */
     * 
     * Must have:
     * - doji body
     * - two long shadows
     * - body near the midpoint of the high-low range
     * The meaning of "doji" and "near" is specified with SetCandleSettings
     * The returned value is always positive(+1) but this does not mean it is bullish: rickshaw man shows uncertainty
     * 
    public class RickshawMan : CandlestickPattern
    {
        private final int _bodyDojiAveragePeriod;
        private final int _shadowLongAveragePeriod;
        private final int _nearAveragePeriod;

        private BigDecimal _bodyDojiPeriodTotal;
        private BigDecimal _shadowLongPeriodTotal;
        private BigDecimal _nearPeriodTotal;

        /**
         * Initializes a new instance of the <see cref="RickshawMan"/> class using the specified name.
        */
         * @param name The name of this indicator
        public RickshawMan( String name)
            : base(name, Math.Max(Math.Max(CandleSettings.Get(CandleSettingType.BodyDoji).AveragePeriod, CandleSettings.Get(CandleSettingType.ShadowLong).AveragePeriod),
                  CandleSettings.Get(CandleSettingType.Near).AveragePeriod) + 1) {
            _bodyDojiAveragePeriod = CandleSettings.Get(CandleSettingType.BodyDoji).AveragePeriod;
            _shadowLongAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowLong).AveragePeriod;
            _nearAveragePeriod = CandleSettings.Get(CandleSettingType.Near).AveragePeriod;
        }

        /**
         * Initializes a new instance of the <see cref="RickshawMan"/> class.
        */
        public RickshawMan()
            : this( "RICKSHAWMAN") {
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

                if( Samples >= Period - _shadowLongAveragePeriod) {
                    _shadowLongPeriodTotal += GetCandleRange(CandleSettingType.ShadowLong, input);
                }

                if( Samples >= Period - _nearAveragePeriod) {
                    _nearPeriodTotal += GetCandleRange(CandleSettingType.Near, input);
                }

                return BigDecimal.ZERO;
            }

            BigDecimal value;
            if( 
                // doji
                GetRealBody(input) <= GetCandleAverage(CandleSettingType.BodyDoji, _bodyDojiPeriodTotal, input) &&
                // long shadow
                GetLowerShadow(input) > GetCandleAverage(CandleSettingType.ShadowLong, _shadowLongPeriodTotal, input) &&
                // long shadow
                GetUpperShadow(input) > GetCandleAverage(CandleSettingType.ShadowLong, _shadowLongPeriodTotal, input) &&
                // body near midpoint
                (
                    Math.Min(input.Open, input.Close)
                        <= input.Low + GetHighLowRange(input) / 2 + GetCandleAverage(CandleSettingType.Near, _nearPeriodTotal, input)
                    &&
                    Math.Max(input.Open, input.Close)
                        >= input.Low + GetHighLowRange(input) / 2 - GetCandleAverage(CandleSettingType.Near, _nearPeriodTotal, input)
                )
              )
                value = 1m;
            else
                value = BigDecimal.ZERO;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyDojiPeriodTotal += GetCandleRange(CandleSettingType.BodyDoji, input) -
                                    GetCandleRange(CandleSettingType.BodyDoji, window[_bodyDojiAveragePeriod]);

            _shadowLongPeriodTotal += GetCandleRange(CandleSettingType.ShadowLong, input) -
                                      GetCandleRange(CandleSettingType.ShadowLong, window[_shadowLongAveragePeriod]);

            _nearPeriodTotal += GetCandleRange(CandleSettingType.Near, input) -
                                GetCandleRange(CandleSettingType.Near, window[_nearAveragePeriod]);

            return value;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _bodyDojiPeriodTotal = 0;
            _shadowLongPeriodTotal = 0;
            _nearPeriodTotal = 0;
            base.Reset();
        }
    }
}
