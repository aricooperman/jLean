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
using System.Net;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using Newtonsoft.Json;
using System.Linq;

package com.quantconnect.lean.ToolBox.CryptoiqDownloader
{
    /**
    /// Cryptoiq Data Downloader class 
    */
    public class CryptoiqDownloader : IDataDownloader
    {
        private final String _exchange;
        private final BigDecimal _scaleFactor;

        /**
        /// Initializes a new instance of the <see cref="CryptoiqDownloader"/> class
        */
         * @param exchange">The bitcoin exchange
         * @param scaleFactor">Scale factor used to scale the data, useful for changing the BTC units
        public CryptoiqDownloader( String exchange = "bitfinex", BigDecimal scaleFactor = 1m) {
            _exchange = exchange;
            _scaleFactor = scaleFactor;
        }

        /**
        /// Get historical data enumerable for a single symbol, type and resolution given this start and end time (in UTC).
        */
         * @param symbol">Symbol for the data we're looking for.
         * @param resolution">Only Tick is currently supported
         * @param startUtc">Start time of the data in UTC
         * @param endUtc">End time of the data in UTC
        @returns Enumerable of base data for this symbol
        public IEnumerable<BaseData> Get(Symbol symbol, Resolution resolution, DateTime startUtc, DateTime endUtc) {
            if( resolution != Resolution.Tick) {
                throw new ArgumentException( "Only tick data is currently supported.");
            }

            hour = 1;
            counter = startUtc;
            static final String url = "http://cryptoiq.io/api/marketdata/ticker/{3}/%3$s/%1$s/%2$s";

            while (counter <= endUtc) {
                while (hour < 24) {
                    using (cl = new WebClient()) {
                        request = String.format(url, counter.toString( "yyyy-MM-dd"), hour, symbol.Value, _exchange);
                        data = cl.DownloadString(request);

                        mbtc = JsonConvert.DeserializeObject<List<CryptoiqBitcoin>>(data);
                        foreach (item in mbtc.OrderBy(x -> x.Time)) {
                            yield return new Tick
                            {
                                Time = item.Time,
                                Symbol = symbol,
                                Value = item.Last/_scaleFactor,
                                AskPrice = item.Ask/_scaleFactor,
                                BidPrice = item.Bid/_scaleFactor,
                                TickType = TickType.Quote
                            };
                        }
                        hour++;
                    }
                }
                counter = counter.AddDays(1);
                hour = 0;
            }
        }

    }
}