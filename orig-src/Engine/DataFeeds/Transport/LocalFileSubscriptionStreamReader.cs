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
 *
*/

using System.IO;
using Ionic.Zip;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Transport
{
    /**
    /// Represents a stream reader capable of reading lines from disk
    */
    public class LocalFileSubscriptionStreamReader : IStreamReader
    {
        private StreamReader _streamReader;
        private final ZipFile _zipFile;

        /**
        /// Initializes a new instance of the <see cref="LocalFileSubscriptionStreamReader"/> class.
        */
         * @param source">The local file to be read
         * @param entryName">Specifies the zip entry to be opened. Leave null if not applicable,
        /// or to open the first zip entry found regardless of name
        public LocalFileSubscriptionStreamReader( String source, String entryName = null ) {
            // unzip if necessary
            _streamReader = source.GetExtension() == ".zip"
                ? Compression.Unzip(source, entryName, out _zipFile)
                : new StreamReader(source);
        }

        /**
        /// Gets <see cref="SubscriptionTransportMedium.LocalFile"/>
        */
        public SubscriptionTransportMedium TransportMedium
        {
            get { return SubscriptionTransportMedium.LocalFile; }
        }

        /**
        /// Gets whether or not there's more data to be read in the stream
        */
        public boolean EndOfStream
        {
            get { return _streamReader == null || _streamReader.EndOfStream; }
        }

        /**
        /// Gets the next line/batch of content from the stream 
        */
        public String ReadLine() {
            return _streamReader.ReadLine();
        }

        /**
        /// Disposes of the stream
        */
        public void Dispose() {
            if( _streamReader != null ) {
                _streamReader.Dispose();
                _streamReader = null;
            }
            if( _zipFile != null ) {
                _zipFile.Dispose();
            }
        }
    }
}