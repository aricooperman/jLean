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
using System.Linq;
using System.Xml.Linq;

package com.quantconnect.lean.Util
{
    /**
     * Provides extension methods for the XML to LINQ types
    */
    public static class XElementExtensions
    {
        /**
         * Gets the value from the element and converts it to the specified type.
        */
         * <typeparam name="T The output type</typeparam>
         * @param element The element to access
         * @param name The attribute name to access on the element
        @returns The converted value
        public static T Get<T>(this XElement element, String name) 
            where T : IConvertible
        {
            xAttribute = element.Descendants(name).Single();
            String value = xAttribute.Value;
            return value.ConvertTo<T>();
        }
    }
}
