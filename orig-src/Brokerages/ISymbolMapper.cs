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

package com.quantconnect.lean.Brokerages
{
    /**
     * Provides the mapping between Lean symbols and brokerage specific symbols.
    */
    public interface ISymbolMapper
    {
        /**
         * Converts a Lean symbol instance to a brokerage symbol
        */
         * @param symbol A Lean symbol instance
        @returns The brokerage symbol
        String GetBrokerageSymbol(Symbol symbol);

        /**
         * Converts a brokerage symbol to a Lean symbol instance
        */
         * @param brokerageSymbol The brokerage symbol
         * @param securityType The security type
         * @param market The market
        @returns A new Lean Symbol instance
        Symbol GetLeanSymbol( String brokerageSymbol, SecurityType securityType, String market);
    }
}
