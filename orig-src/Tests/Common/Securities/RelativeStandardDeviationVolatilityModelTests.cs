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
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Common.Securities
{
    [TestFixture]
    public class RelativeStandardDeviationVolatilityModelTests
    {
        [Test]
        public void UpdatesAfterCorrectPeriodElapses() {
            static final int periods = 3;
            periodSpan = Time.OneMinute;
            reference = new DateTime(2016, 04, 06, 12, 0, 0);
            referenceUtc = reference.ConvertToUtc(TimeZones.NewYork);
            timeKeeper = new TimeKeeper(referenceUtc);
            config = new SubscriptionDataConfig(typeof (TradeBar), Symbols.SPY, Resolution.Minute, TimeZones.NewYork, TimeZones.NewYork, true, false, false);
            security = new Security(SecurityExchangeHours.AlwaysOpen(TimeZones.NewYork), config, new Cash( "USD", 0, 0), SymbolProperties.GetDefault( "USD"));
            security.SetLocalTimeKeeper(timeKeeper.GetLocalTimeKeeper(TimeZones.NewYork));

            model = new RelativeStandardDeviationVolatilityModel(periodSpan, periods);
            security.VolatilityModel = model;

            first = new IndicatorDataPoint(reference, 1);
            security.SetMarketPrice(first);

            Assert.AreEqual(0m, model.Volatility);

            static final BigDecimal value = 0.471404520791032M; // std of 1,2 is ~0.707 over a mean of 1.5
            second = new IndicatorDataPoint(reference.AddMinutes(1), 2);
            security.SetMarketPrice(second);
            Assert.AreEqual(value, model.Volatility);

            // update should not be applied since not enough time has passed
            third = new IndicatorDataPoint(reference.AddMinutes(1.01), 1000);
            security.SetMarketPrice(third);
            Assert.AreEqual(value, model.Volatility);

            fourth = new IndicatorDataPoint(reference.AddMinutes(2), 3m);
            security.SetMarketPrice(fourth);
            Assert.AreEqual(0.5m, model.Volatility);
        }
    }
}
