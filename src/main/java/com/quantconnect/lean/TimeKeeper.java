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

package com.quantconnect.lean;

import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.Arrays;
import java.util.Collections;
import java.util.Map;
import java.util.function.Function;
import java.util.stream.Collectors;
import java.util.stream.StreamSupport;

//using System.Linq;
//using NodaTime;

/// Provides a means of centralizing time for various time zones.
public class TimeKeeper {
    
    private LocalDateTime utcDateTime;

    private final Map<ZoneId,LocalTimeKeeper> localTimeKeepers;
    
    /// Gets the current time in UTC
    public LocalDateTime getUtcTime() {
        return utcDateTime; 
    }
    
    /// Initializes a new instance of the <see cref="TimeKeeper"/> class at the specified
    /// UTC time and for the specified time zones. Each time zone specified will cause the
    /// creation of a <see cref="LocalTimeKeeper"/> to handle conversions for that time zone.
     * @param utcDateTime">The initial time
     * @param timeZones">The time zones used to instantiate <see cref="LocalTimeKeeper"/> instances.
    public TimeKeeper( LocalDateTime utcDateTime, ZoneId... timeZones ) {
        this( utcDateTime, timeZones != null ? Arrays.asList( timeZones ) : Collections.<ZoneId>emptyList() );
    }
    
    /// Initializes a new instance of the <see cref="TimeKeeper"/> class at the specified
    /// UTC time and for the specified time zones. Each time zone specified will cause the
    /// creation of a <see cref="LocalTimeKeeper"/> to handle conversions for that time zone.
     * @param utcDateTime">The initial time
     * @param timeZones">The time zones used to instantiate <see cref="LocalTimeKeeper"/> instances.
    public TimeKeeper( LocalDateTime utcDateTime, Iterable<ZoneId> timeZones )  {
        this.utcDateTime = utcDateTime;
        this.localTimeKeepers = StreamSupport.stream( timeZones.spliterator(), false )
                .distinct()
                .map( x -> new LocalTimeKeeper( utcDateTime, x ) )
                .collect( Collectors.toMap( LocalTimeKeeper::getTimeZone, Function.identity() ) );
    }
    
    /// Sets the current UTC time for this time keeper and the attached child <see cref="LocalTimeKeeper"/> instances.
     * @param utcDateTime">The current time in UTC
    public void setUtcDateTime( LocalDateTime utcDateTime )  {
        this.utcDateTime = utcDateTime;
        for( LocalTimeKeeper timeZone : localTimeKeepers.values() )
            timeZone.updateTime( utcDateTime );
    }
    
    /// Gets the local time in the specified time zone. If the specified <see cref="ZoneId"/>
    /// has not already been added, this will throw a <see cref="KeyNotFoundException"/>.
     * @param timeZone">The time zone to get local time for
    @returns The local time in the specifed time zone
    public LocalDateTime getTimeIn( ZoneId timeZone ) {
        return getLocalTimeKeeper( timeZone ).getLocalTime();
    }
    
    /// Gets the <see cref="LocalTimeKeeper"/> instance for the specified time zone
     * @param timeZone">The time zone whose <see cref="LocalTimeKeeper"/> we seek
    @returns The <see cref="LocalTimeKeeper"/> instance for the specified time zone
    public LocalTimeKeeper getLocalTimeKeeper( ZoneId timeZone ) {
        return localTimeKeepers.computeIfAbsent( timeZone, tz -> new LocalTimeKeeper( utcDateTime, tz ) );
    }
    
    /// Adds the specified time zone to this time keeper
     * @param timeZone">
    public void addTimeZone( ZoneId timeZone ) {
        localTimeKeepers.putIfAbsent( timeZone, new LocalTimeKeeper( utcDateTime, timeZone ) );
    }
}
