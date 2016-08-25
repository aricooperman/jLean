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
using System.IO;
using System.Linq;
using System.Net;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using SevenZip;

package com.quantconnect.lean.ToolBox.DukascopyDownloader
{
    /**
     * Dukascopy Data Downloader class
    */
    public class DukascopyDataDownloader : IDataDownloader
    {
        private final DukascopySymbolMapper _symbolMapper = new DukascopySymbolMapper();
        private static final int DukascopyTickLength = 20;

        /**
         * Checks if downloader can get the data for the symbol
        */
         * @param symbol">
        @returns Returns true if the symbol is available
        public boolean HasSymbol( String symbol) {
            return _symbolMapper.IsKnownLeanSymbol(Symbol.Create(symbol, GetSecurityType(symbol), Market.Dukascopy));
        }

        /**
         * Gets the security type for the specified symbol
        */
         * @param symbol The symbol
        @returns The security type
        public SecurityType GetSecurityType( String symbol) {
            return _symbolMapper.GetLeanSecurityType(symbol);
        }

        /**
         * Get historical data enumerable for a single symbol, type and resolution given this start and end time (in UTC).
        */
         * @param symbol Symbol for the data we're looking for.
         * @param resolution Resolution of the data request
         * @param startUtc Start time of the data in UTC
         * @param endUtc End time of the data in UTC
        @returns Enumerable of base data for this symbol
        public IEnumerable<BaseData> Get(Symbol symbol, Resolution resolution, DateTime startUtc, DateTime endUtc) {
            if( !_symbolMapper.IsKnownLeanSymbol(symbol))
                throw new IllegalArgumentException( "Invalid symbol requested: " + symbol.Value);

            if( symbol.ID.SecurityType != SecurityType.Forex && symbol.ID.SecurityType != SecurityType.Cfd)
                throw new NotSupportedException( "SecurityType not available: " + symbol.ID.SecurityType);

            if( endUtc < startUtc)
                throw new IllegalArgumentException( "The end date must be greater or equal to the start date.");

            // set the starting date
            DateTime date = startUtc;

            // loop until last date
            while (date <= endUtc) {
                // request all ticks for a specific date
                ticks = DownloadTicks(symbol, date);

                switch (resolution) {
                    case Resolution.Tick:
                        foreach (tick in ticks) {
                            yield return new Tick(tick.Time, symbol, tick.BidPrice, tick.AskPrice);
                        }
                        break;

                    case Resolution.Second:
                    case Resolution.Minute:
                    case Resolution.Hour:
                    case Resolution.Daily:
                        foreach (bar in AggregateTicks(symbol, ticks, resolution.ToTimeSpan())) {
                            yield return bar;
                        }
                        break;
                }

                date = date.AddDays(1);
            }
        }

        /**
         * Aggregates a list of ticks at the requested resolution
        */
         * @param symbol">
         * @param ticks">
         * @param resolution">
        @returns 
        internal static IEnumerable<TradeBar> AggregateTicks(Symbol symbol, IEnumerable<Tick> ticks, Duration resolution) {
            return 
                (from t in ticks
                 group t by t.Time.RoundDown(resolution)
                     into g
                     select new TradeBar
                     {
                         Symbol = symbol,
                         Time = g.Key,
                         Open = g.First().LastPrice,
                         High = g.Max(t -> t.LastPrice),
                         Low = g.Min(t -> t.LastPrice),
                         Close = g.Last().LastPrice
                     });
        }

        /**
         * Downloads all ticks for the specified date
        */
         * @param symbol The requested symbol
         * @param date The requested date
        @returns An enumerable of ticks
        private IEnumerable<Tick> DownloadTicks(Symbol symbol, DateTime date) {
            dukascopySymbol = _symbolMapper.GetBrokerageSymbol(symbol);
            pointValue = _symbolMapper.GetPointValue(symbol);

            for (hour = 0; hour < 24; hour++) {
                timeOffset = hour * 3600000;

                url = String.format(@"http://www.dukascopy.com/datafeed/%1$s/{1:D4}/{2:D2}/{3:D2}/{4:D2}h_ticks.bi5",
                    dukascopySymbol, date.Year, date.Month - 1, date.Day, hour);

                using (client = new WebClient()) {
                    byte[] bytes;
                    try
                    {
                        bytes = client.DownloadData(url);
                    }
                    catch (Exception exception) {
                        Log.Error(exception);
                        yield break;
                    }
                    if( bytes != null && bytes.Length > 0) {
                        ticks = AppendTicksToList(symbol, bytes, date, timeOffset, pointValue);
                        foreach (tick in ticks) {
                            yield return tick;
                        }
                    }
                }
            }
        }

        /**
         * Reads ticks from a Dukascopy binary buffer into a list
        */
         * @param symbol The symbol
         * @param bytesBi5 The buffer in binary format
         * @param date The date for the ticks
         * @param timeOffset The time offset in milliseconds
         * @param pointValue The price multiplier
        private static unsafe List<Tick> AppendTicksToList(Symbol symbol, byte[] bytesBi5, DateTime date, int timeOffset, double pointValue) {
            ticks = new List<Tick>();

            using (inStream = new MemoryStream(bytesBi5)) {
                using (outStream = new MemoryStream()) {
                    SevenZipExtractor.DecompressStream(inStream, outStream, (int)inStream.Length, null );

                    byte[] bytes = outStream.GetBuffer();
                    int count = bytes.Length / DukascopyTickLength;

                    // Numbers are big-endian
                    // ii1 = milliseconds within the hour
                    // ii2 = AskPrice * point value
                    // ii3 = BidPrice * point value
                    // ff1 = AskVolume (not used)
                    // ff2 = BidVolume (not used)

                    fixed (byte* pBuffer = &bytes[0]) {
                        uint* p = (uint*)pBuffer;

                        for (int i = 0; i < count; i++) {
                            ReverseBytes(p); UnsignedInt time = *p++;
                            ReverseBytes(p); UnsignedInt ask = *p++;
                            ReverseBytes(p); UnsignedInt bid = *p++;
                            p++; p++;

                            if( bid > 0 && ask > 0) {
                                ticks.Add(new Tick(
                                    date.AddMilliseconds(timeOffset + time), 
                                    symbol, 
                                    new BigDecimal( bid / pointValue), 
                                    new BigDecimal( ask / pointValue)));
                            }
                        }
                    }
                }
            }

            return ticks;
        }

        /**
         * Converts a 32-bit unsigned integer from big-endian to little-endian (and vice-versa)
        */
         * @param p Pointer to the integer value
        private static unsafe void ReverseBytes(uint* p) {
            *p = (*p & 0x000000FF) << 24 | (*p & 0x0000FF00) << 8 | (*p & 0x00FF0000) >> 8 | (*p & 0xFF000000) >> 24;
        }

    }
}
