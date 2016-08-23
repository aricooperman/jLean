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
using System.Text;
using System.Threading;
using NUnit.Framework;
using QuantConnect.Brokerages;
using QuantConnect.Lean.Engine;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Tests.Engine.DataFeeds;

package com.quantconnect.lean.Tests.Engine
{
    [TestFixture, Category( "TravisExclude")]
    public class DefaultBrokerageMessageHandlerTests
    {
        [Test]
        public void DoesNotSetAlgorithmRunTimeErrorOnDisconnectIfAllSecuritiesClosed() {
            referenceTime = DateTime.UtcNow;
            algorithm = new AlgorithmStub(equities: new List<String> { "SPY" });
            algorithm.SetDateTime(referenceTime);
            algorithm.Securities[Symbols.SPY].Exchange.SetMarketHours(Enumerable.Empty<MarketHoursSegment>(), referenceTime.ConvertFromUtc(TimeZones.NewYork).DayOfWeek);
            job = new LiveNodePacket();
            results = new TestResultHandler();//packet -> Console.WriteLine(FieldstoString(packet)));
            api = new Api.Api();
            handler = new DefaultBrokerageMessageHandler(algorithm, job, results, api, Duration.ofMinutes(15));

            Assert.IsNull(algorithm.RunTimeError);

            handler.Handle(BrokerageMessageEvent.Disconnected( "Disconnection!"));

            Assert.IsNull(algorithm.RunTimeError);

            results.Exit();
        }

        [Test]
        public void DoesNotSetRunTimeErrorWhenReconnectMessageComesThrough() {
            algorithm = new AlgorithmStub(equities: new List<String> { "SPY" });
            referenceTime = DateTime.UtcNow;
            algorithm.SetDateTime(referenceTime);
            localReferencTime = referenceTime.ConvertFromUtc(TimeZones.NewYork);
            open = localReferencTime.AddSeconds(1).TimeOfDay;
            closed = Duration.ofDays(1);
            marketHours = new MarketHoursSegment(MarketHoursState.Market, open, closed);
            algorithm.Securities[Symbols.SPY].Exchange.SetMarketHours(new [] {marketHours}, localReferencTime.DayOfWeek);
            job = new LiveNodePacket();
            results = new TestResultHandler();//packet -> Console.WriteLine(FieldstoString(packet)));
            api = new Api.Api();
            handler = new DefaultBrokerageMessageHandler(algorithm, job, results, api, Duration.ofMinutes(15), Duration.ofSeconds(.25));

            Assert.IsNull(algorithm.RunTimeError);

            handler.Handle(BrokerageMessageEvent.Disconnected( "Disconnection!"));

            Thread.Sleep(100);

            handler.Handle(BrokerageMessageEvent.Reconnected( "Reconnected!"));

            Thread.Sleep(500);

            Assert.IsNull(algorithm.RunTimeError);

            results.Exit();
        }
    }
}
