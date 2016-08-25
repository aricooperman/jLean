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

package com.quantconnect.lean.Notifications
{
    /**
     * Local/desktop implementation of messaging system for Lean Engine.
    */
    public abstract class Notification
    {
        /**
         * Method for sending implementations of notification object types.
        */
         * SMS, Email and Web are all handled by the QC Messaging Handler. To implement your own notification type implement it here.
        public void Send() {
            //
        }
    }

    /**
     * Web Notification Class
    */
    public class NotificationWeb : Notification
    {
        /**
         * Send a notification message to this web address
        */
        public String Address;

        /**
         * Object data to send.
        */
        public object Data;

        /**
         * Constructor for sending a notification SMS to a specified phone number
        */
         * @param address">
         * @param data">
        public NotificationWeb( String address, object data = null ) {
            Address = address;
            Data = data;
        }
    }

    /**
     * Sms Notification Class
    */
    public class NotificationSms : Notification
    {
        /**
         * Send a notification message to this phone number
        */
        public String PhoneNumber;

        /**
         * Message to send. Limited to 160 characters
        */
        public String Message;

        /**
         * Constructor for sending a notification SMS to a specified phone number
        */
         * @param number">
         * @param message">
        public NotificationSms( String number, String message) {
            PhoneNumber = number;
            Message = message;
        }
    }


    /**
     * Email notification data.
    */
    public class NotificationEmail : Notification
    {
        /**
         * Send to address:
        */
        public String Address;

        /**
         * Email subject
        */
        public String Subject;

        /**
         * Message to send.
        */
        public String Message;

        /**
         * Email Data
        */
        public String Data;

        /**
         * Default constructor for sending an email notification
        */
         * @param address Address to send to
         * @param subject Subject of the email
         * @param message Message body of the email
         * @param data Data to attach to the email
        public NotificationEmail( String address, String subject, String message, String data) {
            Message = message;
            Data = data;
            Subject = subject;
            Address = address;
        }
    }
}
