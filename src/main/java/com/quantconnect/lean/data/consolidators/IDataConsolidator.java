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

package com.quantconnect.lean.data.consolidators;

import java.time.LocalDateTime;

import com.quantconnect.lean.data.BaseData;

/**
 * Represents a type capable of taking BaseData updates and firing events containing new
 * 'consolidated' data. These types can be used to produce larger bars, or even be used to
 * transform the data before being sent to another component. The most common usage of these
 * types is with indicators.
 */
public interface IDataConsolidator {
   
    /**
     * Event handler type for the IDataConsolidator.DataConsolidated event
     * @param sender The consolidator that fired the event
     * @param consolidated The consolidated piece of data
     */
    @FunctionalInterface
    public interface DataConsolidatedHandler {
        void dataConsolidated( Object sender, BaseData consolidated );
    }
    
    /**
     * Gets the most recently consolidated piece of data. This will be null if this consolidator
     * has not produced any data yet.
     */
    BaseData getConsolidated();

    /**
     * Gets a clone of the data being currently consolidated
     */
    BaseData getWorkingData();

    /**
     * Gets the type consumed by this consolidator
     */
    Class<?> getInputType();

    /**
     * Gets the type produced by this consolidator
     */
    Class<?> getOutputType();

    /**
     * Updates this consolidator with the specified data
     * @param data The new data for the consolidator
     */
    void update( BaseData data);

    /**
     * Scans this consolidator to see if it should emit a bar due to time passing
     * @param currentLocalTime The current time in the local time zone (same as <see cref="BaseData.Time"/>)
     */
    void scan( LocalDateTime currentLocalTime );

//    event DataConsolidatedHandler DataConsolidated;
//     * Event handler that fires when a new piece of data is produced
}