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
using System.Globalization;
using QuantConnect.Configuration;
using QuantConnect.Logging;

package com.quantconnect.lean.ToolBox.QuandlBitfinexDownloader
{
    class Program
    {
        /**
         * Quandl Bitfinex Toolbox Project For LEAN Algorithmic Trading Engine.
        */
        static void Main( String[] args) {
            if( args.Length != 2) {
                Console.WriteLine( "Usage: Downloader FROMDATE APIKEY");
                Console.WriteLine( "FROMDATE = yyyymmdd");
                Environment.Exit(1);
            }

            try
            {
                // Load settings from config.json
                dataDirectory = Config.Get( "data-directory", "../../../Data");
                scaleFactor = Config.GetInt( "bitfinex-scale-factor", 100);

                // Create an instance of the downloader
                static final String market = Market.Bitfinex;
                downloader = new QuandlBitfinexDownloader(args[1], scaleFactor);

                // Download the data
                symbol = Symbol.Create( "BTCUSD", SecurityType.Forex, market);
                data = downloader.Get(symbol, Resolution.Daily, DateTime.ParseExact(args[0], "yyyyMMdd", CultureInfo.CurrentCulture), DateTime.UtcNow);

                // Save the data
                writer = new LeanDataWriter(Resolution.Daily, symbol, dataDirectory, TickType.Quote);
                writer.Write(data);
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }
    }
}
