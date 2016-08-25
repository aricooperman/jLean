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
using QuantConnect.Securities;

package com.quantconnect.lean.Data.UniverseSelection
{
    /**
     * Defines settings required when adding a subscription
    */
    public class UniverseSettings
    {
        /**
         * The resolution to be used
        */
        public Resolution Resolution;

        /**
         * The leverage to be used
        */
        public BigDecimal Leverage;

        /**
         * True to fill data forward, false otherwise
        */
        public boolean FillForward;

        /**
         * True to allow extended market hours data, false otherwise
        */
        public boolean ExtendedMarketHours;

        /**
         * Defines the minimum amount of time a security must be in
         * the universe before being removed.
        */
        public Duration MinimumTimeInUniverse;

        /**
         * Initializes a new instance of the <see cref="UniverseSettings"/> class
        */
         * @param resolution The resolution
         * @param leverage The leverage to be used
         * @param fillForward True to fill data forward, false otherwise
         * @param extendedMarketHours True to allow exended market hours data, false otherwise
         * @param minimumTimeInUniverse Defines the minimum amount of time a security must remain in the universe before being removed
        public UniverseSettings(Resolution resolution, BigDecimal leverage, boolean fillForward, boolean extendedMarketHours, Duration minimumTimeInUniverse) {
            Resolution = resolution;
            Leverage = leverage;
            FillForward = fillForward;
            ExtendedMarketHours = extendedMarketHours;
            MinimumTimeInUniverse = minimumTimeInUniverse;
        }
    }
}