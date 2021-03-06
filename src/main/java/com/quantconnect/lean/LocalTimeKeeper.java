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
import java.util.concurrent.CopyOnWriteArrayList;

import javaslang.Lazy;


/**
 * Represents the current local time. This object is created via the <see cref="TimeKeeper"/> to
 * manage conversions to local time.
 */
public class LocalTimeKeeper {
    
    @FunctionalInterface
    public interface TimeUpdatedEventListener {
        void timeUpdated( LocalDateTime dateTime, ZoneId timeZone );
    }

    private Lazy<LocalDateTime> localTime;
    private final ZoneId timeZone;

    //Event fired each time <see cref="UpdateTime"/> is called
//    public /*event*/ EventHandler<TimeUpdatedEventArgs> TimeUpdated;
    private CopyOnWriteArrayList<TimeUpdatedEventListener> listeners = new CopyOnWriteArrayList<>();
    
    public void addListener( TimeUpdatedEventListener listener ) {
        listeners.add( listener );
    }
    
    public boolean removeListener( TimeUpdatedEventListener listener ) {
        return listeners.remove( listener );
    }
    
    /// Gets the time zone of this <see cref="LocalTimeKeeper"/>
    public ZoneId getTimeZone() {
        return timeZone;
    }

    /// Gets the current time in terms of the <see cref="TimeZone"/>
    public LocalDateTime getLocalTime() {
        return localTime.get();
    }

    /**
     *  Initializes a new instance of the <see cref="LocalTimeKeeper"/> class
     * @param utcDateTime The current time in UTC
     * @param timeZone The time zone
     */
    protected LocalTimeKeeper( LocalDateTime utcDateTime, ZoneId timeZone ) {
        this.timeZone = timeZone;
        this.localTime = Lazy.of( () -> utcDateTime.atZone( timeZone ).toLocalDateTime() );
    }

    /**
     * Updates the current time of this time keeper
     * @param utcDateTime The current time in UTC
     */
    protected void updateTime( LocalDateTime utcDateTime ) {
        // redefine the lazy conversion each time this is set
        localTime = Lazy.of( () -> utcDateTime.atZone( timeZone ).toLocalDateTime() );
        if( listeners.size() > 0 ) {
            final LocalDateTime dateTime = localTime.get();
            listeners.parallelStream().forEach( l -> l.timeUpdated( dateTime, timeZone ) ); 
        }
//        Global.APP_EVENT_BUS.post( new TimeUpdatedEventArgs( localTime.get(), timeZone ) );
    }
}