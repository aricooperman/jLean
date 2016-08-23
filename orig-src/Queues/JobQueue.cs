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
using System.IO;
using Newtonsoft.Json;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Packets;
using QuantConnect.Util;

package com.quantconnect.lean.Queues
{
    /**
    /// Implementation of local/desktop job request:
    */
    public class JobQueue : IJobQueueHandler
    {
        // The type name of the QuantConnect.Brokerages.Paper.PaperBrokerage
        private static final TextWriter Console = System.Console.Out;
        private static final String PaperBrokerageTypeName = "PaperBrokerage";
        private boolean _liveMode = Config.GetBool( "live-mode");
        private static final String AccessToken = Config.Get( "api-access-token");
        private static final int UserId = Config.GetInt( "job-user-id", 0);
        private static final int ProjectId = Config.GetInt( "job-project-id", 0);
        private static final String AlgorithmTypeName = Config.Get( "algorithm-type-name");
        private final Language Language = (Language)Enum.Parse(typeof(Language), Config.Get( "algorithm-language"));

        /**
        /// Physical location of Algorithm DLL.
        */
        private String AlgorithmLocation
        {
            get
            {
                // we expect this dll to be copied into the output directory
                return Config.Get( "algorithm-location", "QuantConnect.Algorithm.CSharp.dll"); 
            }
        }

        /**
        /// Initialize the job queue:
        */
        public void Initialize() {
            //
        }
        
        /**
        /// Desktop/Local Get Next Task - Get task from the Algorithm folder of VS Solution.
        */
        @returns 
        public AlgorithmNodePacket NextJob(out String location) {
            location = AlgorithmLocation;
            Log.Trace( "JobQueue.NextJob(): Selected " + location);

            // check for parameters in the config
            parameters = new Map<String,String>();
            parametersConfigString = Config.Get( "parameters");
            if( parametersConfigString != string.Empty) {
                parameters = JsonConvert.DeserializeObject<Map<String,String>>(parametersConfigString);
            }

            //If this isn't a backtesting mode/request, attempt a live job.
            if( _liveMode) {
                liveJob = new LiveNodePacket
                {
                    Type = PacketType.LiveNode,
                    Algorithm = File.ReadAllBytes(AlgorithmLocation),
                    Brokerage = Config.Get( "live-mode-brokerage", PaperBrokerageTypeName),
                    Channel = AccessToken,
                    UserId = UserId,
                    ProjectId = ProjectId,
                    Version = Globals.Version,
                    DeployId = AlgorithmTypeName,
                    RamAllocation = int.MaxValue,
                    Parameters = parameters,
                    Language = Language,
                };

                try
                { 
                    // import the brokerage data for the configured brokerage
                    brokerageFactory = Composer.Instance.Single<IBrokerageFactory>(factory -> factory.BrokerageType.MatchesTypeName(liveJob.Brokerage));
                    liveJob.BrokerageData = brokerageFactory.BrokerageData;
                }
                catch (Exception err) {
                    Log.Error(err, String.format( "Error resolving BrokerageData for live job for brokerage %1$s:", liveJob.Brokerage));
                }

                return liveJob;
            }

            //Default run a backtesting job.
            backtestJob = new BacktestNodePacket(0, 0, "", new byte[] {}, 10000, "local") {
                Type = PacketType.BacktestNode,
                Algorithm = File.ReadAllBytes(AlgorithmLocation),
                Channel = AccessToken,
                UserId = UserId,
                ProjectId = ProjectId,
                Version = Globals.Version,
                BacktestId = AlgorithmTypeName,
                RamAllocation = int.MaxValue,
                Language = Language,
                Parameters = parameters
            };

            return backtestJob;
        }

        /**
        /// Desktop/Local acknowledge the task processed. Nothing to do.
        */
         * @param job">
        public void AcknowledgeJob(AlgorithmNodePacket job) {
            // Make the console window pause so we can read log output before exiting and killing the application completely
            Console.WriteLine( "Engine.Main(): Analysis Complete. Press any key to continue.");
            System.Console.Read();
        }
    }

}
