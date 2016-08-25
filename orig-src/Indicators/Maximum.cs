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

using System.Linq;

package com.quantconnect.lean.Indicators
{
    /**
     * Represents an indictor capable of tracking the maximum value and how many periods ago it occurred
    */
    public class Maximum : WindowIndicator<IndicatorDataPoint>
    {
        /**
         * The number of periods since the maximum value was encountered
        */
        public int PeriodsSinceMaximum { get; private set; }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= Period; }
        }

        /**
         * Creates a new Maximum indicator with the specified period
        */
         * @param period The period over which to look back
        public Maximum(int period)
            : base( "MAX" + period, period) {
        }

        /**
         * Creates a new Maximum indicator with the specified period
        */
         * @param name The name of this indicator
         * @param period The period over which to look back
        public Maximum( String name, int period)
            : base(name, period) {
        }

         * <inheritdoc />
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            if( Samples == 1 || input.Value >= Current.Value) {
                // our first sample or if we're bigger than our previous indicator value
                // reset the periods since maximum (it's this period) and return the value
                PeriodsSinceMaximum = 0;
                return input.Value;
            }

            if( PeriodsSinceMaximum >= Period - 1) {
                // at this point we need to find a new maximum
                // the window enumerates from most recent to oldest
                // so let's scour the window for the max and it's index

                // this could be done more efficiently if we were to intelligently keep track of the 'next'
                // maximum, so when one falls off, we have the other... but then we would also need the 'next, next' 
                // maximum, so on and so forth, for now this works.

                maximum = window.Select((v, i) -> new
                {
                    Value = v,
                    Index = i
                }).OrderByDescending(x -> x.Value.Value).First();

                PeriodsSinceMaximum = maximum.Index;
                return maximum.Value;
            }

            // if we made it here then we didn't see a new maximum and we haven't reached our period limit,
            // so just increment our periods since maximum and return the same value as we had before
            PeriodsSinceMaximum++;
            return Current;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            PeriodsSinceMaximum = 0;
            base.Reset();
        }
    }
}