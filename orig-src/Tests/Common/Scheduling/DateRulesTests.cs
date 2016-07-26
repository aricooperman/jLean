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
 *
*/

using System;
using System.Collections.Generic;
using NodaTime;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Scheduling;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Common.Scheduling
{
    [TestFixture]
    public class DateRulesTests
    {
        [Test]
        public void EveryDayDateRuleEmitsEveryDay()
        {
            rules = GetDateRules();
            rule = rules.EveryDay();
            dates = rule.GetDates(new DateTime(2000, 01, 01), new DateTime(2000, 12, 31));

            int count = 0;
            DateTime previous = DateTime.MinValue;
            foreach (date in dates)
            {
                count++;
                if (previous != DateTime.MinValue)
                {
                    Assert.AreEqual(Time.OneDay, date - previous);
                }
                previous = date;
            }
            // leap year
            Assert.AreEqual(366, count);
        }

        [Test]
        public void EverySymbolDayRuleEmitsOnTradeableDates()
        {
            rules = GetDateRules();
            rule = rules.EveryDay(Symbols.SPY);
            dates = rule.GetDates(new DateTime(2000, 01, 01), new DateTime(2000, 12, 31));

            int count = 0;
            foreach (date in dates)
            {
                count++;
                Assert.AreNotEqual(DayOfWeek.Saturday, date.DayOfWeek);
                Assert.AreNotEqual(DayOfWeek.Sunday, date.DayOfWeek);
            }

            Assert.AreEqual(252, count);
        }

        [Test]
        public void StartOfMonthNoSymbol()
        {
            rules = GetDateRules();
            rule = rules.MonthStart();
            dates = rule.GetDates(new DateTime(2000, 01, 01), new DateTime(2000, 12, 31));

            int count = 0;
            foreach (date in dates)
            {
                count++;
                Assert.AreEqual(1, date.Day);
            }

            Assert.AreEqual(12, count);
        }

        [Test]
        public void StartOfMonthNoSymbolMidMonthStart()
        {
            rules = GetDateRules();
            rule = rules.MonthStart();
            dates = rule.GetDates(new DateTime(2000, 01, 04), new DateTime(2000, 12, 31));

            int count = 0;
            foreach (date in dates)
            {
                count++;
                Assert.AreEqual(1, date.Day);
            }

            Assert.AreEqual(11, count);
        }

        [Test]
        public void StartOfMonthWithSymbol()
        {
            rules = GetDateRules();
            rule = rules.MonthStart(Symbols.SPY);
            dates = rule.GetDates(new DateTime(2000, 01, 01), new DateTime(2000, 12, 31));

            int count = 0;
            foreach (date in dates)
            {
                count++;
                Assert.AreNotEqual(DayOfWeek.Saturday, date.DayOfWeek);
                Assert.AreNotEqual(DayOfWeek.Sunday, date.DayOfWeek);
                Assert.IsTrue(date.Day <= 3);
                Console.WriteLine(date.Day);
            }

            Assert.AreEqual(12, count);
        }

        [Test]
        public void StartOfMonthWithSymbolMidMonthStart()
        {
            rules = GetDateRules();
            rule = rules.MonthStart(Symbols.SPY);
            dates = rule.GetDates(new DateTime(2000, 01, 04), new DateTime(2000, 12, 31));

            int count = 0;
            foreach (date in dates)
            {
                count++;
                Assert.AreNotEqual(DayOfWeek.Saturday, date.DayOfWeek);
                Assert.AreNotEqual(DayOfWeek.Sunday, date.DayOfWeek);
                Assert.IsTrue(date.Day <= 3);
                Console.WriteLine(date.Day);
            }

            Assert.AreEqual(11, count);
        }

        private static DateRules GetDateRules()
        {
            timeKeeper = new TimeKeeper(DateTime.Today, new List<DateTimeZone>());
            manager = new SecurityManager(timeKeeper);
            securityExchangeHours = MarketHoursDatabase.FromDataFolder().GetExchangeHours(Market.USA, null, SecurityType.Equity);
            config = new SubscriptionDataConfig(typeof(TradeBar), Symbols.SPY, Resolution.Daily, TimeZones.NewYork, TimeZones.NewYork, true, false, false);
            manager.Add(Symbols.SPY, new Security(securityExchangeHours, config, new Cash(CashBook.AccountCurrency, 0, 1m), SymbolProperties.GetDefault(CashBook.AccountCurrency)));
            rules = new DateRules(manager);
            return rules;
        }
    }
}
