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

using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Packets;

package com.quantconnect.lean.Commands
{
    /**
     * Represents a command to update an order by id
    */
    public class UpdateOrderCommand : ICommand
    {
        /**
         * Gets or sets the id of the order to update
        */
        public int OrderId { get; set; }

        /**
         * Gets or sets the new quantity, specify null to not update the quantity
        */
        public OptionalInt Quantity { get; set; }

        /**
         * Gets or sets the new limit price, specify null to not update the limit price.
         * This will only be used if the order has a limit price (Limit/StopLimit orders)
        */
        public Optional<BigDecimal> LimitPrice { get; set; }

        /**
         * Gets or sets the new stop price, specify null to not update the stop price.
         * This will onky be used if the order has a stop price (StopLimit/StopMarket orders)
        */
        public Optional<BigDecimal> StopPrice { get; set; }

        /**
         * Gets or sets the new tag for the order, specify null to not update the tag
        */
        public String Tag { get; set; }

        /**
         * Runs this command against the specified algorithm instance
        */
         * @param algorithm The algorithm to run this command against
        public CommandResultPacket Run(IAlgorithm algorithm) {
            ticket = algorithm.Transactions.UpdateOrder(new UpdateOrderRequest(algorithm.UtcTime, OrderId, new UpdateOrderFields
            {
                Quantity = Quantity,
                LimitPrice = LimitPrice,
                StopPrice = StopPrice,
                Tag = Tag
            }));

            response = ticket.GetMostRecentOrderResponse();
            return new CommandResultPacket(this, response.IsSuccess);
        }
    }
}
