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
     * Represents an indictor capable of tracking the minimum value and how many periods ago it occurred
    */
    public class Minimum : WindowIndicator<IndicatorDataPoint>
    {
        /**
         * The number of periods since the minimum value was encountered
        */
        public int PeriodsSinceMinimum { get; private set; }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= Period; }
        }

        /**
         * Creates a new Minimum indicator with the specified period
        */
         * @param period The period over which to look back
        public Minimum(int period)
            : base( "MIN" + period, period) {
        }

        /**
         * Creates a new Minimum indicator with the specified period
        */
         * @param name The name of this indicator
         * @param period The period over which to look back
        public Minimum( String name, int period)
            : base(name, period) {
        }

         * <inheritdoc />
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            if( Samples == 1 || input.Value <= Current.Value) {
                // our first sample or if we're bigger than our previous indicator value
                // reset the periods since minimum (it's this period) and return the value
                PeriodsSinceMinimum = 0;
                return input.Value;
            }

            if( PeriodsSinceMinimum >= Period - 1) {
                // at this point we need to find a new minimum
                // the window enumerates from most recent to oldest
                // so let's scour the window for the max and it's index

                // this could be done more efficiently if we were to intelligently keep track of the 'next'
                // minimum, so when one falls off, we have the other... but then we would also need the 'next, next'
                // minimum, so on and so forth, for now this works.

                minimum = window.Select((v, i) -> new
                {
                    Value = v,
                    Index = i
                }).OrderBy(x -> x.Value.Value).First();

                PeriodsSinceMinimum = minimum.Index;
                return minimum.Value;
            }

            // if we made it here then we didn't see a new minimum and we haven't reached our period limit,
            // so just increment our periods since minimum and return the same value as we had before
            PeriodsSinceMinimum++;
            return Current;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            PeriodsSinceMinimum = 0;
            base.Reset();
        }
    }
}