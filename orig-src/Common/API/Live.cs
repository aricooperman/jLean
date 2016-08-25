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
using Newtonsoft.Json;
using QuantConnect.Api;

package com.quantconnect.lean.API
{
    /**
     * Live algorithm instance result from the QuantConnect Rest API.
    */
    public class Live : RestResponse
    {
        /**
         * Project id for the live instance
        */
        @JsonProperty( "projectId")]
        public String ProjectId;

        /**
         * Unique live algorithm deployment identifier (similar to a backtest id).
        */
        @JsonProperty( "deployId")]
        public String DeployId;

        /**
         * Algorithm status: running, stopped or runtime error.
        */
        @JsonProperty( "status")]
        public AlgorithmStatus Status;

        /**
         * Datetime the algorithm was launched in UTC.
        */
        @JsonProperty( "launched")]
        public DateTime Launched;

        /**
         * Datetime the algorithm was stopped in UTC, null if its still running.
        */
        @JsonProperty( "stopped")]
        public DateTime? Stopped;

        /**
         * Brokerage
        */
        @JsonProperty( "brokerage")]
        public String Brokerage;

        /**
         * Chart we're subscribed to
        */
         * 
         * Data limitations mean we can only stream one chart at a time to the consumer. See which chart you're watching here.
         * 
        @JsonProperty( "subscription")]
        public String Subscription;

        /**
         * Live algorithm error message from a crash or algorithm runtime error.
        */
        @JsonProperty( "error")]
        public String Error;
    }

    /**
     * List of the live algorithms running which match the requested status
    */
    public class LiveList : RestResponse
    {
        /**
         * Algorithm list matching the requested status.
        */
        @JsonProperty( "live")]
        public List<Live> Algorithms;
    }
}
