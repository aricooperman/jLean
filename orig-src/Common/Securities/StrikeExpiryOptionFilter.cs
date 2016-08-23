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
 *
*/

using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Util;

package com.quantconnect.lean.Securities
{
    /**
    /// Provides an implementation of <see cref="IDerivativeSecurityFilter"/> for use in selecting
    /// options contracts based on a range of strikes and expiries
    */
    public class StrikeExpiryOptionFilter : IDerivativeSecurityFilter
    {
        private BigDecimal _strikeSize;
        private DateTime _strikeSizeResolveDate;

        private final int _minStrike;
        private final int _maxStrike;
        private final Duration _minExpiry;
        private final Duration _maxExpiry;

        /**
        /// Initializes a new instance of the <see cref="StrikeExpiryOptionFilter"/> class
        */
         * @param minStrike">The minimum strike relative to the underlying price, for example, -1 would filter out contracts further than 1 strike below market price
         * @param maxStrike">The maximum strike relative to the underlying price, for example, +1 would filter out contracts further than 1 strike above market price
         * @param minExpiry">The minium time until expiry, for example, 7 days would filter out contracts expiring sooner than 7 days
         * @param maxExpiry">The maximum time until expiry, for example, 30 days would filter out contracts expriring later than 30 days
        public StrikeExpiryOptionFilter(int minStrike, int maxStrike, Duration minExpiry, Duration maxExpiry) {
            _minStrike = minStrike;
            _maxStrike = maxStrike;
            _minExpiry = minExpiry;
            _maxExpiry = maxExpiry;

            // prevent parameter mistakes that would prevent all contracts from coming through
            if( maxStrike < minStrike) throw new ArgumentException( "maxStrike must be greater than minStrike");
            if( maxExpiry < minExpiry) throw new ArgumentException( "maxExpiry must be greater than minExpiry");

            // protect from overflow on additions
            if( _maxExpiry > Time.MaxTimeSpan) _maxExpiry = Time.MaxTimeSpan;
        }

        /**
        /// Filters the input set of symbols using the underlying price data
        */
         * @param symbols">The derivative symbols to be filtered
         * @param underlying">The underlying price data
        @returns The filtered set of symbols
        public IEnumerable<Symbol> Filter(IEnumerable<Symbol> symbols, BaseData underlying) {
            // we can't properly apply this filter without knowing the underlying price
            // so in the event we're missing the underlying, just skip the filtering step
            if( underlying == null ) {
                return symbols;
            }

            if( underlying.Time.Date != _strikeSizeResolveDate) {
                // each day we need to recompute the strike size
                symbols = symbols.ToList();
                uniqueStrikes = symbols.DistinctBy(x -> x.ID.StrikePrice).OrderBy(x -> x.ID.StrikePrice).ToList();
                _strikeSize = uniqueStrikes.Zip(uniqueStrikes.Skip(1), (l, r) -> r.ID.StrikePrice - l.ID.StrikePrice).DefaultIfEmpty(5m).Min();
                _strikeSizeResolveDate = underlying.Time.Date;
            }

            // compute the bounds, no need to worry about rounding and such
            minPrice = underlying.Price + _minStrike*_strikeSize;
            maxPrice = underlying.Price + _maxStrike*_strikeSize;
            minExpiry = underlying.Time.Date + _minExpiry;
            maxExpiry = underlying.Time.Date + _maxExpiry;

            // ReSharper disable once PossibleMultipleEnumeration - ReSharper is wrong here due to the ToList call
            return from symbol in symbols
                   let contract = symbol.ID
                   where contract.StrikePrice >= minPrice
                      && contract.StrikePrice <= maxPrice
                      && contract.Date >= minExpiry
                      && contract.Date <= maxExpiry
                   select symbol;
        }
    }
}