﻿/*
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
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.time.LocalDate;
import java.util.Collections;
import java.util.Iterator;
import java.util.SortedMap;
import java.util.function.Function;
import java.util.stream.Collectors;
import java.util.stream.Stream;

import org.apache.commons.lang3.tuple.Pair;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.common.collect.ImmutableSortedMap;
import com.google.common.collect.ImmutableSortedMap.Builder;
import com.quantconnect.lean.Globals;

//using System.IO;
//using System.Linq;
//using QuantConnect.Logging;

/// Represents an entire map file for a specified symbol
public class MapFile implements Iterable<MapFileRow> {
    
    private static final Logger LOG = LoggerFactory.getLogger( MapFile.class );
    
    private final SortedMap<LocalDate,MapFileRow> _data;

    /// Gets the entity's unique symbol, i.e OIH.1
    private  String Permtick;

    public String getPermtick() {
        return Permtick;
    }

    /// Gets the last date in the map file which is indicative of a delisting event
    public LocalDate getDelistingDate() {
        return _data.isEmpty() ? LocalDate.MAX : _data.lastKey();
    }

    /// Gets the first date in this map file
    public LocalDate getFirstDate() {
        return _data.isEmpty() ? LocalDate.MIN : _data.firstKey();
    }

    /// Initializes a new instance of the <see cref="MapFile"/> class.
    public MapFile( String permtick, Stream<MapFileRow> stream ) {
        this.Permtick = permtick.toUpperCase();
        final Builder<LocalDate,MapFileRow> builder = ImmutableSortedMap.<LocalDate,MapFileRow>naturalOrder();
        builder.putAll( stream.distinct().collect( Collectors.toMap( x -> x.getDate(), Function.identity() ) ) );
        _data = builder.build();
    }

    /// Memory overload search method for finding the mapped symbol for this date.
    /// <param name="searchDate">date for symbol we need to find.</param>
    /// <returns>Symbol on this date.</returns>
    public String getMappedSymbol( LocalDate searchDate ) {
        String mappedSymbol = "";
        //Iterate backwards to find the most recent factor:
        for( LocalDate splitDate : _data.keySet() ) {
            if( splitDate.isBefore( searchDate ) ) continue;
            mappedSymbol = _data.get( splitDate ).getMappedSymbol();
            break;
        }
        
        return mappedSymbol;
    }

    /// Determines if there's data for the requested date
    public boolean hasData( LocalDate date ) {
        // handle the case where we don't have any data
        if( _data.isEmpty() )
            return true;

        if( date.isBefore( _data.firstKey() ) || date.isAfter( _data.lastKey() ) ) {
            // don't even bother checking the disk if the map files state we don't have ze dataz
            return false;
        }
        
        return true;
    }

    /// Reads in an entire map file for the requested symbol from the DataFolder
    public static MapFile read( String permtick, String market ) throws IOException {
        return new MapFile( permtick, MapFileRow.read( permtick, market ) );
    }

    /// Constructs the map file path for the specified market and symbol
    /// <param name="permtick">The symbol as on disk, OIH or OIH.1</param>
    /// <param name="market">The market this symbol belongs to</param>
    /// <returns>The file path to the requested map file</returns>
    public static Path getMapFilePath( String permtick, String market ) {
        return Paths.get( Globals.getDataFolder(), "equity", market, "map_files", permtick.toLowerCase() + ".csv" );
    }

    /// Reads all the map files in the specified directory
    /// <param name="mapFileDirectory">The map file directory path</param>
    /// <returns>An enumerable of all map files</returns>
    public static Iterable<MapFile> getMapFiles( Path mapFileDirectory ) throws IOException {
        return Files.list( mapFileDirectory )
                .filter( p -> p.endsWith( ".csv" ) )
                .map( p -> Pair.of( com.google.common.io.Files.getNameWithoutExtension( p.getFileName().toString() ), safeMapFileRowRead( p ) ) )
                .map( t -> new MapFile( t.getLeft(), t.getRight() ) )
                .collect( Collectors.toList() );
//        return from file in Directory.EnumerateFiles(mapFileDirectory)
//               where file.EndsWith(".csv")
//               let permtick = Path.GetFileNameWithoutExtension(file)
//               let fileRead = SafeMapFileRowRead(file)
//               select new MapFile(permtick, fileRead);
    }

    /// Reads in the map file at the specified path, returning null if any exceptions are encountered
    private static Stream<MapFileRow> safeMapFileRowRead( Path file ) {
        try {
            return MapFileRow.read( file );
        }
        catch( Exception err ) {
            LOG.error( "File: " + file, err );
            return Collections.<MapFileRow>emptyList().stream();
        }
    }

    /// Returns an enumerator that iterates through the collection.
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    @Override
    public Iterator<MapFileRow> iterator() {
        return _data.values().iterator();
    }
}