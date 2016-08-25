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

package com.quantconnect.lean.Brokerages.InteractiveBrokers
{
    /**
     * Provides the mapping between Lean symbols and InteractiveBrokers symbols.
    */
    public class InteractiveBrokersSymbolMapper : ISymbolMapper
    {
        /**
         * Converts a Lean symbol instance to an InteractiveBrokers symbol
        */
         * @param symbol A Lean symbol instance
        @returns The InteractiveBrokers symbol
        public String GetBrokerageSymbol(Symbol symbol) {
            if( symbol == null || symbol == Symbol.Empty || StringUtils.isBlank(symbol.Value))
                throw new IllegalArgumentException( "Invalid symbol: " + (symbol == null ? "null" : symbol.toString()));

            if( symbol.ID.SecurityType != SecurityType.Forex &&
                symbol.ID.SecurityType != SecurityType.Equity &&
                symbol.ID.SecurityType != SecurityType.Option)
                throw new IllegalArgumentException( "Invalid security type: " + symbol.ID.SecurityType);

            if( symbol.ID.SecurityType == SecurityType.Forex && symbol.Value.Length != 6)
                throw new IllegalArgumentException( "Forex symbol length must be equal to 6: " + symbol.Value);

            return symbol.Value;
        }

        /**
         * Converts an InteractiveBrokers symbol to a Lean symbol instance
        */
         * @param brokerageSymbol The InteractiveBrokers symbol
         * @param securityType The security type
         * @param market The market
        @returns A new Lean Symbol instance
        public Symbol GetLeanSymbol( String brokerageSymbol, SecurityType securityType, String market) {
            if(  String.IsNullOrWhiteSpace(brokerageSymbol))
                throw new IllegalArgumentException( "Invalid symbol: " + brokerageSymbol);

            if( securityType != SecurityType.Forex && securityType != SecurityType.Equity)
                throw new IllegalArgumentException( "Invalid security type: " + securityType);

            return Symbol.Create(brokerageSymbol, securityType, market);
        }
    }
}
