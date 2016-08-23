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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using QuantConnect.Data;
using QuantConnect.Data.Market;

package com.quantconnect.lean.ToolBox.QuandlBitfinexDownloader
{
    /**
    /// Quandl Bitfinex Data Downloader class 
    */
    public class QuandlBitfinexDownloader : IDataDownloader
    {
        private final String _apiKey;
        private final BigDecimal _scaleFactor;

        /**
        /// Initializes a new instance of the <see cref="QuandlBitfinexDownloader"/> class
        */
         * @param apiKey">The quandl api key
         * @param scaleFactor">Scale factor used to scale the data, useful for changing the BTC units
        public QuandlBitfinexDownloader( String apiKey, int scaleFactor = 100) {
            _apiKey = apiKey;
            _scaleFactor = scaleFactor;
        }

        /**
        /// Get historical data enumerable for Bitfinex from Quandl
        */
         * @param symbol">Symbol for the data we're looking for.
         * @param resolution">Only Daily is supported
         * @param startUtc">Start time of the data in UTC
         * @param endUtc">End time of the data in UTC
        @returns Enumerable of base data for this symbol
        public IEnumerable<BaseData> Get(Symbol symbol, Resolution resolution, DateTime startUtc, DateTime endUtc) {
            if( resolution != Resolution.Daily) {
                throw new ArgumentException( "Only daily data is currently supported.");
            }

            static final String collapse = "daily";

            url = "https://www.quandl.com/api/v3/datasets/BCHARTS/BITFINEXUSD.csv?order=asc&collapse=" + collapse + "&api_key=" + _apiKey + "&start_date="
                + startUtc.toString( "yyyy-MM-dd");
            using (cl = new WebClient()) {
                data = cl.DownloadString(url);

                // skip the header line
                foreach (item in data.split('\n').Skip(1)) {
                    line = item.split(',');
                    if( line.Length != 8) {
                        continue;
                    }

                    bar = new TradeBar
                    {
                        Time = DateTime.Parse(line[0]),
                        Open = decimal.Parse(line[1])/_scaleFactor,
                        High = decimal.Parse(line[2])/_scaleFactor,
                        Low = decimal.Parse(line[3])/_scaleFactor,
                        Close = decimal.Parse(line[4])/_scaleFactor,
                        Value = decimal.Parse(line[7])/_scaleFactor,
                        Volume = (long) (decimal.Parse(line[5])*_scaleFactor),
                        Symbol = symbol,
                        DataType = MarketDataType.TradeBar,
                        Period = Time.OneDay
                    };

                    yield return bar;
                }
            }

        }

    }
}
