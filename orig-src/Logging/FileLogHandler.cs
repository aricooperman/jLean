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
using System.IO;

package com.quantconnect.lean.Logging
{
    /**
     * Provides an implementation of <see cref="ILogHandler"/> that writes all log messages to a file on disk.
    */
    public class FileLogHandler : ILogHandler
    {
        private boolean _disposed;

        // we need to control synchronization to our stream writer since it's not inherently thread-safe
        private final object _synchronized= new object();
        private final Lazy<TextWriter> _writer;
        private final boolean _useTimestampPrefix;

        /**
         * Initializes a new instance of the <see cref="FileLogHandler"/> class to write messages to the specified file path.
         * The file will be opened using <see cref="FileMode.Append"/>
        */
         * @param filepath The file path use to save the log messages
         * @param useTimestampPrefix True to prefix each line in the log which the UTC timestamp, false otherwise
        public FileLogHandler( String filepath, boolean useTimestampPrefix = true) {
            _useTimestampPrefix = useTimestampPrefix;
            _writer = new Lazy<TextWriter>(
                () -> new StreamWriter(File.Open(filepath, FileMode.Append, FileAccess.Write, FileShare.Read))
                );
        }

        /**
         * Initializes a new instance of the <see cref="FileLogHandler"/> class using 'log.txt' for the filepath.
        */
        public FileLogHandler()
            : this( "log.txt") {
        }

        /**
         * Write error message to log
        */
         * @param text The error text to log
        public void Error( String text) {
            WriteMessage(text, "ERROR");
        }

        /**
         * Write debug message to log
        */
         * @param text The debug text to log
        public void Debug( String text) {
            WriteMessage(text, "DEBUG");
        }

        /**
         * Write debug message to log
        */
         * @param text The trace text to log
        public void Trace( String text) {
            WriteMessage(text, "TRACE");
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
         * <filterpriority>2</filterpriority>
        public void Dispose() {
            synchronized(_lock) {
                if( _writer.IsValueCreated) {
                    _disposed = true;
                    _writer.Value.Dispose();
                }
            }
        }

        /**
         * Creates the message to be logged
        */
         * @param text The text to be logged
         * @param level The logging leel
        @returns 
        protected String CreateMessage( String text, String level) {
            if( _useTimestampPrefix) {
                return String.format( "%1$s %2$s:: %3$s", DateTime.UtcNow.toString( "o"), level, text);
            }
            return String.format( "%1$s:: %2$s", level, text);
        }

        /**
         * Writes the message to the writer
        */
        private void WriteMessage( String text, String level) {
            synchronized(_lock) {
                if( _disposed) return;
                _writer.Value.WriteLine(CreateMessage(text, level));
                _writer.Value.Flush();
            }
        }
    }
}
