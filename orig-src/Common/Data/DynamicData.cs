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
    /// <summary>
    /// Dynamic Data Class: Accept flexible data, adapting to the columns provided by source.
    /// </summary>
    /// <remarks>Intended for use with Quandl class.</remarks>
    public abstract class DynamicData : BaseData, IDynamicMetaObjectProvider
    {
        private readonly Map<String, object> _storage = new Map<String, object>();

        /// <summary>
        /// Get the metaObject required for Dynamism.
        /// </summary>
        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicDataMetaObject(parameter, this);
        }

        /// <summary>
        /// Sets the property with the specified name to the value. This is a case-insensitve search.
        /// </summary>
        /// <param name="name">The property name to set</param>
        /// <param name="value">The new property value</param>
        /// <returns>Returns the input value back to the caller</returns>
        public object SetProperty( String name, object value)
        {
            name = name.toLowerCase();

            if (name == "time")
            {
                Time = (DateTime)value;
            }
            if (name == "value")
            {
                Value = (decimal)value;
            }
            if (name == "symbol")
            {
                if (value is string)
                {
                    Symbol = SymbolCache.GetSymbol(( String) value);
                }
                else
                {
                    Symbol = (Symbol) value;
                }
            }
            // reaodnly
            //if (name == "Price")
            //{
            //    return Price = (decimal) value;
            //}
            _storage[name] = value;
            return value;
        }

        /// <summary>
        /// Gets the property's value with the specified name. This is a case-insensitve search.
        /// </summary>
        /// <param name="name">The property name to access</param>
        /// <returns>object value of BaseData</returns>
        public object GetProperty( String name)
        {
            name = name.toLowerCase();

            // redirect these calls to the base types properties
            if (name == "time")
            {
                return Time;
            }
            if (name == "value")
            {
                return Value;
            }
            if (name == "symbol")
            {
                return Symbol;
            }
            if (name == "price")
            {
                return Price;
            }

            object value;
            if (!_storage.TryGetValue(name, out value))
            {
                // let the user know the property name that we couldn't find
                throw new Exception("Property with name '" + name + "' does not exist. Properties: Time, Symbol, Value " + string.Join(", ", _storage.Keys));
            }

            return value;
        }

        /// <summary>
        /// Gets whether or not this dynamic data instance has a property with the specified name.
        /// This is a case-insensitve search.
        /// </summary>
        /// <param name="name">The property name to check for</param>
        /// <returns>True if the property exists, false otherwise</returns>
        public boolean HasProperty( String name)
        {
            return _storage.ContainsKey(name.toLowerCase());
        }

        /// <summary>
        /// Return a new instance clone of this object, used in fill forward
        /// </summary>
        /// <remarks>
        /// This base implementation uses reflection to copy all public fields and properties
        /// </remarks>
        /// <returns>A clone of the current object</returns>
        public override BaseData Clone()
        {
            clone = ObjectActivator.Clone(this);
            foreach (kvp in _storage)
            {
                // don't forget to add the dynamic members!
                clone._storage.Add(kvp);
            }
            return clone;
        }

        /// <summary>
        /// Custom implementation of Dynamic Data MetaObject
        /// </summary>
        private class DynamicDataMetaObject : DynamicMetaObject
        {
            private static readonly MethodInfo SetPropertyMethodInfo = typeof(DynamicData).GetMethod("SetProperty");
            private static readonly MethodInfo GetPropertyMethodInfo = typeof(DynamicData).GetMethod("GetProperty");

            public DynamicDataMetaObject(Expression expression, DynamicData instance)
                : base(expression, BindingRestrictions.Empty, instance)
            {
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
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

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
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
