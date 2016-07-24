﻿﻿/*
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
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Securities;

namespace QuantConnect.Tests.Common.Securities.Forex
{
    [TestFixture]
    public class ForexTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentException), MatchType = MessageMatch.Contains, ExpectedMessage = "Currency pairs must be exactly 6 characters")]
        public void DecomposeThrowsOnSymbolTooShort()
        {
            String symbol = "12345";
            Assert.AreEqual(5, symbol.Length);
            String basec, quotec;
            QuantConnect.Securities.Forex.Forex.DecomposeCurrencyPair(symbol, out basec, out quotec);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), MatchType = MessageMatch.Contains, ExpectedMessage = "Currency pairs must be exactly 6 characters")]
        public void DecomposeThrowsOnSymbolTooLong()
        {
            String symbol = "1234567";
            Assert.AreEqual(7, symbol.Length);
            String basec, quotec;
            QuantConnect.Securities.Forex.Forex.DecomposeCurrencyPair(symbol, out basec, out quotec);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), MatchType = MessageMatch.Contains, ExpectedMessage = "Currency pairs must be exactly 6 characters")]
        public void DecomposeThrowsOnNullSymbol()
        {
            String symbol = null;
            String basec, quotec;
            QuantConnect.Securities.Forex.Forex.DecomposeCurrencyPair(symbol, out basec, out quotec);
        }

        [Test]
        public void ConstructorDecomposesBaseAndQuoteCurrencies()
        {
            config = new SubscriptionDataConfig(typeof(TradeBar), Symbols.EURUSD, Resolution.Minute, TimeZones.NewYork, TimeZones.NewYork, true, true, true);
            forex = new QuantConnect.Securities.Forex.Forex(SecurityExchangeHours.AlwaysOpen(config.DataTimeZone), new Cash("usd", 0, 0), config, SymbolProperties.GetDefault("usd"));
            Assert.AreEqual("EUR", forex.BaseCurrencySymbol);
            Assert.AreEqual("USD", forex.QuoteCurrency.Symbol);
        }
    }
}
