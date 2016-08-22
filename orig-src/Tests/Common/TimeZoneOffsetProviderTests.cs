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
using NUnit.Framework;

package com.quantconnect.lean.Tests.Common
{
    [TestFixture]
    public class TimeZoneOffsetProviderTests
    {
        [Test]
        public void ReturnsCurrentOffset() {
            utcDate = new DateTime(2015, 07, 07);
            offsetProvider = new TimeZoneOffsetProvider(TimeZones.NewYork, utcDate, utcDate.AddDays(1));
            currentOffset = offsetProvider.GetOffsetTicks(utcDate);
            Assert.AreEqual(-Duration.ofHours(4).TotalHours, Duration.ofTicks(currentOffset).TotalHours);
        }

        [Test]
        public void ReturnsCorrectOffsetBeforeDST() {
            // one tick before DST goes into affect
            utcDate = new DateTime(2015, 03, 08, 2, 0, 0).AddHours(5).AddTicks(-1);
            offsetProvider = new TimeZoneOffsetProvider(TimeZones.NewYork, utcDate, utcDate.AddDays(1));
            currentOffset = offsetProvider.GetOffsetTicks(utcDate);
            Assert.AreEqual(-Duration.ofHours(5).TotalHours, Duration.ofTicks(currentOffset).TotalHours);
        }

        [Test]
        public void ReturnsCorrectOffsetAfterDST() {
            // the exact instant DST goes into affect
            utcDate = new DateTime(2015, 03, 08, 2, 0, 0).AddHours(5);
            offsetProvider = new TimeZoneOffsetProvider(TimeZones.NewYork, utcDate, utcDate.AddDays(1));
            currentOffset = offsetProvider.GetOffsetTicks(utcDate);
            Assert.AreEqual(-Duration.ofHours(4).TotalHours, Duration.ofTicks(currentOffset).TotalHours);
        }
    }
}
