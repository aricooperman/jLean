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

package com.quantconnect.lean.util;

import java.awt.Color;
import java.io.IOException;

import org.apache.commons.lang3.StringUtils;

import com.fasterxml.jackson.core.JsonParser;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.DeserializationContext;
import com.fasterxml.jackson.databind.JsonDeserializer;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.JsonSerializer;

 * Provides a base class for a <see cref="JsonConverter"/> that serializes a
 * an input type as some other output type
 * <typeparam name="T The type to be serialized</typeparam>
 * <typeparam name="TResult The output serialized type</typeparam>
public abstract class TypeChangeJsonConverter<T, TResult> {

    
    /**
     * Writes the JSON representation of the object.
    */
     * @param writer The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.<param name="value The value.<param name="serializer The calling serializer.
    public @Override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        // Convert the value into TResult to be serialized
        valueToSerialize = Convert((T)value);

        serializer.Serialize(writer, valueToSerialize);
    }

     * Determines whether this instance can convert the specified object type.
     * @param objectType Type of the object.
    @returns 
     * <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
     * 
    public @Override boolean CanConvert(Type objectType) {
        return typeof(T) == objectType;
    }

     * Creates an instance of the un-projected type to be deserialized
     * @param type The input object type, this is the data held in the token
     * @param token The input data to be converted into a T
    @returns A new instance of T that is to be serialized using default rules
    protected T create( Type type, JToken token ) {
        // Default impl reads from the token of the requested type
        return convert( token.Value<TResult>());
    }

     * Convert the input value to a value to be serialzied
     * @param value The input value to be converted before serialziation
    @returns A new instance of TResult that is to be serialzied
    protected abstract TResult convert( T value );

     * Converts the input value to be deserialized
     * @param value The deserialized value that needs to be converted to T
    @returns The converted value
    protected abstract T convert( TResult value );
    
    public abstract class TypeChangeJsonConverterDeserializer extends JsonDeserializer<T>gi  {
        
       * Reads the JSON representation of the object.
         * @param reader The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.<param name="objectType Type of the object.<param name="existingValue The existing value of object being read.<param name="serializer The calling serializer.
        @returns 
         * The object value.
         * 
        @Override
        public Object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            // Load token from stream
            token = JToken.Load(reader);

            // Create target object based on token
            target = create( objectType, token );

            targetType = target.GetType();
            if( targetType.IsClass && targetType != typeof( String)) {
                // Populate the object properties
                serializer.Populate(token.CreateReader(), target);
            }

            return target;
        }

        @Override
        public T deserialize( JsonParser p, DeserializationContext ctxt ) throws IOException, JsonProcessingException {
            final JsonNode node = p.getCodec().readTree( p );
            final String value = node.get( "color" ).asText();
            if( StringUtils.isBlank( value ) )
                return null;
            
            if( value.length() != 7)
                throw new NumberFormatException( String.format( "Unable to convert '%s' to a Color. Requires String length of 7 including the leading hashtag.", value ) );
            
            final int red = hexToInt( value.substring( 1, 2 ) );
            final int green = hexToInt( value.substring( 3, 2 ) );
            final int blue = hexToInt( value.substring( 5, 2 ) );
            
            return new Color( red, green, blue );            return null;
        }

    }
}
