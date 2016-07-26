package com.quantconnect.lean.api;

import java.util.List;

/**
 *  Collection container for a list of backtests for a project
 */
public class BacktestList extends RestResponse {

    /// Collection of summarized backtest objects
//    @JsonProperty( "backtests")]
    public List<Backtest> backtests; 
}