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
    /// <summary>
    /// Provides the mapping between Lean symbols and FXCM symbols.
    /// </summary>
    public class FxcmSymbolMapper : ISymbolMapper
    {
        /// <summary>
        /// Helper class to allow collection initializer on a List of tuples
        /// </summary>
        private class TupleList<T1, T2> : List<Tuple<T1, T2>>
        {
            public void Add(T1 item1, T2 item2) {
                Add(new Tuple<T1, T2>(item1, item2));
            }
        }

        /// <summary>
        /// The list of mappings from FXCM symbols to Lean symbols.
        /// </summary>
        /// <remarks>T1 is FXCM symbol, T2 is Lean symbol</remarks>
        private static readonly TupleList<String,String> FxcmSymbolMappings = new TupleList<String,String>
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

        private static readonly Map<String,String> MapFxcmToLean = new Map<String,String>();
        private static readonly Map<String,String> MapLeanToFxcm = new Map<String,String>();

        /// <summary>
        /// The list of known FXCM currencies.
        /// </summary>
        private static readonly HashSet<String> KnownCurrencies = new HashSet<String>
        {
            "AUD", "CAD", "CHF", "CNH", "EUR", "GBP", "HKD", "JPY", "MXN", "NOK", "NZD", "SEK", "TRY", "USD", "ZAR"
        };

        /// <summary>
        /// Static constructor for the <see cref="FxcmSymbolMapper"/> class
        /// </summary>
        static FxcmSymbolMapper() {
            foreach (mapping in FxcmSymbolMappings) {
                MapFxcmToLean[mapping.Item1] = mapping.Item2;
                MapLeanToFxcm[mapping.Item2] = mapping.Item1;
            }
        }

        /// <summary>
        /// Converts a Lean symbol instance to an FXCM symbol
        /// </summary>
        /// <param name="symbol">A Lean symbol instance</param>
        /// <returns>The FXCM symbol</returns>
        public String GetBrokerageSymbol(Symbol symbol) {
            if( symbol == null || symbol == Symbol.Empty || string.IsNullOrWhiteSpace(symbol.Value))
                throw new ArgumentException( "Invalid symbol: " + (symbol == null ? "null" : symbol.toString()));

            if( symbol.ID.SecurityType != SecurityType.Forex && symbol.ID.SecurityType != SecurityType.Cfd)
                throw new ArgumentException( "Invalid security type: " + symbol.ID.SecurityType);

            brokerageSymbol = ConvertLeanSymbolToFxcmSymbol(symbol.Value);

            if( !IsKnownBrokerageSymbol(brokerageSymbol))
                throw new ArgumentException( "Unknown symbol: " + symbol.Value);

            return brokerageSymbol;
        }

        /// <summary>
        /// Converts an FXCM symbol to a Lean symbol instance
        /// </summary>
        /// <param name="brokerageSymbol">The FXCM symbol</param>
        /// <param name="securityType">The security type</param>
        /// <param name="market">The market</param>
        /// <returns>A new Lean Symbol instance</returns>
        public Symbol GetLeanSymbol( String brokerageSymbol, SecurityType securityType, String market) {
            if(  String.IsNullOrWhiteSpace(brokerageSymbol))
                throw new ArgumentException( "Invalid FXCM symbol: " + brokerageSymbol);

            if( !IsKnownBrokerageSymbol(brokerageSymbol))
                throw new ArgumentException( "Unknown FXCM symbol: " + brokerageSymbol);

            if( securityType != SecurityType.Forex && securityType != SecurityType.Cfd)
                throw new ArgumentException( "Invalid security type: " + securityType);

            if( market != Market.FXCM)
                throw new ArgumentException( "Invalid market: " + market);

            return Symbol.Create(ConvertFxcmSymbolToLeanSymbol(brokerageSymbol), GetBrokerageSecurityType(brokerageSymbol), Market.FXCM);
        }

        /// <summary>
        /// Returns the security type for an FXCM symbol
        /// </summary>
        /// <param name="brokerageSymbol">The FXCM symbol</param>
        /// <returns>The security type</returns>
        public SecurityType GetBrokerageSecurityType( String brokerageSymbol) {
            tokens = brokerageSymbol.split('/');
            return tokens.Length == 2 && KnownCurrencies.Contains(tokens[0]) && KnownCurrencies.Contains(tokens[1])
                ? SecurityType.Forex
                : SecurityType.Cfd;
        }

        /// <summary>
        /// Returns the security type for a Lean symbol
        /// </summary>
        /// <param name="leanSymbol">The Lean symbol</param>
        /// <returns>The security type</returns>
        public SecurityType GetLeanSecurityType( String leanSymbol) {
            String fxcmSymbol;
            if( !MapLeanToFxcm.TryGetValue(leanSymbol, out fxcmSymbol))
                throw new ArgumentException( "Unknown Lean symbol: " + leanSymbol);

            return GetBrokerageSecurityType(fxcmSymbol);
        }

        /// <summary>
        /// Checks if the symbol is supported by FXCM
        /// </summary>
        /// <param name="brokerageSymbol">The FXCM symbol</param>
        /// <returns>True if FXCM supports the symbol</returns>
        public boolean IsKnownBrokerageSymbol( String brokerageSymbol) {
            return brokerageSymbol != null && MapFxcmToLean.ContainsKey(brokerageSymbol);
        }

        /// <summary>
        /// Checks if the symbol is supported by FXCM
        /// </summary>
        /// <param name="symbol">The Lean symbol</param>
        /// <returns>True if FXCM supports the symbol</returns>
        public boolean IsKnownLeanSymbol(Symbol symbol) {
            if( symbol == null || string.IsNullOrWhiteSpace(symbol.Value))
                return false;

            fxcmSymbol = ConvertLeanSymbolToFxcmSymbol(symbol.Value);

            return MapFxcmToLean.ContainsKey(fxcmSymbol) && GetBrokerageSecurityType(fxcmSymbol) == symbol.ID.SecurityType;
        }

        /// <summary>
        /// Converts an FXCM symbol to a Lean symbol string
        /// </summary>
        public static String ConvertFxcmSymbolToLeanSymbol( String fxcmSymbol) {
            String leanSymbol;
            return MapFxcmToLean.TryGetValue(fxcmSymbol, out leanSymbol) ? leanSymbol : string.Empty;
        }

        /// <summary>
        /// Converts a Lean symbol String to an FXCM symbol
        /// </summary>
        private static String ConvertLeanSymbolToFxcmSymbol( String leanSymbol) {
            String fxcmSymbol;
            return MapLeanToFxcm.TryGetValue(leanSymbol, out fxcmSymbol) ? fxcmSymbol : string.Empty;
        }

    }
}
