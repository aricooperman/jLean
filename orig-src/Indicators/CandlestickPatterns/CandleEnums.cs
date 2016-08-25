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

package com.quantconnect.lean.Indicators.CandlestickPatterns
{
    /**
     * Types of candlestick settings
    */
    public enum CandleSettingType
    {
        /**
         * Real body is long when it's longer than the average of the 10 previous candles' real body
        */
        BodyLong,

        /**
         * Real body is very long when it's longer than 3 times the average of the 10 previous candles' real body
        */
        BodyVeryLong,

        /**
         * Real body is short when it's shorter than the average of the 10 previous candles' real bodies
        */
        BodyShort,

        /**
         * Real body is like doji's body when it's shorter than 10% the average of the 10 previous candles' high-low range
        */
        BodyDoji,

        /**
         * Shadow is long when it's longer than the real body
        */
        ShadowLong,

        /**
         * Shadow is very long when it's longer than 2 times the real body
        */
        ShadowVeryLong,

        /**
         * Shadow is short when it's shorter than half the average of the 10 previous candles' sum of shadows
        */
        ShadowShort,

        /**
         * Shadow is very short when it's shorter than 10% the average of the 10 previous candles' high-low range
        */
        ShadowVeryShort,

        /**
         * When measuring distance between parts of candles or width of gaps
         * "near" means "&lt;= 20% of the average of the 5 previous candles' high-low range"
        */
        Near,

        /**
         * When measuring distance between parts of candles or width of gaps
         * "far" means "&gt;= 60% of the average of the 5 previous candles' high-low range"
        */
        Far,

        /**
         * When measuring distance between parts of candles or width of gaps
         * "equal" means "&lt;= 5% of the average of the 5 previous candles' high-low range"
        */
        Equal
    }

    /**
     * Types of candlestick ranges
    */
    public enum CandleRangeType
    {
        /**
         * The part of the candle between open and close
        */
        RealBody,

        /**
         * The complete range of the candle
        */
        HighLow,

        /**
         * The shadows (or tails) of the candle
        */
        Shadows
    }

    /**
     * Colors of a candle
    */
    public enum CandleColor
    {
        /**
         * White is an up candle (close higher or equal than open)
        */
        White = 1,

        /**
         * Black is a down candle (close lower than open)
        */
        Black = -1
    }
}
