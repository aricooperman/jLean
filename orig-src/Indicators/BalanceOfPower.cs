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

using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators
{
    /**
     * This indicator computes the Balance Of Power (BOP). 
     * The Balance Of Power is calculated with the following formula:
     * BOP = (Close - Open) / (High - Low)
    */
    public class BalanceOfPower : TradeBarIndicator
    {
        /**
         * Initializes a new instance of the <see cref="BalanceOfPower"/> class using the specified name.
        */ 
         * @param name The name of this indicator
        public BalanceOfPower( String name)
            : base(name) {
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples > 0; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            range = input.High - input.Low;
            return range > 0 ? (input.Close - input.Open) / range : BigDecimal.ZERO;
        }

    }
}
