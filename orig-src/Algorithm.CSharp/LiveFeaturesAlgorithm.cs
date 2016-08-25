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
using Newtonsoft.Json;
using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Data.Market;

package com.quantconnect.lean
{
    /**
     * QuantConnect University: Live Trading Functionality Demonstration:
     * This algorithm demonstrates the underlying functionality specifically for live trading.
    */
    public class LiveTradingFeaturesAlgorithm : QCAlgorithm
    {
        /**
         * Initialise the Algorithm and Prepare Required Data.
        */
        public @Override void Initialize() {
            SetStartDate(2013, 1, 1);
            SetEndDate(DateTime.Now.Date.AddDays(-1));
            SetCash(25000);

            //Equity Data for US Markets:
            AddSecurity(SecurityType.Equity, "AAPL", Resolution.Second);

            //FOREX Data for Weekends: 24/6
            AddSecurity(SecurityType.Forex, "EURUSD", Resolution.Minute);

            //Custom/Bitcoin Live Data: 24/7
            AddData<Bitcoin>( "BTC", Resolution.Second);
        }

        /**
         * New Bitcoin Data Event.
        */
         * @param data Data.
        public void OnData(Bitcoin data) {
            if( LiveMode) //Live Mode Property
            {
                //Configurable title header statistics numbers
                SetRuntimeStatistic( "BTC", data.Close.toString( "C"));
            }

            if( !Portfolio.HoldStock) {
                Order( "BTC", 100);

                //Send a notification email/SMS/web request on events:
                Notify.Email( "myemail@gmail.com", "Test", "Test Body", "test attachment");
                Notify.Sms( "+11233456789", Time.toString( "u") + ">> Test message from live BTC server.");
                Notify.Web( "http://api.quantconnect.com", Time.toString( "u") + ">> Test data packet posted from live BTC server.");
            }
        }

        /**
         * Raises the data event.
        */
         * @param data Data.
        public void OnData(TradeBars data) {
            if( !Portfolio.HoldStock && data.ContainsKey( "AAPL")) {
                int quantity = (int)Math.Floor(Portfolio.Cash / data["AAPL"].Close);
                Order( "AAPL", quantity);
                Debug( "Purchased SPY on " + Time.ToShortDateString());
                Notify.Email( "myemail@gmail.com", "Test", "Test Body", "test attachment");
            }
        }
    }

    /**
     * Custom Data Type: Bitcoin data from Quandl - http://www.quandl.com/help/api-for-bitcoin-data
    */
    public class Bitcoin : BaseData
    {
        //Set the defaults:
        /**
         * Open Price
        */
        public BigDecimal Open = 0;
        
        /**
         * High Price
        */
        public BigDecimal High = 0;
        
        /**
         * Low Price
        */
        public BigDecimal Low = 0;

        /**
         * Closing Price
        */
        public BigDecimal Close = 0;

        /**
         * Volume in BTC
        */
        public BigDecimal VolumeBTC = 0;

        /**
         * Volume in USD
        */
        public BigDecimal VolumeUSD = 0;
        
        /**
         * Volume in USD:
        */
        public BigDecimal WeightedPrice = 0;

        /**
         * 1. DEFAULT CONSTRUCTOR: Custom data types need a default constructor.
         * We search for a default constructor so please provide one here. It won't be used for data, just to generate the "Factory".
        */
        public Bitcoin() {
            Symbol = "BTC";
        }

        /**
         * 2. RETURN THE STRING URL SOURCE LOCATION FOR YOUR DATA:
         * This is a powerful and dynamic select source file method. If you have a large dataset, 10+mb we recommend you break it into smaller files. E.g. One zip per year.
         * We can accept raw text or ZIP files. We read the file extension to determine if it is a zip file.
        */
         * @param config Configuration object
         * @param date Date of this source file
         * @param isLiveMode true if we're in live mode, false for backtesting mode
        @returns String URL of source file.
        public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            if( isLiveMode) {
                return new SubscriptionDataSource( "https://www.bitstamp.net/api/ticker/", SubscriptionTransportMedium.Rest);
            }

            //return "http://my-ftp-server.com/futures-data-" + date.toString( "Ymd") + ".zip";
            // OR simply return a fixed small data file. Large files will slow down your backtest
            return new SubscriptionDataSource( "http://www.quandl.com/api/v1/datasets/BCHARTS/BITSTAMPUSD.csv?sort_order=asc", SubscriptionTransportMedium.RemoteFile);
        }

        /**
         * 3. READER METHOD: Read 1 line from data source and convert it into Object.
         * Each line of the CSV File is presented in here. The backend downloads your file, loads it into memory and then line by line
         * feeds it into your algorithm
        */
         * @param line string line from the data source file submitted above
         * @param config Subscription data, symbol name, data type
         * @param date Current date we're requesting. This allows you to break up the data source into daily files.
         * @param isLiveMode true if we're in live mode, false for backtesting mode
        @returns New Bitcoin Object which extends BaseData.
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
            coin = new Bitcoin();
            if( isLiveMode) {
                //Example Line Format:
                //{"high": "441.00", "last": "421.86", "timestamp": "1411606877", "bid": "421.96", "vwap": "428.58", "volume": "14120.40683975", "low": "418.83", "ask": "421.99"}
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
                return coin;
            }
            
            //Example Line Format:
            //Date      Open   High    Low     Close   Volume (BTC)    Volume (Currency)   Weighted Price
            //2011-09-13 5.8    6.0     5.65    5.97    58.37138238,    346.0973893944      5.929230648356
            try
            {
                string[] data = line.split(',');
                coin.Time = DateTime.Parse(data[0], CultureInfo.InvariantCulture);
                coin.Open = new BigDecimal( data[1], CultureInfo.InvariantCulture);
                coin.High = new BigDecimal( data[2], CultureInfo.InvariantCulture);
                coin.Low = new BigDecimal( data[3], CultureInfo.InvariantCulture);
                coin.Close = new BigDecimal( data[4], CultureInfo.InvariantCulture);
                coin.VolumeBTC = new BigDecimal( data[5], CultureInfo.InvariantCulture);
                coin.VolumeUSD = new BigDecimal( data[6], CultureInfo.InvariantCulture);
                coin.WeightedPrice = new BigDecimal( data[7], CultureInfo.InvariantCulture);
                coin.Symbol = "BTC";
                coin.Value = coin.Close;
            }
            catch { /* Do nothing, skip first title row */ }

            return coin;
        }
    }

    /**
     * Live data structure
    */
    public class LiveBitcoin
    {
        @JsonProperty( "timestamp")]
        public int Timestamp = 0;
        @JsonProperty( "last")]
        public BigDecimal Last = 0;
        @JsonProperty( "high")]
        public BigDecimal High = 0;
        @JsonProperty( "low")]
        public BigDecimal Low = 0;
        @JsonProperty( "bid")]
        public BigDecimal Bid = 0;
        @JsonProperty( "ask")]
        public BigDecimal Ask = 0;
        @JsonProperty( "vwap")]
        public BigDecimal VWAP = 0;
        @JsonProperty( "volume")]
        public BigDecimal Volume = 0;
    }
}