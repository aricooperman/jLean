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

import java.math.BigDecimal;

import com.fasterxml.jackson.annotation.JsonProperty;
import com.quantconnect.lean.api.RestResponse;
import com.quantconnect.lean.packets.BacktestResult;

/**
 * Backtest response packet from the QuantConnect.com API.
 */
public class Backtest extends RestResponse {

    /**
     * Name of the backtest
     */
    @JsonProperty( "name" )
    public String name;

    /**
     * Note on the backtest attached by the user
     */
    @JsonProperty( "note" )
    public String note;

    /**
     * Assigned backtest Id
     */
    @JsonProperty( "backtestId" )
    public String backtestId;

    /**
     * Boolean true when the backtest is completed.
     */
    @JsonProperty( "completed" )
    public boolean completed;

    /**
     * Progress of the backtest in percent 0-1.
     */
    @JsonProperty( "progress" )
    public BigDecimal progress;
    
    /**
     * Result packet for the backtest
     */
    @JsonProperty( "result" )
    public BacktestResult result;
}
