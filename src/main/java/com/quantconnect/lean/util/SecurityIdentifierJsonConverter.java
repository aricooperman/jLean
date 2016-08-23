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

import org.apache.commons.lang3.StringUtils;

import com.fasterxml.jackson.core.JsonGenerator;
import com.fasterxml.jackson.core.JsonParser;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.DeserializationContext;
import com.fasterxml.jackson.databind.JsonDeserializer;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.JsonSerializer;
import com.fasterxml.jackson.databind.SerializerProvider;
import com.quantconnect.lean.SecurityIdentifier;

/// A <see cref="JsonConverter"/> implementation that serializes a <see cref="SecurityIdentifier"/> as a string
public class SecurityIdentifierJsonConverter {
    
    public static class SecurityIdentifierJsonSerializer extends JsonSerializer<SecurityIdentifier> {
        /// Converts as security identifier to a string
         * @param value">The input value to be converted before serialziation
        @returns A new instance of TResult that is to be serialzied
        @Override
        public void serialize( SecurityIdentifier value, JsonGenerator gen, SerializerProvider serializers )
                throws IOException, JsonProcessingException {
            gen.writeString( value.toString() );
        }
    }

    public static class SecurityIdentifierJsonDeserializer extends JsonDeserializer<SecurityIdentifier> {
        /// Converts the input String to a security identifier
         * @param value">The deserialized value that needs to be converted to T
        @returns The converted value
        @Override
        public com.quantconnect.lean.SecurityIdentifier deserialize( JsonParser p, DeserializationContext ctxt )
                throws IOException, JsonProcessingException {
            final JsonNode node = p.getCodec().readTree( p );
            final String value = node.asText();
            if( StringUtils.isBlank( value ) )
                return null;
            
            return SecurityIdentifier.parse( value );
        }
    }
}