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

package com.quantconnect.lean;

import java.util.Optional;
import java.util.concurrent.ConcurrentSkipListMap;

 * Provides a string->Symbol mapping to allow for user defined strings to be lifted into a Symbol
 * This is mainly used via the Symbol implicit operator, but also functions that create securities
 * should also call Set to add new mappings
public class SymbolCache {
    
    // we aggregate the two maps into a class so we can assign a new one as an atomic operation
    private static Cache cache = new Cache();

     * Adds a mapping for the specified ticker
     * @param ticker The String ticker symbol
     * @param symbol The symbol object that maps to the String ticker symbol
    public static void set( String ticker, Symbol symbol ) {
        cache.symbols.put( ticker, symbol );
        cache.tickers.put( symbol, ticker );
    }

     * Gets the Symbol object that is mapped to the specified String ticker symbol
     * @param ticker The String ticker symbol
    @returns The symbol object that maps to the specified String ticker symbol
    public static Symbol getSymbol( String ticker ) {
        return tryGetSymbol( ticker )
                .orElseThrow( () -> new RuntimeException( String.format( "We were unable to locate the ticker '%s'.", ticker ) ) );
    }

     * Gets the Symbol object that is mapped to the specified String ticker symbol
     * @param ticker The String ticker symbol
     * @param symbol The output symbol object
    @returns The symbol object that maps to the specified String ticker symbol
    public static Optional<Symbol> tryGetSymbol( String ticker ) {
        return cache.tryGetSymbol( ticker );
    }

     * Gets the String ticker symbol that is mapped to the specified Symbol
     * @param symbol The symbol object
    @returns The String ticker symbol that maps to the specified symbol object
    public static String getTicker( Symbol symbol ) {
        final String ticker = cache.tickers.get( symbol );
        return ticker != null ? ticker : symbol.getId().toString();
    }

     * Gets the String ticker symbol that is mapped to the specified Symbol
     * @param symbol The symbol object
     * @param ticker The output String ticker symbol
    @returns The String ticker symbol that maps to the specified symbol object
    public static String tryGetTicker( Symbol symbol ) {
        return cache.tickers.get( symbol );
    }

     * Removes the mapping for the specified symbol from the cache
     * @param symbol The symbol whose mappings are to be removed
    @returns True if the symbol mapping were removed from the cache
    public static boolean tryRemove( Symbol symbol ) {
        final String ticker = cache.tickers.remove( symbol );
        return ticker != null && cache.symbols.remove( ticker ) != null;
    }

     * Removes the mapping for the specified symbol from the cache
     * @param ticker The ticker whose mappings are to be removed
    @returns True if the symbol mapping were removed from the cache
    public static boolean tryRemove( String ticker ) {
        final Symbol symbol = cache.symbols.remove( ticker );
        return symbol != null && cache.tickers.remove( symbol ) != null;
    }

     * Clears the current caches
    public static void clear() {
        cache = new Cache();
    }

    private static class Cache {
        private final ConcurrentSkipListMap<String,Symbol> symbols = new ConcurrentSkipListMap<>( String::compareToIgnoreCase );
        private final ConcurrentSkipListMap<Symbol,String> tickers = new ConcurrentSkipListMap<>();

         * Attempts to resolve the ticker to a Symbol via the cache. If not found in the
         * cache then
         * @param ticker The ticker to resolver to a symbol
         * @param symbol The resolves symbol
        @returns True if we successfully resolved a symbol, false otherwise
        public Optional<Symbol> tryGetSymbol( String ticker ) {
            return Optional.ofNullable( 
                    symbols.computeIfAbsent( ticker, t -> SecurityIdentifier.parse( t )
                    .map( sid ->  new Symbol( sid, sid.getSymbol() ) )
                    .orElse( null ) ) );
        }
    }
}