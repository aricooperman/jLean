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

using NUnit.Framework;
using QuantConnect.Indicators;

namespace QuantConnect.Tests.Indicators
{
    [TestFixture]
    class IchimokuKinkoHyoTests
    {
        [Test]
        public void ComparesWithExternalDataTenkanMaximum()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "TenkanMaximum",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).TenkanMaximum.Current.Value)
                );
        }

        [Test]
        public void ComparesWithExternalDataTenkanMinimum()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "TenkanMinimum",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).TenkanMinimum.Current.Value)
                );
        }

        [Test]
        public void ComparesWithExternalDataTenkan()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "Tenkan",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).Tenkan.Current.Value)
                );
        }
        [Test]
        public void ComparesWithExternalDataKijunMaximum()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "KijunMaximum",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).KijunMaximum.Current.Value)
                );
        }
        [Test]
        public void ComparesWithExternalDataKijunMinimum()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "KijunMinimum",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).KijunMinimum.Current.Value)
                );
        }
        [Test]
        public void ComparesWithExternalDataKijun()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            ichimoku.Current.Time.ToString();
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "Kijun",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).Kijun.Current.Value)
                );
        }
        [Test]
        public void ComparesWithExternalDataDelayedTenkanSenkouA()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "DelayedTenkanSenkouA",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).DelayedTenkanSenkouA.Current.Value)
                );
        }


        [Test]
        public void ComparesWithExternalDataDelayedKijunSenkouA()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "DelayedKijunSenkouA",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).DelayedKijunSenkouA.Current.Value)
                );
        }

        [Test]
        public void ComparesWithExternalDataSenkouA()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "Senkou A",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).SenkouA.Current.Value)
                );
        }
        [Test]
        public void ComparesWithExternalDataSenkouBMaximum()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "SenkouBMaximum",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).SenkouBMaximum.Current.Value)
                );
        }

        [Test]
        public void ComparesWithExternalDataSenkouBMinimum()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "SenkouBMinimum",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).SenkouBMinimum.Current.Value)
                );
        }

        [Test]
        public void ComparesWithExternalDataDelayedMaximumSenkouB()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "DelayedMaximumSenkouB",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).DelayedMaximumSenkouB.Current.Value)
                );
        }

        [Test]
        public void ComparesWithExternalDataDelayedMinimumSenkouB()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);
            TestHelper.TestIndicator(
                ichimoku,
                "spy_with_ichimoku.csv",
                "DelayedMinimumSenkouB",
                (ind, expected) => Assert.AreEqual(expected, (double)((IchimokuKinkoHyo)ind).DelayedMinimumSenkouB.Current.Value)
                );
        }

        [Test]
        public void ResetsProperly()
        {
            ichimoku = new IchimokuKinkoHyo("Ichimoku", 9, 26, 26, 52, 26, 26);

            TestHelper.TestIndicatorReset(ichimoku, "spy_with_ichimoku.csv");
        }
    }
}
