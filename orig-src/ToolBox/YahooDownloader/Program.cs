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
using QuantConnect.Configuration;
using QuantConnect.Logging;

package com.quantconnect.lean.ToolBox.YahooDownloader
{
    class Program
    {
        /// <summary>
        /// Yahoo Downloader Toolbox Project For LEAN Algorithmic Trading Engine.
        /// Original by @chrisdk2015, tidied by @jaredbroad
        /// </summary>
        static void Main( String[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: YahooDownloader SYMBOLS RESOLUTION FROMDATE TODATE");
                Console.WriteLine("SYMBOLS = eg SPY,AAPL");
                Console.WriteLine("RESOLUTION = Daily");
                Console.WriteLine("FROMDATE = yyyymmdd");
                Console.WriteLine("TODATE = yyyymmdd");
                Environment.Exit(1);
            }

            try
            {
                // Load settings from command line
                symbols = args[0].Split(',');
                resolution = (Resolution)Enum.Parse(typeof(Resolution), args[1]);
                startDate = DateTime.ParseExact(args[2], "yyyyMMdd", CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(args[3], "yyyyMMdd", CultureInfo.InvariantCulture);

                // Load settings from config.json
                dataDirectory = Config.Get("data-directory", "../../../Data");

                // Create an instance of the downloader
                static final String market = Market.USA;
                downloader = new YahooDataDownloader();

                foreach (symbol in symbols)
                {
                    // Download the data
                    symbolObject = Symbol.Create(symbol, SecurityType.Equity, market);
                    data = downloader.Get(symbolObject, resolution, startDate, endDate);

                    // Save the data
                    writer = new LeanDataWriter(resolution, symbolObject, dataDirectory);
                    writer.Write(data);
                }
            }
            catch (Exception err)
            {
                Log.Error(err);
            }
        }
    }
}
