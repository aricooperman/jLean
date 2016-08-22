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

using System.Linq;
using NUnit.Framework;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Common.Securities
{
    [TestFixture]
    public class CashBookTests
    {
        [Test]
        public void InitializesWithAccountCurrencyAdded() {
            book = new CashBook();
            Assert.AreEqual(1, book.Count);
            cash = book.Single().Value;
            Assert.AreEqual(CashBook.AccountCurrency, cash.Symbol);
            Assert.AreEqual(0, cash.Amount);
            Assert.AreEqual(1m, cash.ConversionRate);
        }

        [Test]
        public void ComputesValueInAccountCurrency() {
            book = new CashBook();
            book["USD"].SetAmount(1000);
            book.Add( "JPY", 1000, 1/100m);
            book.Add( "GBP", 1000, 2m);

            BigDecimal expected = book["USD"].ValueInAccountCurrency + book["JPY"].ValueInAccountCurrency + book["GBP"].ValueInAccountCurrency;
            Assert.AreEqual(expected, book.TotalValueInAccountCurrency);
        }

        [Test]
        public void ConvertsProperly() {
            book = new CashBook();
            book.Add( "EUR", 0, 1.10m);
            book.Add( "GBP", 0, 0.71m);

            expected = 781m;
            actual = book.Convert(1000, "EUR", "GBP");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertsToAccountCurrencyProperly() {
            book = new CashBook();
            book.Add( "EUR", 0, 1.10m);

            expected = 1100m;
            actual = book.ConvertToAccountCurrency(1000, "EUR");
            Assert.AreEqual(expected, actual);
        }
    }
}
