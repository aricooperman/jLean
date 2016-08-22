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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Securities.Cfd;
using QuantConnect.Securities.Equity;
using QuantConnect.Securities.Forex;

package com.quantconnect.lean.Tests.Common.Orders
{
    [TestFixture]
    public class OrderTests
    {
        [Test, TestCaseSource( "GetValueTestParameters")]
        public void GetValueTest(ValueTestParameters parameters) {
            value = parameters.Order.GetValue(parameters.Security);
            Assert.AreEqual(parameters.ExpectedValue, value);
        }

        private static TestCaseData[] GetValueTestParameters() {
            static final BigDecimal delta = 1m;
            static final BigDecimal price = 1.2345m;
            static final int quantity = 100;
            static final BigDecimal pricePlusDelta = price + delta;
            static final BigDecimal priceMinusDelta = price - delta;
            tz = TimeZones.NewYork;

            time = new DateTime(2016, 2, 4, 16, 0, 0).ConvertToUtc(tz);

            equity = new Equity(SecurityExchangeHours.AlwaysOpen(tz), new SubscriptionDataConfig(typeof(TradeBar), Symbols.SPY, Resolution.Minute, tz, tz, true, false, false), new Cash(CashBook.AccountCurrency, 0, 1m), SymbolProperties.GetDefault(CashBook.AccountCurrency));
            equity.SetMarketPrice(new Tick {Value = price});

            gbpCash = new Cash( "GBP", 0, 1.46m);
            properties = SymbolProperties.GetDefault(gbpCash.Symbol);
            forex = new Forex(SecurityExchangeHours.AlwaysOpen(tz), gbpCash, new SubscriptionDataConfig(typeof(TradeBar), Symbols.EURGBP, Resolution.Minute, tz, tz, true, false, false), properties);
            forex.SetMarketPrice(new Tick {Value= price});

            eurCash = new Cash( "EUR", 0, 1.12m);
            properties = new SymbolProperties( "Euro-Bund", eurCash.Symbol, 10, 0.1m, 1);
            cfd = new Cfd(SecurityExchangeHours.AlwaysOpen(tz), eurCash, new SubscriptionDataConfig(typeof(TradeBar), Symbols.DE10YBEUR, Resolution.Minute, tz, tz, true, false, false), properties);
            cfd.SetMarketPrice(new Tick { Value = price });
            multiplierTimesConversionRate = properties.ContractMultiplier*eurCash.ConversionRate;

            return new List<ValueTestParameters>
            {
                // equity orders
                new ValueTestParameters( "EquityLongMarketOrder", equity, new MarketOrder(Symbols.SPY, quantity, time), quantity*price),
                new ValueTestParameters( "EquityShortMarketOrder", equity, new MarketOrder(Symbols.SPY, -quantity, time), -quantity*price),
                new ValueTestParameters( "EquityLongLimitOrder", equity, new LimitOrder(Symbols.SPY, quantity, priceMinusDelta, time), quantity*priceMinusDelta),
                new ValueTestParameters( "EquityShortLimit Order", equity, new LimitOrder(Symbols.SPY, -quantity, pricePlusDelta, time), -quantity*pricePlusDelta),
                new ValueTestParameters( "EquityLongStopLimitOrder", equity, new StopLimitOrder(Symbols.SPY, quantity,.5m*priceMinusDelta, priceMinusDelta, time), quantity*priceMinusDelta),
                new ValueTestParameters( "EquityShortStopLimitOrder", equity, new StopLimitOrder(Symbols.SPY, -quantity, 1.5m*pricePlusDelta, pricePlusDelta, time), -quantity*pricePlusDelta),
                new ValueTestParameters( "EquityLongStopMarketOrder", equity, new StopMarketOrder(Symbols.SPY, quantity, priceMinusDelta, time), quantity*priceMinusDelta),
                new ValueTestParameters( "EquityLongStopMarketOrder", equity, new StopMarketOrder(Symbols.SPY, quantity, pricePlusDelta, time), quantity*price),
                new ValueTestParameters( "EquityShortStopMarketOrder", equity, new StopMarketOrder(Symbols.SPY, -quantity, pricePlusDelta, time), -quantity*pricePlusDelta),
                new ValueTestParameters( "EquityShortStopMarketOrder", equity, new StopMarketOrder(Symbols.SPY, -quantity, priceMinusDelta, time), -quantity*price),

                // forex orders
                new ValueTestParameters( "ForexLongMarketOrder", forex, new MarketOrder(Symbols.EURGBP, quantity, time), quantity*price*forex.QuoteCurrency.ConversionRate),
                new ValueTestParameters( "ForexShortMarketOrder", forex, new MarketOrder(Symbols.EURGBP, -quantity, time), -quantity*price*forex.QuoteCurrency.ConversionRate),
                new ValueTestParameters( "ForexLongLimitOrder", forex, new LimitOrder(Symbols.EURGBP, quantity, priceMinusDelta, time), quantity*priceMinusDelta*forex.QuoteCurrency.ConversionRate),
                new ValueTestParameters( "ForexShortLimit Order", forex, new LimitOrder(Symbols.EURGBP, -quantity, pricePlusDelta, time), -quantity*pricePlusDelta*forex.QuoteCurrency.ConversionRate),
                new ValueTestParameters( "ForexLongStopLimitOrder", forex, new StopLimitOrder(Symbols.EURGBP, quantity,.5m*priceMinusDelta, priceMinusDelta, time), quantity*priceMinusDelta*forex.QuoteCurrency.ConversionRate),
                new ValueTestParameters( "ForexShortStopLimitOrder", forex, new StopLimitOrder(Symbols.EURGBP, -quantity, 1.5m*pricePlusDelta, pricePlusDelta, time), -quantity*pricePlusDelta*forex.QuoteCurrency.ConversionRate),
                new ValueTestParameters( "ForexLongStopMarketOrder", forex, new StopMarketOrder(Symbols.EURGBP, quantity, priceMinusDelta, time), quantity*priceMinusDelta*forex.QuoteCurrency.ConversionRate),
                new ValueTestParameters( "ForexLongStopMarketOrder", forex, new StopMarketOrder(Symbols.EURGBP, quantity, pricePlusDelta, time), quantity*price*forex.QuoteCurrency.ConversionRate),
                new ValueTestParameters( "ForexShortStopMarketOrder", forex, new StopMarketOrder(Symbols.EURGBP, -quantity, pricePlusDelta, time), -quantity*pricePlusDelta*forex.QuoteCurrency.ConversionRate),
                new ValueTestParameters( "ForexShortStopMarketOrder", forex, new StopMarketOrder(Symbols.EURGBP, -quantity, priceMinusDelta, time), -quantity*price*forex.QuoteCurrency.ConversionRate),
                
                // cfd orders
                new ValueTestParameters( "CfdLongMarketOrder", cfd, new MarketOrder(Symbols.DE10YBEUR, quantity, time), quantity*price*multiplierTimesConversionRate),
                new ValueTestParameters( "CfdShortMarketOrder", cfd, new MarketOrder(Symbols.DE10YBEUR, -quantity, time), -quantity*price*multiplierTimesConversionRate),
                new ValueTestParameters( "CfdLongLimitOrder", cfd, new LimitOrder(Symbols.DE10YBEUR, quantity, priceMinusDelta, time), quantity*priceMinusDelta*multiplierTimesConversionRate),
                new ValueTestParameters( "CfdShortLimit Order", cfd, new LimitOrder(Symbols.DE10YBEUR, -quantity, pricePlusDelta, time), -quantity*pricePlusDelta*multiplierTimesConversionRate),
                new ValueTestParameters( "CfdLongStopLimitOrder", cfd, new StopLimitOrder(Symbols.DE10YBEUR, quantity,.5m*priceMinusDelta, priceMinusDelta, time), quantity*priceMinusDelta*multiplierTimesConversionRate),
                new ValueTestParameters( "CfdShortStopLimitOrder", cfd, new StopLimitOrder(Symbols.DE10YBEUR, -quantity, 1.5m*pricePlusDelta, pricePlusDelta, time), -quantity*pricePlusDelta*multiplierTimesConversionRate),
                new ValueTestParameters( "CfdLongStopMarketOrder", cfd, new StopMarketOrder(Symbols.DE10YBEUR, quantity, priceMinusDelta, time), quantity*priceMinusDelta*multiplierTimesConversionRate),
                new ValueTestParameters( "CfdLongStopMarketOrder", cfd, new StopMarketOrder(Symbols.DE10YBEUR, quantity, pricePlusDelta, time), quantity*price*multiplierTimesConversionRate),
                new ValueTestParameters( "CfdShortStopMarketOrder", cfd, new StopMarketOrder(Symbols.DE10YBEUR, -quantity, pricePlusDelta, time), -quantity*pricePlusDelta*multiplierTimesConversionRate),
                new ValueTestParameters( "CfdShortStopMarketOrder", cfd, new StopMarketOrder(Symbols.DE10YBEUR, -quantity, priceMinusDelta, time), -quantity*price*multiplierTimesConversionRate),

            }.Select(x => new TestCaseData(x).SetName(x.Name)).ToArray();
        }

        public class ValueTestParameters
        {
            public readonly String Name;
            public readonly Security Security;
            public readonly Order Order;
            public readonly BigDecimal ExpectedValue;

            public ValueTestParameters( String name, Security security, Order order, BigDecimal expectedValue) {
                Name = name;
                Security = security;
                Order = order;
                ExpectedValue = expectedValue;
            }
        }
    }
}
