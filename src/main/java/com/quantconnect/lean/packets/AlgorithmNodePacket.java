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

import com.quantconnect.lean.Global.Language;
import com.quantconnect.lean.Global.ServerType;
import com.quantconnect.lean.Global.UserPlan;

/// Algorithm Node Packet is a work task for the Lean Engine
public class AlgorithmNodePacket extends Packet {
    
    /// Default constructor for the algorithm node:
    /// <param name="type"></param>
    public AlgorithmNodePacket( PacketType type ) {
        super( type );
    }

    /// User Id placing request
//    @JsonProperty( "iUserID")]
    public int userId = 0;

    /// Project Id of the request
//    @JsonProperty( "iProjectID")]
    public int projectId = 0;

    /// Algorithm Id - BacktestId or DeployId - Common Id property between packets.
//    @JsonProperty( "sAlgorithmID")]
    public String algorithmId;
    
    
    public String getAlgorithmId() {
        if( type == PacketType.LiveNode ) {
            return ((LiveNodePacket)this).DeployId;
        }
        
        return ((BacktestNodePacket)this).backtestId;
    }

    /// User session Id for authentication
//    @JsonProperty( "sSessionID")]
    public String sessionId = "";

    /// User subscriptions state - free or paid.
//    @JsonProperty( "sUserPlan")]
    public UserPlan userPlan = UserPlan.Free;

    /// Language flag: Currently represents IL code or Dynamic Scripted Types.
//    @JsonProperty( "eLanguage")]
    public Language language = Language.CSharp;

    /// Server type for the deployment (512, 1024, 2048)
//    @JsonProperty( "sServerType")]
    public ServerType serverType = ServerType.Server512;

    /// Unique compile id of this backtest
//    @JsonProperty( "sCompileID")]
    public String compileId = "";

    /// Version number identifier for the lean engine.
//    @JsonProperty( "sVersion")]
    public String version;

    /// An algorithm packet which has already been run and is being redelivered on this node.
    /// In this event we don't want to relaunch the task as it may result in unexpected behaviour for user.
//    @JsonProperty( "bRedelivered")]
    public boolean redelivered = false;

    /// Algorithm binary with zip of contents
//    @JsonProperty( "oAlgorithm")]
    public byte[] algorithm = new byte[] { };

    /// Request source - Web IDE or API - for controling result handler behaviour
//    @JsonProperty( "sRequestSource")]
    public String requestSource = "WebIDE";

    /// The maximum amount of RAM (in MB) this algorithm is allowed to utilize
//    @JsonProperty( "iMaxRamAllocation")]
    public int ramAllocation;

    /// Specifies values to control algorithm limits
//    @JsonProperty( "oControls")]
    public Controls controls;

    /// The parameter values used to set algorithm parameters
//    @JsonProperty( "aParameters")]
    public Map<String,String> parameters = new HashMap<String,String>();

} // End Node Packet:


/*

using System.Collections.Generic;
using Newtonsoft.Json;

package com.quantconnect.lean.Packets
{
    /// <summary>
    /// Algorithm Node Packet is a work task for the Lean Engine
    /// </summary>
    public class AlgorithmNodePacket : Packet
    {
        /// <summary>
        /// Default constructor for the algorithm node:
        /// </summary>
        /// <param name="type"></param>
        public AlgorithmNodePacket(PacketType type)
            : base(type) { }

        /// <summary>
        /// User Id placing request
        /// </summary>
        @JsonProperty( "iUserID")]
        public int UserId = 0;

        /// <summary>
        /// Project Id of the request
        /// </summary>
        @JsonProperty( "iProjectID")]
        public int ProjectId = 0;

        /// <summary>
        /// Algorithm Id - BacktestId or DeployId - Common Id property between packets.
        /// </summary>
        @JsonProperty( "sAlgorithmID")]
        public String AlgorithmId
        {
            get
            {
                if( Type == PacketType.LiveNode) {
                    return ((LiveNodePacket)this).DeployId;
                }
                return ((BacktestNodePacket)this).BacktestId;
            }
        }

        /// <summary>
        /// User session Id for authentication
        /// </summary>
        @JsonProperty( "sSessionID")]
        public String SessionId = "";

        /// <summary>
        /// User subscriptions state - free or paid.
        /// </summary>
        @JsonProperty( "sUserPlan")]
        public UserPlan UserPlan = UserPlan.Free;

        /// <summary>
        /// Language flag: Currently represents IL code or Dynamic Scripted Types.
        /// </summary>
        @JsonProperty( "eLanguage")]
        public Language Language = Language.CSharp;

        /// <summary>
        /// Server type for the deployment (512, 1024, 2048)
        /// </summary>
        @JsonProperty( "sServerType")]
        public ServerType ServerType = ServerType.Server512;

        /// <summary>
        /// Unique compile id of this backtest
        /// </summary>
        @JsonProperty( "sCompileID")]
        public String CompileId = "";

        /// <summary>
        /// Version number identifier for the lean engine.
        /// </summary>
        @JsonProperty( "sVersion")]
        public String Version;

        /// <summary>
        /// An algorithm packet which has already been run and is being redelivered on this node.
        /// In this event we don't want to relaunch the task as it may result in unexpected behaviour for user.
        /// </summary>
        @JsonProperty( "bRedelivered")]
        public boolean Redelivered = false;

        /// <summary>
        /// Algorithm binary with zip of contents
        /// </summary>
        @JsonProperty( "oAlgorithm")]
        public byte[] Algorithm = new byte[] { };

        /// <summary>
        /// Request source - Web IDE or API - for controling result handler behaviour
        /// </summary>
        @JsonProperty( "sRequestSource")]
        public String RequestSource = "WebIDE";

        /// <summary>
        /// The maximum amount of RAM (in MB) this algorithm is allowed to utilize
        /// </summary>
        @JsonProperty( "iMaxRamAllocation")]
        public int RamAllocation;

        /// <summary>
        /// Specifies values to control algorithm limits
        /// </summary>
        @JsonProperty( "oControls")]
        public Controls Controls;

        /// <summary>
        /// The parameter values used to set algorithm parameters
        /// </summary>
        @JsonProperty( "aParameters")]
        public Map<String,String> Parameters = new Map<String,String>();
    } // End Node Packet:

} // End of Namespace:
*/