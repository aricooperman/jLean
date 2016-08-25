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
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
     * In this algortihm we show how you can easily use the universe selection feature to fetch symbols
     * to be traded using the BaseData custom data system in combination with the AddUniverse{T} method.
     * AddUniverse{T} requires a function that will return the symbols to be traded.
    */
    public class DropboxBaseDataUniverseSelectionAlgorithm : QCAlgorithm
    {
        // the changes from the previous universe selection
        private SecurityChanges _changes = SecurityChanges.None;

        /**
         * Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        */
         * <seealso cref="QCAlgorithm.SetStartDate(System.DateTime)"/>
         * <seealso cref="QCAlgorithm.SetEndDate(System.DateTime)"/>
         * <seealso cref="QCAlgorithm.SetCash(decimal)"/>
        public @Override void Initialize() {
            UniverseSettings.Resolution = Resolution.Daily;

            SetStartDate(2013, 01, 01);
            SetEndDate(2013, 12, 31);

            AddUniverse<StockDataSource>( "my-stock-data-source", stockDataSource =>
            {
                return stockDataSource.SelectMany(x -> x.Symbols);
            });
        }

        /**
         * Event - v3.0 DATA EVENT HANDLER: (Pattern) Basic template for user to @Override for receiving all subscription data in a single event
        */
         * <code>
         * TradeBars bars = slice.Bars;
         * Ticks ticks = slice.Ticks;
         * TradeBar spy = slice["SPY"];
         * List{Tick} aaplTicks = slice["AAPL"]
         * Quandl oil = slice["OIL"]
         * dynamic anySymbol = slice[symbol];
         * DataDictionary{Quandl} allQuandlData = slice.Get{Quand}
         * Quandl oil = slice.Get{Quandl}( "OIL")
         * </code>
         * @param slice The current slice of data keyed by symbol string
        public @Override void OnData(Slice slice) {
            if( slice.Bars.Count == 0) return;
            if( _changes == SecurityChanges.None) return;

            // start fresh
            
            Liquidate();

            percentage = 1m / slice.Bars.Count;
            foreach (tradeBar in slice.Bars.Values) {
                SetHoldings(tradeBar.Symbol, percentage);
            }

            // reset changes
            _changes = SecurityChanges.None;
        }

        /**
         * Event fired each time the we add/remove securities from the data feed
        */
         * @param changes">
        public @Override void OnSecuritiesChanged(SecurityChanges changes) {
            // each time our securities change we'll be notified here
            _changes = changes;
        }

        /**
         * Our custom data type that defines where to get and how to read our backtest and live data.
        */
        class StockDataSource : BaseData
        {
            private static final String LiveUrl = @"https://www.dropbox.com/s/2az14r5xbx4w5j6/daily-stock-picker-live.csv?dl=1";
            private static final String BacktestUrl = @"https://www.dropbox.com/s/rmiiktz0ntpff3a/daily-stock-picker-backtest.csv?dl=1";

            /**
             * The symbols to be selected
            */
            public List<String> Symbols { get; set; }

            /**
             * Required default constructor
            */
            public StockDataSource() {
                // initialize our list to empty
                Symbols = new List<String>();
            }

            /**
             * Return the URL String source of the file. This will be converted to a stream 
            */
             * @param config Configuration object
             * @param date Date of this source file
             * @param isLiveMode true if we're in live mode, false for backtesting mode
            @returns String URL of source file.
            public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
                url = isLiveMode ? LiveUrl : BacktestUrl;
                return new SubscriptionDataSource(url, SubscriptionTransportMedium.RemoteFile);
            }

            /**
             * Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
             * each time it is called. The returned object is assumed to be time stamped in the config.ExchangeTimeZone.
            */
             * @param config Subscription data config setup object
             * @param line Line of the source document
             * @param date Date of the requested data
             * @param isLiveMode true if we're in live mode, false for backtesting mode
            @returns Instance of the T:BaseData object generated by this line of the CSV
            public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
                try
                {
                    // create a new StockDataSource and set the symbol using config.Symbol
                    stocks = new StockDataSource {Symbol = config.Symbol};
                    // break our line into csv pieces
                    csv = line Extensions.toCsv( );
                    if( isLiveMode) {
                        // our live mode format does not have a date in the first column, so use date parameter
                        stocks.Time = date;
                        stocks.Symbols.AddRange(csv);
                    }
                    else
                    {
                        // our backtest mode format has the first column as date, parse it
                        stocks.Time = DateTime.ParseExact(csv[0], "yyyyMMdd", null );
                        // any following comma separated values are symbols, save them off
                        stocks.Symbols.AddRange(csv.Skip(1));
                    }
                    return stocks;
                }
                // return null if we encounter any errors
                catch { return null; }
            }
        }
    }
}