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

package com.quantconnect.lean.Brokerages.Oanda
{
    /**
     * Provides the mapping between Lean symbols and Oanda symbols.
    */
    public class OandaSymbolMapper : ISymbolMapper
    {
        /**
         * The list of known Oanda symbols.
        */
        private static final HashSet<String> KnownSymbols = new HashSet<String>
        {
            "AU200_AUD",
            "AUD_CAD",
            "AUD_CHF",
            "AUD_HKD",
            "AUD_JPY",
            "AUD_NZD",
            "AUD_SGD",
            "AUD_USD",
            "BCO_USD",
            "CAD_CHF",
            "CAD_HKD",
            "CAD_JPY",
            "CAD_SGD",
            "CH20_CHF",
            "CHF_HKD",
            "CHF_JPY",
            "CHF_ZAR",
            "CORN_USD",
            "DE10YB_EUR",
            "DE30_EUR",
            "EU50_EUR",
            "EUR_AUD",
            "EUR_CAD",
            "EUR_CHF",
            "EUR_CZK",
            "EUR_DKK",
            "EUR_GBP",
            "EUR_HKD",
            "EUR_HUF",
            "EUR_JPY",
            "EUR_NOK",
            "EUR_NZD",
            "EUR_PLN",
            "EUR_SEK",
            "EUR_SGD",
            "EUR_TRY",
            "EUR_USD",
            "EUR_ZAR",
            "FR40_EUR",
            "GBP_AUD",
            "GBP_CAD",
            "GBP_CHF",
            "GBP_HKD",
            "GBP_JPY",
            "GBP_NZD",
            "GBP_PLN",
            "GBP_SGD",
            "GBP_USD",
            "GBP_ZAR",
            "HK33_HKD",
            "HKD_JPY",
            "JP225_USD",
            "NAS100_USD",
            "NATGAS_USD",
            "NL25_EUR",
            "NZD_CAD",
            "NZD_CHF",
            "NZD_HKD",
            "NZD_JPY",
            "NZD_SGD",
            "NZD_USD",
            "SG30_SGD",
            "SGD_CHF",
            "SGD_HKD",
            "SGD_JPY",
            "SOYBN_USD",
            "SPX500_USD",
            "SUGAR_USD",
            "TRY_JPY",
            "UK100_GBP",
            "UK10YB_GBP",
            "US2000_USD",
            "US30_USD",
            "USB02Y_USD",
            "USB05Y_USD",
            "USB10Y_USD",
            "USB30Y_USD",
            "USD_CAD",
            "USD_CHF",
            "USD_CNH",
            "USD_CNY",
            "USD_CZK",
            "USD_DKK",
            "USD_HKD",
            "USD_HUF",
            "USD_INR",
            "USD_JPY",
            "USD_MXN",
            "USD_NOK",
            "USD_PLN",
            "USD_SAR",
            "USD_SEK",
            "USD_SGD",
            "USD_THB",
            "USD_TRY",
            "USD_TWD",
            "USD_ZAR",
            "WHEAT_USD",
            "WTICO_USD",
            "XAG_AUD",
            "XAG_CAD",
            "XAG_CHF",
            "XAG_EUR",
            "XAG_GBP",
            "XAG_HKD",
            "XAG_JPY",
            "XAG_NZD",
            "XAG_SGD",
            "XAG_USD",
            "XAU_AUD",
            "XAU_CAD",
            "XAU_CHF",
            "XAU_EUR",
            "XAU_GBP",
            "XAU_HKD",
            "XAU_JPY",
            "XAU_NZD",
            "XAU_SGD",
            "XAU_USD",
            "XAU_XAG",
            "XCU_USD",
            "XPD_USD",
            "XPT_USD",
            "ZAR_JPY"
        };

        /**
         * The list of known Oanda currencies.
        */
        private static final HashSet<String> KnownCurrencies = new HashSet<String>
        {
            "AUD", "CAD", "CHF", "CNY", "CZK", "DKK", "EUR", "GBP", "HKD", "HUF", "INR", "JPY", 
            "MXN", "NOK", "NZD", "PLN", "SAR", "SEK", "SGD", "THB", "TRY", "TWD", "USD", "ZAR"
        };

        /**
         * Converts a Lean symbol instance to an Oanda symbol
        */
         * @param symbol A Lean symbol instance
        @returns The Oanda symbol
        public String GetBrokerageSymbol(Symbol symbol) {
            if( symbol == null || symbol == Symbol.Empty || StringUtils.isBlank(symbol.Value))
                throw new IllegalArgumentException( "Invalid symbol: " + (symbol == null ? "null" : symbol.toString()));

            if( symbol.ID.SecurityType != SecurityType.Forex && symbol.ID.SecurityType != SecurityType.Cfd)
                throw new IllegalArgumentException( "Invalid security type: " + symbol.ID.SecurityType);

            brokerageSymbol = ConvertLeanSymbolToOandaSymbol(symbol.Value);

            if( !IsKnownBrokerageSymbol(brokerageSymbol))
                throw new IllegalArgumentException( "Unknown symbol: " + symbol.Value);

            return brokerageSymbol;
        }

        /**
         * Converts an Oanda symbol to a Lean symbol instance
        */
         * @param brokerageSymbol The Oanda symbol
         * @param securityType The security type
         * @param market The market
        @returns A new Lean Symbol instance
        public Symbol GetLeanSymbol( String brokerageSymbol, SecurityType securityType, String market) {
            if(  String.IsNullOrWhiteSpace(brokerageSymbol))
                throw new IllegalArgumentException( "Invalid Oanda symbol: " + brokerageSymbol);

            if( !IsKnownBrokerageSymbol(brokerageSymbol))
                throw new IllegalArgumentException( "Unknown Oanda symbol: " + brokerageSymbol);

            if( securityType != SecurityType.Forex && securityType != SecurityType.Cfd)
                throw new IllegalArgumentException( "Invalid security type: " + securityType);

            if( market != Market.Oanda)
                throw new IllegalArgumentException( "Invalid market: " + market);

            return Symbol.Create(ConvertOandaSymbolToLeanSymbol(brokerageSymbol), GetBrokerageSecurityType(brokerageSymbol), Market.Oanda);
        }

        /**
         * Returns the security type for an Oanda symbol
        */
         * @param brokerageSymbol The Oanda symbol
        @returns The security type
        public SecurityType GetBrokerageSecurityType( String brokerageSymbol) {
            tokens = brokerageSymbol.split('_');
            if( tokens.Length != 2)
                throw new IllegalArgumentException( "Unable to determine SecurityType for Oanda symbol: " + brokerageSymbol);

            return KnownCurrencies.Contains(tokens[0]) && KnownCurrencies.Contains(tokens[1])
                ? SecurityType.Forex
                : SecurityType.Cfd;
        }

        /**
         * Returns the security type for a Lean symbol
        */
         * @param leanSymbol The Lean symbol
        @returns The security type
        public SecurityType GetLeanSecurityType( String leanSymbol) {
            return GetBrokerageSecurityType(ConvertLeanSymbolToOandaSymbol(leanSymbol));
        }

        /**
         * Checks if the symbol is supported by Oanda
        */
         * @param brokerageSymbol The Oanda symbol
        @returns True if Oanda supports the symbol
        public boolean IsKnownBrokerageSymbol( String brokerageSymbol) {
            return KnownSymbols.Contains(brokerageSymbol);
        }

        /**
         * Checks if the symbol is supported by Oanda
        */
         * @param symbol The Lean symbol
        @returns True if Oanda supports the symbol
        public boolean IsKnownLeanSymbol(Symbol symbol) {
            if( symbol == null || StringUtils.isBlank(symbol.Value)) 
                return false;

            oandaSymbol = ConvertLeanSymbolToOandaSymbol(symbol.Value);

            return KnownSymbols.Contains(oandaSymbol) && GetBrokerageSecurityType(oandaSymbol) == symbol.ID.SecurityType;
        }

        /**
         * Converts an Oanda symbol to a Lean symbol string
        */
        private static String ConvertOandaSymbolToLeanSymbol( String oandaSymbol) {
            // Lean symbols are equal to Oanda symbols with underscores removed
            return oandaSymbol.Replace( "_", "");
        }

        /**
         * Converts a Lean symbol String to an Oanda symbol
        */
        private static String ConvertLeanSymbolToOandaSymbol( String leanSymbol) {
            // All Oanda symbols end with '_XYZ', where XYZ is the quote currency
            return leanSymbol.Insert(leanSymbol.Length - 3, "_");
        }

    }
}
