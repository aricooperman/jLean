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

package com.quantconnect.lean.Orders
{
    /**
    /// Specifies the data in an order to be updated
    */
    public class UpdateOrderFields
    {
        /**
        /// Specify to update the quantity of the order
        */
        public int? Quantity { get; set; }

        /**
        /// Specify to update the limit price of the order
        */
        public decimal? LimitPrice { get; set; }

        /**
        /// Specify to update the stop price of the order
        */
        public decimal? StopPrice { get; set; }

        /**
        /// Specify to update the order's tag
        */
        public String Tag { get; set; }
    }
}