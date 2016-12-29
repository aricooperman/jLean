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

//using System.Linq.Expressions;
//using CloneExtensions;
//using Fasterflect;

package com.quantconnect.lean.util;

import java.lang.reflect.Constructor;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.Arrays;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.function.Supplier;

import com.quantconnect.lean.SecurityIdentifier;
import com.quantconnect.lean.Symbol;

/**
 * Provides methods for creating new instances of objects
 */
public /*static*/ class ObjectActivator {

    private static final Object[] _emptyObjectArray = new Object[0];
    private static final Map<Class<?>,Method> _cloneMethodsByType = new HashMap<>();
    private static final ConcurrentMap<Class<?>,Supplier<?>> _activatorsByType = new ConcurrentHashMap<>();
    private static final Set<Class<?>> knownImmutableTypes = new HashSet<>();
    private static final Class<?>[] NO_PARAMETER_ARRAY = new Class[0];

    static {
        // we can reuse the symbol instance in the clone since it's immutable
        knownImmutableTypes.add( Symbol.class );
        knownImmutableTypes.add( SecurityIdentifier.class );
    }

    private ObjectActivator() { }

    /**
     * Fast Object Creator from Generic Type:
     * Modified from http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/
     * This assumes that the type has a parameterless, default constructor
     * @param dataType Class of the object we wish to create
     * @returns Method to return an instance of object
     */
    @SuppressWarnings("unchecked")
    public static <T> Supplier<T> getActivator( final Class<T> dataType ) {
        // if we already have it, just use it
        return (Supplier<T>)_activatorsByType.computeIfAbsent( dataType, dt -> {
            try {
                final Constructor<T> ctor = dataType.getConstructor( NO_PARAMETER_ARRAY );

                //User has forgotten to include a parameterless constructor:
                if( ctor == null )
                    return null;

                final Supplier<T> factory = () -> {
                    try {
                        return ctor.newInstance( _emptyObjectArray );
                    }
                    catch( InstantiationException | IllegalAccessException | IllegalArgumentException
                            | InvocationTargetException e ) {
                        throw new RuntimeException( e );
                    }
                };

                return factory;
            }
            catch( NoSuchMethodException | SecurityException e ) {
                throw new RuntimeException( e );
            }
        } );
    }

    /**
     * Clones the specified instance using reflection
     * @param instanceToClone The instance to be cloned
     * @returns A field/property wise, non-recursive clone of the instance
     */
    @SuppressWarnings("unchecked")
    public static <T> T clone( final T instanceToClone ) {
        final Class<? extends Object> type = instanceToClone.getClass();
        final Method method = _cloneMethodsByType.computeIfAbsent( type, t ->
        Arrays.stream( type.getMethods() ).filter( x -> x.getName().equals( "getClone" ) && x.getParameters().length == 1 ).findFirst().orElse( null ) );

        try {
            return (T)method.invoke( null, instanceToClone ); //Static method
        }
        catch( IllegalAccessException | IllegalArgumentException | InvocationTargetException e ) {
            throw new RuntimeException( e );
        }
    }
}
