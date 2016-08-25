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

package com.quantconnect.lean.Data.Consolidators
{
    /**
     * Represents a type that consumes BaseData instances and fires an event with consolidated
     * and/or aggregated data.
    */
     * <typeparam name="TInput The type consumed by the consolidator</typeparam>
    public abstract class DataConsolidator<TInput> : IDataConsolidator
        where TInput : class, IBaseData
    {
        /**
         * Updates this consolidator with the specified data
        */
         * @param data The new data for the consolidator
        public void Update(BaseData data) {
            typedData = data as TInput;
            if( typedData == null ) {
                throw new ArgumentNullException( "data", "Received type of " + data.GetType().Name + " but expected " + typeof(TInput).Name);
            }
            Update(typedData);
        }

        /**
         * Scans this consolidator to see if it should emit a bar due to time passing
        */
         * @param currentLocalTime The current time in the local time zone (same as <see cref="BaseData.Time"/>)
        public abstract void Scan(DateTime currentLocalTime);

        /**
         * Event handler that fires when a new piece of data is produced
        */
        public event DataConsolidatedHandler DataConsolidated;

        /**
         * Gets the most recently consolidated piece of data. This will be null if this consolidator
         * has not produced any data yet.
        */
        public BaseData Consolidated
        {
            get; private set;
        }

        /**
         * Gets a clone of the data being currently consolidated
        */
        public abstract BaseData WorkingData
        {
            get;
        }

        /**
         * Gets the type consumed by this consolidator
        */
        public Type InputType
        {
            get { return typeof (TInput); }
        }

        /**
         * Gets the type produced by this consolidator
        */
        public abstract Type OutputType
        {
            get;
        }

        /**
         * Updates this consolidator with the specified data. This method is
         * responsible for raising the DataConsolidated event
        */
         * @param data The new data for the consolidator
        public abstract void Update(TInput data);

        /**
         * Event invocator for the DataConsolidated event. This should be invoked
         * by derived classes when they have consolidated a new piece of data.
        */
         * @param consolidated The newly consolidated data
        protected void OnDataConsolidated(BaseData consolidated) {
            handler = DataConsolidated;
            if( handler != null ) handler(this, consolidated);

            // assign the Consolidated property after the event handlers are fired,
            // this allows the event handlers to look at the new consolidated data
            // and the previous consolidated data at the same time without extra bookkeeping
            Consolidated = consolidated;
        }
    }
}