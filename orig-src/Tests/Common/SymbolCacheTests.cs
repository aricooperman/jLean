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

using NUnit.Framework;
using QuantConnect.Securities;

namespace QuantConnect.Tests.Common
{
    [TestFixture]
    public class SymbolCacheTests
    {
        [Test]
        public void HandlesRoundTripAccessSymbolToTicker()
        {
            ticker = "ticker";
            SymbolCache.Set(ticker, Symbols.EURUSD);
            actual = SymbolCache.GetSymbol(ticker);
            Assert.AreEqual(Symbols.EURUSD, actual);
        }

        [Test]
        public void HandlesRoundTripAccessTickerToSymbol()
        {
            expected = "ticker";
            expected = Symbols.EURUSD.Value;
            SymbolCache.Set(expected, Symbols.EURUSD);
            actual = SymbolCache.GetTicker(Symbols.EURUSD);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TryGetSymbol()
        {
            SymbolCache.Set("EURUSD", Symbols.EURUSD);

            Symbol actual;
            Assert.IsTrue(SymbolCache.TryGetSymbol("EURUSD", out actual));
            Assert.AreEqual(Symbols.EURUSD, actual);

            Assert.IsFalse(SymbolCache.TryGetSymbol("EURUSD1", out actual));
            Assert.AreEqual(default(Symbol), actual);
        }

        [Test]
        public void TryGetTicker()
        {
            SymbolCache.Set("EURUSD", Symbols.EURUSD);

            String ticker;
            Assert.IsTrue(SymbolCache.TryGetTicker(Symbols.EURUSD, out ticker));
            Assert.AreEqual(Symbols.EURUSD.Value, ticker);

            symbol = new Symbol(SecurityIdentifier.GenerateForex("NOT A FOREX PAIR", Market.FXCM), "EURGBP");
            Assert.IsFalse(SymbolCache.TryGetTicker(symbol, out ticker));
            Assert.AreEqual(default( String), ticker);
        }

        [Test]
        public void TryGetSymbolFromSidString()
        {
            sid = Symbols.EURUSD.ID.ToString();
            symbol = SymbolCache.GetSymbol(sid);
            Assert.AreEqual(Symbols.EURUSD, symbol);
        }

        [Test]
        public void TryGetTickerFromUncachedSymbol()
        {
            symbol = Symbol.Create("My Ticker", SecurityType.Equity, Market.USA);
            ticker = SymbolCache.GetTicker(symbol);
            Assert.AreEqual(symbol.ID.ToString(), ticker);
        }

        [Test]
        public void TryRemoveSymbolRemovesSymbolMappings()
        {
            String ticker;
            Symbol symbol;
            SymbolCache.Set("SPY", Symbols.SPY);
            Assert.IsTrue(SymbolCache.TryRemove(Symbols.SPY));
            Assert.IsFalse(SymbolCache.TryGetSymbol("SPY", out symbol));
            Assert.IsFalse(SymbolCache.TryGetTicker(Symbols.SPY, out ticker));
        }

        [Test]
        public void TryRemoveTickerRemovesSymbolMappings()
        {
            String ticker;
            Symbol symbol;
            SymbolCache.Set("SPY", Symbols.SPY);
            Assert.IsTrue(SymbolCache.TryRemove("SPY"));
            Assert.IsFalse(SymbolCache.TryGetSymbol("SPY", out symbol));
            Assert.IsFalse(SymbolCache.TryGetTicker(Symbols.SPY, out ticker));
        }
    }
}
