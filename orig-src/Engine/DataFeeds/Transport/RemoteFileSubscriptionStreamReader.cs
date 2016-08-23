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

using System;
using System.IO;
using System.Net;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Transport
{
    /**
    /// Represents a stream reader capabable of downloading a remote file and then
    /// reading it from disk
    */
    public class RemoteFileSubscriptionStreamReader : IStreamReader
    {
        private final IStreamReader _streamReader;

        /**
        /// Initializes a new insance of the <see cref="RemoteFileSubscriptionStreamReader"/> class.
        */
         * @param source">The remote url to be downloaded via web client
         * @param downloadDirectory">The local directory and destination of the download
        public RemoteFileSubscriptionStreamReader( String source, String downloadDirectory) {
            // create a hash for a new filename
            filename = Guid.NewGuid() + source.GetExtension();
            destination = Path.Combine(downloadDirectory, filename);

            using (client = new WebClient()) {
                client.Proxy = WebRequest.GetSystemWebProxy();
                client.DownloadFile(source, destination);
            }

            // now we can just use the local file reader
            _streamReader = new LocalFileSubscriptionStreamReader(destination);
        }

        /**
        /// Gets <see cref="SubscriptionTransportMedium.RemoteFile"/>
        */
        public SubscriptionTransportMedium TransportMedium
        {
            get { return SubscriptionTransportMedium.RemoteFile; }
        }

        /**
        /// Gets whether or not there's more data to be read in the stream
        */
        public boolean EndOfStream
        {
            get { return _streamReader.EndOfStream; }
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
            _streamReader.Dispose();
        }
    }
}
