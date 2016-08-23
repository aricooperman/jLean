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
using System.IO;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Notifications;
using QuantConnect.Packets;

package com.quantconnect.lean.Messaging
{
    /**
    /// Local/desktop implementation of messaging system for Lean Engine.
    */
    public class Messaging : IMessagingHandler
    {
        // used to aid in generating regression tests via Cosole.WriteLine(...)
        private static final TextWriter Console = System.Console.Out;

        private AlgorithmNodePacket _job;

        /**
        /// This implementation ignores the <seealso cref="HasSubscribers"/> flag and
        /// instead will always write to the log.
        */
        public boolean HasSubscribers
        {
            get; 
            set;
        }

        /**
        /// Initialize the messaging system
        */
        public void Initialize() {
            //
        }

        /**
        /// Set the messaging channel
        */
        public void SetAuthentication(AlgorithmNodePacket job) {
            _job = job;
        }

        /**
        /// Send a generic base packet without processing
        */
        public void Send(Packet packet) {
            switch (packet.Type) {
                case PacketType.Debug:
                    debug = (DebugPacket) packet;
                    Log.Trace( "Debug: " + debug.Message);
                    break;

                case PacketType.Log:
                    log = (LogPacket) packet;
                    Log.Trace( "Log: " + log.Message);
                    break;

                case PacketType.RuntimeError:
                    runtime = (RuntimeErrorPacket) packet;
                    rstack = (!StringUtils.isEmpty(runtime.StackTrace) ? (Environment.NewLine + " " + runtime.StackTrace) : string.Empty);
                    Log.Error(runtime.Message + rstack);
                    break;

                case PacketType.HandledError:
                    handled = (HandledErrorPacket) packet;
                    hstack = (!StringUtils.isEmpty(handled.StackTrace) ? (Environment.NewLine + " " + handled.StackTrace) : string.Empty);
                    Log.Error(handled.Message + hstack);
                    break;

                case PacketType.BacktestResult:
                    result = (BacktestResultPacket) packet;

                    if( result.Progress == 1) {
                        // uncomment these code traces to help write regression tests
                        //Console.WriteLine( "new Map<String,String>");
                        //Console.WriteLine( "\t\t\t{");
                        foreach (pair in result.Results.Statistics) {
                            Log.Trace( "STATISTICS:: " + pair.Key + " " + pair.Value);
                            //Console.WriteLine( "\t\t\t\t{{\"%1$s\",\"%2$s\"}},", pair.Key, pair.Value);
                        }
                        //Console.WriteLine( "\t\t\t};");

                        //foreach (pair in statisticsResults.RollingPerformances)
                        //{
                        //    Log.Trace( "ROLLINGSTATS:: " + pair.Key + " SharpeRatio: " + Math.Round(pair.Value.PortfolioStatistics.SharpeRatio, 3));
                        //}
                    }
                    break;
            }


            if( StreamingApi.IsEnabled) {
                StreamingApi.Transmit(_job.UserId, _job.Channel, packet);
            }
        }

        /**
        /// Send any notification with a base type of Notification.
        */
        public void SendNotification(Notification notification) {
            type = notification.GetType();
            if( type == typeof (NotificationEmail)
             || type == typeof (NotificationWeb)
             || type == typeof (NotificationSms)) {
                Log.Error( "Messaging.SendNotification(): Send not implemented for notification of type: " + type.Name);
                return;
            }
            notification.Send();
        }
    }
}
