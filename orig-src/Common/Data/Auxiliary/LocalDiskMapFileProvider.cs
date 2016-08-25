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

using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using QuantConnect.Interfaces;
using QuantConnect.Logging;

package com.quantconnect.lean.Data.Auxiliary
{
    /**
     * Provides a default implementation of <see cref="IMapFileProvider"/> that reads from
     * the local disk
    */
    public class LocalDiskMapFileProvider : IMapFileProvider
    {
        private static int _wroteTraceStatement;
        private final ConcurrentMap<String, MapFileResolver> _cache = new ConcurrentMap<String, MapFileResolver>();

        /**
         * Gets a <see cref="MapFileResolver"/> representing all the map
         * files for the specified market
        */
         * @param market The equity market, for example, 'usa'
        @returns A <see cref="MapFileRow"/> containing all map files for the specified market
        public MapFileResolver Get( String market) {
            market = market.toLowerCase();
            return _cache.GetOrAdd(market, GetMapFileResolver);
        }

        private static MapFileResolver GetMapFileResolver( String market) {
            mapFileDirectory = Path.Combine(Globals.DataFolder, "equity", market.toLowerCase(), "map_files");
            if( !Directory.Exists(mapFileDirectory)) {
                // only write this message once per application instance
                if( Interlocked.CompareExchange(ref _wroteTraceStatement, 1, 0) == 0) {
                    Log.Error( "LocalDiskMapFileProvider.GetMapFileResolver(%1$s): The specified directory does not exist: %2$s", market, mapFileDirectory);
                }
                return MapFileResolver.Empty;
            }
            return new MapFileResolver(MapFile.GetMapFiles(mapFileDirectory));
        }
    }
}
