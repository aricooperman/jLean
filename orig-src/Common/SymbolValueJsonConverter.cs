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

using System;
using Newtonsoft.Json;

package com.quantconnect.lean
{
    /**
     * Defines a <see cref="JsonConverter"/> to be used when you only want to serialize
     * the <see cref="Symbol.Value"/> property instead of the full <see cref="Symbol"/>
     * instance
    */
    public class SymbolValueJsonConverter : JsonConverter
    {
        /**
         * Writes the JSON representation of the object.
        */
         * @param writer The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.<param name="value The value.<param name="serializer The calling serializer.
        public @Override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            symbol = value as Symbol;
            if( symbol != null ) {
                writer.WriteValue(symbol.Value);
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }

        /**
         * Reads the JSON representation of the object.
        */
         * @param reader The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.<param name="objectType Type of the object.<param name="existingValue The existing value of object being read.<param name="serializer The calling serializer.
        @returns 
         * The object value.
         * 
        public @Override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new UnsupportedOperationException( "The SymbolValueJsonConverter is write-only.");
        }

        /**
         * Determines whether this instance can convert the specified object type.
        */
         * @param objectType Type of the object.
        @returns 
         * <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
         * 
        public @Override boolean CanConvert(Type objectType) {
            throw new UnsupportedOperationException( "The SymbolValueJsonConverter is intended to be decorated on the appropriate member directly.");
        }
    }
}