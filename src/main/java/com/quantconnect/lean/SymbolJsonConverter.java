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

package com.quantconnect.lean;

import java.io.IOException;

import org.apache.commons.lang3.StringUtils;

import com.fasterxml.jackson.core.JsonGenerator;
import com.fasterxml.jackson.core.JsonParser;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.DeserializationContext;
import com.fasterxml.jackson.databind.JsonDeserializer;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.JsonSerializer;
import com.fasterxml.jackson.databind.SerializerProvider;

//using QuantConnect.Securities;

/// Defines a <see cref="JsonConverter"/> to be used when deserializing to 
/// the <see cref="Symbol"/> class.
public class SymbolJsonConverter {
    
    public static class SymbolJsonSerializer extends JsonSerializer<Symbol> {

        /// Writes the JSON representation of the object.
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        @Override
        public void serialize( Symbol symbol, JsonGenerator gen, SerializerProvider serializers )
                throws IOException, JsonProcessingException {
            
            if( symbol == null ) 
                return;
            
            gen.writeStartObject();
//            gen.writeFieldName( "$type" );
//            gen.WriteValue( "QuantConnect.Symbol, QuantConnect.Common" );
            gen.writeFieldName( "value" );
            gen.writeString( symbol.getValue() );
            gen.writeFieldName( "id" );
            gen.writeString( symbol.getId().toString() );
//            gen.writeFieldName( "Permtick" );
//            gen.WriteValue( symbol.Value );
            gen.writeEndObject();
        }
    }

    public static class SymbolJsonDeserializer extends JsonDeserializer<Symbol> {

        /// Reads the JSON representation of the object.
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        @Override
        public Symbol deserialize( JsonParser p, DeserializationContext ctxt )
                throws IOException, JsonProcessingException {
            
            final JsonNode node = p.getCodec().readTree( p );
            final String value = node.get( "value" ).asText();
            final String id = node.get( "id" ).asText();
            if( StringUtils.isBlank( value ) || StringUtils.isBlank( id ) )
                return null;
            
            return SecurityIdentifier.parse( id ).map( sid -> new Symbol( sid, value ) ).orElse( null );
        }
    }
    
    

//    /// Determines whether this instance can convert the specified object type.
//    /// <param name="objectType">Type of the object.</param>
//    /// <returns>
//    /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
//    /// </returns>
//    public override boolean CanConvert(Type objectType)
//    {
//        return objectType == typeof (Symbol);
//    }
}