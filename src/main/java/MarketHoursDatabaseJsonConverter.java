package com.quantconnect.lean.util;

//using System.Linq;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using NodaTime;
//using QuantConnect.Logging;
//using QuantConnect.Securities;

 * Provides json conversion for the <see cref="MarketHoursDatabase"/> class
public class MarketHoursDatabaseJsonConverter : TypeChangeJsonConverter<MarketHoursDatabase, MarketHoursDatabaseJsonConverter.MarketHoursDatabaseJson>
{
    /**
     * Convert the input value to a value to be serialzied
    */
     * @param value The input value to be converted before serialziation
    @returns A new instance of TResult that is to be serialzied
    protected @Override MarketHoursDatabaseJson Convert(MarketHoursDatabase value) {
        return new MarketHoursDatabaseJson( value );
    }

    /**
     * Converts the input value to be deserialized
    */
     * @param value The deserialized value that needs to be converted to T
    @returns The converted value
    protected @Override MarketHoursDatabase Convert(MarketHoursDatabaseJson value) {
        return value.Convert();
    }

    /**
     * Creates an instance of the un-projected type to be deserialized
    */
     * @param type The input object type, this is the data held in the token
     * @param token The input data to be converted into a T
    @returns A new instance of T that is to be serialized using default rules
    protected @Override MarketHoursDatabase Create(Type type, JToken token) {
        jobject = (JObject) token;
        instance = jobject.ToObject<MarketHoursDatabaseJson>();
        return Convert(instance);
    }

    /**
     * Defines the json structure of the market-hours-database.json file
    */
    [JsonObject(MemberSerialization.OptIn)]
    public class MarketHoursDatabaseJson
    {
        /**
         * The entries in the market hours database, keyed by <see cref="MarketHoursDatabase.Key.toString"/>
        */
        @JsonProperty( "entries")]
        public Map<String, MarketHoursDatabaseEntryJson> Entries;

        /**
         * Initializes a new instance of the <see cref="MarketHoursDatabaseJson"/> class
        */
         * @param database The database instance to copy
        public MarketHoursDatabaseJson(MarketHoursDatabase database) {
            if( database == null ) return;
            Entries = new Map<String, MarketHoursDatabaseEntryJson>();
            foreach (kvp in database.ExchangeHoursListing) {
                key = kvp.Key;
                entry = kvp.Value;
                Entries[key.toString()] = new MarketHoursDatabaseEntryJson(entry);
            }
        }

        /**
         * Converts this json representation to the <see cref="MarketHoursDatabase"/> type
        */
        @returns A new instance of the <see cref="MarketHoursDatabase"/> class
        public MarketHoursDatabase Convert() {
            entries = new Map<SecurityDatabaseKey, MarketHoursDatabase.Entry>();
            foreach (entry in Entries) {
                try
                {
                    key = SecurityDatabaseKey.Parse(entry.Key);
                    entries[key] = entry.Value.Convert();
                }
                catch (Exception err) {
                    Log.Error(err);
                }
            }
            return new MarketHoursDatabase(entries);
        }
    }

    /**
     * Defines the json structure of a single entry in the market-hours-database.json file
    */
    [JsonObject(MemberSerialization.OptIn)]
    public class MarketHoursDatabaseEntryJson
    {
        /**
         * The data's raw time zone
        */
        @JsonProperty( "dataTimeZone")]
        public String DataTimeZone;
        /**
         * The exchange's time zone id from the tzdb
        */
        @JsonProperty( "exchangeTimeZone")]
        public String ExchangeTimeZone;
        /**
         * Sunday market hours segments
        */
        @JsonProperty( "sunday")]
        public List<MarketHoursSegment> Sunday;
        /**
         * Monday market hours segments
        */
        @JsonProperty( "monday")]
        public List<MarketHoursSegment> Monday;
        /**
         * Tuesday market hours segments
        */
        @JsonProperty( "tuesday")]
        public List<MarketHoursSegment> Tuesday;
        /**
         * Wednesday market hours segments
        */
        @JsonProperty( "wednesday")]
        public List<MarketHoursSegment> Wednesday;
        /**
         * Thursday market hours segments
        */
        @JsonProperty( "thursday")]
        public List<MarketHoursSegment> Thursday;
        /**
         * Friday market hours segments
        */
        @JsonProperty( "friday")]
        public List<MarketHoursSegment> Friday;
        /**
         * Saturday market hours segments
        */
        @JsonProperty( "saturday")]
        public List<MarketHoursSegment> Saturday;
        /**
         * Holiday date strings
        */
        @JsonProperty( "holidays")]
        public List<String> Holidays;

        /**
         * Initializes a new instance of the <see cref="MarketHoursDatabaseEntryJson"/> class
        */
         * @param entry The entry instance to copy
        public MarketHoursDatabaseEntryJson(MarketHoursDatabase.Entry entry) {
            if( entry == null ) return;
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
            Holidays = hours.Holidays.Select(x -> x.toString( "M/d/yyyy", CultureInfo.InvariantCulture)).ToList();
        }

        /**
         * Converts this json representation to the <see cref="MarketHoursDatabase.Entry"/> type
        */
        @returns A new instance of the <see cref="MarketHoursDatabase.Entry"/> class
        public MarketHoursDatabase.Entry Convert() {
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
            holidayDates = Holidays.Select(x -> DateTime.ParseExact(x, "M/d/yyyy", CultureInfo.InvariantCulture)).ToHashSet();
            exchangeHours = new SecurityExchangeHours(ZoneIdProviders.Tzdb[ExchangeTimeZone], holidayDates, hours);
            return new MarketHoursDatabase.Entry(ZoneIdProviders.Tzdb[DataTimeZone], exchangeHours);
        }

        private void SetSegmentsForDay(SecurityExchangeHours hours, DayOfWeek day, out List<MarketHoursSegment> segments) {
            LocalMarketHours local;
            if( hours.MarketHours.TryGetValue(day, out local)) {
                segments = local.Segments.ToList();
            }
            else
            {
                segments = new List<MarketHoursSegment>();
            }
        }
    }
}
