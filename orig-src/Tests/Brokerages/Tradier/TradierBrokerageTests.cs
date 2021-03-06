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
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Brokerages.Tradier;
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Brokerages.Tradier
{
    [TestFixture, Ignore( "This test requires a configured and active Tradier account")]
    public class TradierBrokerageTests : BrokerageTests
    {
        /**
         * Creates the brokerage under test
        */
        @returns A connected brokerage instance
        protected @Override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider) {
            accountID = TradierBrokerageFactory.Configuration.AccountID;
            tradier = new TradierBrokerage(orderProvider, securityProvider, accountID);

            qcUserID = TradierBrokerageFactory.Configuration.QuantConnectUserID;
            tokens = TradierBrokerageFactory.GetTokens();
            tradier.SetTokens(qcUserID, tokens.AccessToken, tokens.RefreshToken, tokens.IssuedAt, Duration.ofSeconds(tokens.ExpiresIn));

            // keep the tokens up to date in the event of a refresh
            tradier.SessionRefreshed += (sender, args) =>
            {
                File.WriteAllText(TradierBrokerageFactory.TokensFile, JsonConvert.SerializeObject(args, Formatting.Indented));
            };

            return tradier;
        }

        /**
         * Gets the symbol to be traded, must be shortable
        */
        protected @Override Symbol Symbol
        {
            get { return Symbols.AAPL; }
        }

        /**
         * Gets the security type associated with the <see cref="BrokerageTests.Symbol"/>
        */
        protected @Override SecurityType SecurityType
        {
            get { return SecurityType.Equity; }
        }

        /**
         * Gets a high price for the specified symbol so a limit sell won't fill
        */
        protected @Override BigDecimal HighPrice
        {
            get { return 1000m; }
        }

        /**
         * Gets a low price for the specified symbol so a limit buy won't fill
        */
        protected @Override BigDecimal LowPrice
        {
            get { return 0.01m; }
        }

        /**
         * Gets the current market price of the specified security
        */
        protected @Override BigDecimal GetAskPrice(Symbol symbol) {
            tradier = (TradierBrokerage) Brokerage;
            quotes = tradier.GetQuotes(new List<String> {symbol.Value});
            return quotes.Single().Ask;
        }

        [Test, TestCaseSource( "OrderParameters")]
        public void AllowsOneActiveOrderPerSymbol(OrderTestParameters parameters) {
            // tradier's api gets special with zero holdings crossing in that they need to fill the order
            // before the next can be submitted, so we just limit this impl to only having on active order
            // by symbol at a time, new orders will issue cancel commands for the existing order

            boolean orderFilledOrCanceled = false;
            order = parameters.CreateLongOrder(1);
            EventHandler<OrderEvent> brokerageOnOrderStatusChanged = (sender, args) =>
            {
                // we expect all orders to be cancelled except for market orders, they may fill before the next order is submitted
                if( args.OrderId == order.Id && args.Status == OrderStatus.Canceled || (order is MarketOrder && args.Status == OrderStatus.Filled)) {
                    orderFilledOrCanceled = true;
                }
            };

            Brokerage.OrderStatusChanged += brokerageOnOrderStatusChanged;

            // starting from zero initiate two long orders and see that the first is canceled
            PlaceOrderWaitForStatus(order, OrderStatus.Submitted);
            PlaceOrderWaitForStatus(parameters.CreateLongMarketOrder(1));

            Brokerage.OrderStatusChanged -= brokerageOnOrderStatusChanged;

            Assert.IsTrue(orderFilledOrCanceled);
        }

        [Test, Ignore( "This test exists to manually verify how rejected orders are handled when we don't receive an order ID back from Tradier.")]
        public void ShortZnga() {
            PlaceOrderWaitForStatus(new MarketOrder(Symbols.ZNGA, -1, DateTime.Now), OrderStatus.Invalid, allowFailedSubmission: true);

            // wait for output to be generated
            Thread.Sleep(20*1000);
        }
    }
}
