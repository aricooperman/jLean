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
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Data.UniverseSelection;

package com.quantconnect.lean.Data.Custom
{
    /**
    /// Helper data type for FXCM's public macro economic sentiment API.
    /// Data source used to create: https://www.dailyfx.com/calendar
    */
    /// 
    /// Data sourced by Thomson Reuters
    /// DailyFX provides traders with an easy to use and customizable real-time calendar that updates automatically during 
    /// announcements.Keep track of significant events that traders care about.As soon as event data is released, the DailyFX 
    /// calendar automatically updates to provide traders with instantaneous information that they can use to formulate their trading decisions.
    /// 
    public class DailyFx : BaseData
    {
        JsonSerializerSettings _jsonSerializerSettings;

        /**
        /// Title of the event.
        */
        @JsonProperty( "title")]
        public String Title;

        /**
        /// Date the event was displayed on DailyFX
        */
        @JsonProperty( "displayDate")]
        public DateTimeOffset DisplayDate; 

        /**
        /// Time of the day the event was displayed.
        */
        /// 
        ///  This is dated 1970, ignore the date component.
        /// 
        @JsonProperty( "displayTime")]
        public DateTimeOffset DisplayTime;

        /**
        /// Importance assignment from FxDaily API.
        */
        @JsonProperty( "importance")]
        public FxDailyImportance Importance;

        /**
        /// What is the perceived meaning of this announcement result?
        */
        @JsonProperty( "better")]
        [JsonConverter(typeof(DailyFxMeaningEnumConverter))]
        public FxDailyMeaning Meaning;

        /**
        /// Currency for this event.
        */
        @JsonProperty( "currency")]
        public String Currency;

        /**
        /// Realized value of the economic tracker
        */
        @JsonProperty( "actual")]
        public String Actual;

        /**
        /// Forecast value of the economic tracker
        */
        @JsonProperty( "forecast")]
        public String Forecast;

        /**
        /// Previous value of the economic tracker
        */
        @JsonProperty( "previous")]
        public String Previous;

        /**
        /// Is this a daily event?
        */
        @JsonProperty( "daily")]
        public boolean DailyEvent;

        /**
        /// Description and commentary on the event.
        */
        @JsonProperty( "commentary")]
        public String Commentary;

        /**
        /// Language for this event.
        */
        @JsonProperty( "language")]
        public String Language;

        /**
        /// Create a new basic FxDaily object.
        */
        public DailyFx() {
            _jsonSerializerSettings = new JsonSerializerSettings() {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                ZoneIdHandling = ZoneIdHandling.RoundtripKind
            };
        }

        /**
        /// Get the source URL for this date. 
        */
        /// 
        ///     FXCM API allows up to 3mo blocks at a time, so we'll return the same URL for each
        ///     quarter and store the results in a local cache for speed.
        /// 
         * @param config">Susbcription configuration
         * @param date">Date we're seeking.
         * @param isLiveMode">Live mode flag
        @returns Subscription source.
        public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            // Live mode just always get today's results, backtesting get all the results for the quarter.
            url = "https://content.dailyfx.com/getData?contenttype=calendarEvent&description=true&format=json_pretty";

            // If we're backtesting append the quarters.
            if( !isLiveMode) {
                url += GetQuarter(date);
            }
            
            return new SubscriptionDataSource(url, SubscriptionTransportMedium.Rest, FileFormat.Collection);
        }

        /**
        /// Create a new Daily FX Object
        */
         * @param config">Subscription data config which created this factory
         * @param content">Line from a <seealso cref="SubscriptionDataSource"/> result
         * @param date">Date of the request
         * @param isLiveMode">Live mode
        @returns 
        public @Override BaseData Reader(SubscriptionDataConfig config, String content, DateTime date, boolean isLiveMode) {
            dailyfxList = JsonConvert.DeserializeObject<List<DailyFx>>(content, _jsonSerializerSettings);

            foreach (dailyfx in dailyfxList) {
                // Custom data format without settings in market hours are assumed UTC.
                dailyfx.Time = dailyfx.DisplayDate.Date.AddHours(dailyfx.DisplayTime.TimeOfDay.TotalHours);

                // Assign a value to this event: 
                // Fairly meaningless between unrelated events, but meaningful with the same event over time.
                dailyfx.Value = 0;
                try
                {
                    if( !StringUtils.isEmpty(Actual)) {
                        dailyfx.Value = new BigDecimal( RemoveSpecialCharacters(Actual));
                    }
                }
                catch
                {
                }
            }

            return new BaseDataCollection(date, config.Symbol, dailyfxList);
        }

        /**
        /// Actual values from the API have lots of units, strip these to generate a "value" for the basedata.
        */
        private static String RemoveSpecialCharacters( String str) {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        /**
        /// Get the date search String for the quarter.
        */
         * @param date">
        @returns 
        private String GetQuarter(DateTime date) {
            start = date.toString( "yyyy", CultureInfo.InvariantCulture);
            end = start;

            if( date.Month < 4) {
                start += "0101";
                end += "03312359"; 
            }
            else if( date.Month < 7) {
                start += "0401";
                end += "06302359";
            }
            else if( date.Month < 10) {
                start += "0701";
                end += "09302359";
            }
            else
            {
                start += "1001";
                end += "12312359";
            }
            return String.format( "&startdate=%1$s&enddate=%2$s", start, end);
        }

        /**
        /// Pretty format output String for the DailyFx.
        */
        @returns 
        public @Override String toString() {
            return String.format( "DailyFx [%1$s %2$s %3$s {3} {4}]", Time.toString( "u"), Title, Currency, Importance, Meaning);
        }
    }

    /**
    /// FXDaily Importance Assignment.
    */
    public enum FxDailyImportance
    {
        /**
        /// Low importance
        */
        @JsonProperty( "low")]
        Low,

        /**
        /// Medium importance
        */
        @JsonProperty( "medium")]
        Medium,

        /**
        /// High importance
        */
        @JsonProperty( "high")]
        High
    }

    /**
    /// What is the meaning of the event?
    */
    public enum FxDailyMeaning
    {
        /**
        /// The impact is perceived to be neutral.
        */
        @JsonProperty( "NONE")]
        None,

        /**
        /// The economic impact is perceived to be better.
        */
        @JsonProperty( "TRUE")]
        Better,

        /**
        /// The economic impact is perceived to be worse.
        */
        @JsonProperty( "FALSE")]
        Worse
    }

    /**
    /// Helper to parse the Daily Fx API.
    */
    public class DailyFxMeaningEnumConverter : JsonConverter
    {
        /**
        /// Parse DailyFx API enum
        */
        public @Override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            enumString = ( String)reader.Value;
            FxDailyMeaning? meaning = null;

            switch (enumString) {
                case "TRUE":
                    meaning = FxDailyMeaning.Better;
                    break;
                case "FALSE":
                    meaning = FxDailyMeaning.Worse;
                    break;
                default:
                case "NONE":
                    meaning = FxDailyMeaning.None;
                    break;
            }
            return meaning;
        }

        /**
        /// Write DailyFxEnum objects to JSON
        */
        public @Override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException( "DailyFx Enum Converter is ReadOnly");
        }

        /**
        /// Indicate if we can convert this object.
        */
        public @Override boolean CanConvert(Type objectType) {
            return objectType == typeof( String);
        }
    }
}
