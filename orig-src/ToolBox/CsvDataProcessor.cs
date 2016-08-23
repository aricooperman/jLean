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
using System.Threading.Tasks;
using QuantConnect.Data;
using QuantConnect.Util;

package com.quantconnect.lean.ToolBox
{
    /**
    /// Provides an implementation of <see cref="IDataProcessor"/> that writes the incoming
    /// stream of data to a csv file.
    */
    public class CsvDataProcessor : IDataProcessor
    {
        private static final int TicksPerFlush = 50;
        private static final object DirectoryCreateSync = new object();
        
        private final String _dataDirectory;
        private final Resolution _resolution;
        private final TickType _tickType;
        private final Map<Symbol, Writer> _writers;

        /**
        /// Initializes a new instance of the <see cref="CsvDataProcessor"/> class
        */
         * @param dataDirectory">The root data directory, /Data
         * @param resolution">The resolution being sent into the Process method
         * @param tickType">The tick type, trade or quote
        public CsvDataProcessor( String dataDirectory, Resolution resolution, TickType tickType) {
            _dataDirectory = dataDirectory;
            _resolution = resolution;
            _tickType = tickType;
            _writers = new Map<Symbol, Writer>();
        }

        /**
        /// Invoked for each piece of data from the source file
        */
         * @param data">The data to be processed
        public void Process(BaseData data) {
            Writer writer;
            if( !_writers.TryGetValue(data.Symbol, out writer)) {
                writer = CreateTextWriter(data);
                _writers[data.Symbol] = writer;
            }

            // flush every so often
            if( ++writer.ProcessCount%TicksPerFlush == 0) {
                writer.TextWriter.Flush();
            }

            line = LeanData.GenerateLine(data, data.Symbol.ID.SecurityType, _resolution);
            writer.TextWriter.WriteLine(line);
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        public void Dispose() {
            foreach (kvp in _writers) {
                kvp.Value.TextWriter.Dispose();
            }
        }

        /**
        /// Creates the <see cref="TextWriter"/> that writes data to csv files
        */
        private Writer CreateTextWriter(BaseData data) {
            entry = LeanData.GenerateZipEntryName(data.Symbol, data.Time.Date, _resolution, _tickType);
            relativePath = LeanData.GenerateRelativeZipFilePath(data.Symbol, data.Time.Date, _resolution, _tickType)
                .Replace( ".zip", string.Empty);
            path = Path.Combine(Path.Combine(_dataDirectory, relativePath), entry);
            directory = new FileInfo(path).Directory.FullName;
            if( !Directory.Exists(directory)) {
                // lock before checking again
                lock (DirectoryCreateSync) if( !Directory.Exists(directory)) Directory.CreateDirectory(directory);
            }

            return new Writer(path, new StreamWriter(path));
        }


        private sealed class Writer
        {
            public final String Path;
            public final TextWriter TextWriter;
            public int ProcessCount;
            public Writer( String path, TextWriter textWriter) {
                Path = path;
                TextWriter = textWriter;
            }
        }
    }
}