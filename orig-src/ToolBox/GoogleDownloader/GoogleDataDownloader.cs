﻿/*
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
using System.Net;
using QuantConnect.Data;
using QuantConnect.Data.Market;

package com.quantconnect.lean.ToolBox.GoogleDownloader
{
    /**
     * Google Data Downloader class
    */
    public class GoogleDataDownloader : IDataDownloader
    {
        // q = SYMBOL
        // i = resolution in seconds
        // p = period in days
        // ts = start time
        // Strangely Google forces CHLO format instead of normal OHLC.
        private static final String UrlPrototype = @"http://www.google.com/finance/getprices?q=%1$s&i=%2$s&p=%3$sd&f=d,c,h,l,o,v&ts=%4$s";

        /**
         * Get historical data enumerable for a single symbol, type and resolution given this start and end time (in UTC).
        */
         * @param symbol Symbol for the data we're looking for.
         * @param resolution Resolution of the data request
         * @param startUtc Start time of the data in UTC
         * @param endUtc End time of the data in UTC
        @returns Enumerable of base data for this symbol
        public IEnumerable<BaseData> Get(Symbol symbol, Resolution resolution, DateTime startUtc, DateTime endUtc) {
            if( resolution != Resolution.Minute && resolution != Resolution.Hour)
                throw new NotSupportedException( "Resolution not available: " + resolution);

            if( symbol.ID.SecurityType != SecurityType.Equity)
                throw new NotSupportedException( "SecurityType not available: " + symbol.ID.SecurityType);

            if( endUtc < startUtc)
                throw new IllegalArgumentException( "The end date must be greater or equal than the start date.");

            numberOfDays = (int)(endUtc - startUtc).TotalDays;
            resolutionSeconds = (int)resolution.ToTimeSpan().TotalSeconds;
            endUnixTime = ToUnixTime(endUtc);

            // Create the Google formatted URL.
            url = String.format(UrlPrototype, symbol.Value, resolutionSeconds, numberOfDays, endUnixTime);

            // Download the data from Google.
            string[] lines;
            using (client = new WebClient()) {
                data = client.DownloadString(url);
                lines = data.split('\n');
            }

            // First 7 lines are headers 
            currentLine = 7;

            while (currentLine < lines.Length - 1) {
                firstPass = true;

                // Each day google starts date time at 930am and then 
                // has 390 minutes over the day. Look for the starter rows "a".
                columns = lines[currentLine].split(',');
                startTime = FromUnixTime(columns[0].Remove(0, 1) Long.parseLong(  ));

                while (currentLine < lines.Length - 1) {
                    str = lines[currentLine].split(',');
                    if( str.Length < 6)
                        throw new InvalidDataException( "Short record: " + str);

                    // If its the start of a new day, break out of this sub-loop.
                    titleRow = str[0][0] == 'a';
                    if( titleRow && !firstPass) 
                        break;

                    firstPass = false;

                    // Build the current datetime, from the row offset
                    time = startTime.AddSeconds(resolutionSeconds * (titleRow ? 0 : str[0] Long.parseLong(  )));

                    // Bar: d0, c1, h2, l3, o4, v5
                    open = str[4] new BigDecimal(  );
                    high = str[2] new BigDecimal(  );
                    low = str[3] new BigDecimal(  );
                    close = str[1] new BigDecimal(  );
                    volume = str[5] Long.parseLong(  );

                    currentLine++;

                    yield return new TradeBar(time, symbol, open, high, low, close, volume, resolution.ToTimeSpan());
                }
            }
        }

        /**
         * Convert a DateTime object into a Unix time long value
        */
         * @param utcDateTime The DateTime object (UTC)
        @returns A Unix long time value.
         * When we move to NET 4.6, we can replace this with DateTimeOffset.ToUnixTimeSeconds()
        private static long ToUnixTime(DateTime utcDateTime) {
            epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(utcDateTime - epoch).TotalSeconds;
        }

        /**
         * Convert a Unix time long value into a DateTime object
        */
         * @param unixTime Unix long time.
        @returns A DateTime value (UTC)
         * When we move to NET 4.6, we can replace this with DateTimeOffset.FromUnixTimeSeconds()
        private static DateTime FromUnixTime(long unixTime) {
            epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

    }
}
