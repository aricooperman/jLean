﻿/*
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

package com.quantconnect.lean.Indicators
{
    /**
     * Represents a type capable of ingesting a piece of data and producing a new piece of data.
     * Indicators can be used to filter and transform data into a new, more informative form.
    */
    public abstract class Indicator : IndicatorBase<IndicatorDataPoint>
    {
        /**
         * Initializes a new instance of the Indicator class using the specified name.
        */
         * @param name The name of this indicator
        protected Indicator( String name) 
            : base(name) {
        }
    }
}