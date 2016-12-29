package com.quantconnect.lean.util;

import java.io.IOException;
import java.time.DayOfWeek;
import java.time.LocalDate;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.Collection;
import java.util.EnumMap;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.stream.Collectors;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.fasterxml.jackson.annotation.JsonProperty;
import com.quantconnect.lean.Global;
import com.quantconnect.lean.securities.LocalMarketHours;
import com.quantconnect.lean.securities.MarketHoursDatabase;
import com.quantconnect.lean.securities.MarketHoursDatabase.DatabaseEntry;
import com.quantconnect.lean.securities.MarketHoursSegment;
import com.quantconnect.lean.securities.SecurityDatabaseKey;
import com.quantconnect.lean.securities.SecurityExchangeHours;

//using System.Linq;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using NodaTime;
//using QuantConnect.Logging;
//using QuantConnect.Securities;

/**
 * Provides json conversion for the <see cref="MarketHoursDatabase"/> class
 */
public class MarketHoursDatabaseJsonConverter extends TypeChangeJsonConverter<MarketHoursDatabase,MarketHoursDatabaseJsonConverter.MarketHoursDatabaseJson> {

    public final static Logger LOG = LoggerFactory.getLogger( MarketHoursDatabaseJsonConverter.class );

    /**
     * Convert the input value to a value to be serialzied
     * @param value The input value to be converted before serialziation
     * @returns A new instance of TResult that is to be serialzied
     */
    @Override
    protected MarketHoursDatabaseJson convertToResult( final MarketHoursDatabase value ) {
        return new MarketHoursDatabaseJson( value );
    }
    
    /**
     * Converts the input value to be deserialized
     * @param value The deserialized value that needs to be converted to T
     * @returns The converted value
     */
    @Override
    protected MarketHoursDatabase convertFromResult( final MarketHoursDatabaseJson value ) {
        return value.convert();
    }

    /**
     * Creates an instance of the un-projected type to be deserialized
     * @param type The input object type, this is the data held in the token
     * @param token The input data to be converted into a T
     * @throws IOException
     * @returns A new instance of T that is to be serialized using default rules
     */
    @Override
    protected MarketHoursDatabase create( final Class<? extends MarketHoursDatabaseJson> type, final String token ) throws IOException {
        final MarketHoursDatabaseJson instance = Global.OBJECT_MAPPER.readValue( token, MarketHoursDatabaseJson.class );
        return convertFromResult( instance );
    }

    /**
     * Defines the json structure of the market-hours-database.json file
    */
//    TODO [JsonObject(MemberSerialization.OptIn)]
    public static class MarketHoursDatabaseJson {
        /**
         * The entries in the market hours database, keyed by <see cref="MarketHoursDatabase.Key.toString"/>
        */
        @JsonProperty( "entries" )
        public Map<String,MarketHoursDatabaseEntryJson> Entries;

        /**
         * Initializes a new instance of the <see cref="MarketHoursDatabaseJson"/> class
         * @param database The database instance to copy
         */
        public MarketHoursDatabaseJson( final MarketHoursDatabase database ) {
            if( database == null ) return;
            Entries = new HashMap<>();
            for( final Entry<SecurityDatabaseKey,MarketHoursDatabase.DatabaseEntry> kvp : database.getExchangeHoursListing() ) {
                final SecurityDatabaseKey key = kvp.getKey();
                final DatabaseEntry entry = kvp.getValue();
                Entries.put( key.toString(), new MarketHoursDatabaseEntryJson( entry ) );
            }
        }

        /**
         * Converts this json representation to the <see cref="MarketHoursDatabase"/> type
         * @returns A new instance of the <see cref="MarketHoursDatabase"/> class
         */
        public MarketHoursDatabase convert() {
            final Map<SecurityDatabaseKey,DatabaseEntry> newEntries = new HashMap<>();
            for( final Entry<String,MarketHoursDatabaseEntryJson> entry : Entries.entrySet() ) {
                try {
                    final SecurityDatabaseKey key = SecurityDatabaseKey.parse( entry.getKey() );
                    newEntries.put( key, entry.getValue().convert() );
                }
                catch( final Exception err ) {
                    LOG.error( err.getMessage(), err );
                }
            }
            return new MarketHoursDatabase( newEntries );
        }
    }

    /**
     * Defines the json structure of a single entry in the market-hours-database.json file
     */
//  TODO  [JsonObject(MemberSerialization.OptIn)]
    public static class MarketHoursDatabaseEntryJson {

        private static final DateTimeFormatter DATE_FORMATTER = DateTimeFormatter.ofPattern( "M/d/yyyy" );
        
        /**
         * The data's raw time zone
         */
        @JsonProperty( "dataTimeZone" )
        public final String dataTimeZone;
        
        /**
         * The exchange's time zone id from the tzdb
         */
        @JsonProperty( "exchangeTimeZone" )
        public final String exchangeTimeZone;
        
        /**
         * Sunday market hours segments
         */
        @JsonProperty( "sunday" )
        public final List<MarketHoursSegment> sunday;
        
        /**
         * Monday market hours segments
         */
        @JsonProperty( "monday" )
        public final List<MarketHoursSegment> monday;
        
        /**
         * Tuesday market hours segments
        */
        @JsonProperty( "tuesday" )
        public final List<MarketHoursSegment> tuesday;
        
        /**
         * Wednesday market hours segments
        */
        @JsonProperty( "wednesday" )
        public final List<MarketHoursSegment> wednesday;
        
        /**
         * Thursday market hours segments
        */
        @JsonProperty( "thursday" )
        public final List<MarketHoursSegment> thursday;
        
        /**
         * Friday market hours segments
        */
        @JsonProperty( "friday" )
        public final List<MarketHoursSegment> friday;
        
        /**
         * Saturday market hours segments
        */
        @JsonProperty( "saturday" )
        public final List<MarketHoursSegment> saturday;
        
        /**
         * Holiday date strings
        */
        @JsonProperty( "holidays" )
        public final List<String> holidays;
        

        /**
         * Initializes a new instance of the <see cref="MarketHoursDatabaseEntryJson"/> class
         * @param entry The entry instance to copy
         */
        public MarketHoursDatabaseEntryJson( final MarketHoursDatabase.DatabaseEntry entry) {
            if( entry == null ) {
                dataTimeZone = exchangeTimeZone = null;
                sunday = monday = tuesday = wednesday = thursday = friday = saturday = null;
                holidays = null;
                return;
            }
            
            dataTimeZone = entry.dataTimeZone.getId();
            final SecurityExchangeHours hours = entry.exchangeHours;
            exchangeTimeZone = hours.getTimeZone().getId();
            
            sunday = getSegmentsForDay( hours, DayOfWeek.SUNDAY );
            monday = getSegmentsForDay( hours, DayOfWeek.MONDAY );
            tuesday = getSegmentsForDay( hours, DayOfWeek.TUESDAY );
            wednesday = getSegmentsForDay( hours, DayOfWeek.WEDNESDAY );
            thursday = getSegmentsForDay( hours, DayOfWeek.THURSDAY );
            friday = getSegmentsForDay( hours, DayOfWeek.FRIDAY );
            saturday = getSegmentsForDay( hours, DayOfWeek.SATURDAY );
            
            holidays = hours.getHolidays().stream()
                    .map( x -> x.format( DATE_FORMATTER ) ).collect( Collectors.toList() );
        }

        /**
         * Converts this json representation to the <see cref="MarketHoursDatabase.Entry"/> type
         * @returns A new instance of the <see cref="MarketHoursDatabase.Entry"/> class
         */
        public MarketHoursDatabase.DatabaseEntry convert() {
            final Map<DayOfWeek,LocalMarketHours> hours = new EnumMap<>( DayOfWeek.class );
            hours.put( DayOfWeek.SUNDAY, new LocalMarketHours( DayOfWeek.SUNDAY, sunday ) );
            hours.put( DayOfWeek.MONDAY, new LocalMarketHours(DayOfWeek.MONDAY, monday ) );
            hours.put( DayOfWeek.TUESDAY, new LocalMarketHours(DayOfWeek.TUESDAY, tuesday ) );
            hours.put( DayOfWeek.WEDNESDAY, new LocalMarketHours(DayOfWeek.WEDNESDAY, wednesday ) );
            hours.put( DayOfWeek.THURSDAY, new LocalMarketHours(DayOfWeek.THURSDAY, thursday ) );
            hours.put( DayOfWeek.FRIDAY, new LocalMarketHours(DayOfWeek.FRIDAY, friday ) );
            hours.put( DayOfWeek.SATURDAY, new LocalMarketHours(DayOfWeek.SATURDAY, saturday ) );
            
            final Collection<LocalDate> holidayDates = holidays.stream().map( x -> LocalDate.parse( x, DATE_FORMATTER ) ).collect( Collectors.toCollection( HashSet::new ) );
            final SecurityExchangeHours exchangeHours = new SecurityExchangeHours( ZoneId.of( exchangeTimeZone ), holidayDates, hours );
            return new MarketHoursDatabase.DatabaseEntry( ZoneId.of( dataTimeZone ), exchangeHours );
        }

        private List<MarketHoursSegment> getSegmentsForDay( final SecurityExchangeHours hours, final DayOfWeek day ) {
            final LocalMarketHours local = hours.getMarketHours().get( day );
            if( local != null )
                return local.getSegments().collect( Collectors.toList() );
            else
                return new ArrayList<>();
        }
    }
}
