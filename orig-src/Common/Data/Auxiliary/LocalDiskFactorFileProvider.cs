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
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Util;

package com.quantconnect.lean.Data.Auxiliary
{
    /**
     * Provides an implementation of <see cref="IFactorFileProvider"/> that searches the local disk
    */
    public class LocalDiskFactorFileProvider : IFactorFileProvider
    {
        private final IMapFileProvider _mapFileProvider;
        private final ConcurrentMap<Symbol, FactorFile> _cache;

        /**
         * Initializes a new instance of <see cref="LocalDiskFactorFileProvider"/> that uses configuration
         * to resolve an instance of <see cref="IMapFileProvider"/> from the <see cref="Composer.Instance"/>
        */
        public LocalDiskFactorFileProvider()
            : this(Composer.Instance.GetExportedValueByTypeName<IMapFileProvider>(Config.Get( "map-file-provider", "LocalDiskMapFileProvider"))) {
        }

        /**
         * Initializes a new instance of the <see cref="LocalDiskFactorFileProvider"/> using the specified
         * map file provider
        */
         * @param mapFileProvider The map file provider used to resolve permticks of securities
        public LocalDiskFactorFileProvider(IMapFileProvider mapFileProvider) {
            _mapFileProvider = mapFileProvider;
            _cache = new ConcurrentMap<Symbol, FactorFile>();
        }

        /**
         * Gets a <see cref="FactorFile"/> instance for the specified symbol, or null if not found
        */
         * @param symbol The security's symbol whose factor file we seek
        @returns The resolved factor file, or null if not found
        public FactorFile Get(Symbol symbol) {
            FactorFile factorFile;
            if( _cache.TryGetValue(symbol, out factorFile)) {
                return factorFile;
            }

            market = symbol.ID.Market;

            // we first need to resolve the map file to get a permtick, that's how the factor files are stored
            mapFileResolver = _mapFileProvider.Get(market);
            if( mapFileResolver == null ) {
                return GetFactorFile(symbol, symbol.Value, market);
            }

            mapFile = mapFileResolver.ResolveMapFile(symbol.ID.Symbol, symbol.ID.Date);
            if( mapFile.IsNullOrEmpty()) {
                return GetFactorFile(symbol, symbol.Value, market);
            }

            return GetFactorFile(symbol, mapFile.Permtick, market);
        }

        /**
         * Checks that the factor file exists on disk, and if it does, loads it into memory
        */
        private FactorFile GetFactorFile(Symbol symbol, String permtick, String market) {
            if( FactorFile.HasScalingFactors(permtick, market)) {
                factorFile = FactorFile.Read(permtick, market);
                _cache.AddOrUpdate(symbol, factorFile, (s, c) -> factorFile);
                return factorFile;
            }
            // return null if not found
            return null;
        }
    }
}
