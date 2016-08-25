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
using System.Reflection;
using QuantConnect.Logging;
using QuantConnect.Packets;

package com.quantconnect.lean.Parameters
{
    /**
     * Specifies a field or property is a parameter that can be set
     * from an <see cref="AlgorithmNodePacket.Parameters"/> dictionary
    */
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ParameterAttribute : Attribute
    {
        /**
         * Specifies the binding flags used by this implementation to resolve parameter attributes
        */
        public static final BindingFlags BindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance;

        private static final String ParameterAttributeNameProperty = "Name";
        private static final String ParameterAttributeFullName = typeof (ParameterAttribute).FullName;
        
        /**
         * Gets the name of this parameter
        */
        public String Name { get; private set; }

        /**
         * Initializes a new instance of the <see cref="ParameterAttribute"/> class
        */
         * @param name The name of the parameter. If null is specified
         * then the field or property name will be used
        public ParameterAttribute( String name = null ) {
            Name = name;
        }

        /**
         * Uses reflections to inspect the instance for any parameter attributes.
         * If a value is found in the parameters dictionary, it is set.
        */
         * @param parameters The parameters Map
         * @param instance The instance to set parameters on
        public static void ApplyAttributes(Map<String,String> parameters, object instance) {
            if( instance == null ) throw new ArgumentNullException( "instance");

            type = instance.GetType();

            // get all fields/properties on the instance
            members = type.GetFields(BindingFlags).Concat<MemberInfo>(type.GetProperties(BindingFlags));
            foreach (memberInfo in members) {
                fieldInfo = memberInfo as FieldInfo;
                propertyInfo = memberInfo as PropertyInfo;

                // this line make static analysis a little happier, but should never actually throw
                if( fieldInfo == null && propertyInfo == null ) {
                    throw new Exception( "Resolved member that is neither FieldInfo or PropertyInfo");
                }

                // check the member for our custom attribute
                attribute = memberInfo.GetCustomAttribute<ParameterAttribute>();
                if( attribute == null ) continue;

                // if no name is specified in the attribute then use the member name
                parameterName = attribute.Name ?? memberInfo.Name;

                // get the parameter String value to apply to the member
                String parameterValue;
                if( !parameters.TryGetValue(parameterName, out parameterValue)) continue;

                // if it's a read-only property with a parameter value we can't really do anything, bail
                if( propertyInfo != null && !propertyInfo.CanWrite) {
                    message = String.format( "The specified property is read only: %1$s.%2$s", propertyInfo.DeclaringType, propertyInfo.Name);
                    throw new Exception(message);
                }

                // resolve the member type
                memberType = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;

                // convert the parameter String value to the member type
                value = parameterValue Extensions.convertTo(  )memberType);

                // set the value to the field/property
                if( fieldInfo != null ) {
                    fieldInfo.SetValue(instance, value);
                }
                else
                {
                    propertyInfo.SetValue(instance, value);
                }
            }
        }

        /**
         * Resolves all parameter attributes from the specified compiled assembly path
        */
         * @param assembly The assembly to inspect
        @returns Parameters dictionary keyed by parameter name with a value of the member type
        public static Map<String,String> GetParametersFromAssembly(Assembly assembly) {
            parameters = new Map<String,String>();
            foreach (type in assembly.GetTypes()) {
                Log.Debug( "ParameterAttribute.GetParametersFromAssembly(): Checking type " + type.Name);
                foreach (field in type.GetFields(BindingFlags)) {
                    attribute = field.GetCustomAttribute<ParameterAttribute>();
                    if( attribute != null ) {
                        parameterName = attribute.Name ?? field.Name;
                        parameters[parameterName] = field.FieldType.GetBetterTypeName();
                    }
                }
                foreach (property in type.GetProperties(BindingFlags)) {
                    // ignore non-writeable properties
                    if( !property.CanWrite) continue;
                    attribute = property.GetCustomAttribute<ParameterAttribute>();
                    if( attribute != null ) {
                        parameterName = attribute.Name ?? property.Name;
                        parameters[parameterName] = property.PropertyType.Name;
                    }
                }
            }
            return parameters;
        }
    }
}
