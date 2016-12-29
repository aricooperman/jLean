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

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using QuantConnect.Logging;
using QuantConnect.Orders;

package com.quantconnect.lean.Packets
{
    /**
     * Live result packet from a lean engine algorithm.
    */
    public class LiveResultPacket : Packet 
    {
        /**
         * User Id sending result packet
        */
        @JsonProperty( "iUserID")]
        public int UserId = 0;

        /**
         * Project Id of the result packet
        */
        @JsonProperty( "iProjectID")]
        public int ProjectId = 0;

        /**
         * User session Id who issued the result packet
        */
        @JsonProperty( "sSessionID")]
        public String SessionId = "";

        /**
         * Live Algorithm Id (DeployId) for this result packet
        */
        @JsonProperty( "sDeployID")]
        public String DeployId = "";

        /**
         * Compile Id algorithm which generated this result packet
        */
        @JsonProperty( "sCompileID")]
        public String CompileId = "";

        /**
         * Result data object for this result packet
        */
        @JsonProperty( "oResults")]
        public LiveResult Results = new LiveResult();

        /**
         * Processing time / running time for the live algorithm.
        */
        @JsonProperty( "dProcessingTime")]
        public double ProcessingTime = 0;

        /**
         * Default constructor for JSON Serialization
        */
        public LiveResultPacket()
            : base(PacketType.LiveResult) { }
        
        /**
         * Compose the packet from a JSON string:
        */
        public LiveResultPacket( String json)
            : base(PacketType.LiveResult) {
            try
            {
                packet = JsonConvert.DeserializeObject<LiveResultPacket>(json);
                CompileId          = packet.CompileId;
                Channel            = packet.Channel;
                SessionId          = packet.SessionId;
                DeployId           = packet.DeployId;
                Class               = packet.Type;
                UserId             = packet.UserId;
                ProjectId          = packet.ProjectId;
                Results            = packet.Results;
                ProcessingTime     = packet.ProcessingTime;
            } 
            catch (Exception err) {
                Log.Trace( "LiveResultPacket(): Error converting json: " + err);
            }
        }

        /**
         * Compose Live Result Data Packet - With tradable dates
        */
         * @param job Job that started this request
         * @param results Results class for the Backtest job
        public LiveResultPacket(LiveNodePacket job, LiveResult results) 
            :base (PacketType.LiveResult) {
            try
            {
                SessionId = job.SessionId;
                CompileId = job.CompileId;
                DeployId = job.DeployId;
                Results = results;
                UserId = job.UserId;
                ProjectId = job.ProjectId;
                SessionId = job.SessionId;
                Channel = job.Channel;
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }
    } // End Queue Packet:


    /**
     * Live results object class for packaging live result data.
    */
    public class LiveResult
    {
        /**
         * Charts updates for the live algorithm since the last result packet
        */
        public Map<String, Chart> Charts = new Map<String, Chart>();

        /**
         * Holdings dictionary of algorithm holdings information
        */
        public Map<String, Holding> Holdings = new Map<String, Holding>();
        
        /**
         * Order updates since the last result packet
        */
        public Map<Integer, Order> Orders = new Map<Integer, Order>();
        
        /**
         * Trade profit and loss information since the last algorithm result packet
        */
        public Map<DateTime, decimal> ProfitLoss = new Map<DateTime, decimal>();

        /**
         * Statistics information sent during the algorithm operations.
        */
         * Intended for update mode -- send updates to the existing statistics in the result GUI. If statistic key does not exist in GUI, create it
        public Map<String,String> Statistics = new Map<String,String>();

        /**
         * Runtime banner/updating statistics in the title banner of the live algorithm GUI.
        */
        public Map<String,String> RuntimeStatistics = new Map<String,String>();

        /**
         * Server status information, including CPU/RAM usage, ect...
        */
        public Map<String,String> ServerStatistics = new Map<String,String>();

        /**
         * Default Constructor
        */
        public LiveResult() { }

        /**
         * Constructor for the result class for dictionary objects
        */
        public LiveResult(Map<String, Chart> charts, Map<Integer, Order> orders, Map<DateTime, decimal> profitLoss, Map<String, Holding> holdings, Map<String,String> statistics, Map<String,String> runtime, Map<String,String> serverStatistics = null ) {
            Charts = charts;
            Orders = orders;
            ProfitLoss = profitLoss;
            Statistics = statistics;
            Holdings = holdings;
            RuntimeStatistics = runtime;
            ServerStatistics = serverStatistics ?? OS.GetServerStatistics();
        } 
    }

} // End of Namespace:
