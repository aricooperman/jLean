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

import java.util.HashMap;
import java.util.Map;

import com.google.common.collect.BiMap;
import com.google.common.collect.ImmutableBiMap;

/**
 * Markets Collection: Soon to be expanded to a collection of items specifying the market hour, timezones and country codes.
 */
public class Market {
    // the upper bound (non-inclusive) for market identifiers
    private static final int MaxMarketIdentifier = 1000;
   
    /**
     * USA Market
     */ 
    public static final String USA = "usa";
    
    /**
     * Oanda Market
     */
    public static final String Oanda = "oanda";
    
    /**
     * FXCM Market Hours
     */
    public static final String FXCM = "fxcm";
    
    /**
     * Dukascopy Market
     */
    public static final String Dukascopy = "dukascopy";
    
    /**
     * Bitfinex market
     */
    public static final String Bitfinex = "bitfinex";

    private static final Object lock = new Object();
    
    private static final BiMap<String,Integer> HardcodedMarkets = ImmutableBiMap.<String,Integer>builder()
        .put( "empty", 0 )
        .put( USA, 1 )
        .put( FXCM, 2 )
        .put( Oanda, 3 )
        .put( Dukascopy, 4 )
        .put( Bitfinex, 5 )
        .build();
    private static final Map<String,Integer> Markets = new HashMap<>( HardcodedMarkets );
    private static final Map<Integer,String> ReverseMarkets = HardcodedMarkets.inverse();

    /**
     * Adds the specified market to the map of available markets with the specified identifier.
     * @param market The market String to add
     * @param identifier The identifier for the market, this value must be positive and less than 1000
     */
    public static void add( String market, int identifier ) {
        if( identifier >= MaxMarketIdentifier )
            throw new IndexOutOfBoundsException( String.format( "The market identifier is limited to positive values less than %d.", MaxMarketIdentifier ) );

        market = market.toLowerCase();

        // we synchronizedsince we don't want multiple threads getting these two dictionaries out of sync
        synchronized( lock ) {
            final Integer marketIdentifier = Markets.get( market );
            if( marketIdentifier != null && identifier != marketIdentifier)
                throw new IllegalArgumentException( "Attempted to add an already added market with a different identifier. Market: " + market );

            final String existingMarket = ReverseMarkets.get( identifier );
            if( existingMarket != null )
                throw new IllegalArgumentException( "Attempted to add a market identifier that is already in use. New Market: " + market + 
                        " Existing Market: " + existingMarket );

            // update our maps
            Markets.put( market, identifier );
            ReverseMarkets.put( identifier, market );
        }
    }

    /**
     * Gets the market code for the specified market. Returns <c>null</c> if the market is not found
     * @param market The market to check for (case sensitive)
     * @returns The internal code used for the market. Corresponds to the value used when calling <see cref="Add"/>
     */
    public static Integer encode( String market ) {
        synchronized( lock) {
            return Markets.get( market );
        }
    }

    /**
     * Gets the market String for the specified market code.
     * @param code The market code to be decoded
     * @returns The String representation of the market, or null if not found
     */
    public static String decode( int code ) {
        synchronized( lock) {
            return ReverseMarkets.get( code );
        }
    }
}