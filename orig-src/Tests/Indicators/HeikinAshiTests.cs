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
using QuantConnect.Data.Market;
using QuantConnect.Indicators;

package com.quantconnect.lean.Tests.Indicators
{
    [TestFixture]
    public class HeikinAshiTests : CommonIndicatorTests<TradeBar>
    {
        protected @Override IndicatorBase<TradeBar> CreateIndicator() {
            return new HeikinAshi();
        }

        protected @Override String TestFileName
        {
            get { return "spy_heikin_ashi.txt"; }
        }

        protected @Override String TestColumnName
        {
            get { return ""; }
        }

        [Test]
        public @Override void ComparesAgainstExternalData() {
            TestHelper.TestIndicator(new HeikinAshi(), TestFileName, "HA_Open", (ind, expected) => Assert.AreEqual(expected, (double)((HeikinAshi)ind).Open.Current.Value, 1e-3));
            TestHelper.TestIndicator(new HeikinAshi(), TestFileName, "HA_High", (ind, expected) => Assert.AreEqual(expected, (double)((HeikinAshi)ind).High.Current.Value, 1e-3));
            TestHelper.TestIndicator(new HeikinAshi(), TestFileName, "HA_Low", (ind, expected) => Assert.AreEqual(expected, (double)((HeikinAshi)ind).Low.Current.Value, 1e-3));
            TestHelper.TestIndicator(new HeikinAshi(), TestFileName, "HA_Close", (ind, expected) => Assert.AreEqual(expected, (double)((HeikinAshi)ind).Close.Current.Value, 1e-3));
        }

        [Test]
        public @Override void ComparesAgainstExternalDataAfterReset() {
            indicator = new HeikinAshi();
            for (i = 1; i <= 2; i++) {
                TestHelper.TestIndicator(indicator, TestFileName, "HA_Open", (ind, expected) => Assert.AreEqual(expected, (double)((HeikinAshi)ind).Open.Current.Value, 1e-3));
                indicator.Reset();
                TestHelper.TestIndicator(indicator, TestFileName, "HA_High", (ind, expected) => Assert.AreEqual(expected, (double)((HeikinAshi)ind).High.Current.Value, 1e-3));
                indicator.Reset();
                TestHelper.TestIndicator(indicator, TestFileName, "HA_Low", (ind, expected) => Assert.AreEqual(expected, (double)((HeikinAshi)ind).Low.Current.Value, 1e-3));
                indicator.Reset();
                TestHelper.TestIndicator(indicator, TestFileName, "HA_Close", (ind, expected) => Assert.AreEqual(expected, (double)((HeikinAshi)ind).Close.Current.Value, 1e-3));
                indicator.Reset();
            }
        }
    }
}
