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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using QuantConnect.Configuration;
using QuantConnect.Logging;

package com.quantconnect.lean.Brokerages.InteractiveBrokers
{
    /**
     * Handles launching and killing the IB Controller script
    */
     * 
     * Requires TWS or IB Gateway and IBController installed to run
     * 
    public static class InteractiveBrokersGatewayRunner
    {
        // process that's running the IB Controller script
        private static int ScriptProcessID;

        /**
         * Starts the interactive brokers gateway using values from configuration
        */
        public static void StartFromConfiguration() {
            Start(Config.Get( "ib-controller-dir"),
                Config.Get( "ib-tws-dir"),
                Config.Get( "ib-user-name"),
                Config.Get( "ib-password"),
                Config.GetBool( "ib-use-tws")
                );
        }

        /**
         * Starts the IB Gateway
        */
         * @param ibControllerDirectory Directory to the IB controller installation
         * @param twsDirectory">
         * @param userID The log in user id
         * @param password The log in password
         * @param useTws True to use Trader Work Station, false to just launch the API gateway
        public static void Start( String ibControllerDirectory, String twsDirectory, String userID, String password, boolean useTws = false) {
            useTwsSwitch = useTws ? "TWS" : "GATEWAY";
            batchFilename = Path.Combine( "InteractiveBrokers", "run-ib-controller.bat");
            bashFilename = Path.Combine( "InteractiveBrokers", "run-ib-controller.sh");

            try
            {
                file = OS.IsWindows ? batchFilename : bashFilename;
                arguments = String.format( "%1$s %2$s %3$s %4$s %5$s %6$s", file, ibControllerDirectory, twsDirectory, userID, password, useTwsSwitch);

                Log.Trace( "InteractiveBrokersGatewayRunner.Start(): Launching IBController for account " + userID + "...");

                processStartInfo = OS.IsWindows ? new ProcessStartInfo( "cmd.exe", "/C " + arguments) : new ProcessStartInfo( "bash", arguments);

                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = false;
                process = Process.Start(processStartInfo);
                ScriptProcessID = process.Id;
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
         * Stops the IB Gateway
        */
        public static void Stop() {
            if( ScriptProcessID == 0) {
                return;
            }

            try
            {
                Log.Trace( "InteractiveBrokersGatewayRunner.Stop(): Stopping IBController...");

                // we need to materialize this ienumerable since if we start killing some of them
                // we may leave some daemon processes hanging
                foreach (process in GetSpawnedProcesses(ScriptProcessID).ToList()) {
                    // kill all spawned processes
                    process.Kill();
                }

                ScriptProcessID = 0;
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        private static IEnumerable<Process> GetSpawnedProcesses(int id) {
            // loop over all the processes and return those that were spawned by the specified processed ID
            return Process.GetProcesses().Where(x =>
            {
                try
                {
                    parent = ProcessExtensions.Parent(x);
                    if( parent != null ) {
                        return parent.Id == id;
                    }
                }
                catch
                {
                    return false;
                }
                return false;
            });
        }

        //http://stackoverflow.com/questions/394816/how-to-get-parent-process-in-net-in-managed-way
        private static class ProcessExtensions
        {
            private static String FindIndexedProcessName(int pid) {
                processName = Process.GetProcessById(pid).ProcessName;
                processesByName = Process.GetProcessesByName(processName);
                String processIndexdName = null;

                for (index = 0; index < processesByName.Length; index++) {
                    processIndexdName = index == 0 ? processName : processName + "#" + index;
                    processId = new PerformanceCounter( "Process", "ID Process", processIndexdName);
                    if( (int)processId.NextValue() == pid) {
                        return processIndexdName;
                    }
                }

                return processIndexdName;
            }

            private static Process FindPidFromIndexedProcessName( String indexedProcessName) {
                parentId = new PerformanceCounter( "Process", "Creating Process ID", indexedProcessName);
                return Process.GetProcessById((int)parentId.NextValue());
            }

            public static Process Parent(Process process) {
                return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
            }
        }
    }
}