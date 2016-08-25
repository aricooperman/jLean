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

using QuantConnect.Data;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Securities.Option
{
    /**
     * Provides a default implementation of <see cref="IOptionPriceModel"/> that does not compute any
     * greeks and uses the current price for the theoretical price. 
     * This is a stub implementation until the real models are implemented
    */
    public class CurrentPriceOptionPriceModel : IOptionPriceModel
    {
        /**
         * Creates a new <see cref="OptionPriceModelResult"/> containing the current <see cref="Security.Price"/>
         * and a default, empty instance of <see cref="FirstOrderGreeks"/>
        */
         * @param security The option security object
         * @param slice The current data slice. This can be used to access other information
         * available to the algorithm
         * @param contract The option contract to evaluate
        @returns An instance of <see cref="OptionPriceModelResult"/> containing the theoretical
         * price of the specified option contract
        public OptionPriceModelResult Evaluate(Security security, Slice slice, OptionContract contract) {
            return new OptionPriceModelResult(security.Price, new FirstOrderGreeks());
        }
    }
}