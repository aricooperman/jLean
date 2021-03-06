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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuantConnect.Indicators;

package com.quantconnect.lean.Tests.Indicators
{
    [TestFixture]
    public class WindowIdentityTests
    {
        [Test]
        public void WindowIdentityComputesCorrectly() {
            indicator = new WindowIdentity(4);
            data = new[] {1m, 10m, 100m, 1000m, 10000m, 1234m, 56789m};

            seen = new List<decimal>();
            for (int i = 0; i < data.Length; i++) {
                datum = data[i];
                seen.Add(datum);
                indicator.Update(new IndicatorDataPoint(DateTime.Now.AddSeconds(i), datum));
                Assert.AreEqual(seen.LastOrDefault(), indicator.Current.Value);
            }
        }

        [Test]
        public void IsReadyAfterPeriodUpdates() {
            indicator = new WindowIdentity(3);

            indicator.Update(DateTime.UtcNow, 1m);
            indicator.Update(DateTime.UtcNow, 1m);
            Assert.IsFalse(indicator.IsReady);
            indicator.Update(DateTime.UtcNow, 1m);
            Assert.IsTrue(indicator.IsReady);
        }

        [Test]
        public void ResetsProperly() {
            indicator = new WindowIdentity(3);

            foreach (data in TestHelper.GetDataStream(4)) {
                indicator.Update(data);
            }
            Assert.IsTrue(indicator.IsReady);
            
            indicator.Reset();

            TestHelper.AssertIndicatorIsInDefaultState(indicator);
            indicator.Update(DateTime.UtcNow, 2.0m);
            Assert.AreEqual(indicator.Current.Value, 2.0m);
        }

        [Test]
        public void CompareAgainstExternalData() {
            indicator = new WindowIdentity(14);
            TestHelper.TestIndicator(indicator, "Close", 1e-2); // test file only has
        }
    }
}
