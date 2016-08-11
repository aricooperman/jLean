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

package com.quantconnect.lean.data.auxiliary;

import java.io.IOException;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.time.LocalDate;
import java.util.Collections;
import java.util.Comparator;
import java.util.Iterator;
import java.util.Map;
import java.util.SortedMap;
import java.util.TreeMap;

//using System.Linq;
//using QuantConnect.Logging;
//using QuantConnect.Util;

/// Provides a means of mapping a symbol at a point in time to the map file
/// containing that share class's mapping information
public class MapFileResolver implements Iterable<MapFile> {
    
    private static final Comparator<String> IGNORE_CASE_COMP = String::compareToIgnoreCase;
    
    private final Map<String,MapFile> mapFilesByPermtick;
    private final Map<String,SortedMap<LocalDate,MapFileRowEntry>> bySymbol;

    /// Gets an empty <see cref="MapFileResolver"/>, that is an instance that contains
    /// zero mappings
//    public static final MapFileResolver Empty = new MapFileResolver( Collections.emptyList() ) ;

    /// Initializes a new instance of the <see cref="MapFileResolver"/> by reading
    /// in all files in the specified directory.
    /// <param name="mapFiles">The data used to initialize this resolver.</param>
    public MapFileResolver( Iterable<MapFile> mapFiles ) {
        mapFilesByPermtick = new TreeMap<String,MapFile>( IGNORE_CASE_COMP );
        bySymbol = new TreeMap<String,SortedMap<LocalDate,MapFileRowEntry>>( IGNORE_CASE_COMP );

        for( MapFile mapFile : mapFiles ) {
            // add to our by path map
            mapFilesByPermtick.put( mapFile.getPermtick(), mapFile );

            for( MapFileRow row : mapFile ) {
                
                final MapFileRowEntry mapFileRowEntry = new MapFileRowEntry( mapFile.getPermtick(), row );
                SortedMap<LocalDate,MapFileRowEntry> entries = bySymbol.get( row.getMappedSymbol() );
                
                if( entries == null ) {
                    entries = new TreeMap<LocalDate,MapFileRowEntry>();
                    bySymbol.put( row.getMappedSymbol(), entries );
                }

                if( entries.containsKey( mapFileRowEntry.mapFileRow.getDate() ) ) {
                    // check to verify it' the same data
                    if( !entries.get( mapFileRowEntry.mapFileRow.getDate() ).equals( mapFileRowEntry ) )
                        throw new RuntimeException( "Attempted to assign different history for symbol." );
                }
                else
                    entries.put( mapFileRowEntry.mapFileRow.getDate(), mapFileRowEntry );
            }
        }
    }

    /// Creates a new instance of the <see cref="MapFileResolver"/> class by reading all map files
    /// for the specified market into memory
    /// <param name="dataDirectory">The root data directory</param>
    /// <param name="market">The equity market to produce a map file collection for</param>
    /// <returns>The collection of map files capable of mapping equity symbols within the specified market</returns>
    public static MapFileResolver create( String dataDirectory, String market ) throws IOException {
        return create( Paths.get( dataDirectory, "equity", market.toLowerCase(), "map_files" ) );
    }

    /// Creates a new instance of the <see cref="MapFileResolver"/> class by reading all map files
    /// for the specified market into memory
    /// <param name="mapFileDirectory">The directory containing the map files</param>
    /// <returns>The collection of map files capable of mapping equity symbols within the specified market</returns>
    public static MapFileResolver create( Path mapFileDirectory ) throws IOException {
        return new MapFileResolver( MapFile.getMapFiles( mapFileDirectory ) );
    }

    /// Gets the map file matching the specified permtick
    /// <param name="permtick">The permtick to match on</param>
    /// <returns>The map file matching the permtick, or null if not found</returns>
    public MapFile getByPermtick( String permtick ) {
        return mapFilesByPermtick.get( permtick.toUpperCase() );
    }

    /// Resolves the map file path containing the mapping information for the symbol defined at <paramref name="date"/>
    /// <param name="symbol">The symbol as of <paramref name="date"/> to be mapped</param>
    /// <param name="date">The date associated with the <paramref name="symbol"/></param>
    /// <returns>The map file responsible for mapping the symbol, if no map file is found, null is returned</returns>
    public MapFile resolveMapFile( String symbol, LocalDate date ) {
        // lookup the symbol's history
        final SortedMap<LocalDate,MapFileRowEntry> entries = bySymbol.get( symbol );
        if( entries != null ) {
            if( entries.isEmpty() )
                return new MapFile( symbol, Collections.<MapFileRow>emptyList().stream() );

            MapFileRowEntry mapFileRowEntry = entries.get( date );
            if( mapFileRowEntry != null )
                symbol = mapFileRowEntry.entitySymbol;
            else {
                if( date.isBefore( entries.firstKey() ) || date.isAfter( entries.lastKey() ) )
                    return new MapFile( symbol, Collections.<MapFileRow>emptyList().stream() );

                final SortedMap<LocalDate,MapFileRowEntry> headMap = entries.headMap( date );
                symbol = headMap.get( headMap.lastKey() ).entitySymbol;
            }
        }
        
        // secondary search for exact mapping, find path than ends with symbol.csv
        MapFile mapFile = mapFilesByPermtick.get( symbol );
        if( mapFile == null )
            return new MapFile( symbol, Collections.<MapFileRow>emptyList().stream() );

        return mapFile;
    }

    /// Returns an enumerator that iterates through the collection.
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    @Override
    public Iterator<MapFile> iterator() {
        return mapFilesByPermtick.values().iterator();
    }

    
    /// Combines the map file row with the map file path that produced the row
    class MapFileRowEntry {
        /// Gets the map file row
        private MapFileRow mapFileRow;
//        { get; private set; }

        /// Gets the full path to the map file that produced this row
        public String entitySymbol;
//        { get; private set; }

        /// Initializes a new instance of the <see cref="MapFileRowEntry"/> class
        /// <param name="entitySymbol">The map file that produced this row</param>
        /// <param name="mapFileRow">The map file row data</param>
        public MapFileRowEntry( String entitySymbol, MapFileRow mapFileRow ) {
            this.mapFileRow = mapFileRow;
            this.entitySymbol = entitySymbol;
        }

        /// Indicates whether the current object is equal to another object of the same type.
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public boolean equals( MapFileRowEntry other ) {
            if( other == null ) return false;
            return other.mapFileRow.getDate().equals( mapFileRow.getDate() )
                && other.mapFileRow.getMappedSymbol().equals( mapFileRow.getMappedSymbol() );
        }

        /// Returns a String that represents the current object.
        /// <returns>
        /// A String that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public String toString() {
            return mapFileRow.getDate() + ": " + mapFileRow.getMappedSymbol() + ": " + entitySymbol;
        }
    }
}
