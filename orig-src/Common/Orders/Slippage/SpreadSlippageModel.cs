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

using QuantConnect.Data.Market;
using QuantConnect.Securities;

package com.quantconnect.lean.Orders.Slippage
{
    /// <summary>
    /// A slippage model that uses half of the bid/ask spread if available,
    /// if not available, zero slippage is assumed.
    /// </summary>
    public class SpreadSlippageModel : ISlippageModel
    {
        /// <summary>
        /// Slippage Model. Return a BigDecimal cash slippage approximation on the order.
        /// </summary>
        public virtual BigDecimal GetSlippageApproximation(Security asset, Order order) {
            lastData = asset.GetLastData();
            lastTick = lastData as Tick;

            // if we have tick data use the spread
            if( lastTick != null ) {
                return (lastTick.AskPrice - lastTick.BidPrice) / 2;
            }

            return 0m;
        }
    }
}