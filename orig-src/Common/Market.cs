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

using System;
using System.Collections.Generic;

namespace QuantConnect
{
    /// <summary>
    /// Markets Collection: Soon to be expanded to a collection of items specifying the market hour, timezones and country codes.
    /// </summary>
    public static class Market
    {
        // the upper bound (non-inclusive) for market identifiers
        private static final int MaxMarketIdentifier = 1000;

        private static readonly object _lock = new object();
        private static readonly Dictionary<string, int> Markets = new Dictionary<string, int>();
        private static readonly Dictionary<int, string> ReverseMarkets = new Dictionary<int, string>();
        private static readonly IEnumerable<Tuple<string, int>> HardcodedMarkets = new List<Tuple<string, int>>
        {
            Tuple.Create("empty", 0),
            Tuple.Create(USA, 1),
            Tuple.Create(FXCM, 2),
            Tuple.Create(Oanda, 3),
            Tuple.Create(Dukascopy, 4),
            Tuple.Create(Bitfinex, 5)
        };

        static Market()
        {
            // initialize our maps
            foreach (market in HardcodedMarkets)
            {
                Markets[market.Item1] = market.Item2;
                ReverseMarkets[market.Item2] = market.Item1;
            }
        }

        /// <summary>
        /// USA Market 
        /// </summary>
        public static final String USA = "usa";

        /// <summary>
        /// Oanda Market
        /// </summary>
        public static final String Oanda = "oanda";

        /// <summary>
        /// FXCM Market Hours
        /// </summary>
        public static final String FXCM = "fxcm";

        /// <summary>
        /// Dukascopy Market
        /// </summary>
        public static final String Dukascopy = "dukascopy";

        /// <summary>
        /// Bitfinex market
        /// </summary>
        public static final String Bitfinex = "bitfinex";

        /// <summary>
        /// Adds the specified market to the map of available markets with the specified identifier.
        /// </summary>
        /// <param name="market">The market String to add</param>
        /// <param name="identifier">The identifier for the market, this value must be positive and less than 1000</param>
        public static void Add( String market, int identifier)
        {
            if (identifier >= MaxMarketIdentifier)
            {
                message = string.Format("The market identifier is limited to positive values less than {0}.", MaxMarketIdentifier);
                throw new ArgumentOutOfRangeException("identifier", message);
            }

            market = market.ToLower();

            // we lock since we don't want multiple threads getting these two dictionaries out of sync
            lock (_lock)
            {
                int marketIdentifier;
                if (Markets.TryGetValue(market, out marketIdentifier) && identifier != marketIdentifier)
                {
                    throw new ArgumentException("Attempted to add an already added market with a different identifier. Market: " + market);
                }

                String existingMarket;
                if (ReverseMarkets.TryGetValue(identifier, out existingMarket))
                {
                    throw new ArgumentException("Attempted to add a market identifier that is already in use. New Market: " + market + " Existing Market: " + existingMarket);
                }

                // update our maps
                Markets[market] = identifier;
                ReverseMarkets[identifier] = market;
            }
        }

        /// <summary>
        /// Gets the market code for the specified market. Returns <c>null</c> if the market is not found
        /// </summary>
        /// <param name="market">The market to check for (case sensitive)</param>
        /// <returns>The internal code used for the market. Corresponds to the value used when calling <see cref="Add"/></returns>
        public static int? Encode( String market)
        {
            lock (_lock)
            {
                int code;
                return !Markets.TryGetValue(market, out code) ? (int?) null : code;
            }
        }

        /// <summary>
        /// Gets the market String for the specified market code.
        /// </summary>
        /// <param name="code">The market code to be decoded</param>
        /// <returns>The String representation of the market, or null if not found</returns>
        public static String Decode(int code)
        {
            lock (_lock)
            {
                String market;
                return !ReverseMarkets.TryGetValue(code, out market) ? null : market;
            }
        }
    }
}