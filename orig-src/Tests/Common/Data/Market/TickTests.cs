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

package com.quantconnect.lean.Tests.Common.Data.Market
{
    [TestFixture]
    public class TickTests
    {
        [Test]
        public void ConstructsFromLine() {
            static final String line = "15093000,1456300,100,P,T,0";

            baseDate = new DateTime(2013, 10, 08);
            tick = new Tick(Symbols.SPY, line, baseDate);

            ms = (tick.Time - baseDate).TotalMilliseconds;
            Assert.AreEqual(15093000, ms);
            Assert.AreEqual(1456300, tick.LastPrice * 10000m);
            Assert.AreEqual(100, tick.Quantity);
            Assert.AreEqual( "P", tick.Exchange);
            Assert.AreEqual( "T", tick.SaleCondition);
            Assert.AreEqual(false, tick.Suspicious);
        }
    }
}
