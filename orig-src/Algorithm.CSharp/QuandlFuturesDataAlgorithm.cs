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
using QuantConnect.Algorithm;
using QuantConnect.Data.Custom;

package com.quantconnect.lean
{
    /**
     * QuantConnect University: Futures Example
     * 
     * QuantConnect allows importing generic data sources! This example demonstrates importing a futures
     * data from the popular open data source Quandl.
     * 
     * QuantConnect has a special deal with Quandl giving you access to Stevens Continuous Futurs (SCF) for free.
     * If you'd like to download SCF for local backtesting, you can download it through Quandl.com.
    */
    public class QCUQuandlFutures : QCAlgorithm
    {
        String _crude = "SCF/CME_CL1_ON";

        /**
         * Initialize the data and resolution you require for your strategy
        */
        public @Override void Initialize() {
            SetStartDate(2000, 1, 1);
            SetEndDate(DateTime.Now.Date.AddDays(-1));
            SetCash(25000);
            AddData<QuandlFuture>(_crude, Resolution.Daily);
        }

        /**
         * Data Event Handler: New data arrives here. "TradeBars" type is a dictionary of strings so you can access it by symbol.
        */
         * @param data Data.
        public void OnData(Quandl data) {
            if( !Portfolio.HoldStock) {
                SetHoldings(_crude, 1);
                Debug(Time.toString( "u") + " Purchased Crude Oil: " + _crude);
            }
        }
    }

    /**
     * Custom quandl data type for setting customized value column name. Value column is used for the primary trading calculations and charting.
    */
    public class QuandlFuture : Quandl
    {
        /**
         * Initializes a new instance of the <see cref="QuantConnect.QuandlFuture"/> class.
        */
        public QuandlFuture()
            : base(valueColumnName: "Settle") {
        }
    }
}