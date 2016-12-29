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

package com.quantconnect.lean.securities;

import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.BaseData;

/**
 * Filters a set of derivative symbols using the underlying price data.
 */
public interface IDerivativeSecurityFilter {
    /**
     * Filters the input set of symbols using the underlying price data
     * @param symbols The derivative symbols to be filtered
     * @param underlying The underlying price data
     * @returns The filtered set of symbols
     */
    Iterable<Symbol> filter( Iterable<Symbol> symbols, BaseData underlying );
}
