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
using QuantConnect.Indicators;

package com.quantconnect.lean.Securities
{
    /**
    /// Provides an implementation of <see cref="IVolatilityModel"/> that uses an indicator
    /// to compute its value
    */
    /// <typeparam name="T">The indicator's input type</typeparam>
    public class IndicatorVolatilityModel<T> : IVolatilityModel
        where T : BaseData
    {
        private final IIndicator<T> _indicator;
        private final Action<Security, BaseData, IIndicator<T>> _indicatorUpdate;

        /**
        /// Gets the volatility of the security as a percentage
        */
        public BigDecimal Volatility
        {
            get { return _indicator.Current; }
        }

        /**
        /// Initializes a new instance of the <see cref="IVolatilityModel"/> using
        /// the specified <paramref name="indicator"/>. The <paramref name="indicator"/>
        /// is assumed to but updated externally from this model, such as being registered
        /// into the consolidator system.
        */
         * @param indicator">The auto-updating indicator
        public IndicatorVolatilityModel(IIndicator<T> indicator) {
            _indicator = indicator;
        }

        /**
        /// Initializes a new instance of the <see cref="IVolatilityModel"/> using
        /// the specified <paramref name="indicator"/>. The <paramref name="indicator"/>
        /// is assumed to but updated externally from this model, such as being registered
        /// into the consolidator system.
        */
         * @param indicator">The auto-updating indicator
         * @param indicatorUpdate">Function delegate used to update the indicator on each call to <see cref="Update"/>
        public IndicatorVolatilityModel(IIndicator<T> indicator, Action<Security, BaseData, IIndicator<T>> indicatorUpdate) {
            _indicator = indicator;
            _indicatorUpdate = indicatorUpdate;
        }

        /**
        /// Updates this model using the new price information in
        /// the specified security instance
        */
         * @param security">The security to calculate volatility for
         * @param data">The new piece of data for the security
        public void Update(Security security, BaseData data) {
            if( _indicatorUpdate != null ) {
                _indicatorUpdate(security, data, _indicator);
            }
        }
    }
}