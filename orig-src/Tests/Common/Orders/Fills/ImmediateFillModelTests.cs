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
using QuantConnect.Brokerages;
using QuantConnect.Data.Market;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Securities.Forex;

package com.quantconnect.lean.Tests.Common.Orders.Fills
{
    [TestFixture]
    public class ImmediateFillModelTests
    {
        [TestCase(OrderDirection.Buy)]
        [TestCase(OrderDirection.Sell)]
        public void MarketOrderFillsAtBidAsk(OrderDirection direction) {
            symbol = Symbol.Create( "EURUSD", SecurityType.Forex, "fxcm");
            exchangeHours = SecurityExchangeHours.AlwaysOpen(TimeZones.NewYork);
            quoteCash = new Cash( "USD", 1000, 1);
            symbolProperties = SymbolProperties.GetDefault( "USD");
            security = new Forex(symbol, exchangeHours, quoteCash, symbolProperties);

            reference = DateTime.Now;
            referenceUtc = reference.ConvertToUtc(TimeZones.NewYork);
            timeKeeper = new TimeKeeper(referenceUtc);
            security.SetLocalTimeKeeper(timeKeeper.GetLocalTimeKeeper(TimeZones.NewYork));

            brokerageModel = new FxcmBrokerageModel();
            fillModel = brokerageModel.GetFillModel(security);

            static final BigDecimal bidPrice = 1.13739m;
            static final BigDecimal askPrice = 1.13746m;

            security.SetMarketPrice(new Tick(DateTime.Now, symbol, bidPrice, askPrice));

            quantity = direction == OrderDirection.Buy ? 1 : -1;
            order = new MarketOrder(symbol, quantity, DateTime.Now);
            fill = fillModel.MarketFill(security, order);

            expected = direction == OrderDirection.Buy ? askPrice : bidPrice;
            Assert.AreEqual(expected, fill.FillPrice);
        }


    }
}
