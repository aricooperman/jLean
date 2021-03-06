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
using QuantConnect.Orders;

package com.quantconnect.lean.Securities
{
    /**
     * Provides a transaction model that always returns the same order fee.
    */
    public sealed class ConstantFeeTransactionModel : SecurityTransactionModel
    {
        private final BigDecimal _fee;

        /**
         * Initializes a new instance of the <see cref="ConstantFeeTransactionModel"/> class with the specified <paramref name="fee"/>
        */
         * @param fee The constant order fee used by the model
        public ConstantFeeTransactionModel( BigDecimal fee) {
            _fee = Math.Abs(fee);
        }

        /**
         * Returns the constant fee for the model
        */
         * @param security The security matching the order
         * @param order The order to compute fees for
        @returns The cost of the order in units of the account currency
        public @Override BigDecimal GetOrderFee(Security security, Order order) {
            return _fee;
        }
    }
}