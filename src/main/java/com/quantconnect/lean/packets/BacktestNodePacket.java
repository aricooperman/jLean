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

package com.quantconnect.lean.packets;

import java.math.BigDecimal;
import java.time.LocalDate;

import com.quantconnect.lean.Global.Language;
import com.quantconnect.lean.Global.UserPlan;

/// Algorithm backtest task information packet.
public class BacktestNodePacket extends AlgorithmNodePacket {

    /// Name of the backtest as randomly defined in the IDE.
//    @JsonProperty( "sName")]
    public String name = "";

    /// BacktestId / Algorithm Id for this task
//    @JsonProperty( "sBacktestID")]
    public String backtestId = "";

    /// Backtest start-date as defined in the Initialize() method.
//    @JsonProperty( "dtPeriodStart")]
    public LocalDate periodStart = LocalDate.now();

    /// Backtest end date as defined in the Initialize() method.
//    @JsonProperty( "dtPeriodFinish")]
    public LocalDate periodFinish = LocalDate.now();

    /// Estimated number of trading days in this backtest task based on the start-end dates.
//    @JsonProperty( "iTradeableDates")]
    public int tradeableDates = 0;

    /// Series or parallel runmode for the backtest
    /// <obsolete>The RunMode property is now obsolete and will always default to Series mode.</obsolete>
//    [Obsolete( "This property is no longer in use and will always default to series mode.")]
//    @JsonProperty( "eRunMode")]
//    public RunMode RunMode = RunMode.Series;

    /// Default constructor for JSON
    public BacktestNodePacket() { 
        super( PacketType.BacktestNode );
        controls = new Controls( 500, 100, 30 );
    }

    /// Initialize the backtest task packet.
    public BacktestNodePacket( int userId, int projectId, String sessionId, byte[] algorithmData, BigDecimal startingCapital, String name ) {
        this( userId, projectId, sessionId, algorithmData, startingCapital, name, UserPlan.Free );
    }
    
    public BacktestNodePacket( int userId, int projectId, String sessionId, byte[] algorithmData, BigDecimal startingCapital, String name, UserPlan userPlan ) {
        this();
        
        this.userId = userId;
        this.algorithm = algorithmData;
        this.sessionId = sessionId;
        this.projectId = projectId;
        this.userPlan = userPlan;
        this.name = name;
        this.language = Language.CSharp;
    }
}

/*

using System;
using Newtonsoft.Json;

package com.quantconnect.lean.Packets
{
    /// <summary>
    /// Algorithm backtest task information packet.
    /// </summary>
    public class BacktestNodePacket : AlgorithmNodePacket
    {
        /// <summary>
        /// Name of the backtest as randomly defined in the IDE.
        /// </summary>
        @JsonProperty( "sName")]
        public String Name = "";

        /// <summary>
        /// BacktestId / Algorithm Id for this task
        /// </summary>
        @JsonProperty( "sBacktestID")]
        public String BacktestId = "";

        /// <summary>
        /// Backtest start-date as defined in the Initialize() method.
        /// </summary>
        @JsonProperty( "dtPeriodStart")]
        public DateTime PeriodStart = DateTime.Now;

        /// <summary>
        /// Backtest end date as defined in the Initialize() method.
        /// </summary>
        @JsonProperty( "dtPeriodFinish")]
        public DateTime PeriodFinish = DateTime.Now;

        /// <summary>
        /// Estimated number of trading days in this backtest task based on the start-end dates.
        /// </summary>
        @JsonProperty( "iTradeableDates")]
        public int TradeableDates = 0;

        /// <summary>
        /// Series or parallel runmode for the backtest
        /// </summary>
        /// <obsolete>The RunMode property is now obsolete and will always default to Series mode.</obsolete>
        [Obsolete( "This property is no longer in use and will always default to series mode.")]
        @JsonProperty( "eRunMode")]
        public RunMode RunMode = RunMode.Series;

        /// <summary>
        /// Default constructor for JSON
        /// </summary>
        public BacktestNodePacket() 
            : base(PacketType.BacktestNode) {
            Controls = new Controls
            {
                MinuteLimit = 500,
                SecondLimit = 100,
                TickLimit = 30
            };
        }

        /// <summary>
        /// Initialize the backtest task packet.
        /// </summary>
        public BacktestNodePacket(int userId, int projectId, String sessionId, byte[] algorithmData, BigDecimal startingCapital, String name, UserPlan userPlan = UserPlan.Free) 
            : base (PacketType.BacktestNode) {
            UserId = userId;
            Algorithm = algorithmData;
            SessionId = sessionId;
            ProjectId = projectId;
            UserPlan = userPlan;
            Name = name;
            Language = Language.CSharp;
            Controls = new Controls
            {
                MinuteLimit = 500,
                SecondLimit = 100,
                TickLimit = 30
            };
        }
    }
}
*/