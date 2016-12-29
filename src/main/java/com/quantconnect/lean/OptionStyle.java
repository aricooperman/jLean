package com.quantconnect.lean;

import java.util.Arrays;
import java.util.function.Function;
import java.util.stream.Collectors;

import com.google.common.collect.ImmutableMap;

/// Specifies the style of an option
public enum OptionStyle {
    /// American style options are able to be exercised at any time on or before the expiration date
    AMERICAN,

    /// European style options are able to be exercised on the expiration date only.
    EUROPEAN;

    private static final ImmutableMap<Integer,OptionStyle> ordinalToTypeMap = ImmutableMap.<Integer,OptionStyle>builder()
            .putAll( Arrays.stream( OptionStyle.values() ).collect( Collectors.toMap( os -> os.ordinal(), Function.identity() ) ) )
            .build();

    public static OptionStyle fromOrdinal( int ord ) {
        return ordinalToTypeMap.get( ord );
    }
}