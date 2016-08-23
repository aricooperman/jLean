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

package com.quantconnect.lean.interfaces;

import java.io.Closeable;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;

import com.quantconnect.lean.Global.AlgorithmControl;
import com.quantconnect.lean.Global.AlgorithmStatus;
import com.quantconnect.lean.Global.Language;
import com.quantconnect.lean.Global.StoragePermissions;
import com.quantconnect.lean.api.Compile;
import com.quantconnect.lean.api.Project;
import com.quantconnect.lean.api.ProjectFile;
import com.quantconnect.lean.api.ProjectList;
import com.quantconnect.lean.api.RestResponse;

//using System;
//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using QuantConnect.Api;
//using QuantConnect.API;
//using QuantConnect.Securities;

    /// API for QuantConnect.com 
//    [InheritedExport(typeof(IApi))]
public interface IApi extends Closeable {
    
    /// Initialize the control system
    void initialize( int userId, String token );

    /// Create a project with the specified name and language via QuantConnect.com API
     * @param name">Project name
     * @param language">Programming language to use
    @returns Project object from the API.
    Project createProject( String name, Language language );

    /**
    /// Read in a project from the QuantConnect.com API.
    */
     * @param projectId">Project id you own
    @returns 
    Project readProject( int projectId );

    /**
    /// Update a specific project with a list of files. All other files will be deleted.
    */
     * @param projectId">Project id for project to be updated
     * @param files">Files list to update
    @returns RestResponse indicating success
    RestResponse updateProject( int projectId, List<ProjectFile> files );

    /**
    /// Delete a specific project owned by the user from QuantConnect.com
    */
     * @param projectId">Project id we own and wish to delete
    @returns RestResponse indicating success
    RestResponse delete( int projectId );

    /// Read back a list of all projects on the account for a user.
    @returns Container for list of projects
    ProjectList projectList();

    /// Create a new compile job request for this project id.
     * @param projectId">Project id we wish to compile.
    @returns Compile object result
    Compile createCompile( int projectId );

    /// Read a compile packet job result.
     * @param projectId">Project id we sent for compile
     * @param compileId">Compile id return from the creation request
    @returns Compile object result
    Compile readCompile( int projectId, String compileId );

    /// Create a new backtest from a specified projectId and compileId
     * @param projectId">
     * @param compileId">
     * @param backtestName">
    @returns 
    Backtest createBacktest( int projectId, String compileId, String backtestName );

    /**
    /// Read out the full result of a specific backtest
    */
     * @param projectId">Project id for the backtest we'd like to read
     * @param backtestId">Backtest id for the backtest we'd like to read
    @returns Backtest result object
    Backtest readBacktest(int projectId, String backtestId);

    /**
    /// Update the backtest name
    */
     * @param projectId">Project id to update
     * @param backtestId">Backtest id to update
     * @param backtestName">New backtest name to set
     * @param backtestNote">Note attached to the backtest
    @returns Rest response on success
    RestResponse updateBacktest(int projectId, String backtestId, String backtestName = "", String backtestNote = "");

    /**
    /// Delete a backtest from the specified project and backtestId.
    */
     * @param projectId">Project for the backtest we want to delete
     * @param backtestId">Backtest id we want to delete
    @returns RestResponse on success
    RestResponse deleteBacktest(int projectId, String backtestId);

    /**
    /// Get a list of backtests for a specific project id
    */
     * @param projectId">Project id to search
    @returns BacktestList container for list of backtests
    BacktestList backtestList(int projectId);

    /**
    /// Get a list of live running algorithms for a logged in user.
    */
    @returns List of live algorithm instances
    LiveList liveList();
    



    //Status StatusRead(int projectId, String algorithmId);
    //RestResponse StatusUpdate(int projectId, String algorithmId, AlgorithmStatus status, String message = "");
    //LogControl LogAllowanceRead();
    //void LogAllowanceUpdate( String backtestId, String url, int length);
    //void StatisticsUpdate(int projectId, String algorithmId, BigDecimal unrealized, BigDecimal fees, BigDecimal netProfit, BigDecimal holdings, BigDecimal equity, BigDecimal netReturn, BigDecimal volume, int trades, double sharpe);
    //void NotifyOwner(int projectId, String algorithmId, String subject, String body);
    //IEnumerable<MarketHoursSegment> MarketHours(int projectId, DateTime time, Symbol symbol);



    /**
    /// Read the maximum log allowance
    */
    int[] readLogAllowance( int userId, String userToken );

    /// Update running total of log usage
    default void updateDailyLogUsed( int userId, String backtestId, String url, int length, String userToken ) {
        updateDailyLogUsed( userId, backtestId, url, length, userToken, false );
    }
    
    void updateDailyLogUsed( int userId, String backtestId, String url, int length, String userToken, boolean hitLimit );

    /**
    /// Get the algorithm current status, active or cancelled from the user
    */
     * @param algorithmId">
     * @param userId">The user id of the algorithm
    @returns 
    AlgorithmControl getAlgorithmStatus( String algorithmId, int userId );

    /**
    /// Set the algorithm status from the worker to update the UX e.g. if there was an error.
    */
     * @param algorithmId">Algorithm id we're setting.
     * @param status">Status enum of the current worker
     * @param message">Message for the algorithm status event
    void setAlgorithmStatus( String algorithmId, AlgorithmStatus status, String message = "" );

    /**
    /// Send the statistics to storage for performance tracking.
    */
     * @param algorithmId">Identifier for algorithm
     * @param unrealized">Unrealized gainloss
     * @param fees">Total fees
     * @param netProfit">Net profi
     * @param holdings">Algorithm holdings
     * @param equity">Total equity
     * @param netReturn">Algorithm return
     * @param volume">Volume traded
     * @param trades">Total trades since inception
     * @param sharpe">Sharpe ratio since inception
    void sendStatistics( String algorithmId, BigDecimal unrealized, BigDecimal fees, BigDecimal netProfit, BigDecimal holdings, BigDecimal equity, BigDecimal netReturn, BigDecimal volume, int trades, double sharpe);

    /**
    /// Market Status Today: REST call.
    */
     * @param time">The date we need market hours for
     * @param symbol">
    @returns Market open hours.
    Iterable<MarketHoursSegment> marketToday( LocalDateTime time, Symbol symbol);

    default void store( String data, String location, StoragePermissions permissions ) {
        store( data, location, permissions, false );
    }
    
    /// Store the algorithm logs.
    void store( String data, String location, StoragePermissions permissions, boolean async );

    /// Send an email to the user associated with the specified algorithm id
     * @param algorithmId">The algorithm id
     * @param subject">The email subject
     * @param body">The email message body
    void sendUserEmail( String algorithmId, String subject, String body );
}

/*
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using QuantConnect.Api;
using QuantConnect.API;
using QuantConnect.Securities;

package com.quantconnect.lean.Interfaces
{
    /**
    /// API for QuantConnect.com 
    */
    [InheritedExport(typeof(IApi))]
    public interface IApi : IDisposable
    {
        /**
        /// Initialize the control system
        */
        void Initialize(int userId, String token);

        /**
        /// Create a project with the specified name and language via QuantConnect.com API
        */
         * @param name">Project name
         * @param language">Programming language to use
        @returns Project object from the API.
        Project CreateProject( String name, Language language);

        /**
        /// Read in a project from the QuantConnect.com API.
        */
         * @param projectId">Project id you own
        @returns 
        Project ReadProject(int projectId);

        /**
        /// Update a specific project with a list of files. All other files will be deleted.
        */
         * @param projectId">Project id for project to be updated
         * @param files">Files list to update
        @returns RestResponse indicating success
        RestResponse UpdateProject(int projectId, List<ProjectFile> files);

        /**
        /// Delete a specific project owned by the user from QuantConnect.com
        */
         * @param projectId">Project id we own and wish to delete
        @returns RestResponse indicating success
        RestResponse Delete(int projectId);

        /**
        /// Read back a list of all projects on the account for a user.
        */
        @returns Container for list of projects
        ProjectList ProjectList();

        /**
        /// Create a new compile job request for this project id.
        */
         * @param projectId">Project id we wish to compile.
        @returns Compile object result
        Compile CreateCompile(int projectId);

        /**
        /// Read a compile packet job result.
        */
         * @param projectId">Project id we sent for compile
         * @param compileId">Compile id return from the creation request
        @returns Compile object result
        Compile ReadCompile(int projectId, String compileId);

        /**
        /// Create a new backtest from a specified projectId and compileId
        */
         * @param projectId">
         * @param compileId">
         * @param backtestName">
        @returns 
        Backtest CreateBacktest(int projectId, String compileId, String backtestName);

        /**
        /// Read out the full result of a specific backtest
        */
         * @param projectId">Project id for the backtest we'd like to read
         * @param backtestId">Backtest id for the backtest we'd like to read
        @returns Backtest result object
        Backtest ReadBacktest(int projectId, String backtestId);

        /**
        /// Update the backtest name
        */
         * @param projectId">Project id to update
         * @param backtestId">Backtest id to update
         * @param backtestName">New backtest name to set
         * @param backtestNote">Note attached to the backtest
        @returns Rest response on success
        RestResponse UpdateBacktest(int projectId, String backtestId, String backtestName = "", String backtestNote = "");

        /**
        /// Delete a backtest from the specified project and backtestId.
        */
         * @param projectId">Project for the backtest we want to delete
         * @param backtestId">Backtest id we want to delete
        @returns RestResponse on success
        RestResponse DeleteBacktest(int projectId, String backtestId);

        /**
        /// Get a list of backtests for a specific project id
        */
         * @param projectId">Project id to search
        @returns BacktestList container for list of backtests
        BacktestList BacktestList(int projectId);

        /**
        /// Get a list of live running algorithms for a logged in user.
        */
        @returns List of live algorithm instances
        LiveList LiveList();
        



        //Status StatusRead(int projectId, String algorithmId);
        //RestResponse StatusUpdate(int projectId, String algorithmId, AlgorithmStatus status, String message = "");
        //LogControl LogAllowanceRead();
        //void LogAllowanceUpdate( String backtestId, String url, int length);
        //void StatisticsUpdate(int projectId, String algorithmId, BigDecimal unrealized, BigDecimal fees, BigDecimal netProfit, BigDecimal holdings, BigDecimal equity, BigDecimal netReturn, BigDecimal volume, int trades, double sharpe);
        //void NotifyOwner(int projectId, String algorithmId, String subject, String body);
        //IEnumerable<MarketHoursSegment> MarketHours(int projectId, DateTime time, Symbol symbol);



        /**
        /// Read the maximum log allowance
        */
        int[] ReadLogAllowance(int userId, String userToken);

        /**
        /// Update running total of log usage
        */
        void UpdateDailyLogUsed(int userId, String backtestId, String url, int length, String userToken, boolean hitLimit = false);

        /**
        /// Get the algorithm current status, active or cancelled from the user
        */
         * @param algorithmId">
         * @param userId">The user id of the algorithm
        @returns 
        AlgorithmControl GetAlgorithmStatus( String algorithmId, int userId);

        /**
        /// Set the algorithm status from the worker to update the UX e.g. if there was an error.
        */
         * @param algorithmId">Algorithm id we're setting.
         * @param status">Status enum of the current worker
         * @param message">Message for the algorithm status event
        void SetAlgorithmStatus( String algorithmId, AlgorithmStatus status, String message = "");

        /**
        /// Send the statistics to storage for performance tracking.
        */
         * @param algorithmId">Identifier for algorithm
         * @param unrealized">Unrealized gainloss
         * @param fees">Total fees
         * @param netProfit">Net profi
         * @param holdings">Algorithm holdings
         * @param equity">Total equity
         * @param netReturn">Algorithm return
         * @param volume">Volume traded
         * @param trades">Total trades since inception
         * @param sharpe">Sharpe ratio since inception
        void SendStatistics( String algorithmId, BigDecimal unrealized, BigDecimal fees, BigDecimal netProfit, BigDecimal holdings, BigDecimal equity, BigDecimal netReturn, BigDecimal volume, int trades, double sharpe);

        /**
        /// Market Status Today: REST call.
        */
         * @param time">The date we need market hours for
         * @param symbol">
        @returns Market open hours.
        IEnumerable<MarketHoursSegment> MarketToday(DateTime time, Symbol symbol);

        /**
        /// Store the algorithm logs.
        */
        void Store( String data, String location, StoragePermissions permissions, boolean async = false);

        /**
        /// Send an email to the user associated with the specified algorithm id
        */
         * @param algorithmId">The algorithm id
         * @param subject">The email subject
         * @param body">The email message body
        void SendUserEmail( String algorithmId, String subject, String body);
    }
}
*/