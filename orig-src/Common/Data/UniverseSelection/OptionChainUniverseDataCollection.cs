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
using System.Collections.Generic;

package com.quantconnect.lean.Data.UniverseSelection
{
    /**
     * Defines the universe selection data type for <see cref="OptionChainUniverse"/>
    */
    public class OptionChainUniverseDataCollection : BaseDataCollection
    {
        /**
         * The option chain's underlying price data
        */
        public BaseData Underlying { get; set; }

        /**
         * Gets or sets the contracts selected by the universe
        */
        public HashSet<Symbol> FilteredContracts { get; set; }
        
        /**
         * Initializes a new default instance of the <see cref="OptionChainUniverseDataCollection"/> c;ass
        */
        public OptionChainUniverseDataCollection()
            : this(DateTime.MinValue, Symbol.Empty) {
            FilteredContracts = new HashSet<Symbol>();
        }

        /**
         * Initializes a new instance of the <see cref="OptionChainUniverseDataCollection"/> class
        */
         * @param time The time of this data
         * @param symbol A common identifier for all data in this packet
         * @param data The data to add to this collection
         * @param underlying The option chain's underlying price data
        public OptionChainUniverseDataCollection(DateTime time, Symbol symbol, IEnumerable<BaseData> data = null, BaseData underlying = null )
            : this(time, time, symbol, data, underlying) {
        }

        /**
         * Initializes a new instance of the <see cref="OptionChainUniverseDataCollection"/> class
        */
         * @param time The start time of this data
         * @param endTime The end time of this data
         * @param symbol A common identifier for all data in this packet
         * @param data The data to add to this collection
         * @param underlying The option chain's underlying price data
        public OptionChainUniverseDataCollection(DateTime time, DateTime endTime, Symbol symbol, IEnumerable<BaseData> data = null, BaseData underlying = null )
            : base(time, endTime, symbol, data) {
            Underlying = underlying;
        }

        /**
         * Return a new instance clone of this object, used in fill forward
        */
         * 
         * This base implementation uses reflection to copy all public fields and properties
         * 
        @returns A clone of the current object
        public @Override BaseData Clone() {
            return new OptionChainUniverseDataCollection
            {
                Underlying = Underlying,
                Symbol = Symbol,
                Time = Time,
                EndTime = EndTime,
                Data = Data,
                DataType = DataType,
                FilteredContracts = FilteredContracts
            };
        }
    }
}