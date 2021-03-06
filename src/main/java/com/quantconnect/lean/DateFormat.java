package com.quantconnect.lean;

import java.time.format.DateTimeFormatter;

/// Shortcut date format strings
public class DateFormat {
    
    /// Year-Month-Date 6 Character Date Representation
    public static final DateTimeFormatter SixCharacter = DateTimeFormatter.ofPattern( "yyMMdd" );
    
    /// YYYY-MM-DD Eight Character Date Representation
    public static final DateTimeFormatter EightCharacter = DateTimeFormatter.ofPattern( "yyyyMMdd" );
    
    /// Daily and hourly time format
    public static final DateTimeFormatter TwelveCharacter = DateTimeFormatter.ofPattern( "yyyyMMdd HH:mm" );
    
    /// JSON Format Date Representation
    public static final DateTimeFormatter JsonFormat = DateTimeFormatter.ofPattern( "yyyy-MM-ddThh:mm:ss" );
    
    /// MySQL Format Date Representation
    public static final DateTimeFormatter DB = DateTimeFormatter.ofPattern( "yyyy-MM-dd HH:mm:ss" );
    
    /// QuantConnect UX Date Representation
    public static final DateTimeFormatter UI = DateTimeFormatter.ofPattern( "yyyy-MM-dd HH:mm:ss" );
    
    /// en-US format
    public static final DateTimeFormatter US = DateTimeFormatter.ofPattern( "M/d/yyyy h:mm:ss tt" );
    
    /// Date format of QC forex data
    public static final DateTimeFormatter Forex = DateTimeFormatter.ofPattern( "yyyyMMdd HH:mm:ss.SSSS" );
}