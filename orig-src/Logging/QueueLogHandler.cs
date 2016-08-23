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
using System.Collections.Concurrent;
using System.IO;

package com.quantconnect.lean.Logging
{
    /**
    /// ILogHandler implementation that queues all logs and writes them when instructed.
    */
    public class QueueLogHandler : ILogHandler
    {
        private final ConcurrentQueue<LogEntry> _logs;
        private static final String DateFormat = "yyyyMMdd HH:mm:ss";
        private final TextWriter _trace;
        private final TextWriter _error;

        /**
        /// Public access to the queue for log processing.
        */
        public ConcurrentQueue<LogEntry> Logs
        {
            get { return _logs; }
        }

        /**
        /// LOgging event delegate
        */
        public delegate void LogEventRaised(LogEntry log);

        /**
        /// Logging Event Handler
        */
        public event LogEventRaised LogEvent;
        
        /**
        /// Initializes a new instance of the <see cref="QueueLogHandler"/> class.
        */
        public QueueLogHandler() {
            _logs = new ConcurrentQueue<LogEntry>();
            _trace = Console.Out;
            _error = Console.Error;
        }

        /**
        /// Write error message to log
        */
         * @param text">The error text to log
        public void Error( String text) {
            log = new LogEntry(text, DateTime.Now, LogType.Error);
            _logs.Enqueue(log);
            OnLogEvent(log);

            Console.ForegroundColor = ConsoleColor.Red;
            _error.WriteLine(DateTime.Now.toString(DateFormat) + " Error:: " + text);
            Console.ResetColor();
        }

        /**
        /// Write debug message to log
        */
         * @param text">The debug text to log
        public void Debug( String text) {
            log = new LogEntry(text, DateTime.Now, LogType.Debug);
            _logs.Enqueue(log);
            OnLogEvent(log);

            _trace.WriteLine(DateTime.Now.toString(DateFormat) + " Debug:: " + text);
        }

        /**
        /// Write debug message to log
        */
         * @param text">The trace text to log
        public void Trace( String text) {
            log = new LogEntry(text, DateTime.Now, LogType.Trace);
            _logs.Enqueue(log);
            OnLogEvent(log);

            _trace.WriteLine(DateTime.Now.toString(DateFormat) + " Trace:: " + text);
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        /// <filterpriority>2</filterpriority>
        public void Dispose() {
        }

        /**
        /// Raise a log event safely
        */
        protected virtual void OnLogEvent(LogEntry log) {
            handler = LogEvent;

            if( handler != null ) {
                handler(log);
            }
        }
    }
}