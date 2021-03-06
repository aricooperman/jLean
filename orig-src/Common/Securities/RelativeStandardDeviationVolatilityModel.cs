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
using MathNet.Numerics.Statistics;
using QuantConnect.Data;
using QuantConnect.Indicators;

package com.quantconnect.lean.Securities
{
    /**
     * Provides an implementation of <see cref="IVolatilityModel"/> that computes the
     * relative standard deviation as the volatility of the security
    */
    public class RelativeStandardDeviationVolatilityModel : IVolatilityModel
    {
        private boolean _needsUpdate;
        private BigDecimal _volatility;
        private DateTime _lastUpdate;
        private final Duration _periodSpan;
        private final object _sync = new object();
        private final RollingWindow<double> _window;

        /**
         * Gets the volatility of the security as a percentage
        */
        public BigDecimal Volatility
        {
            get
            {
                synchronized(_sync) {
                    if( _window.Count < 2) {
                        return BigDecimal.ZERO;
                    }

                    if( _needsUpdate) {
                        _needsUpdate = false;
                        mean = Math.Abs(_window.Mean().SafeDecimalCast());
                        if( mean != BigDecimal.ZERO) {
                            // volatility here is supposed to be a percentage
                            std = _window.StandardDeviation().SafeDecimalCast();
                            _volatility = std/mean;
                        }
                    }
                }
                return _volatility;
            }
        }

        /**
         * Initializes a new instance of the <see cref="RelativeStandardDeviationVolatilityModel"/> class
        */
         * @param periodSpan The time span representing one 'period' length
         * @param periods The nuber of 'period' lengths to wait until updating the value
        public RelativeStandardDeviationVolatilityModel(TimeSpan periodSpan, int periods) {
            if( periods < 2) throw new ArgumentOutOfRangeException( "periods", "'periods' must be greater than or equal to 2.");
            _periodSpan = periodSpan;
            _window = new RollingWindow<double>(periods);
            _lastUpdate = DateTime.MinValue + Duration.ofMilliseconds(periodSpan.TotalMilliseconds*periods);
        }

        /**
         * Updates this model using the new price information in
         * the specified security instance
        */
         * @param security The security to calculate volatility for
         * @param data">
        public void Update(Security security, BaseData data) {
            timeSinceLastUpdate = data.EndTime - _lastUpdate;
            if( timeSinceLastUpdate >= _periodSpan) {
                synchronized(_sync) {
                    _needsUpdate = true;
                    // we purposefully use security.Price for consistency in our reporting
                    // some streams of data will have trade/quote data, so if we just use
                    // data.Value we could be mixing and matching data streams
                    _window.Add((double) security.Price);
                }
                _lastUpdate = data.EndTime;
            }
        }
    }
}