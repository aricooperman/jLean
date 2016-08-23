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
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Lean.Engine.DataFeeds.Transport;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
    /// Collection Subscription Factory takes a BaseDataCollection from BaseData factories
    /// and yields it one point at a time to the algorithm
    */
    public class CollectionSubscriptionDataSourceReader : ISubscriptionDataSourceReader
    {
        
        private final DateTime _date;
        private final boolean _isLiveMode;
        private final BaseData _factory;
        private final SubscriptionDataConfig _config;

        /**
        /// Initializes a new instance of the <see cref="CollectionSubscriptionDataSourceReader"/> class
        */
         * @param config">The subscription's configuration
         * @param date">The date this factory was produced to read data for
         * @param isLiveMode">True if we're in live mode, false for backtesting
        public CollectionSubscriptionDataSourceReader(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            _date = date;
            _config = config;
            _isLiveMode = isLiveMode;
            _factory = (BaseData)ObjectActivator.GetActivator(config.Type).Invoke(new object[0]);
        }

        /**
        /// Event fired when the specified source is considered invalid, this may
        /// be from a missing file or failure to download a remote source
        */
        public event EventHandler<InvalidSourceEventArgs> InvalidSource;

        /**
        /// Event fired when an exception is thrown during a call to 
        /// <see cref="BaseData.Reader(SubscriptionDataConfig, string, DateTime, bool)"/>
        */
        public event EventHandler<ReaderErrorEventArgs> ReaderError;

        /**
        /// Reads the specified <paramref name="source"/>
        */
         * @param source">The source to be read
        @returns An <see cref="IEnumerable{BaseData}"/> that contains the data in the source
        public IEnumerable<BaseData> Read(SubscriptionDataSource source) {
            IStreamReader reader = null;
            instances = new BaseDataCollection();
            try
            {
                switch (source.TransportMedium) {
                    default:
                    case SubscriptionTransportMedium.Rest:
                        reader = new RestSubscriptionStreamReader(source.Source);
                        break;
                    case SubscriptionTransportMedium.LocalFile:
                        reader = new LocalFileSubscriptionStreamReader(source.Source);
                        break;
                    case SubscriptionTransportMedium.RemoteFile:
                        reader = new RemoteFileSubscriptionStreamReader(source.Source, Globals.Cache);
                        break;
                }

                raw = "";
                try
                {
                    raw = reader.ReadLine();
                    result = _factory.Reader(_config, raw, _date, _isLiveMode);
                    instances = result as BaseDataCollection;
                    if( instances == null ) {
                        OnInvalidSource(source, new Exception( "Reader must generate a BaseDataCollection with the FileFormat.Collection"));
                    }
                }
                catch (Exception err) {
                    OnReaderError(raw, err);
                }

                foreach (instance in instances.Data) {
                    yield return instance;
                }
            }
            finally
            {
                if( reader != null )
                    reader.Dispose();
            }
        }

        /**
        /// Event invocator for the <see cref="ReaderError"/> event
        */
         * @param line">The line that caused the exception
         * @param exception">The exception that was caught
        private void OnReaderError( String line, Exception exception) {
            handler = ReaderError;
            if( handler != null ) handler(this, new ReaderErrorEventArgs(line, exception));
        }

        /**
        /// Event invocator for the <see cref="InvalidSource"/> event
        */
         * @param source">The <see cref="SubscriptionDataSource"/> that was invalid
         * @param exception">The exception if one was raised, otherwise null
        private void OnInvalidSource(SubscriptionDataSource source, Exception exception) {
            handler = InvalidSource;
            if( handler != null ) handler(this, new InvalidSourceEventArgs(source, exception));
        }
    }
}
