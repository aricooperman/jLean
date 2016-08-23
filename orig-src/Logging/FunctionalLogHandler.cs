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
using System.ComponentModel.Composition;

package com.quantconnect.lean.Logging
{
    /**
    /// ILogHandler implementation that writes log output to result handler
    */
    [PartNotDiscoverable]
    public class FunctionalLogHandler : ILogHandler
    {
        private static final String DateFormat = "yyyyMMdd HH:mm:ss";
        private final Action<String> _debug;
        private final Action<String> _trace;
        private final Action<String> _error;

        /**
        /// Default constructor to handle MEF.
        */
        public FunctionalLogHandler() {

        }

        /**
        /// Initializes a new instance of the <see cref="QuantConnect.Logging.FunctionalLogHandler"/> class.
        */
        public FunctionalLogHandler(Action<String> debug, Action<String> trace, Action<String> error) {
            // saves references to the real console text writer since in a deployed state we may overwrite this in order
            // to redirect messages from algorithm to result handler
            _debug = debug;
            _trace = trace;
            _error = error;
        }

        /**
        /// Write error message to log
        */
         * @param text">The error text to log
        public void Error( String text) {
            if( _error != null ) {
                _error(DateTime.Now.toString(DateFormat) + " ERROR " + text);
            }
        }

        /**
        /// Write debug message to log
        */
         * @param text">The debug text to log
        public void Debug( String text) {
            if( _debug != null ) {
                _debug(DateTime.Now.toString(DateFormat) + " DEBUG " + text);
            }
        }

        /**
        /// Write debug message to log
        */
         * @param text">The trace text to log
        public void Trace( String text) {
            if( _trace != null ) {
                _trace(DateTime.Now.toString(DateFormat) + " TRACE " + text);
            }
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        /// <filterpriority>2</filterpriority>
        public void Dispose() {
        }
    }
}