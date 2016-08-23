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
using QuantConnect.Data;

package com.quantconnect.lean.Algorithm.Examples
{
    /**
    /// 3.0 CUSTOM DATA SOURCE: USE YOUR OWN MARKET DATA (OPTIONS, FOREX, FUTURES, DERIVATIVES etc).
    /// 
    /// The new QuantConnect Lean Backtesting Engine is incredibly flexible and allows you to define your own data source. 
    /// 
    /// This includes any data source which has a TIME and VALUE. These are the *only* requirements. To demonstrate this we're loading
    /// in "Nifty" data. This by itself isn't special, the cool part is next:
    /// 
    /// We load the "Nifty" data as a tradable security we're calling "NIFTY".
    /// 
    */
    public class CustomDataNIFTYAlgorithm : QCAlgorithm
    {
        //Create variables for analyzing Nifty
        CorrelationPair today = new CorrelationPair();
        List<CorrelationPair> prices = new List<CorrelationPair>();
        int minimumCorrelationHistory = 50;

        /**
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        */
        public @Override void Initialize() {
            SetStartDate(2008, 1, 8);
            SetEndDate(2014, 7, 25);

            //Set the cash for the strategy:
            SetCash(100000);

            //Define the symbol and "type" of our generic data:
            AddData<DollarRupee>( "USDINR");
            AddData<Nifty>( "NIFTY");
        }

        /**
        /// Event Handler for Nifty Data Events: These Nifty objects are created from our 
        /// "Nifty" type below and fired into this event handler.
        */
         * @param data">One(1) Nifty Object, streamed into our algorithm synchronised in time with our other data streams
        public void OnData(DollarRupee data) {
            today = new CorrelationPair(data.Time);
            today.CurrencyPrice = Convert.ToDouble(data.Close);
        }

        /**
        /// OnData is the primary entry point for youm algorithm. New data is piped into your algorithm here
        /// via TradeBars objects.
        */
         * @param data">TradeBars IDictionary object
        public void OnData(Nifty data) {
            try
            {
                int quantity = (int)(Portfolio.TotalPortfolioValue * 0.9m / data.Close);

                today.NiftyPrice = Convert.ToDouble(data.Close);
                if( today.Date == data.Time) {
                    prices.Add(today);

                    if( prices.Count > minimumCorrelationHistory) {
                        prices.RemoveAt(0);
                    }
                }

                //Strategy
                double highestNifty = (from pair in prices select pair.NiftyPrice).Max();
                double lowestNifty = (from pair in prices select pair.NiftyPrice).Min();
                if( Time.DayOfWeek == DayOfWeek.Wednesday) //prices.Count >= minimumCorrelationHistory && 
                {
                    //List<double> niftyPrices = (from pair in prices select pair.NiftyPrice).ToList();
                    //List<double> currencyPrices = (from pair in prices select pair.CurrencyPrice).ToList();
                    //double correlation = Correlation.Pearson(niftyPrices, currencyPrices);
                    //double niftyFraction = (correlation)/2;

                    if( Convert.ToDouble(data.Open) >= highestNifty) {
                        int code = Order( "NIFTY", quantity - Portfolio["NIFTY"].Quantity);
                        Debug( "LONG " + code + " Time: " + Time.ToShortDateString() + " Quantity: " + quantity + " Portfolio:" + Portfolio["NIFTY"].Quantity + " Nifty: " + data.Close + " Buying Power: " + Portfolio.TotalPortfolioValue);
                    }
                    else if( Convert.ToDouble(data.Open) <= lowestNifty) {
                        int code = Order( "NIFTY", -quantity - Portfolio["NIFTY"].Quantity);
                        Debug( "SHORT " + code + " Time: " + Time.ToShortDateString() + " Quantity: " + quantity + " Portfolio:" + Portfolio["NIFTY"].Quantity + " Nifty: " + data.Close + " Buying Power: " + Portfolio.TotalPortfolioValue);
                    }
                }
            }
            catch (Exception err) {
                Debug( "Error: " + err.Message);
            }
        }

        /**
        /// End of a trading day event handler. This method is called at the end of the algorithm day (or multiple times if trading multiple assets).
        */
        /// Method is called 10 minutes before closing to allow user to close out position.
        public @Override void OnEndOfDay() {
            //if(niftyData != null ) {
                Plot( "Nifty Closing Price", today.NiftyPrice);
            }
        }
    }

    /**
    /// NIFTY Custom Data Class
    */
    public class Nifty : BaseData
    {
        /**
        /// Opening Price
        */
        public BigDecimal Open = 0;
        /**
        /// High Price
        */
        public BigDecimal High = 0;
        /**
        /// Low Price
        */
        public BigDecimal Low = 0;
        /**
        /// Closing Price
        */
        public BigDecimal Close = 0;

        /**
        /// Default initializer for NIFTY.
        */
        public Nifty() {
            Symbol = "NIFTY";
        }

        /**
        /// Return the URL String source of the file. This will be converted to a stream 
        */
        public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            return new SubscriptionDataSource( "https://www.dropbox.com/s/rsmg44jr6wexn2h/CNXNIFTY.csv?dl=1", SubscriptionTransportMedium.RemoteFile);
        }

        /**
        /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
        /// each time it is called. 
        */
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
            //New Nifty object
            Nifty index = new Nifty();

            try
            {
                //Example File Format:
                //Date,       Open       High        Low       Close     Volume      Turnover
                //2011-09-13  7792.9    7799.9     7722.65    7748.7    116534670    6107.78
                string[] data = line.split(',');
                index.Time = DateTime.Parse(data[0], CultureInfo.InvariantCulture);
                index.Open = new BigDecimal( data[1], CultureInfo.InvariantCulture);
                index.High = new BigDecimal( data[2], CultureInfo.InvariantCulture);
                index.Low = new BigDecimal( data[3], CultureInfo.InvariantCulture);
                index.Close = new BigDecimal( data[4], CultureInfo.InvariantCulture);
                index.Symbol = "NIFTY";
                index.Value = index.Close;
            }
            catch
            {

            }

            return index;
        }
    }


    /**
    /// Dollar Rupe is a custom data type we create for this algorithm
    */
    public class DollarRupee : BaseData
    {
        /**
        /// Open Price 
        */
        public BigDecimal Open = 0;
        /**
        /// High Price
        */
        public BigDecimal High = 0;
        /**
        /// Low Price
        */
        public BigDecimal Low = 0;
        /**
        /// Closing Price
        */
        public BigDecimal Close = 0;

        /**
        /// Default constructor for the custom data class.
        */
        public DollarRupee() {
            Symbol = "USDINR";
        }

        /**
        /// Return the URL String source of the file. This will be converted to a stream 
        */
        public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            return new SubscriptionDataSource( "https://www.dropbox.com/s/m6ecmkg9aijwzy2/USDINR.csv?dl=1", SubscriptionTransportMedium.RemoteFile);
        }

        /**
        /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
        /// each time it is called. 
        */
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
            //New USDINR object
            DollarRupee currency = new DollarRupee();

            try
            {
                string[] data = line.split(',');
                currency.Time = DateTime.Parse(data[0], CultureInfo.InvariantCulture);
                currency.Close = new BigDecimal( data[1], CultureInfo.InvariantCulture);
                currency.Symbol = "USDINR";
                currency.Value = currency.Close;
            }
            catch
            {

            }

            return currency;
        }
    }

    /**
    /// Correlation Pair is a helper class to combine two data points which we'll use to perform the correlation. 
    */
    public class CorrelationPair
    {
        /**
        /// Date of the correlation pair
        */
        public DateTime Date = new DateTime();

        /**
        /// Nifty price for this correlation pair
        */
        public double NiftyPrice = 0;

        /**
        /// Currency price for this correlation pair
        */
        public double CurrencyPrice = 0;

        /**
        /// Default initializer
        */
        public CorrelationPair() { }

        /**
        /// Date based correlation pair initializer
        */
        public CorrelationPair(DateTime date) {
            Date = date.Date;
        }
    }
}