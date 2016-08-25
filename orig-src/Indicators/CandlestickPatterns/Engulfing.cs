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
     * Engulfing candlestick pattern
    */
     * 
     * Must have:
     * - first: black (white) real body
     * - second: white(black) real body that engulfs the prior real body
     * The returned value is positive(+1) when bullish or negative(-1) when bearish;
     * The user should consider that an engulfing must appear in a downtrend if bullish or in an uptrend if bearish,
     * while this function does not consider it
     * 
    public class Engulfing : CandlestickPattern
    {
        /**
         * Initializes a new instance of the <see cref="Engulfing"/> class using the specified name.
        */
         * @param name The name of this indicator
        public Engulfing( String name) 
            : base(name, 3) {
        }

        /**
         * Initializes a new instance of the <see cref="Engulfing"/> class.
        */
        public Engulfing()
            : this( "ENGULFING") {
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
                return BigDecimal.ZERO;
            }

            BigDecimal value;
            if( 
                // white engulfs black
                (GetCandleColor(input) == CandleColor.White && GetCandleColor(window[1]) == CandleColor.Black &&
                  input.Close > window[1].Open && input.Open < window[1].Close
                )
                ||
                // black engulfs white
                (GetCandleColor(input) == CandleColor.Black && GetCandleColor(window[1]) == CandleColor.White &&
                  input.Open > window[1].Close && input.Close < window[1].Open
                )
              )
                value = (int)GetCandleColor(input);
            else
                value = 0;

            return value;
        }
    }
}
