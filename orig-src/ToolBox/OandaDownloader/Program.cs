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
using System.Linq;
using QuantConnect.Configuration;
using QuantConnect.Data.Market;
using QuantConnect.Logging;

package com.quantconnect.lean.ToolBox.OandaDownloader
{
    class Program
    {
        /**
         * Primary entry point to the program
        */
        static void Main( String[] args) {
            if( args.Length != 4) {
                Console.WriteLine( "Usage: OandaDownloader SYMBOLS RESOLUTION FROMDATE TODATE");
                Console.WriteLine( "SYMBOLS = eg EURUSD,USDJPY");
                Console.WriteLine( "RESOLUTION = Second/Minute/Hour/Daily/All");
                Console.WriteLine( "FROMDATE = yyyymmdd");
                Console.WriteLine( "TODATE = yyyymmdd");
                Environment.Exit(1);
            }

            try
            {
                // Load settings from command line
                tickers = args[0].split(',');
                allResolutions = args[1].toLowerCase() == "all";
                resolution = allResolutions ? Resolution.Second : (Resolution)Enum.Parse(typeof(Resolution), args[1]);
                startDate = DateTime.ParseExact(args[2], "yyyyMMdd", CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(args[3], "yyyyMMdd", CultureInfo.InvariantCulture);

                // Load settings from config.json
                dataDirectory = Config.Get( "data-directory", "../../../Data");
                accessToken = Config.Get( "access-token", "73eba38ad5b44778f9a0c0fec1a66ed1-44f47f052c897b3e1e7f24196bbc071f");
                accountId =  Integer.parseInt( Config.Get( "account-id", "621396"));

                // Create an instance of the downloader
                static final String market = Market.Oanda;
                downloader = new OandaDataDownloader(accessToken, accountId);

                foreach (ticker in tickers) {
                    if( !downloader.HasSymbol(ticker))
                        throw new IllegalArgumentException( "The symbol " + ticker + " is not available.");
                }

                foreach (ticker in tickers) {
                    // Download the data
                    securityType = downloader.GetSecurityType(ticker);
                    symbol = Symbol.Create(ticker, securityType, market);

                    data = downloader.Get(symbol, resolution, startDate, endDate);

                    if( allResolutions) {
                        bars = data.Cast<TradeBar>().ToList();

                        // Save the data (second resolution)
                        writer = new LeanDataWriter(resolution, symbol, dataDirectory);
                        writer.Write(bars);

                        // Save the data (other resolutions)
                        foreach (res in new[] { Resolution.Minute, Resolution.Hour, Resolution.Daily }) {
                            resData = AggregateBars(symbol, bars, res.ToTimeSpan());

                            writer = new LeanDataWriter(res, symbol, dataDirectory);
                            writer.Write(resData);
                        }
                    }
                    else
                    {
                        // Save the data (single resolution)
                        writer = new LeanDataWriter(resolution, symbol, dataDirectory);
                        writer.Write(data);
                    }
                }
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
         * Aggregates a list of 5-second bars at the requested resolution
        */
         * @param symbol">
         * @param bars">
         * @param resolution">
        @returns 
        private static IEnumerable<TradeBar> AggregateBars(Symbol symbol, IEnumerable<TradeBar> bars, Duration resolution) {
            return
                (from b in bars
                 group b by b.Time.RoundDown(resolution)
                     into g
                     select new TradeBar
                     {
                         Symbol = symbol,
                         Time = g.Key,
                         Open = g.First().Open,
                         High = g.Max(b -> b.High),
                         Low = g.Min(b -> b.Low),
                         Close = g.Last().Close
                     });
        }
    }
}
