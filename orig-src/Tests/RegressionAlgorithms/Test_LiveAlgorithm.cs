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
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;

using System.Diagnostics;
using Newtonsoft.Json;
using QuantConnect.Data;
using QuantConnect.Data.Market;

package com.quantconnect.lean
{
    // Name your algorithm class anything, as long as it inherits QCAlgorithm
    public class TestLiveAlgorithm : QCAlgorithm
    {
        //Initialize the data and resolution you require for your strategy:
        public @Override void Initialize() {
            res = Resolution.Second;

            AddSecurity(SecurityType.Equity, "AAPL", res, fillDataForward: false, extendedMarketHours: false);
            AddSecurity(SecurityType.Equity, "YHOO", res, fillDataForward: false, extendedMarketHours: false);
            AddSecurity(SecurityType.Equity, "BQR", res, fillDataForward: true, extendedMarketHours: false);
            //AddSecurity(SecurityType.Equity, "GOOG", res, fillDataForward: false, extendedMarketHours: false);
            //AddSecurity(SecurityType.Equity, "TSLA", res, fillDataForward: false, extendedMarketHours: false);
            //AddData<Bitcoin>( "BTC", res);
        }

        //Data Event Handler: New data arrives here. Upload Data "TradeBars" type is a dictionary of strings so you can access it by symbol.
        public void OnData(TradeBars data) {
            String display = "";
            foreach (bar in data.Values) {
                display += ">> " + bar.Symbol + ": " + bar.Value.toString( "C");
            }
            Debug( "ALGO>> OnData(TradeBar) >> " + Time.toString() + " >> " + data.Count + " >> " + display);
        }

        //Bitcoin Handler:
        public void OnData(Bitcoin data) {
            Debug(Time.ToLongTimeString() + " >> ALGO >> OnData(BTC) >> BTC: " + data.Close);
        }

        /// <summary>
        /// Send the end of day event:
        /// </summary>
        public @Override void OnEndOfDay( String symbol) {
            Debug( "ALGO>> OnEndOfDay() >> " + symbol);
        }
    }

    /// <summary>
    /// Custom Data Type: Bitcoin data from Quandl - http://www.quandl.com/help/api-for-bitcoin-data
    /// </summary>
    public class Bitcoin : BaseData
    {
        //Set the defaults:
        public BigDecimal Open = 0;
        public BigDecimal High = 0;
        public BigDecimal Low = 0;
        public BigDecimal Close = 0;
        public BigDecimal VolumeBTC = 0;
        public BigDecimal WeightedPrice = 0;

        /// <summary>
        /// Default Constructor Required.
        /// </summary>
        public Bitcoin() {
            this.Symbol = "BTC";
        }

        /// <summary>
        /// Source URL's of Backtesting and Live Streams:
        /// </summary>
        public @Override String GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed) {
            source = "";

            switch (datafeed) {
                //Historical backtesting data:
                case DataFeedEndpoint.Backtesting:
                    source = "http://www.quandl.com/api/v1/datasets/BITCOIN/BITSTAMPUSD.csv?sort_order=asc";
                    break;

                //Live socket for bitcoin prices:
                case DataFeedEndpoint.Tradier:
                case DataFeedEndpoint.LiveTrading:
                    //Live refreshing endpoint.
                    source = "https://www.bitstamp.net/api/ticker/";
                    break;
            }

            return source;
        }

        /// <summary>
        /// Clone the bitcoin object, required for live data.
        /// </summary>
        /// <returns></returns>
        public @Override BaseData Clone() {
            Bitcoin coin = new Bitcoin();
            coin.Close = this.Close;
            coin.High = this.High;
            coin.Low = this.Low;
            coin.Open = this.Open;
            coin.Symbol = this.Symbol;
            coin.Value = this.Close;
            coin.Time = this.Time;
            coin.VolumeBTC = this.VolumeBTC;
            coin.WeightedPrice = this.WeightedPrice;
            return coin;
        }

        /// <summary>
        /// Backtesting & Live Bitcoin Decoder:
        /// </summary>
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, DataFeedEndpoint datafeed) {
            Bitcoin coin = new Bitcoin();
            switch (datafeed) {
                //Example Line Format:
                //Date      Open   High    Low     Close   Volume (BTC)    Volume (Currency)   Weighted Price
                //2011-09-13 5.8    6.0     5.65    5.97    58.37138238,    346.0973893944      5.929230648356
                case DataFeedEndpoint.Backtesting:
                    try
                    {
                        string[] data = line.split(',');
                        coin.Time = DateTime.Parse(data[0], CultureInfo.InvariantCulture);
                        coin.Open = new BigDecimal( data[1], CultureInfo.InvariantCulture);
                        coin.High = new BigDecimal( data[2], CultureInfo.InvariantCulture);
                        coin.Low = new BigDecimal( data[3], CultureInfo.InvariantCulture);
                        coin.Close = new BigDecimal( data[4], CultureInfo.InvariantCulture);
                        coin.VolumeBTC = new BigDecimal( data[5]);
                        coin.WeightedPrice = new BigDecimal( data[7]);
                        coin.Symbol = "BTC";
                        coin.Value = coin.Close;
                    }
                    catch { /* Do nothing, skip first title row */ }
                    break;

                //Example Line Format:
                //{"high": "441.00", "last": "421.86", "timestamp": "1411606877", "bid": "421.96", "vwap": "428.58", "volume": "14120.40683975", "low": "418.83", "ask": "421.99"}
                case DataFeedEndpoint.Tradier:
                case DataFeedEndpoint.LiveTrading:
                    try
                    {
                        liveBTC = JsonConvert.DeserializeObject<LiveBitcoin>(line);
                        coin.Time = DateTime.Now;
                        coin.Open = liveBTC.Last;
                        coin.High = liveBTC.High;
                        coin.Low = liveBTC.Low;
                        coin.Close = liveBTC.Last;
                        coin.VolumeBTC = liveBTC.Volume;
                        coin.WeightedPrice = liveBTC.VWAP;
                        coin.Symbol = "BTC";
                        coin.Value = coin.Close;
                    }
                    catch { /* Do nothing, possible error in json decoding */ }
                    break;
            }
            
            return coin;
        }
    }

    public class LiveBitcoin
    {
        public int Timestamp = 0;
        public BigDecimal Last = 0;
        public BigDecimal High = 0;
        public BigDecimal Low = 0;
        public BigDecimal Bid = 0;
        public BigDecimal Ask = 0;
        public BigDecimal VWAP = 0;
        public BigDecimal Volume = 0;
    }
}