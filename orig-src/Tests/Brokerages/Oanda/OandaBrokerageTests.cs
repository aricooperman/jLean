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
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using QuantConnect.Brokerages.Oanda;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Securities;
using Environment = QuantConnect.Brokerages.Oanda.Environment;

package com.quantconnect.lean.Tests.Brokerages.Oanda
{
    [TestFixture, Ignore( "This test requires a configured and testable Oanda practice account")]
    public partial class OandaBrokerageTests : BrokerageTests
    {
        /**
         *     Creates the brokerage under test and connects it
        */
        @returns A connected brokerage instance
        protected @Override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider) {
            environment = Config.Get( "oanda-environment").ConvertTo<Environment>();
            accessToken = Config.Get( "oanda-access-token");
            accountId = Config.Get( "oanda-account-id").ConvertTo<Integer>();

            return new OandaBrokerage(orderProvider, securityProvider, environment, accessToken, accountId);
        }

        /**
         *     Gets the symbol to be traded, must be shortable
        */
        protected @Override Symbol Symbol
        {
            get { return Symbol.Create( "EURUSD", SecurityType.Forex, Market.Oanda); }
        }

        /**
         *     Gets the security type associated with the <see cref="BrokerageTests.Symbol" />
        */
        protected @Override SecurityType SecurityType
        {
            get { return SecurityType.Forex; }
        }

        /**
         *     Gets a high price for the specified symbol so a limit sell won't fill
        */
        protected @Override BigDecimal HighPrice
        {
            get { return 5m; }
        }

        /**
         *     Gets a low price for the specified symbol so a limit buy won't fill
        */
        protected @Override BigDecimal LowPrice
        {
            get { return 0.32m; }
        }

        /**
         *     Gets the current market price of the specified security
        */
        protected @Override BigDecimal GetAskPrice(Symbol symbol) {
            oanda = (OandaBrokerage) Brokerage;
            quotes = oanda.GetRates(new List<String> { new OandaSymbolMapper().GetBrokerageSymbol(symbol) });
            return (decimal)quotes[0].ask;
        }

        [Test]
        public void ValidateLimitOrders() {
            oanda = (OandaBrokerage)Brokerage;
            symbol = Symbol;
            quotes = oanda.GetRates(new List<String> { new OandaSymbolMapper().GetBrokerageSymbol(symbol) });

            // Buy Limit order below market
            limitPrice = new BigDecimal( quotes[0].bid - 0.5);
            order = new LimitOrder(symbol, 1, limitPrice, DateTime.Now);
            Assert.IsTrue(oanda.PlaceOrder(order));

            // update Buy Limit order with no changes
            Assert.IsTrue(oanda.UpdateOrder(order));

            // move Buy Limit order above market
            order.LimitPrice = new BigDecimal( quotes[0].ask + 0.5);
            Assert.IsTrue(oanda.UpdateOrder(order));
        }

        [Test]
        public void ValidateStopMarketOrders() {
            oanda = (OandaBrokerage)Brokerage;
            symbol = Symbol;
            quotes = oanda.GetRates(new List<String> { new OandaSymbolMapper().GetBrokerageSymbol(symbol) });

            // Buy StopMarket order below market
            price = new BigDecimal( quotes[0].bid - 0.5);
            order = new StopMarketOrder(symbol, 1, price, DateTime.Now);
            Assert.IsTrue(oanda.PlaceOrder(order));

            // Buy StopMarket order above market
            price = new BigDecimal( quotes[0].ask + 0.5);
            order = new StopMarketOrder(symbol, 1, price, DateTime.Now);
            Assert.IsTrue(oanda.PlaceOrder(order));

            // Sell StopMarket order below market
            price = new BigDecimal( quotes[0].bid - 0.5);
            order = new StopMarketOrder(symbol, -1, price, DateTime.Now);
            Assert.IsTrue(oanda.PlaceOrder(order));

            // Sell StopMarket order above market
            price = new BigDecimal( quotes[0].ask + 0.5);
            order = new StopMarketOrder(symbol, -1, price, DateTime.Now);
            Assert.IsTrue(oanda.PlaceOrder(order));
        }

        [Test]
        public void ValidateStopLimitOrders() {
            oanda = (OandaBrokerage) Brokerage;
            symbol = Symbol;
            quotes = oanda.GetRates(new List<String> {new OandaSymbolMapper().GetBrokerageSymbol(symbol)});

            // Buy StopLimit order below market (Oanda accepts this order but cancels it immediately)
            stopPrice = new BigDecimal( quotes[0].bid - 0.5);
            limitPrice = stopPrice + 0.0005m;
            order = new StopLimitOrder(symbol, 1, stopPrice, limitPrice, DateTime.Now);
            Assert.IsTrue(oanda.PlaceOrder(order));

            // Buy StopLimit order above market
            stopPrice = new BigDecimal( quotes[0].ask + 0.5);
            limitPrice = stopPrice + 0.0005m;
            order = new StopLimitOrder(symbol, 1, stopPrice, limitPrice, DateTime.Now);
            Assert.IsTrue(oanda.PlaceOrder(order));

            // Sell StopLimit order below market
            stopPrice = new BigDecimal( quotes[0].bid - 0.5);
            limitPrice = stopPrice - 0.0005m;
            order = new StopLimitOrder(symbol, -1, stopPrice, limitPrice, DateTime.Now);
            Assert.IsTrue(oanda.PlaceOrder(order));

            // Sell StopLimit order above market (Oanda accepts this order but cancels it immediately)
            stopPrice = new BigDecimal( quotes[0].ask + 0.5);
            limitPrice = stopPrice - 0.0005m;
            order = new StopLimitOrder(symbol, -1, stopPrice, limitPrice, DateTime.Now);
            Assert.IsTrue(oanda.PlaceOrder(order));
        }

        [Test, Ignore( "This test requires disconnecting the internet to test for connection resiliency")]
        public void ClientReconnectsAfterInternetDisconnect() {
            brokerage = Brokerage;
            Assert.IsTrue(brokerage.IsConnected);

            tenMinutes = Duration.ofMinutes(10);

            Console.WriteLine( "------");
            Console.WriteLine( "Waiting for internet disconnection ");
            Console.WriteLine( "------");

            // spin while we manually disconnect the internet
            while (brokerage.IsConnected) {
                Thread.Sleep(2500);
                Console.Write( ".");
            }

            stopwatch = Stopwatch.StartNew();

            Console.WriteLine( "------");
            Console.WriteLine( "Trying to reconnect ");
            Console.WriteLine( "------");

            // spin until we're reconnected
            while (!brokerage.IsConnected && stopwatch.Elapsed < tenMinutes) {
                Thread.Sleep(2500);
                Console.Write( ".");
            }

            Assert.IsTrue(brokerage.IsConnected);
        }

    }
}
