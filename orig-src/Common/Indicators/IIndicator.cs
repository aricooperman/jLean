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
using QuantConnect.Data;

package com.quantconnect.lean.Indicators
{
    /**
    /// Represents an indicator that can receive data updates and emit events when the value of
    /// the indicator has changed.
    */
    /// <typeparam name="T">The indicators input data type for the <see cref="Update"/> method</typeparam>
    public interface IIndicator<T> : IComparable<IIndicator<T>>, IComparable
        where T : BaseData
    {
        /**
        /// Event handler that fires after this indicator is updated
        */
        event IndicatorUpdatedHandler Updated;

        /**
        /// Gets a name for this indicator
        */
        String Name { get; }

        /**
        /// Gets a flag indicating when this indicator is ready and fully initialized
        */
        boolean IsReady { get; }

        /**
        /// Gets the current state of this indicator. If the state has not been updated
        /// then the time on the value will equal DateTime.MinValue.
        */
        IndicatorDataPoint Current { get; }

        /**
        /// Gets the number of samples processed by this indicator
        */
        long Samples { get; }

        /**
        /// Updates the state of this indicator with the given value and returns true
        /// if this indicator is ready, false otherwise
        */
         * @param input">The value to use to update this indicator
        @returns True if this indicator is ready, false otherwise
        boolean Update(T input);

        /**
        /// Resets this indicator to its initial state
        */
        void Reset();
    }
}