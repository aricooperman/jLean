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

package com.quantconnect.lean.ToolBox
{
    /**
    /// Defines how to open/close a source file
    */
    public interface IStreamProvider : IDisposable
    {
        /**
        /// Opens the specified source as read to be consumed stream
        */
         * @param source">The source file to be opened
        @returns The stream representing the specified source
        IEnumerable<Stream> Open( String source);

        /**
        /// Closes the specified source file stream
        */
         * @param source">The source file to be closed
        void Close( String source);
    }

    /**
    /// Provides factor method for creating an <see cref="IStreamProvider"/> from a file name
    */
    public static class StreamProvider
    {
        /**
        /// Creates a new <see cref="IStreamProvider"/> capable of reading a file with the specified extenson
        */
         * @param extension">The file extension
        @returns A new stream provider capable of reading files with the specified extension
        public static IStreamProvider ForExtension( String extension) {
            ext = Path.GetExtension(extension);
            if( ext == ".zip") {
                return new ZipStreamProvider();
            }
            if( ext == ".bz2") {
                return new Bz2StreamProvider();
            }

            return new FileStreamProvider();
        }
    }
}