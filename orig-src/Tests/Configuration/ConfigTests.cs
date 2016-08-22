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
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using QuantConnect.Configuration;

package com.quantconnect.lean.Tests.Configuration
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void SetRespectsEnvironment() {
            boolean betaMode = Config.GetBool( "beta-mode");
            env = Config.Get( "environment");
            Config.Set(env + ".beta-mode", betaMode ? "false" : "true");


            boolean betaMode2 = Config.GetBool( "beta-mode");
            Assert.AreNotEqual(betaMode, betaMode2);
        }

        [Test]
        public void FlattenTest() {
            // read in and rewrite the environment based on the settings
            static final String @OverrideEnvironment = "live-paper.beta";

            config = JObject.Parse(
@"{
   'some-setting': 'false',                 
    environments: {
        'live-paper': {
            'some-setting': 'true',
            'environments': {
                'beta': {
                    'some-setting2': 'true'
                }
            }
        }
    }
}");

            configCopy = config.DeepClone();

            clone = Config.Flatten(config, @OverrideEnvironment);

            // remove environments
            Assert.IsNull(clone.Property( "environment"));
            Assert.IsNull(clone.Property( "environments"));

            // properly applied environment
            Assert.AreEqual( "true", clone.Property( "some-setting").Value.toString());
            Assert.AreEqual( "true", clone.Property( "some-setting2").Value.toString());

            Assert.AreEqual(configCopy, config);
        }

        [Test]
        public void GetValueHandlesDateTime() {
            GetValueHandles(new DateTime(2015, 1, 2, 3, 4, 5));
        }

        [Test]
        public void GetValueHandlesTimeSpan() {
            GetValueHandles(new TimeSpan(1, 2, 3, 4, 5));
        }

        private void GetValueHandles<T>(T value) {
            configValue = value.toString();
            Config.Set( "temp-value", configValue);
            actual = Config.GetValue<T>( "temp-value");
            Assert.AreEqual(value, actual);
        }
    }
}
