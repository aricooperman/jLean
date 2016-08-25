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

package com.quantconnect.lean.Logging
{
    /**
     * Provides an <see cref="ILogHandler"/> implementation that composes multiple handlers
    */
    public class CompositeLogHandler : ILogHandler
    {
        private final ILogHandler[] _handlers;

        /**
         * Initializes a new instance of the <see cref="CompositeLogHandler"/> that pipes log messages to the console and log.txt
        */
        public CompositeLogHandler()
            : this(new ILogHandler[] {new ConsoleLogHandler(), new FileLogHandler()}) {
        }

        /**
         * Initializes a new instance of the <see cref="CompositeLogHandler"/> class from the specified handlers
        */
         * @param handlers The implementations to compose
        public CompositeLogHandler(ILogHandler[] handlers) {
            if( handlers == null || handlers.Length == 0) {
                throw new ArgumentNullException( "handlers");
            }

            _handlers = handlers;
        }

        /**
         * Write error message to log
        */
         * @param text">
        public void Error( String text) {
            foreach (handler in _handlers) {
                handler.Error(text);
            }
        }

        /**
         * Write debug message to log
        */
         * @param text">
        public void Debug( String text) {
            foreach (handler in _handlers) {
                handler.Debug(text);
            }
        }

        /**
         * Write debug message to log
        */
         * @param text">
        public void Trace( String text) {
            foreach (handler in _handlers) {
                handler.Trace(text);
            }
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
         * <filterpriority>2</filterpriority>
        public void Dispose() {
            foreach (handler in _handlers) {
                handler.Dispose();
            }
        }
    }
}