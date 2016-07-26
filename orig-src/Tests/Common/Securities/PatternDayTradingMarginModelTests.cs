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
using System.Linq;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Common.Securities
{
    [TestFixture]
    public class PatternDayTradingMarginModelTests
    {
        private static readonly DateTime Noon = new DateTime(2016, 02, 16, 12, 0, 0);
        private static readonly DateTime Midnight = new DateTime(2016, 02, 16, 0, 0, 0);
        private static readonly DateTime NoonWeekend = new DateTime(2016, 02, 14, 12, 0, 0);
        private static readonly DateTime NoonHoliday = new DateTime(2016, 02, 15, 12, 0, 0);
        private static readonly TimeKeeper TimeKeeper = new TimeKeeper(Noon.ConvertToUtc(TimeZones.NewYork), TimeZones.NewYork);

        [Test]
        public void InitializationTests()
        {
            // No parameters initialization, used default PDT 4x leverage open market and 2x leverage otherwise
            model = new PatternDayTradingMarginModel();
            leverage = model.GetLeverage(CreateSecurity(Noon));

            Assert.AreEqual(4.0m, leverage);

            model = new PatternDayTradingMarginModel(2.0m, 5.0m);
            leverage = model.GetLeverage(CreateSecurity(Noon));

            Assert.AreEqual(5.0m, leverage);
        }

        [Test]
        public void SetLeverageTest()
        {
            model = new PatternDayTradingMarginModel();

            // Open market
            security = CreateSecurity(Noon);

            security.MarginModel = new PatternDayTradingMarginModel();

            model.SetLeverage(security, 10m);
            Assert.AreNotEqual(10m, model.GetLeverage(security));

            // Closed market
            security = CreateSecurity(Midnight);

            model.SetLeverage(security, 10m);
            Assert.AreNotEqual(10m, model.GetLeverage(security));

            security.Holdings.SetHoldings(100m, 100);
        }

        [Test]
        public void VerifyOpenMarketLeverage()
        {
            // Market is Open on Tuesday, Feb, 16th 2016 at Noon

            leverage = 4m;
            expected = 100 * 100m / leverage + 1;

            model = new PatternDayTradingMarginModel();
            security = CreateSecurity(Noon);
            order = new MarketOrder(security.Symbol, 100, security.LocalTime);

            Assert.AreEqual((double)leverage, (double)model.GetLeverage(security), 1e-3);
            Assert.AreEqual((double)expected, (double)model.GetInitialMarginRequiredForOrder(security, order), 1e-3);
        }

        [Test]
        public void VerifyOpenMarketLeverageAltVersion()
        {
            // Market is Open on Tuesday, Feb, 16th 2016 at Noon

            leverage = 5m;
            expected = 100 * 100m / leverage + 1;

            model = new PatternDayTradingMarginModel(2m, leverage);
            security = CreateSecurity(Noon);
            order = new MarketOrder(security.Symbol, 100, security.LocalTime);

            Assert.AreEqual((double)leverage, (double)model.GetLeverage(security), 1e-3);
            Assert.AreEqual((double)expected, (double)model.GetInitialMarginRequiredForOrder(security, order), 1e-3);
        }

        [Test]
        public void VerifyClosedMarketLeverage()
        {
            leverage = 2m;
            expected = 100 * 100m / leverage + 1;

            model = new PatternDayTradingMarginModel();

            // Market is Closed on Tuesday, Feb, 16th 2016 at Midnight
            security = CreateSecurity(Midnight);
            order = new MarketOrder(security.Symbol, 100, security.LocalTime);

            Assert.AreEqual((double)leverage, (double)model.GetLeverage(security), 1e-3);
            Assert.AreEqual((double)expected, (double)model.GetInitialMarginRequiredForOrder(security, order), 1e-3);

            // Market is Closed on Monday, Feb, 15th 2016 at Noon (US President Day)
            security = CreateSecurity(NoonHoliday);
            order = new MarketOrder(security.Symbol, 100, security.LocalTime);

            Assert.AreEqual((double)leverage, (double)model.GetLeverage(security), 1e-3);
            Assert.AreEqual((double)expected, (double)model.GetInitialMarginRequiredForOrder(security, order), 1e-3);

            // Market is Closed on Sunday, Feb, 14th 2016 at Noon
            security = CreateSecurity(NoonWeekend);
            order = new MarketOrder(security.Symbol, 100, security.LocalTime);

            Assert.AreEqual((double)leverage, (double)model.GetLeverage(security), 1e-3);
            Assert.AreEqual((double)expected, (double)model.GetInitialMarginRequiredForOrder(security, order), 1e-3);
        }

        [Test]
        public void VerifyClosedMarketLeverageAltVersion()
        {
            leverage = 3m;
            expected = 100 * 100m / leverage + 1;

            model = new PatternDayTradingMarginModel(leverage, 4m);

            // Market is Closed on Tuesday, Feb, 16th 2016 at Midnight
            security = CreateSecurity(Midnight);
            order = new MarketOrder(security.Symbol, 100, security.LocalTime);

            Assert.AreEqual((double)leverage, (double)model.GetLeverage(security), 1e-3);
            Assert.AreEqual((double)expected, (double)model.GetInitialMarginRequiredForOrder(security, order), 1e-3);

            // Market is Closed on Monday, Feb, 15th 2016 at Noon (US President Day)
            security = CreateSecurity(NoonHoliday);
            order = new MarketOrder(security.Symbol, 100, security.LocalTime);

            Assert.AreEqual((double)leverage, (double)model.GetLeverage(security), 1e-3);
            Assert.AreEqual((double)expected, (double)model.GetInitialMarginRequiredForOrder(security, order), 1e-3);

            // Market is Closed on Sunday, Feb, 14th 2016 at Noon
            security = CreateSecurity(NoonWeekend);
            order = new MarketOrder(security.Symbol, 100, security.LocalTime);

            Assert.AreEqual((double)leverage, (double)model.GetLeverage(security), 1e-3);
            Assert.AreEqual((double)expected, (double)model.GetInitialMarginRequiredForOrder(security, order), 1e-3);
        }

        [Test]
        public void VerifyMaintenaceMargin()
        {
            model = new PatternDayTradingMarginModel();

            // Open Market
            security = CreateSecurity(Noon);
            security.Holdings.SetHoldings(100m, 100);

            Assert.AreEqual((double)100 * 100 / 4, (double)model.GetMaintenanceMargin(security), 1e-3);

            // Closed Market
            security = CreateSecurity(Midnight);
            security.Holdings.SetHoldings(100m, 100);

            Assert.AreEqual((double)100 * 100 / 2, (double)model.GetMaintenanceMargin(security), 1e-3);
        }

        [Test]
        public void VerifyMarginCallOrderLong()
        {
            netLiquidationValue = 5000m;
            totalMargin = 10000m;
            securityPrice = 100m;
            quantity = 300;

            model = new PatternDayTradingMarginModel();

            // Open Market
            security = CreateSecurity(Noon);
            security.Holdings.SetHoldings(securityPrice, quantity);

            expected = -(int)(Math.Round((totalMargin - netLiquidationValue) / securityPrice, MidpointRounding.AwayFromZero) * 4m);
            actual = model.GenerateMarginCallOrder(security, netLiquidationValue, totalMargin).Quantity;

            Assert.AreEqual(expected, actual);

            // Closed Market
            security = CreateSecurity(Midnight);
            security.Holdings.SetHoldings(securityPrice, quantity);

            expected = -(int)(Math.Round((totalMargin - netLiquidationValue) / securityPrice, MidpointRounding.AwayFromZero) * 2m);
            actual = model.GenerateMarginCallOrder(security, netLiquidationValue, totalMargin).Quantity;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void VerifyMarginCallOrderShort()
        {
            netLiquidationValue = 5000m;
            totalMargin = 10000m;
            securityPrice = 100m;
            quantity = -300;

            model = new PatternDayTradingMarginModel();

            // Open Market
            security = CreateSecurity(Noon);
            security.Holdings.SetHoldings(securityPrice, quantity);

            expected = (int)(Math.Round((totalMargin - netLiquidationValue) / securityPrice, MidpointRounding.AwayFromZero) * 4m);
            actual = model.GenerateMarginCallOrder(security, netLiquidationValue, totalMargin).Quantity;

            Assert.AreEqual(expected, actual);

            // Closed Market
            security = CreateSecurity(Midnight);
            security.Holdings.SetHoldings(securityPrice, quantity);

            expected = (int)(Math.Round((totalMargin - netLiquidationValue) / securityPrice, MidpointRounding.AwayFromZero) * 2m);
            actual = model.GenerateMarginCallOrder(security, netLiquidationValue, totalMargin).Quantity;

            Assert.AreEqual(expected, actual);
        }

        private static Security CreateSecurity(DateTime newLocalTime)
        {
            security = new Security(CreateUsEquitySecurityExchangeHours(), CreateTradeBarConfig(), new Cash(CashBook.AccountCurrency, 0, 1m), SymbolProperties.GetDefault(CashBook.AccountCurrency));
            TimeKeeper.SetUtcDateTime(newLocalTime.ConvertToUtc(security.Exchange.TimeZone));
            security.Exchange.SetLocalDateTimeFrontier(newLocalTime);
            security.SetLocalTimeKeeper(TimeKeeper.GetLocalTimeKeeper(TimeZones.NewYork));
            security.SetMarketPrice(new IndicatorDataPoint(Symbols.SPY, newLocalTime, 100m));
            return security;
        }

        private static SecurityExchangeHours CreateUsEquitySecurityExchangeHours()
        {
            sunday = LocalMarketHours.ClosedAllDay(DayOfWeek.Sunday);
            monday = new LocalMarketHours(DayOfWeek.Monday, new TimeSpan(9, 30, 0), new TimeSpan(16, 0, 0));
            tuesday = new LocalMarketHours(DayOfWeek.Tuesday, new TimeSpan(9, 30, 0), new TimeSpan(16, 0, 0));
            wednesday = new LocalMarketHours(DayOfWeek.Wednesday, new TimeSpan(9, 30, 0), new TimeSpan(16, 0, 0));
            thursday = new LocalMarketHours(DayOfWeek.Thursday, new TimeSpan(9, 30, 0), new TimeSpan(16, 0, 0));
            friday = new LocalMarketHours(DayOfWeek.Friday, new TimeSpan(9, 30, 0), new TimeSpan(16, 0, 0));
            saturday = LocalMarketHours.ClosedAllDay(DayOfWeek.Saturday);

            return new SecurityExchangeHours(TimeZones.NewYork, USHoliday.Dates.Select(x => x.Date), new[]
            {
                sunday, monday, tuesday, wednesday, thursday, friday, saturday
            }.ToDictionary(x => x.DayOfWeek));
        }

        private static SubscriptionDataConfig CreateTradeBarConfig()
        {
            return new SubscriptionDataConfig(typeof(TradeBar), Symbols.SPY, Resolution.Minute, TimeZones.NewYork,
                TimeZones.NewYork, true, true, false);
        }
    }
}