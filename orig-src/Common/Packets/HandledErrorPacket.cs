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

using Newtonsoft.Json;

package com.quantconnect.lean.Packets
{
    /**
    /// Algorithm runtime error packet from the lean engine. 
    /// This is a managed error which stops the algorithm execution.
    */
    public class HandledErrorPacket : Packet
    {
        /**
        /// Runtime error message from the exception
        */
        @JsonProperty( "sMessage")]
        public String Message;

        /**
        /// Algorithm id which generated this runtime error
        */
        @JsonProperty( "sAlgorithmID")]
        public String AlgorithmId;

        /**
        /// Error stack trace information String passed through from the Lean exception
        */
        @JsonProperty( "sStackTrace")]
        public String StackTrace;

        /**
        /// Default constructor for JSON
        */
        public HandledErrorPacket()
            : base (PacketType.HandledError) { }

        /**
        /// Create a new handled error packet
        */
        public HandledErrorPacket( String algorithmId, String message, String stacktrace = "")
            : base(PacketType.HandledError) {
            Message = message;
            AlgorithmId = algorithmId;
            StackTrace = stacktrace;
        }
    
    } // End Work Packet:

} // End of Namespace:
