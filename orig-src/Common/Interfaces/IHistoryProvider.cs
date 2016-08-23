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
using System.ComponentModel.Composition;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Packets;
using HistoryRequest = QuantConnect.Data.HistoryRequest;

package com.quantconnect.lean.Interfaces
{
    /**
    /// Provides historical data to an algorithm at runtime
    */
    [InheritedExport(typeof(IHistoryProvider))]
    public interface IHistoryProvider
    {
        /**
        /// Gets the total number of data points emitted by this history provider
        */
        int DataPointCount { get; }

        /**
        /// Initializes this history provider to work for the specified job
        */
         * @param job">The job
         * @param mapFileProvider">Provider used to get a map file resolver to handle equity mapping
         * @param factorFileProvider">Provider used to get factor files to handle equity price scaling
         * @param statusUpdate">Function used to send status updates
        void Initialize(AlgorithmNodePacket job, IMapFileProvider mapFileProvider, IFactorFileProvider factorFileProvider, Action<Integer> statusUpdate);

        /**
        /// Gets the history for the requested securities
        */
         * @param requests">The historical data requests
         * @param sliceTimeZone">The time zone used when time stamping the slice instances
        @returns An enumerable of the slices of data covering the span specified in each request
        IEnumerable<Slice> GetHistory(IEnumerable<HistoryRequest> requests, ZoneId sliceTimeZone);
    }
}
