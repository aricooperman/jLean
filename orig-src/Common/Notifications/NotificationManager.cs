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
using System.Collections.Concurrent;

package com.quantconnect.lean.Notifications
{
    /**
     * Local/desktop implementation of messaging system for Lean Engine.
    */
    public class NotificationManager
    {
        private int _count;
        private DateTime _resetTime;
        private static final int _rateLimit = 30;
        private final boolean _liveMode;

        /**
         * Public access to the messages
        */
        public ConcurrentQueue<Notification> Messages
        {
            get; set;
        }

        /**
         * Initialize the messaging system
        */
        public NotificationManager( boolean liveMode) {
            _count = 0;
            _liveMode = liveMode;
            _resetTime = DateTime.Now;
            Messages = new ConcurrentQueue<Notification>();
        }

        /**
         * Maintain a rate limit of the notification messages per hour send of roughly 20 messages per hour.
        */
        @returns True on under rate limit and acceptable to send message
        private boolean Allow() {
            if( DateTime.Now > _resetTime) {
                _count = 0;
                _resetTime = DateTime.Now.RoundUp(Duration.ofHours(1));
            }

            if( _count < _rateLimit) {
                _count++;
                return true;
            }
            return false;
        }

        /**
         * Send an email to the address specified for live trading notifications.
        */
         * @param subject Subject of the email
         * @param message Message body, up to 10kb
         * @param data Data attachment (optional)
         * @param address Email address to send to
        public boolean Email( String address, String subject, String message, String data = "") {
            if( !_liveMode) return false;
            allow = Allow();

            if( allow) {
                email = new NotificationEmail(address, subject, message, data);
                Messages.Enqueue(email);
            }

            return allow;
        }

        /**
         * Send an SMS to the phone number specified
        */
         * @param phoneNumber Phone number to send to
         * @param message Message to send
        public boolean Sms( String phoneNumber, String message) {
            if( !_liveMode) return false;
            allow = Allow();
            if( allow) {
                sms = new NotificationSms(phoneNumber, message);
                Messages.Enqueue(sms);
            }
            return allow;
        }

        /**
         * Place REST POST call to the specified address with the specified DATA.
        */
         * @param address Endpoint address
         * @param data Data to send in body JSON encoded (optional)
        public boolean Web( String address, object data = null ) {
            if( !_liveMode) return false;
            allow = Allow();
            if( allow) {
                web = new NotificationWeb(address, data);
                Messages.Enqueue(web);
            }
            return allow;
        }
    }
}
