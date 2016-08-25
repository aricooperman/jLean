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
import java.nio.file.Files;
import java.nio.file.Path;
import java.time.LocalDate;
import java.util.Collections;
import java.util.stream.Stream;

import org.apache.commons.lang3.StringUtils;

//using System.IO;
//using System.Linq;

 * Represents a single row in a map_file. This is a csv file ordered as {date, mapped symbol}
public class MapFileRow {
    
     * Gets the date associated with this data
    private LocalDate date;

     * Gets the mapped symbol
    private String mappedSymbol;

     * Initializes a new instance of the <see cref="MapFileRow"/> class.
    public MapFileRow( LocalDate date, String mappedSymbol ) {
        this.date = date;
        this.mappedSymbol = mappedSymbol.toUpperCase();
    }

    public LocalDate getDate() {
        return date;
    }

    public String getMappedSymbol() {
        return mappedSymbol;
    }

     * Reads in the map_file for the specified equity symbol
    public static Stream<MapFileRow> read( String permtick, String market ) throws IOException {
        final Path path = MapFile.getMapFilePath( permtick, market );
        return Files.exists( path ) ? read( path ) : Collections.<MapFileRow>emptyList().stream();
    }

     * Reads in the map_file at the specified path
    public static Stream<MapFileRow> read( Path path ) throws IOException {
        return Files.lines( path ).filter( StringUtils::isNotBlank ).map( MapFileRow::parse );
    }

     * Parses the specified line into a MapFileRow
    public static MapFileRow parse( String line ) {
        final String[] csv = line.split( "," );
        return new MapFileRow( LocalDate.parse( csv[0] ), csv[1] ); //DateFormat.EightCharacter
    }

     * Indicates whether the current object is equal to another object of the same type.
    @returns 
     * true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
     * 
     * @param other An object to compare with this object.
    public boolean equals( MapFileRow other ) {
        if( null == other ) return false;
        if( this == other ) return true;
        
        return this.date.equals( other.date ) && mappedSymbol.equals( other.mappedSymbol );
    }

     * Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
    @returns 
     * true if the specified object  is equal to the current object; otherwise, false.
     * 
     * @param obj The object to compare with the current object. <filterpriority>2</filterpriority>
    public boolean equals( Object obj ) {
        if( null == obj ) return false;
        if( this == obj ) return true;
        if( !(obj instanceof MapFileRow) ) return false;
        return equals( (MapFileRow)obj );
    }

     * Serves as a hash function for a particular type. 
    @returns 
     * A hash code for the current <see cref="T:System.Object"/>.
     * 
     * <filterpriority>2</filterpriority>
    public int hashCode() {
        return (date.hashCode() * 397) ^ (mappedSymbol != null ? mappedSymbol.hashCode() : 0);
    }

//    /**
//     * Determines whether or not the two instances are equal
//    */
//    public static boolean operator ==(MapFileRow left, MapFileRow right)
//    {
//        return Equals(left, right);
//    }
//
//    /**
//     * Determines whether or not the two instances are not equal
//    */
//    public static boolean operator !=(MapFileRow left, MapFileRow right)
//    {
//        return !Equals(left, right);
//    }

    public String toString() {
        return date.toString() + ": " + mappedSymbol;
    }
}
