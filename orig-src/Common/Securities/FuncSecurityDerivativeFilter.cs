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
using QuantConnect.Data;

package com.quantconnect.lean.Securities
{
    /**
    /// Provides a functional implementation of <see cref="IDerivativeSecurityFilter"/>
    */
    public class FuncSecurityDerivativeFilter : IDerivativeSecurityFilter
    {
        private final Func<IEnumerable<Symbol>, BaseData, IEnumerable<Symbol>> _filter;

        /**
        /// Initializes a new instance of the <see cref="FuncSecurityDerivativeFilter"/> class
        */
         * @param filter">The functional implementation of the <see cref="Filter"/> method
        public FuncSecurityDerivativeFilter(Func<IEnumerable<Symbol>, BaseData, IEnumerable<Symbol>> filter) {
            _filter = filter;
        }

        /**
        /// Filters the input set of symbols using the underlying price data
        */
         * @param symbols">The derivative symbols to be filtered
         * @param underlying">The underlying price data
        @returns The filtered set of symbols
        public IEnumerable<Symbol> Filter(IEnumerable<Symbol> symbols, BaseData underlying) {
            return _filter(symbols, underlying);
        }
    }
}