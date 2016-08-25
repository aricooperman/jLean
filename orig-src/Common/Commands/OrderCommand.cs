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
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Packets;

package com.quantconnect.lean.Commands
{
    /**
     * Represents a command to submit an order to the algorithm
    */
    public sealed class OrderCommand : ICommand
    {
        /**
         * Gets or sets the security type for the symbol
        */
        public SecurityType SecurityType { get; set; }

        /**
         * Gets or sets the symbol to be ordered
        */
        public Symbol Symbol { get; set; }

        /**
         * Gets or sets the order type to be submted
        */
        public OrderType OrderType { get; set; }

        /**
         * Gets or sets the number of units to be ordered (directional)
        */
        public int Quantity { get; set; }

        /**
         * Gets or sets the limit price. Only applies to <see cref="QuantConnect.Orders.OrderType.Limit"/> and <see cref="QuantConnect.Orders.OrderType.StopLimit"/>
        */
        public BigDecimal LimitPrice { get; set; }

        /**
         * Gets or sets the stop price. Only applies to <see cref="QuantConnect.Orders.OrderType.StopLimit"/> and <see cref="QuantConnect.Orders.OrderType.StopMarket"/>
        */
        public BigDecimal StopPrice { get; set; }

        /**
         * Gets or sets an arbitrary tag to be attached to the order
        */
        public String Tag { get; set; }

        /**
         * Runs this command against the specified algorithm instance
        */
         * @param algorithm The algorithm to run this command against
        public CommandResultPacket Run(IAlgorithm algorithm) {
            request = new SubmitOrderRequest(OrderType, SecurityType, Symbol, Quantity, StopPrice, LimitPrice, DateTime.UtcNow, Tag);
            ticket = algorithm.Transactions.ProcessRequest(request);
            response = ticket.GetMostRecentOrderResponse();
            message = String.format( "%1$s for %2$s units of %3$s: %4$s", OrderType, Quantity, Symbol, response);
            
            if( response.IsSuccess) {
                algorithm.Debug(message);
            }
            else
            {
                algorithm.Error(message);
            }

            return new CommandResultPacket(this, response.IsSuccess);
        }

        /**
         * Returns a String that represents the current object.
        */
        @returns 
         * A String that represents the current object.
         * 
         * <filterpriority>2</filterpriority>
        public @Override String toString() {
            // delegate to the order request
            return new SubmitOrderRequest(OrderType, SecurityType, Symbol, Quantity, StopPrice, LimitPrice, DateTime.UtcNow, Tag).toString();
        }
    }
}