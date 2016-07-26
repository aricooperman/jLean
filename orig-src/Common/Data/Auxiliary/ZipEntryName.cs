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
using QuantConnect.Util;

package com.quantconnect.lean.Data.Auxiliary
{
    /// <summary>
    /// Defines a data type that just produces data points from the zip entry names in a zip file
    /// </summary>
    public class ZipEntryName : BaseData
    {
        /// <summary>
        /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
        /// each time it is called. The returned object is assumed to be time stamped in the config.ExchangeTimeZone.
        /// </summary>
        /// <param name="config">Subscription data config setup object</param>
        /// <param name="line">Line of the source document</param>
        /// <param name="date">Date of the requested data</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>Instance of the T:BaseData object generated by this line of the CSV</returns>
        public override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode)
        {
            symbol = LeanData.ReadSymbolFromZipEntry(config.SecurityType, config.Resolution, line);
            return new ZipEntryName {Time = date, Symbol = symbol};
        }

        /// <summary>
        /// Return the URL String source of the file. This will be converted to a stream 
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode)
        {
            if (isLiveMode)
            {
                return new SubscriptionDataSource( String.Empty, SubscriptionTransportMedium.LocalFile);
            }

            source = LeanData.GenerateZipFilePath(Globals.DataFolder, config.Symbol, date, config.Resolution, config.TickType);
            return new SubscriptionDataSource(source, SubscriptionTransportMedium.LocalFile, FileFormat.ZipEntryName);
        }
    }
}
