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
using System.Text;

package com.quantconnect.lean.Util
{
    /**
    /// Provides an implementation of <see cref="TextWriter"/> that redirects Write( String) and WriteLine( String)
    */
    public class FuncTextWriter : TextWriter
    {
        private final Action<String> _writer;

        /// <inheritdoc />
        public @Override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        /**
        /// Initializes a new instance of the <see cref="FuncTextWriter"/> that will direct
        /// messages to the algorithm's Debug function.
        */
         * @param writer">The algorithm hosting the Debug function where messages will be directed
        public FuncTextWriter(Action<String> writer) {
            _writer = writer;
        }

        /**
        /// Writes the String value using the delegate provided at construction
        */
         * @param value">The String value to be written
        public @Override void Write( String value) {
            _writer(value);
        }

        /**
        /// Writes the String value using the delegate provided at construction
        */
         * @param value">
        public @Override void WriteLine( String value) {
            // these are grouped in a list so we don't need to add new line characters here
            _writer(value);
        }
    }
}
