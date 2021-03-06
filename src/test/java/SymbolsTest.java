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

package com.quantconnect.lean.Tests
{
    /**
     * Provides symbol instancs for unit tests
    */
    public static class Symbols
    {
        public static final Symbol SPY = CreateEquitySymbol( "SPY");
        public static final Symbol AAPL = CreateEquitySymbol( "AAPL");
        public static final Symbol MSFT = CreateEquitySymbol( "MSFT");
        public static final Symbol ZNGA = CreateEquitySymbol( "ZNGA");
        public static final Symbol FXE = CreateEquitySymbol( "FXE");

        public static final Symbol USDJPY = CreateForexSymbol( "USDJPY");
        public static final Symbol EURUSD = CreateForexSymbol( "EURUSD");
        public static final Symbol EURGBP = CreateForexSymbol( "EURGBP");
        public static final Symbol GBPUSD = CreateForexSymbol( "GBPUSD");
        
        public static final Symbol DE10YBEUR = CreateCfdSymbol( "DE10YBEUR", Market.FXCM);

        public static final Symbol SPY_P_192_Feb19_2016 = CreateOptionSymbol( "SPY", OptionRight.Put, 192m, new DateTime(2016, 02, 19));

        private static Symbol CreateForexSymbol( String symbol) {
            return Symbol.Create(symbol, SecurityType.Forex, Market.FXCM);
        }

        private static Symbol CreateEquitySymbol( String symbol) {
            return Symbol.Create(symbol, SecurityType.Equity, Market.USA);
        }

        private static Symbol CreateCfdSymbol( String symbol, String market) {
            return Symbol.Create(symbol, SecurityType.Cfd, market);
        }

        private static Symbol CreateOptionSymbol( String symbol, OptionRight right, BigDecimal strike, DateTime expiry) {
            return Symbol.CreateOption(symbol, Market.USA, OptionStyle.American, right, strike, expiry);
        }
    }
}
