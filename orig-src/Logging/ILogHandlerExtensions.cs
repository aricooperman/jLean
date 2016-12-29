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
     * Logging extensions.
    */
    public static class LogHandlerExtensions
    {
        /**
         * Write error message to log
        */
         * @param logHandler">
         * @param text Message
         * @param args Arguments to format.
        public static void Error(this ILogHandler logHandler, String text, params object[] args) {
            if( logHandler == null ) {
                throw new NullPointerException( "logHandler", "Log Handler cannot be null");
            }

            logHandler.Error( String.format(text, args));
        }

        /**
         * Write debug message to log
        */
         * @param logHandler">
         * @param text Message
         * @param args Arguments to format.
        public static void Debug(this ILogHandler logHandler, String text, params object[] args) {
            if( logHandler == null ) {
                throw new NullPointerException( "logHandler", "Log Handler cannot be null");
            }

            logHandler.Debug( String.format(text, args));
        }

        /**
         * Write debug message to log
        */
         * @param logHandler">
         * @param text Message
         * @param args Arguments to format.
        public static void Trace(this ILogHandler logHandler, String text, params object[] args) {
            if( logHandler == null ) {
                throw new NullPointerException( "logHandler", "Log Handler cannot be null");
            }

            logHandler.Trace( String.format(text, args));
        }
    }
}
