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

//using System.ComponentModel.Composition;
//using System.ComponentModel.Composition.Hosting;
//using System.ComponentModel.Composition.Primitives;
//using System.ComponentModel.Composition.ReflectionModel;
//using System.Linq;

package com.quantconnect.lean.util;

import java.util.Collection;
import java.util.Objects;
import java.util.function.Predicate;
import java.util.stream.Stream;
import java.util.stream.StreamSupport;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.common.collect.ArrayListMultimap;
import com.google.common.collect.Multimap;

/**
 * Provides methods for obtaining exported MEF instances
*/
public class Composer {
//    private static final String PluginDirectory = Config.get( "plugin-directory" );

    /**
     * Gets the singleton instance
    */
    public static final Composer INSTANCE = new Composer();

    /**
     * Initializes a new instance of the <see cref="Composer"/> class. This type
     * is a light wrapper on top of an MEF <see cref="CompositionContainer"/>
    */
    public Composer() {
        reset();
    }

//    private CompositionContainer _compositionContainer;
    private final Object exportedValuesLockObject = new Object();
    private final Multimap<Class<?>,Object> exportedValues = ArrayListMultimap.create();
    private final Logger log = LoggerFactory.getLogger( getClass() );

    /**
     * Gets the export matching the predicate
     * @param predicate Function used to pick which imported instance to return, if null the first instance is returned
     * @returns The only export matching the specified predicate
     */
    public <T> T single( final Class<? extends T> type, final Predicate<T> predicate ) {
        Objects.requireNonNull( predicate );
        return getExportedValues( type ).filter( predicate ).findFirst().orElse( null );
    }

    /**
     * Adds the specified instance to this instance to allow it to be recalled via GetExportedValueByTypeName
     * <typeparam name="T The contract type</typeparam>
     * @param instance The instance to add
     */
    public <T> void addPart( final T instance ) {
        synchronized(exportedValuesLockObject) {
            exportedValues.put( instance.getClass(), instance );
        }
    }

    /**
     * Extension method to searches the composition container for an export that has a matching type name. This function
     * will first try to match on Type.AssemblyQualifiedName, then Type.FullName, and finally on Type.Name
     * 
     * This method will not throw if multiple types are found matching the name, it will just return the first one it finds.
     * <typeparam name="T The type of the export</typeparam>
     * @param typeName The name of the type to find. This can be an assembly qualified name, a full name, or just the type's name
     * @returns The export instance
     */
    public <T> T getExportedValueByTypeName( final String typeName ) {
        try {
            synchronized( exportedValuesLockObject ) {
                T instance;
                @SuppressWarnings("unchecked")
                final Class<T> type = (Class<T>)Class.forName( typeName );
                @SuppressWarnings("unchecked")
                final Iterable<T> values = (Iterable<T>)exportedValues.get( type );
                if( values != null ) {
                    // if we've already loaded this part, then just return the same one
                    instance = StreamSupport.stream( values.spliterator(), false ).filter( x -> type.isAssignableFrom( x.getClass() ) ).findFirst().orElse( null );
                    if( instance != null )
                        return instance;
                }

//                // we want to get the requested part without instantiating each one of that type
//                selectedPart = _compositionContainer.Catalog.Parts
//                    .Select(x -> new { part = x, Class = ReflectionModelServices.GetPartType(x).Value })
//                    .Where(x -> type.IsAssignableFrom(x.Type))
//                    .Where(x -> x.Type.MatchesTypeName(typeName))
//                    .Select(x -> x.part)
//                    .FirstOrDefault();
//
//                if( selectedPart == null ) {
//                    throw new IllegalArgumentException(
//                        "Unable to locate any exports matching the requested typeName: " + typeName, "typeName");
//                }
//
//                exportDefinition =
//                    selectedPart.ExportDefinitions.First(
//                        x -> x.ContractName == AttributedModelServices.GetContractName(type));
//                instance = (T)selectedPart.CreatePart().GetExportedValue(exportDefinition);
                instance = type.newInstance();
                //TODO https://github.com/ronmamo/reflections

                // cache the new value for next time
                exportedValues.put( type, instance );

                return instance;
            }
        }
        catch( final Throwable err ) {
            log.error( err.getMessage(), err );
            throw new RuntimeException( err );
        }
    }
    
    /**
     * Gets all exports of type T
     */
    public <T> Stream<T> getExportedValues( final Class<? extends T> type ) {
        synchronized( exportedValuesLockObject ) {
            @SuppressWarnings("unchecked")
            final Collection<T> values = (Collection<T>)exportedValues.get( type );
//            if( values != null )
            return values.stream();

            //TODO class scanning for subclass implementations
//            type.
//            values = _compositionContainer.<T>getExportedValues().ToList();
//            _exportedValues.putAll( type, values );
//            return values();
        }
    }

    /**
     * Clears the cache of exported values, causing new instances to be created.
     */
    public void reset() {
        synchronized( exportedValuesLockObject ) {
//            _compositionContainer = new CompositionContainer(aggregate);
            exportedValues.clear();
        }
    }
}
