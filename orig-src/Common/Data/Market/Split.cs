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

package com.quantconnect.lean.Data.Market
{
    /// <summary>
    /// Split event from a security
    /// </summary>
    public class Split : BaseData
    {
        /// <summary>
        /// Initializes a new instance of the Split class
        /// </summary>
        public Split() {
            DataType = MarketDataType.Auxiliary;
        }

        /// <summary>
        /// Initializes a new instance of the Split class
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="date">The date</param>
        /// <param name="price">The price at the time of the split</param>
        /// <param name="splitFactor">The split factor to be applied to current holdings</param>
        public Split(Symbol symbol, DateTime date, BigDecimal price, BigDecimal splitFactor)
             : this() {
            Symbol = symbol;
            Time = date;
            ReferencePrice = price;
            SplitFactor = splitFactor;
        }

        /// <summary>
        /// Gets the split factor
        /// </summary>
        public BigDecimal SplitFactor
        {
            get; private set;
        }

        /// <summary>
        /// Gets the price at which the split occurred
        /// </summary>
        public BigDecimal ReferencePrice
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <summary>
        /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
        /// each time it is called. 
        /// </summary>
        /// <param name="config">Subscription data config setup object</param>
        /// <param name="line">Line of the source document</param>
        /// <param name="date">Date of the requested data</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>Instance of the T:BaseData object generated by this line of the CSV</returns>
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
            // this is implemented in the SubscriptionDataReader.CheckForSplit
            throw new NotImplementedException( "This method is not supposed to be called on the Split type.");
        }

        /// <summary>
        /// Return the URL String source of the file. This will be converted to a stream 
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            // this data is derived from map files and factor files in backtesting
            throw new NotImplementedException( "This method is not supposed to be called on the Split type.");
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="QuantConnect.Data.Market.split"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="QuantConnect.Data.Market.split"/>.</returns>
        public @Override String toString() {
            return String.format( "%1$s: %2$s", Symbol, SplitFactor);
        }

        /// <summary>
        /// Return a new instance clone of this object, used in fill forward
        /// </summary>
        /// <remarks>
        /// This base implementation uses reflection to copy all public fields and properties
        /// </remarks>
        /// <returns>A clone of the current object</returns>
        public @Override BaseData Clone() {
            return new Split(Symbol, Time, Price, SplitFactor);
        }
    }
}
