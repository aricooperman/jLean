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

package com.quantconnect.lean.Brokerages
{
    /**
     * Represents a message received from a brokerage
    */
    public class BrokerageMessageEvent
    {
        /**
         * Gets the type of brokerage message
        */
        public BrokerageMessageType Class { get; private set; }

        /**
         * Gets the brokerage specific code for this message, zero if no code was specified
        */
        public String Code { get; private set; }

        /**
         * Gets the message text received from the brokerage
        */
        public String Message { get; private set; }

        /**
         * Initializes a new instance of the BrokerageMessageEvent class
        */
         * @param type The type of brokerage message
         * @param code The brokerage specific code
         * @param message The message text received from the brokerage
        public BrokerageMessageEvent(BrokerageMessageType type, int code, String message) {
            Class = type;
            Code = code.toString();
            Message = message;
        }

        /**
         * Initializes a new instance of the BrokerageMessageEvent class
        */
         * @param type The type of brokerage message
         * @param code The brokerage specific code
         * @param message The message text received from the brokerage
        public BrokerageMessageEvent(BrokerageMessageType type, String code, String message) {
            Class = type;
            Code = code;
            Message = message;
        }

        /**
         * Creates a new <see cref="BrokerageMessageEvent"/> to represent a disconnect message
        */
         * @param message The message from the brokerage
        @returns A brokerage disconnect message
        public static BrokerageMessageEvent Disconnected( String message) {
            return new BrokerageMessageEvent(BrokerageMessageType.Disconnect, "Disconnect", message);
        }

        /**
         * Creates a new <see cref="BrokerageMessageEvent"/> to represent a reconnect message
        */
         * @param message The message from the brokerage
        @returns A brokerage reconnect message
        public static BrokerageMessageEvent Reconnected( String message) {
            return new BrokerageMessageEvent(BrokerageMessageType.Reconnect, "Reconnect", message);
        }

        /**
         * Returns a String that represents the current object.
        */
        @returns 
         * A String that represents the current object.
         * 
         * <filterpriority>2</filterpriority>
        public @Override String toString() {
            return String.format( "%1$s - Code: %2$s - %3$s", Type, Code, Message);
        }
    }
}