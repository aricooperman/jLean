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
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Lean.Engine.DataFeeds.Transport;
using QuantConnect.Util;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    /// <summary>
    /// Collection Subscription Factory takes a BaseDataCollection from BaseData factories
    /// and yields it one point at a time to the algorithm
    /// </summary>
    public class CollectionSubscriptionDataSourceReader : ISubscriptionDataSourceReader
    {
        
        private readonly DateTime _date;
        private readonly boolean _isLiveMode;
        private readonly BaseData _factory;
        private readonly SubscriptionDataConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionSubscriptionDataSourceReader"/> class
        /// </summary>
        /// <param name="config">The subscription's configuration</param>
        /// <param name="date">The date this factory was produced to read data for</param>
        /// <param name="isLiveMode">True if we're in live mode, false for backtesting</param>
        public CollectionSubscriptionDataSourceReader(SubscriptionDataConfig config, DateTime date, boolean isLiveMode)
        {
            _date = date;
            _config = config;
            _isLiveMode = isLiveMode;
            _factory = (BaseData)ObjectActivator.GetActivator(config.Type).Invoke(new object[0]);
        }

        /// <summary>
        /// Event fired when the specified source is considered invalid, this may
        /// be from a missing file or failure to download a remote source
        /// </summary>
        public event EventHandler<InvalidSourceEventArgs> InvalidSource;

        /// <summary>
        /// Event fired when an exception is thrown during a call to 
        /// <see cref="BaseData.Reader(SubscriptionDataConfig, string, DateTime, bool)"/>
        /// </summary>
        public event EventHandler<ReaderErrorEventArgs> ReaderError;

        /// <summary>
        /// Reads the specified <paramref name="source"/>
        /// </summary>
        /// <param name="source">The source to be read</param>
        /// <returns>An <see cref="IEnumerable{BaseData}"/> that contains the data in the source</returns>
        public IEnumerable<BaseData> Read(SubscriptionDataSource source)
        {
            IStreamReader reader = null;
            instances = new BaseDataCollection();
            try
            {
                switch (source.TransportMedium)
                {
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
                    if (instances == null)
                    {
                        OnInvalidSource(source, new Exception("Reader must generate a BaseDataCollection with the FileFormat.Collection"));
                    }
                }
                catch (Exception err)
                {
                    OnReaderError(raw, err);
                }

                foreach (instance in instances.Data)
                {
                    yield return instance;
                }
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }
        }

        /// <summary>
        /// Event invocator for the <see cref="ReaderError"/> event
        /// </summary>
        /// <param name="line">The line that caused the exception</param>
        /// <param name="exception">The exception that was caught</param>
        private void OnReaderError( String line, Exception exception)
        {
            handler = ReaderError;
            if (handler != null) handler(this, new ReaderErrorEventArgs(line, exception));
        }

        /// <summary>
        /// Event invocator for the <see cref="InvalidSource"/> event
        /// </summary>
        /// <param name="source">The <see cref="SubscriptionDataSource"/> that was invalid</param>
        /// <param name="exception">The exception if one was raised, otherwise null</param>
        private void OnInvalidSource(SubscriptionDataSource source, Exception exception)
        {
            handler = InvalidSource;
            if (handler != null) handler(this, new InvalidSourceEventArgs(source, exception));
        }
    }
}
