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

using System;
using QuantConnect.Data;

package com.quantconnect.lean.Indicators
{
    /**
    /// The functional indicator is used to lift any function into an indicator. This can be very useful
    /// when trying to combine output of several indicators, or for expression a mathematical equation
    */
    /// <typeparam name="T">The input type for this indicator</typeparam>
    public class FunctionalIndicator<T> : IndicatorBase<T>
        where T : BaseData
    {
        /**function implementation of the IndicatorBase.IsReady property</summary>
        private final Func<IndicatorBase<T>, bool> _isReady;
        /**Action used to reset this indicator completely along with any indicators this one is dependent on</summary>
        private final Action _reset;
        /**function implementation of the IndicatorBase.ComputeNextValue method</summary>
        private final Func<T, decimal> _computeNextValue;

        /**
        /// Creates a new FunctionalIndicator using the specified functions as its implementation.
        */
         * @param name">The name of this indicator
         * @param computeNextValue">A function accepting the input value and returning this indicator's output value
         * @param isReady">A function accepting this indicator and returning true if the indicator is ready, false otherwise
        public FunctionalIndicator( String name, Func<T, decimal> computeNextValue, Func<IndicatorBase<T>, bool> isReady)
            : base(name) {
            _computeNextValue = computeNextValue;
            _isReady = isReady;
        }

        /**
        /// Creates a new FunctionalIndicator using the specified functions as its implementation.
        */
         * @param name">The name of this indicator
         * @param computeNextValue">A function accepting the input value and returning this indicator's output value
         * @param isReady">A function accepting this indicator and returning true if the indicator is ready, false otherwise
         * @param reset">Function called to reset this indicator and any indicators this is dependent on
        public FunctionalIndicator( String name, Func<T, decimal> computeNextValue, Func<IndicatorBase<T>, bool> isReady, Action reset)
            : base(name) {
            _computeNextValue = computeNextValue;
            _isReady = isReady;
            _reset = reset;
        }

        /**
        /// Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return _isReady(this); }
        }

        /**
        /// Computes the next value of this indicator from the given state
        */
         * @param input">The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(T input) {
            return _computeNextValue(input);
        }

        /**
        /// Resets this indicator to its initial state, optionally using the reset action passed via the constructor
        */
        public @Override void Reset() {
            if( _reset != null ) {
                // if a reset function was specified then use that
                _reset.Invoke();
            }
            base.Reset();
        }
    }
}