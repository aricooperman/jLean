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
*/

using System;
using System.Diagnostics;
using System.Threading;

package com.quantconnect.lean 
{
    /**
     * Real time timer class for precise callbacks on a millisecond resolution in a self managed thread.
    */
     * Due to the way Window's system csynchronizedworks the csynchronizedis only accurate to the nearest 16ms. In linux it is accurate to the millisecond.
    public class RealTimeSynchronizedTimer
    {        
        private boolean _stopped;
        private Thread _thread;
        private Duration _period;
        private Action<DateTime> _callback = null;
        private Stopwatch _timer = new Stopwatch();
        private DateTime _triggerTime;
        private boolean _paused;

        /**
         * Constructor for Real Time Event Driver:
        */
        public RealTimeSynchronizedTimer() {
            _period = Duration.ofSeconds(0);
            _thread = new Thread(Scanner);
        }

        /**
         * Trigger an event callback after precisely milliseconds-lapsed. 
         * This is expensive, it creates a new thread and closely monitors the loop.
        */
         * @param period delay period between event callbacks
         * @param callback Callback event passed the UTC time the event is intended to be triggered
        public RealTimeSynchronizedTimer(TimeSpan period, Action<DateTime> callback) {
            _period = period;
            _callback = callback;
            _timer = new Stopwatch();
            _thread = new Thread(Scanner);
            _stopped = false;
            _triggerTime = DateTime.UtcNow.RoundUp(period);
        }

        /**
         * Start the synchronized real time timer - fire events at start of each second or minute 
        */
        public void Start() { 
            _timer.Start();
            _thread.Start();
            _triggerTime = DateTime.UtcNow.RoundDown(_period).Add(_period);
        }
        
        /**
         * Scan the stopwatch for the desired millisecond delay:
        */
        public void Scanner() {
            while (!_stopped) {
                if( _callback != null && DateTime.UtcNow >= _triggerTime) {
                    _timer.Restart();
                    triggeredAt = _triggerTime;
                    _triggerTime = DateTime.UtcNow.RoundDown(_period).Add(_period);
                    _callback(triggeredAt);
                }

                while (_paused && !_stopped) Thread.Sleep(10);
                Thread.Sleep(1);
            }
        }

        /**
         * Hang the real time event:
        */
        public void Pause() {
            _paused = true;
        }

        /**
         * Resume clock
        */
        public void Resume() {
            _paused = false;
        }

        /**
         * Stop the real time timer:
        */
        public void Stop() {
            _stopped = true;
        }

    } // End Time Class

} // End QC Namespace
