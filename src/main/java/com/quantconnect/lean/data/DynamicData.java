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


package com.quantconnect.lean.data;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.HashMap;
import java.util.Map;
import java.util.stream.Collectors;

import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.SymbolCache;
import com.quantconnect.lean.util.ObjectActivator;

/**
 * Dynamic Data Class: Accept flexible data, adapting to the columns provided by source.
 * Intended for use with Quandl class.
 */
public abstract class DynamicData extends BaseData implements Cloneable/*, IDynamicMetaObjectProvider*/  {

    private final Map<String,Object> _storage = new HashMap<>();

    //    /**
    //     * Get the metaObject required for Dynamism.
    //     */
    //    public DynamicMetaObject getMetaObject( Expression parameter ) {
    //        return new DynamicDataMetaObject( parameter, this );
    //    }

    /**
     * Sets the property with the specified name to the value. This is a case-insensitve search.
     * @param name The property name to set
     * @param value The new property value
     * @returns Returns the input value back to the caller
     */
    public Object setProperty( String name, final Object value ) {
        name = name.toLowerCase();

        if( name.equals( "time" ) )
            setTime( (LocalDateTime)value );
        else if( name.equals( "value" ) )
            setValue( (BigDecimal)value );
        else if( name.equals( "symbol" ) ) {
            if( value instanceof String )
                setSymbol( SymbolCache.getSymbol( (String)value ) );
            else
                setSymbol( (Symbol)value );
        }
        // reaodnly
        //if( name.equals( "Price")
        //{
        //    return Price = (decimal) value;
        //}
        _storage.put( name, value );

        return value;
    }

    /**
     * Gets the property's value with the specified name. This is a case-insensitve search.
     * @param name The property name to access
     * @returns object value of BaseData
     */
    public Object getProperty( String name ) {
        name = name.toLowerCase();

        // redirect these calls to the base types properties
        if( name.equals( "time" ) )
            return getTime();

        if( name.equals( "value" ) )
            return getValue();

        if( name.equals( "symbol" ) )
            return getSymbol();

        if( name.equals( "price" ) )
            return getPrice();


        final Object value = _storage.get( name );
        if( value == null ) {
            // let the user know the property name that we couldn't find
            throw new RuntimeException( "Property with name '" + name + "' does not exist. Properties: Time, Symbol, Value " + _storage.keySet().stream().collect( Collectors.joining( ", " ) ) );
        }

        return value;
    }

    /**
     * Gets whether or not this dynamic data instance has a property with the specified name.
     * This is a case-insensitve search.
     * @param name The property name to check for
     * @returns True if the property exists, false otherwise
     */
    public boolean hasProperty( final String name ) {
        return _storage.containsKey( name.toLowerCase() );
    }

    /**
     * Return a new instance clone of this object, used in fill forward
     * 
     * This base implementation uses reflection to copy all public fields and properties
     * 
     * @returns A clone of the current object
     */
    @Override
    public BaseData clone() {
        final DynamicData clone = ObjectActivator.clone( this );
        // don't forget to add the dynamic members!
        clone._storage.putAll( _storage );
        return clone;
    }

    //    /**
    //     * Custom implementation of Dynamic Data MetaObject
    //     */
    //    private class DynamicDataMetaObject extends DynamicMetaObject {
    //        private static final Method SetPropertyMethodInfo = DynamicData.class.getMethod( "setProperty", String.class, Object.class );
    //        private static final Method GetPropertyMethodInfo = DynamicData.class.getMethod( "getProperty", String.class );
    //
    //        //        public DynamicDataMetaObject( final Expression expression, final DynamicData instance ) {
    //        //            super( expression, BindingRestrictions.Empty, instance );
    //        //        }
    //
    //        //        @Override
    //        public DynamicMetaObject BindSetMember( final SetMemberBinder binder, final DynamicMetaObject value) {
    //            // we need to build up an expression tree that represents accessing our instance
    //            restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
    //
    //            args = new Expression[]
    //                    {
    //                            // this is the name of the property to set
    //                            Expression.Constant(binder.Name),
    //
    //                            // this is the value
    //                            Expression.Convert(value.Expression, typeof (object))
    //                    };
    //
    //            // set the 'this' reference
    //            self = Expression.Convert(Expression, LimitType);
    //
    //            call = Expression.Call(self, SetPropertyMethodInfo, args);
    //
    //            return new DynamicMetaObject(call, restrictions);
    //        }
    //
    //        //        @Override
    //        public DynamicMetaObject BindGetMember(final GetMemberBinder binder) {
    //            // we need to build up an expression tree that represents accessing our instance
    //            restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
    //
    //            // arguments for 'call'
    //            args = new Expression[]
    //                    {
    //                            // this is the name of the property to set
    //                            Expression.Constant(binder.Name)
    //                    };
    //
    //            // set the 'this' reference
    //            self = Expression.Convert(Expression, LimitType);
    //
    //            call = Expression.Call(self, GetPropertyMethodInfo, args);
    //
    //            return new DynamicMetaObject(call, restrictions);
    //        }
    //    }
}
