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

using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Data.Auxiliary;
using QuantConnect.Data.UniverseSelection;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Enumerators
{
    /**
     * Aggregates an enumerator into <see cref="OptionChainUniverseDataCollection"/> instances
    */
    public class OptionChainUniverseDataCollectionAggregatorEnumerator : BaseDataCollectionAggregatorEnumerator<OptionChainUniverseDataCollection>
    {
        /**
         * Initializes a new instance of the <see cref="OptionChainUniverseDataCollectionAggregatorEnumerator"/> class
        */
         * @param enumerator The enumerator to aggregate
         * @param symbol The output data's symbol
        public OptionChainUniverseDataCollectionAggregatorEnumerator(IEnumerator<BaseData> enumerator, Symbol symbol)
            : base(enumerator, symbol) {
        }

        /**
         * Adds the specified instance of <see cref="BaseData"/> to the current collection
        */
         * @param collection The collection to be added to
         * @param current The data to be added
        protected @Override void Add(OptionChainUniverseDataCollection collection, BaseData current) {
            AddSingleItem(collection, current);
        }

        private static void AddSingleItem(OptionChainUniverseDataCollection collection, BaseData current) {
            baseDataCollection = current as BaseDataCollection;
            if( baseDataCollection != null ) {
                foreach (data in baseDataCollection.Data) {
                    AddSingleItem(collection, data);
                }
                return;
            }

            if( current is ZipEntryName) {
                collection.Data.Add(current);
                return;
            }

            collection.Underlying = current;
        }
    }
}