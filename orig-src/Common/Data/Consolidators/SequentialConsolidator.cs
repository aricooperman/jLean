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

package com.quantconnect.lean.Data.Consolidators
{
    /**
     * This consolidator wires up the events on its First and Second consolidators
     * such that data flows from the First to Second consolidator. It's output comes
     * from the Second.
    */
    public class SequentialConsolidator : IDataConsolidator
    {
        /**
         * Gets the first consolidator to receive data
        */
        public IDataConsolidator First
        {
            get; private set;
        }

        /**
         * Gets the second consolidator that ends up receiving data produced
         * by the first
        */
        public IDataConsolidator Second
        {
            get; private set;
        }

        /**
         * Gets the most recently consolidated piece of data. This will be null if this consolidator
         * has not produced any data yet.
         * 
         * For a SequentialConsolidator, this is the output from the 'Second' consolidator.
        */
        public BaseData Consolidated
        {
            get { return Second.Consolidated; }
        }

        /**
         * Gets a clone of the data being currently consolidated
        */
        public BaseData WorkingData
        {
            get { return Second.WorkingData; }
        }

        /**
         * Gets the type consumed by this consolidator
        */
        public Class InputType
        {
            get { return First.InputType; }
        }

        /**
         * Gets the type produced by this consolidator
        */
        public Class OutputType
        {
            get { return Second.OutputType; }
        }

        /**
         * Updates this consolidator with the specified data
        */
         * @param data The new data for the consolidator
        public void Update(BaseData data) {
            First.Update(data);
        }

        /**
         * Scans this consolidator to see if it should emit a bar due to time passing
        */
         * @param currentLocalTime The current time in the local time zone (same as <see cref="BaseData.Time"/>)
        public void Scan(DateTime currentLocalTime) {
            First.Scan(currentLocalTime);
        }

        /**
         * Event handler that fires when a new piece of data is produced
        */
        public event DataConsolidatedHandler DataConsolidated;

        /**
         * Creates a new consolidator that will pump date through the first, and then the output
         * of the first into the second. This enables 'wrapping' or 'composing' of consolidators
        */
         * @param first The first consolidator to receive data
         * @param second The consolidator to receive first's output
        public SequentialConsolidator(IDataConsolidator first, IDataConsolidator second) {
            if( !second.InputType.IsAssignableFrom(first.OutputType)) {
                throw new IllegalArgumentException( "first.OutputType must equal second.OutputType!");
            }
            First = first;
            Second = second;

            // wire up the second one to get data from the first
            first.DataConsolidated += (sender, consolidated) -> second.Update(consolidated);
            
            // wire up the second one's events to also fire this consolidator's event so consumers
            // can attach
            second.DataConsolidated += (sender, consolidated) -> OnDataConsolidated(consolidated);
        }

        /**
         * Event invocator for the DataConsolidated event. This should be invoked
         * by derived classes when they have consolidated a new piece of data.
        */
         * @param consolidated The newly consolidated data
        protected void OnDataConsolidated(BaseData consolidated) {
            handler = DataConsolidated;
            if( handler != null ) handler(this, consolidated);
        }
    }
}
