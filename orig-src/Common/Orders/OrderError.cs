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

using System.ComponentModel;

package com.quantconnect.lean.Orders
{
    /**
    /// Specifies the possible error states during presubmission checks
    */
    public enum OrderError
    {
        /**
        /// Order has already been filled and cannot be modified
        */
        [Description( "Order has already been filled and cannot be modified")]
        CanNotUpdateFilledOrder = -8,

        /**
        /// General error in order
        */
        [Description( "General error in order")]
        GeneralError = -7,

        /**
        /// Order timestamp error. Order appears to be executing in the future
        */
        [Description( "Order timestamp error. Order appears to be executing in the future")]
        TimestampError = -6,

        /**
        /// Exceeded maximum allowed orders for one analysis period
        */
        [Description( "Exceeded maximum allowed orders for one analysis period")]
        MaxOrdersExceeded = -5,

        /**
        /// Insufficient capital to execute order
        */
        [Description( "Insufficient capital to execute order")]
        InsufficientCapital = -4,

        /**
        /// Attempting market order outside of market hours
        */
        [Description( "Attempting market order outside of market hours")]
        MarketClosed = -3,

        /**
        /// There is no data yet for this security - please wait for data (market order price not available yet)
        */
        [Description( "There is no data yet for this security - please wait for data (market order price not available yet)")]
        NoData = -2,

        /**
        /// Order quantity must not be zero
        */
        [Description( "Order quantity must not be zero")]
        ZeroQuantity = -1,

        /**
        /// The order is OK
        */
        [Description( "The order is OK")]
        None = 0
    }
}
