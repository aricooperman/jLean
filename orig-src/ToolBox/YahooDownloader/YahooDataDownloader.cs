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

namespace QuantConnect.ToolBox.YahooDownloader
{
    /// <summary>
    /// Yahoo Data Downloader class 
    /// </summary>
    public class YahooDataDownloader : IDataDownloader
    {
        //Initialize
        private String _urlPrototype = @"http://ichart.finance.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&g={7}&ignore=.csv";

        /// <summary>
        /// Get historical data enumerable for a single symbol, type and resolution given this start and end time (in UTC).
        /// </summary>
        /// <param name="symbol">Symbol for the data we're looking for.</param>
        /// <param name="resolution">Resolution of the data request</param>
        /// <param name="startUtc">Start time of the data in UTC</param>
        /// <param name="endUtc">End time of the data in UTC</param>
        /// <returns>Enumerable of base data for this symbol</returns>
        public IEnumerable<BaseData> Get(Symbol symbol, Resolution resolution, DateTime startUtc, DateTime endUtc)
        {
            if (resolution != Resolution.Daily)
                throw new ArgumentException("The YahooDataDownloader can only download daily data.");

            if (symbol.ID.SecurityType != SecurityType.Equity)
                throw new NotSupportedException("SecurityType not available: " + symbol.ID.SecurityType);

            if (endUtc < startUtc)
                throw new ArgumentException("The end date must be greater or equal than the start date.");

            // Note: Yahoo syntax requires the month zero-based (0-11)
            url = string.Format(_urlPrototype, symbol.Value, startUtc.Month - 1, startUtc.Day, startUtc.Year, endUtc.Month - 1, endUtc.Day, endUtc.Year, "d");

            using (cl = new WebClient())
            {
                data = cl.DownloadString(url);
                lines = data.Split('\n');

                for (i = lines.Length - 1; i >= 1; i--)
                {
                    str = lines[i].Split(',');
                    if (str.Length < 6) continue;
                    ymd = str[0].Split('-');
                    year = ymd[0].ToInt32();
                    month = ymd[1].ToInt32();
                    day = ymd[2].ToInt32();
                    open = str[1].ToDecimal();
                    high = str[2].ToDecimal();
                    low = str[3].ToDecimal();
                    close = str[4].ToDecimal();
                    volume = str[5].ToInt64();
                    yield return new TradeBar(new DateTime(year, month, day), symbol, open, high, low, close, volume, TimeSpan.FromDays(1));
                }
            }
        }
    }
}
