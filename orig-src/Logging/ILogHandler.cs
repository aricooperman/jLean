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
    /// Interface for redirecting log output
    */
    [InheritedExport(typeof(ILogHandler))]
    public interface ILogHandler : IDisposable
    {
        /**
        /// Write error message to log
        */
         * @param text">The error text to log
        void Error( String text);
       
        /**
        /// Write debug message to log
        */
         * @param text">The debug text to log
        void Debug( String text);
       
        /**
        /// Write debug message to log
        */
         * @param text">The trace text to log
        void Trace( String text);
    }
}