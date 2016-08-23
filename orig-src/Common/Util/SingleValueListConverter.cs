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
using Newtonsoft.Json;

package com.quantconnect.lean.Util
{
    /**
    /// Reads json and always produces a List, even if the input has just an object
    */
    public class SingleValueListConverter<T> : JsonConverter
    {
        /**
        /// Writes the JSON representation of the object. If the instance is not a list then it will
        /// be wrapped in a list
        */
         * @param writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.
         * @param value">The value.
         * @param serializer">The calling serializer.
        public @Override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if( value is T) {
                value = new List<T> {(T)value};
            }
            serializer.Serialize(writer, value);
        }

        /**
        /// Reads the JSON representation of the object. If the JSON represents a singular instance, it will be returned
        /// in a list.
        */
         * @param reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.
         * @param objectType">Type of the object.
         * @param existingValue">The existing value of object being read.
         * @param serializer">The calling serializer.
        @returns The object value
        public @Override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            switch (reader.TokenType) {
                case JsonToken.StartObject:
                    return new List<T> {serializer.Deserialize<T>(reader)};
                case JsonToken.StartArray:
                    return serializer.Deserialize<List<T>>(reader);
                default:
                    throw new ArgumentException( "The JsonReader is expected to point at a JsonToken.StartObject or JsonToken.StartArray.");
            }
        }

        /**
        /// Determines whether this instance can convert the specified object type.
        */
         * @param objectType">Type of the object.
        @returns <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        public @Override boolean CanConvert(Type objectType) {
            return objectType == typeof(T) || objectType == typeof(List<T>);
        }
    }
}