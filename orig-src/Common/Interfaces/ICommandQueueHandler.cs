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
using QuantConnect.Commands;
using QuantConnect.Packets;

package com.quantconnect.lean.Interfaces
{
    /**
     * Represents a command queue for the algorithm. This is an entry point
     * for external messages to act upon the running algorithm instance.
    */
    [InheritedExport(typeof(ICommandQueueHandler))]
    public interface ICommandQueueHandler : IDisposable
    {
        /**
         * Initializes this command queue for the specified job
        */
         * @param job The job that defines what queue to bind to
         * @param algorithm The algorithm instance
        void Initialize(AlgorithmNodePacket job, IAlgorithm algorithm);

        /**
         * Gets the commands in the queue
        */
        @returns The next command in the queue, if present, null if no commands present
        IEnumerable<ICommand> GetCommands();
    }
}
