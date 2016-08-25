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

using System;

package com.quantconnect.lean.Indicators
{
    /**
     * Represents the traditional Weighted Moving Average indicator.  The weight are linearly
     * distributed according to the number of periods in the indicator. 
     * 
     * For example, a 4 period indicator will have a numerator of (4 * window[0]) + (3 * window[1]) + (2 * window[2]) + window[3]
     * and a denominator of 4 + 3 + 2 + 1 = 10
     * 
     * During the warm up period, IsReady will return false, but the LWMA will still be computed correctly because
     * the denominator will be the minimum of Samples factorial or Size factorial and 
     * the computation iterates over that minimum value.
     * 
     * The RollingWindow of inputs is created when the indicator is created.
     * A RollingWindow of LWMAs is not saved.  That is up to the caller.
    */
    public class LinearWeightedMovingAverage : WindowIndicator<IndicatorDataPoint>
    {

        /**
         *     Initializes a new instance of the LinearWeightedMovingAverage class with the specified name and period
        */
         * @param name The name of this indicator
         * @param period The period of the LWMA
        public LinearWeightedMovingAverage( String name, int period)
            : base(name, period) {
        }

        /**
         *     Initializes a new instance of the LinearWeightedMovingAverage class with the default name and period
        */
         * @param period The period of the LWMA
        public LinearWeightedMovingAverage(int period)
            : this( "LWMA" + period, period) {
        }

        /**
         * Computes the next value for this indicator from the given state.
        */
         * @param window The window of data held in this indicator
         * @param input The input value to this indicator on this time step
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            BigDecimal numerator = BigDecimal.ZERO;
            long denominator = 0;

            // The denominator is calculated each time in case the Size is less than the period.
            //  There may be a more efficient way of calculating the factorial.
            for (int i = 0; i <= window.Size; i++) {
                denominator += i;
            }

            // our first data point just return identity
            if( window.Size == 1) {
                return input.Value;
            }
            long index = window.Size;

            // If the indicator is not ready, the LWMA will still be correct
            //  because the numerator has the minimum of the Size (number of
            //  entries or the Samples (the allocated space)
            long minSizeSamples = (long)Math.Min(window.Size, window.Samples);
            for (long i = 0; i < minSizeSamples; i++) {
                BigDecimal x = (index-- * window[(int)i]);
                numerator += x;
            }
            //System.Diagnostics.Debug.WriteLine( String.format( "LWMA = %1$s", (smooth/denominator)));
            return numerator / denominator;
        }


    }
}
