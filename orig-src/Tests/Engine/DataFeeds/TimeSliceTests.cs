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
using System.Linq;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Custom;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Securities;

namespace QuantConnect.Tests.Engine.DataFeeds
{
    [TestFixture]
    public class TimeSliceTests
    {
        [Test]
        public void HandlesTicks_ExpectInOrderWithNoDuplicates()
        {
            subscriptionDataConfig = new SubscriptionDataConfig(
                typeof(Tick), 
                Symbols.EURUSD, 
                Resolution.Tick, 
                TimeZones.Utc, 
                TimeZones.Utc, 
                true, 
                true, 
                false);

            security = new Security(
                SecurityExchangeHours.AlwaysOpen(TimeZones.Utc), 
                subscriptionDataConfig, 
                new Cash(CashBook.AccountCurrency, 0, 1m), 
                SymbolProperties.GetDefault(CashBook.AccountCurrency));

            DateTime refTime = DateTime.UtcNow;

            Tick[] rawTicks = Enumerable
                .Range(0, 10)
                .Select(i => new Tick(refTime.AddSeconds(i), Symbols.EURUSD, 1.3465m, 1.34652m))
                .ToArray();

            IEnumerable<TimeSlice> timeSlices = rawTicks.Select(t => TimeSlice.Create(
                t.Time,
                TimeZones.Utc,
                new CashBook(),
                new List<DataFeedPacket> {new DataFeedPacket(security, subscriptionDataConfig, new List<BaseData>() {t})},
                new SecurityChanges(Enumerable.Empty<Security>(), Enumerable.Empty<Security>())));

            Tick[] timeSliceTicks = timeSlices.SelectMany(ts => ts.Slice.Ticks.Values.SelectMany(x => x)).ToArray();

            Assert.AreEqual(rawTicks.Length, timeSliceTicks.Length);
            for (int i = 0; i < rawTicks.Length; i++)
            {
                Assert.IsTrue(Compare(rawTicks[i], timeSliceTicks[i]));
            }
        }

        private boolean Compare(Tick expected, Tick actual)
        {
            return expected.Time == actual.Time
                   && expected.BidPrice == actual.BidPrice
                   && expected.AskPrice == actual.AskPrice
                   && expected.Quantity == actual.Quantity;
        }

        [Test]
        public void HandlesMultipleCustomDataOfSameTypeWithDifferentSymbols()
        {
            symbol1 = Symbol.Create("SCF/CBOE_VX1_EW", SecurityType.Base, Market.USA);
            symbol2 = Symbol.Create("SCF/CBOE_VX2_EW", SecurityType.Base, Market.USA);

            subscriptionDataConfig1 = new SubscriptionDataConfig(
                typeof(QuandlFuture), symbol1, Resolution.Daily, TimeZones.Utc, TimeZones.Utc, true, true, false, isCustom: true);
            subscriptionDataConfig2 = new SubscriptionDataConfig(
                typeof(QuandlFuture), symbol2, Resolution.Daily, TimeZones.Utc, TimeZones.Utc, true, true, false, isCustom: true);

            security1 = new Security(
                SecurityExchangeHours.AlwaysOpen(TimeZones.Utc),
                subscriptionDataConfig1,
                new Cash(CashBook.AccountCurrency, 0, 1m),
                SymbolProperties.GetDefault(CashBook.AccountCurrency));

            security2 = new Security(
                SecurityExchangeHours.AlwaysOpen(TimeZones.Utc),
                subscriptionDataConfig1,
                new Cash(CashBook.AccountCurrency, 0, 1m),
                SymbolProperties.GetDefault(CashBook.AccountCurrency));

            timeSlice = TimeSlice.Create(DateTime.UtcNow, TimeZones.Utc, new CashBook(),
                new List<DataFeedPacket>
                {
                    new DataFeedPacket(security1, subscriptionDataConfig1, new List<BaseData> {new QuandlFuture { Symbol = symbol1, Time = DateTime.UtcNow.Date, Value = 15 } }),
                    new DataFeedPacket(security2, subscriptionDataConfig2, new List<BaseData> {new QuandlFuture { Symbol = symbol2, Time = DateTime.UtcNow.Date, Value = 20 } }),
                },
                new SecurityChanges(Enumerable.Empty<Security>(), Enumerable.Empty<Security>()));

            Assert.AreEqual(2, timeSlice.CustomData.Count);

            data1 = timeSlice.CustomData[0].Data[0];
            data2 = timeSlice.CustomData[1].Data[0];

            Assert.IsInstanceOf(typeof(QuandlFuture), data1);
            Assert.IsInstanceOf(typeof(QuandlFuture), data2);
            Assert.AreEqual(symbol1, data1.Symbol);
            Assert.AreEqual(symbol2, data2.Symbol);
            Assert.AreEqual(15, data1.Value);
            Assert.AreEqual(20, data2.Value);
        }

        [Test]
        public void HandlesMultipleCustomDataOfSameTypeSameSymbol()
        {
            symbol = Symbol.Create("DFX", SecurityType.Base, Market.USA);

            subscriptionDataConfig = new SubscriptionDataConfig(
                typeof(DailyFx), symbol, Resolution.Daily, TimeZones.Utc, TimeZones.Utc, true, true, false, isCustom: true);

            security = new Security(
                SecurityExchangeHours.AlwaysOpen(TimeZones.Utc),
                subscriptionDataConfig,
                new Cash(CashBook.AccountCurrency, 0, 1m),
                SymbolProperties.GetDefault(CashBook.AccountCurrency));

            refTime = DateTime.UtcNow;

            timeSlice = TimeSlice.Create(refTime, TimeZones.Utc, new CashBook(),
                new List<DataFeedPacket>
                {
                    new DataFeedPacket(security, subscriptionDataConfig, new List<BaseData>
                    {
                        new DailyFx { Symbol = symbol, Time = refTime, Title = "Item 1" },
                        new DailyFx { Symbol = symbol, Time = refTime, Title = "Item 2" },
                    }),
                },
                new SecurityChanges(Enumerable.Empty<Security>(), Enumerable.Empty<Security>()));

            Assert.AreEqual(1, timeSlice.CustomData.Count);

            data1 = timeSlice.CustomData[0].Data[0];
            data2 = timeSlice.CustomData[0].Data[1];

            Assert.IsInstanceOf(typeof(DailyFx), data1);
            Assert.IsInstanceOf(typeof(DailyFx), data2);
            Assert.AreEqual(symbol, data1.Symbol);
            Assert.AreEqual(symbol, data2.Symbol);
            Assert.AreEqual("Item 1", ((DailyFx)data1).Title);
            Assert.AreEqual("Item 2", ((DailyFx)data2).Title);
        }

    }
}
