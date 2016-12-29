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

package com.quantconnect.lean.orders;

public class OrderTypes {
    
    /// Class of the order: market, limit or stop
    public enum OrderType {
        /// Market Order Type
        Market,
        /// Limit Order Type
        Limit,
        /// Stop Market Order Class - Fill at market price when break target price
        StopMarket,
        /// Stop limit order type - trigger fill once pass the stop price; but limit fill to limit price.
        StopLimit,
        /// Market on open type - executed on exchange open
        MarketOnOpen,
        /// Market on close type - executed on exchange close
        MarketOnClose;
        
        /**
         * Determines whether or not the specified order is a limit order
         * @param orderType The order to check
         * @returns True if the order is a limit order, false otherwise
         */
        public boolean isLimitOrder() {
            return this == OrderType.Limit || this == OrderType.StopLimit;
        }

        /**
         * Determines whether or not the specified order is a stop order
         * @param orderType The order to check
         * @returns True if the order is a stop order, false otherwise
         */
        public boolean isStopOrder() {
            return this == OrderType.StopMarket || this == OrderType.StopLimit;
        }
    }


    /// Order duration in market
    public enum OrderDuration {
        /// Order good until its filled.
        GTC,
        /*
        /// Order valid for today only: -- CURRENTLY ONLY GTC ORDER DURATION TYPE IN BACKTESTS.
        Day
        */
        /// Order valid until a custom set date time value.
        Custom
    }

    /// Direction of the order
    public enum OrderDirection {
        /// Buy Order 
        Buy,
        /// Sell Order
        Sell,
        /// Default Value - No Order Direction
        /// <remarks>
        /// Unfortunately this does not have a value of zero because
        /// there are backtests saved that reference the values in this order
        /// </remarks>
        Hold
    }

    /// Fill status of the order class.
    public enum OrderStatus {
        /// New order pre-submission to the order processor.
        New( 0 ),
        /// Order submitted to the market
        Submitted( 1 ),
        /// Partially filled, In Market Order.
        PartiallyFilled( 2 ),
        /// Completed, Filled, In Market Order.
        Filled( 3 ),
        /// Order cancelled before it was filled
        Canceled( 5 ),
        /// No Order State Yet
        None( 6 ),
        /// Order invalidated before it hit the market (e.g. insufficient capital)..
        Invalid( 7 );
        
        private final int value;

        OrderStatus( int value ) {
            this.value = value;
        }
        
        public int getValue() {
            return value;
        }
        
        /**
         * Determines if the specified status is in a closed state.
         * @param status The status to check
         * @returns True if the status is <see cref="OrderStatus.Filled"/>, <see cref="OrderStatus.Canceled"/>, or <see cref="OrderStatus.Invalid"/>
         */
        public boolean isClosed() {
            return this == OrderStatus.Filled || this == OrderStatus.Canceled || this == OrderStatus.Invalid;
        }

        /**
         * Determines if the specified status is in an open state.
         * @param status The status to check
         * @returns True if the status is not <see cref="OrderStatus.Filled"/>, <see cref="OrderStatus.Canceled"/>, or <see cref="OrderStatus.Invalid"/>
         */
        public boolean isOpen() {
            return !isClosed();
        }

        /**
         * Determines if the specified status is a fill, that is, <see cref="OrderStatus.Filled"/>
         * order <see cref="OrderStatus.PartiallyFilled"/>
         * @param status The status to check
         * @returns True if the status is <see cref="OrderStatus.Filled"/> or <see cref="OrderStatus.PartiallyFilled"/>, false otherwise
         */
        public boolean isFill() {
            return this == OrderStatus.Filled || this == OrderStatus.PartiallyFilled;
        }
    }
}

