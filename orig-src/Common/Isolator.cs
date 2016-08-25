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
using System.Threading;
using System.Threading.Tasks;
using QuantConnect.Logging;

package com.quantconnect.lean 
{
    /**
     * Isolator class - create a new instance of the algorithm and ensure it doesn't 
     * exceed memory or time execution limits.
    */
    public class Isolator
    {
        /**
         * Algo cancellation controls - cancel source.
        */
        public CancellationTokenSource CancellationTokenSource
        {
            get; private set;
        }

        /**
         * Algo cancellation controls - cancellation token for algorithm thread.
        */
        public CancellationToken CancellationToken
        {
            get { return CancellationTokenSource.Token; }
        }

        /**
         * Check if this task isolator is cancelled, and exit the analysis
        */
        public boolean IsCancellationRequested
        {
            get { return CancellationTokenSource.IsCancellationRequested; }
        }

        /**
         * Initializes a new instance of the <see cref="Isolator"/> class
        */
        public Isolator() {
            CancellationTokenSource = new CancellationTokenSource();
        }

        /**
         * Execute a code bsynchronizedwith a maximum limit on time and memory.
        */
         * @param timeSpan Timeout in timespan
         * @param withinCustomLimits Function used to determine if the codeBsynchronizedis within custom limits, such as with algorithm manager
         * timing individual time loops, return a non-null and non-empty String with a message indicating the error/reason for stoppage
         * @param codeBlock Action codebsynchronizedto execute
         * @param memoryCap Maximum memory allocation, default 1024Mb
        @returns True if algorithm exited successfully, false if cancelled because it exceeded limits.
        public boolean ExecuteWithTimeLimit(TimeSpan timeSpan, Func<String> withinCustomLimits, Action codeBlock, long memoryCap = 1024) {
            // default to always within custom limits
            withinCustomLimits = withinCustomLimits ?? (() -> null );

            message = "";
            end = DateTime.Now + timeSpan;
            memoryLogger = DateTime.Now + Duration.ofMinutes(1);

            //Convert to bytes
            memoryCap *= 1024 * 1024;

            //Launch task
            task = Task.Factory.StartNew(codeBlock, CancellationTokenSource.Token);

            while (!task.IsCompleted && DateTime.Now < end) {
                memoryUsed = GC.GetTotalMemory(false);

                if( memoryUsed > memoryCap) {
                    if( GC.GetTotalMemory(true) > memoryCap) {
                        message = "Execution Security Error: Memory Usage Maxed Out - " + Math.Round(Convert.ToDouble(memoryCap / (1024 * 1024))) + "MB max.";
                        break;
                    }
                }

                if( DateTime.Now > memoryLogger) {
                    if( memoryUsed > (memoryCap * 0.8)) {
                        memoryUsed = GC.GetTotalMemory(true);
                        Log.Error( "Execution Security Error: Memory usage over 80% capacity.");
                    }
                    Log.Trace(DateTime.Now.toString( "u") + " Isolator.ExecuteWithTimeLimit(): Used: " + Math.Round(Convert.ToDouble(memoryUsed / (1024 * 1024))));
                    memoryLogger = DateTime.Now.AddMinutes(1);
                }

                // check to see if we're within other custom limits defined by the caller
                possibleMessage = withinCustomLimits();
                if( !StringUtils.isEmpty(possibleMessage)) {
                    message = possibleMessage;
                    break;
                }

                Thread.Sleep(100);
            }

            if( task.IsCompleted == false && message == "") {
                message = "Execution Security Error: Operation timed out - " + timeSpan.TotalMinutes + " minutes max. Check for recursive loops.";
                Log.Trace( "Isolator.ExecuteWithTimeLimit(): " + message);
            }

            if( message != "") {
                CancellationTokenSource.Cancel();
                Log.Error( "Security.ExecuteWithTimeLimit(): " + message);
                throw new Exception(message);
            }
            return task.IsCompleted;
        }

        /**
         * Execute a code bsynchronizedwith a maximum limit on time and memory.
        */
         * @param timeSpan Timeout in timespan
         * @param codeBlock Action codebsynchronizedto execute
         * @param memoryCap Maximum memory allocation, default 1024Mb
        @returns True if algorithm exited successfully, false if cancelled because it exceeded limits.
        public boolean ExecuteWithTimeLimit(TimeSpan timeSpan, Action codeBlock, long memoryCap = 1024) {
            return ExecuteWithTimeLimit(timeSpan, null, codeBlock, memoryCap);
        }
    }
}
