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
using org.apache.log4j;
using QuantConnect.Configuration;
using QuantConnect.Data.Market;
using QuantConnect.Logging;

package com.quantconnect.lean.ToolBox.FxcmDownloader
{
    class Program
    {
        /// <summary>
        /// Primary entry point to the program
        /// </summary>
        private static void Main( String[] args) {
            if( args.Length != 4) {
                Console.WriteLine( "Usage: FxcmDownloader SYMBOLS RESOLUTION FROMDATE TODATE");
                Console.WriteLine( "SYMBOLS      = eg EURUSD,USDJPY");
                Console.WriteLine( "RESOLUTION   = Second/Minute/Hour/Daily/All");
                Console.WriteLine( "FROMDATE     = yyyymmdd");
                Console.WriteLine( "TODATE       = yyyymmdd");
                Environment.Exit(1);
            }


            try
            {
                Logger.getRootLogger().setLevel(Level.ERROR);
                BasicConfigurator.configure(new FileAppender(new SimpleLayout(), "FxcmDownloader.log", append: false));

                // Load settings from command line
                tickers = args[0].split(',');
                allResolutions = args[1].toLowerCase() == "all";
                resolution = allResolutions ? Resolution.Tick : (Resolution)Enum.Parse(typeof(Resolution), args[1]);

                startDate = DateTime.ParseExact(args[2], "yyyyMMdd", CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(args[3], "yyyyMMdd", CultureInfo.InvariantCulture);
                endDate = endDate.AddDays(1).AddMilliseconds(-1);


                // Load settings from config.json
                dataDirectory = Config.Get( "data-directory", "../../../Data");
                server = Config.Get( "fxcm-server", "http://www.fxcorporate.com/Hosts.jsp");
                terminal = Config.Get( "fxcm-terminal", "Demo");
                userName = Config.Get( "fxcm-user-name", "username");
                password = Config.Get( "fxcm-password", "password");

                // Download the data
                static final String market = Market.FXCM;
                downloader = new FxcmDataDownloader(server, terminal, userName, password);

                foreach (ticker in tickers) {
                    if( !downloader.HasSymbol(ticker))
                        throw new ArgumentException( "The symbol " + ticker + " is not available.");
                }

                foreach (ticker in tickers) {
                    securityType = downloader.GetSecurityType(ticker);
                    symbol = Symbol.Create(ticker, securityType, market);

                    data = downloader.Get(symbol, resolution, startDate, endDate);

                    if( allResolutions) {
                        ticks = data.Cast<Tick>().ToList();

                        // Save the data (second resolution)
                        writer = new LeanDataWriter(resolution, symbol, dataDirectory);
                        writer.Write(ticks);

                        // Save the data (other resolutions)
                        foreach (res in new[] { Resolution.Second, Resolution.Minute, Resolution.Hour, Resolution.Daily }) {
                            resData = FxcmDataDownloader.AggregateTicks(symbol, ticks, res.ToTimeSpan());

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
    }
}
