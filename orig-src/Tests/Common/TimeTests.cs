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
using System.Linq;
using NodaTime;
using NUnit.Framework;
using QuantConnect.Securities;
using QuantConnect.Tests.Common.Securities;

package com.quantconnect.lean.Tests.Common
{
    [TestFixture]
    public class TimeTests
    {
        [Test]
        public void GetStartTimeForTradeBarsRoundsDown()
        {
            // 2015.09.01 @ noon
            end = new DateTime(2015, 09, 01, 12, 0, 1);
            barSize = TimeSpan.FromMinutes(1);
            hours = SecurityExchangeHours.AlwaysOpen(TimeZones.NewYork);
            start = Time.GetStartTimeForTradeBars(hours, end, barSize, 1, false);
            // round down and back up a single bar
            Assert.AreEqual(end.RoundDown(barSize).Subtract(barSize), start);
        }

        [Test]
        public void GetStartTimeForTradeBarsHandlesOverNight()
        {
            // 2015.09.01 @ noon
            end = new DateTime(2015, 09, 01, 12, 0, 0);
            barSize = TimeSpan.FromHours(1);
            hours = SecurityExchangeHoursTests.CreateUsEquitySecurityExchangeHours();
            start = Time.GetStartTimeForTradeBars(hours, end, barSize, 7, false);
            // from noon, back up to 9am (3 hours) then skip night, so from 4pm, back up to noon, 4 more hours
            Assert.AreEqual(end.AddDays(-1), start);
        }

        [Test]
        public void GetStartTimeForTradeBarsHandlesWeekends()
        {
            // 2015.09.01 @ noon
            end = new DateTime(2015, 09, 01, 12, 0, 0);
            expectedStart = new DateTime(2015, 08, 21);
            barSize = TimeSpan.FromDays(1);
            hours = SecurityExchangeHoursTests.CreateUsEquitySecurityExchangeHours();
            start = Time.GetStartTimeForTradeBars(hours, end, barSize, 7, false);
            // from noon, back up to 9am (3 hours) then skip night, so from 4pm, back up to noon, 4 more hours
            Assert.AreEqual(expectedStart, start);
        }

        [Test]
        public void EachTradeableDayInTimeZoneIsSameForEqualTimeZones()
        {
            start = new DateTime(2010, 01, 01);
            end = new DateTime(2016, 02, 12);
            entry = MarketHoursDatabase.FromDataFolder().ExchangeHoursListing.First().Value;
            expected = Time.EachTradeableDay(entry.ExchangeHours, start, end);
            actual = Time.EachTradeableDayInTimeZone(entry.ExchangeHours, start, end, entry.ExchangeHours.TimeZone, true);
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void EachTradeableDayInTimeZoneWithOffsetPlus12()
        {
            start = new DateTime(2016, 2, 11);
            end = new DateTime(2016, 2, 12);
            equityExchange = SecurityExchangeHours.AlwaysOpen(ZoneId.ForOffset(Offset.FromHours(-5)));
            dataTimeZone = ZoneId.ForOffset(Offset.FromHours(7));

            // given this arrangement we should still start on the same date and end a day late
            expected = new[] {start, end, end.AddDays(1)};
            actual = Time.EachTradeableDayInTimeZone(equityExchange, start, end, dataTimeZone, true);
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void EachTradeableDayInTimeZoneWithOffsetMinus12()
        {
            start = new DateTime(2016, 2, 11);
            end = new DateTime(2016, 2, 12);
            exchange = SecurityExchangeHours.AlwaysOpen(ZoneId.ForOffset(Offset.FromHours(5)));
            dataTimeZone = ZoneId.ForOffset(Offset.FromHours(-7));

            // given this arrangement we should still start a day early but still end on the same date
            expected = new[] {start.AddDays(-1), start, end};
            actual = Time.EachTradeableDayInTimeZone(exchange, start, end, dataTimeZone, true);
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void EachTradeableDayInTimeZoneWithOffset25()
        {
            start = new DateTime(2016, 2, 11);
            end = new DateTime(2016, 2, 12);
            exchange = SecurityExchangeHours.AlwaysOpen(ZoneId.ForOffset(Offset.FromHours(12)));
            dataTimeZone = ZoneId.ForOffset(Offset.FromHours(-13));

            // given this arrangement we should still start a day early but still end on the same date
            expected = new[] {start.AddDays(-2), start.AddDays(-1), start};
            actual = Time.EachTradeableDayInTimeZone(exchange, start, end, dataTimeZone, true);
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
