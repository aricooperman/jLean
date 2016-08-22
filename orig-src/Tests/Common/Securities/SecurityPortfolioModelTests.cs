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
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Orders;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Common.Securities
{
    [TestFixture]
    public class SecurityPortfolioModelTests
    {
        [Test]
        public void LastTradeProfit_FlatToLong() {
            reference = new DateTime(2016, 02, 16, 11, 53, 30);
            SecurityPortfolioManager portfolio;
            security = InitializeTest(reference, out portfolio);

            fillPrice = 100m;
            fillQuantity = 100;
            orderFee = 1m;
            orderDirection = fillQuantity > 0 ? OrderDirection.Buy : OrderDirection.Sell;
            fill = new OrderEvent(1, security.Symbol, reference, OrderStatus.Filled, orderDirection, fillPrice, fillQuantity, orderFee);
            portfolio.ProcessFill(fill);

            // zero since we're from flat
            Assert.AreEqual(0, security.Holdings.LastTradeProfit);
        }

        [Test]
        public void LastTradeProfit_FlatToShort() {
            reference = new DateTime(2016, 02, 16, 11, 53, 30);
            SecurityPortfolioManager portfolio;
            security = InitializeTest(reference, out portfolio);

            fillPrice = 100m;
            fillQuantity = -100;
            orderFee = 1m;
            orderDirection = fillQuantity > 0 ? OrderDirection.Buy : OrderDirection.Sell;
            fill = new OrderEvent(1, security.Symbol, reference, OrderStatus.Filled, orderDirection, fillPrice, fillQuantity, orderFee);
            portfolio.ProcessFill(fill);

            // zero since we're from flat
            Assert.AreEqual(0, security.Holdings.LastTradeProfit);
        }

        [Test]
        public void LastTradeProfit_LongToLonger() {
            reference = new DateTime(2016, 02, 16, 11, 53, 30);
            SecurityPortfolioManager portfolio;
            security = InitializeTest(reference, out portfolio);

            security.Holdings.SetHoldings(50m, 100);

            fillPrice = 100m;
            fillQuantity = 100;
            orderFee = 1m;
            orderDirection = fillQuantity > 0 ? OrderDirection.Buy : OrderDirection.Sell;
            fill = new OrderEvent(1, security.Symbol, reference, OrderStatus.Filled, orderDirection, fillPrice, fillQuantity, orderFee);
            portfolio.ProcessFill(fill);

            // zero since we're from flat
            Assert.AreEqual(0, security.Holdings.LastTradeProfit);
        }

        [Test]
        public void LastTradeProfit_LongToFlat() {
            reference = new DateTime(2016, 02, 16, 11, 53, 30);
            SecurityPortfolioManager portfolio;
            security = InitializeTest(reference, out portfolio);

            security.Holdings.SetHoldings(50m, 100);

            fillPrice = 100m;
            fillQuantity = -security.Holdings.Quantity;
            orderFee = 1m;
            orderDirection = fillQuantity > 0 ? OrderDirection.Buy : OrderDirection.Sell;
            fill = new OrderEvent(1, security.Symbol, reference, OrderStatus.Filled, orderDirection, fillPrice, fillQuantity, orderFee);
            portfolio.ProcessFill(fill);

            // bought @50 and sold @100 = (-50*100)+(100*100 - 1) = 4999
            // current implementation doesn't back out fees.
            Assert.AreEqual(5000m, security.Holdings.LastTradeProfit);
        }

        [Test]
        public void LastTradeProfit_LongToShort() {
            reference = new DateTime(2016, 02, 16, 11, 53, 30);
            SecurityPortfolioManager portfolio;
            security = InitializeTest(reference, out portfolio);

            security.Holdings.SetHoldings(50m, 100);

            fillPrice = 100m;
            fillQuantity = -2*security.Holdings.Quantity;
            orderFee = 1m;
            orderDirection = fillQuantity > 0 ? OrderDirection.Buy : OrderDirection.Sell;
            fill = new OrderEvent(1, security.Symbol, reference, OrderStatus.Filled, orderDirection, fillPrice, fillQuantity, orderFee);
            portfolio.ProcessFill(fill);

            // we can only take 'profit' on the closing part of the position, so we closed 100
            // shares and opened a new for the second 100, so ony the frst 100 go into the calculation
            // bought @50 and sold @100 = (-50*100)+(100*100 - 1) = 4999
            // current implementation doesn't back out fees.
            Assert.AreEqual(5000m, security.Holdings.LastTradeProfit);
        }

        [Test]
        public void LastTradeProfit_ShortToShorter() {
            reference = new DateTime(2016, 02, 16, 11, 53, 30);
            SecurityPortfolioManager portfolio;
            security = InitializeTest(reference, out portfolio);

            security.Holdings.SetHoldings(50m, -100);

            fillPrice = 100m;
            fillQuantity = -100;
            orderFee = 1m;
            orderDirection = fillQuantity > 0 ? OrderDirection.Buy : OrderDirection.Sell;
            fill = new OrderEvent(1, security.Symbol, reference, OrderStatus.Filled, orderDirection, fillPrice, fillQuantity, orderFee);
            portfolio.ProcessFill(fill);

            Assert.AreEqual(0, security.Holdings.LastTradeProfit);
        }

        [Test]
        public void LastTradeProfit_ShortToFlat() {
            reference = new DateTime(2016, 02, 16, 11, 53, 30);
            SecurityPortfolioManager portfolio;
            security = InitializeTest(reference, out portfolio);

            security.Holdings.SetHoldings(50m, -100);

            fillPrice = 100m;
            fillQuantity = -security.Holdings.Quantity;
            orderFee = 1m;
            orderDirection = fillQuantity > 0 ? OrderDirection.Buy : OrderDirection.Sell;
            fill = new OrderEvent(1, security.Symbol, reference, OrderStatus.Filled, orderDirection, fillPrice, fillQuantity, orderFee);
            portfolio.ProcessFill(fill);


            // sold @50 and bought @100 = (50*100)+(-100*100 - 1) = -5001
            // current implementation doesn't back out fees.
            Assert.AreEqual(-5000m, security.Holdings.LastTradeProfit);
        }

        [Test]
        public void LastTradeProfit_ShortToLong() {
            reference = new DateTime(2016, 02, 16, 11, 53, 30);
            SecurityPortfolioManager portfolio;
            security = InitializeTest(reference, out portfolio);

            security.Holdings.SetHoldings(50m, -100);

            fillPrice = 100m;
            fillQuantity = -2*security.Holdings.Quantity; // flip from -100 to +100
            orderFee = 1m;
            orderDirection = fillQuantity > 0 ? OrderDirection.Buy : OrderDirection.Sell;
            fill = new OrderEvent(1, security.Symbol, reference, OrderStatus.Filled, orderDirection, fillPrice, fillQuantity, orderFee);
            portfolio.ProcessFill(fill);

            // we can only take 'profit' on the closing part of the position, so we closed 100
            // shares and opened a new for the second 100, so ony the frst 100 go into the calculation
            // sold @50 and bought @100 = (50*100)+(-100*100 - 1) = -5001
            // current implementation doesn't back out fees.
            Assert.AreEqual(-5000m, security.Holdings.LastTradeProfit);
        }

        private Security InitializeTest(DateTime reference, out SecurityPortfolioManager portfolio) {
            security = new Security(SecurityExchangeHours.AlwaysOpen(TimeZones.NewYork), CreateTradeBarConfig(), new Cash(CashBook.AccountCurrency, 0, 1m), SymbolProperties.GetDefault(CashBook.AccountCurrency));
            security.SetMarketPrice(new Tick { Value = 100 });
            timeKeeper = new TimeKeeper(reference);
            securityManager = new SecurityManager(timeKeeper);
            securityManager.Add(security);
            transactionManager = new SecurityTransactionManager(securityManager);
            portfolio = new SecurityPortfolioManager(securityManager, transactionManager);
            portfolio.SetCash( "USD", 100 * 1000m, 1m);
            Assert.AreEqual(0, security.Holdings.Quantity);
            Assert.AreEqual(100*1000m, portfolio.CashBook[CashBook.AccountCurrency].Amount);
            return security;
        }

        private static SubscriptionDataConfig CreateTradeBarConfig() {
            return new SubscriptionDataConfig(typeof(TradeBar), Symbols.SPY, Resolution.Minute, TimeZones.NewYork, TimeZones.NewYork, true, true, false);
        }
    }
}