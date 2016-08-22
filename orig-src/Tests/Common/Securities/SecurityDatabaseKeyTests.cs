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
using NUnit.Framework;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Common.Securities
{
    [TestFixture]
    public class SecurityDatabaseKeyTests
    {
        [Test]
        public void ConstructorWithNoWildcards() {
            key = new SecurityDatabaseKey(Market.USA, "SPY", SecurityType.Equity);
            Assert.AreEqual(SecurityType.Equity, key.SecurityType);
            Assert.AreEqual(Market.USA, key.Market);
            Assert.AreEqual( "SPY", key.Symbol);
        }

        [Test]
        public void ConstructorWithNullSymbolConvertsToWildcard() {
            key = new SecurityDatabaseKey(Market.USA, null, SecurityType.Equity);
            Assert.AreEqual(SecurityType.Equity, key.SecurityType);
            Assert.AreEqual(Market.USA, key.Market);
            Assert.AreEqual( "[*]", key.Symbol);
        }

        [Test]
        public void ConstructorWithEmptySymbolConvertsToWildcard() {
            key = new SecurityDatabaseKey(Market.USA, string.Empty, SecurityType.Equity);
            Assert.AreEqual(SecurityType.Equity, key.SecurityType);
            Assert.AreEqual(Market.USA, key.Market);
            Assert.AreEqual( "[*]", key.Symbol);
        }

        [Test]
        public void ConstructorWithNullMarketConvertsToWildcard() {
            key = new SecurityDatabaseKey(null, "SPY", SecurityType.Equity);
            Assert.AreEqual(SecurityType.Equity, key.SecurityType);
            Assert.AreEqual( "[*]", key.Market);
            Assert.AreEqual( "SPY", key.Symbol);
        }

        [Test]
        public void ConstructorWithEmptyMarketConvertsToWildcard() {
            key = new SecurityDatabaseKey( String.Empty, "SPY", SecurityType.Equity);
            Assert.AreEqual(SecurityType.Equity, key.SecurityType);
            Assert.AreEqual( "[*]", key.Market);
            Assert.AreEqual( "SPY", key.Symbol);
        }

        [Test]
        public void ParsesKeyProperly() {
            static final String input = "Equity-usa-SPY";
            key = SecurityDatabaseKey.Parse(input);
            Assert.AreEqual(SecurityType.Equity, key.SecurityType);
            Assert.AreEqual(Market.USA, key.Market);
            Assert.AreEqual( "SPY", key.Symbol);
        }

        [Test]
        public void ParsesWildcardSymbol() {
            static final String input = "Equity-usa-[*]";
            key = SecurityDatabaseKey.Parse(input);
            Assert.AreEqual(SecurityType.Equity, key.SecurityType);
            Assert.AreEqual(Market.USA, key.Market);
            Assert.AreEqual( "[*]", key.Symbol);
        }

        [Test]
        public void ParsesWildcardMarket() {
            static final String input = "Equity-[*]-SPY";
            key = SecurityDatabaseKey.Parse(input);
            Assert.AreEqual(SecurityType.Equity, key.SecurityType);
            Assert.AreEqual( "[*]", key.Market);
            Assert.AreEqual( "SPY", key.Symbol);
        }

        [Test, ExpectedException(typeof(ArgumentException), MatchType = MessageMatch.Contains, ExpectedMessage = "as a SecurityType")]
        public void ThrowsOnWildcardSecurityType() {
            static final String input = "[*]-usa-SPY";
            SecurityDatabaseKey.Parse(input);
        }

        [Test, ExpectedException(typeof (FormatException), MatchType = MessageMatch.Contains, ExpectedMessage = "expected format")]
        public void ThrowsOnInvalidFormat() {
            static final String input = "Equity-[*]";
            SecurityDatabaseKey.Parse(input);
        }
    }
}
