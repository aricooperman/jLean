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

package com.quantconnect.lean.Logging
{
    /**
     * Log entry wrapper to make logging simpler:
    */
    public class LogEntry
    {
        /**
         * Time of the log entry
        */
        public DateTime Time;

        /**
         * Message of the log entry
        */
        public String Message;

        /**
         * Descriptor of the message type.
        */
        public LogType MessageType;

        /**
         * Create a default log message with the current time.
        */
         * @param message">
        public LogEntry( String message) {
            Time = DateTime.UtcNow;
            Message = message;
            MessageType = LogType.Trace;
        }

        /**
         * Create a log entry at a specific time in the analysis (for a backtest).
        */
         * @param message Message for log
         * @param time Time of the message
         * @param type Type of the log entry
        public LogEntry( String message, DateTime time, LogType type = LogType.Trace) {
            Time = time.ToUniversalTime();
            Message = message;
            MessageType = type;
        }

        /**
         * Helper @Override on the log entry.
        */
        @returns 
        public @Override String toString() {
            return String.format( "%1$s %2$s %3$s", Time.toString( "o"), MessageType, Message);
        }
    }
}
