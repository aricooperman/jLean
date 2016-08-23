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

import com.fasterxml.jackson.core.JsonGenerator;
import com.fasterxml.jackson.core.JsonParser;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.DeserializationContext;
import com.fasterxml.jackson.databind.JsonDeserializer;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.JsonSerializer;
import com.fasterxml.jackson.databind.SerializerProvider;

//using System.Drawing;
//using System.Globalization;

public class ColorJsonConverters {
    
    public static class ColorJsonSerializer extends JsonSerializer<Color> {
    
    
//        /// Converts a Color to a hexadecimal as a string
//         * @param value">The input value to be converted before serialization
//        @returns Hexadecimal number as a string. If Color is null, returns default #000000
//        protected @Override String Convert( Color value ) {
//        }
        @Override
        public void serialize( Color value, JsonGenerator gen, SerializerProvider serializers )
                throws IOException, JsonProcessingException {
//            return value.IsEmpty ? string.Empty : String.format( "#{0:X2}{1:X2}{2:X2}", value.getRed(), value.G, value.B);
            
            gen.writeStartObject();
            if( value == null )
                gen.writeNull();
            else
                gen.writeStringField( "color", String.format( "#%02X%02X%02X", value.getRed(), value.getGreen(), value.getBlue() ) );
            gen.writeEndObject();
        }
    }
    
    public static class ColorJsonDeserializer extends JsonDeserializer<Color> {
        
        /// Converts hexadecimal number to integer
         * @param hexValue">Hexadecimal number
        @returns Integer representation of the hexadecimal
        private int hexToInt( String hexValue ) {
            if( hexValue.length() != 2 )
                throw new NumberFormatException( String.format( "Unable to convert '%s' to an Integer. Requires String length of 2.", hexValue ) );
            
            return Integer.parseInt( hexValue, 16 );
        }

        /// Converts the input String to a Color object
         * @param value">The deserialized value that needs to be converted to T
        @returns The converted value
        @Override
        public Color deserialize( JsonParser p, DeserializationContext ctxt )
                throws IOException, JsonProcessingException {
            final JsonNode node = p.getCodec().readTree( p );
            final String value = node.get( "color" ).asText();
            if( StringUtils.isBlank( value ) )
                return null;
            
            if( value.length() != 7)
                throw new NumberFormatException( String.format( "Unable to convert '%s' to a Color. Requires String length of 7 including the leading hashtag.", value ) );
            
            final int red = hexToInt( value.substring( 1, 2 ) );
            final int green = hexToInt( value.substring( 3, 2 ) );
            final int blue = hexToInt( value.substring( 5, 2 ) );
            
            return new Color( red, green, blue );
        }
    }
}
