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

package com.quantconnect.lean.securities;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.math.BigDecimal;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;
import java.util.stream.Collectors;

import org.apache.commons.lang3.StringUtils;
import org.apache.commons.lang3.tuple.ImmutablePair;

import com.google.common.collect.ImmutableMap;
import com.quantconnect.lean.Globals;
import com.quantconnect.lean.SecurityType;

/**
 * Provides access to specific properties for various symbols
 */
public class SymbolPropertiesDatabase {
    
    private static final Object DATA_FOLDER_SYMBOL_PROPS_DB_LOCK = new Object();
    
    private static SymbolPropertiesDatabase dataFolderSymbolPropertiesDatabase;

    private final ImmutableMap<SecurityDatabaseKey,SymbolProperties> entries;

    private SymbolPropertiesDatabase( Map<SecurityDatabaseKey,SymbolProperties> entries ) {
        this.entries = ImmutableMap.copyOf( entries );
    }

    /**
     * Gets the symbol properties for the specified market/symbol/security-type
     * @param market The market the exchange resides in, i.e, 'usa', 'fxcm', ect...
     * @param symbol The particular symbol being traded
     * @param securityType The security type of the symbol
     * @param defaultQuoteCurrency Specifies the quote currency to be used when returning a default instance of an entry is not found in the database
     * @returns The symbol properties matching the specified market/symbol/security-type or null if not found
     */
    public SymbolProperties getSymbolProperties( String market, String symbol, SecurityType securityType, String defaultQuoteCurrency ) {
        final SecurityDatabaseKey key = new SecurityDatabaseKey( market, symbol, securityType );

        SymbolProperties symbolProperties = entries.get( key );
        if( symbolProperties == null ) {
            // now check with null symbol key
            
            symbolProperties = entries.get( new SecurityDatabaseKey( market, null, securityType ) );
            if( symbolProperties == null ) {
                // no properties found, return object with default property values
                return SymbolProperties.getDefault( defaultQuoteCurrency );
            }
        }

        return symbolProperties;
    }

    /**
     * Gets the instance of the <see cref="SymbolPropertiesDatabase"/> class produced by reading in the symbol properties
     * data found in /Data/symbol-properties/
     * @throws IOException 
     * @returns A <see cref="SymbolPropertiesDatabase"/> class that represents the data in the symbol-properties folder
     */
    public static SymbolPropertiesDatabase fromDataFolder() throws IOException {
        synchronized( DATA_FOLDER_SYMBOL_PROPS_DB_LOCK ) {
            if( dataFolderSymbolPropertiesDatabase == null ) {
                final Path directory = Paths.get( Globals.getDataFolder(), "symbol-properties" );
                dataFolderSymbolPropertiesDatabase = fromCsvFile( directory.resolve( "symbol-properties-database.csv" ) );
            }
        }
        return dataFolderSymbolPropertiesDatabase;
    }

    /**
     * Creates a new instance of the <see cref="SymbolPropertiesDatabase"/> class by reading the specified csv file
     * @param path  The csv file to be read
     * @throws IOException 
     * @returns A new instance of the <see cref="SymbolPropertiesDatabase"/> class representing the data in the specified file
     */
    private static SymbolPropertiesDatabase fromCsvFile( Path file ) throws IOException {
        final Map<SecurityDatabaseKey,SymbolProperties> entries = new HashMap<SecurityDatabaseKey,SymbolProperties>();

        if( !Files.exists( file ) )
            throw new FileNotFoundException( "Unable to locate symbol properties file: " + file );

        // skip the first header line, also skip #'s as these are comment lines
        for( String line : Files.lines( file ).filter( x -> !x.startsWith( "#" ) && !StringUtils.isBlank( x ) ).skip( 1 ).collect( Collectors.toList() ) ) {
            final Entry<SecurityDatabaseKey,SymbolProperties> entry = fromCsvLine( line );
            final SecurityDatabaseKey key = entry.getKey();
            final SymbolProperties value = entry.getValue();
            if( entries.containsKey( key ) )
                throw new IOException( "Encountered duplicate key while processing file: " + file + ". Key: " + key );

            entries.put( key, value );
        }

        return new SymbolPropertiesDatabase( entries );
    }

    /**
     * Creates a new instance of <see cref="SymbolProperties"/> from the specified csv line
     * @param line The csv line to be parsed
     * @param key The key used to uniquely identify this security
     * @returns A new <see cref="SymbolProperties"/> for the specified csv line
     */
    private static Entry<SecurityDatabaseKey,SymbolProperties> fromCsvLine( String line ) {
        final String[] csv = line.split( "," );

        return ImmutablePair.of( 
                new SecurityDatabaseKey( csv[0], csv[1], SecurityType.valueOfIgnoreCase( csv[2] ) ),
                new SymbolProperties( csv[3], csv[4], new BigDecimal( csv[5] ), new BigDecimal( csv[6] ), new BigDecimal( csv[7] ) ) );
    }
}
