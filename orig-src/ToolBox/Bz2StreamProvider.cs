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
using Ionic.BZip2;

package com.quantconnect.lean.ToolBox
{
    public class Bz2StreamProvider : IStreamProvider
    {
        /**
         * Opens the specified source as read to be consumed stream
        */
         * @param source The source file to be opened
        @returns The stream representing the specified source
        public IEnumerable<Stream> Open( String source) {
            yield return new BZip2InputStream(File.OpenRead(source));
        }

        /**
         * Closes the specified source file stream
        */
         * @param source The source file to be closed
        public void Close( String source) {
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        public void Dispose() {
        }
    }
}