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
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;

using System.Globalization;
using QuantConnect.Data;
using QuantConnect.Data.Market;

package com.quantconnect.lean
{
    // Name your algorithm class anything, as long as it inherits QCAlgorithm
    public class TestMixedAssets : QCAlgorithm
    {
        private BigDecimal _vix = 0;
        private BigDecimal _deployedCapital = 1;
        private BigDecimal _safeCapital = 0;
        private DateTime _lastRebalance = new DateTime();

        //Initialize the data and resolution you require for your strategy:
        public @Override void Initialize() {
            SetStartDate(2013, 7, 1);
            SetEndDate(2014, 10, 31);
            SetCash(250000);
            AddSecurity(SecurityType.Equity, "SPY", Resolution.Minute, fillDataForward: false, leverage: 1, extendedMarketHours: false);
            AddSecurity(SecurityType.Equity, "IBM", Resolution.Minute, fillDataForward: false, leverage: 1, extendedMarketHours: false);
            AddData<VIX>( "VIX", Resolution.Minute);
        }

        // Data Event Handler: New data arrives here. "TradeBars" type is a dictionary of strings so you can access it by symbol.
        public void OnData(TradeBars data) {
            if( _vix == 0) return;

            if( Time.Date > _lastRebalance.Date.AddDays(5)) {
                //Rebalance every 5 days:
                _lastRebalance = Time;

                //Scale VIX fractionally 0-1 for 8-30.
                _deployedCapital = 1 - ((_vix - 8m) / 22m);

                //Don't allow negative scaling:
                if( _deployedCapital < -0.20m) _deployedCapital = -0.20m;

                //Fraction of capital preserved for bonds:
                _safeCapital = 1 - _deployedCapital;

                tag = "Deployed: " + _deployedCapital.toString( "0.00") + " Safe: " + _safeCapital.toString( "0.00");

                SetHoldings( "SPY", _deployedCapital, true, tag);
                SetHoldings( "IBM", _safeCapital - 0.01m, false, tag);
            }
        }

        // 
        public void OnData(VIX vix) {
            _vix = vix.Close;
        }
    }


    /**
     * Custom imported data -- VIX indicator:
    */
    public class VIX : BaseData
    {
        public BigDecimal Open = 0;
        public BigDecimal High = 0;
        public BigDecimal Low = 0;
        public BigDecimal Close = 0;

        public VIX() { this.Symbol = "VIX"; }

        public @Override String GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed) {
            return "https://www.quandl.com/api/v1/datasets/YAHOO/INDEX_VIX.csv?trim_start=2000-01-01&trim_end=2014-10-31&sort_order=asc&exclude_headers=true";
        }
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, DataFeedEndpoint datafeed) {
            VIX fear = new VIX();
            //try
            //{
            //Date          Open     High     Low   Close    Volume    Adjusted Close
            //10/27/2014    17.24    17.87    16    16.04    0         16.04
            string[] data = line.split(',');
            fear.Time = DateTime.ParseExact(data[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            fear.Open = new BigDecimal( data[1], CultureInfo.InvariantCulture); 
            fear.High = new BigDecimal( data[2], CultureInfo.InvariantCulture);
            fear.Low = new BigDecimal( data[3], CultureInfo.InvariantCulture); 
            fear.Close = new BigDecimal( data[4], CultureInfo.InvariantCulture);
            fear.Symbol = "VIX"; fear.Value = fear.Close;
            //}
            //catch 
            //{ }
            return fear;
        }
        public @Override BaseData Clone() {
            VIX fear = new VIX();
            fear.Open = Open; fear.High = High; fear.Low = Low; fear.Close = Close;
            return fear;
        }
    }
}