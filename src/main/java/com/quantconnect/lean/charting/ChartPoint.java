package com.quantconnect.lean.charting;

import java.math.BigDecimal;
import java.sql.Time;
import java.time.ZoneId;
import java.time.ZonedDateTime;

/**
 *  Single Chart Point Value Type for QCAlgorithm.Plot();
 */
//    [JsonObject]
public class ChartPoint {
    /// Time of this chart point: lower case for javascript encoding simplicty
    public long x;

    /// Value of this chart point:  lower case for javascript encoding simplicty
    public BigDecimal y;

    ///Constructor for datetime-value arguements:
    public ChartPoint( ZonedDateTime time, BigDecimal value ) {
        x = time.toInstant().atZone( ZoneId.of( "UTC" ) ).toInstant().toEpochMilli();
        y = value.SmartRounding();
    }

    ///Cloner Constructor:
    public ChartPoint( ChartPoint point ) {
        x = point.x;
        y = point.y.SmartRounding();
    }

    /// Provides a readable String representation of this instance.
    @Overrides
    public String toString() {
        return Time.UnixTimeStampToDateTime(x).toString("o") + " - " + y;
    }
}