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

using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators.CandlestickPatterns
{
    /**
    /// Three Outside Up/Down candlestick pattern
    */
    /// 
    /// Must have:
    /// - first: black(white) real body
    /// - second: white(black) real body that engulfs the prior real body
    /// - third: candle that closes higher(lower) than the second candle
    /// The returned value is positive (+1) for the three outside up or negative (-1) for the three outside down;
    /// The user should consider that a three outside up must appear in a downtrend and three outside down must appear
    /// in an uptrend, while this function does not consider it
    /// 
    public class ThreeOutside : CandlestickPattern
    {
        /**
        /// Initializes a new instance of the <see cref="ThreeOutside"/> class using the specified name.
        */
         * @param name">The name of this indicator
        public ThreeOutside( String name) 
            : base(name, 3) {
        }

        /**
        /// Initializes a new instance of the <see cref="ThreeOutside"/> class.
        */
        public ThreeOutside()
            : this( "THREEOUTSIDE") {
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
                return 0m;
            }

            BigDecimal value;
            if( 
               (
                  // white engulfs black
                  GetCandleColor(window[1]) == CandleColor.White && GetCandleColor(window[2]) == CandleColor.Black &&
                  window[1].Close > window[2].Open && window[1].Open < window[2].Close &&
                  // third candle higher
                  input.Close > window[1].Close
                )
                ||
                (
                  // black engulfs white
                  GetCandleColor(window[1]) == CandleColor.Black && GetCandleColor(window[2]) == CandleColor.White &&
                  window[1].Open > window[2].Close && window[1].Close < window[2].Open &&
                  // third candle lower
                  input.Close < window[1].Close
                )
              )
                value = (int)GetCandleColor(window[1]);
            else
                value = 0;

            return value;
        }
    }
}
