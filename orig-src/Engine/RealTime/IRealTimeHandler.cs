﻿/*
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

using System;
using System.ComponentModel.Composition;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Packets;
using QuantConnect.Scheduling;

package com.quantconnect.lean.Lean.Engine.RealTime
{
    /**
    /// Real time event handler, trigger functions at regular or pretimed intervals
    */
    [InheritedExport(typeof(IRealTimeHandler))]
    public interface IRealTimeHandler : IEventSchedule
    {
        /**
        /// Thread status flag.
        */
        boolean IsActive
        {
            get;
        }

        /**
        /// Intializes the real time handler for the specified algorithm and job
        */
        void Setup(IAlgorithm algorithm, AlgorithmNodePacket job, IResultHandler resultHandler, IApi api);

        /**
        /// Main entry point to scan and trigger the realtime events.
        */
        void Run();
        
        /**
        /// Set the current time for the event scanner (so we can use same code for backtesting and live events)
        */
         * @param time">Current real or backtest time.
        void SetTime(DateTime time);

        /**
        /// Trigger and exit signal to terminate real time event scanner.
        */
        void Exit();
    }
}
