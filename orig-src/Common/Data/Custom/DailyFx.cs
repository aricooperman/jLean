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
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Data.UniverseSelection;

package com.quantconnect.lean.Data.Custom
{
    /// <summary>
    /// Helper data type for FXCM's public macro economic sentiment API.
    /// Data source used to create: https://www.dailyfx.com/calendar
    /// </summary>
    /// <remarks>
    /// Data sourced by Thomson Reuters
    /// DailyFX provides traders with an easy to use and customizable real-time calendar that updates automatically during 
    /// announcements.Keep track of significant events that traders care about.As soon as event data is released, the DailyFX 
    /// calendar automatically updates to provide traders with instantaneous information that they can use to formulate their trading decisions.
    /// </remarks>
    public class DailyFx : BaseData
    {
        JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Title of the event.
        /// </summary>
        @JsonProperty( "title")]
        public String Title;

        /// <summary>
        /// Date the event was displayed on DailyFX
        /// </summary>
        @JsonProperty( "displayDate")]
        public DateTimeOffset DisplayDate; 

        /// <summary>
        /// Time of the day the event was displayed.
        /// </summary>
        /// <remarks>
        ///  This is dated 1970, ignore the date component.
        /// </remarks>
        @JsonProperty( "displayTime")]
        public DateTimeOffset DisplayTime;

        /// <summary>
        /// Importance assignment from FxDaily API.
        /// </summary>
        @JsonProperty( "importance")]
        public FxDailyImportance Importance;

        /// <summary>
        /// What is the perceived meaning of this announcement result?
        /// </summary>
        @JsonProperty( "better")]
        [JsonConverter(typeof(DailyFxMeaningEnumConverter))]
        public FxDailyMeaning Meaning;

        /// <summary>
        /// Currency for this event.
        /// </summary>
        @JsonProperty( "currency")]
        public String Currency;

        /// <summary>
        /// Realized value of the economic tracker
        /// </summary>
        @JsonProperty( "actual")]
        public String Actual;

        /// <summary>
        /// Forecast value of the economic tracker
        /// </summary>
        @JsonProperty( "forecast")]
        public String Forecast;

        /// <summary>
        /// Previous value of the economic tracker
        /// </summary>
        @JsonProperty( "previous")]
        public String Previous;

        /// <summary>
        /// Is this a daily event?
        /// </summary>
        @JsonProperty( "daily")]
        public boolean DailyEvent;

        /// <summary>
        /// Description and commentary on the event.
        /// </summary>
        @JsonProperty( "commentary")]
        public String Commentary;

        /// <summary>
        /// Language for this event.
        /// </summary>
        @JsonProperty( "language")]
        public String Language;

        /// <summary>
        /// Create a new basic FxDaily object.
        /// </summary>
        public DailyFx()
        {
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                ZoneIdHandling = ZoneIdHandling.RoundtripKind
            };
        }

        /// <summary>
        /// Get the source URL for this date. 
        /// </summary>
        /// <remarks>
        ///     FXCM API allows up to 3mo blocks at a time, so we'll return the same URL for each
        ///     quarter and store the results in a local cache for speed.
        /// </remarks>
        /// <param name="config">Susbcription configuration</param>
        /// <param name="date">Date we're seeking.</param>
        /// <param name="isLiveMode">Live mode flag</param>
        /// <returns>Subscription source.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode)
        {
            // Live mode just always get today's results, backtesting get all the results for the quarter.
            url = "https://content.dailyfx.com/getData?contenttype=calendarEvent&description=true&format=json_pretty";

            // If we're backtesting append the quarters.
            if (!isLiveMode)
            {
                url += GetQuarter(date);
            }
            
            return new SubscriptionDataSource(url, SubscriptionTransportMedium.Rest, FileFormat.Collection);
        }

        /// <summary>
        /// Create a new Daily FX Object
        /// </summary>
        /// <param name="config">Subscription data config which created this factory</param>
        /// <param name="content">Line from a <seealso cref="SubscriptionDataSource"/> result</param>
        /// <param name="date">Date of the request</param>
        /// <param name="isLiveMode">Live mode</param>
        /// <returns></returns>
        public override BaseData Reader(SubscriptionDataConfig config, String content, DateTime date, boolean isLiveMode)
        {
            dailyfxList = JsonConvert.DeserializeObject<List<DailyFx>>(content, _jsonSerializerSettings);

            foreach (dailyfx in dailyfxList)
            {
                // Custom data format without settings in market hours are assumed UTC.
                dailyfx.Time = dailyfx.DisplayDate.Date.AddHours(dailyfx.DisplayTime.TimeOfDay.TotalHours);

                // Assign a value to this event: 
                // Fairly meaningless between unrelated events, but meaningful with the same event over time.
                dailyfx.Value = 0;
                try
                {
                    if (!string.IsNullOrEmpty(Actual))
                    {
                        dailyfx.Value = Convert.ToDecimal(RemoveSpecialCharacters(Actual));
                    }
                }
                catch
                {
                }
            }

            return new BaseDataCollection(date, config.Symbol, dailyfxList);
        }

        /// <summary>
        /// Actual values from the API have lots of units, strip these to generate a "value" for the basedata.
        /// </summary>
        private static String RemoveSpecialCharacters( String str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        /// <summary>
        /// Get the date search String for the quarter.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private String GetQuarter(DateTime date)
        {
            start = date.toString("yyyy", CultureInfo.InvariantCulture);
            end = start;

            if (date.Month < 4)
            {
                start += "0101";
                end += "03312359"; 
            }
            else if (date.Month < 7)
            {
                start += "0401";
                end += "06302359";
            }
            else if (date.Month < 10)
            {
                start += "0701";
                end += "09302359";
            }
            else
            {
                start += "1001";
                end += "12312359";
            }
            return String.format("&startdate={0}&enddate={1}", start, end);
        }

        /// <summary>
        /// Pretty format output String for the DailyFx.
        /// </summary>
        /// <returns></returns>
        public override String toString()
        {
            return String.format("DailyFx [{0} {1} {2} {3} {4}]", Time.toString("u"), Title, Currency, Importance, Meaning);
        }
    }

    /// <summary>
    /// FXDaily Importance Assignment.
    /// </summary>
    public enum FxDailyImportance
    {
        /// <summary>
        /// Low importance
        /// </summary>
        @JsonProperty( "low")]
        Low,

        /// <summary>
        /// Medium importance
        /// </summary>
        @JsonProperty( "medium")]
        Medium,

        /// <summary>
        /// High importance
        /// </summary>
        @JsonProperty( "high")]
        High
    }

    /// <summary>
    /// What is the meaning of the event?
    /// </summary>
    public enum FxDailyMeaning
    {
        /// <summary>
        /// The impact is perceived to be neutral.
        /// </summary>
        @JsonProperty( "NONE")]
        None,

        /// <summary>
        /// The economic impact is perceived to be better.
        /// </summary>
        @JsonProperty( "TRUE")]
        Better,

        /// <summary>
        /// The economic impact is perceived to be worse.
        /// </summary>
        @JsonProperty( "FALSE")]
        Worse
    }

    /// <summary>
    /// Helper to parse the Daily Fx API.
    /// </summary>
    public class DailyFxMeaningEnumConverter : JsonConverter
    {
        /// <summary>
        /// Parse DailyFx API enum
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            enumString = ( String)reader.Value;
            FxDailyMeaning? meaning = null;

            switch (enumString)
            {
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

        /// <summary>
        /// Write DailyFxEnum objects to JSON
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("DailyFx Enum Converter is ReadOnly");
        }

        /// <summary>
        /// Indicate if we can convert this object.
        /// </summary>
        public override boolean CanConvert(Type objectType)
        {
            return objectType == typeof( String);
        }
    }
}
