﻿/*
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

package com.quantconnect.lean.packets;

import java.util.HashMap;
import java.util.Map;

//using System.Collections.Generic;
//using Newtonsoft.Json;

/// Live job task packet: container for any live specific job variables
public class LiveNodePacket extends AlgorithmNodePacket {

    /// Deploy Id for this live algorithm.
//    @JsonProperty( "sDeployID")]
    public String DeployId = "";

    /// String name of the brokerage we're trading with
//    @JsonProperty( "sBrokerage")]
    public String Brokerage = "";

    /// String-String Dictionary of Brokerage Data for this Live Job
//    @JsonProperty( "aBrokerageData")]
    public Map<String,String> BrokerageData = new HashMap<String,String>();

    /// <summary>
    /// Default constructor for JSON of the Live Task Packet
    /// </summary>
    public LiveNodePacket() {
        super( PacketType.LiveNode );
        controls = new Controls( 50, 25, 15 );
    }

} // End Work Packet:


/*
using System.Collections.Generic;
using Newtonsoft.Json;

package com.quantconnect.lean.Packets
{
    /// <summary>
    /// Live job task packet: container for any live specific job variables
    /// </summary>
    public class LiveNodePacket : AlgorithmNodePacket 
    {
        /// <summary>
        /// Deploy Id for this live algorithm.
        /// </summary>
        @JsonProperty( "sDeployID")]
        public String DeployId = "";

        /// <summary>
        /// String name of the brokerage we're trading with
        /// </summary>
        @JsonProperty( "sBrokerage")]
        public String Brokerage = "";

        /// <summary>
        /// String-String Dictionary of Brokerage Data for this Live Job
        /// </summary>
        @JsonProperty( "aBrokerageData")]
        public Map<String,String> BrokerageData = new Map<String,String>();

        /// <summary>
        /// Default constructor for JSON of the Live Task Packet
        /// </summary>
        public LiveNodePacket() 
            : base(PacketType.LiveNode) {
            Controls = new Controls
            {
                MinuteLimit = 50,
                SecondLimit = 25,
                TickLimit = 15
            };
        }

    } // End Work Packet:

} // End of Namespace:
*/