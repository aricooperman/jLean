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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using QuantConnect.Indicators;

package com.quantconnect.lean.Tests.Indicators
{
    /**
    ///     Test class for QuantConnect.Indicators.Indicator
    */
    [TestFixture]
    public class IndicatorTests
    {
        [Test]
        public void NameSaves() {
            // just testing that we get the right name out
            static final String name = "name";
            target = new TestIndicator(name);
            Assert.AreEqual(name, target.Name);
        }

        [Test]
        public void UpdatesProperly() {
            // we want to make sure the initialized value is the default value
            // for a datapoint, and also verify the our indicator updates as we
            // expect it to, in this case, it should return identity
            target = new TestIndicator();

            Assert.AreEqual(DateTime.MinValue, target.Current.Time);
            Assert.AreEqual(0m, target.Current.Value);

            time = DateTime.UtcNow;
            data = new IndicatorDataPoint(time, 1m);

            target.Update(data);
            Assert.AreEqual(1m, target.Current.Value);

            target.Update(new IndicatorDataPoint(time.AddMilliseconds(1), 2m));
            Assert.AreEqual(2m, target.Current.Value);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), MatchType = MessageMatch.Contains, ExpectedMessage = "forward only")]
        public void ThrowsOnPastTimes() {
            target = new TestIndicator();

            time = DateTime.UtcNow;

            target.Update(new IndicatorDataPoint(time, 1m));
            target.Update(new IndicatorDataPoint(time.AddMilliseconds(-1), 2m));
        }

        [Test]
        public void PassesOnDuplicateTimes() {
            target = new TestIndicator();

            time = DateTime.UtcNow;

            static final BigDecimal value1 = 1m;
            data = new IndicatorDataPoint(time, value1);
            target.Update(data);
            Assert.AreEqual(value1, target.Current.Value);

            // this won't update because we told it to ignore duplicate
            // data based on time
            target.Update(data);
            Assert.AreEqual(value1, target.Current.Value);
        }

        [Test]
        public void SortsTheSameAsDecimalDescending() {
            int count = 100;
            targets = Enumerable.Range(0, count).Select(x -> new TestIndicator(x.toString())).ToList();
            for (int i = 0; i < targets.Count; i++) {
                targets[i].Update(DateTime.Today, i);
            }

            expected = Enumerable.Range(0, count).Select(x -> (decimal)x).OrderByDescending(x -> x).ToList();
            actual = targets.OrderByDescending(x -> x).ToList();
            foreach (pair in expected.Zip<decimal, TestIndicator, Tuple<decimal, TestIndicator>>(actual, Tuple.Create)) {
                Assert.AreEqual(pair.Item1, pair.Item2.Current.Value);
            }
        }

        [Test]
        public void SortsTheSameAsDecimalAsecending() {
            int count = 100;
            targets = Enumerable.Range(0, count).Select(x -> new TestIndicator(x.toString())).ToList();
            for (int i = 0; i < targets.Count; i++) {
                targets[i].Update(DateTime.Today, i);
            }

            expected = Enumerable.Range(0, count).Select(x -> (decimal)x).OrderBy(x -> x).ToList();
            actual = targets.OrderBy(x -> x).ToList();
            foreach (pair in expected.Zip<decimal, TestIndicator, Tuple<decimal, TestIndicator>>(actual, Tuple.Create)) {
                Assert.AreEqual(pair.Item1, pair.Item2.Current.Value);
            }
        }

        [Test]
        public void ComparisonFunctions() {   
            TestComparisonOperators<Integer>();
            TestComparisonOperators<long>();
            TestComparisonOperators<float>();
            TestComparisonOperators<double>();
        }

        private static void TestComparisonOperators<TValue>() {
            indicator = new TestIndicator();
            TestOperator(indicator, default(TValue), "GreaterThan", true, false);
            TestOperator(indicator, default(TValue), "GreaterThan", false, false);
            TestOperator(indicator, default(TValue), "GreaterThanOrEqual", true, true);
            TestOperator(indicator, default(TValue), "GreaterThanOrEqual", false, true);
            TestOperator(indicator, default(TValue), "LessThan", true, false);
            TestOperator(indicator, default(TValue), "LessThan", false, false);
            TestOperator(indicator, default(TValue), "LessThanOrEqual", true, true);
            TestOperator(indicator, default(TValue), "LessThanOrEqual", false, true);
            TestOperator(indicator, default(TValue), "Equality", true, true);
            TestOperator(indicator, default(TValue), "Equality", false, true);
            TestOperator(indicator, default(TValue), "Inequality", true, false);
            TestOperator(indicator, default(TValue), "Inequality", false, false);
        }

        private static void TestOperator<TIndicator, TValue>(TIndicator indicator, TValue value, String opName, boolean tvalueIsFirstParm, boolean expected) {
            method = GetOperatorMethodInfo<TValue>(opName, tvalueIsFirstParm ? 0 : 1);
            ctIndicator = Expression.Constant(indicator);
            ctValue = Expression.Constant(value);
            call = tvalueIsFirstParm ? Expression.Call(method, ctValue, ctIndicator) : Expression.Call(method, ctIndicator, ctValue);
            lamda = Expression.Lambda<Func<bool>>(call);
            func = lamda.Compile();
            Assert.AreEqual(expected, func());
        }

        private static MethodInfo GetOperatorMethodInfo<T>( String @operator, int argIndex) {
            methodName = "op_" + @operator;
            method =
                typeof (IndicatorBase<IndicatorDataPoint>).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .SingleOrDefault(x -> x.Name == methodName && x.GetParameters()[argIndex].ParameterType == typeof(T));

            if( method == null ) {
                Assert.Fail( "Failed to find method for " + @operator + " of type " + typeof(T).Name + " at index: " + argIndex);
            }

            return method;
        }

        private class TestIndicator : Indicator
        {
            /**
            ///     Initializes a new instance of the Indicator class using the specified name.
            */
             * @param name">The name of this indicator
            public TestIndicator( String name)
                : base(name) {
            }
            /**
            ///     Initializes a new instance of the Indicator class using the name "test"
            */
            public TestIndicator()
                : base( "test") {
            }

            /**
            ///     Gets a flag indicating when this indicator is ready and fully initialized
            */
            public @Override boolean IsReady
            {
                get { return true; }
            }

            /**
            ///     Computes the next value of this indicator from the given state
            */
             * @param input">The input given to the indicator
            @returns A new value for this indicator
            protected @Override BigDecimal ComputeNextValue(IndicatorDataPoint input) {
                return input;
            }
        }
    }
}