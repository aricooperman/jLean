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
using Newtonsoft.Json.Converters;

package com.quantconnect.lean.Packets
{
    /**
     * Algorithm status update information packet
    */
    public class AlgorithmStatusPacket : Packet
    {
        /**
         * Current algorithm status
        */
        @JsonProperty( "eStatus")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AlgorithmStatus Status;

        /**
         * Chart we're subscribed to for live trading.
        */
        @JsonProperty( "sChartSubscription")]
        public String ChartSubscription;

        /**
         * Optional message or reason for state change.
        */
        @JsonProperty( "sMessage")]
        public String Message;

        /**
         * Algorithm Id associated with this status packet
        */
        @JsonProperty( "sAlgorithmID")]
        public String AlgorithmId;

        /**
         * Project Id associated with this status packet
        */
        @JsonProperty( "iProjectID")]
        public int ProjectId;

        /**
         * The current state of the channel
        */
        @JsonProperty( "sChannelStatus")]
        public String ChannelStatus;

        /**
         * Default constructor for JSON
        */
        public AlgorithmStatusPacket()
            : base(PacketType.AlgorithmStatus) {
        }

        /**
         * Initialize algorithm state packet:
        */
        public AlgorithmStatusPacket( String algorithmId, int projectId, AlgorithmStatus status, String message = "")
            : base (PacketType.AlgorithmStatus) {
            Status = status;
            ProjectId = projectId;
            AlgorithmId = algorithmId;
            Message = message;
        }   
    }
}
