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
    /// This indicator computes the MidPoint (MIDPOINT)
    /// The MidPoint is calculated using the following formula:
    /// MIDPOINT = (Highest Value + Lowest Value) / 2
    */
    public class MidPoint : IndicatorBase<IndicatorDataPoint>
    {
        private final int _period;
        private final Maximum _maximum;
        private final Minimum _minimum;

        /**
        /// Initializes a new instance of the <see cref="MidPoint"/> class using the specified name and period.
        */ 
         * @param name">The name of this indicator
         * @param period">The period of the MIDPOINT
        public MidPoint( String name, int period) 
            : base(name) {
            _period = period;
            _maximum = new Maximum(period);
            _minimum = new Minimum(period);
        }

        /**
        /// Initializes a new instance of the <see cref="MidPoint"/> class using the specified period.
        */ 
         * @param period">The period of the MIDPOINT
        public MidPoint(int period)
            : this( "MIDPOINT" + period, period) {
        }

        /**
        /// Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= _period; }
        }

        /**
        /// Computes the next value of this indicator from the given state
        */
         * @param input">The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IndicatorDataPoint input) {
            _maximum.Update(input);
            _minimum.Update(input);

            return (_maximum + _minimum) / 2;
        }

        /**
        /// Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _maximum.Reset();
            _minimum.Reset();
            base.Reset();
        }
    }
}
