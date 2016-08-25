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

using QuantConnect.Data.UniverseSelection;

package com.quantconnect.lean.Algorithm
{
    /**
     * Provides helpers for defining universes in algorithms
    */
    public class UniverseDefinitions
    {
        /**
         * Specifies that universe selection should not make changes on this iteration
        */
        public Universe.UnchangedUniverse Unchanged
        {
            get { return Universe.Unchanged; }
        }

        /**
         * Gets a helper that provides methods for creating universes based on daily dollar volumes
        */
        public DollarVolumeUniverseDefinitions DollarVolume
        {
            get; private set;
        }

        /**
         * Initializes a new instance of the <see cref="UniverseDefinitions"/> class
        */
         * @param algorithm The algorithm instance, used for obtaining the default <see cref="UniverseSettings"/>
        public UniverseDefinitions(QCAlgorithm algorithm) {
            DollarVolume = new DollarVolumeUniverseDefinitions(algorithm);
        }
    }
}
