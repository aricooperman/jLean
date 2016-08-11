﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using QuantConnect.Logging;
using QuantConnect.Securities;

package com.quantconnect.lean.Util
{
    /// <summary>
    /// Provides json conversion for the <see cref="MarketHoursDatabase"/> class
    /// </summary>
    public class MarketHoursDatabaseJsonConverter : TypeChangeJsonConverter<MarketHoursDatabase, MarketHoursDatabaseJsonConverter.MarketHoursDatabaseJson>
    {
        /// <summary>
        /// Convert the input value to a value to be serialzied
        /// </summary>
        /// <param name="value">The input value to be converted before serialziation</param>
        /// <returns>A new instance of TResult that is to be serialzied</returns>
        protected override MarketHoursDatabaseJson Convert(MarketHoursDatabase value)
        {
            return new MarketHoursDatabaseJson(value);
        }

        /// <summary>
        /// Converts the input value to be deserialized
        /// </summary>
        /// <param name="value">The deserialized value that needs to be converted to T</param>
        /// <returns>The converted value</returns>
        protected override MarketHoursDatabase Convert(MarketHoursDatabaseJson value)
        {
            return value.Convert();
        }

        /// <summary>
        /// Creates an instance of the un-projected type to be deserialized
        /// </summary>
        /// <param name="type">The input object type, this is the data held in the token</param>
        /// <param name="token">The input data to be converted into a T</param>
        /// <returns>A new instance of T that is to be serialized using default rules</returns>
        protected override MarketHoursDatabase Create(Type type, JToken token)
        {
            jobject = (JObject) token;
            instance = jobject.ToObject<MarketHoursDatabaseJson>();
            return Convert(instance);
        }

        /// <summary>
        /// Defines the json structure of the market-hours-database.json file
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class MarketHoursDatabaseJson
        {
            /// <summary>
            /// The entries in the market hours database, keyed by <see cref="MarketHoursDatabase.Key.toString"/>
            /// </summary>
            [JsonProperty("entries")]
            public Map<String, MarketHoursDatabaseEntryJson> Entries;

            /// <summary>
            /// Initializes a new instance of the <see cref="MarketHoursDatabaseJson"/> class
            /// </summary>
            /// <param name="database">The database instance to copy</param>
            public MarketHoursDatabaseJson(MarketHoursDatabase database)
            {
                if (database == null) return;
                Entries = new Map<String, MarketHoursDatabaseEntryJson>();
                foreach (kvp in database.ExchangeHoursListing)
                {
                    key = kvp.Key;
                    entry = kvp.Value;
                    Entries[key.toString()] = new MarketHoursDatabaseEntryJson(entry);
                }
            }

            /// <summary>
            /// Converts this json representation to the <see cref="MarketHoursDatabase"/> type
            /// </summary>
            /// <returns>A new instance of the <see cref="MarketHoursDatabase"/> class</returns>
            public MarketHoursDatabase Convert()
            {
                entries = new Map<SecurityDatabaseKey, MarketHoursDatabase.Entry>();
                foreach (entry in Entries)
                {
                    try
                    {
                        key = SecurityDatabaseKey.Parse(entry.Key);
                        entries[key] = entry.Value.Convert();
                    }
                    catch (Exception err)
                    {
                        Log.Error(err);
                    }
                }
                return new MarketHoursDatabase(entries);
            }
        }

        /// <summary>
        /// Defines the json structure of a single entry in the market-hours-database.json file
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class MarketHoursDatabaseEntryJson
        {
            /// <summary>
            /// The data's raw time zone
            /// </summary>
            [JsonProperty("dataTimeZone")]
            public String DataTimeZone;
            /// <summary>
            /// The exchange's time zone id from the tzdb
            /// </summary>
            [JsonProperty("exchangeTimeZone")]
            public String ExchangeTimeZone;
            /// <summary>
            /// Sunday market hours segments
            /// </summary>
            [JsonProperty("sunday")]
            public List<MarketHoursSegment> Sunday;
            /// <summary>
            /// Monday market hours segments
            /// </summary>
            [JsonProperty("monday")]
            public List<MarketHoursSegment> Monday;
            /// <summary>
            /// Tuesday market hours segments
            /// </summary>
            [JsonProperty("tuesday")]
            public List<MarketHoursSegment> Tuesday;
            /// <summary>
            /// Wednesday market hours segments
            /// </summary>
            [JsonProperty("wednesday")]
            public List<MarketHoursSegment> Wednesday;
            /// <summary>
            /// Thursday market hours segments
            /// </summary>
            [JsonProperty("thursday")]
            public List<MarketHoursSegment> Thursday;
            /// <summary>
            /// Friday market hours segments
            /// </summary>
            [JsonProperty("friday")]
            public List<MarketHoursSegment> Friday;
            /// <summary>
            /// Saturday market hours segments
            /// </summary>
            [JsonProperty("saturday")]
            public List<MarketHoursSegment> Saturday;
            /// <summary>
            /// Holiday date strings
            /// </summary>
            [JsonProperty("holidays")]
            public List<String> Holidays;

            /// <summary>
            /// Initializes a new instance of the <see cref="MarketHoursDatabaseEntryJson"/> class
            /// </summary>
            /// <param name="entry">The entry instance to copy</param>
            public MarketHoursDatabaseEntryJson(MarketHoursDatabase.Entry entry)
            {
                if (entry == null) return;
                DataTimeZone = entry.DataTimeZone.Id;
                hours = entry.ExchangeHours;
                ExchangeTimeZone = hours.TimeZone.Id;
                SetSegmentsForDay(hours, DayOfWeek.Sunday, out Sunday);
                SetSegmentsForDay(hours, DayOfWeek.Monday, out Monday);
                SetSegmentsForDay(hours, DayOfWeek.Tuesday, out Tuesday);
                SetSegmentsForDay(hours, DayOfWeek.Wednesday, out Wednesday);
                SetSegmentsForDay(hours, DayOfWeek.Thursday, out Thursday);
                SetSegmentsForDay(hours, DayOfWeek.Friday, out Friday);
                SetSegmentsForDay(hours, DayOfWeek.Saturday, out Saturday);
                Holidays = hours.Holidays.Select(x => x.toString("M/d/yyyy", CultureInfo.InvariantCulture)).ToList();
            }

            /// <summary>
            /// Converts this json representation to the <see cref="MarketHoursDatabase.Entry"/> type
            /// </summary>
            /// <returns>A new instance of the <see cref="MarketHoursDatabase.Entry"/> class</returns>
            public MarketHoursDatabase.Entry Convert()
            {
                hours = new Map<DayOfWeek, LocalMarketHours>
                {
                    { DayOfWeek.Sunday, new LocalMarketHours(DayOfWeek.Sunday, Sunday) },
                    { DayOfWeek.Monday, new LocalMarketHours(DayOfWeek.Monday, Monday) },
                    { DayOfWeek.Tuesday, new LocalMarketHours(DayOfWeek.Tuesday, Tuesday) },
                    { DayOfWeek.Wednesday, new LocalMarketHours(DayOfWeek.Wednesday, Wednesday) },
                    { DayOfWeek.Thursday, new LocalMarketHours(DayOfWeek.Thursday, Thursday) },
                    { DayOfWeek.Friday, new LocalMarketHours(DayOfWeek.Friday, Friday) },
                    { DayOfWeek.Saturday, new LocalMarketHours(DayOfWeek.Saturday, Saturday) }
                };
                holidayDates = Holidays.Select(x => DateTime.ParseExact(x, "M/d/yyyy", CultureInfo.InvariantCulture)).ToHashSet();
                exchangeHours = new SecurityExchangeHours(ZoneIdProviders.Tzdb[ExchangeTimeZone], holidayDates, hours);
                return new MarketHoursDatabase.Entry(ZoneIdProviders.Tzdb[DataTimeZone], exchangeHours);
            }

            private void SetSegmentsForDay(SecurityExchangeHours hours, DayOfWeek day, out List<MarketHoursSegment> segments)
            {
                LocalMarketHours local;
                if (hours.MarketHours.TryGetValue(day, out local))
                {
                    segments = local.Segments.ToList();
                }
                else
                {
                    segments = new List<MarketHoursSegment>();
                }
            }
        }
    }
}
