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
     * Collections of <see cref="Delisting"/> keyed by <see cref="Symbol"/>
    */
    public class Delistings : DataMap<Delisting>
    {
        /**
         * Initializes a new instance of the <see cref="Delistings"/> dictionary
        */
        public Delistings() {
        }

        /**
         * Initializes a new instance of the <see cref="Delistings"/> dictionary
        */
         * @param frontier The time associated with the data in this Map
        public Delistings(DateTime frontier)
            : base(frontier) {
        }
    }
}
