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

using System.Collections.Generic;

package com.quantconnect.lean.Indicators.CandlestickPatterns
{
    /**
    /// Candle settings for all candlestick patterns
    */
    public static class CandleSettings
    {
        /**
        /// Default settings for all candle setting types
        */
        private static final Map<CandleSettingType, CandleSetting> DefaultSettings = new Map<CandleSettingType, CandleSetting>
        {
            { CandleSettingType.BodyLong, new CandleSetting(CandleRangeType.RealBody, 10, 1m) },
            { CandleSettingType.BodyVeryLong, new CandleSetting(CandleRangeType.RealBody, 10, 3m) },
            { CandleSettingType.BodyShort, new CandleSetting(CandleRangeType.RealBody, 10, 1m) },
            { CandleSettingType.BodyDoji, new CandleSetting(CandleRangeType.HighLow, 10, 0.1m) },
            { CandleSettingType.ShadowLong, new CandleSetting(CandleRangeType.RealBody, 0, 1m) },
            { CandleSettingType.ShadowVeryLong, new CandleSetting(CandleRangeType.RealBody, 0, 2m) },
            { CandleSettingType.ShadowShort, new CandleSetting(CandleRangeType.Shadows, 10, 1m) },
            { CandleSettingType.ShadowVeryShort, new CandleSetting(CandleRangeType.HighLow, 10, 0.1m) },
            { CandleSettingType.Near, new CandleSetting(CandleRangeType.HighLow, 5, 0.2m) },
            { CandleSettingType.Far, new CandleSetting(CandleRangeType.HighLow, 5, 0.6m) },
            { CandleSettingType.Equal, new CandleSetting(CandleRangeType.HighLow, 5, 0.05m) }
        };

        /**
        /// Returns the candle setting for the requested type
        */
         * @param type">The candle setting type
        public static CandleSetting Get(CandleSettingType type) {
            CandleSetting setting;
            DefaultSettings.TryGetValue(type, out setting);
            return setting;
        }

        /**
        /// Changes the default candle setting for the requested type
        */
         * @param type">The candle setting type
         * @param setting">The candle setting
        public static void Set(CandleSettingType type, CandleSetting setting) {
            DefaultSettings[type] = setting;
        }
    }

    /**
    /// Represents a candle setting
    */
    public class CandleSetting
    {
        /**
        /// The candle range type
        */
        public CandleRangeType RangeType
        {
            get;
            private set;
        }

        /**
        /// The number of previous candles to average
        */
        public int AveragePeriod
        {
            get;
            private set;
        }

        /**
        /// A multiplier to calculate candle ranges
        */
        public BigDecimal Factor
        {
            get;
            private set;
        }

        /**
        /// Creates an instance of the <see cref="CandleSetting"/> class
        */
         * @param rangeType">The range type
         * @param averagePeriod">The average period
         * @param factor">The factor
        public CandleSetting(CandleRangeType rangeType, int averagePeriod, BigDecimal factor) {
            RangeType = rangeType;
            AveragePeriod = averagePeriod;
            Factor = factor;
        }
    }
}
