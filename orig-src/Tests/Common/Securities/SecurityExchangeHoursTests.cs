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
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Common.Securities
{
    [TestFixture]
    public class SecurityExchangeHoursTests
    {
        [Test]
        public void StartIsOpen()
        {
            exchangeHours = CreateForexSecurityExchangeHours();

            date = new DateTime(2015, 6, 21);
            marketOpen = exchangeHours.MarketHours[DayOfWeek.Sunday].GetMarketOpen(TimeSpan.Zero, false);
            Assert.IsTrue(marketOpen.HasValue);
            time = (date + marketOpen.Value).AddTicks(-1);
            Assert.IsFalse(exchangeHours.IsOpen(time, false));

            time = time + TimeSpan.FromTicks(1);
            Assert.IsTrue(exchangeHours.IsOpen(time, false));
        }

        [Test]
        public void EndIsClosed()
        {
            exchangeHours = CreateForexSecurityExchangeHours();

            date = new DateTime(2015, 6, 19);
            localMarketHours = exchangeHours.MarketHours[DayOfWeek.Friday];
            marketClose = localMarketHours.GetMarketClose(TimeSpan.Zero, false);
            Assert.IsTrue(marketClose.HasValue);
            time = (date + marketClose.Value).AddTicks(-1);
            Assert.IsTrue(exchangeHours.IsOpen(time, false));

            time = time + TimeSpan.FromTicks(1);
            Assert.IsFalse(exchangeHours.IsOpen(time, false));
        }

        [Test]
        public void IntervalOverlappingStartIsOpen()
        {
            exchangeHours = CreateForexSecurityExchangeHours();

            date = new DateTime(2015, 6, 21);
            marketOpen = exchangeHours.MarketHours[DayOfWeek.Sunday].GetMarketOpen(TimeSpan.Zero, false);
            Assert.IsTrue(marketOpen.HasValue);
            startTime = (date + marketOpen.Value).AddMinutes(-1);

            Assert.IsFalse(exchangeHours.IsOpen(startTime, startTime.AddMinutes(1), false));

            // now the end is 1 tick after open, should return true
            startTime = startTime + TimeSpan.FromTicks(1);
            Assert.IsTrue(exchangeHours.IsOpen(startTime, startTime.AddMinutes(1), false));
        }

        [Test]
        public void IntervalOverlappingEndIsOpen()
        {
            exchangeHours = CreateForexSecurityExchangeHours();

            date = new DateTime(2015, 6, 19);
            marketClose = exchangeHours.MarketHours[DayOfWeek.Friday].GetMarketClose(TimeSpan.Zero, false);
            Assert.IsTrue(marketClose.HasValue);
            startTime = (date + marketClose.Value).AddMinutes(-1);

            Assert.IsTrue(exchangeHours.IsOpen(startTime, startTime.AddMinutes(1), false));

            // now the start is on the close, returns false
            startTime = startTime.AddMinutes(1);
            Assert.IsFalse(exchangeHours.IsOpen(startTime, startTime.AddMinutes(1), false));
        }

        [Test]
        public void MultiDayInterval()
        {
            exchangeHours = CreateForexSecurityExchangeHours();

            date = new DateTime(2015, 6, 19);
            marketClose = exchangeHours.MarketHours[DayOfWeek.Friday].GetMarketClose(TimeSpan.Zero, false);
            Assert.IsTrue(marketClose.HasValue);
            startTime = date + marketClose.Value;

            Assert.IsFalse(exchangeHours.IsOpen(startTime, startTime.AddDays(2), false));

            // if we back up one tick it means the bar started at the last moment before market close, this should be included
            Assert.IsTrue(exchangeHours.IsOpen(startTime.AddTicks(-1), startTime.AddDays(2).AddTicks(-1), false));

            // if we advance one tick, it means the bar closed in the first moment after market open
            Assert.IsTrue(exchangeHours.IsOpen(startTime.AddTicks(1), startTime.AddDays(2).AddTicks(1), false));
        }

        [Test]
        public void GetNextMarketOpenIsNonInclusiveOfStartTime()
        {
            exhangeHours = CreateUsEquitySecurityExchangeHours();

            startTime = new DateTime(2015, 6, 30, 9, 30, 0);
            nextMarketOpen = exhangeHours.GetNextMarketOpen(startTime, false);
            Assert.AreEqual(startTime.AddDays(1), nextMarketOpen);
        }

        [Test]
        public void GetNextMarketOpenWorksOverWeekends()
        {
            exhangeHours = CreateUsEquitySecurityExchangeHours();

            startTime = new DateTime(2015, 6, 26, 9, 30, 1);
            nextMarketOpen = exhangeHours.GetNextMarketOpen(startTime, false);
            Assert.AreEqual(new DateTime(2015, 6, 29, 9, 30, 0), nextMarketOpen);
        }

        [Test]
        public void GetNextMarketCloseIsNonInclusiveOfStartTime()
        {
            exhangeHours = CreateUsEquitySecurityExchangeHours();

            startTime = new DateTime(2015, 6, 30, 16, 0, 0);
            nextMarketOpen = exhangeHours.GetNextMarketClose(startTime, false);
            Assert.AreEqual(startTime.AddDays(1), nextMarketOpen);
        }

        [Test]
        public void GetNextMarketCloseWorksOverWeekends()
        {
            exhangeHours = CreateUsEquitySecurityExchangeHours();

            startTime = new DateTime(2015, 6, 26, 16, 0, 1);
            nextMarketClose = exhangeHours.GetNextMarketClose(startTime, false);
            Assert.AreEqual(new DateTime(2015, 6, 29, 16, 0, 0), nextMarketClose);
        }

        [Test]
        public void Benchmark()
        {
            forex = CreateForexSecurityExchangeHours();

            reference = new DateTime(1991, 06, 20);
            forex.IsOpen(reference, false);
            forex.IsOpen(reference, reference.AddDays(1), false);

            static final int length = 1000*1000*1;

            stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < length; i++)
            {
                forex.IsOpen(reference.AddMinutes(1), false);
            }
            stopwatch.Stop();

            Console.WriteLine("forex1: " + stopwatch.Elapsed);
        }

        public static SecurityExchangeHours CreateForexSecurityExchangeHours()
        {
            sunday = new LocalMarketHours(DayOfWeek.Sunday, new TimeSpan(17, 0, 0), TimeSpan.FromTicks(Time.OneDay.Ticks - 1));
            monday = LocalMarketHours.OpenAllDay(DayOfWeek.Monday);
            tuesday = LocalMarketHours.OpenAllDay(DayOfWeek.Tuesday);
            wednesday = LocalMarketHours.OpenAllDay(DayOfWeek.Wednesday);
            thursday = LocalMarketHours.OpenAllDay(DayOfWeek.Thursday);
            friday = new LocalMarketHours(DayOfWeek.Friday, TimeSpan.Zero, new TimeSpan(17, 0, 0));
            saturday = LocalMarketHours.ClosedAllDay(DayOfWeek.Saturday);

            exchangeHours = new SecurityExchangeHours(TimeZones.NewYork, USHoliday.Dates.Select(x => x.Date), new[]
            {
                sunday, monday, tuesday, wednesday, thursday, friday//, saturday
            }.ToDictionary(x => x.DayOfWeek));
            return exchangeHours;
        }

        public static SecurityExchangeHours CreateUsEquitySecurityExchangeHours()
        {
            sunday = LocalMarketHours.ClosedAllDay(DayOfWeek.Sunday);
            monday = new LocalMarketHours(DayOfWeek.Monday, new TimeSpan(9, 30, 0), new TimeSpan(16, 0, 0));
            tuesday = new LocalMarketHours(DayOfWeek.Tuesday, new TimeSpan(9, 30, 0), new TimeSpan(16, 0, 0));
            wednesday = new LocalMarketHours(DayOfWeek.Wednesday, new TimeSpan(9, 30, 0), new TimeSpan(16, 0, 0));
            thursday = new LocalMarketHours(DayOfWeek.Thursday, new TimeSpan(9, 30, 0), new TimeSpan(16, 0, 0));
            friday = new LocalMarketHours(DayOfWeek.Friday, new TimeSpan(9, 30, 0), new TimeSpan(16, 0, 0));
            saturday = LocalMarketHours.ClosedAllDay(DayOfWeek.Saturday);

            exchangeHours = new SecurityExchangeHours(TimeZones.NewYork, USHoliday.Dates.Select(x => x.Date), new[]
            {
                sunday, monday, tuesday, wednesday, thursday, friday, saturday
            }.ToDictionary(x => x.DayOfWeek));
            return exchangeHours;
        }
    }
}
