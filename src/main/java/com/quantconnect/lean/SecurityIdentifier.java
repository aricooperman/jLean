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

package com.quantconnect.lean;

import java.math.BigDecimal;
import java.math.BigInteger;
import java.time.LocalDate;
import java.util.Arrays;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

import org.apache.commons.lang3.StringUtils;
import org.apache.commons.lang3.tuple.Pair;

import com.fasterxml.jackson.databind.annotation.JsonDeserialize;
import com.fasterxml.jackson.databind.annotation.JsonSerialize;
import com.quantconnect.lean.configuration.Config;
import com.quantconnect.lean.data.auxiliary.MapFile;
import com.quantconnect.lean.data.auxiliary.MapFileResolver;
import com.quantconnect.lean.interfaces.IMapFileProvider;
import com.quantconnect.lean.util.SecurityIdentifierJsonConverter.SecurityIdentifierJsonDeserializer;
import com.quantconnect.lean.util.SecurityIdentifierJsonConverter.SecurityIdentifierJsonSerializer;


/**
 *  Defines a unique identifier for securities
 *  The SecurityIdentifier contains information about a specific security.
 *  This includes the symbol and other data specific to the SecurityType.
 *  The symbol is limited to 12 characters
 */
@JsonSerialize( using = SecurityIdentifierJsonSerializer.class, as = String.class )
@JsonDeserialize( using = SecurityIdentifierJsonDeserializer.class, as = SecurityIdentifier.class )
public class SecurityIdentifier {

    private static final BigInteger THIRTY_SIX = BigInteger.valueOf( 36 );

    private static final BigDecimal ONE_MILLION = BigDecimal.valueOf( 1_000_000 );

    private static final String MapFileProviderTypeName = Config.get( "map-file-provider", "LocalDiskMapFileProvider" );

    /**
     * Gets an instance of <see cref="SecurityIdentifier"/> that is empty, that is, one with no symbol specified
     */
    public static final SecurityIdentifier EMPTY = new SecurityIdentifier( null, BigInteger.ZERO );

    /**
     * Gets the date to be used when it does not apply.
     */
    public static final LocalDate DefaultDate = LocalDate.MIN;

    // these values define the structure of the 'otherData'
    // the constant width fields are used via modulus, so the width is the number of zeros specified,
    // {put/call:1}{oa-date:5}{style:1}{strike:6}{strike-scale:2}{market:3}{security-type:2}

    private static final BigInteger SecurityTypeWidth = BigInteger.valueOf( 100 );
    private static final BigInteger SecurityTypeOffset = BigInteger.valueOf( 1 );

    private static final BigInteger MarketWidth = BigInteger.valueOf( 1000 );
    private static final BigInteger MarketOffset = SecurityTypeOffset.multiply( SecurityTypeWidth );

    private static final int StrikeDefaultScale = 4;
    private static final BigDecimal StrikeDefaultScaleExpanded = BigDecimal.TEN.pow( StrikeDefaultScale );

    private static final BigInteger StrikeScaleWidth = BigInteger.valueOf( 100 );
    private static final BigInteger StrikeScaleOffset = MarketOffset.multiply( MarketWidth );

    private static final BigInteger StrikeWidth = BigInteger.valueOf( 1000000 );
    private static final BigInteger StrikeOffset = StrikeScaleOffset.multiply( StrikeScaleWidth );

    private static final BigInteger OptionStyleWidth = BigInteger.valueOf( 10 );
    private static final BigInteger OptionStyleOffset = StrikeOffset.multiply( StrikeWidth );

    private static final BigInteger DaysWidth = BigInteger.valueOf( 100000 );
    private static final BigInteger DaysOffset = OptionStyleOffset.multiply( OptionStyleWidth );

    private static final BigInteger PutCallOffset = DaysOffset.multiply( DaysWidth );
    private static final BigInteger PutCallWidth = BigInteger.valueOf( 10 );


    private final String symbol;
    private final BigInteger properties;

    /**
     * Gets the date component of this identifier. For equities this
     * is the first date the security traded. Technically speaking,
     * in LEAN, this is the first date mentioned in the map_files.
     * For options this is the expiry date. For futures this is the
     * settlement date. For forex and cfds this property will throw an
     * exception as the field is not specified.
     */
    public LocalDate getDate() {
        switch( getSecurityType() ) {
            case Equity:
            case Option:
            case Future:
                final BigInteger oadate = extractFromProperties( DaysOffset, DaysWidth );
                return LocalDate.ofEpochDay( oadate.longValue() );
            default:
                throw new IllegalArgumentException( "Date is only defined for SecurityType.Equity, SecurityType.Option and SecurityType.Future" );
        }
    }

    /**
     * Gets the original symbol used to generate this security identifier.
     * For equities, by convention this is the first ticker symbol for which
     * the security traded
     */
    public String getSymbol() {
        return symbol;
    }

    /**
     * Gets the market component of this security identifier. If located in the
     * internal mappings, the full String is returned. If the value is unknown,
     * the integer value is returned as a string.
     */
    public String getMarket() {
        final BigInteger marketCode = extractFromProperties( MarketOffset, MarketWidth );
        final String market = Market.decode( marketCode.intValue() );

        // if we couldn't find it, send back the numeric representation
        return market != null ? market : marketCode.toString();
    }

    /**
     * Gets the security type component of this security identifier.
     */
    public SecurityType getSecurityType() {
        return SecurityType.fromOrdinal( extractFromProperties( SecurityTypeOffset, SecurityTypeWidth ).intValue() );
    }

    /**
     * Gets the option strike price. This only applies to SecurityType.Option
     * and will thrown anexception if accessed otherwse.
     */
    public BigDecimal getStrikePrice() {
        if( getSecurityType() != SecurityType.Option )
                throw new IllegalArgumentException( "OptionType is only defined for SecurityType.Option" );

        final BigInteger scale = extractFromProperties( StrikeScaleOffset, StrikeScaleWidth );
        final BigInteger unscaled = extractFromProperties( StrikeOffset, StrikeWidth );
        final BigDecimal pow = BigDecimal.valueOf( 10 ).pow( scale.intValue() - StrikeDefaultScale );
        return pow.multiply( new BigDecimal( unscaled ) );
    }

    /**
     * Gets the option type component of this security identifier. This
     * only applies to SecurityType.Open and will throw an exception if
     * accessed otherwise.
     */
    public OptionRight getOptionRight() {
        if( getSecurityType() != SecurityType.Option )
                throw new IllegalArgumentException( "OptionRight is only defined for SecurityType.Option" );

        return OptionRight.fromOrdinal( extractFromProperties( PutCallOffset, PutCallWidth ).intValue() );
    }

    /**
     * Gets the option style component of this security identifier. This
     * only applies to SecurityType.Open and will throw an exception if
     * accessed otherwise.
     */
    public OptionStyle getOptionStyle() {
        if( getSecurityType() != SecurityType.Option )
            throw new IllegalArgumentException( "OptionStyle is only defined for SecurityType.Option");
            
        return OptionStyle.fromOrdinal( extractFromProperties( OptionStyleOffset, OptionStyleWidth ).intValue() );
    }

    /**
     * Initializes a new instance of the <see cref="SecurityIdentifier"/> class
     * @param symbol The base36 String encoded as a long using alpha [0-9A-Z]
     * @param properties Other data defining properties of the symbol including market,
     * security type, listing or expiry date, strike/call/put/style for options, ect...
     */
    public SecurityIdentifier( String symbol, BigInteger properties) {
        if( symbol == null )
            throw new IllegalArgumentException( "SecurityIdentifier requires a non-null String 'symbol'" );

        this.symbol = symbol;
        this.properties = properties;
    }

    /**
     * Generates a new <see cref="SecurityIdentifier"/> for an option
     * @param expiry The date the option expires
     * @param underlying The underlying security's symbol
     * @param market The market
     * @param strike The strike price
     * @param optionRight The option type, call or put
     * @param optionStyle The option style, American or European
     * @returns A new <see cref="SecurityIdentifier"/> representing the specified option security
     */
    public static SecurityIdentifier generateOption( LocalDate expiry, String underlying, String market,
        BigDecimal strike, OptionRight optionRight, OptionStyle optionStyle ) {
        return generate( expiry, underlying, SecurityType.Option, market, strike, optionRight, optionStyle );
    }

    /**
     * Helper overload that will search the mapfiles to resolve the first date. This implementation
     * uses the configured <see cref="IMapFileProvider"/> via the <see cref="Composer.Instance"/>
     * @param symbol The symbol as it is known today
     * @param market The market
     * @returns A new <see cref="SecurityIdentifier"/> representing the specified symbol today
     */
    public static SecurityIdentifier generateEquity( String symbol, String market ) {
        IMapFileProvider provider;
        try {
            provider = (IMapFileProvider)Class.forName( MapFileProviderTypeName ).newInstance();
        }
        catch( Exception e ) {
            throw new RuntimeException( e );
        }
        
        final MapFileResolver resolver = provider.get( market );
        final MapFile mapFile = resolver.resolveMapFile( symbol, LocalDate.now() );
        final LocalDate firstDate = mapFile.getFirstDate();
        if( !mapFile.isEmpty() )
            symbol = mapFile.getFirst().getMappedSymbol();

        return generateEquity( firstDate, symbol, market );
    }

    /**
     * Generates a new <see cref="SecurityIdentifier"/> for an equity
     * @param date The first date this security traded (in LEAN this is the first date in the map_file
     * @param symbol The ticker symbol this security traded under on the <paramref name="date"/>
     * @param market The security's market
     * @returns A new <see cref="SecurityIdentifier"/> representing the specified equity security
     */
    public static SecurityIdentifier generateEquity( LocalDate date, String symbol, String market ) {
        return generate( date, symbol, SecurityType.Equity, market );
    }

    /**
     * Generates a new <see cref="SecurityIdentifier"/> for a custom security
     * @param symbol The ticker symbol of this security
     * @param market The security's market
     * @returns A new <see cref="SecurityIdentifier"/> representing the specified base security
     */
    public static SecurityIdentifier generateBase( String symbol, String market ) {
        return generate( DefaultDate, symbol, SecurityType.Base, market );
    }

    /**
     * Generates a new <see cref="SecurityIdentifier"/> for a forex pair
     * @param symbol The currency pair in the format similar to: 'EURUSD'
     * @param market The security's market
     * @returns A new <see cref="SecurityIdentifier"/> representing the specified forex pair
     */
    public static SecurityIdentifier generateForex( String symbol, String market ) {
        return generate( DefaultDate, symbol, SecurityType.Forex, market );
    }

    /**
     * Generates a new <see cref="SecurityIdentifier"/> for a CFD security
     * @param symbol The CFD contract symbol
     * @param market The security's market
     * @returns A new <see cref="SecurityIdentifier"/> representing the specified CFD security
     */
    public static SecurityIdentifier generateCfd( String symbol, String market ) {
        return generate( DefaultDate, symbol, SecurityType.Cfd, market );
    }

    /**
     * Generic generate method. This method should be used carefully as some parameters are not required and
     * some parameters mean different things for different security types
     */
    private static SecurityIdentifier generate( LocalDate date, String symbol, SecurityType securityType,
            String market ) {
        return generate( date, symbol, securityType, market, BigDecimal.ZERO, OptionRight.CALL, OptionStyle.AMERICAN );
    }
    
    private static SecurityIdentifier generate( LocalDate date, String symbol, SecurityType securityType,
        String market, BigDecimal strike, OptionRight optionRight, OptionStyle optionStyle ) {
        if( securityType == null || securityType.ordinal() >= SecurityTypeWidth.intValue() )
            throw new IllegalArgumentException( "SecurityType must be between 0 and 99" );

        if( optionRight == null )
            throw new IllegalArgumentException( "OptionType must be non-null" );

        // normalize input strings
        market = market.toLowerCase();
        symbol = symbol.toUpperCase();

        final Integer marketIdentifier = Market.encode( market );
        if( marketIdentifier == null ) {
            throw new IllegalArgumentException( String.format( "The specified market wasn't found in the markets lookup. Requested: %s. " +
                "You can add markets by calling Market.addMarket( String,ushort)", market ) );
        }

        final BigInteger days = BigInteger.valueOf( date.toEpochDay() ).multiply( DaysOffset );
        final BigInteger marketCode = MarketOffset.multiply( BigInteger.valueOf( marketIdentifier ) );

        final Pair<BigInteger,BigInteger> strikeScalePair = normalizeStrike( strike );
        final BigInteger strk = strikeScalePair.getKey().multiply( StrikeOffset );
        final BigInteger strikeScale = strikeScalePair.getRight().multiply( StrikeScaleOffset );
        final BigInteger style = BigInteger.valueOf( optionStyle.ordinal() ).multiply( OptionStyleOffset );
        final BigInteger putcall = BigInteger.valueOf( optionRight.ordinal() ).multiply( PutCallOffset );

        final BigInteger otherData = putcall.add( days ).add( style ).add( strk ).add( strikeScale )
                .add( marketCode ).add( BigInteger.valueOf( securityType.ordinal() ) );

        return new SecurityIdentifier( symbol, otherData );
    }

    /**
     * Converts an upper case alpha numeric String into a long
     */
    private static BigInteger decodeBase36( String symbol ) {
        int pos = 0;
        BigInteger result = BigInteger.ZERO;
        
        for( int i = symbol.length() - 1; i > -1; i-- ) {
            char c = symbol.charAt( i );

            // assumes alpha numeric upper case only strings
            BigInteger value = BigInteger.valueOf( c <= 57
                ? c - '0'
                : c - 'A' + 10 );

            result = result.add( value.multiply( pow( THIRTY_SIX, pos++ ) ) );
        }
        
        return result;
    }

    /**
     * Converts a long to an uppercase alpha numeric string
     */
    private static String encodeBase36( BigInteger data ) {
        final StringBuilder stack = new StringBuilder();
        
        while( data.compareTo( BigInteger.ZERO ) != 0 ) {
            int value = data.mod( THIRTY_SIX ).intValue();
            char c = value < 10
                ? (char)(value + '0')
                : (char)(value - 10 + 'A');

            stack.append( c );
            data = data.divide( THIRTY_SIX );
        }
        
        return stack.reverse().toString();
    }

    /**
     * The strike is normalized into deci-cents and then a scale factor
     * is also saved to bring it back to un-normalized
     */
    private static Pair<BigInteger,BigInteger> normalizeStrike( BigDecimal strike )  {
        int scale = 0;

        if( strike.signum() == 0 )
            return Pair.of( BigInteger.ZERO, BigInteger.valueOf( scale ) );

        // convert strike to default scaling, this keeps the scale always positive
        strike = strike.multiply( StrikeDefaultScaleExpanded );

        while( strike.remainder( BigDecimal.TEN ).signum() == 0 ) {
            strike = strike.divide( BigDecimal.TEN );
            scale++;
        }

        if( strike.compareTo( ONE_MILLION ) >= 0 )
            throw new IllegalArgumentException( "The specified strike price's precision is too high: " + strike );

        return Pair.of( strike.toBigIntegerExact(), BigInteger.valueOf( scale ) );
    }

    /**
     * Accurately performs the integer exponentiation
     */
    private static BigInteger pow( BigInteger x, int pow ) {
        // don't use Math.Pow(double, double) due to precision issues
        return x.pow( pow );
//        return (BigInteger)BigInteger.Pow( x, pow );
    }

    /**
     * Parses the specified String into a <see cref="SecurityIdentifier"/>
     * The String must be a 40 digit number. The first 20 digits must be parseable
     * to a 64 bit unsigned integer and contain ancillary data about the security.
     * The second 20 digits must also be parseable as a 64 bit unsigned integer and
     * contain the symbol encoded from base36, this provides for 12 alpha numeric case
     * insensitive characters.
     * @param value The String value to be parsed
     * @returns A new <see cref="SecurityIdentifier"/> instance if the <paramref name="value"/> is able to be parsed.
     * @exception FormatException This exception is thrown if the string's length is not exactly 40 characters, or
     * if the components are unable to be parsed as 64 bit unsigned integers
     */
    public static Optional<SecurityIdentifier> parse( String value ) {
        try {
            return Optional.of( tryParse( value ) );
        }
        catch( Exception e ) {
            return Optional.empty();
        }
    }

    /**
     * Attempts to parse the specified <see paramref="value"/> as a <see cref="SecurityIdentifier"/>.
     * @param value The String value to be parsed
     * @param identifier The result of parsing, when this function returns true, <paramref name="identifier"/>
     * was properly created and reflects the input string, when this function returns false <paramref name="identifier"/>
     * will equal default(SecurityIdentifier)
     * @returns True on success, otherwise false
     * Helper method impl to be used by parse and tryparse
     */
    private static SecurityIdentifier tryParse( String value ) {
        final Pair<BigInteger,String> parsed = tryParseProperties( value );
        return new SecurityIdentifier( parsed.getRight(), parsed.getLeft() );
    }

    /**
     * Parses the String into its component BigInteger pieces
     */
    private static Pair<BigInteger,String> tryParseProperties( String value ) {
        if( value == null )
            throw new IllegalArgumentException( "Value is null" );

        if( StringUtils.isBlank( value ) )
            return Pair.of( BigInteger.ZERO, null );

        final List<String> parts = Arrays.stream( value.split( " " ) ).filter( StringUtils::isNotBlank ).collect( Collectors.toList() ); 
        if( parts.size() != 2 )
            throw new IllegalArgumentException( "The String must be splittable on space into two parts." );

        final String symbol = parts.get( 0 );
        final String otherData = parts.get( 1 );
        final BigInteger props = decodeBase36( otherData );
        
        return Pair.of( props, symbol );
    }

    /**
     * Extracts the embedded value from _otherData
     */
    private BigInteger extractFromProperties( BigInteger offset, BigInteger width ) {
        return properties.divide( offset ).mod( width );
    }

    /**
     * Indicates whether the current object is equal to another object of the same type.
     * @param other An object to compare with this object.
     * @returns true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
     */
    public boolean equals( SecurityIdentifier other ) {
        return properties.compareTo( other.properties ) == 0 && symbol.equals( other.symbol );
    }

    /**
     * Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
     * @param obj The object to compare with the current object. <filterpriority>2</filterpriority>
     * @returns true if the specified object  is equal to the current object; otherwise, false.
     */
    public boolean equals( Object obj ) {
        if( obj == null ) return false;
        if( !(obj instanceof SecurityIdentifier) ) return false;
        return equals( (SecurityIdentifier)obj );
    }

    /**
     * Serves as a hash function for a particular type. 
     * @returns A hash code for the current <see cref="T:System.Object"/>.
     */
    public int hashCode() {
        return (symbol.hashCode()*397) ^ properties.hashCode();
    }

//     * Override equals operator
//    public static boolean operator ==(SecurityIdentifier left, SecurityIdentifier right)
//    {
//        return Equals(left, right);
//    }
//
//     * Override not equals operator
//    public static boolean operator !=(SecurityIdentifier left, SecurityIdentifier right)
//    {
//        return !Equals(left, right);
//    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    public String toString() {
        return symbol + ' ' + encodeBase36( properties );
    }
}
