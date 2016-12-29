package com.quantconnect.lean.charting;

import java.math.BigDecimal;
import java.time.ZonedDateTime;

import com.quantconnect.lean.Extensions;
import com.quantconnect.lean.Global;
import com.quantconnect.lean.Time;

/**
 *  Single Chart Point Value Class for QCAlgorithm.Plot();
 */
//    [JsonObject]
public class ChartPoint {
    /**
     * Time of this chart point: lower case for javascript encoding simplicty
     */
    public long x;

    /**
     * Value of this chart point:  lower case for javascript encoding simplicty
     */
    public BigDecimal y;

    /**
     * Constructor for datetime-value arguements:
     * @param time
     * @param value
     */
    public ChartPoint( ZonedDateTime time, BigDecimal value ) {
        x = time.withZoneSameInstant( Global.UTC_ZONE_TZ_ID ).toInstant().toEpochMilli();
        y = Extensions.smartRounding( value );
    }

    /**
     * Cloner Constructor:
     * @param point
     */
    public ChartPoint( ChartPoint point ) {
        x = point.x;
        y = Extensions.smartRounding( point.y );
    }

    /**
     * Provides a readable String representation of this instance.
     */
    @Override
    public String toString() {
        return Time.unixTimeStampToDateTime( x ).toString() + " - " + y;
    }
}