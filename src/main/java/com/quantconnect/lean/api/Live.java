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

package com.quantconnect.lean.api;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

import com.quantconnect.lean.AlgorithmStatus;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * Live algorithm instance result from the QuantConnect Rest API.
 */
public class Live extends RestResponse {
 
    /**
     * Project id for the live instance
     */
    @JsonProperty( "projectId" )
    public String projectId;

    /**
     * Unique live algorithm deployment identifier (similar to a backtest id).
     */
    @JsonProperty( "deployId" )
    public String deployId;

    /**
     * Algorithm status: running, stopped or runtime error.
     */
    @JsonProperty( "status" )
    public AlgorithmStatus status;

    /**
     * Date time the algorithm was launched in UTC.
     */
    @JsonProperty( "launched" )
    public LocalDateTime launched;

    /**
     * Date time the algorithm was stopped in UTC, null if its still running.
     */
    @JsonProperty( "stopped" )
    public Optional<LocalDateTime> stopped;

    /**
     * Brokerage
     */
    @JsonProperty( "brokerage" )
    public String brokerage;

    /**
     * Chart we're subscribed to
     * Data limitations mean we can only stream one chart at a time to the consumer. See which chart you're watching here.
     */
    @JsonProperty( "subscription" )
    public String subscription;

    /**
     * Live algorithm error message from a crash or algorithm runtime error.
     */
    @JsonProperty( "error" )
    public String error;

    
    /**
     * List of the live algorithms running which match the requested status
     */
    public static class LiveList extends RestResponse {
        
        /**
         * Algorithm list matching the requested status.
         */
        @JsonProperty( "live" )
        public List<Live> Algorithms;
    }
}
