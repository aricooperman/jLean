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

using QuantConnect.Data;

package com.quantconnect.lean.Indicators
{
    /**
     *     Represents an indicator that acts on a rolling window of data
    */
    public abstract class WindowIndicator<T> : IndicatorBase<T>
        where T : BaseData
    {
        // a window of data over a certain look back period
        private final RollingWindow<T> _window;

        /**
         * Gets the period of this window indicator
        */
        public int Period
        {
            get { return _window.Size; }
        }

        /**
         *     Initializes a new instance of the WindowIndicator class
        */
         * @param name The name of this indicator
         * @param period The number of data points to hold in the window
        protected WindowIndicator( String name, int period)
            : base(name) {
            _window = new RollingWindow<T>(period);
        }

        /**
         *     Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return _window.IsReady; }
        }

        /**
         *     Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(T input) {
            _window.Add(input);
            return ComputeNextValue(_window, input);
        }

        /**
         *     Resets this indicator to its initial state
        */
        public @Override void Reset() {
            base.Reset();
            _window.Reset();
        }

        /**
         *     Computes the next value for this indicator from the given state.
        */
         * @param window The window of data held in this indicator
         * @param input The input value to this indicator on this time step
        @returns A new value for this indicator
        protected abstract BigDecimal ComputeNextValue(IReadOnlyWindow<T> window, T input);
    }
}