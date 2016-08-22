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

package com.quantconnect.lean.Data.Custom
{
    /// <summary>
    /// Quandl Data Type - Import generic data from quandl, without needing to define Reader methods. 
    /// This reads the headers of the data imported, and dynamically creates properties for the imported data.
    /// </summary>
    public class Quandl : DynamicData
    {
        private boolean _isInitialized;
        private readonly List<String> _propertyNames = new List<String>();
        private readonly String _valueColumn;
        private static String _authCode = "";

        /// <summary>
        /// Flag indicating whether or not the Quanl auth code has been set yet
        /// </summary>
        public static boolean IsAuthCodeSet
        {
            get;
            private set;
        }

        /// <summary>
        /// The end time of this data. Some data covers spans (trade bars) and as such we want
        /// to know the entire time span covered
        /// </summary>
        public @Override DateTime EndTime
        {
            get { return Time + Period; }
            set { Time = value - Period; }
        }

        /// <summary>
        /// Gets a time span of one day
        /// </summary>
        public TimeSpan Period
        {
            get { return QuantConnect.Time.OneDay; }
        }

        /// <summary>
        /// Default quandl constructor uses Close as its value column
        /// </summary>
        public Quandl() : this( "Close") {
        }

        /// <summary>
        /// Constructor for creating customized quandl instance which doesn't use "Close" as its value item.
        /// </summary>
        /// <param name="valueColumnName"></param>
        protected Quandl( String valueColumnName) {
            _valueColumn = valueColumnName;
        }

        /// <summary>
        /// Generic Reader Implementation for Quandl Data.
        /// </summary>
        /// <param name="config">Subscription configuration</param>
        /// <param name="line">CSV line of data from the souce</param>
        /// <param name="date">Date of the requested line</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns></returns>
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
            // be sure to instantiate the correct type
            data = (Quandl) Activator.CreateInstance(GetType());
            data.Symbol = config.Symbol;
            csv = line.split(',');

            if( !_isInitialized) {
                _isInitialized = true;
                foreach (propertyName in csv) {
                    property = propertyName.TrimStart().TrimEnd();
                    // should we remove property names like Time?
                    // do we need to alias the Time??
                    data.SetProperty(property, 0m);
                    _propertyNames.Add(property);
                }
                // Returns null at this point where we are only reading the properties names
                return null;
            }

            data.Time = DateTime.ParseExact(csv[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);

            for (i = 1; i < csv.Length; i++) {
                value = csv[i] new BigDecimal(  );
                data.SetProperty(_propertyNames[i], value);
            }

            // we know that there is a close property, we want to set that to 'Value'
            data.Value = (decimal)data.GetProperty(_valueColumn);

            return data;
        }

        /// <summary>
        /// Quandl Source Locator: Using the Quandl V1 API automatically set the URL for the dataset.
        /// </summary>
        /// <param name="config">Subscription configuration object</param>
        /// <param name="date">Date of the data file we're looking for</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>STRING API Url for Quandl.</returns>
        public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            source = @"https://www.quandl.com/api/v3/datasets/" + config.Symbol.Value + ".csv?order=asc&api_key=" + _authCode;
            return new SubscriptionDataSource(source, SubscriptionTransportMedium.RemoteFile);
        }

        /// <summary>
        /// Set the auth code for the quandl set to the QuantConnect auth code.
        /// </summary>
        /// <param name="authCode"></param>
        public static void SetAuthCode( String authCode) {
            if(  String.IsNullOrWhiteSpace(authCode)) return;

            _authCode = authCode;
            IsAuthCodeSet = true;
        }
    }
}
