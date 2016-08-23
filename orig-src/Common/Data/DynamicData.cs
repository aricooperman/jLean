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
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using QuantConnect.Util;

package com.quantconnect.lean.Data
{
    /**
    /// Dynamic Data Class: Accept flexible data, adapting to the columns provided by source.
    */
    /// Intended for use with Quandl class.
    public abstract class DynamicData : BaseData, IDynamicMetaObjectProvider
    {
        private final Map<String, object> _storage = new Map<String, object>();

        /**
        /// Get the metaObject required for Dynamism.
        */
        public DynamicMetaObject GetMetaObject(Expression parameter) {
            return new DynamicDataMetaObject(parameter, this);
        }

        /**
        /// Sets the property with the specified name to the value. This is a case-insensitve search.
        */
         * @param name">The property name to set
         * @param value">The new property value
        @returns Returns the input value back to the caller
        public object SetProperty( String name, object value) {
            name = name.toLowerCase();

            if( name == "time") {
                Time = (DateTime)value;
            }
            if( name == "value") {
                Value = (decimal)value;
            }
            if( name == "symbol") {
                if( value is string) {
                    Symbol = SymbolCache.GetSymbol(( String) value);
                }
                else
                {
                    Symbol = (Symbol) value;
                }
            }
            // reaodnly
            //if( name == "Price")
            //{
            //    return Price = (decimal) value;
            //}
            _storage[name] = value;
            return value;
        }

        /**
        /// Gets the property's value with the specified name. This is a case-insensitve search.
        */
         * @param name">The property name to access
        @returns object value of BaseData
        public object GetProperty( String name) {
            name = name.toLowerCase();

            // redirect these calls to the base types properties
            if( name == "time") {
                return Time;
            }
            if( name == "value") {
                return Value;
            }
            if( name == "symbol") {
                return Symbol;
            }
            if( name == "price") {
                return Price;
            }

            object value;
            if( !_storage.TryGetValue(name, out value)) {
                // let the user know the property name that we couldn't find
                throw new Exception( "Property with name '" + name + "' does not exist. Properties: Time, Symbol, Value " + String.join( ", ", _storage.Keys));
            }

            return value;
        }

        /**
        /// Gets whether or not this dynamic data instance has a property with the specified name.
        /// This is a case-insensitve search.
        */
         * @param name">The property name to check for
        @returns True if the property exists, false otherwise
        public boolean HasProperty( String name) {
            return _storage.ContainsKey(name.toLowerCase());
        }

        /**
        /// Return a new instance clone of this object, used in fill forward
        */
        /// 
        /// This base implementation uses reflection to copy all public fields and properties
        /// 
        @returns A clone of the current object
        public @Override BaseData Clone() {
            clone = ObjectActivator.Clone(this);
            foreach (kvp in _storage) {
                // don't forget to add the dynamic members!
                clone._storage.Add(kvp);
            }
            return clone;
        }

        /**
        /// Custom implementation of Dynamic Data MetaObject
        */
        private class DynamicDataMetaObject : DynamicMetaObject
        {
            private static final MethodInfo SetPropertyMethodInfo = typeof(DynamicData).GetMethod( "SetProperty");
            private static final MethodInfo GetPropertyMethodInfo = typeof(DynamicData).GetMethod( "GetProperty");

            public DynamicDataMetaObject(Expression expression, DynamicData instance)
                : base(expression, BindingRestrictions.Empty, instance) {
            }

            public @Override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
                // we need to build up an expression tree that represents accessing our instance
                restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

                args = new Expression[]
                {
                    // this is the name of the property to set
                    Expression.Constant(binder.Name),

                    // this is the value
                    Expression.Convert(value.Expression, typeof (object))
                };

                // set the 'this' reference
                self = Expression.Convert(Expression, LimitType);

                call = Expression.Call(self, SetPropertyMethodInfo, args);

                return new DynamicMetaObject(call, restrictions);
            }

            public @Override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                // we need to build up an expression tree that represents accessing our instance
                restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

                // arguments for 'call'
                args = new Expression[]
                {
                    // this is the name of the property to set
                    Expression.Constant(binder.Name)
                };

                // set the 'this' reference
                self = Expression.Convert(Expression, LimitType);

                call = Expression.Call(self, GetPropertyMethodInfo, args);

                return new DynamicMetaObject(call, restrictions);
            }
        }
    }
}
