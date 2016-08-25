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
using System.IO;
using QuantConnect.Securities;

package com.quantconnect.lean.Data.UniverseSelection
{
    /**
     * Defines summary information about a single symbol for a given date
    */
    public class CoarseFundamental : BaseData
    {
        /**
         * Gets the market for this symbol
        */
        public String Market { get; set; }

        /**
         * Gets the day's dollar volume for this symbol
        */
        public BigDecimal DollarVolume { get; set; }

        /**
         * Gets the day's total volume
        */
        public long Volume { get; set; }

        /**
         * The end time of this data.
        */
        public @Override DateTime EndTime
        {
            get { return Time + QuantConnect.Time.OneDay; }
            set { Time = value - QuantConnect.Time.OneDay; }
        }

        /**
         * Initializes a new instance of the <see cref="CoarseFundamental"/> class
        */
        public CoarseFundamental() {
            DataType = MarketDataType.Auxiliary;
        }

        /**
         * Return the URL String source of the file. This will be converted to a stream 
        */
         * @param config Configuration object
         * @param date Date of this source file
         * @param isLiveMode true if we're in live mode, false for backtesting mode
        @returns String URL of source file.
        public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            path = Path.Combine(Globals.DataFolder, "equity", config.Market, "fundamental", "coarse", date.toString( "yyyyMMdd") + ".csv");
            return new SubscriptionDataSource(path, SubscriptionTransportMedium.LocalFile, FileFormat.Csv);
        }

        /**
         * Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
         * each time it is called. 
        */
         * @param config Subscription data config setup object
         * @param line Line of the source document
         * @param date Date of the requested data
         * @param isLiveMode true if we're in live mode, false for backtesting mode
        @returns Instance of the T:BaseData object generated by this line of the CSV
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
            try
            {
                csv = line.split(',');
                return new CoarseFundamental
                {
                    Symbol = new Symbol(SecurityIdentifier.Parse(csv[0]), csv[1]),
                    Time = date,
                    Market = config.Market,
                    Value = csv[2] new BigDecimal(  ),
                    Volume = csv[3] Long.parseLong(  ),
                    DollarVolume = csv[4] new BigDecimal(  )
                };
            }
            catch (Exception) {
                return null;
            }
        }

        /**
         * Return a new instance clone of this object, used in fill forward
        */
        @returns A clone of the current object
        public @Override BaseData Clone() {
            return new CoarseFundamental
            {
                Symbol = Symbol,
                Time = Time,
                DollarVolume = DollarVolume,
                Market = Market,
                Value = Value,
                Volume = Volume,
                DataType = MarketDataType.Auxiliary
            };
        }

        /**
         * Creates the symbol used for coarse fundamental data
        */
         * @param market The market
        @returns A coarse universe symbol for the specified market
        public static Symbol CreateUniverseSymbol( String market) {
            market = market.toLowerCase();
            ticker = "qc-universe-coarse-" + market;
            sid = SecurityIdentifier.GenerateEquity(SecurityIdentifier.DefaultDate, ticker, market);
            return new Symbol(sid, ticker);
        }
    }
}
