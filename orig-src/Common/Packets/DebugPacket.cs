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
     * Send a simple debug message from the users algorithm to the console.
    */
    public class DebugPacket : Packet
    {
        /**
         * String debug message to send to the users console
        */
        @JsonProperty( "sMessage")]
        public String Message;

        /**
         * Associated algorithm Id.
        */
        @JsonProperty( "sAlgorithmID")]
        public String AlgorithmId;

        /**
         * Compile id of the algorithm sending this message
        */
        @JsonProperty( "sCompileID")]
        public String CompileId;

        /**
         * Project Id for this message
        */
        @JsonProperty( "iProjectID")]
        public int ProjectId;

        /**
         * True to emit message as a popup notification (toast),
         * false to emit message in console as text
        */
        @JsonProperty( "bToast")]
        public boolean Toast;

        /**
         * Default constructor for JSON
        */
        public DebugPacket()
            : base (PacketType.Debug) { }

        /**
         * Create a new instance of the notify debug packet:
        */
        public DebugPacket(int projectId, String algorithmId, String compileId, String message, boolean toast = false)
            : base(PacketType.Debug) {
            ProjectId = projectId;
            Message = message;
            CompileId = compileId;
            AlgorithmId = algorithmId;
            Toast = toast;
        }
    
    } // End Work Packet:

} // End of Namespace:
