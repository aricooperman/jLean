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
using QuantConnect.Orders;
using QuantConnect.Securities;

package com.quantconnect.lean.Brokerages
{
    /**
    /// Represents the base Brokerage implementation. This provides logging on brokerage events.
    */
    public abstract class Brokerage : IBrokerage
    {
        /**
        /// Event that fires each time an order is filled
        */
        public event EventHandler<OrderEvent> OrderStatusChanged;

        /**
        /// Event that fires each time a user's brokerage account is changed
        */
        public event EventHandler<AccountEvent> AccountChanged;

        /**
        /// Event that fires when an error is encountered in the brokerage
        */
        public event EventHandler<BrokerageMessageEvent> Message;

        /**
        /// Gets the name of the brokerage
        */
        public String Name { get; private set; }

        /**
        /// Returns true if we're currently connected to the broker
        */
        public abstract boolean IsConnected { get; }

        /**
        /// Creates a new Brokerage instance with the specified name
        */
         * @param name">The name of the brokerage
        protected Brokerage( String name) {
            Name = name;
        }

        /**
        /// Places a new order and assigns a new broker ID to the order
        */
         * @param order">The order to be placed
        @returns True if the request for a new order has been placed, false otherwise
        public abstract boolean PlaceOrder(Order order);

        /**
        /// Updates the order with the same id
        */
         * @param order">The new order information
        @returns True if the request was made for the order to be updated, false otherwise
        public abstract boolean UpdateOrder(Order order);

        /**
        /// Cancels the order with the specified ID
        */
         * @param order">The order to cancel
        @returns True if the request was made for the order to be canceled, false otherwise
        public abstract boolean CancelOrder(Order order);

        /**
        /// Connects the client to the broker's remote servers
        */
        public abstract void Connect();

        /**
        /// Disconnects the client from the broker's remote servers
        */
        public abstract void Disconnect();

        /**
        /// Event invocator for the OrderFilled event
        */
         * @param e">The OrderEvent
        protected virtual void OnOrderEvent(OrderEvent e) {
            try
            {
                Log.Debug( "Brokerage.OnOrderEvent(): " + e);

                handler = OrderStatusChanged;
                if( handler != null ) handler(this, e);
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
        /// Event invocator for the AccountChanged event
        */
         * @param e">The AccountEvent
        protected virtual void OnAccountChanged(AccountEvent e) {
            try
            {
                Log.Trace( "Brokerage.OnAccountChanged(): " + e);

                handler = AccountChanged;
                if( handler != null ) handler(this, e);
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
        /// Event invocator for the Message event
        */
         * @param e">The error
        protected virtual void OnMessage(BrokerageMessageEvent e) {
            try
            {
                if( e.Type == BrokerageMessageType.Error) {
                    Log.Error( "Brokerage.OnMessage(): " + e);
                }
                else
                {
                    Log.Trace( "Brokerage.OnMessage(): " + e);
                }

                handler = Message;
                if( handler != null ) handler(this, e);
            }
            catch (Exception err) {
                Log.Error(err);
            }
        }

        /**
        /// Gets all open orders on the account. 
        /// NOTE: The order objects returned do not have QC order IDs.
        */
        @returns The open orders returned from IB
        public abstract List<Order> GetOpenOrders();

        /**
        /// Gets all holdings for the account
        */
        @returns The current holdings from the account
        public abstract List<Holding> GetAccountHoldings();

        /**
        /// Gets the current cash balance for each currency held in the brokerage account
        */
        @returns The current cash balance for each currency available for trading
        public abstract List<Cash> GetCashBalance();

        /**
        /// Specifies whether the brokerage will instantly update account balances
        */
        public virtual boolean AccountInstantlyUpdated
        {
            get { return false; }
        }
    }
}
