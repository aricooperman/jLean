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

using QuantConnect.Securities;

package com.quantconnect.lean.Orders.Fees
{
    /**
    /// Represents a model the simulates order fees
    */
    public interface IFeeModel
    {
        /**
        /// Gets the order fee associated with the specified order. This returns the cost
        /// of the transaction in the account currency
        */
         * @param security">The security matching the order
         * @param order">The order to compute fees for
        @returns The cost of the order in units of the account currency
        BigDecimal GetOrderFee(Security security, Order order);
    }
}