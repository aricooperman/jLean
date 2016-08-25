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
using System.IO;
using Ionic.Zip;
using QuantConnect.Data;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Provides an implementation of <see cref="ISubscriptionDataSourceReader"/> that reads zip entry names
    */
    public class ZipEntryNameSubscriptionDataSourceReader : ISubscriptionDataSourceReader
    {
        private final SubscriptionDataConfig _config;
        private final DateTime _dateTime;
        private final boolean _isLiveMode;
        private final BaseData _factory;

        /**
         * Event fired when the specified source is considered invalid, this may
         * be from a missing file or failure to download a remote source
        */
        public event EventHandler<InvalidSourceEventArgs> InvalidSource;

        /**
         * Initializes a new instance of the <see cref="ZipEntryNameSubscriptionDataSourceReader"/> class
        */
         * @param config The subscription's configuration
         * @param dateTime The date this factory was produced to read data for
         * @param isLiveMode True if we're in live mode, false for backtesting
        public ZipEntryNameSubscriptionDataSourceReader(SubscriptionDataConfig config, DateTime dateTime, boolean isLiveMode) {
            _config = config;
            _dateTime = dateTime;
            _isLiveMode = isLiveMode;
            _factory = (BaseData) Activator.CreateInstance(config.Type);
        }

        /**
         * Reads the specified <paramref name="source"/>
        */
         * @param source The source to be read
        @returns An <see cref="IEnumerable{BaseData}"/> that contains the data in the source
        public IEnumerable<BaseData> Read(SubscriptionDataSource source) {
            if( !File.Exists(source.Source)) {
                OnInvalidSource(source, new FileNotFoundException( "The specified file was not found", source.Source));
            }

            ZipFile zip;
            try
            {
                zip = new ZipFile(source.Source);
            }
            catch (ZipException err) {
                OnInvalidSource(source, err);
                yield break;
            }

            foreach (entryFileName in zip.EntryFileNames) {
                yield return _factory.Reader(_config, entryFileName, _dateTime, _isLiveMode);
            }
        }

        /**
         * Event invocator for the <see cref="InvalidSource"/> event
        */
         * @param source The <see cref="SubscriptionDataSource"/> that was invalid
         * @param exception The exception if one was raised, otherwise null
        private void OnInvalidSource(SubscriptionDataSource source, Exception exception) {
            handler = InvalidSource;
            if( handler != null ) handler(this, new InvalidSourceEventArgs(source, exception));
        }
    }
}
