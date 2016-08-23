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

using QuantConnect.Interfaces;
using QuantConnect.Packets;

package com.quantconnect.lean.Commands
{
    /**
    /// Represents a command that will change the algorithm's status
    */
    public class AlgorithmStatusCommand : ICommand
    {
        /**
        /// Gets or sets the algorithm status
        */
        public AlgorithmStatus Status { get; set; }

        /**
        /// Initializes a new instance of the <see cref="AlgorithmStatusCommand"/>
        */
        public AlgorithmStatusCommand() {
            Status = AlgorithmStatus.Running;
        }

        /**
        /// Initializes a new instance of the <see cref="AlgorithmStatusCommand"/> with
        /// the specified status
        */
        public AlgorithmStatusCommand(AlgorithmStatus status) {
            Status = status;
        }

        /**
        /// Sets the algoritm's status to <see cref="Status"/>
        */
         * @param algorithm">The algorithm to run this command against
        public CommandResultPacket Run(IAlgorithm algorithm) {
            algorithm.Status = Status;
            return new CommandResultPacket(this, true);
        }
    }
}