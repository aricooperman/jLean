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

using System.Collections.Generic;

package com.quantconnect.lean.Indicators
{
    /**
     *     Interface type used to pass windows around without worry of external modification
    */
     * <typeparam name="T The type of data in the window</typeparam>
    public interface IReadOnlyWindow<out T> : IEnumerable<T>
    {
        /**
         *     Gets the size of this window
        */
        int Size { get; }

        /**
         *     Gets the current number of elements in this window
        */
        int Count { get; }

        /**
         *     Gets the number of samples that have been added to this window over its lifetime
        */
        BigDecimal Samples { get; }

        /**
         *     Indexes into this window, where index 0 is the most recently
         *     entered value
        */
         * @param i the index, i
        @returns the ith most recent entry
        T this[int i] { get; }

        /**
         *     Gets a value indicating whether or not this window is ready, i.e,
         *     it has been filled to its capacity, this is when the Size==Count
        */
        boolean IsReady { get; }

        /**
         *     Gets the most recently removed item from the window. This is the
         *     piece of data that just 'fell off' as a result of the most recent
         *     add. If no items have been removed, this will throw an exception.
        */
        T MostRecentlyRemoved { get; }
    }
}