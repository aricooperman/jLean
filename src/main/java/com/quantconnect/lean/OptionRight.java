package com.quantconnect.lean;

import java.util.Arrays;
import java.util.function.Function;
import java.util.stream.Collectors;

import com.google.common.collect.ImmutableMap;
import com.quantconnect.lean.Global.OptionRight;

/// Specifies the different types of options
public enum OptionRight {
    /// A call option, the right to buy at the strike price
    CALL,

    /// A put option, the right to sell at the strike price
    PUT;

    private static final ImmutableMap<Integer,OptionRight> ordinalToTypeMap = ImmutableMap.<Integer,OptionRight>builder()
            .putAll( Arrays.stream( OptionRight.values() ).collect( Collectors.toMap( or -> or.ordinal(), Function.identity() ) ) )
            .build();

    public static OptionRight fromOrdinal( int ord ) {
        return ordinalToTypeMap.get( ord );
    }
}