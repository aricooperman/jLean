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

package com.quantconnect.lean.Orders.Fills
{
    /**
    /// Represents a model that simulates order fill events
    */
    public interface IFillModel
    {
        /**
        /// Model the slippage on a market order: fixed percentage of order price
        */
         * @param asset">Asset we're trading this order
         * @param order">Order to update
        @returns Order fill information detailing the average price and quantity filled.
        OrderEvent MarketFill(Security asset, MarketOrder order);

        /**
        /// Stop Market Fill Model. Return an order event with the fill details.
        */
         * @param asset">Asset we're trading this order
         * @param order">Stop Order to Check, return filled if true
        @returns Order fill information detailing the average price and quantity filled.
        OrderEvent StopMarketFill(Security asset, StopMarketOrder order);

        /**
        /// Stop Limit Fill Model. Return an order event with the fill details.
        */
         * @param asset">Asset we're trading this order
         * @param order">Stop Limit Order to Check, return filled if true
        @returns Order fill information detailing the average price and quantity filled.
        OrderEvent StopLimitFill(Security asset, StopLimitOrder order);

        /**
        /// Limit Fill Model. Return an order event with the fill details.
        */
         * @param asset">Stock Object to use to help model limit fill
         * @param order">Order to fill. Alter the values directly if filled.
        @returns Order fill information detailing the average price and quantity filled.
        OrderEvent LimitFill(Security asset, LimitOrder order);

        /**
        /// Market on Open Fill Model. Return an order event with the fill details
        */
         * @param asset">Asset we're trading with this order
         * @param order">Order to be filled
        @returns Order fill information detailing the average price and quantity filled.
        OrderEvent MarketOnOpenFill(Security asset, MarketOnOpenOrder order);

        /**
        /// Market on Close Fill Model. Return an order event with the fill details
        */
         * @param asset">Asset we're trading with this order
         * @param order">Order to be filled
        @returns Order fill information detailing the average price and quantity filled.
        OrderEvent MarketOnCloseFill(Security asset, MarketOnCloseOrder order);
    }
}