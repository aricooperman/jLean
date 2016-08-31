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
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Securities;

package com.quantconnect.lean.Tests.Common.Util
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void IsSubclassOfGenericWorksWorksForNonGenericType() {
            Assert.IsTrue(typeof(Derived2).IsSubclassOfGeneric(typeof(Derived1)));
        }

        [Test]
        public void IsSubclassOfGenericWorksForGenericTypeWithParameter() {
            Assert.IsTrue(typeof(Derived1).IsSubclassOfGeneric(typeof(Super<Integer>)));
            Assert.IsFalse(typeof(Derived1).IsSubclassOfGeneric(typeof(Super<bool>)));
        }

        [Test]
        public void IsSubclassOfGenericWorksForGenericTypeDefinitions() {
            Assert.IsTrue(typeof(Derived1).IsSubclassOfGeneric(typeof(Super<>)));
            Assert.IsTrue(typeof(Derived2).IsSubclassOfGeneric(typeof(Super<>)));
        }

        [Test]
        public void DateTimeRoundDownFullDayDoesntRoundDownByDay() {
            date = new DateTime(2000, 01, 01);
            rounded = date.RoundDown(Duration.ofDays(1));
            Assert.AreEqual(date, rounded);
        }

        [Test]
        public void GetBetterTypeNameHandlesRecursiveGenericTypes() {
            type = typeof (Map<List<Integer>, Map<Integer,String>>);
            static final String expected = "Map<List<Integer32>, Map<Integer32,String>>";
            actual = type.GetBetterTypeName();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExchangeRoundDownSkipsWeekends() {
            time = new DateTime(2015, 05, 02, 18, 01, 00);
            expected = new DateTime(2015, 05, 01);
            hours = MarketHoursDatabase.FromDataFolder().GetExchangeHours(Market.FXCM, null, SecurityType.Forex);
            exchangeRounded = time.ExchangeRoundDown(Duration.ofDays( 1 ), hours, false);
            Assert.AreEqual(expected, exchangeRounded);
        }

        [Test]
        public void ExchangeRoundDownHandlesMarketOpenTime() {
            time = new DateTime(2016, 1, 25, 9, 31, 0);
            expected = time.Date;
            hours = MarketHoursDatabase.FromDataFolder().GetExchangeHours(Market.USA, null, SecurityType.Equity);
            exchangeRounded = time.ExchangeRoundDown(Duration.ofDays( 1 ), hours, false);
        }

        [Test]
        public void ConvertsInt32FromString() {
            static final String input = "12345678";
            value = input Integer.parseInt(  );
            Assert.AreEqual(12345678, value);
        }

        [Test]
        public void ConvertsInt64FromString() {
            static final String input = "12345678900";
            value = input Long.parseLong(  );
            Assert.AreEqual(12345678900, value);
        }

        [Test]
        public void ConvertsDecimalFromString() {
            static final String input = "123.45678";
            value = input new BigDecimal(  );
            Assert.AreEqual(123.45678m, value);
        }

        [Test]
        public void ConvertsZeroDecimalFromString() {
            static final String input = "0.45678";
            value = input new BigDecimal(  );
            Assert.AreEqual(0.45678m, value);
        }

        [Test]
        public void ConvertsOneNumberDecimalFromString() {
            static final String input = "1.45678";
            value = input new BigDecimal(  );
            Assert.AreEqual(1.45678m, value);
        }

        [Test]
        public void ConvertsTimeSpanFromString() {
            static final String input = "16:00";
            timespan = input.ConvertTo<TimeSpan>();
            Assert.AreEqual(Duration.ofHours(16), timespan);
        }

        [Test]
        public void ConvertsDictionaryFromString() {
            expected = new Map<String,Integer> {{"a", 1}, {"b", 2}};
            input = JsonConvert.SerializeObject(expected);
            actual = input.ConvertTo<Map<String,Integer>>();
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void DictionaryAddsItemToExistsList() {
            static final int key = 0;
            list = new List<Integer> {1, 2};
            dictionary = new Map<Integer, List<Integer>> {{key, list}};
            Extensions.Add(dictionary, key, 3);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(3, list[2]);
        }

        [Test]
        public void DictionaryAddCreatesNewList() {
            static final int key = 0;
            dictionary = new Map<Integer, List<Integer>>();
            Extensions.Add(dictionary, key, 1);
            Assert.IsTrue(dictionary.ContainsKey(key));
            list = dictionary[key];
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list[0]);
        }

        [Test]
        public void SafeDecimalCasts() {
            input = 2d;
            output = input.SafeDecimalCast();
            Assert.AreEqual(2m, output);
        }

        [Test]
        public void SafeDecimalCastRespectsUpperBound() {
            input = (double) decimal.MaxValue;
            output = input.SafeDecimalCast();
            Assert.AreEqual(decimal.MaxValue, output);
        }

        [Test]
        public void SafeDecimalCastRespectsLowerBound() {
            input = (double) decimal.MinValue;
            output = input.SafeDecimalCast();
            Assert.AreEqual(decimal.MinValue, output);
        }

        private class Super<T>
        {
        }

        private class Derived1 : Super<Integer>
        {
        }

        private class Derived2 : Derived1
        {
        }
    }
}
