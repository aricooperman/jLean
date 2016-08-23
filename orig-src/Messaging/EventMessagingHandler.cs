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
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Notifications;
using QuantConnect.Packets;

package com.quantconnect.lean.Messaging
{
    /**
    /// Desktop implementation of messaging system for Lean Engine
    */
    public class EventMessagingHandler : IMessagingHandler
    {
        private AlgorithmNodePacket _job;
        private volatile boolean _loaded;
        private Queue<Packet> _queue; 

        /**
        /// Gets or sets whether this messaging handler has any current subscribers.
        /// When set to false, messages won't be sent.
        */
        public boolean HasSubscribers
        {
            get;
            set;
        }

        /**
        /// Initialize the Messaging System Plugin. 
        */
        public void Initialize() {
            _queue = new Queue<Packet>();

            ConsumerReadyEvent += () -> { _loaded = true; };
        }

        public void LoadingComplete() {
            _loaded = true;
        }

        /**
        /// Set the user communication channel
        */
         * @param job">
        public void SetAuthentication(AlgorithmNodePacket job) {
            _job = job;
        }

        public delegate void DebugEventRaised(DebugPacket packet);
        public event DebugEventRaised DebugEvent;

        public delegate void LogEventRaised(LogPacket packet);
        public event LogEventRaised LogEvent;

        public delegate void RuntimeErrorEventRaised(RuntimeErrorPacket packet);
        public event RuntimeErrorEventRaised RuntimeErrorEvent;

        public delegate void HandledErrorEventRaised(HandledErrorPacket packet);
        public event HandledErrorEventRaised HandledErrorEvent;

        public delegate void BacktestResultEventRaised(BacktestResultPacket packet);
        public event BacktestResultEventRaised BacktestResultEvent;

        public delegate void ConsumerReadyEventRaised();
        public event ConsumerReadyEventRaised ConsumerReadyEvent;

        /**
        /// Send any message with a base type of Packet.
        */
        public void Send(Packet packet) {
            //Until we're loaded queue it up
            if( !_loaded) {
                _queue.Enqueue(packet);
                return;
            }

            //Catch up if this is the first time
            while (_queue.Count > 0) {
                ProcessPacket(_queue.Dequeue());
            }

            //Finally process this new packet
            ProcessPacket(packet);
        }
        
        /**
        /// Send any notification with a base type of Notification.
        */
         * @param notification">The notification to be sent.
        public void SendNotification(Notification notification) {
            type = notification.GetType();
            if( type == typeof (NotificationEmail) || type == typeof (NotificationWeb) || type == typeof (NotificationSms)) {
                Log.Error( "Messaging.SendNotification(): Send not implemented for notification of type: " + type.Name);
                return;
            }
            notification.Send();
        }

        /**
        /// Packet processing implementation
        */
        private void ProcessPacket(Packet packet) {
            //Packets we handled in the UX.
            switch (packet.Type) {
                case PacketType.Debug:
                    debug = (DebugPacket)packet;
                    OnDebugEvent(debug);
                    break;

                case PacketType.Log:
                    log = (LogPacket)packet;
                    OnLogEvent(log);
                    break;

                case PacketType.RuntimeError:
                    runtime = (RuntimeErrorPacket)packet;
                    OnRuntimeErrorEvent(runtime);
                    break;

                case PacketType.HandledError:
                    handled = (HandledErrorPacket)packet;
                    OnHandledErrorEvent(handled);
                    break;

                case PacketType.BacktestResult:
                    result = (BacktestResultPacket)packet;
                    OnBacktestResultEvent(result);
                    break;
            }

            if( StreamingApi.IsEnabled) {
                StreamingApi.Transmit(_job.UserId, _job.Channel, packet);
            }
        }

        /**
        /// Raise a debug event safely
        */
        protected virtual void OnDebugEvent(DebugPacket packet) {
            handler = DebugEvent;

            if( handler != null ) {
                handler(packet);
            }
        }

        /**
        /// Handler for consumer ready code.
        */
        public virtual void OnConsumerReadyEvent() {
            handler = ConsumerReadyEvent;
            if( handler != null ) {
                handler();
            }
        }

        /**
        /// Raise a log event safely
        */
        protected virtual void OnLogEvent(LogPacket packet) {
            handler = LogEvent;
            if( handler != null ) {
                handler(packet);
            }
        }

        /**
        /// Raise a handled error event safely
        */
        protected virtual void OnHandledErrorEvent(HandledErrorPacket packet) {
            handler = HandledErrorEvent;
            if( handler != null ) {
                handler(packet);
            }
        }

        /**
        /// Raise runtime error safely
        */
        protected virtual void OnRuntimeErrorEvent(RuntimeErrorPacket packet) {
            handler = RuntimeErrorEvent;
            if( handler != null ) {
                handler(packet);
            }
        }

        /**
        /// Raise a backtest result event safely.
        */
        protected virtual void OnBacktestResultEvent(BacktestResultPacket packet) {
            handler = BacktestResultEvent;
            if( handler != null ) {
                handler(packet);
            }
        }
    }
}