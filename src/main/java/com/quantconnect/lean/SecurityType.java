package com.quantconnect.lean;

import java.util.Arrays;
import java.util.function.Function;
import java.util.stream.Collectors;

import com.google.common.collect.ImmutableMap;

/// Class of tradable security / underlying asset
public enum SecurityType {
    /// Base class for all security types:
    Base,

    /// US Equity Security
    Equity,

    /// Option Security Type
    Option,

    /// Commodity Security Type
    Commodity,

    /// FOREX Security
    Forex,

    /// Future Security Type
    Future,

    /// Contract For a Difference Security Type.
    Cfd;
    
    private static final ImmutableMap<Integer,SecurityType> ordinalToTypeMap = ImmutableMap.<Integer,SecurityType>builder()
            .putAll( Arrays.stream( SecurityType.values() ).collect( Collectors.toMap( st -> st.ordinal(), Function.identity() ) ) )
            .build();

    public static SecurityType fromOrdinal( final int ord ) {
        return ordinalToTypeMap.get( ord );
    }

    public static SecurityType valueOfIgnoreCase( String string ) {
        try {
            return SecurityType.valueOf( string );
        }
        catch( final Exception e ) {
            string = string.toLowerCase();
            for( final SecurityType st : SecurityType.values() ) {
                if( st.name().toLowerCase().equals( string ) )
                    return st;
            }
            return null;
        }
    }
}