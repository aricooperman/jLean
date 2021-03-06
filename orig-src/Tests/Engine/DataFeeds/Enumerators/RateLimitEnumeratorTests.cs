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
using System.Linq;
using NUnit.Framework;
using QuantConnect.Data.Market;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.DataFeeds.Enumerators;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Engine.DataFeeds.Enumerators
{
    [TestFixture]
    public class RateLimitEnumeratorTests
    {
        [Test]
        public void LimitsBasedOnTimeBetweenCalls() {
            currentTime = new DateTime(2015, 10, 10, 13, 6, 0);
            timeProvider = new ManualTimeProvider(currentTime, Global.UTC_ZONE_ID);
            data = Enumerable.Range(0, 100).Select(x -> new Tick {Symbol = CreateSymbol(x)}).GetEnumerator();
            rateLimit = new RateLimitEnumerator(data, timeProvider, Time.OneSecond);

            Assert.IsTrue(rateLimit.MoveNext());

            while (rateLimit.MoveNext() && rateLimit.Current == null ) {
                timeProvider.AdvanceSeconds(0.1);
            }

            delta = (timeProvider.GetUtcNow() - currentTime).TotalSeconds;

            Assert.AreEqual(1, delta);

            Assert.AreEqual( "1", data.Current.Symbol.Value);
        }

        private static Symbol CreateSymbol(int x) {
            return new Symbol(SecurityIdentifier.GenerateBase(x.toString(), Market.USA), x.toString());
        }
    }
}
