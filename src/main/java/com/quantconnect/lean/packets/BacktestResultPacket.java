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
import java.time.ZoneId;
import java.time.ZonedDateTime;
import java.util.Dictionary;
import java.util.HashMap;
import java.util.Map;

import org.apache.commons.math3.util.Precision;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.fasterxml.jackson.annotation.JsonProperty;
import com.quantconnect.lean.Global;
import com.quantconnect.lean.charting.Chart;

/// Backtest result packet: send backtest information to GUI for user consumption.
public class BacktestResultPacket extends Packet {
    
    private final Logger log = LoggerFactory.getLogger( getClass() );

    /// User Id placing this task
    @JsonProperty( "iUserID" )
    public int userId = 0;

    /// Project Id of the this task.
    @JsonProperty( "iProjectID" )
    public int projectId = 0;

    /// User Session Id
    @JsonProperty( "sSessionID" )
    public String sessionId = "";

    /// BacktestId for this result packet
    @JsonProperty( "sBacktestID" )
    public String backtestId = "";

    /// Compile Id for the algorithm which generated this result packet.
    @JsonProperty( "sCompileID" )
    public String compileId = "";

    /// Start of the backtest period as defined in Initialize() method.
    @JsonProperty( "dtPeriodStart" )
    public LocalDate periodStart = LocalDate.now();

    /// End of the backtest period as defined in the Initialize() method.
    @JsonProperty( "dtPeriodFinish" )
    public LocalDate periodFinish = LocalDate.now();

    /// DateTime (EST) the user requested this backtest.
    @JsonProperty( "dtDateRequested" )
    public ZonedDateTime dateRequested = ZonedDateTime.now( ZoneId.of( "America/New_York" ) );

    /// DateTime (EST) when the backtest was completed.
    @JsonProperty( "dtDateFinished" )
    public ZonedDateTime dateFinished = ZonedDateTime.now( ZoneId.of( "America/New_York" ) );

    /// Progress of the backtest as a percentage from 0-1 based on the days lapsed from start-finish.
    @JsonProperty( "dProgress" )
    public double progress = 0.0;

    /// Name of this backtest.
    @JsonProperty( "sName" )
    public String name = "";

    /// Result data object for this backtest
    @JsonProperty( "oResults" )
    public BacktestResult results = new BacktestResult();

    /// Processing time of the algorithm (from moment the algorithm arrived on the algorithm node)
    @JsonProperty( "dProcessingTime" )
    public double processingTime = 0.0;

    /// Estimated number of tradeable days in the backtest based on the start and end date or the backtest
    @JsonProperty( "iTradeableDates" )
    public int tradeableDates = 0;

    /// Default constructor for JSON Serialization
    public BacktestResultPacket() {
        super( PacketType.BacktestResult );
    }
    
    /// Compose the packet from a JSON string:
    public BacktestResultPacket( String json ) { 
        this();
        
        try {
            final BacktestResultPacket packet = Global.OBJECT_MAPPER.readValue( json, BacktestResultPacket.class );
            this.compileId          = packet.compileId;
            this.channel            = packet.channel;
            this.periodFinish       = packet.periodFinish;
            this.periodStart        = packet.periodStart;
            this.progress           = packet.progress;
            this.sessionId          = packet.sessionId;
            this.backtestId         = packet.backtestId;
            this.type               = packet.type;
            this.userId             = packet.userId;
            this.dateFinished       = packet.dateFinished;
            this.dateRequested      = packet.dateRequested;
            this.name               = packet.name;
            this.projectId          = packet.projectId;
            this.results            = packet.results;
            this.processingTime     = packet.processingTime;
            this.tradeableDates     = packet.tradeableDates;
        } 
        catch( Exception err ) {
            log.trace( "BacktestResultPacket(): Error converting json", err );
        }
    }


    /// Compose result data packet - with tradable dates from the backtest job task and the partial result packet.
     * @param job">Job that started this request
     * @param results">Results class for the Backtest job
     * @param progress">Progress of the packet. For the packet we assume progess of 100%.
    public BacktestResultPacket( BacktestNodePacket job, BacktestResult results ) {
        this( job, results, 1.0D );
    }
    
    
    public BacktestResultPacket( BacktestNodePacket job, BacktestResult results, double progress ) {
        this();
        
        try {
            this.progress = Precision.round( progress, 3 );
            this.sessionId = job.sessionId;
            this.periodFinish = job.periodFinish;
            this.periodStart = job.periodStart;
            this.compileId = job.compileId;
            this.channel = job.channel;
            this.backtestId = job.backtestId;
            this.results = results;
            this.name = job.name;
            this.userId = job.userId;
            this.projectId = job.projectId;
            this.sessionId = job.sessionId;
            this.tradeableDates = job.tradeableDates;
        }
        catch( Exception err ) {
            log.error( "BacktestResultPacket(): Error constructing", err );
        }
    }
}


/*
 *  Backtest results object class - result specific items from the packet.
 */
public class BacktestResult {

    /// Chart updates in this backtest since the last backtest result packet was sent.
    public Map<String,Chart> Charts = new HashMap<String,Chart>();
    
    /// Order updates since the last backtest result packet was sent.
    public Map<Integer, Order> Orders = new Map<Integer, Order>();
    
    /// Profit and loss results from closed trades.
    public Map<DateTime,BigDecimal> ProfitLoss = new HashMap<DateTime,BigDecimal>();

    /// Statistics information for the backtest.
    /// The statistics are only generated on the last result packet of the backtest.
    public Map<String,String> Statistics = new HashMap<String,String>();

    /// The runtime / dynamic statistics generated while a backtest is running.
    public Map<String,String> RuntimeStatistics = new HashMap<String,String>();

    /// Rolling window detailed statistics.
    public Map<String,AlgorithmPerformance> RollingWindow = new HashMap<String,AlgorithmPerformance>();

    /// Rolling window detailed statistics.
    public AlgorithmPerformance TotalPerformance = null;

    /// Default Constructor
    public BacktestResult() { }

    public BacktestResult( Map<String,Chart> charts, Int2ObjectMap<Order> orders, Map<DateTime,BigDecimal> profitLoss, Map<String,String> statistics, 
            Map<String,String> runtimeStatistics, Map<String,AlgorithmPerformance> rollingWindow ) {
        this( charts, orders, profitLoss, statistics, runtimeStatistics, rollingWindow, null );
    }
    
    /// Constructor for the result class using dictionary objects.
    public BacktestResult( Map<String,Chart> charts, Int2ObjectMap<Order> orders, map<DateTime,BigDecimal> profitLoss, Map<String,String> statistics, 
            Map<String,String> runtimeStatistics, Map<String,AlgorithmPerformance> rollingWindow, AlgorithmPerformance totalPerformance ) {
        Charts = charts;
        Orders = orders;
        ProfitLoss = profitLoss;
        Statistics = statistics;
        RuntimeStatistics = runtimeStatistics;
        RollingWindow = rollingWindow;
        TotalPerformance = totalPerformance;
    }
}
