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
     * The possible states returned by <see cref="IndicatorBase{T}.ComputeNextValue"/>
    */
    public enum IndicatorStatus
    {
        /**
         * The indicator successfully calculated a value for the input data
        */
        Success,

        /**
         * The indicator detected an invalid input data point or tradebar
        */
        InvalidInput,

        /**
         * The indicator encountered a math error during calculations
        */
        MathError,
    }
}
