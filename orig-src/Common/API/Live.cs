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
*/
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using QuantConnect.Api;

package com.quantconnect.lean.API
{
    /// <summary>
    /// Live algorithm instance result from the QuantConnect Rest API.
    /// </summary>
    public class Live : RestResponse
    {
        /// <summary>
        /// Project id for the live instance
        /// </summary>
        @JsonProperty( "projectId")]
        public String ProjectId;

        /// <summary>
        /// Unique live algorithm deployment identifier (similar to a backtest id).
        /// </summary>
        @JsonProperty( "deployId")]
        public String DeployId;

        /// <summary>
        /// Algorithm status: running, stopped or runtime error.
        /// </summary>
        @JsonProperty( "status")]
        public AlgorithmStatus Status;

        /// <summary>
        /// Datetime the algorithm was launched in UTC.
        /// </summary>
        @JsonProperty( "launched")]
        public DateTime Launched;

        /// <summary>
        /// Datetime the algorithm was stopped in UTC, null if its still running.
        /// </summary>
        @JsonProperty( "stopped")]
        public DateTime? Stopped;

        /// <summary>
        /// Brokerage
        /// </summary>
        @JsonProperty( "brokerage")]
        public String Brokerage;

        /// <summary>
        /// Chart we're subscribed to
        /// </summary>
        /// <remarks>
        /// Data limitations mean we can only stream one chart at a time to the consumer. See which chart you're watching here.
        /// </remarks>
        @JsonProperty( "subscription")]
        public String Subscription;

        /// <summary>
        /// Live algorithm error message from a crash or algorithm runtime error.
        /// </summary>
        @JsonProperty( "error")]
        public String Error;
    }

    /// <summary>
    /// List of the live algorithms running which match the requested status
    /// </summary>
    public class LiveList : RestResponse
    {
        /// <summary>
        /// Algorithm list matching the requested status.
        /// </summary>
        @JsonProperty( "live")]
        public List<Live> Algorithms;
    }
}
