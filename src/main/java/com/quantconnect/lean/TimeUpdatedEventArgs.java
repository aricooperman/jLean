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

import java.io.Serializable;
import java.time.LocalDateTime;
import java.time.ZoneId;

/// Event arguments class for the <see cref="LocalTimeKeeper.TimeUpdated"/> event
public final class TimeUpdatedEventArgs implements Serializable {
    private static final long serialVersionUID = 3577409383237159799L;

    /// Gets the new time
    public final LocalDateTime time;

    /// Gets the time zone
    public final ZoneId timeZone;

    /// Initializes a new instance of the <see cref="TimeUpdatedEventArgs"/> class
     * @param time">The newly updated time
     * @param timeZone">The time zone of the new time
    public TimeUpdatedEventArgs( LocalDateTime time, ZoneId timeZone ) {
        this.time = time;
        this.timeZone = timeZone;
    }
}
