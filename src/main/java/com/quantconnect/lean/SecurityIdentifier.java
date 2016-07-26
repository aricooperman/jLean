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
import java.util.stream.Collectors;

import org.apache.commons.lang3.StringUtils;
import org.apache.commons.lang3.tuple.Pair;

import com.fasterxml.jackson.databind.annotation.JsonDeserialize;
import com.fasterxml.jackson.databind.annotation.JsonSerialize;
import com.google.common.primitives.UnsignedInteger;
import com.google.common.primitives.UnsignedLong;
import com.quantconnect.lean.Global.OptionRight;
import com.quantconnect.lean.Global.OptionStyle;
import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.configuration.Config;
import com.quantconnect.lean.util.SecurityIdentifierJsonConverter.SecurityIdentifierJsonDeserializer;
import com.quantconnect.lean.util.SecurityIdentifierJsonConverter.SecurityIdentifierJsonSerializer;

//using System.Linq;
//using System.Numerics;
//using QuantConnect.Configuration;
//using QuantConnect.Interfaces;
//using QuantConnect.Util;

/* Defines a unique identifier for securities
 *  The SecurityIdentifier contains information about a specific security.
 *  This includes the symbol and other data specific to the SecurityType.
 *  The symbol is limited to 12 characters
*/
//[JsonConverter(typeof(SecurityIdentifierJsonConverter))]
@JsonSerialize( using = SecurityIdentifierJsonSerializer.class, as = String.class )
@JsonDeserialize( using = SecurityIdentifierJsonDeserializer.class, as = SecurityIdentifier.class )
public class SecurityIdentifier {

    private static final String MapFileProviderTypeName = Config.get( "map-file-provider", "LocalDiskMapFileProvider" );

    /// Gets an instance of <see cref="SecurityIdentifier"/> that is empty, that is, one with no symbol specified
    public static final SecurityIdentifier Empty = new SecurityIdentifier( null, UnsignedLong.ZERO );

    /// Gets the date to be used when it does not apply.
    public static final LocalDate DefaultDate = LocalDate.MIN;

    // these values define the structure of the 'otherData'
    // the constant width fields are used via modulus, so the width is the number of zeros specified,
    // {put/call:1}{oa-date:5}{style:1}{strike:6}{strike-scale:2}{market:3}{security-type:2}

    private static final UnsignedLong SecurityTypeWidth = UnsignedLong.valueOf( 100 );
    private static final UnsignedLong SecurityTypeOffset = UnsignedLong.valueOf( 1 );

    private static final UnsignedLong MarketWidth = UnsignedLong.valueOf( 1000 );
    private static final UnsignedLong MarketOffset = SecurityTypeOffset.times( SecurityTypeWidth );

    private static final int StrikeDefaultScale = 4;
    private static final UnsignedLong StrikeDefaultScaleExpanded = pow( UnsignedInteger.valueOf( 10 ), StrikeDefaultScale );

    private static final UnsignedLong StrikeScaleWidth = UnsignedLong.valueOf( 100 );
    private static final UnsignedLong StrikeScaleOffset = MarketOffset.times( MarketWidth );

    private static final UnsignedLong StrikeWidth = UnsignedLong.valueOf( 1000000 );
    private static final UnsignedLong StrikeOffset = StrikeScaleOffset.times( StrikeScaleWidth );

    private static final UnsignedLong OptionStyleWidth = UnsignedLong.valueOf( 10 );
    private static final UnsignedLong OptionStyleOffset = StrikeOffset.times( StrikeWidth );

    private static final UnsignedLong DaysWidth = UnsignedLong.valueOf( 100000 );
    private static final UnsignedLong DaysOffset = OptionStyleOffset.times( OptionStyleWidth );

    private static final UnsignedLong PutCallOffset = DaysOffset.times( DaysWidth );
    private static final UnsignedLong PutCallWidth = UnsignedLong.valueOf( 10 );


    private final String _symbol;
    private final UnsignedLong _properties;

    /// Gets the date component of this identifier. For equities this
    /// is the first date the security traded. Technically speaking,
    /// in LEAN, this is the first date mentioned in the map_files.
    /// For options this is the expiry date. For futures this is the
    /// settlement date. For forex and cfds this property will throw an
    /// exception as the field is not specified.
    public LocalDate getDate() {
        switch( getSecurityType() ) {
            case Equity:
            case Option:
            case Future:
                final UnsignedLong oadate = ExtractFromProperties( DaysOffset, DaysWidth );
                return LocalDate.ofEpochDay( oadate.longValue() );
            default:
                throw new IllegalArgumentException( "Date is only defined for SecurityType.Equity, SecurityType.Option and SecurityType.Future" );
        }
    }

    /// Gets the original symbol used to generate this security identifier.
    /// For equities, by convention this is the first ticker symbol for which
    /// the security traded
    public String getSymbol() {
        return _symbol;
    }

    /// Gets the market component of this security identifier. If located in the
    /// internal mappings, the full String is returned. If the value is unknown,
    /// the integer value is returned as a string.
    public String getMarket() {
        final UnsignedLong marketCode = ExtractFromProperties( MarketOffset, MarketWidth );
        final String market = Market.decode( marketCode.intValue() );

        // if we couldn't find it, send back the numeric representation
        return market != null ? market : marketCode.toString();
    }

    /// Gets the security type component of this security identifier.
    public SecurityType getSecurityType() {
        return SecurityType.fromOrdinal( ExtractFromProperties( SecurityTypeOffset, SecurityTypeWidth ).intValue() );
    }

    /// Gets the option strike price. This only applies to SecurityType.Option
    /// and will thrown anexception if accessed otherwse.
    public BigDecimal getStrikePrice() {
        if( getSecurityType() != SecurityType.Option )
                throw new IllegalArgumentException( "OptionType is only defined for SecurityType.Option" );

        final UnsignedLong scale = ExtractFromProperties( StrikeScaleOffset, StrikeScaleWidth );
        final UnsignedLong unscaled = ExtractFromProperties( StrikeOffset, StrikeWidth );
        final BigDecimal pow = BigDecimal.valueOf( 10 ).pow( scale.intValue() - StrikeDefaultScale );
        return pow.multiply( new BigDecimal( unscaled.bigIntegerValue() ) );
    }

    /// Gets the option type component of this security identifier. This
    /// only applies to SecurityType.Open and will throw an exception if
    /// accessed otherwise.
    public OptionRight getOptionRight() {
        if( getSecurityType() != SecurityType.Option )
                throw new IllegalArgumentException( "OptionRight is only defined for SecurityType.Option" );

        return OptionRight.fromOrdinal( ExtractFromProperties( PutCallOffset, PutCallWidth ).intValue() );
    }

    /// Gets the option style component of this security identifier. This
    /// only applies to SecurityType.Open and will throw an exception if
    /// accessed otherwise.
    public OptionStyle getOptionStyle() {
        if( getSecurityType() != SecurityType.Option )
            throw new IllegalArgumentException("OptionStyle is only defined for SecurityType.Option");
            
        return OptionStyle.fromOrdinal( ExtractFromProperties( OptionStyleOffset, OptionStyleWidth ).intValue() );
    }

    /// Initializes a new instance of the <see cref="SecurityIdentifier"/> class
    /// <param name="symbol">The base36 String encoded as a long using alpha [0-9A-Z]</param>
    /// <param name="properties">Other data defining properties of the symbol including market,
    /// security type, listing or expiry date, strike/call/put/style for options, ect...</param>
    public SecurityIdentifier( String symbol, UnsignedLong properties) {
        if (symbol == null)
            throw new IllegalArgumentException( "SecurityIdentifier requires a non-null String 'symbol'" );

        _symbol = symbol;
        _properties = properties;
    }

    /// Generates a new <see cref="SecurityIdentifier"/> for an option
    /// <param name="expiry">The date the option expires</param>
    /// <param name="underlying">The underlying security's symbol</param>
    /// <param name="market">The market</param>
    /// <param name="strike">The strike price</param>
    /// <param name="optionRight">The option type, call or put</param>
    /// <param name="optionStyle">The option style, American or European</param>
    /// <returns>A new <see cref="SecurityIdentifier"/> representing the specified option security</returns>
    public static SecurityIdentifier GenerateOption( LocalDate expiry, String underlying, String market,
        BigDecimal strike, OptionRight optionRight, OptionStyle optionStyle ) {
        return generate( expiry, underlying, SecurityType.Option, market, strike, optionRight, optionStyle );
    }

    /// Helper overload that will search the mapfiles to resolve the first date. This implementation
    /// uses the configured <see cref="IMapFileProvider"/> via the <see cref="Composer.Instance"/>
    /// <param name="symbol">The symbol as it is known today</param>
    /// <param name="market">The market</param>
    /// <returns>A new <see cref="SecurityIdentifier"/> representing the specified symbol today</returns>
    public static SecurityIdentifier generateEquity( String symbol, String market ) {
        provider = Composer.Instance.GetExportedValueByTypeName<IMapFileProvider>(MapFileProviderTypeName);
        resolver = provider.Get(market);
        mapFile = resolver.ResolveMapFile(symbol, DateTime.Today);
        firstDate = mapFile.FirstDate;
        if (mapFile.Any())
        {
            symbol = mapFile.OrderBy(x => x.Date).First().MappedSymbol;
        }
        return GenerateEquity(firstDate, symbol, market);
    }

    /// <summary>
    /// Generates a new <see cref="SecurityIdentifier"/> for an equity
    /// </summary>
    /// <param name="date">The first date this security traded (in LEAN this is the first date in the map_file</param>
    /// <param name="symbol">The ticker symbol this security traded under on the <paramref name="date"/></param>
    /// <param name="market">The security's market</param>
    /// <returns>A new <see cref="SecurityIdentifier"/> representing the specified equity security</returns>
    public static SecurityIdentifier GenerateEquity(DateTime date, String symbol, String market)
    {
        return Generate(date, symbol, SecurityType.Equity, market);
    }

    /// <summary>
    /// Generates a new <see cref="SecurityIdentifier"/> for a custom security
    /// </summary>
    /// <param name="symbol">The ticker symbol of this security</param>
    /// <param name="market">The security's market</param>
    /// <returns>A new <see cref="SecurityIdentifier"/> representing the specified base security</returns>
    public static SecurityIdentifier GenerateBase( String symbol, String market)
    {
        return Generate(DefaultDate, symbol, SecurityType.Base, market);
    }

    /// <summary>
    /// Generates a new <see cref="SecurityIdentifier"/> for a forex pair
    /// </summary>
    /// <param name="symbol">The currency pair in the format similar to: 'EURUSD'</param>
    /// <param name="market">The security's market</param>
    /// <returns>A new <see cref="SecurityIdentifier"/> representing the specified forex pair</returns>
    public static SecurityIdentifier GenerateForex( String symbol, String market)
    {
        return Generate(DefaultDate, symbol, SecurityType.Forex, market);
    }

    /// <summary>
    /// Generates a new <see cref="SecurityIdentifier"/> for a CFD security
    /// </summary>
    /// <param name="symbol">The CFD contract symbol</param>
    /// <param name="market">The security's market</param>
    /// <returns>A new <see cref="SecurityIdentifier"/> representing the specified CFD security</returns>
    public static SecurityIdentifier GenerateCfd( String symbol, String market)
    {
        return generate( DefaultDate, symbol, SecurityType.Cfd, market );
    }

    /// Generic generate method. This method should be used carefully as some parameters are not required and
    /// some parameters mean different things for different security types
    private static SecurityIdentifier generate( LocalDate date, String symbol, SecurityType securityType,
            String market ) {
        return generate( date, symbol, securityType, market, BigDecimal.ZERO, OptionRight.Call, OptionStyle.American );
    }
    
    private static SecurityIdentifier generate( LocalDate date, String symbol, SecurityType securityType,
        String market, BigDecimal strike, OptionRight optionRight, OptionStyle optionStyle ) {
        if ((UnsignedLong)securityType >= SecurityTypeWidth || securityType < 0)
            throw new IllegalArgumentException("securityType", "securityType must be between 0 and 99");

        if( optionRight == null )
            throw new IllegalArgumentException("optionRight", "optionType must be either 0 or 1");

        // normalize input strings
        market = market.toLowerCase();
        symbol = symbol.toUpperCase();

        marketIdentifier = QuantConnect.Market.Encode(market);
        if (!marketIdentifier.HasValue)
        {
            throw new ArgumentOutOfRangeException("market", string.Format("The specified market wasn't found in the markets lookup. Requested: {0}. " +
                "You can add markets by calling QuantConnect.Market.AddMarket( String,ushort)", market));
        }

        days = ((UnsignedLong)date.ToOADate()) * DaysOffset;
        marketCode = (UnsignedLong)marketIdentifier * MarketOffset;

        UnsignedLong strikeScale;
        strk = NormalizeStrike(strike, out strikeScale) * StrikeOffset;
        strikeScale *= StrikeScaleOffset;
        style = ((UnsignedLong)optionStyle) * OptionStyleOffset;
        putcall = (UnsignedLong)(optionRight) * PutCallOffset;

        otherData = putcall + days + style + strk + strikeScale + marketCode + (UnsignedLong)securityType;

        return new SecurityIdentifier(symbol, otherData);
    }

    /// <summary>
    /// Converts an upper case alpha numeric String into a long
    /// </summary>
    private static UnsignedLong DecodeBase36( String symbol)
    {
        int pos = 0;
        UnsignedLong result = 0;
        for (int i = symbol.Length - 1; i > -1; i--)
        {
            c = symbol[i];

            // assumes alpha numeric upper case only strings
            value = (uint)(c <= 57
                ? c - '0'
                : c - 'A' + 10);

            result += value * Pow(36, pos++);
        }
        return result;
    }

    /// <summary>
    /// Converts a long to an uppercase alpha numeric string
    /// </summary>
    private static String EncodeBase36(UnsignedLong data)
    {
        stack = new Stack<char>();
        while (data != 0)
        {
            value = data % 36;
            c = value < 10
                ? (char)(value + '0')
                : (char)(value - 10 + 'A');

            stack.Push(c);
            data /= 36;
        }
        return new string(stack.ToArray());
    }

    /// <summary>
    /// The strike is normalized into deci-cents and then a scale factor
    /// is also saved to bring it back to un-normalized
    /// </summary>
    private static UnsignedLong NormalizeStrike(decimal strike, out UnsignedLong scale)
    {
        str = strike;

        if (strike == 0)
        {
            scale = 0;
            return 0;
        }

        // convert strike to default scaling, this keeps the scale always positive
        strike *= StrikeDefaultScaleExpanded;

        scale = 0;
        while (strike % 10 == 0)
        {
            strike /= 10;
            scale++;
        }

        if (strike >= 1000000)
        {
            throw new ArgumentException("The specified strike price's precision is too high: " + str);
        }

        return (UnsignedLong)strike;
    }

    /// Accurately performs the integer exponentiation
    private static UnsignedLong pow( UnsignedInteger x, int pow ) {
        // don't use Math.Pow(double, double) due to precision issues
        return UnsignedLong.valueOf( x.bigIntegerValue().pow( pow ).longValueExact() );
//        return (UnsignedLong)BigInteger.Pow( x, pow );
    }

    /// Parses the specified String into a <see cref="SecurityIdentifier"/>
    /// The String must be a 40 digit number. The first 20 digits must be parseable
    /// to a 64 bit unsigned integer and contain ancillary data about the security.
    /// The second 20 digits must also be parseable as a 64 bit unsigned integer and
    /// contain the symbol encoded from base36, this provides for 12 alpha numeric case
    /// insensitive characters.
    /// <param name="value">The String value to be parsed</param>
    /// <returns>A new <see cref="SecurityIdentifier"/> instance if the <paramref name="value"/> is able to be parsed.</returns>
    /// <exception cref="FormatException">This exception is thrown if the string's length is not exactly 40 characters, or
    /// if the components are unable to be parsed as 64 bit unsigned integers</exception>
    public static SecurityIdentifier parse( String value ) {
        SecurityIdentifier identifier = tryParse( value )
        return identifier;
    }

    /// Attempts to parse the specified <see paramref="value"/> as a <see cref="SecurityIdentifier"/>.
    /// <param name="value">The String value to be parsed</param>
    /// <param name="identifier">The result of parsing, when this function returns true, <paramref name="identifier"/>
    /// was properly created and reflects the input string, when this function returns false <paramref name="identifier"/>
    /// will equal default(SecurityIdentifier)</param>
    /// <returns>True on success, otherwise false</returns>
    /// Helper method impl to be used by parse and tryparse
    private static SecurityIdentifier tryParse( String value ) {
        final Pair<UnsignedLong,String> parsed = tryParseProperties( value );
        UnsignedLong props = parsed;
        String symbol;
        return new SecurityIdentifier( symbol, props );
    }

    /// Parses the String into its component UnsignedLong pieces
    private static Pair<UnsignedLong,String> tryParseProperties( String value ) {
        UnsignedLong props = UnsignedLong.MaxValue;
        String symbol = null;

        if( value == null )
            throw new IllegalArgumentException( "Value is null" );

        if( StringUtils.isBlank( value ) )
            return Pair.of( UnsignedLong.ZERO, symbol );

        parts = Arrays.stream( value.split( ' ' ).filter( StringUtils::isNotBlank ).collect( Collectors.toList() ); 
        if( parts.size() != 2 )
            throw new IllegalArgumentException( "The String must be splittable on space into two parts." );

        symbol = parts.get( 0 );
        otherData = parts.get( 1 );

        props = Long.parseLong( otherData, 36 ); //DecodeBase36( otherData );
        
        return Pair.of( props, symbol );
    }

    /// Extracts the embedded value from _otherData
    private UnsignedLong ExtractFromProperties( UnsignedLong offset, UnsignedLong width ) {
        return _properties.dividedBy( offset ).mod( width );
    }

    /// Indicates whether the current object is equal to another object of the same type.
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
    /// <param name="other">An object to compare with this object.</param>
    public boolean Equals(SecurityIdentifier other) {
        return _properties == other._properties && _symbol == other._symbol;
    }

    /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
    /// <returns>
    /// true if the specified object  is equal to the current object; otherwise, false.
    /// </returns>
    /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
    public boolean equals( Object obj ) {
        if( obj == null ) return false;
        if (obj.GetType() != GetType()) return false;
        return Equals((SecurityIdentifier)obj);
    }

    /// <summary>
    /// Serves as a hash function for a particular type. 
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"/>.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public int hashCode() {
        unchecked { return (_symbol.GetHashCode()*397) ^ _properties.GetHashCode(); }
    }

//    /// Override equals operator
//    public static boolean operator ==(SecurityIdentifier left, SecurityIdentifier right)
//    {
//        return Equals(left, right);
//    }
//
//    /// Override not equals operator
//    public static boolean operator !=(SecurityIdentifier left, SecurityIdentifier right)
//    {
//        return !Equals(left, right);
//    }

    /// Returns a String that represents the current object.
    /// <returns>
    /// A String that represents the current object.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public String toString() {
        return _symbol + ' ' + Long.toString( _properties, 36 ); //EncodeBase36(_properties);
    }

}
