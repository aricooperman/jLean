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

package com.quantconnect.lean.securities;

import org.apache.commons.lang3.StringUtils;

import com.quantconnect.lean.SecurityType;

/**
 * Represents the key to a single entry in the <see cref="MarketHoursDatabase"/> or the <see cref="SymbolPropertiesDatabase"/>
 */
public class SecurityDatabaseKey {
    /**
     * Represents that the specified symbol or market field will match all
     */
    public static final String WILDCARD = "[*]";
    
    /**
     * The market. If null, ignore market filtering
     */
    public final String market;
    
    /**
     * The symbol. If null, ignore symbol filtering
     */
    public final String symbol;
    
    /**
     * The security type
     */
    public final SecurityType securityType;

    /**
     * Initializes a new instance of the <see cref="SecurityDatabaseKey"/> class
     * @param market The market
     * @param symbol The symbol. specify null to apply to all symbols in market/security type
     * @param securityType The security type
     */
    public SecurityDatabaseKey( String market, String symbol, SecurityType securityType) {
        this.market = StringUtils.isEmpty(  market ) ? WILDCARD : market;
        this.securityType = securityType;
        this.symbol = StringUtils.isEmpty( symbol ) ? WILDCARD : symbol;
    }

    /**
     * Parses the specified String as a <see cref="SecurityDatabaseKey"/>
     * @param key The String representation of the key
     * @returns A new <see cref="SecurityDatabaseKey"/> instance
     */
    public static SecurityDatabaseKey parse( String key) {
        final String[] parts = key.split( "-" );
        if( parts.length != 3 )
            throw new IllegalArgumentException( "The specified key was not in the expected format: " + key );

        SecurityType type;
        try {
            type = SecurityType.valueOf( parts[0] );
        }
        catch( Exception e ) {
            throw new IllegalArgumentException( "Unable to parse '" + parts[2] + "' as a SecurityType.", e );
        }

        return new SecurityDatabaseKey(parts[1], parts[2], type);
    }

    /**
     * Indicates whether the current object is equal to another object of the same type.
     * @returns true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
     * @param other An object to compare with this object.
     */
    public boolean equals( SecurityDatabaseKey other ) {
        if( null == other ) return false;
        if( this == other ) return true;
        return market.equals( other.market ) && symbol.equals( other.symbol ) && securityType == other.securityType;
    }

    /**
     * Determines whether the specified object is equal to the current object.
     * @returns true if the specified object  is equal to the current object; otherwise, false.
     * @param obj The object to compare with the current object.
     */ 
    public @Override boolean equals( Object obj) {
        if( null == obj ) return false;
        if( this == obj ) return true;
        if( !(obj instanceof SecurityDatabaseKey ) ) return false;
        return equals( (SecurityDatabaseKey) obj );
    }

    /**
     * Serves as the default hash function. 
     * @returns A hash code for the current object.
     */
    @Override
    public int hashCode() {
        int hashCode = (market != null ? market.hashCode() : 0);
        hashCode = (hashCode*397) ^ (symbol != null ? symbol.hashCode() : 0);
        hashCode = (hashCode*397) ^ securityType.ordinal();
        return hashCode;
    }

//    public static boolean operator ==(SecurityDatabaseKey left, SecurityDatabaseKey right) {
//        return Equals(left, right);
//    }
//
//    public static boolean operator !=(SecurityDatabaseKey left, SecurityDatabaseKey right) {
//        return !Equals(left, right);
//    }
    
    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        return String.format( "%1$s-%2$s-%3$s", securityType, market != null ? market : WILDCARD, symbol != null ? symbol : WILDCARD );
    }
}
