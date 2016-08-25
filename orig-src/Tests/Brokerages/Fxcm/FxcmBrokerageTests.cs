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
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using QuantConnect.Brokerages.Fxcm;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Brokerages.Fxcm
{
    [TestFixture, Ignore( "These tests require a configured and active FXCM practice account")]
    public partial class FxcmBrokerageTests : BrokerageTests
    {
        /**
         * Creates the brokerage under test
        */
        @returns A connected brokerage instance
        protected @Override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider) {
            server = Config.Get( "fxcm-server");
            terminal = Config.Get( "fxcm-terminal");
            userName = Config.Get( "fxcm-user-name");
            password = Config.Get( "fxcm-password");
            accountId = Config.Get( "fxcm-account-id");

            return new FxcmBrokerage(orderProvider, securityProvider, server, terminal, userName, password, accountId);
        }

        /**
         * Disposes of the brokerage and any external resources started in order to create it
        */
         * @param brokerage The brokerage instance to be disposed of
        protected @Override void DisposeBrokerage(IBrokerage brokerage) {
            brokerage.Disconnect();
        }

        /**
         * Provides the data required to test each order type in various cases
        */
        public @Override TestCaseData[] OrderParameters
        {
            get
            {
                return new[]
                {
                    new TestCaseData(new MarketOrderTestParameters(Symbol)).SetName( "MarketOrder"),
                    new TestCaseData(new FxcmLimitOrderTestParameters(Symbol, HighPrice, LowPrice)).SetName( "LimitOrder"),
                    new TestCaseData(new FxcmStopMarketOrderTestParameters(Symbol, HighPrice, LowPrice)).SetName( "StopMarketOrder"),
                };
            }
        }

        /**
         * Gets the symbol to be traded, must be shortable
        */
        protected @Override Symbol Symbol
        {
            get { return Symbols.EURUSD; }
        }

        /**
         * Gets the security type associated with the <see cref="BrokerageTests.Symbol"/>
        */
        protected @Override SecurityType SecurityType
        {
            get { return SecurityType.Forex; }
        }

        /**
         * Gets a high price for the specified symbol so a limit sell won't fill
        */
        protected @Override BigDecimal HighPrice
        {
            // FXCM requires order prices to be not more than 5600 pips from the market price (at least for EURUSD)
            get { return 1.5m; }
        }

        /**
         * Gets a low price for the specified symbol so a limit buy won't fill
        */
        protected @Override BigDecimal LowPrice
        {
            // FXCM requires order prices to be not more than 5600 pips from the market price (at least for EURUSD)
            get { return 0.7m; }
        }

        /**
         * Gets the current market price of the specified security
        */
        protected @Override BigDecimal GetAskPrice(Symbol symbol) {
            // not used, we use bid/ask prices
            return 0;
        }

        /**
         * Gets the default order quantity
        */
        protected @Override int GetDefaultQuantity() {
            // FXCM requires a multiple of 1000 for Forex instruments
            return 1000;
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
