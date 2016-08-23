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
    /// Provides extension methods for the <see cref="Order"/> class and for the <see cref="OrderStatus"/> enumeration
    */
    public static class OrderExtensions
    {
        /**
        /// Determines if the specified status is in a closed state.
        */
         * @param status">The status to check
        @returns True if the status is <see cref="OrderStatus.Filled"/>, <see cref="OrderStatus.Canceled"/>, or <see cref="OrderStatus.Invalid"/>
        public static boolean IsClosed(this OrderStatus status) {
            return status == OrderStatus.Filled
                || status == OrderStatus.Canceled
                || status == OrderStatus.Invalid;
        }

        /**
        /// Determines if the specified status is in an open state.
        */
         * @param status">The status to check
        @returns True if the status is not <see cref="OrderStatus.Filled"/>, <see cref="OrderStatus.Canceled"/>, or <see cref="OrderStatus.Invalid"/>
        public static boolean IsOpen(this OrderStatus status) {
            return !status.IsClosed();
        }

        /**
        /// Determines if the specified status is a fill, that is, <see cref="OrderStatus.Filled"/>
        /// order <see cref="OrderStatus.PartiallyFilled"/>
        */
         * @param status">The status to check
        @returns True if the status is <see cref="OrderStatus.Filled"/> or <see cref="OrderStatus.PartiallyFilled"/>, false otherwise
        public static boolean IsFill(this OrderStatus status) {
            return status == OrderStatus.Filled || status == OrderStatus.PartiallyFilled;
        }

        /**
        /// Determines whether or not the specified order is a limit order
        */
         * @param orderType">The order to check
        @returns True if the order is a limit order, false otherwise
        public static boolean IsLimitOrder(this OrderType orderType) {
            return orderType == OrderType.Limit || orderType == OrderType.StopLimit;
        }

        /**
        /// Determines whether or not the specified order is a stop order
        */
         * @param orderType">The order to check
        @returns True if the order is a stop order, false otherwise
        public static boolean IsStopOrder(this OrderType orderType) {
            return orderType == OrderType.StopMarket || orderType == OrderType.StopLimit;
        }
    }
}
