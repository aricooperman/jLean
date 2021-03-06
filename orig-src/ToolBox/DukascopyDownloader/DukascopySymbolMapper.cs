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
using System.Collections.Generic;
using QuantConnect.Brokerages;

package com.quantconnect.lean.ToolBox.DukascopyDownloader
{
    /**
     * Provides the mapping between Lean symbols and Dukascopy symbols.
    */
    public class DukascopySymbolMapper : ISymbolMapper
    {
        /**
         * Helper class to allow collection initializer on a List of tuples
        */
        private class TupleList<T1, T2, T3> : List<Tuple<T1, T2, T3>>
        {
            public void Add(T1 item1, T2 item2, T3 item3) {
                Add(new Tuple<T1, T2, T3>(item1, item2, item3));
            }
        }

        /**
         * The list of mappings from Dukascopy symbols to Lean symbols.
        */
         * T1 is Dukascopy symbol, T2 is Lean symbol, T3 is point value (used by downloader)
        private static final TupleList<String, string,Integer> DukascopySymbolMappings = new TupleList<String, string,Integer>
        {
            { "AUDCAD", "AUDCAD", 100000 },
            { "AUDCHF", "AUDCHF", 100000 },
            { "AUDJPY", "AUDJPY", 1000 },
            { "AUDNZD", "AUDNZD", 100000 },
            { "AUDSGD", "AUDSGD", 100000 },
            { "AUDUSD", "AUDUSD", 100000 },
            { "AUSIDXAUD", "AU200AUD", 1000 },
            { "BRAIDXBRL", "BRIDXBRL", 1000 },
            { "BRENTCMDUSD", "BCOUSD", 1000 },
            { "CADCHF", "CADCHF", 100000 },
            { "CADHKD", "CADHKD", 100000 },
            { "CADJPY", "CADJPY", 1000 },
            { "CHEIDXCHF", "CH20CHF", 1000 },
            { "CHFJPY", "CHFJPY", 1000 },
            { "CHFPLN", "CHFPLN", 100000 },
            { "CHFSGD", "CHFSGD", 100000 },
            { "COPPERCMDUSD", "XCUUSD", 1000 },
            { "DEUIDXEUR", "DE30EUR", 1000 },
            { "ESPIDXEUR", "ES35EUR", 1000 },
            { "EURAUD", "EURAUD", 100000 },
            { "EURCAD", "EURCAD", 100000 },
            { "EURCHF", "EURCHF", 100000 },
            { "EURDKK", "EURDKK", 100000 },
            { "EURGBP", "EURGBP", 100000 },
            { "EURHKD", "EURHKD", 100000 },
            { "EURHUF", "EURHUF", 1000 },
            { "EURJPY", "EURJPY", 1000 },
            { "EURMXN", "EURMXN", 100000 },
            { "EURNOK", "EURNOK", 100000 },
            { "EURNZD", "EURNZD", 100000 },
            { "EURPLN", "EURPLN", 100000 },
            { "EURRUB", "EURRUB", 100000 },
            { "EURSEK", "EURSEK", 100000 },
            { "EURSGD", "EURSGD", 100000 },
            { "EURTRY", "EURTRY", 100000 },
            { "EURUSD", "EURUSD", 100000 },
            { "EURZAR", "EURZAR", 100000 },
            { "EUSIDXEUR", "EU50EUR", 1000 },
            { "FRAIDXEUR", "FR40EUR", 1000 },
            { "GBPAUD", "GBPAUD", 100000 },
            { "GBPCAD", "GBPCAD", 100000 },
            { "GBPCHF", "GBPCHF", 100000 },
            { "GBPJPY", "GBPJPY", 1000 },
            { "GBPNZD", "GBPNZD", 100000 },
            { "GBPUSD", "GBPUSD", 100000 },
            { "GBRIDXGBP", "UK100GBP", 1000 },
            { "HKDJPY", "HKDJPY", 100000 },
            { "HKGIDXHKD", "HK33HKD", 1000 },
            { "ITAIDXEUR", "IT40EUR", 1000 },
            { "JPNIDXJPY", "JP225JPY", 1000 },
            { "LIGHTCMDUSD", "WTICOUSD", 1000 },
            { "MXNJPY", "MXNJPY", 1000 },
            { "NGASCMDUSD", "NATGASUSD", 1000 },
            { "NLDIDXEUR", "NL25EUR", 1000 },
            { "NZDCAD", "NZDCAD", 100000 },
            { "NZDCHF", "NZDCHF", 100000 },
            { "NZDJPY", "NZDJPY", 1000 },
            { "NZDSGD", "NZDSGD", 100000 },
            { "NZDUSD", "NZDUSD", 100000 },
            { "PDCMDUSD", "XPDUSD", 1000 },
            { "PTCMDUSD", "XPTUSD", 1000 },
            { "SGDJPY", "SGDJPY", 1000 },
            { "USA30IDXUSD", "US30USD", 1000 },
            { "USA500IDXUSD", "SPX500USD", 1000 },
            { "USATECHIDXUSD", "NAS100USD", 1000 },
            { "USDBRL", "USDBRL", 100000 },
            { "USDCAD", "USDCAD", 100000 },
            { "USDCHF", "USDCHF", 100000 },
            { "USDCNH", "USDCNY", 100000 },
            { "USDDKK", "USDDKK", 100000 },
            { "USDHKD", "USDHKD", 100000 },
            { "USDHUF", "USDHUF", 1000 },
            { "USDJPY", "USDJPY", 1000 },
            { "USDMXN", "USDMXN", 100000 },
            { "USDNOK", "USDNOK", 100000 },
            { "USDPLN", "USDPLN", 100000 },
            { "USDRUB", "USDRUB", 100000 },
            { "USDSEK", "USDSEK", 100000 },
            { "USDSGD", "USDSGD", 100000 },
            { "USDTRY", "USDTRY", 100000 },
            { "USDZAR", "USDZAR", 100000 },
            { "XAGUSD", "XAGUSD", 1000 },
            { "XAUUSD", "XAUUSD", 1000 },
            { "ZARJPY", "ZARJPY", 100000 }
        };

        private static final Map<String,String> MapDukascopyToLean = new Map<String,String>();
        private static final Map<String,String> MapLeanToDukascopy = new Map<String,String>();
        private static final Map<String,Integer> PointValues = new Map<String,Integer>();

        /**
         * The list of known Dukascopy currencies.
        */
        private static final HashSet<String> KnownCurrencies = new HashSet<String>
        {
            "AUD", "BRL", "CAD", "CHF", "CNH", "DKK", "EUR", "GBP", "HKD", "HUF", "JPY", "MXN", "NOK", "NZD", "PLN", "RUB", "SEK", "SGD", "TRY", "USD", "ZAR"
        };

        /**
         * Static constructor for the <see cref="DukascopySymbolMapper"/> class
        */
        static DukascopySymbolMapper() {
            foreach (mapping in DukascopySymbolMappings) {
                MapDukascopyToLean[mapping.Item1] = mapping.Item2;
                MapLeanToDukascopy[mapping.Item2] = mapping.Item1;
                PointValues[mapping.Item2] = mapping.Item3;
            }
        }

        /**
         * Converts a Lean symbol instance to a Dukascopy symbol
        */
         * @param symbol A Lean symbol instance
        @returns The Dukascopy symbol
        public String GetBrokerageSymbol(Symbol symbol) {
            if( symbol == null || symbol == Symbol.Empty || StringUtils.isBlank(symbol.Value))
                throw new IllegalArgumentException( "Invalid symbol: " + (symbol == null ? "null" : symbol.toString()));

            if( symbol.ID.SecurityType != SecurityType.Forex && symbol.ID.SecurityType != SecurityType.Cfd)
                throw new IllegalArgumentException( "Invalid security type: " + symbol.ID.SecurityType);

            brokerageSymbol = ConvertLeanSymbolToDukascopySymbol(symbol.Value);

            if( !IsKnownBrokerageSymbol(brokerageSymbol))
                throw new IllegalArgumentException( "Unknown symbol: " + symbol.Value);

            return brokerageSymbol;
        }

        /**
         * Converts a Dukascopy symbol to a Lean symbol instance
        */
         * @param brokerageSymbol The Dukascopy symbol
         * @param securityType The security type
         * @param market The market
        @returns A new Lean Symbol instance
        public Symbol GetLeanSymbol( String brokerageSymbol, SecurityType securityType, String market) {
            if(  String.IsNullOrWhiteSpace(brokerageSymbol))
                throw new IllegalArgumentException( "Invalid Dukascopy symbol: " + brokerageSymbol);

            if( !IsKnownBrokerageSymbol(brokerageSymbol))
                throw new IllegalArgumentException( "Unknown Dukascopy symbol: " + brokerageSymbol);

            if( securityType != SecurityType.Forex && securityType != SecurityType.Cfd)
                throw new IllegalArgumentException( "Invalid security type: " + securityType);

            if( market != Market.Dukascopy)
                throw new IllegalArgumentException( "Invalid market: " + market);

            return Symbol.Create(ConvertDukascopySymbolToLeanSymbol(brokerageSymbol), GetBrokerageSecurityType(brokerageSymbol), Market.Dukascopy);
        }

        /**
         * Returns the security type for a Dukascopy symbol
        */
         * @param brokerageSymbol The Dukascopy symbol
        @returns The security type
        public SecurityType GetBrokerageSecurityType( String brokerageSymbol) {
            return (brokerageSymbol.Length == 6 && KnownCurrencies.Contains(brokerageSymbol.Substring(0, 3)) && KnownCurrencies.Contains(brokerageSymbol.Substring(3, 3)))
                ? SecurityType.Forex
                : SecurityType.Cfd;
        }

        /**
         * Returns the security type for a Lean symbol
        */
         * @param leanSymbol The Lean symbol
        @returns The security type
        public SecurityType GetLeanSecurityType( String leanSymbol) {
            String dukascopySymbol;
            if( !MapLeanToDukascopy.TryGetValue(leanSymbol, out dukascopySymbol))
                throw new IllegalArgumentException( "Unknown Lean symbol: " + leanSymbol);

            return GetBrokerageSecurityType(dukascopySymbol);
        }

        /**
         * Checks if the symbol is supported by Dukascopy
        */
         * @param brokerageSymbol The Dukascopy symbol
        @returns True if Dukascopy supports the symbol
        public boolean IsKnownBrokerageSymbol( String brokerageSymbol) {
            return brokerageSymbol != null && MapDukascopyToLean.ContainsKey(brokerageSymbol);
        }

        /**
         * Checks if the symbol is supported by Dukascopy
        */
         * @param symbol The Lean symbol
        @returns True if Dukascopy supports the symbol
        public boolean IsKnownLeanSymbol(Symbol symbol) {
            if( symbol == null || StringUtils.isBlank(symbol.Value))
                return false;

            dukascopySymbol = ConvertLeanSymbolToDukascopySymbol(symbol.Value);

            return MapDukascopyToLean.ContainsKey(dukascopySymbol) && GetBrokerageSecurityType(dukascopySymbol) == symbol.ID.SecurityType;
        }

        /**
         * Returns the point value for a Lean symbol
        */
        public int GetPointValue(Symbol symbol) {
            return PointValues[symbol.Value];
        }

        /**
         * Converts a Dukascopy symbol to a Lean symbol string
        */
        private static String ConvertDukascopySymbolToLeanSymbol( String dukascopySymbol) {
            String leanSymbol;
            return MapDukascopyToLean.TryGetValue(dukascopySymbol, out leanSymbol) ? leanSymbol : string.Empty;
        }

        /**
         * Converts a Lean symbol String to a Dukascopy symbol
        */
        private static String ConvertLeanSymbolToDukascopySymbol( String leanSymbol) {
            String dukascopySymbol;
            return MapLeanToDukascopy.TryGetValue(leanSymbol, out dukascopySymbol) ? dukascopySymbol : string.Empty;
        }

    }
}
