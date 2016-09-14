package com.quantconnect.lean;

import java.util.Map;

import com.google.common.collect.ImmutableMap;

/// Global Market Short Codes and their full versions: (used in tick objects)
public class MarketCodes {
    /// US Market Codes
    public static final Map<String,String> US = ImmutableMap.<String,String>builder()
            .put( "A", "American Stock Exchange" )
            .put( "B", "Boston Stock Exchange" )
            .put( "C", "National Stock Exchange" )
            .put( "D", "FINRA ADF" )
            .put( "I", "Int.put( rnational Securities Exchange" )
            .put( "J", "Direct Edge A" )
            .put( "K", "Direct Edge X" )
            .put( "M", "Chicago Stock Exchange" )
            .put( "N", "New York Stock Exchange" )
            .put( "P", "Nyse Arca Exchange" )
            .put( "Q", "NASDAQ OMX" )
            .put( "T", "NASDAQ OMX" )
            .put( "U", "OTC Bulletin Board" )
            .put( "u", "Over-the-Counter trade in Non-NASDAQ issue" )
            .put( "W", "Chicago Board Options Exchange" )
            .put( "X", "Philadelphia Stock Exchange" )
            .put( "Y", "BATS Y-Exchange, Inc" )
            .put( "Z", "BATS Exchange, Inc" )
            .build();

    /// Canada Market Short Codes:
    public static final Map<String,String> Canada = ImmutableMap.<String,String>builder()
            .put( "T", "Toronto" )
            .put( "V", "Venture" )
            .build();
}