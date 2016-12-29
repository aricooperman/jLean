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
using System.Globalization;
using System.Linq;
using QuantConnect.Configuration;
using QuantConnect.Data.Market;
using QuantConnect.Logging;

package com.quantconnect.lean.ToolBox.DukascopyDownloader
{
    class Program
    {
        /**
         * Primary entry point to the program
        */
        static void Main( String[] args) {
            if( args.Length != 4) {
                Console.WriteLine( "Usage: DukascopyDownloader SYMBOLS RESOLUTION FROMDATE TODATE");
                Console.WriteLine( "SYMBOLS = eg EURUSD,USDJPY");
                Console.WriteLine( "RESOLUTION = Tick/Second/Minute/Hour/Daily/All");
                Console.WriteLine( "FROMDATE = yyyymmdd");
                Console.WriteLine( "TODATE = yyyymmdd");
                Environment.Exit(1);
            }

            try
            {
                // Load settings from command line
                symbols = args[0].split(',');
                allResolutions = args[1].toLowerCase().equals( "all";
                resolution = allResolutions ? Resolution.Tick : (Resolution)Enum.Parse(typeof(Resolution), args[1]);
                startDate = DateTime.ParseExact(args[2], "yyyyMMdd", CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(args[3], "yyyyMMdd", CultureInfo.InvariantCulture);

                // Load settings from config.json
                dataDirectory = Config.Get( "data-directory", "../../../Data");

                // Download the data
                static final String market = Market.Dukascopy;
                downloader = new DukascopyDataDownloader();

                foreach (symbol in symbols) {
                    if( !downloader.HasSymbol(symbol))
                        throw new IllegalArgumentException( "The symbol " + symbol + " is not available.");
                }

                foreach (symbol in symbols) {
                    securityType = downloader.GetSecurityType(symbol);
                    symbolObject = Symbol.Create(symbol, securityType, Market.Dukascopy);
                    data = downloader.Get(symbolObject, resolution, startDate, endDate);

                    if( allResolutions) {
                        ticks = data.Cast<Tick>().ToList();

                        // Save the data (tick resolution)
                        writer = new LeanDataWriter(resolution, symbolObject, dataDirectory);
                        writer.Write(ticks);

                        // Save the data (other resolutions)
                        foreach (res in new[] { Resolution.Second, Resolution.Minute, Resolution.Hour, Resolution.Daily }) {
                            resData = DukascopyDataDownloader.AggregateTicks(symbolObject, ticks, res.ToTimeSpan());

                            writer = new LeanDataWriter(res, symbolObject, dataDirectory);
                            writer.Write(resData);
                        }
                    }
                    else
                    {
                        // Save the data (single resolution)
                        writer = new LeanDataWriter(resolution, symbolObject, dataDirectory);
                        writer.Write(data);
                    }
                }
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }
    }
}
