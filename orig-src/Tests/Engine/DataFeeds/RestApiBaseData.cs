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
 *
*/

using System;
using Newtonsoft.Json;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Tests.Engine.DataFeeds
{
    /**
     * Custom data type that causes rest api calls
    */
    public class RestApiBaseData : TradeBar
    {
        public static int ReaderCount = 0;
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
            ReaderCount++;
            //[{"symbol":"SPY","time":1444271505,"alpha":1,"beta":2}]
            array = JsonConvert.DeserializeObject<JsonSerialization[]>(line);
            if( array.Length > 0) {
                return array[0].ToBaseData(config.DataTimeZone, config.Increment, config.Symbol);
            }
            return null;
        }

        public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            remoteFileSource = @"https://www.quantconnect.com/live-test?type=rest&symbols=" + config.Symbol.Value;
            //remoteFileSource = @"http://beta.quantconnect.com/live-test?type=rest&symbols=" + config.Symbol.Value;
            return new SubscriptionDataSource(remoteFileSource, SubscriptionTransportMedium.Rest, FileFormat.Csv);
        }

        private class JsonSerialization
        {
            public String symbol;
            public double time;
            public double alpha;
            public double beta;

            public RestApiBaseData ToBaseData(ZoneId timeZone, Duration period, Symbol sym) {
                dateTime = QuantConnect.Time.UnixTimeStampToDateTime(time) Extensions.convertFromUtc(timeZone).Subtract(period);
                return new RestApiBaseData
                {
                    Symbol = sym,
                    Time = dateTime,
                    EndTime = dateTime.Add(period),
                    Value = (decimal) alpha
                };
            }
        }
    }
}