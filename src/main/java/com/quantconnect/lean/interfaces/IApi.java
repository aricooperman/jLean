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
    /// <param name="name">Project name</param>
    /// <param name="language">Programming language to use</param>
    /// <returns>Project object from the API.</returns>
    Project createProject( String name, Language language );

    /// <summary>
    /// Read in a project from the QuantConnect.com API.
    /// </summary>
    /// <param name="projectId">Project id you own</param>
    /// <returns></returns>
    Project readProject( int projectId );

    /// <summary>
    /// Update a specific project with a list of files. All other files will be deleted.
    /// </summary>
    /// <param name="projectId">Project id for project to be updated</param>
    /// <param name="files">Files list to update</param>
    /// <returns>RestResponse indicating success</returns>
    RestResponse updateProject( int projectId, List<ProjectFile> files );

    /// <summary>
    /// Delete a specific project owned by the user from QuantConnect.com
    /// </summary>
    /// <param name="projectId">Project id we own and wish to delete</param>
    /// <returns>RestResponse indicating success</returns>
    RestResponse delete( int projectId );

    /// Read back a list of all projects on the account for a user.
    /// <returns>Container for list of projects</returns>
    ProjectList projectList();

    /// Create a new compile job request for this project id.
    /// <param name="projectId">Project id we wish to compile.</param>
    /// <returns>Compile object result</returns>
    Compile createCompile( int projectId );

    /// Read a compile packet job result.
    /// <param name="projectId">Project id we sent for compile</param>
    /// <param name="compileId">Compile id return from the creation request</param>
    /// <returns>Compile object result</returns>
    Compile readCompile( int projectId, String compileId );

    /// Create a new backtest from a specified projectId and compileId
    /// <param name="projectId"></param>
    /// <param name="compileId"></param>
    /// <param name="backtestName"></param>
    /// <returns></returns>
    Backtest createBacktest( int projectId, String compileId, String backtestName );

    /// <summary>
    /// Read out the full result of a specific backtest
    /// </summary>
    /// <param name="projectId">Project id for the backtest we'd like to read</param>
    /// <param name="backtestId">Backtest id for the backtest we'd like to read</param>
    /// <returns>Backtest result object</returns>
    Backtest readBacktest(int projectId, String backtestId);

    /// <summary>
    /// Update the backtest name
    /// </summary>
    /// <param name="projectId">Project id to update</param>
    /// <param name="backtestId">Backtest id to update</param>
    /// <param name="backtestName">New backtest name to set</param>
    /// <param name="backtestNote">Note attached to the backtest</param>
    /// <returns>Rest response on success</returns>
    RestResponse updateBacktest(int projectId, String backtestId, String backtestName = "", String backtestNote = "");

    /// <summary>
    /// Delete a backtest from the specified project and backtestId.
    /// </summary>
    /// <param name="projectId">Project for the backtest we want to delete</param>
    /// <param name="backtestId">Backtest id we want to delete</param>
    /// <returns>RestResponse on success</returns>
    RestResponse deleteBacktest(int projectId, String backtestId);

    /// <summary>
    /// Get a list of backtests for a specific project id
    /// </summary>
    /// <param name="projectId">Project id to search</param>
    /// <returns>BacktestList container for list of backtests</returns>
    BacktestList backtestList(int projectId);

    /// <summary>
    /// Get a list of live running algorithms for a logged in user.
    /// </summary>
    /// <returns>List of live algorithm instances</returns>
    LiveList liveList();
    



    //Status StatusRead(int projectId, String algorithmId);
    //RestResponse StatusUpdate(int projectId, String algorithmId, AlgorithmStatus status, String message = "");
    //LogControl LogAllowanceRead();
    //void LogAllowanceUpdate( String backtestId, String url, int length);
    //void StatisticsUpdate(int projectId, String algorithmId, BigDecimal unrealized, BigDecimal fees, BigDecimal netProfit, BigDecimal holdings, BigDecimal equity, BigDecimal netReturn, BigDecimal volume, int trades, double sharpe);
    //void NotifyOwner(int projectId, String algorithmId, String subject, String body);
    //IEnumerable<MarketHoursSegment> MarketHours(int projectId, DateTime time, Symbol symbol);



    /// <summary>
    /// Read the maximum log allowance
    /// </summary>
    int[] readLogAllowance( int userId, String userToken );

    /// Update running total of log usage
    default void updateDailyLogUsed( int userId, String backtestId, String url, int length, String userToken ) {
        updateDailyLogUsed( userId, backtestId, url, length, userToken, false );
    }
    
    void updateDailyLogUsed( int userId, String backtestId, String url, int length, String userToken, boolean hitLimit );

    /// <summary>
    /// Get the algorithm current status, active or cancelled from the user
    /// </summary>
    /// <param name="algorithmId"></param>
    /// <param name="userId">The user id of the algorithm</param>
    /// <returns></returns>
    AlgorithmControl getAlgorithmStatus( String algorithmId, int userId );

    /// <summary>
    /// Set the algorithm status from the worker to update the UX e.g. if there was an error.
    /// </summary>
    /// <param name="algorithmId">Algorithm id we're setting.</param>
    /// <param name="status">Status enum of the current worker</param>
    /// <param name="message">Message for the algorithm status event</param>
    void setAlgorithmStatus( String algorithmId, AlgorithmStatus status, String message = "" );

    /// <summary>
    /// Send the statistics to storage for performance tracking.
    /// </summary>
    /// <param name="algorithmId">Identifier for algorithm</param>
    /// <param name="unrealized">Unrealized gainloss</param>
    /// <param name="fees">Total fees</param>
    /// <param name="netProfit">Net profi</param>
    /// <param name="holdings">Algorithm holdings</param>
    /// <param name="equity">Total equity</param>
    /// <param name="netReturn">Algorithm return</param>
    /// <param name="volume">Volume traded</param>
    /// <param name="trades">Total trades since inception</param>
    /// <param name="sharpe">Sharpe ratio since inception</param>
    void sendStatistics( String algorithmId, BigDecimal unrealized, BigDecimal fees, BigDecimal netProfit, BigDecimal holdings, BigDecimal equity, BigDecimal netReturn, BigDecimal volume, int trades, double sharpe);

    /// <summary>
    /// Market Status Today: REST call.
    /// </summary>
    /// <param name="time">The date we need market hours for</param>
    /// <param name="symbol"></param>
    /// <returns>Market open hours.</returns>
    Iterable<MarketHoursSegment> marketToday( LocalDateTime time, Symbol symbol);

    default void store( String data, String location, StoragePermissions permissions ) {
        store( data, location, permissions, false );
    }
    
    /// Store the algorithm logs.
    void store( String data, String location, StoragePermissions permissions, boolean async );

    /// Send an email to the user associated with the specified algorithm id
    /// <param name="algorithmId">The algorithm id</param>
    /// <param name="subject">The email subject</param>
    /// <param name="body">The email message body</param>
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
    /// <summary>
    /// API for QuantConnect.com 
    /// </summary>
    [InheritedExport(typeof(IApi))]
    public interface IApi : IDisposable
    {
        /// <summary>
        /// Initialize the control system
        /// </summary>
        void Initialize(int userId, String token);

        /// <summary>
        /// Create a project with the specified name and language via QuantConnect.com API
        /// </summary>
        /// <param name="name">Project name</param>
        /// <param name="language">Programming language to use</param>
        /// <returns>Project object from the API.</returns>
        Project CreateProject( String name, Language language);

        /// <summary>
        /// Read in a project from the QuantConnect.com API.
        /// </summary>
        /// <param name="projectId">Project id you own</param>
        /// <returns></returns>
        Project ReadProject(int projectId);

        /// <summary>
        /// Update a specific project with a list of files. All other files will be deleted.
        /// </summary>
        /// <param name="projectId">Project id for project to be updated</param>
        /// <param name="files">Files list to update</param>
        /// <returns>RestResponse indicating success</returns>
        RestResponse UpdateProject(int projectId, List<ProjectFile> files);

        /// <summary>
        /// Delete a specific project owned by the user from QuantConnect.com
        /// </summary>
        /// <param name="projectId">Project id we own and wish to delete</param>
        /// <returns>RestResponse indicating success</returns>
        RestResponse Delete(int projectId);

        /// <summary>
        /// Read back a list of all projects on the account for a user.
        /// </summary>
        /// <returns>Container for list of projects</returns>
        ProjectList ProjectList();

        /// <summary>
        /// Create a new compile job request for this project id.
        /// </summary>
        /// <param name="projectId">Project id we wish to compile.</param>
        /// <returns>Compile object result</returns>
        Compile CreateCompile(int projectId);

        /// <summary>
        /// Read a compile packet job result.
        /// </summary>
        /// <param name="projectId">Project id we sent for compile</param>
        /// <param name="compileId">Compile id return from the creation request</param>
        /// <returns>Compile object result</returns>
        Compile ReadCompile(int projectId, String compileId);

        /// <summary>
        /// Create a new backtest from a specified projectId and compileId
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="compileId"></param>
        /// <param name="backtestName"></param>
        /// <returns></returns>
        Backtest CreateBacktest(int projectId, String compileId, String backtestName);

        /// <summary>
        /// Read out the full result of a specific backtest
        /// </summary>
        /// <param name="projectId">Project id for the backtest we'd like to read</param>
        /// <param name="backtestId">Backtest id for the backtest we'd like to read</param>
        /// <returns>Backtest result object</returns>
        Backtest ReadBacktest(int projectId, String backtestId);

        /// <summary>
        /// Update the backtest name
        /// </summary>
        /// <param name="projectId">Project id to update</param>
        /// <param name="backtestId">Backtest id to update</param>
        /// <param name="backtestName">New backtest name to set</param>
        /// <param name="backtestNote">Note attached to the backtest</param>
        /// <returns>Rest response on success</returns>
        RestResponse UpdateBacktest(int projectId, String backtestId, String backtestName = "", String backtestNote = "");

        /// <summary>
        /// Delete a backtest from the specified project and backtestId.
        /// </summary>
        /// <param name="projectId">Project for the backtest we want to delete</param>
        /// <param name="backtestId">Backtest id we want to delete</param>
        /// <returns>RestResponse on success</returns>
        RestResponse DeleteBacktest(int projectId, String backtestId);

        /// <summary>
        /// Get a list of backtests for a specific project id
        /// </summary>
        /// <param name="projectId">Project id to search</param>
        /// <returns>BacktestList container for list of backtests</returns>
        BacktestList BacktestList(int projectId);

        /// <summary>
        /// Get a list of live running algorithms for a logged in user.
        /// </summary>
        /// <returns>List of live algorithm instances</returns>
        LiveList LiveList();
        



        //Status StatusRead(int projectId, String algorithmId);
        //RestResponse StatusUpdate(int projectId, String algorithmId, AlgorithmStatus status, String message = "");
        //LogControl LogAllowanceRead();
        //void LogAllowanceUpdate( String backtestId, String url, int length);
        //void StatisticsUpdate(int projectId, String algorithmId, BigDecimal unrealized, BigDecimal fees, BigDecimal netProfit, BigDecimal holdings, BigDecimal equity, BigDecimal netReturn, BigDecimal volume, int trades, double sharpe);
        //void NotifyOwner(int projectId, String algorithmId, String subject, String body);
        //IEnumerable<MarketHoursSegment> MarketHours(int projectId, DateTime time, Symbol symbol);



        /// <summary>
        /// Read the maximum log allowance
        /// </summary>
        int[] ReadLogAllowance(int userId, String userToken);

        /// <summary>
        /// Update running total of log usage
        /// </summary>
        void UpdateDailyLogUsed(int userId, String backtestId, String url, int length, String userToken, boolean hitLimit = false);

        /// <summary>
        /// Get the algorithm current status, active or cancelled from the user
        /// </summary>
        /// <param name="algorithmId"></param>
        /// <param name="userId">The user id of the algorithm</param>
        /// <returns></returns>
        AlgorithmControl GetAlgorithmStatus( String algorithmId, int userId);

        /// <summary>
        /// Set the algorithm status from the worker to update the UX e.g. if there was an error.
        /// </summary>
        /// <param name="algorithmId">Algorithm id we're setting.</param>
        /// <param name="status">Status enum of the current worker</param>
        /// <param name="message">Message for the algorithm status event</param>
        void SetAlgorithmStatus( String algorithmId, AlgorithmStatus status, String message = "");

        /// <summary>
        /// Send the statistics to storage for performance tracking.
        /// </summary>
        /// <param name="algorithmId">Identifier for algorithm</param>
        /// <param name="unrealized">Unrealized gainloss</param>
        /// <param name="fees">Total fees</param>
        /// <param name="netProfit">Net profi</param>
        /// <param name="holdings">Algorithm holdings</param>
        /// <param name="equity">Total equity</param>
        /// <param name="netReturn">Algorithm return</param>
        /// <param name="volume">Volume traded</param>
        /// <param name="trades">Total trades since inception</param>
        /// <param name="sharpe">Sharpe ratio since inception</param>
        void SendStatistics( String algorithmId, BigDecimal unrealized, BigDecimal fees, BigDecimal netProfit, BigDecimal holdings, BigDecimal equity, BigDecimal netReturn, BigDecimal volume, int trades, double sharpe);

        /// <summary>
        /// Market Status Today: REST call.
        /// </summary>
        /// <param name="time">The date we need market hours for</param>
        /// <param name="symbol"></param>
        /// <returns>Market open hours.</returns>
        IEnumerable<MarketHoursSegment> MarketToday(DateTime time, Symbol symbol);

        /// <summary>
        /// Store the algorithm logs.
        /// </summary>
        void Store( String data, String location, StoragePermissions permissions, boolean async = false);

        /// <summary>
        /// Send an email to the user associated with the specified algorithm id
        /// </summary>
        /// <param name="algorithmId">The algorithm id</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email message body</param>
        void SendUserEmail( String algorithmId, String subject, String body);
    }
}
*/