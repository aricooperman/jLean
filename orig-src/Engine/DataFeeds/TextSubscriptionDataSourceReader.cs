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
using System.ComponentModel;
using System.IO;
using QuantConnect.Data;
using QuantConnect.Lean.Engine.DataFeeds.Transport;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Provides an implementations of <see cref="ISubscriptionDataSourceReader"/> that uses the 
     * <see cref="BaseData.Reader(QuantConnect.Data.SubscriptionDataConfig,string,System.DateTime,bool)"/>
     * method to read lines of text from a <see cref="SubscriptionDataSource"/>
    */
    public class TextSubscriptionDataSourceReader : ISubscriptionDataSourceReader
    {
        private final boolean _isLiveMode;
        private final BaseData _factory;
        private final DateTime _date;
        private final SubscriptionDataConfig _config;

        /**
         * Event fired when the specified source is considered invalid, this may
         * be from a missing file or failure to download a remote source
        */
        public event EventHandler<InvalidSourceEventArgs> InvalidSource;

        /**
         * Event fired when an exception is thrown during a call to 
         * <see cref="BaseData.Reader(QuantConnect.Data.SubscriptionDataConfig,string,System.DateTime,bool)"/>
        */
        public event EventHandler<ReaderErrorEventArgs> ReaderError;

        /**
         * Event fired when there's an error creating an <see cref="IStreamReader"/> or the
         * instantiated <see cref="IStreamReader"/> has no data.
        */
        public event EventHandler<CreateStreamReaderErrorEventArgs> CreateStreamReaderError;

        /**
         * Initializes a new instance of the <see cref="TextSubscriptionDataSourceReader"/> class
        */
         * @param config The subscription's configuration
         * @param date The date this factory was produced to read data for
         * @param isLiveMode True if we're in live mode, false for backtesting
        public TextSubscriptionDataSourceReader(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            _date = date;
            _config = config;
            _isLiveMode = isLiveMode;
            _factory = (BaseData) ObjectActivator.GetActivator(config.Type).Invoke(new object[0]);
        }

        /**
         * Reads the specified <paramref name="source"/>
        */
         * @param source The source to be read
        @returns An <see cref="IEnumerable{BaseData}"/> that contains the data in the source
        public IEnumerable<BaseData> Read(SubscriptionDataSource source) {
            using (reader = CreateStreamReader(source)) {
                // if the reader doesn't have data then we're done with this subscription
                if( reader == null || reader.EndOfStream) {
                    OnCreateStreamReaderError(_date, source);
                    yield break;
                }

                // while the reader has data
                while (!reader.EndOfStream) {
                    // read a line and pass it to the base data factory
                    line = reader.ReadLine();
                    BaseData instance = null;
                    try
                    {
                        instance  = _factory.Reader(_config, line, _date, _isLiveMode);
                    }
                    catch (Exception err) {
                        OnReaderError(line, err);
                    }

                    if( instance != null ) {
                        yield return instance;
                    }
                }
            }
        }

        /**
         * Creates a new <see cref="IStreamReader"/> for the specified <paramref name="subscriptionDataSource"/>
        */
         * @param subscriptionDataSource The source to produce an <see cref="IStreamReader"/> for
        @returns A new instance of <see cref="IStreamReader"/> to read the source, or null if there was an error
        private IStreamReader CreateStreamReader(SubscriptionDataSource subscriptionDataSource) {
            IStreamReader reader;
            switch (subscriptionDataSource.TransportMedium) {
                case SubscriptionTransportMedium.LocalFile:
                    reader = HandleLocalFileSource(subscriptionDataSource);
                    break;

                case SubscriptionTransportMedium.RemoteFile:
                    reader = HandleRemoteSourceFile(subscriptionDataSource);
                    break;

                case SubscriptionTransportMedium.Rest:
                    reader = new RestSubscriptionStreamReader(subscriptionDataSource.Source);
                    break;

                default:
                    throw new InvalidEnumArgumentException( "Unexpected SubscriptionTransportMedium specified: " + subscriptionDataSource.TransportMedium);
            }
            return reader;
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

        /**
         * Event invocator for the <see cref="ReaderError"/> event
        */
         * @param line The line that caused the exception
         * @param exception The exception that was caught
        private void OnReaderError( String line, Exception exception) {
            handler = ReaderError;
            if( handler != null ) handler(this, new ReaderErrorEventArgs(line, exception));
        }

        /**
         * Event invocator for the <see cref="CreateStreamReaderError"/> event
        */
         * @param date The date of the source
         * @param source The source that caused the error
        private void OnCreateStreamReaderError(DateTime date, SubscriptionDataSource source) {
            handler = CreateStreamReaderError;
            if( handler != null ) handler(this, new CreateStreamReaderErrorEventArgs(date, source));
        }

        /**
         * Opens up an IStreamReader for a local file source
        */
        private IStreamReader HandleLocalFileSource(SubscriptionDataSource source) {
            String entryName = null; // default to all entries
            file = source.Source;
            hashIndex = source.Source.LastIndexOf( "#", StringComparison.Ordinal);
            if( hashIndex != -1) {
                entryName = source.Source.Substring(hashIndex + 1);
                file = source.Source.Substring(0, hashIndex);
            }

            if( !File.Exists(file)) {
                OnInvalidSource(source, new FileNotFoundException( "The specified file was not found", file));
                return null;
            }

            // handles zip or text files
            return new LocalFileSubscriptionStreamReader(file, entryName);
        }

        /**
         * Opens up an IStreamReader for a remote file source
        */
        private IStreamReader HandleRemoteSourceFile(SubscriptionDataSource source) {
            // clean old files out of the cache
            if( !Directory.Exists(Globals.Cache)) Directory.CreateDirectory(Globals.Cache);
            foreach (file in Directory.EnumerateFiles(Globals.Cache)) {
                if( File.GetCreationTime(file) < DateTime.Now.AddHours(-24)) File.Delete(file);
            }

            try
            {
                // this will fire up a web client in order to download the 'source' file to the cache
                return new RemoteFileSubscriptionStreamReader(source.Source, Globals.Cache);
            }
            catch (Exception err) {
                OnInvalidSource(source, err);
                return null;
            }
        }
    }
}