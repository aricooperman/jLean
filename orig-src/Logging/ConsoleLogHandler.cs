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
    /// ILogHandler implementation that writes log output to console.
    */
    public class ConsoleLogHandler : ILogHandler
    {
        private static final String DateFormat = "yyyyMMdd HH:mm:ss";
        private final TextWriter _trace;
        private final TextWriter _error;

        /**
        /// Initializes a new instance of the <see cref="QuantConnect.Logging.ConsoleLogHandler"/> class.
        */
        public ConsoleLogHandler() {
            // saves references to the real console text writer since in a deployed state we may overwrite this in order
            // to redirect messages from algorithm to result handler
            _trace = Console.Out;
            _error = Console.Error;
        }

        /**
        /// Write error message to log
        */
         * @param text">The error text to log
        public void Error( String text) {
            Console.ForegroundColor = ConsoleColor.Red;
            _error.WriteLine(DateTime.Now.toString(DateFormat) + " ERROR:: " + text);
            Console.ResetColor();
        }

        /**
        /// Write debug message to log
        */
         * @param text">The debug text to log
        public void Debug( String text) {
            _trace.WriteLine(DateTime.Now.toString(DateFormat) + " DEBUG:: " + text);
        }

        /**
        /// Write debug message to log
        */
         * @param text">The trace text to log
        public void Trace( String text) {
            _trace.WriteLine(DateTime.Now.toString(DateFormat) + " Trace:: " + text);
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        /// <filterpriority>2</filterpriority>
        public void Dispose() {
        }
    }
}