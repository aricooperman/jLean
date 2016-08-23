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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.ToolBox.OandaDownloader.OandaRestLibrary;
using QuantConnect.Brokerages.Oanda;

package com.quantconnect.lean.ToolBox.OandaDownloader
{
    /**
    /// Oanda Data Downloader class
    */
    public class OandaDataDownloader : IDataDownloader
    {
        private final OandaSymbolMapper _symbolMapper = new OandaSymbolMapper();
        private static final int BarsPerRequest = 5000;

        /**
        /// Initializes a new instance of the <see cref="OandaDataDownloader"/> class
        */
        public OandaDataDownloader( String accessToken, int accountId) {
            // Set Oanda account credentials
            Credentials.SetCredentials(EEnvironment.Practice, accessToken, accountId);
        }

        /**
        /// Checks if downloader can get the data for the Lean symbol
        */
         * @param symbol">The Lean symbol
        @returns Returns true if the symbol is available
        public boolean HasSymbol( String symbol) {
            return _symbolMapper.IsKnownLeanSymbol(Symbol.Create(symbol, GetSecurityType(symbol), Market.Oanda));
        }

        /**
        /// Gets the security type for the specified Lean symbol
        */
         * @param symbol">The Lean symbol
        @returns The security type
        public SecurityType GetSecurityType( String symbol) {
            return _symbolMapper.GetLeanSecurityType(symbol);
        }

        /**
        /// Get historical data enumerable for a single symbol, type and resolution given this start and end time (in UTC).
        */
         * @param symbol">Symbol for the data we're looking for.
         * @param resolution">Resolution of the data request
         * @param startUtc">Start time of the data in UTC
         * @param endUtc">End time of the data in UTC
        @returns Enumerable of base data for this symbol
        public IEnumerable<BaseData> Get(Symbol symbol, Resolution resolution, DateTime startUtc, DateTime endUtc) {
            if( !_symbolMapper.IsKnownLeanSymbol(symbol))
                throw new ArgumentException( "Invalid symbol requested: " + symbol.Value);

            if( resolution == Resolution.Tick)
                throw new NotSupportedException( "Resolution not available: " + resolution);

            if( symbol.ID.SecurityType != SecurityType.Forex && symbol.ID.SecurityType != SecurityType.Cfd)
                throw new NotSupportedException( "SecurityType not available: " + symbol.ID.SecurityType);

            if( endUtc < startUtc)
                throw new ArgumentException( "The end date must be greater or equal than the start date.");

            barsTotalInPeriod = new List<Candle>();
            barsToSave = new List<Candle>();

            // set the starting date/time
            DateTime date = startUtc;
            DateTime startDateTime = date;

            // loop until last date
            while (startDateTime <= endUtc.AddDays(1)) {
                String start = startDateTime.toString( "yyyy-MM-ddTHH:mm:ssZ");

                // request blocks of 5-second bars with a starting date/time
                oandaSymbol = _symbolMapper.GetBrokerageSymbol(symbol);
                bars = DownloadBars(oandaSymbol, start, BarsPerRequest);
                if( bars.Count == 0)
                    break;

                groupedBars = GroupBarsByDate(bars);

                if( groupedBars.Count > 1) {
                    // we received more than one day, so we save the completed days and continue
                    while (groupedBars.Count > 1) {
                        currentDate = groupedBars.Keys.First();
                        if( currentDate > endUtc)
                            break;

                        barsToSave.AddRange(groupedBars[currentDate]);

                        barsTotalInPeriod.AddRange(barsToSave);

                        barsToSave.Clear();

                        // remove the completed date 
                        groupedBars.Remove(currentDate);
                    }

                    // update the current date
                    date = groupedBars.Keys.First();

                    if( date <= endUtc) {
                        barsToSave.AddRange(groupedBars[date]);
                    }
                }
                else
                {
                    currentDate = groupedBars.Keys.First();
                    if( currentDate > endUtc)
                        break;

                    // update the current date
                    date = currentDate;

                    barsToSave.AddRange(groupedBars[date]);
                }

                // calculate the next request datetime (next 5-sec bar time)
                startDateTime = GetDateTimeFromString(bars[bars.Count - 1].time).AddSeconds(5);
            }

            if( barsToSave.Count > 0) {
                barsTotalInPeriod.AddRange(barsToSave);
            }

            switch (resolution) {
                case Resolution.Second:
                case Resolution.Minute:
                case Resolution.Hour:
                case Resolution.Daily:
                    foreach (bar in AggregateBars(symbol, barsTotalInPeriod, resolution.ToTimeSpan())) {
                        yield return bar;
                    }
                    break;
            }
        }

        /**
        /// Aggregates a list of 5-second bars at the requested resolution
        */
         * @param symbol">
         * @param bars">
         * @param resolution">
        @returns 
        private static IEnumerable<TradeBar> AggregateBars(Symbol symbol, List<Candle> bars, Duration resolution) {
            return
                (from b in bars
                 group b by GetDateTimeFromString(b.time).RoundDown(resolution)
                     into g
                     select new TradeBar
                     {
                         Symbol = symbol,
                         Time = g.Key,
                         Open = new BigDecimal( g.First().openMid),
                         High = new BigDecimal( g.Max(b -> b.highMid)),
                         Low = new BigDecimal( g.Min(b -> b.lowMid)),
                         Close = new BigDecimal( g.Last().closeMid)
                     });
        }

        /**
        /// Groups a list of bars into a dictionary keyed by date
        */
         * @param bars">
        @returns 
        private static SortedMap<DateTime, List<Candle>> GroupBarsByDate(List<Candle> bars) {
            groupedBars = new SortedMap<DateTime, List<Candle>>();

            foreach (bar in bars) {
                date = GetDateTimeFromString(bar.time).Date;

                if( !groupedBars.ContainsKey(date))
                    groupedBars[date] = new List<Candle>();

                groupedBars[date].Add(bar);
            }

            return groupedBars;
        }

        /**
        /// Returns a DateTime from an RFC3339 String (with microsecond resolution)
        */
         * @param time">
        private static DateTime GetDateTimeFromString( String time) {
            return DateTime.ParseExact(time, "yyyy-MM-dd'T'HH:mm:ss.000000'Z'", CultureInfo.InvariantCulture);
        }

        /**
        /// Downloads a block of 5-second bars from a starting datetime
        */
         * @param oandaSymbol">
         * @param start">
         * @param barsPerRequest">
        @returns 
        private static List<Candle> DownloadBars( String oandaSymbol, String start, int barsPerRequest) {
            request = new CandlesRequest
            {
                instrument = oandaSymbol,
                granularity = EGranularity.S5,
                candleFormat = ECandleFormat.midpoint,
                count = barsPerRequest,
                start = Uri.EscapeDataString(start)
            };
            return GetCandles(request);
        }

        /**
        /// More detailed request to retrieve candles
        */
         * @param request">the request data to use when retrieving the candles
        @returns List of Candles received (or empty list)
        public static List<Candle> GetCandles(CandlesRequest request) {
            String requestString = Credentials.GetDefaultCredentials().GetServer(EServer.Rates) + request.GetRequestString();

            CandlesResponse candlesResponse = MakeRequest<CandlesResponse>(requestString);
            List<Candle> candles = new List<Candle>();
            if( candlesResponse != null ) {
                candles.AddRange(candlesResponse.candles);
            }
            return candles;
        }

        /**
        /// Primary (internal) request handler
        */
        /// <typeparam name="T">The response type</typeparam>
         * @param requestString">the request to make
         * @param method">method for the request (defaults to GET)
         * @param requestParams">optional parameters (note that if provided, it's assumed the requestString doesn't contain any)
        @returns response via type T
        private static T MakeRequest<T>( String requestString, String method = "GET", Map<String,String> requestParams = null ) {
            if( requestParams != null && requestParams.Count > 0) {
                parameters = CreateParamString(requestParams);
                requestString = requestString + "?" + parameters;
            }
            HttpWebRequest request = WebRequest.CreateHttp(requestString);
            request.Headers[HttpRequestHeader.Authorization] = "Bearer " + Credentials.GetDefaultCredentials().AccessToken;
            request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            request.Method = method;

            try
            {
                using (WebResponse response = request.GetResponse()) {
                    stream = GetResponseStream(response);
                    reader = new StreamReader(stream);
                    result = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(result);
                }
            }
            catch (WebException ex) {
                stream = GetResponseStream(ex.Response);
                reader = new StreamReader(stream);
                result = reader.ReadToEnd();
                throw new Exception(result);
            }
        }

        private static Stream GetResponseStream(WebResponse response) {
            stream = response.GetResponseStream();
            if( response.Headers["Content-Encoding"] == "gzip") {	// if we received a gzipped response, handle that
                stream = new GZipStream(stream, CompressionMode.Decompress);
            }
            return stream;
        }

        /**
        /// Helper function to create the parameter String out of a dictionary of parameters
        */
         * @param requestParams">the parameters to convert
        @returns string containing all the parameters for use in requests
        private static String CreateParamString(Map<String,String> requestParams) {
            return String.join( ",", requestParams.Select(x -> x.Key + "=" + x.Value).Select(WebUtility.UrlEncode));
        }


    }
}
