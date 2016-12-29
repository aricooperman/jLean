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
 *
*/

package com.quantconnect.lean.securities;

import java.math.BigDecimal;
import java.time.Duration;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.stream.Collectors;

import com.google.common.collect.Iterables;
import com.quantconnect.lean.Global;
import com.quantconnect.lean.SecurityIdentifier;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.Time;
import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.util.FunctionalUtilities;

/**
 * Provides an implementation of <see cref="IDerivativeSecurityFilter"/> for use in selecting
 * options contracts based on a range of strikes and expiries
 */
public class StrikeExpiryOptionFilter implements IDerivativeSecurityFilter {
    
    private BigDecimal strikeSize;
    private LocalDate strikeSizeResolveDate;

    private final int minStrike;
    private final int maxStrike;
    private final Duration minExpiry;
    private final Duration maxExpiry;

    /**
     * Initializes a new instance of the <see cref="StrikeExpiryOptionFilter"/> class
     * @param minStrike The minimum strike relative to the underlying price, for example, -1 would filter out contracts further than 1 strike below market price
     * @param maxStrike The maximum strike relative to the underlying price, for example, +1 would filter out contracts further than 1 strike above market price
     * @param minExpiry The minium time until expiry, for example, 7 days would filter out contracts expiring sooner than 7 days
     * @param maxExpiry The maximum time until expiry, for example, 30 days would filter out contracts expriring later than 30 days
     */
    public StrikeExpiryOptionFilter(int minStrike, int maxStrike, Duration minExpiry, Duration maxExpiry) {
        this.minStrike = minStrike;
        this.maxStrike = maxStrike;
        this.minExpiry = minExpiry;
        // protect from overflow on additions
        this.maxExpiry = maxExpiry.compareTo( Time.MAX_TIME_SPAN ) > 0 ? Time.MAX_TIME_SPAN : maxExpiry;

        // prevent parameter mistakes that would prevent all contracts from coming through
        if( maxStrike < minStrike) throw new IllegalArgumentException( "maxStrike must be greater than minStrike" );
        if( maxExpiry.compareTo( minExpiry ) < 0 ) throw new IllegalArgumentException( "maxExpiry must be greater than minExpiry" );

    }

    /**
     * Filters the input set of symbols using the underlying price data
     * @param symbols The derivative symbols to be filtered
     * @param underlying The underlying price data
     * @returns The filtered set of symbols
     */
    public Iterable<Symbol> filter( Iterable<Symbol> symbols, BaseData underlying ) {
        // we can't properly apply this filter without knowing the underlying price
        // so in the event we're missing the underlying, just skip the filtering step
        if( underlying == null )
            return symbols;

        final LocalDate date = underlying.getTime().toLocalDate();
        if( !date.equals( strikeSizeResolveDate ) ) {
            // each day we need to recompute the strike size
            final List<Symbol> symbolsList = new ArrayList<>();
            Iterables.addAll( symbolsList, symbols );
            final List<BigDecimal> uniqueStrikes = symbolsList.stream().map( x -> x.getId().getStrikePrice() ).distinct().sorted().collect( Collectors.toList() );
            strikeSize = FunctionalUtilities.zip( uniqueStrikes.stream(), uniqueStrikes.stream().skip( 1 ), (l, r) -> r.subtract( l ) ).min( Comparator.naturalOrder() ).orElse( Global.FIVE );
//            _strikeSize = uniqueStrikes.Zip( uniqueStrikes.Skip(1), (l, r) -> r.ID.StrikePrice - l.ID.StrikePrice).DefaultIfEmpty( BigDecimal.valueOf( 5 ) ).Min();
            strikeSizeResolveDate = date;
        }

        // compute the bounds, no need to worry about rounding and such
        final BigDecimal price = underlying.getPrice();
        final BigDecimal minPrice = price.add( strikeSize.multiply( BigDecimal.valueOf( minStrike ) ) );
        final BigDecimal maxPrice = price.add( strikeSize.multiply( BigDecimal.valueOf( maxStrike ) ) );
        final LocalDate minExp = date.plus( this.minExpiry );
        final LocalDate maxExp = date.plus( this.maxExpiry );

        final List<Symbol> filtered = new ArrayList<>();
        for( Symbol symbol : symbols ) {
            final SecurityIdentifier contract = symbol.getId();
            final BigDecimal strikePrice = contract.getStrikePrice();
            final LocalDate d = contract.getDate();
            if( strikePrice.compareTo( minPrice ) >= 0 && strikePrice.compareTo( maxPrice ) <= 0 && 
                    !d.isBefore( minExp ) && !d.isAfter( maxExp ) )
                filtered.add( symbol );
            
        }

        return filtered;
    }
}
