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

package com.quantconnect.lean.ToolBox
{
    /**
     * Represents a single instrument as listed in the file instruments.txt
    */
    public class LeanInstrument
    {
        /**
         * The symbol of the instrument
        */
        public String Symbol { get; set; }

        /**
         * The name/description of the instrument
        */
        public String Name { get; set; }

        /**
         * The instrument type
        */
        public SecurityType Class { get; set; }

        /**
         * The point value
        */
        public double PointValue { get; set; }
    }
}