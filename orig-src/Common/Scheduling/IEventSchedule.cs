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

package com.quantconnect.lean.Scheduling
{
    /**
     * Provides the ability to add/remove scheduled events from the real time handler
    */
    public interface IEventSchedule
    {
        /**
         * Adds the specified event to the schedule using the <see cref="ScheduledEvent.Name"/> as a key.
        */
         * @param scheduledEvent The event to be scheduled, including the date/times the event fires and the callback
        void Add(ScheduledEvent scheduledEvent);

        /**
         * Removes the event with the specified name from the schedule
        */
         * @param name The name of the event to be removed
        void Remove( String name);
    }
}