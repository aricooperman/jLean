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

using System;
using System.Collections.Generic;
using NodaTime;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Scheduling;
using QuantConnect.Securities;

namespace QuantConnect.Tests.Common.Scheduling
{
    [TestFixture]
    public class TimeRulesTests
    {
        [Test]
        public void AtSpecificTimeFromUtc()
        {
            rules = GetTimeRules(TimeZones.Utc);
            rule = rules.At(TimeSpan.FromHours(12));
            times = rule.CreateUtcEventTimes(new[] {new DateTime(2000, 01, 01)});

            int count = 0;
            foreach (time in times)
            {
                count++;
                Assert.AreEqual(TimeSpan.FromHours(12), time.TimeOfDay);
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void AtSpecificTimeFromNonUtc()
        {
            rules = GetTimeRules(TimeZones.NewYork);
            rule = rules.At(TimeSpan.FromHours(12));
            times = rule.CreateUtcEventTimes(new[] {new DateTime(2000, 01, 01)});

            int count = 0;
            foreach (time in times)
            {
                count++;
                Assert.AreEqual(TimeSpan.FromHours(12+5), time.TimeOfDay);
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void RegularMarketOpenNoDelta()
        {
            rules = GetTimeRules(TimeZones.Utc);
            rule = rules.AfterMarketOpen(Symbols.SPY);
            times = rule.CreateUtcEventTimes(new[] { new DateTime(2000, 01, 03) });

            int count = 0;
            foreach (time in times)
            {
                count++;
                Assert.AreEqual(TimeSpan.FromHours(9.5 + 5), time.TimeOfDay);
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void RegularMarketOpenWithDelta()
        {
            rules = GetTimeRules(TimeZones.Utc);
            rule = rules.AfterMarketOpen(Symbols.SPY, 30);
            times = rule.CreateUtcEventTimes(new[] { new DateTime(2000, 01, 03) });

            int count = 0;
            foreach (time in times)
            {
                count++;
                Assert.AreEqual(TimeSpan.FromHours(9.5 + 5 + .5), time.TimeOfDay);
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ExtendedMarketOpenNoDelta()
        {
            rules = GetTimeRules(TimeZones.Utc);
            rule = rules.AfterMarketOpen(Symbols.SPY, 0, true);
            times = rule.CreateUtcEventTimes(new[] { new DateTime(2000, 01, 03) });

            int count = 0;
            foreach (time in times)
            {
                count++;
                Assert.AreEqual(TimeSpan.FromHours(4 + 5), time.TimeOfDay);
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ExtendedMarketOpenWithDelta()
        {
            rules = GetTimeRules(TimeZones.Utc);
            rule = rules.AfterMarketOpen(Symbols.SPY, 30, true);
            times = rule.CreateUtcEventTimes(new[] { new DateTime(2000, 01, 03) });

            int count = 0;
            foreach (time in times)
            {
                count++;
                Assert.AreEqual(TimeSpan.FromHours(4 + 5 + .5), time.TimeOfDay);
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void RegularMarketCloseNoDelta()
        {
            rules = GetTimeRules(TimeZones.Utc);
            rule = rules.BeforeMarketClose(Symbols.SPY);
            times = rule.CreateUtcEventTimes(new[] { new DateTime(2000, 01, 03) });

            int count = 0;
            foreach (time in times)
            {
                count++;
                Assert.AreEqual(TimeSpan.FromHours(16 + 5), time.TimeOfDay);
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void RegularMarketCloseWithDelta()
        {
            rules = GetTimeRules(TimeZones.Utc);
            rule = rules.BeforeMarketClose(Symbols.SPY, 30);
            times = rule.CreateUtcEventTimes(new[] { new DateTime(2000, 01, 03) });

            int count = 0;
            foreach (time in times)
            {
                count++;
                Assert.AreEqual(TimeSpan.FromHours(16 + 5 - .5), time.TimeOfDay);
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ExtendedMarketCloseNoDelta()
        {
            rules = GetTimeRules(TimeZones.Utc);
            rule = rules.BeforeMarketClose(Symbols.SPY, 0, true);
            times = rule.CreateUtcEventTimes(new[] { new DateTime(2000, 01, 03) });

            int count = 0;
            foreach (time in times)
            {
                count++;
                Assert.AreEqual(TimeSpan.FromHours((20 + 5)%24), time.TimeOfDay);
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ExtendedMarketCloseWithDelta()
        {
            rules = GetTimeRules(TimeZones.Utc);
            rule = rules.BeforeMarketClose(Symbols.SPY, 30, true);
            times = rule.CreateUtcEventTimes(new[] { new DateTime(2000, 01, 03) });
            
            int count = 0;
            foreach (time in times)
            {
                count++;
                Assert.AreEqual(TimeSpan.FromHours((20 + 5 - .5)%24), time.TimeOfDay);
            }
            Assert.AreEqual(1, count);
        }

        private static TimeRules GetTimeRules(DateTimeZone dateTimeZone)
        {
            timeKeeper = new TimeKeeper(DateTime.Today, new List<DateTimeZone>());
            manager = new SecurityManager(timeKeeper);
            marketHourDbEntry = MarketHoursDatabase.FromDataFolder().GetEntry(Market.USA, null, SecurityType.Equity);
            securityExchangeHours = marketHourDbEntry.ExchangeHours;
            config = new SubscriptionDataConfig(typeof(TradeBar), Symbols.SPY, Resolution.Daily, marketHourDbEntry.DataTimeZone, securityExchangeHours.TimeZone, true, false, false);
            manager.Add(Symbols.SPY, new Security(securityExchangeHours, config, new Cash(CashBook.AccountCurrency, 0, 1m), SymbolProperties.GetDefault(CashBook.AccountCurrency)));
            rules = new TimeRules(manager, dateTimeZone);
            return rules;
        }
    }
}
