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
using QuantConnect.Brokerages;
using QuantConnect.Orders;
using QuantConnect.Securities;

package com.quantconnect.lean.Interfaces
{
    /**
    /// Brokerage interface that defines the operations all brokerages must implement. The IBrokerage implementation
    /// must have a matching IBrokerageFactory implementation.
    */
    public interface IBrokerage
    {
        /**
        /// Event that fires each time an order is filled
        */
        event EventHandler<OrderEvent> OrderStatusChanged;

        /**
        /// Event that fires each time a user's brokerage account is changed
        */
        event EventHandler<AccountEvent> AccountChanged;

        /**
        /// Event that fires when a message is received from the brokerage
        */
        event EventHandler<BrokerageMessageEvent> Message;

        /**
        /// Gets the name of the brokerage
        */
        String Name { get; }

        /**
        /// Returns true if we're currently connected to the broker
        */
        boolean IsConnected { get; }

        /**
        /// Gets all open orders on the account
        */
        @returns The open orders returned from IB
        List<Order> GetOpenOrders();

        /**
        /// Gets all holdings for the account
        */
        @returns The current holdings from the account
        List<Holding> GetAccountHoldings();

        /**
        /// Gets the current cash balance for each currency held in the brokerage account
        */
        @returns The current cash balance for each currency available for trading
        List<Cash> GetCashBalance();

        /**
        /// Places a new order and assigns a new broker ID to the order
        */
         * @param order">The order to be placed
        @returns True if the request for a new order has been placed, false otherwise
        boolean PlaceOrder(Order order);

        /**
        /// Updates the order with the same id
        */
         * @param order">The new order information
        @returns True if the request was made for the order to be updated, false otherwise
        boolean UpdateOrder(Order order);

        /**
        /// Cancels the order with the specified ID
        */
         * @param order">The order to cancel
        @returns True if the request was made for the order to be canceled, false otherwise
        boolean CancelOrder(Order order);

        /**
        /// Connects the client to the broker's remote servers
        */
        void Connect();

        /**
        /// Disconnects the client from the broker's remote servers
        */
        void Disconnect();

        /**
        /// Specifies whether the brokerage will instantly update account balances
        */
        boolean AccountInstantlyUpdated { get; }
    }
}