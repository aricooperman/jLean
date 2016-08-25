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

using System.Collections.Generic;
using System.IO;

package com.quantconnect.lean.ToolBox
{
    /**
     * Provides an implementation of <see cref="IStreamProvider"/> that just returns a file stream
    */
    public class FileStreamProvider : IStreamProvider
    {
        private final Map<String, FileStream> _files = new Map<String, FileStream>();

        /**
         * Opens the specified source as read to be consumed stream
        */
         * @param source The source file to be opened
        @returns The stream representing the specified source
        public IEnumerable<Stream> Open( String source) {
            yield return File.OpenRead(source);
        }

        /**
         * Closes the specified source file stream
        */
         * @param source The source file to be closed
        public void Close( String source) {
            // it's expected that users will dispose the stream
            // from the open call, this is used to clean up any
            // other resources, for example a ZipFile stream
            // when we returned a ZipEntry stream
            _files.Remove(source);
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        public void Dispose() {
            foreach (kvp in _files) {
                kvp.Value.Dispose();
            }
        }
    }
}