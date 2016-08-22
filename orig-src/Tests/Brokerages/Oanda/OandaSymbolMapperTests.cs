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
using NUnit.Framework;
using QuantConnect.Brokerages.Oanda;

package com.quantconnect.lean.Tests.Brokerages.Oanda
{
    [TestFixture]
    public class OandaSymbolMapperTests
    {
        [Test]
        public void ReturnsCorrectLeanSymbol() {
            mapper = new OandaSymbolMapper();

            symbol = mapper.GetLeanSymbol( "EUR_USD", SecurityType.Forex, Market.Oanda);
            Assert.AreEqual( "EURUSD", symbol.Value);
            Assert.AreEqual(SecurityType.Forex, symbol.ID.SecurityType);
            Assert.AreEqual(Market.Oanda, symbol.ID.Market);

            symbol = mapper.GetLeanSymbol( "DE30_EUR", SecurityType.Cfd, Market.Oanda);
            Assert.AreEqual( "DE30EUR", symbol.Value);
            Assert.AreEqual(SecurityType.Cfd, symbol.ID.SecurityType);
            Assert.AreEqual(Market.Oanda, symbol.ID.Market);
        }

        [Test]
        public void ReturnsCorrectBrokerageSymbol() {
            mapper = new OandaSymbolMapper();

            symbol = Symbol.Create( "EURUSD", SecurityType.Forex, Market.Oanda);
            brokerageSymbol = mapper.GetBrokerageSymbol(symbol);
            Assert.AreEqual( "EUR_USD", brokerageSymbol);

            symbol = Symbol.Create( "DE30EUR", SecurityType.Cfd, Market.Oanda);
            brokerageSymbol = mapper.GetBrokerageSymbol(symbol);
            Assert.AreEqual( "DE30_EUR", brokerageSymbol);
        }

        [Test]
        public void ThrowsOnNullOrEmptySymbol() {
            mapper = new OandaSymbolMapper();

            Assert.Throws<ArgumentException>(() => mapper.GetLeanSymbol(null, SecurityType.Forex, Market.Oanda));

            Assert.Throws<ArgumentException>(() => mapper.GetLeanSymbol( "", SecurityType.Forex, Market.Oanda));

            symbol = Symbol.Empty;
            Assert.Throws<ArgumentException>(() => mapper.GetBrokerageSymbol(symbol));

            symbol = null;
            Assert.Throws<ArgumentException>(() => mapper.GetBrokerageSymbol(symbol));

            symbol = Symbol.Create( "", SecurityType.Forex, Market.Oanda);
            Assert.Throws<ArgumentException>(() => mapper.GetBrokerageSymbol(symbol));
        }

        [Test]
        public void ThrowsOnUnknownSymbol() {
            mapper = new OandaSymbolMapper();

            Assert.Throws<ArgumentException>(() => mapper.GetLeanSymbol( "ABC_USD", SecurityType.Forex, Market.Oanda));

            symbol = Symbol.Create( "ABCUSD", SecurityType.Forex, Market.Oanda);
            Assert.Throws<ArgumentException>(() => mapper.GetBrokerageSymbol(symbol));
        }

        [Test]
        public void ThrowsOnInvalidSecurityType() {
            mapper = new OandaSymbolMapper();

            Assert.Throws<ArgumentException>(() => mapper.GetLeanSymbol( "AAPL", SecurityType.Equity, Market.Oanda));

            symbol = Symbol.Create( "AAPL", SecurityType.Equity, Market.Oanda);
            Assert.Throws<ArgumentException>(() => mapper.GetBrokerageSymbol(symbol));
        }

        [Test]
        public void ChecksForKnownSymbols() {
#pragma warning disable 0618 // This test requires implicit operators
            mapper = new OandaSymbolMapper();

            Assert.IsFalse(mapper.IsKnownBrokerageSymbol(null ));
            Assert.IsFalse(mapper.IsKnownBrokerageSymbol( ""));
            Assert.IsTrue(mapper.IsKnownBrokerageSymbol( "EUR_USD"));
            Assert.IsTrue(mapper.IsKnownBrokerageSymbol( "DE30_EUR"));
            Assert.IsFalse(mapper.IsKnownBrokerageSymbol( "ABC_USD"));

            Assert.IsFalse(mapper.IsKnownLeanSymbol(null ));
            Assert.IsFalse(mapper.IsKnownLeanSymbol( ""));
            Assert.IsFalse(mapper.IsKnownLeanSymbol(Symbol.Empty));
            Assert.IsTrue(mapper.IsKnownLeanSymbol(Symbol.Create( "EURUSD", SecurityType.Forex, Market.Oanda)));
            Assert.IsFalse(mapper.IsKnownLeanSymbol(Symbol.Create( "ABCUSD", SecurityType.Forex, Market.Oanda)));
            Assert.IsFalse(mapper.IsKnownLeanSymbol(Symbol.Create( "EURUSD", SecurityType.Cfd, Market.Oanda)));
            Assert.IsFalse(mapper.IsKnownLeanSymbol(Symbol.Create( "DE30EUR", SecurityType.Forex, Market.Oanda)));
#pragma warning restore 0618
        }

    }
}
