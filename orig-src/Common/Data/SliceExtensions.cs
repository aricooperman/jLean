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
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data.Market;
using QuantConnect.Util;

package com.quantconnect.lean.Data
{
    /**
     * Provides extension methods to slice enumerables
    */
    public static class SliceExtensions
    {
        /**
         * Selects into the slice and returns the TradeBars that have data in order
        */
         * @param slices The enumerable of slice
        @returns An enumerable of TradeBars
        public static IEnumerable<TradeBars> TradeBars(this IEnumerable<Slice> slices) {
            return slices.Where(x -> x.Bars.Count > 0).Select(x -> x.Bars);
        }

        /**
         * Selects into the slice and returns the Ticks that have data in order
        */
         * @param slices The enumerable of slice
        @returns An enumerable of Ticks
        public static IEnumerable<Ticks> Ticks(this IEnumerable<Slice> slices) {
            return slices.Where(x -> x.Ticks.Count > 0).Select(x -> x.Ticks);
        }

        /**
         * Gets an enumerable of TradeBar for the given symbol. This method does not verify
         * that the specified symbol points to a TradeBar
        */
         * @param slices The enumerable of slice
         * @param symbol The symbol to retrieve
        @returns An enumerable of TradeBar for the matching symbol, of no TradeBar found for symbol, empty enumerable is returned
        public static IEnumerable<TradeBar> Get(this IEnumerable<Slice> slices, Symbol symbol) {
            return slices.TradeBars().Where(x -> x.ContainsKey(symbol)).Select(x -> x[symbol]);
        }

        /**
         * Gets an enumerable of T for the given symbol. This method does not vify
         * that the specified symbol points to a T
        */
         * <typeparam name="T The data type</typeparam>
         * @param dataDictionaries The data dictionary enumerable to access
         * @param symbol The symbol to retrieve
        @returns An enumerable of T for the matching symbol, if no T is found for symbol, empty enumerable is returned
        public static IEnumerable<T> Get<T>(this IEnumerable<DataMap<T>> dataDictionaries, Symbol symbol)
            where T : BaseData
        {
            return dataDictionaries.Where(x -> x.ContainsKey(symbol)).Select(x -> x[symbol]);
        }

        /**
         * Gets an enumerable of decimals by accessing the specified field on data for the symbol
        */
         * <typeparam name="T The data type</typeparam>
         * @param dataDictionaries An enumerable of data dictionaries
         * @param symbol The symbol to retrieve
         * @param field The field to access
        @returns An enumerable of decimals
        public static IEnumerable<decimal> Get<T>(this IEnumerable<DataMap<T>> dataDictionaries, Symbol symbol, String field) {
            Func<T, decimal> selector;
            if( typeof (DynamicData).IsAssignableFrom(typeof (T))) {
                selector = data =>
                {
                    dyn = (DynamicData) (object) data;
                    return (decimal) dyn.GetProperty(field);
                };
            }
            else if( typeof (T) == typeof (List<Tick>)) {
                // perform the selection on the last tick
                // NOTE: This is a known bug, should be updated to perform the selection on each item in the list
                dataSelector = (Func<Tick, decimal>) ExpressionBuilder.MakePropertyOrFieldSelector(typeof (Tick), field).Compile();
                selector = ticks -> dataSelector(((List<Tick>) (object) ticks).Last());
            }
            else
            {
                selector = (Func<T, decimal>) ExpressionBuilder.MakePropertyOrFieldSelector(typeof (T), field).Compile();
            }

            foreach (dataDictionary in dataDictionaries) {
                T item;
                if( dataDictionary.TryGetValue(symbol, out item)) {
                    yield return selector(item);
                }
            }
        }

        /**
         * Gets the data dictionaries of the requested type in each slice
        */
         * <typeparam name="T The data type</typeparam>
         * @param slices The enumerable of slice
        @returns An enumerable of data dictionary of the requested type
        public static IEnumerable<DataMap<T>> Get<T>(this IEnumerable<Slice> slices)
            where T : BaseData
        {
            return slices.Select(x -> x.Get<T>()).Where(x -> x.Count > 0);
        }

        /**
         * Gets an enumerable of T by accessing the slices for the requested symbol
        */
         * <typeparam name="T The data type</typeparam>
         * @param slices The enumerable of slice
         * @param symbol The symbol to retrieve
        @returns An enumerable of T by accessing each slice for the requested symbol
        public static IEnumerable<T> Get<T>(this IEnumerable<Slice> slices, Symbol symbol)
            where T : BaseData
        {
            return slices.Select(x -> x.Get<T>()).Where(x -> x.ContainsKey(symbol)).Select(x -> x[symbol]);
        }

        /**
         * Gets an enumerable of BigDecimal by accessing the slice for the symbol and then retrieving the specified
         * field on each piece of data
        */
         * @param slices The enumerable of slice
         * @param symbol The symbol to retrieve
         * @param field The field selector used to access the dats
        @returns An enumerable of decimal
        public static IEnumerable<decimal> Get(this IEnumerable<Slice> slices, Symbol symbol, Func<BaseData, decimal> field) {
            foreach (slice in slices) {
                dynamic item;
                if( slice.TryGetValue(symbol, out item)) {
                    if( item is List<Tick>) yield return field(item.Last());
                    else yield return field(item);
                }
            }
        }

        /**
         * Converts the specified enumerable of decimals into a double array
        */
         * @param decimals The enumerable of decimal
        @returns Double array representing the enumerable of decimal
        public static double[] ToDoubleArray(this IEnumerable<decimal> decimals) {
            return decimals.Select(x -> (double) x).ToArray();
        }
    }
}