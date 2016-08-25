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

package com.quantconnect.lean.Indicators
{
    /**
     * Defines the different types of moving averages
    */  
    public enum MovingAverageType
    {
        /**
         * An unweighted, arithmetic mean
        */
        Simple,
        /**
         * The standard exponential moving average, using a smoothing factor of 2/(n+1)
        */
        Exponential,
        /**
         * The standard exponential moving average, using a smoothing factor of 1/n
        */
        Wilders,
        /**
         * A weighted moving average type
        */
        LinearWeightedMovingAverage,
        /**
         * The double exponential moving average
        */
        DoubleExponential,
        /**
         * The triple exponential moving average
        */
        TripleExponential,
        /**
         * The triangular moving average
        */
        Triangular,
        /**
         * The T3 moving average
        */
        T3,
        /**
         * The Kaufman Adaptive Moving Average
        */
        Kama
    }
}
