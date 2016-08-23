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
using QuantConnect.API;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Securities;
using RestSharp;

package com.quantconnect.lean.Api
{
    /**
    /// QuantConnect.com Interaction Via API.
    */
    public class Api : IApi
    {
        private ApiConnection _connection;
        private static MarketHoursDatabase _marketHoursDatabase;

        /**
        /// Initialize the API using the config.json file.
        */
        public virtual void Initialize(int userId, String token) {
            _connection = new ApiConnection(userId, token);
            _marketHoursDatabase = MarketHoursDatabase.FromDataFolder();

            //Allow proper decoding of orders from the API.
            JsonConvert.DefaultSettings = () -> new JsonSerializerSettings
            {
                Converters = { new OrderJsonConverter() }
            };
        }

        /**
        /// Create a project with the specified name and language via QuantConnect.com API
        */
         * @param name">Project name
         * @param language">Programming language to use
        @returns Project object from the API.
        public Project CreateProject( String name, Language language) {
            request = new RestRequest( "projects/create", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter( "application/json", JsonConvert.SerializeObject(new
            {
                name = name,
                language = language
            }), ParameterType.RequestBody);

            Project result;
            _connection.TryRequest(request, out result);
            return result;
        }

        /**
        /// Read in a project from the QuantConnect.com API.
        */
         * @param projectId">Project id you own
        @returns 
        public Project ReadProject(int projectId) {
            request = new RestRequest( "projects/read", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter( "projectId", projectId);
            Project result;
            _connection.TryRequest(request, out result);
            return result;
        }

        /**
        /// Read back a list of all projects on the account for a user.
        */
        @returns Container for list of projects
        public ProjectList ProjectList() {
            request = new RestRequest( "projects/read", Method.GET);
            request.RequestFormat = DataFormat.Json;
            ProjectList result;
            _connection.TryRequest(request, out result);
            return result;
        }

        /**
        /// Update a specific project with a list of files. All other files will be deleted.
        */
         * @param projectId">Project id for project to be updated
         * @param files">Files list to update
        @returns RestResponse indicating success
        public RestResponse UpdateProject(int projectId, List<ProjectFile> files) {
            request = new RestRequest( "projects/update", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter( "application/json", JsonConvert.SerializeObject(new
            {
                projectId = projectId,
                files = files
            }), ParameterType.RequestBody);
            RestResponse result;
            _connection.TryRequest(request, out result);
            return result;
        }

        /**
        /// Delete a specific project owned by the user from QuantConnect.com
        */
         * @param projectId">Project id we own and wish to delete
        @returns RestResponse indicating success
        public RestResponse Delete(int projectId) {
            request = new RestRequest( "projects/delete", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter( "application/json", JsonConvert.SerializeObject(new
            {
                projectId = projectId
            }), ParameterType.RequestBody);
            RestResponse result;
            _connection.TryRequest(request, out result);
            return result;
        }

        /**
        /// Create a new compile job request for this project id.
        */
         * @param projectId">Project id we wish to compile.
        @returns Compile object result
        public Compile CreateCompile(int projectId) {
            request = new RestRequest( "compile/create", Method.POST);
            request.AddParameter( "application/json", JsonConvert.SerializeObject(new
            {
                projectId = projectId
            }), ParameterType.RequestBody);
            Compile result;
            _connection.TryRequest(request, out result);
            return result;
        }

        /**
        /// Read a compile packet job result.
        */
         * @param projectId">Project id we sent for compile
         * @param compileId">Compile id return from the creation request
        @returns Compile object result
        public Compile ReadCompile(int projectId, String compileId) {
            request = new RestRequest( "compile/read", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter( "projectId", projectId);
            request.AddParameter( "compileId", compileId);
            Compile result;
            _connection.TryRequest(request, out result);
            return result;
        }


        /**
        /// Create a new backtest request and get the id.
        */
         * @param projectId">Id for the project we'd like to backtest
         * @param compileId">Successfuly compile id for the project
         * @param backtestName">Name for the new backtest
        @returns Backtest object
        public Backtest CreateBacktest(int projectId, String compileId, String backtestName) {
            request = new RestRequest( "backtests/create", Method.POST);
            request.AddParameter( "projectId", projectId);
            request.AddParameter( "compileId", compileId);
            request.AddParameter( "backtestName", backtestName);
            Backtest result;
            _connection.TryRequest(request, out result);
            return result;
        }

        /**
        /// Read out a backtest in the project id specified.
        */
         * @param projectId">Project id to read
         * @param backtestId">Specific backtest id to read
        @returns Backtest object with the results
        public Backtest ReadBacktest(int projectId, String backtestId) {
            request = new RestRequest( "backtests/read", Method.GET);
            request.AddParameter( "backtestId", backtestId);
            request.AddParameter( "projectId", projectId);
            Backtest result;
            _connection.TryRequest(request, out result);
            return result;
        }

        /**
        /// Update a backtest name
        */
         * @param projectId">Project for the backtest we want to update
         * @param backtestId">Backtest id we want to update
         * @param name">Name we'd like to assign to the backtest
         * @param note">Note attached to the backtest
        @returns Rest response class indicating success
        public RestResponse UpdateBacktest(int projectId, String backtestId, String name = "", String note = "") {
            request = new RestRequest( "backtests/update", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter( "application/json", JsonConvert.SerializeObject(new
            {
                projectId = projectId,
                backtestId = backtestId,
                name = name,
                note = note
            }), ParameterType.RequestBody);
            Backtest result;
            _connection.TryRequest(request, out result);
            return result;
        }

        /**
        /// List all the backtests in a prokect
        */
         * @param projectId">Project id we'd like to get a list of backtest for
        @returns Backtest list container
        public BacktestList BacktestList(int projectId) {
            request = new RestRequest( "backtests/read", Method.GET);
            request.AddParameter( "projectId", projectId);
            BacktestList result;
            _connection.TryRequest(request, out result);
            return result;
        }
        
        /**
        /// Delete a backtest from the specified project and backtestId.
        */
         * @param projectId">Project for the backtest we want to delete
         * @param backtestId">Backtest id we want to delete
        @returns 
        public RestResponse DeleteBacktest(int projectId, String backtestId) {
            request = new RestRequest( "backtests/delete", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter( "backtestId", backtestId);
            request.AddParameter( "projectId", projectId);
            RestResponse result;
            _connection.TryRequest(request, out result);
            return result;
        }

        /**
        /// Get a list of live running algorithms for a logged in user.
        */
        @returns List of live algorithm instances
        public LiveList LiveList() {
            request = new RestRequest( "live/read", Method.GET);
            LiveList result;
            _connection.TryRequest(request, out result);
            return result;
        }


        /**
        /// Calculate the remaining bytes of user log allowed based on the user's cap and daily cumulative usage.
        */
         * @param userId">User ID
         * @param userToken">User API token
        @returns int[3] iUserBacktestLimit, iUserDailyLimit, remaining
        public virtual int[] ReadLogAllowance(int userId, String userToken) {
            return new[] { int.MaxValue, int.MaxValue, int.MaxValue };
        }

        /**
        /// Update the daily log of allowed logging-data
        */
         * @param userId">Id of the User
         * @param backtestId">BacktestId
         * @param url">URL of the log entry
         * @param length">length of data
         * @param userToken">User access token
         * @param hitLimit">Boolean signifying hit log limit
        @returns Number of bytes remaining
        public virtual void UpdateDailyLogUsed(int userId, String backtestId, String url, int length, String userToken, boolean hitLimit = false) {
            //
        }

        /**
        /// Get the algorithm status from the user with this algorithm id.
        */
         * @param algorithmId">String algorithm id we're searching for.
         * @param userId">The user id of the algorithm
        @returns Algorithm status enum
        public virtual AlgorithmControl GetAlgorithmStatus( String algorithmId, int userId) {
            return new AlgorithmControl();
        }

        /**
        /// Algorithm passes back its current status to the UX.
        */
         * @param status">Status of the current algorithm
         * @param algorithmId">String algorithm id we're setting.
         * @param message">Message for the algorithm status event
        @returns Algorithm status enum
        public virtual void SetAlgorithmStatus( String algorithmId, AlgorithmStatus status, String message = "") {
            //
        }

        /**
        /// Send the statistics to storage for performance tracking.
        */
         * @param algorithmId">Identifier for algorithm
         * @param unrealized">Unrealized gainloss
         * @param fees">Total fees
         * @param netProfit">Net profi
         * @param holdings">Algorithm holdings
         * @param equity">Total equity
         * @param netReturn">Net return for the deployment
         * @param volume">Volume traded
         * @param trades">Total trades since inception
         * @param sharpe">Sharpe ratio since inception
        public virtual void SendStatistics( String algorithmId, BigDecimal unrealized, BigDecimal fees, BigDecimal netProfit, BigDecimal holdings, BigDecimal equity, BigDecimal netReturn, BigDecimal volume, int trades, double sharpe) {
            // 
        }

        /**
        /// Get the calendar open hours for the date.
        */
        public virtual IEnumerable<MarketHoursSegment> MarketToday(DateTime time, Symbol symbol) {
            if( Config.GetBool( "force-exchange-always-open")) {
                yield return MarketHoursSegment.OpenAllDay();
                yield break;
            }

            hours = _marketHoursDatabase.GetExchangeHours(symbol.ID.Market, symbol, symbol.ID.SecurityType);
            foreach (segment in hours.MarketHours[time.DayOfWeek].Segments) {
                yield return segment;
            }
        }

        /**
        /// Store logs with these authentication type
        */
        public virtual void Store( String data, String location, StoragePermissions permissions, boolean async = false) {
            //
        }

        /**
        /// Send an email to the user associated with the specified algorithm id
        */
         * @param algorithmId">The algorithm id
         * @param subject">The email subject
         * @param body">The email message body
        public virtual void SendUserEmail( String algorithmId, String subject, String body) {
            //
        }

        /**
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose() {
            // NOP
        }
        
    }
}