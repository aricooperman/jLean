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
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Util;

package com.quantconnect.lean.Tests.Common.Util
{
    [TestFixture]
    public class SingleValueListConverterTests
    {
        private static final String Data = "some data";

        private static final WellFormedContainer WellFormedInstance = new WellFormedContainer
        {
            ComplexTypes = new List<ComplexType>
            {
                new ComplexType
                {
                    ID = 1,
                    Data = Data
                }
            }
        };

        private static final PoorlyFormedContainer PoorlyFormedInstance = new PoorlyFormedContainer
        {
            ComplexTypes = new ComplexType
            {
                ID = 1,
                Data = Data
            }
        };

        private final static String ListJson = JsonConvert.SerializeObject(WellFormedInstance);
        private final static String ObjectJson = JsonConvert.SerializeObject(PoorlyFormedInstance);

        [Test]
        public void DeserializesList() {
            converted = JsonConvert.DeserializeObject(ListJson, typeof(WellFormedContainer));
            Assert.IsInstanceOf<WellFormedContainer>(converted);
            instance = (WellFormedContainer)converted;
            Assert.AreEqual(1, instance.ComplexTypes.Count);
            Assert.AreEqual(1, instance.ComplexTypes[0].ID);
            Assert.AreEqual(Data, instance.ComplexTypes[0].Data);
        }

        [Test]
        public void DeserializesSingleValue() {
            converted = JsonConvert.DeserializeObject(ObjectJson, typeof(WellFormedContainer));
            Assert.IsInstanceOf<WellFormedContainer>(converted);
            instance = (WellFormedContainer)converted;
            Assert.AreEqual(1, instance.ComplexTypes.Count);
            Assert.AreEqual(1, instance.ComplexTypes[0].ID);
            Assert.AreEqual(Data, instance.ComplexTypes[0].Data);
        }

        [Test]
        public void SerializesListWithOneValue() {
            serialized = JsonConvert.SerializeObject(WellFormedInstance);
            Assert.AreEqual(ListJson, serialized);
        }

        class WellFormedContainer
        {
            [JsonConverter(typeof(SingleValueListConverter<ComplexType>))]
            public List<ComplexType> ComplexTypes;
        }

        class PoorlyFormedContainer
        {
            public ComplexType ComplexTypes;
        }

        class ComplexType
        {
            public int ID;
            public String Data;
        }
    }
}
