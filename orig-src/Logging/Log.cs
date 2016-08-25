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
using System.Collections;
using System.Text;
using System.Threading;

package com.quantconnect.lean.Logging 
{
    /**
     * Logging management class.
    */
    public static class Log
    {
        private static String _lastTraceText = "";
        private static String _lastErrorText = "";
        private static boolean _debuggingEnabled;
        private static int _level = 1;
        private static ILogHandler _logHandler = new ConsoleLogHandler();

        /**
         * Gets or sets the ILogHandler instance used as the global logging implementation.
        */
        public static ILogHandler LogHandler
        {
            get { return _logHandler; }
            set { _logHandler = value; }
        }

        /**
         * Global flag whether to enable debugging logging:
        */
        public static boolean DebuggingEnabled
        {
            get { return _debuggingEnabled; }
            set { _debuggingEnabled = value; }
        }

        /**
         * Set the minimum message level:
        */
        public static int DebuggingLevel
        {
            get { return _level; }
            set { _level = value; }
        }

        /**
         * Log error
        */
         * @param error String Error
         * @param @OverrideMessageFloodProtection Force sending a message, overriding the "do not flood" directive
        public static void Error( String error, boolean @OverrideMessageFloodProtection = false) {
            try 
            {
                if( error == _lastErrorText && !@OverrideMessageFloodProtection) return;
                _logHandler.Error(error);
                _lastErrorText = error; //Stop message flooding filling diskspace.
            } 
            catch (Exception err) {
                Console.WriteLine( "Log.Error(): Error writing error: " + err.Message);
            }
        }

        /**
         * Log error. This overload is usefull when exceptions are being thrown from within an anonymous function.
        */
         * @param method The method identifier to be used
         * @param exception The exception to be logged
         * @param message An optional message to be logged, if null/whitespace the messge text will be extracted
         * @param @OverrideMessageFloodProtection Force sending a message, overriding the "do not flood" directive
        private static void Error( String method, Exception exception, String message = null, boolean @OverrideMessageFloodProtection = false) {
            message = method + "(): " + (message ?? string.Empty) + " " + exception;
            Error(message, @OverrideMessageFloodProtection);
        }

        /**
         * Log error
        */
         * @param exception The exception to be logged
         * @param message An optional message to be logged, if null/whitespace the messge text will be extracted
         * @param @OverrideMessageFloodProtection Force sending a message, overriding the "do not flood" directive
        public static void Error(Exception exception, String message = null, boolean @OverrideMessageFloodProtection = false) {
            Error(WhoCalledMe.GetMethodName(1), exception, message, @OverrideMessageFloodProtection);
        }

        /**
         * Log trace
        */
        public static void Trace( String traceText, boolean @OverrideMessageFloodProtection = false) { 
            try 
            {
                if( traceText == _lastTraceText && !@OverrideMessageFloodProtection) return;
                _logHandler.Trace(traceText);
                _lastTraceText = traceText;
            } 
            catch (Exception err) {
                Console.WriteLine( "Log.Trace(): Error writing trace: "  +err.Message);
            }
        }

        /**
         * Writes the message in normal text
        */
        public static void Trace( String format, params object[] args) {
            Trace( String.format(format, args));
        }

        /**
         * Writes the message in red
        */
        public static void Error( String format, params object[] args) {
            Error( String.format(format, args));
        }

        /**
         * Output to the console, and sleep the thread for a little period to monitor the results.
        */
         * @param text">
         * @param level debug level
         * @param delay">
        public static void Debug( String text, int level = 1, int delay = 0) {
            try
            {
                if( !_debuggingEnabled || level < _level) return;
                _logHandler.Debug(text);
                Thread.Sleep(delay);
            }
            catch (Exception err) {
                Console.WriteLine( "Log.Debug(): Error writing debug: " + err.Message);
            }
        }

        /**
         * C# Equivalent of Print_r in PHP:
        */
         * @param obj">
         * @param recursion">
        @returns 
        public static String VarDump(object obj, int recursion = 0) {
            result = new StringBuilder();

            // Protect the method against endless recursion
            if( recursion < 5) {
                // Determine object type
                t = obj.GetType();

                // Get array with properties for this object
                properties = t.GetProperties();

                foreach (property in properties) {
                    try
                    {
                        // Get the property value
                        value = property.GetValue(obj, null );

                        // Create indenting String to put in front of properties of a deeper level
                        // We'll need this when we display the property name and value
                        indent = String.Empty;
                        spaces = "|   ";
                        trail = "|...";

                        if( recursion > 0) {
                            indent = new StringBuilder(trail).Insert(0, spaces, recursion - 1).toString();
                        }

                        if( value != null ) {
                            // If the value is a string, add quotation marks
                            displayValue = value.toString();
                            if( value is string) displayValue = String.Concat('"', displayValue, '"');

                            // Add property name and value to return string
                            result.AppendFormat( "%1$s%2$s = %3$s\n", indent, property.Name, displayValue);

                            try 
                            {
                                if( !(value is ICollection)) {
                                    // Call var_dump() again to list child properties
                                    // This throws an exception if the current property value
                                    // is of an unsupported type (eg. it has not properties)
                                    result.Append(VarDump(value, recursion + 1));
                                } 
                                else 
                                {
                                    // 2009-07-29: added support for collections
                                    // The value is a collection (eg. it's an arraylist or generic list)
                                    // so loop through its elements and dump their properties
                                    elementCount = 0;
                                    foreach (element in ((ICollection)value)) {
                                        elementName = String.format( "%1$s[%2$s]", property.Name, elementCount);
                                        indent = new StringBuilder(trail).Insert(0, spaces, recursion).toString();

                                        // Display the collection element name and type
                                        result.AppendFormat( "%1$s%2$s = %3$s\n", indent, elementName, element.toString());

                                        // Display the child properties
                                        result.Append(VarDump(element, recursion + 2));
                                        elementCount++;
                                    }

                                    result.Append(VarDump(value, recursion + 1));
                                }
                            } catch { }
                        } 
                        else 
                        {
                            // Add empty (null ) property to return string
                            result.AppendFormat( "%1$s%2$s = %3$s\n", indent, property.Name, "null");
                        }
                    } 
                    catch 
                    {
                        // Some properties will throw an exception on property.GetValue()
                        // I don't know exactly why this happens, so for now i will ignore them...
                    }
                }
            }

            return result.toString();
        }
    }
}
