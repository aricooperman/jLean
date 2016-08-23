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
using QuantConnect.Indicators;
using System.Linq;

package com.quantconnect.lean.Tests.Indicators
{
    [TestFixture]
    public class IndicatorExtensionsTests
    {
        [Test]
        public void PipesDataUsingOfFromFirstToSecond() {
            first = new SimpleMovingAverage(2);
            second = new Delay(1);
            
            // this is a configuration step, but returns the reference to the second for method chaining
            second.Of(first);

            data1 = new IndicatorDataPoint(DateTime.UtcNow, 1m);
            data2 = new IndicatorDataPoint(DateTime.UtcNow, 2m);
            data3 = new IndicatorDataPoint(DateTime.UtcNow, 3m);
            data4 = new IndicatorDataPoint(DateTime.UtcNow, 4m);

            // sma has one item
            first.Update(data1);
            Assert.IsFalse(first.IsReady);
            Assert.AreEqual(0m, second.Current.Value);

            // sma is ready, delay will repeat this value
            first.Update(data2);
            Assert.IsTrue(first.IsReady);
            Assert.IsFalse(second.IsReady);
            Assert.AreEqual(1.5m, second.Current.Value);

            // delay is ready, and repeats its first input
            first.Update(data3);
            Assert.IsTrue(second.IsReady);
            Assert.AreEqual(1.5m, second.Current.Value);

            // now getting the delayed data
            first.Update(data4);
            Assert.AreEqual(2.5m, second.Current.Value);
        }

        [Test]
        public void PipesDataFirstWeightedBySecond() {
            static final int period = 4;
            value = new Identity( "Value");
            weight = new Identity( "Weight");
     
            third = value.WeightedBy(weight, period);

            data = Enumerable.Range(1, 10).ToList();
            window = Enumerable.Reverse(data).Take(period);
            current = window.Sum(x -> 2 * x * x) / (decimal)window.Sum(x -> x);

            foreach (item in data) {
                value.Update(new IndicatorDataPoint(DateTime.UtcNow, new BigDecimal( 2 * item)));
                weight.Update(new IndicatorDataPoint(DateTime.UtcNow, new BigDecimal( item)));
            }

            Assert.AreEqual(current, third.Current.Value);
        }

        [Test]
        public void NewDataPushesToDerivedIndicators() {
            identity = new Identity( "identity");
            sma = new SimpleMovingAverage(3);

            identity.Updated += (sender, consolidated) =>
            {
                sma.Update(consolidated);
            };

            identity.Update(DateTime.UtcNow, 1m);
            identity.Update(DateTime.UtcNow, 2m);
            Assert.IsFalse(sma.IsReady);

            identity.Update(DateTime.UtcNow, 3m);
            Assert.IsTrue(sma.IsReady);
            Assert.AreEqual(2m, sma);
        }

        [Test]
        public void MultiChainSMA() {
            identity = new Identity( "identity");
            delay = new Delay(2);

            // create the SMA of the delay of the identity
            sma = delay.Of(identity).SMA(2);

            identity.Update(DateTime.UtcNow, 1m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsFalse(delay.IsReady);
            Assert.IsFalse(sma.IsReady);

            identity.Update(DateTime.UtcNow, 2m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsFalse(delay.IsReady);
            Assert.IsFalse(sma.IsReady);

            identity.Update(DateTime.UtcNow, 3m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsTrue(delay.IsReady);
            Assert.IsFalse(sma.IsReady);

            identity.Update(DateTime.UtcNow, 4m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsTrue(delay.IsReady);
            Assert.IsTrue(sma.IsReady);

            Assert.AreEqual(1.5m, sma);
        }

        [Test]
        public void MultiChainEMA() {
            identity = new Identity( "identity");
            delay = new Delay(2);

            // create the EMA of chained methods
            ema = delay.Of(identity).EMA(2, 1);
            
            // Assert.IsTrue(ema. == 1);
            identity.Update(DateTime.UtcNow, 1m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsFalse(delay.IsReady);
            Assert.IsFalse(ema.IsReady);

            identity.Update(DateTime.UtcNow, 2m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsFalse(delay.IsReady);
            Assert.IsFalse(ema.IsReady);

            identity.Update(DateTime.UtcNow, 3m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsTrue(delay.IsReady);
            Assert.IsFalse(ema.IsReady);

            identity.Update(DateTime.UtcNow, 4m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsTrue(delay.IsReady);
            Assert.IsTrue(ema.IsReady);
        }

        [Test]
        public void MultiChainMAX() {
            identity = new Identity( "identity");
            delay = new Delay(2);

            // create the MAX of the delay of the identity
            max = delay.Of(identity).MAX(2);

            identity.Update(DateTime.UtcNow, 1m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsFalse(delay.IsReady);
            Assert.IsFalse(max.IsReady);

            identity.Update(DateTime.UtcNow, 2m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsFalse(delay.IsReady);
            Assert.IsFalse(max.IsReady);

            identity.Update(DateTime.UtcNow, 3m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsTrue(delay.IsReady);
            Assert.IsFalse(max.IsReady);

            identity.Update(DateTime.UtcNow, 4m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsTrue(delay.IsReady);
            Assert.IsTrue(max.IsReady);
        }

        [Test]
        public void MultiChainMIN() {
            identity = new Identity( "identity");
            delay = new Delay(2);

            // create the MIN of the delay of the identity
            min = delay.Of(identity).MIN(2);

            identity.Update(DateTime.UtcNow, 1m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsFalse(delay.IsReady);
            Assert.IsFalse(min.IsReady);

            identity.Update(DateTime.UtcNow, 2m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsFalse(delay.IsReady);
            Assert.IsFalse(min.IsReady);

            identity.Update(DateTime.UtcNow, 3m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsTrue(delay.IsReady);
            Assert.IsFalse(min.IsReady);

            identity.Update(DateTime.UtcNow, 4m);
            Assert.IsTrue(identity.IsReady);
            Assert.IsTrue(delay.IsReady);
            Assert.IsTrue(min.IsReady);
        }

        [Test]
        public void PlusAddsLeftAndRightAfterBothUpdated() {
            left = new Identity( "left");
            right = new Identity( "right");
            composite = left.Plus(right);

            left.Update(DateTime.Today, 1m);
            right.Update(DateTime.Today, 1m);
            Assert.AreEqual(2m, composite.Current.Value);

            left.Update(DateTime.Today, 2m);
            Assert.AreEqual(2m, composite.Current.Value);

            left.Update(DateTime.Today, 3m);
            Assert.AreEqual(2m, composite.Current.Value);

            right.Update(DateTime.Today, 4m);
            Assert.AreEqual(7m, composite.Current.Value);
        }

        [Test]
        public void MinusSubtractsLeftAndRightAfterBothUpdated() {
            left = new Identity( "left");
            right = new Identity( "right");
            composite = left.Minus(right);

            left.Update(DateTime.Today, 1m);
            right.Update(DateTime.Today, 1m);
            Assert.AreEqual(0m, composite.Current.Value);

            left.Update(DateTime.Today, 2m);
            Assert.AreEqual(0m, composite.Current.Value);

            left.Update(DateTime.Today, 3m);
            Assert.AreEqual(0m, composite.Current.Value);

            right.Update(DateTime.Today, 4m);
            Assert.AreEqual(-1m, composite.Current.Value);
        }

        [Test]
        public void OverDivdesLeftAndRightAfterBothUpdated() {
            left = new Identity( "left");
            right = new Identity( "right");
            composite = left.Over(right);

            left.Update(DateTime.Today, 1m);
            right.Update(DateTime.Today, 1m);
            Assert.AreEqual(1m, composite.Current.Value);

            left.Update(DateTime.Today, 2m);
            Assert.AreEqual(1m, composite.Current.Value);

            left.Update(DateTime.Today, 3m);
            Assert.AreEqual(1m, composite.Current.Value);

            right.Update(DateTime.Today, 4m);
            Assert.AreEqual(3m / 4m, composite.Current.Value);
        }

        [Test]
        public void OverHandlesDivideByZero() {
            left = new Identity( "left");
            right = new Identity( "right");
            composite = left.Over(right);
            updatedEventFired = false;
            composite.Updated += delegate { updatedEventFired = true; };

            left.Update(DateTime.Today, 1m);
            Assert.IsFalse(updatedEventFired);
            right.Update(DateTime.Today, 0m);
            Assert.IsFalse(updatedEventFired);

            // submitting another update to right won't cause an update without corresponding update to left
            right.Update(DateTime.Today, 1m);
            Assert.IsFalse(updatedEventFired);
            left.Update(DateTime.Today, 1m);
            Assert.IsTrue(updatedEventFired);
        }

        [Test]
        public void TimesMultipliesLeftAndRightAfterBothUpdated() {
            left = new Identity( "left");
            right = new Identity( "right");
            composite = left.Times(right);

            left.Update(DateTime.Today, 1m);
            right.Update(DateTime.Today, 1m);
            Assert.AreEqual(1m, composite.Current.Value);

            left.Update(DateTime.Today, 2m);
            Assert.AreEqual(1m, composite.Current.Value);

            left.Update(DateTime.Today, 3m);
            Assert.AreEqual(1m, composite.Current.Value);

            right.Update(DateTime.Today, 4m);
            Assert.AreEqual(12m, composite.Current.Value);
        }
    }
}
