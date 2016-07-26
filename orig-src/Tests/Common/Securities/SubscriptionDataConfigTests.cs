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

using System.Collections.Generic;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Tests.Common.Securities
{
    [TestFixture]
    public class SubscriptionDataConfigTests
    {
        [Test]
        public void UsesValueEqualsSemantics()
        {
            config1 = new SubscriptionDataConfig(typeof(TradeBar), Symbols.SPY, Resolution.Minute, TimeZones.NewYork, TimeZones.NewYork, false, false, false, false, TickType.Trade, false);
            config2 = new SubscriptionDataConfig(config1);
            Assert.AreEqual(config1, config2);
        }

        [Test]
        public void UsedAsDictionaryKey()
        {
            set = new HashSet<SubscriptionDataConfig>();
            config1 = new SubscriptionDataConfig(typeof(TradeBar), Symbols.SPY, Resolution.Minute, TimeZones.NewYork, TimeZones.NewYork, false, false, false, false, TickType.Trade, false);
            Assert.IsTrue(set.Add(config1));
            config2 = new SubscriptionDataConfig(config1);
            Assert.IsFalse(set.Add(config2));
        }
    }
}