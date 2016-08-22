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
using System.Linq;
using Ionic.Zip;

package com.quantconnect.lean.ToolBox
{
    /// <summary>
    /// Provides an implementation of <see cref="IStreamProvider"/> that opens zip files
    /// </summary>
    public class ZipStreamProvider : IStreamProvider
    {
        private readonly object _sync = new object();
        private readonly Map<String, ZipFile> _zipFiles = new Map<String, ZipFile>();

        /// <summary>
        /// Opens the specified source as read to be consumed stream
        /// </summary>
        /// <param name="source">The source file to be opened</param>
        /// <returns>The stream representing the specified source</returns>
        public IEnumerable<Stream> Open( String source) {
            lock (_sync) {
                archive = new ZipFile(source);
                _zipFiles.Add(source, archive);
                foreach (entry in archive) {
                    yield return entry.OpenReader();
                }
            }
        }

        /// <summary>
        /// Closes the specified source file stream
        /// </summary>
        /// <param name="source">The source file to be closed</param>
        public void Close( String source) {
            lock (_sync) {
                ZipFile archive;
                if( _zipFiles.TryGetValue(source, out archive)) {
                    _zipFiles.Remove(source);
                    archive.Dispose();
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            lock (_sync) {
                foreach (zipFile in _zipFiles.Values) {
                    zipFile.Dispose();
                }
                _zipFiles.Clear();
            }
        }
    }
}