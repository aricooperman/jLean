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
using System.Linq;

package com.quantconnect.lean.Data.UniverseSelection
{
    /**
     * This type exists for transport of data as a single packet
    */
    public class BaseDataCollection : BaseData
    {
        private DateTime _endTime;

        /**
         * Gets the data list
        */
        public List<BaseData> Data { get; set; }

        /**
         * Gets or sets the end time of this data
        */
        public @Override DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value; }
        }

        /**
         * Initializes a new default instance of the <see cref="BaseDataCollection"/> c;ass
        */
        public BaseDataCollection()
            : this(DateTime.MinValue, Symbol.Empty) {
        }

        /**
         * Initializes a new instance of the <see cref="BaseDataCollection"/> class
        */
         * @param time The time of this data
         * @param symbol A common identifier for all data in this packet
         * @param data The data to add to this collection
        public BaseDataCollection(DateTime time, Symbol symbol, IEnumerable<BaseData> data = null )
            : this(time, time, symbol, data) {
        }

        /**
         * Initializes a new instance of the <see cref="BaseDataCollection"/> class
        */
         * @param time The start time of this data
         * @param endTime The end time of this data
         * @param symbol A common identifier for all data in this packet
         * @param data The data to add to this collection
        public BaseDataCollection(DateTime time, DateTime endTime, Symbol symbol, IEnumerable<BaseData> data = null ) {
            Symbol = symbol;
            Time = time;
            _endTime = endTime;
            Data = data != null ? data.ToList() : new List<BaseData>();
        }

        /**
         * Return a new instance clone of this object, used in fill forward
        */
         * 
         * This base implementation uses reflection to copy all public fields and properties
         * 
        @returns A clone of the current object
        public @Override BaseData Clone() {
            return new BaseDataCollection(Time, EndTime, Symbol, Data);
        }
    }
}
