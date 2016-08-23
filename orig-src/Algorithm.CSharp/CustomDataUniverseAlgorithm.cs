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
 *
*/

using System;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
    /// This algorithm shows how to grab symbols from an external api each day
    /// and load data using the universe selection feature. In this example we
    /// define a custom data type for the NYSE top gainers and then short the 
    /// top 2 gainers each day
    */
    public class CustomDataUniverseAlgorithm : QCAlgorithm
    {
        private SecurityChanges _changes;

        public @Override void Initialize() {
            UniverseSettings.Resolution = Resolution.Daily;

            SetStartDate(2015, 01, 05);
            SetEndDate(2015, 07, 01);

            SetCash(100000);

            AddSecurity(SecurityType.Equity, "SPY", Resolution.Daily);
            SetBenchmark( "SPY");

            // add a custom universe data source (defaults to usa-equity)
            AddUniverse<NyseTopGainers>( "universe-nyse-top-gainers", Resolution.Daily, data =>
            {
                // define our selection criteria
                return from d in data
                       // pick top 2 gainers to bet against
                       where d.TopGainersRank <= 2
                       select d.Symbol;
            });
        }

        public @Override void OnData(Slice slice) {
        }

        public @Override void OnSecuritiesChanged(SecurityChanges changes) {
            _changes = changes;

            foreach (security in changes.RemovedSecurities) {
                // liquidate securities that have been removed
                if( security.Invested) {
                    Liquidate(security.Symbol);
                    Log( "Exit " + security.Symbol + " at " + security.Close);
                }
            }

            foreach (security in changes.AddedSecurities) {
                // enter short positions on new securities
                if( !security.Invested && security.Close != 0) {
                    qty = CalculateOrderQuantity(security.Symbol, -0.25m);
                    MarketOnOpenOrder(security.Symbol, qty);
                    Log( "Enter  " + security.Symbol + " at " + security.Close);
                }
            }
        }

        /**
        /// Custom data type that uses the wall street journal's top 100 nyse gainers
        /// html page as a live data source, and a csv file that contains the top 10
        /// nyse gainers since the beginning of 2009 until 2015/10/19
        */
        public class NyseTopGainers : BaseData
        {
            public int TopGainersRank;

            public @Override DateTime EndTime
            {
                // define end time as exactly 1 day after Time
                get { return Time + QuantConnect.Time.OneDay; }
                set { Time = value - QuantConnect.Time.OneDay; }
            }

            private int count;
            private DateTime lastDate;
            public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
                if( isLiveMode) {
                    // this is actually an html file, we'll handle the parsing accordingly
                    return new SubscriptionDataSource(@"http://www.wsj.com/mdc/public/page/2_3021-gainnyse-gainer.html", SubscriptionTransportMedium.RemoteFile);
                }

                // this has data from 2009.01.01 to 2015.10.19 for top 10 nyse gainers
                return new SubscriptionDataSource(@"https://www.dropbox.com/s/vrn3p38qberw3df/nyse-gainers.csv?dl=1", SubscriptionTransportMedium.RemoteFile);
            }

            public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
                if( !isLiveMode) {
                    // backtest gets data from csv file in dropbox
                    csv = line.split(',');
                    return new NyseTopGainers
                    {
                        Time = DateTime.ParseExact(csv[0], "yyyyMMdd", null ),
                        Symbol = Symbol.Create(csv[1], SecurityType.Equity, Market.USA),
                        TopGainersRank = int.Parse(csv[2])
                    };
                }

                if( lastDate != date) {
                    // reset our counter for the new day
                    lastDate = date;
                    count = 0;
                }

                // parse the html into a symbol

                if( !line.StartsWith(@"<a href=""/public/quotes/main.html?symbol=")) {
                    // we're only looking for lines that contain the symbols
                    return null;
                }

                lastCloseParen = line.LastIndexOf( ")", StringComparison.Ordinal);
                lastOpenParen = line.LastIndexOf( "( ", StringComparison.Ordinal);
                if( lastOpenParen == -1 || lastCloseParen == -1) {
                    return null;
                }

                symbolString = line.Substring(lastOpenParen + 1, lastCloseParen - lastOpenParen - 1);
                return new NyseTopGainers
                {
                    Symbol = Symbol.Create(symbolString, SecurityType.Equity, Market.USA),
                    Time = date,
                    // the html has these in order, so we'll keep incrementing until a new day
                    TopGainersRank = ++count
                };
            }
        }
    }
}