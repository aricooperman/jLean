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
 *
*/

using System;

package com.quantconnect.lean.Data.Market
{
    /**
     * Collection of splits keyed by <see cref="Symbol"/>
    */
    public class Splits : DataMap<Split>
    {
        /**
         * Initializes a new instance of the <see cref="Splits"/> dictionary
        */
        public Splits() {
        }

        /**
         * Initializes a new instance of the <see cref="Splits"/> dictionary
        */
         * @param frontier The time associated with the data in this Map
        public Splits(DateTime frontier)
            : base(frontier) {
        }
    }
}