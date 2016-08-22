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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuantConnect.Util;

package com.quantconnect.lean.Tests.Common.Util
{
    [TestFixture]
    public class MemoizingEnumerableTests
    {
        [Test]
        public void EnumeratesList() {
            list = new List<Integer> {1, 2, 3, 4, 5};
            memoized = new MemoizingEnumerable<Integer>(list);
            CollectionAssert.AreEqual(list, memoized);
        }

        [Test]
        public void EnumeratesOnce() {
            int i = 0;
            enumerable = Enumerable.Range(0, 10).Select(x => i++);
            memoized = new MemoizingEnumerable<Integer>(enumerable);
            // enumerating memoized twice shouldn't matter
            CollectionAssert.AreEqual(memoized.ToList(), memoized.ToList());
        }
    }
}
