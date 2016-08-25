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
using System.Collections.Generic;

package com.quantconnect.lean.Brokerages.Fxcm
{
    /**
     * Provides the mapping between Lean symbols and FXCM symbols.
    */
    public class FxcmSymbolMapper : ISymbolMapper
    {
        /**
         * Helper class to allow collection initializer on a List of tuples
        */
        private class TupleList<T1, T2> : List<Tuple<T1, T2>>
        {
            public void Add(T1 item1, T2 item2) {
                Add(new Tuple<T1, T2>(item1, item2));
            }
        }

        /**
         * The list of mappings from FXCM symbols to Lean symbols.
        */
         * T1 is FXCM symbol, T2 is Lean symbol
        private static final TupleList<String,String> FxcmSymbolMappings = new TupleList<String,String>
        {
            { "AUD/CAD", "AUDCAD" },
            { "AUD/CHF", "AUDCHF" },
            { "AUD/JPY", "AUDJPY" },
            { "AUD/NZD", "AUDNZD" },
            { "AUD/USD", "AUDUSD" },
            { "AUS200", "AU200AUD" },
            { "Bund", "DE10YBEUR" },
            { "CAD/CHF", "CADCHF" },
            { "CAD/JPY", "CADJPY" },
            { "CHF/JPY", "CHFJPY" },
            { "Copper", "XCUUSD" },
            { "ESP35", "ES35EUR" },
            { "EUR/AUD", "EURAUD" },
            { "EUR/CAD", "EURCAD" },
            { "EUR/CHF", "EURCHF" },
            { "EUR/GBP", "EURGBP" },
            { "EUR/JPY", "EURJPY" },
            { "EUR/NOK", "EURNOK" },
            { "EUR/NZD", "EURNZD" },
            { "EUR/SEK", "EURSEK" },
            { "EUR/TRY", "EURTRY" },
            { "EUR/USD", "EURUSD" },
            { "EUSTX50", "EU50EUR" },
            { "FRA40", "FR40EUR" },
            { "GBP/AUD", "GBPAUD" },
            { "GBP/CAD", "GBPCAD" },
            { "GBP/CHF", "GBPCHF" },
            { "GBP/JPY", "GBPJPY" },
            { "GBP/NZD", "GBPNZD" },
            { "GBP/USD", "GBPUSD" },
            { "GER30", "DE30EUR" },
            { "HKG33", "HK33HKD" },
            { "ITA40", "IT40EUR" },
            { "JPN225", "JP225JPY" },
            { "NAS100", "NAS100USD" },
            { "NGAS", "NATGASUSD" },
            { "NZD/CAD", "NZDCAD" },
            { "NZD/CHF", "NZDCHF" },
            { "NZD/JPY", "NZDJPY" },
            { "NZD/USD", "NZDUSD" },
            { "SPX500", "SPX500USD" },
            { "SUI20", "CH20CHF" },
            { "TRY/JPY", "TRYJPY" },
            { "UK100", "UK100GBP" },
            { "UKOil", "BCOUSD" },
            { "US30", "US30USD" },
            { "USD/MXN", "USDMXN" },
            { "USDOLLAR", "DXYUSD" },
            { "USD/CAD", "USDCAD" },
            { "USD/CHF", "USDCHF" },
            { "USD/CNH", "USDCNY" },
            { "USD/HKD", "USDHKD" },
            { "USD/JPY", "USDJPY" },
            { "USD/NOK", "USDNOK" },
            { "USD/SEK", "USDSEK" },
            { "USD/TRY", "USDTRY" },
            { "USD/ZAR", "USDZAR" },
            { "USOil", "WTICOUSD" },
            { "XAG/USD", "XAGUSD" },
            { "XAU/USD", "XAUUSD" },
            { "XPD/USD", "XPDUSD" },
            { "XPT/USD", "XPTUSD" },
            { "ZAR/JPY", "ZARJPY" }
        };

        private static final Map<String,String> MapFxcmToLean = new Map<String,String>();
        private static final Map<String,String> MapLeanToFxcm = new Map<String,String>();

        /**
         * The list of known FXCM currencies.
        */
        private static final HashSet<String> KnownCurrencies = new HashSet<String>
        {
            "AUD", "CAD", "CHF", "CNH", "EUR", "GBP", "HKD", "JPY", "MXN", "NOK", "NZD", "SEK", "TRY", "USD", "ZAR"
        };

        /**
         * Static constructor for the <see cref="FxcmSymbolMapper"/> class
        */
        static FxcmSymbolMapper() {
            foreach (mapping in FxcmSymbolMappings) {
                MapFxcmToLean[mapping.Item1] = mapping.Item2;
                MapLeanToFxcm[mapping.Item2] = mapping.Item1;
            }
        }

        /**
         * Converts a Lean symbol instance to an FXCM symbol
        */
         * @param symbol A Lean symbol instance
        @returns The FXCM symbol
        public String GetBrokerageSymbol(Symbol symbol) {
            if( symbol == null || symbol == Symbol.Empty || StringUtils.isBlank(symbol.Value))
                throw new IllegalArgumentException( "Invalid symbol: " + (symbol == null ? "null" : symbol.toString()));

            if( symbol.ID.SecurityType != SecurityType.Forex && symbol.ID.SecurityType != SecurityType.Cfd)
                throw new IllegalArgumentException( "Invalid security type: " + symbol.ID.SecurityType);

            brokerageSymbol = ConvertLeanSymbolToFxcmSymbol(symbol.Value);

            if( !IsKnownBrokerageSymbol(brokerageSymbol))
                throw new IllegalArgumentException( "Unknown symbol: " + symbol.Value);

            return brokerageSymbol;
        }

        /**
         * Converts an FXCM symbol to a Lean symbol instance
        */
         * @param brokerageSymbol The FXCM symbol
         * @param securityType The security type
         * @param market The market
        @returns A new Lean Symbol instance
        public Symbol GetLeanSymbol( String brokerageSymbol, SecurityType securityType, String market) {
            if(  String.IsNullOrWhiteSpace(brokerageSymbol))
                throw new IllegalArgumentException( "Invalid FXCM symbol: " + brokerageSymbol);

            if( !IsKnownBrokerageSymbol(brokerageSymbol))
                throw new IllegalArgumentException( "Unknown FXCM symbol: " + brokerageSymbol);

            if( securityType != SecurityType.Forex && securityType != SecurityType.Cfd)
                throw new IllegalArgumentException( "Invalid security type: " + securityType);

            if( market != Market.FXCM)
                throw new IllegalArgumentException( "Invalid market: " + market);

            return Symbol.Create(ConvertFxcmSymbolToLeanSymbol(brokerageSymbol), GetBrokerageSecurityType(brokerageSymbol), Market.FXCM);
        }

        /**
         * Returns the security type for an FXCM symbol
        */
         * @param brokerageSymbol The FXCM symbol
        @returns The security type
        public SecurityType GetBrokerageSecurityType( String brokerageSymbol) {
            tokens = brokerageSymbol.split('/');
            return tokens.Length == 2 && KnownCurrencies.Contains(tokens[0]) && KnownCurrencies.Contains(tokens[1])
                ? SecurityType.Forex
                : SecurityType.Cfd;
        }

        /**
         * Returns the security type for a Lean symbol
        */
         * @param leanSymbol The Lean symbol
        @returns The security type
        public SecurityType GetLeanSecurityType( String leanSymbol) {
            String fxcmSymbol;
            if( !MapLeanToFxcm.TryGetValue(leanSymbol, out fxcmSymbol))
                throw new IllegalArgumentException( "Unknown Lean symbol: " + leanSymbol);

            return GetBrokerageSecurityType(fxcmSymbol);
        }

        /**
         * Checks if the symbol is supported by FXCM
        */
         * @param brokerageSymbol The FXCM symbol
        @returns True if FXCM supports the symbol
        public boolean IsKnownBrokerageSymbol( String brokerageSymbol) {
            return brokerageSymbol != null && MapFxcmToLean.ContainsKey(brokerageSymbol);
        }

        /**
         * Checks if the symbol is supported by FXCM
        */
         * @param symbol The Lean symbol
        @returns True if FXCM supports the symbol
        public boolean IsKnownLeanSymbol(Symbol symbol) {
            if( symbol == null || StringUtils.isBlank(symbol.Value))
                return false;

            fxcmSymbol = ConvertLeanSymbolToFxcmSymbol(symbol.Value);

            return MapFxcmToLean.ContainsKey(fxcmSymbol) && GetBrokerageSecurityType(fxcmSymbol) == symbol.ID.SecurityType;
        }

        /**
         * Converts an FXCM symbol to a Lean symbol string
        */
        public static String ConvertFxcmSymbolToLeanSymbol( String fxcmSymbol) {
            String leanSymbol;
            return MapFxcmToLean.TryGetValue(fxcmSymbol, out leanSymbol) ? leanSymbol : string.Empty;
        }

        /**
         * Converts a Lean symbol String to an FXCM symbol
        */
        private static String ConvertLeanSymbolToFxcmSymbol( String leanSymbol) {
            String fxcmSymbol;
            return MapLeanToFxcm.TryGetValue(leanSymbol, out fxcmSymbol) ? fxcmSymbol : string.Empty;
        }

    }
}
