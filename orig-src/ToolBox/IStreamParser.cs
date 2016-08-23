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
using QuantConnect.Data;

package com.quantconnect.lean.ToolBox
{
    /**
    /// Represents a type capable of accepting a stream and parsing it into an enumerable of data
    */
    public interface IStreamParser : IDisposable
    {
        /**
        /// Parses the specified input stream into an enumerable of data
        */
         * @param source">The source of the stream
         * @param stream">The input stream to be parsed
        @returns An enumerable of base data
        IEnumerable<BaseData> Parse( String source, Stream stream);
    }
}