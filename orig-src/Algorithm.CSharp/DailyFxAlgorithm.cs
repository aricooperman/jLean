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
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.Custom;

package com.quantconnect.lean.Algorithm.CSharp
{
    /**
    /// Daily Fx demonstration to call on and use the FXCM Calendar API
    */
    public class DailyFxAlgorithm : QCAlgorithm
    {
        /**
        /// Add the Daily FX type to our algorithm and use its events.
        */
        public @Override void Initialize() {
            SetStartDate(2016, 05, 26);  //Set Start Date
            SetEndDate(2016, 05, 27);    //Set End Date
            SetCash(100000);             //Set Strategy Cash
            AddData<DailyFx>( "DFX", Resolution.Second, ZoneId.Utc);
        }

        private int _sliceCount = 0;
        public @Override void OnData(Slice slice) {
            result = slice.Get<DailyFx>();
            Console.WriteLine( "SLICE >> %1$s : %2$s", _sliceCount++, result);
        }

        /**
        /// Trigger an event on a complete calendar event which has an actual value.
        */
        private int _eventCount = 0;
        private Map<String, DailyFx> _uniqueConfirmation = new Map<String, DailyFx>();
        public void OnData(DailyFx calendar) {
            _uniqueConfirmation.Add(calendar.toString(), calendar);
            Console.WriteLine( "ONDATA >> %1$s: %2$s", _eventCount++, calendar);
        }
    }
}