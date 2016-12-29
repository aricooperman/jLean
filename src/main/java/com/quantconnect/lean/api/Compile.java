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

import java.util.List;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * Response from the compiler on a build event
 */
public class Compile extends RestResponse {

    /**
     * Compile Id for a sucessful build
     */
    @JsonProperty( "compileId" )
    public String compileId;

    /**
     * True on successful compile
     */
    @JsonProperty( "state" )
//    [JsonConverter(typeof(StringEnumConverter))]
    public CompileState state;

    /**
     * Logs of the compilation request
     */
    @JsonProperty( "logs" )
    public List<String> logs;
}