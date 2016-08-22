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
using NUnit.Framework;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;

package com.quantconnect.lean.Tests.Indicators
{
    [TestFixture]
    public class AverageTrueRangeTests
    {
        [Test]
        public void ComparesAgainstExternalData() {
            atr = new AverageTrueRange(14, MovingAverageType.Simple);
            TestHelper.TestIndicator(atr, "spy_atr.txt", "Average True Range 14");
        }

        [Test]
        public void ResetsProperly() {
            atr = new AverageTrueRange(14, MovingAverageType.Simple);
            atr.Update(new TradeBar
            {
                Time = DateTime.Today,
                Open = 1m,
                High = 3m,
                Low = .5m,
                Close = 2.75m,
                Volume = 1234567890
            });

            atr.Reset();

            TestHelper.AssertIndicatorIsInDefaultState(atr);
            TestHelper.AssertIndicatorIsInDefaultState(atr.TrueRange);
        }

        [Test]
        public void TrueRangePropertyIsReadyAfterOneSample() {
            atr = new AverageTrueRange(14, MovingAverageType.Simple);
            Assert.IsFalse(atr.TrueRange.IsReady);

            atr.Update(new TradeBar
            {
                Time = DateTime.Today,
                Open = 1m,
                High = 3m,
                Low = .5m,
                Close = 2.75m,
                Volume = 1234567890
            });

            Assert.IsTrue(atr.TrueRange.IsReady);
        }
    }
}
