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

import java.io.IOException;
import java.io.Writer;

import com.fasterxml.jackson.core.JsonParseException;
import com.fasterxml.jackson.databind.JsonDeserializer;
import com.fasterxml.jackson.databind.JsonMappingException;
import com.fasterxml.jackson.databind.JsonSerializer;
import com.quantconnect.lean.Global;

/**
 * Provides a base class for a <see cref="JsonConverter"/> that serializes a
 * an input type as some other output type
 * <typeparam name="T The type to be serialized</typeparam>
 * <typeparam name="TResult The output serialized type</typeparam>
 */
public abstract class TypeChangeJsonConverter<T,TResult> {

//    /**
//     * Determines whether this instance can convert the specified object type.
//     * @param objectType Class of the object.
//     * @returns true if this instance can convert the specified object type; otherwise, false.
//     */
//    @Override
//    public boolean canConvert( final Class<T> objectType ) {
//        return typeof(T) == objectType;
//    }

    /**
     * Creates an instance of the un-projected type to be deserialized
     * @param type The input object type, this is the data held in the token
     * @param token The input data to be converted into a T
     * @throws IOException
     * @throws JsonMappingException
     * @throws JsonParseException
     * @returns A new instance of T that is to be serialized using default rules
     */
    protected T create( final Class<? extends TResult> type, final String token ) throws IOException {
        // Default impl reads from the token of the requested type
        return convertFromResult( Global.OBJECT_MAPPER.readValue( token, type ) );
    }

    /**
     * Convert the input value to a value to be serialzied
     * @param value The input value to be converted before serialziation
     * @returns A new instance of TResult that is to be serialzied
     */
    protected abstract TResult convertToResult( T value );

    /**
     * Converts the input value to be deserialized
     * @param value The deserialized value that needs to be converted to T
     * @returns The converted value
     */
    protected abstract T convertFromResult( TResult value );
    
    //TODO
    public abstract class TypeChangeJsonConverterDeserializer extends JsonDeserializer<T>  {

//        /**
//         * Reads the JSON representation final of the object.
//         * @param reader The <final see cref="T:Newtonsoft.Json.JsonReader"/> to read from.<param name="objectType Class of the object.<param name="existingValue The existing value of object being read.<param name="serializer The calling serializer.
//         * @returns The object value.
//         */
//        @Override
//        public T deserialize( final JsonParser p, final DeserializationContext ctxt ) throws IOException, JsonProcessingException {
//            // Load token from stream
//            token = JToken.Load(reader);
//
//            // Create target object based on token
//            target = create( objectType, token );
//
//            targetType = target.GetType();
//            if( targetType.IsClass && targetType != typeof( String)) {
//                // Populate the object properties
//                serializer.Populate(token.CreateReader(), target);
//            }
//
//            return target;
//        }

//        @Override
//        public T deserialize( final JsonParser p, final DeserializationContext ctxt ) throws IOException, JsonProcessingException {
//            final JsonNode node = p.getCodec().readTree( p );
//            final String value = node.get( "color" ).asText();
//            if( StringUtils.isBlank( value ) )
//                return null;
//
//            if( value.length() != 7)
//                throw new NumberFormatException( String.format( "Unable to convert '%s' to a Color. Requires String length of 7 including the leading hashtag.", value ) );
//
//            final int red = hexToInt( value.substring( 1, 2 ) );
//            final int green = hexToInt( value.substring( 3, 2 ) );
//            final int blue = hexToInt( value.substring( 5, 2 ) );
//
//            return new Color( red, green, blue );            return null;
//        }
    }
    
    public abstract class TypeChangeJsonConverterSerializer extends JsonSerializer<T>  {
    
        /**
         * Writes the JSON representation of the object.
         * @param writer The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.<param name="value The value.<param name="serializer The calling serializer.
         * @throws IOException
         * @throws JsonMappingException
         * @throws
         */
        public void writeJson( final Writer writer, final T value ) throws IOException {
            // Convert the value into TResult to be serialized
            final TResult valueToSerialize = convertToResult( value );
            Global.OBJECT_MAPPER.writeValue( writer, valueToSerialize );
        }
    }
}
