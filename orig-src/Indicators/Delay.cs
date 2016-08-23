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
    /// An indicator that delays its input for a certain period
    */
    public class Delay : WindowIndicator<IndicatorDataPoint>
    {
        /**
        /// Creates a new Delay indicator that delays its input by the specified period
        */
         * @param period">The period to delay input, must be greater than zero
        public Delay(int period)
            : this( "DELAY" + period, period) {
            
        }

        /**
        /// Creates a new Delay indicator that delays its input by the specified period
        */
         * @param name">Name of the delay window indicator
         * @param period">The period to delay input, must be greater than zero
        public Delay( String name, int period) 
            : base(name, period) {
        }

        /**
        ///     Computes the next value for this indicator from the given state.
        */
         * @param window">The window of data held in this indicator
         * @param input">The input value to this indicator on this time step
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            if( !window.IsReady) {
                // grab the initial value until we're ready
                return window[window.Count - 1];
            }

            return window.MostRecentlyRemoved;
        }
    }
}
