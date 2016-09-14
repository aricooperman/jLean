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

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDate;

import com.fasterxml.jackson.databind.annotation.JsonDeserialize;
import com.fasterxml.jackson.databind.annotation.JsonSerialize;
import com.quantconnect.lean.SymbolJsonConverter.SymbolJsonDeserializer;
import com.quantconnect.lean.SymbolJsonConverter.SymbolJsonSerializer;

/**
 *  Represents a unique security identifier. This is made of two components,
 *  the unique SID and the Value. The value is the current ticker symbol while
 *  the SID is constant over the life of a security
 */
@JsonSerialize( using = SymbolJsonSerializer.class )
@JsonDeserialize( using = SymbolJsonDeserializer.class )
public final class Symbol implements Comparable<Object> {

    /**
     * Represents an unassigned symbol. This is intended to be used as an
     * uninitialized, default value
     */
    public static final Symbol EMPTY = new Symbol( SecurityIdentifier.EMPTY, null );
    
    /**
     * Provides a convience method for creating a Symbol for most security types.
     * This method currently does not support Option, Commodity, and Future
     * @param ticker The String ticker symbol
     * @param securityType The security type of the ticker. If securityType == Option, then a canonical symbol is created
     * @param market The market the ticker resides in
     * @param alias An alias to be used for the symbol cache. Required when
     * adding the same security from different markets
     * @returns A new Symbol object for the specified ticker
     */
    public static Symbol create( String ticker, SecurityType securityType, String market ) {
        return create( ticker, securityType, market, null );
    }
    
    public static Symbol create( String ticker, SecurityType securityType, String market, String alias ) {
        SecurityIdentifier sid;
        switch( securityType ) {
            case Base:
                sid = SecurityIdentifier.generateBase( ticker, market );
                break;
            case Equity:
                sid = SecurityIdentifier.generateEquity( ticker, market );
                break;
            case Forex:
                sid = SecurityIdentifier.generateForex( ticker, market );
                break;
            case Cfd:
                sid = SecurityIdentifier.generateCfd( ticker, market );
                break;
            case Option:
                alias = alias != null ? alias : "?" + ticker.toUpperCase();
                sid = SecurityIdentifier.generateOption( SecurityIdentifier.DefaultDate, ticker, market, BigDecimal.ZERO, OptionRight.CALL, OptionStyle.AMERICAN );
                break;
            case Commodity:
            case Future:
            default:
                throw new UnsupportedOperationException( "The security type has not been implemented yet: " + securityType );
        }
    
        return new Symbol( sid, alias != null ? alias : ticker );
    }
    
    /**
     * Provides a convenience method for creating an option Symbol.
     * @param underlying The underlying ticker
     * @param market The market the underlying resides in
     * @param style The option style (American, European, ect..)
     * @param right The option right (Put/Call)
     * @param strike The option strike price
     * @param expiry The option expiry date
     * @param alias An alias to be used for the symbol cache. Required when 
     * adding the same security from diferent markets
     * @returns A new Symbol object for the specified option contract
     */
    public static Symbol createOption( String underlying, String market, OptionStyle style, OptionRight right, BigDecimal strike, LocalDate expiry ) {
        return createOption( underlying, market, style, right, strike, expiry, null );
    }
    
    public static Symbol createOption( String underlying, String market, OptionStyle style, OptionRight right, BigDecimal strike, LocalDate expiry, String alias ) {
        final SecurityIdentifier sid = SecurityIdentifier.generateOption( expiry, underlying, market, strike, right, style );
        String sym = sid.getSymbol();
        if( sym.length() > 5 ) 
            sym += " ";
        
        // format spec: http://www.optionsclearing.com/components/docs/initiatives/symbology/symbology_initiative_v1_8.pdf
        if( alias == null ) {
            alias = String.format( "%-6s%02d%02d%02d%s%08d", sym, expiry.getYear() - 2000, expiry.getMonth().getValue(), expiry.getDayOfMonth(), 
                    sid.getOptionRight() == OptionRight.CALL ? "C" : "P", strike.setScale( 3, RoundingMode.HALF_UP ).movePointRight( 3 ) );
//            alias = String.format( "%-6s%2$s%3$s{3:00000000}", sym, sid.Date.toString(DateFormat.SixCharacter), sid.getOptionRight().toString().charAt( 0 ), sid.StrikePrice * 1000m);
        }
        
        return new Symbol( sid, alias );
    }
    
    /**
     * Gets the current symbol for this ticker
     */
    private String value;
    
    /**
     * Gets the security identifier for this symbol
     */
    private SecurityIdentifier id;
    
    /**
     * Initializes a new instance of the <see cref="Symbol"/> class
     * @param sid The security identifier for this symbol
     * @param value The current ticker symbol value
     */
    public Symbol( SecurityIdentifier sid, String value ) {
        if( value == null )
            throw new NullPointerException( "value" );

        this.id = sid;
        this.value = value.toUpperCase();
    }
    
    public String getValue() {
        return value;
    }
    
    public SecurityIdentifier getId() {
        return id;
    }
    
    /**
     * Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
     * @param obj The object to compare with the current object.
     * @returns true if the specified object  is equal to the current object; otherwise, false.
     */ 
    public boolean equals( Object obj ) {
        if( null == obj ) return false;
        if( this == obj ) return true;
    
        // compare strings just as you would a symbol object
        if( obj instanceof String ) {
            final String sidString = (String)obj;
            return SecurityIdentifier.parse( sidString ).map( sid -> id.equals( sid ) ).orElse( false );
        }
        
        // compare a sid just as you would a symbol object
        if( obj instanceof SecurityIdentifier ) 
            return id.equals( (SecurityIdentifier) obj );
    
        if( !(obj instanceof Symbol) ) return false;
        
        return equals( (Symbol)obj );
    }

    /**
     * Serves as a hash function for a particular type. 
     * @returns A hash code for the current <see cref="T:System.Object"/>.
     */
    public int hashCode() {
        // only SID is used for comparisons
        return id.hashCode();
    }
    
    /**
     * Compares the current instance with another object of the same type and returns an integer that indicates whether the current 
     * instance precedes, follows, or occurs in the same position in the sort order as the other object.
     * @returns A value that indicates the relative order of the objects being compared. The return value has these meanings: 
     *      Value Meaning Less than zero This instance precedes <paramref name="obj"/> in the sort order. Zero This instance occurs in the same 
     *      position in the sort order as <paramref name="obj"/>. Greater than zero This instance follows <paramref name="obj"/> in the sort order. 
     * @param obj An object to compare with this instance. 
     * @throws IllegalArgumentException obj is not the same type as this instance.
     */
    public int compareTo( Object obj ) {
        if( obj instanceof String ) {
            final String str = (String)obj;
            return value.compareToIgnoreCase( str );
        }
        
        if( obj instanceof Symbol ) {
            final Symbol sym = (Symbol)obj;
            return value.compareToIgnoreCase( sym.value );
        }
        
        throw new IllegalArgumentException( "Object must be of type Symbol or string.");
    }
    
    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    public String toString() {
        return SymbolCache.getTicker( this );
    }
    
    /**
     * Indicates whether the current object is equal to another object of the same type.
     * @param other An object to compare with this object.
     * @returns true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
     */
    public boolean equals( Symbol other ) {
        if( null == other ) return false;
        if( this == other ) return true;
        // only SID is used for comparisons
        return id.equals( other.id );
    }
    
//     * Equals operator 
//     * @param left The left operand
//     * @param right The right operand
//    @returns True if both symbols are equal, otherwise false
//    public static boolean operator ==(Symbol left, Symbol right)
//    {
//        if( ReferenceEquals(left, null )) return ReferenceEquals(right, null );
//        return left.Equals(right);
//    }
//    
//    /**
//     * Not equals operator 
//    */
//     * @param left The left operand
//     * @param right The right operand
//    @returns True if both symbols are not equal, otherwise false
//    public static boolean operator !=(Symbol left, Symbol right)
//    {
//        if( ReferenceEquals(left, null )) return ReferenceEquals(right, null );
//        return !left.Equals(right);
//    }
//    
//     * Returns the symbol's String ticker
//     * @param symbol The symbol
//    @returns The String ticker
//    [Obsolete( "Symbol implicit operator to String is provided for algorithm use only.")]
//    public static implicit operator string(Symbol symbol)
//    {
//        return symbol.toString();
//    }
//    
//     * Creates symbol using String as sid
//     * @param ticker The string
//    @returns The symbol
//    [Obsolete( "Symbol implicit operator from String is provided for algorithm use only.")]
//    public static implicit operator Symbol( String ticker)
//    {
//        Symbol symbol;
//        if( SymbolCache.TryGetSymbol(ticker, out symbol))
//        {
//            return symbol;
//        }
//    
//        SecurityIdentifier sid;
//        if( SecurityIdentifier.TryParse(ticker, out sid))
//        {
//            return new Symbol(sid, sid.Symbol);
//        }
//        
//        return Empty;
//    }
//    
//    // in order to maintain better compile time backwards compatibility,
//    // we'll redirect a few common String methods to Value, but mark obsolete
//    #pragma warning disable 1591
//            [Obsolete( "Symbol.Contains is a pass-through for Symbol.Value.Contains")]
//    public boolean Contains( String value) { return Value.Contains(value); }
//    [Obsolete( "Symbol.EndsWith is a pass-through for Symbol.Value.EndsWith")]
//    public boolean EndsWith( String value) { return Value.EndsWith(value); }
//    [Obsolete( "Symbol.StartsWith is a pass-through for Symbol.Value.StartsWith")]
//    public boolean StartsWith( String value) { return Value.StartsWith(value); }
//    [Obsolete( "Symbol.ToLower is a pass-through for Symbol.Value.ToLower")]
//    public String toLowerCase() { return Value.toLowerCase(); }
//    [Obsolete( "Symbol.ToUpper is a pass-through for Symbol.Value.ToUpper")]
//            public String toUpperCase() { return Value.toUpperCase(); }
//    #pragma warning restore 1591
}
