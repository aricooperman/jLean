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

using System;
using System.Collections.Concurrent;

namespace QuantConnect
{
    /// <summary>
    /// Provides a string->Symbol mapping to allow for user defined strings to be lifted into a Symbol
    /// This is mainly used via the Symbol implicit operator, but also functions that create securities
    /// should also call Set to add new mappings
    /// </summary>
    public static class SymbolCache
    {
        // we aggregate the two maps into a class so we can assign a new one as an atomic operation
        private static Cache _cache = new Cache();

        /// <summary>
        /// Adds a mapping for the specified ticker
        /// </summary>
        /// <param name="ticker">The String ticker symbol</param>
        /// <param name="symbol">The symbol object that maps to the String ticker symbol</param>
        public static void Set( String ticker, Symbol symbol)
        {
            _cache.Symbols[ticker] = symbol;
            _cache.Tickers[symbol] = ticker;
        }

        /// <summary>
        /// Gets the Symbol object that is mapped to the specified String ticker symbol
        /// </summary>
        /// <param name="ticker">The String ticker symbol</param>
        /// <returns>The symbol object that maps to the specified String ticker symbol</returns>
        public static Symbol GetSymbol( String ticker)
        {
            Symbol symbol;
            if (TryGetSymbol(ticker, out symbol)) return symbol;
            throw new Exception( String.Format("We were unable to locate the ticker '{0}'.", ticker));
        }

        /// <summary>
        /// Gets the Symbol object that is mapped to the specified String ticker symbol
        /// </summary>
        /// <param name="ticker">The String ticker symbol</param>
        /// <param name="symbol">The output symbol object</param>
        /// <returns>The symbol object that maps to the specified String ticker symbol</returns>
        public static boolean TryGetSymbol( String ticker, out Symbol symbol)
        {
            return _cache.TryGetSymbol(ticker, out symbol);
        }

        /// <summary>
        /// Gets the String ticker symbol that is mapped to the specified Symbol
        /// </summary>
        /// <param name="symbol">The symbol object</param>
        /// <returns>The String ticker symbol that maps to the specified symbol object</returns>
        public static String GetTicker(Symbol symbol)
        {
            String ticker;
            return _cache.Tickers.TryGetValue(symbol, out ticker) ? ticker : symbol.ID.ToString();
        }

        /// <summary>
        /// Gets the String ticker symbol that is mapped to the specified Symbol
        /// </summary>
        /// <param name="symbol">The symbol object</param>
        /// <param name="ticker">The output String ticker symbol</param>
        /// <returns>The String ticker symbol that maps to the specified symbol object</returns>
        public static boolean TryGetTicker(Symbol symbol, out String ticker)
        {
            return _cache.Tickers.TryGetValue(symbol, out ticker);
        }

        /// <summary>
        /// Removes the mapping for the specified symbol from the cache
        /// </summary>
        /// <param name="symbol">The symbol whose mappings are to be removed</param>
        /// <returns>True if the symbol mapping were removed from the cache</returns>
        public static boolean TryRemove(Symbol symbol)
        {
            String ticker;
            return _cache.Tickers.TryRemove(symbol, out ticker) && _cache.Symbols.TryRemove(ticker, out symbol);
        }

        /// <summary>
        /// Removes the mapping for the specified symbol from the cache
        /// </summary>
        /// <param name="ticker">The ticker whose mappings are to be removed</param>
        /// <returns>True if the symbol mapping were removed from the cache</returns>
        public static boolean TryRemove( String ticker)
        {
            Symbol symbol;
            return _cache.Symbols.TryRemove(ticker, out symbol) && _cache.Tickers.TryRemove(symbol, out ticker);
        }

        /// <summary>
        /// Clears the current caches
        /// </summary>
        public static void Clear()
        {
            _cache = new Cache();
        }

        class Cache
        {
            public readonly ConcurrentDictionary<string, Symbol> Symbols = new ConcurrentDictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
            public readonly ConcurrentDictionary<Symbol, string> Tickers = new ConcurrentDictionary<Symbol, string>();

            /// <summary>
            /// Attempts to resolve the ticker to a Symbol via the cache. If not found in the
            /// cache then
            /// </summary>
            /// <param name="ticker">The ticker to resolver to a symbol</param>
            /// <param name="symbol">The resolves symbol</param>
            /// <returns>True if we successfully resolved a symbol, false otherwise</returns>
            public boolean TryGetSymbol( String ticker, out Symbol symbol)
            {
                if (Symbols.TryGetValue(ticker, out symbol))
                {
                    return true;
                }
                SecurityIdentifier sid;
                if (SecurityIdentifier.TryParse(ticker, out sid))
                {
                    symbol = new Symbol(sid, sid.Symbol);
                    return true;
                }
                return false;
            }
        }
    }
}