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

package com.quantconnect.lean.Securities.Forex 
{
    /**
    /// Forex packet by packet data filtering mechanism for dynamically detecting bad ticks.
    */
    /// <seealso cref="SecurityDataFilter"/>
    public class ForexDataFilter : SecurityDataFilter
    {
        /**
        /// Initialize forex data filter class:
        */
        public ForexDataFilter()
            : base() {
            
        }

        /**
        /// Forex data filter: a true value means accept the packet, a false means fail.
        */
         * @param data">Data object we're scanning to filter
         * @param vehicle">Security asset
        public @Override boolean Filter(Security vehicle, BaseData data) {
            //FX data is from FXCM and fairly clean already. Accept all packets.
            return true;
        }

    } //End Filter

} //End Namespace