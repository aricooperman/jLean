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
using System.Linq;
using System.Linq.Expressions;
using CloneExtensions;
using Fasterflect;
using QuantConnect.Securities;

package com.quantconnect.lean.Util
{
    /**
    /// Provides methods for creating new instances of objects
    */
    public static class ObjectActivator
    {
        private static final object _lock = new object();
        private static final object[] _emptyObjectArray = new object[0];
        private static final Map<Type, MethodInvoker> _cloneMethodsByType = new Map<Type, MethodInvoker>();
        private static final Map<Type, Func<object[], object>> _activatorsByType = new Map<Type, Func<object[], object>>();

        static ObjectActivator() {
            // we can reuse the symbol instance in the clone since it's immutable
            ((HashSet<Type>) CloneFactory.KnownImmutableTypes).Add(typeof (Symbol));
            ((HashSet<Type>) CloneFactory.KnownImmutableTypes).Add(typeof (SecurityIdentifier));
        }

        /**
        /// Fast Object Creator from Generic Type:
        /// Modified from http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/
        */
        /// This assumes that the type has a parameterless, default constructor
         * @param dataType">Type of the object we wish to create
        @returns Method to return an instance of object
        public static Func<object[], object> GetActivator(Type dataType) {
            lock (_lock) {
                // if we already have it, just use it
                Func<object[], object> factory;
                if( _activatorsByType.TryGetValue(dataType, out factory)) {
                    return factory;
                }

                ctor = dataType.GetConstructor(new Type[] {});

                //User has forgotten to include a parameterless constructor:
                if( ctor == null ) return null;

                paramsInfo = ctor.GetParameters();

                //create a single param of type object[]
                param = Expression.Parameter(typeof (object[]), "args");
                argsExp = new Expression[paramsInfo.Length];

                for (i = 0; i < paramsInfo.Length; i++) {
                    index = Expression.Constant(i);
                    paramType = paramsInfo[i].ParameterType;
                    paramAccessorExp = Expression.ArrayIndex(param, index);
                    paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                    argsExp[i] = paramCastExp;
                }

                newExp = Expression.New(ctor, argsExp);
                lambda = Expression.Lambda(typeof (Func<object[], object>), newExp, param);
                factory = (Func<object[], object>) lambda.Compile();

                // save it for later
                _activatorsByType.Add(dataType, factory);

                return factory;
            }
        }

        /**
        /// Clones the specified instance using reflection
        */
         * @param instanceToClone">The instance to be cloned
        @returns A field/property wise, non-recursive clone of the instance
        public static object Clone(object instanceToClone) {
            type = instanceToClone.GetType();
            MethodInvoker func;
            if( _cloneMethodsByType.TryGetValue(type, out func)) {
                return func(null, instanceToClone);
            }

            // public static T GetClone<T>(this T source, CloningFlags flags)
            method = typeof (CloneFactory).GetMethods().FirstOrDefault(x -> x.Name == "GetClone" && x.GetParameters().Length == 1);
            method = method.MakeGenericMethod(type);
            func = method.DelegateForCallMethod();
            _cloneMethodsByType[type] = func;
            return func(null, instanceToClone);
        }

        /**
        /// Clones the specified instance and then casts it to T before returning
        */
        public static T Clone<T>(T instanceToClone) where T : class
        {
            clone = Clone((object)instanceToClone) as T;
            if( clone == null ) {
                throw new Exception( "Unable to clone instance of type " + instanceToClone.GetType().Name + " to " + typeof(T).Name);
            }
            return clone;
        }
    }
}
