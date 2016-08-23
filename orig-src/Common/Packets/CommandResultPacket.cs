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

using QuantConnect.Commands;

package com.quantconnect.lean.Packets
{
    /**
    /// Contains data held as the result of executing a command
    */
    public class CommandResultPacket : Packet
    {
        /**
        /// Gets or sets the command that produced this packet
        */
        public String CommandName { get; set; }

        /**
        /// Gets or sets whether or not the
        */
        public boolean Success { get; set; }

        /**
        /// Initializes a new instance of the <see cref="CommandResultPacket"/> class
        */
        public CommandResultPacket(ICommand command, boolean success)
            : base(PacketType.CommandResult) {
            Success = success;
            CommandName = command.GetType().Name;
        }
    }
}
