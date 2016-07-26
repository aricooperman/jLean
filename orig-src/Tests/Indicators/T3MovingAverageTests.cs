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

using NUnit.Framework;
using QuantConnect.Indicators;

package com.quantconnect.lean.Tests.Indicators
{
    [TestFixture]
    public class T3MovingAverageTests
    {
        [Test]
        public void ComparesAgainstExternalData()
        {
            indicator = new T3MovingAverage(5);

            RunTestIndicator(indicator);
        }

        [Test]
        public void ComparesAgainstExternalDataAfterReset()
        {
            indicator = new T3MovingAverage(5);

            RunTestIndicator(indicator);
            indicator.Reset();
            RunTestIndicator(indicator);
        }

        [Test]
        public void ResetsProperly()
        {
            indicator = new T3MovingAverage(5, 1);

            TestHelper.TestIndicatorReset(indicator, "spy_t3.txt");
        }

        private static void RunTestIndicator(IndicatorBase<IndicatorDataPoint> indicator)
        {
            TestHelper.TestIndicator(indicator, "spy_t3.txt", "T3_5", (ind, expected) => Assert.AreEqual(expected, (double)ind.Current.Value, 2e-2));
        }
    }
}
