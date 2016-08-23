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

using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators.CandlestickPatterns
{
    /**
    /// Doji candlestick pattern indicator
    */
    /// 
    /// Must have:
    /// - open quite equal to close
    /// How much can be the maximum distance between open and close is specified with SetCandleSettings
    /// The returned value is always positive(+1) but this does not mean it is bullish: doji shows uncertainty and it is
    /// neither bullish nor bearish when considered alone
    /// 
    public class Doji : CandlestickPattern
    {
        private final int _bodyDojiAveragePeriod;

        private BigDecimal _bodyDojiPeriodTotal;

        /**
        /// Initializes a new instance of the <see cref="Doji"/> class using the specified name.
        */
         * @param name">The name of this indicator
        public Doji( String name) 
            : base(name, CandleSettings.Get(CandleSettingType.BodyDoji).AveragePeriod + 1) {
            _bodyDojiAveragePeriod = CandleSettings.Get(CandleSettingType.BodyDoji).AveragePeriod;
        }

        /**
        /// Initializes a new instance of the <see cref="Doji"/> class.
        */
        public Doji()
            : this( "DOJI") {
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
                if( Samples >= Period - _bodyDojiAveragePeriod) {
                    _bodyDojiPeriodTotal += GetCandleRange(CandleSettingType.BodyDoji, input);
                }

                return 0m;
            }

            value = GetRealBody(input) <= GetCandleAverage(CandleSettingType.BodyDoji, _bodyDojiPeriodTotal, input) ? 1m : 0m;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyDojiPeriodTotal += GetCandleRange(CandleSettingType.BodyDoji, input) -
                                    GetCandleRange(CandleSettingType.BodyDoji, window[_bodyDojiAveragePeriod]);

            return value;
        }

        /**
        /// Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _bodyDojiPeriodTotal = 0m;
            base.Reset();
        }
    }
}
