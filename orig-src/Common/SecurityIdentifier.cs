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
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Util;

namespace QuantConnect
{
    /// <summary>
    /// Defines a unique identifier for securities
    /// </summary>
    /// <remarks>
    /// The SecurityIdentifier contains information about a specific security.
    /// This includes the symbol and other data specific to the SecurityType.
    /// The symbol is limited to 12 characters
    /// </remarks>
    [JsonConverter(typeof(SecurityIdentifierJsonConverter))]
    public struct SecurityIdentifier : IEquatable<SecurityIdentifier>
    {
        #region Empty, DefaultDate Fields

        private static readonly String MapFileProviderTypeName = Config.Get("map-file-provider", "LocalDiskMapFileProvider");

        /// <summary>
        /// Gets an instance of <see cref="SecurityIdentifier"/> that is empty, that is, one with no symbol specified
        /// </summary>
        public static readonly SecurityIdentifier Empty = new SecurityIdentifier( String.Empty, 0);

        /// <summary>
        /// Gets the date to be used when it does not apply.
        /// </summary>
        public static readonly DateTime DefaultDate = DateTime.FromOADate(0);

        #endregion

        #region Scales, Widths and Market Maps

        // these values define the structure of the 'otherData'
        // the constant width fields are used via modulus, so the width is the number of zeros specified,
        // {put/call:1}{oa-date:5}{style:1}{strike:6}{strike-scale:2}{market:3}{security-type:2}

        private static final ulong SecurityTypeWidth = 100;
        private static final ulong SecurityTypeOffset = 1;

        private static final ulong MarketWidth = 1000;
        private static final ulong MarketOffset = SecurityTypeOffset * SecurityTypeWidth;

        private static final int StrikeDefaultScale = 4;
        private static readonly ulong StrikeDefaultScaleExpanded = Pow(10, StrikeDefaultScale);

        private static final ulong StrikeScaleWidth = 100;
        private static final ulong StrikeScaleOffset = MarketOffset * MarketWidth;

        private static final ulong StrikeWidth = 1000000;
        private static final ulong StrikeOffset = StrikeScaleOffset * StrikeScaleWidth;

        private static final ulong OptionStyleWidth = 10;
        private static final ulong OptionStyleOffset = StrikeOffset * StrikeWidth;

        private static final ulong DaysWidth = 100000;
        private static final ulong DaysOffset = OptionStyleOffset * OptionStyleWidth;

        private static final ulong PutCallOffset = DaysOffset * DaysWidth;
        private static final ulong PutCallWidth = 10;

        #endregion

        #region Member variables

        private readonly String _symbol;
        private readonly ulong _properties;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the date component of this identifier. For equities this
        /// is the first date the security traded. Technically speaking,
        /// in LEAN, this is the first date mentioned in the map_files.
        /// For options this is the expiry date. For futures this is the
        /// settlement date. For forex and cfds this property will throw an
        /// exception as the field is not specified.
        /// </summary>
        public DateTime Date
        {
            get
            {
                stype = SecurityType;
                switch (stype)
                {
                    case SecurityType.Equity:
                    case SecurityType.Option:
                    case SecurityType.Future:
                        oadate = ExtractFromProperties(DaysOffset, DaysWidth);
                        return DateTime.FromOADate(oadate);
                    default:
                        throw new InvalidOperationException("Date is only defined for SecurityType.Equity, SecurityType.Option and SecurityType.Future");
                }
            }
        }

        /// <summary>
        /// Gets the original symbol used to generate this security identifier.
        /// For equities, by convention this is the first ticker symbol for which
        /// the security traded
        /// </summary>
        public String Symbol
        {
            get { return _symbol; }
        }

        /// <summary>
        /// Gets the market component of this security identifier. If located in the
        /// internal mappings, the full String is returned. If the value is unknown,
        /// the integer value is returned as a string.
        /// </summary>
        public String Market
        {
            get
            {
                marketCode = ExtractFromProperties(MarketOffset, MarketWidth);
                market = QuantConnect.Market.Decode((int)marketCode);

                // if we couldn't find it, send back the numeric representation
                return market ?? marketCode.ToString();
            }
        }

        /// <summary>
        /// Gets the security type component of this security identifier.
        /// </summary>
        public SecurityType SecurityType
        {
            get { return (SecurityType)ExtractFromProperties(SecurityTypeOffset, SecurityTypeWidth); }
        }

        /// <summary>
        /// Gets the option strike price. This only applies to SecurityType.Option
        /// and will thrown anexception if accessed otherwse.
        /// </summary>
        public BigDecimal StrikePrice
        {
            get
            {
                if (SecurityType != SecurityType.Option)
                {
                    throw new InvalidOperationException("OptionType is only defined for SecurityType.Option");
                }
                scale = ExtractFromProperties(StrikeScaleOffset, StrikeScaleWidth);
                unscaled = ExtractFromProperties(StrikeOffset, StrikeWidth);
                pow = Math.Pow(10, (int)scale - StrikeDefaultScale);
                return unscaled * (decimal)pow;
            }
        }

        /// <summary>
        /// Gets the option type component of this security identifier. This
        /// only applies to SecurityType.Open and will throw an exception if
        /// accessed otherwise.
        /// </summary>
        public OptionRight OptionRight
        {
            get
            {
                if (SecurityType != SecurityType.Option)
                {
                    throw new InvalidOperationException("OptionRight is only defined for SecurityType.Option");
                }
                return (OptionRight)ExtractFromProperties(PutCallOffset, PutCallWidth);
            }
        }

        /// <summary>
        /// Gets the option style component of this security identifier. This
        /// only applies to SecurityType.Open and will throw an exception if
        /// accessed otherwise.
        /// </summary>
        public OptionStyle OptionStyle
        {
            get
            {
                if (SecurityType != SecurityType.Option)
                {
                    throw new InvalidOperationException("OptionStyle is only defined for SecurityType.Option");
                }
                return (OptionStyle)(ExtractFromProperties(OptionStyleOffset, OptionStyleWidth));
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityIdentifier"/> class
        /// </summary>
        /// <param name="symbol">The base36 String encoded as a long using alpha [0-9A-Z]</param>
        /// <param name="properties">Other data defining properties of the symbol including market,
        /// security type, listing or expiry date, strike/call/put/style for options, ect...</param>
        public SecurityIdentifier( String symbol, ulong properties)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol", "SecurityIdentifier requires a non-null String 'symbol'");
            }
            _symbol = symbol;
            _properties = properties;
        }

        #endregion

        #region AddMarket, GetMarketCode, and Generate

        /// <summary>
        /// Generates a new <see cref="SecurityIdentifier"/> for an option
        /// </summary>
        /// <param name="expiry">The date the option expires</param>
        /// <param name="underlying">The underlying security's symbol</param>
        /// <param name="market">The market</param>
        /// <param name="strike">The strike price</param>
        /// <param name="optionRight">The option type, call or put</param>
        /// <param name="optionStyle">The option style, American or European</param>
        /// <returns>A new <see cref="SecurityIdentifier"/> representing the specified option security</returns>
        public static SecurityIdentifier GenerateOption(DateTime expiry,
            String underlying,
            String market,
            BigDecimal strike,
            OptionRight optionRight,
            OptionStyle optionStyle)
        {
            return Generate(expiry, underlying, SecurityType.Option, market, strike, optionRight, optionStyle);
        }

        /// <summary>
        /// Helper overload that will search the mapfiles to resolve the first date. This implementation
        /// uses the configured <see cref="IMapFileProvider"/> via the <see cref="Composer.Instance"/>
        /// </summary>
        /// <param name="symbol">The symbol as it is known today</param>
        /// <param name="market">The market</param>
        /// <returns>A new <see cref="SecurityIdentifier"/> representing the specified symbol today</returns>
        public static SecurityIdentifier GenerateEquity( String symbol, String market)
        {
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
            return Generate(DefaultDate, symbol, SecurityType.Cfd, market);
        }

        /// <summary>
        /// Generic generate method. This method should be used carefully as some parameters are not required and
        /// some parameters mean different things for different security types
        /// </summary>
        private static SecurityIdentifier Generate(DateTime date,
            String symbol,
            SecurityType securityType,
            String market,
            BigDecimal strike = 0,
            OptionRight optionRight = 0,
            OptionStyle optionStyle = 0)
        {
            if ((ulong)securityType >= SecurityTypeWidth || securityType < 0)
            {
                throw new ArgumentOutOfRangeException("securityType", "securityType must be between 0 and 99");
            }
            if ((int)optionRight > 1 || optionRight < 0)
            {
                throw new ArgumentOutOfRangeException("optionRight", "optionType must be either 0 or 1");
            }

            // normalize input strings
            market = market.ToLower();
            symbol = symbol.ToUpper();

            marketIdentifier = QuantConnect.Market.Encode(market);
            if (!marketIdentifier.HasValue)
            {
                throw new ArgumentOutOfRangeException("market", string.Format("The specified market wasn't found in the markets lookup. Requested: {0}. " +
                    "You can add markets by calling QuantConnect.Market.AddMarket( String,ushort)", market));
            }

            days = ((ulong)date.ToOADate()) * DaysOffset;
            marketCode = (ulong)marketIdentifier * MarketOffset;

            ulong strikeScale;
            strk = NormalizeStrike(strike, out strikeScale) * StrikeOffset;
            strikeScale *= StrikeScaleOffset;
            style = ((ulong)optionStyle) * OptionStyleOffset;
            putcall = (ulong)(optionRight) * PutCallOffset;

            otherData = putcall + days + style + strk + strikeScale + marketCode + (ulong)securityType;

            return new SecurityIdentifier(symbol, otherData);
        }

        /// <summary>
        /// Converts an upper case alpha numeric String into a long
        /// </summary>
        private static ulong DecodeBase36( String symbol)
        {
            int pos = 0;
            ulong result = 0;
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
        private static String EncodeBase36(ulong data)
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
        private static ulong NormalizeStrike(decimal strike, out ulong scale)
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

            return (ulong)strike;
        }

        /// <summary>
        /// Accurately performs the integer exponentiation
        /// </summary>
        private static ulong Pow(uint x, int pow)
        {
            // don't use Math.Pow(double, double) due to precision issues
            return (ulong)BigInteger.Pow(x, pow);
        }

        #endregion

        #region Parsing routines

        /// <summary>
        /// Parses the specified String into a <see cref="SecurityIdentifier"/>
        /// The String must be a 40 digit number. The first 20 digits must be parseable
        /// to a 64 bit unsigned integer and contain ancillary data about the security.
        /// The second 20 digits must also be parseable as a 64 bit unsigned integer and
        /// contain the symbol encoded from base36, this provides for 12 alpha numeric case
        /// insensitive characters.
        /// </summary>
        /// <param name="value">The String value to be parsed</param>
        /// <returns>A new <see cref="SecurityIdentifier"/> instance if the <paramref name="value"/> is able to be parsed.</returns>
        /// <exception cref="FormatException">This exception is thrown if the string's length is not exactly 40 characters, or
        /// if the components are unable to be parsed as 64 bit unsigned integers</exception>
        public static SecurityIdentifier Parse( String value)
        {
            Exception exception;
            SecurityIdentifier identifier;
            if (!TryParse(value, out identifier, out exception))
            {
                throw exception;
            }

            return identifier;
        }

        /// <summary>
        /// Attempts to parse the specified <see paramref="value"/> as a <see cref="SecurityIdentifier"/>.
        /// </summary>
        /// <param name="value">The String value to be parsed</param>
        /// <param name="identifier">The result of parsing, when this function returns true, <paramref name="identifier"/>
        /// was properly created and reflects the input string, when this function returns false <paramref name="identifier"/>
        /// will equal default(SecurityIdentifier)</param>
        /// <returns>True on success, otherwise false</returns>
        public static boolean TryParse( String value, out SecurityIdentifier identifier)
        {
            Exception exception;
            return TryParse(value, out identifier, out exception);
        }

        /// <summary>
        /// Helper method impl to be used by parse and tryparse
        /// </summary>
        private static boolean TryParse( String value, out SecurityIdentifier identifier, out Exception exception)
        {
            ulong props;
            String symbol;
            identifier = default(SecurityIdentifier);
            if (!TryParseProperties(value, out exception, out props, out symbol))
            {
                return false;
            }

            identifier = new SecurityIdentifier(symbol, props);
            return true;
        }

        /// <summary>
        /// Parses the String into its component ulong pieces
        /// </summary>
        private static boolean TryParseProperties( String value, out Exception exception, out ulong props, out String symbol)
        {
            props = ulong.MaxValue;
            symbol = string.Empty;
            exception = null;

            if (value == null)
            {
                return false;
            }

            if ( String.IsNullOrWhiteSpace(value))
            {
                props = 0;
                return true;
            }

            parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                exception = new FormatException("The String must be splittable on space into two parts.");
                return false;
            }

            symbol = parts[0];
            otherData = parts[1];

            props = DecodeBase36(otherData);
            return true;
        }

        /// <summary>
        /// Extracts the embedded value from _otherData
        /// </summary>
        private ulong ExtractFromProperties(ulong offset, ulong width)
        {
            return (_properties/offset)%width;
        }

        #endregion

        #region Equality members and ToString

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public boolean Equals(SecurityIdentifier other)
        {
            return _properties == other._properties && _symbol == other._symbol;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override boolean Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
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
        public override int GetHashCode()
        {
            unchecked { return (_symbol.GetHashCode()*397) ^ _properties.GetHashCode(); }
        }

        /// <summary>
        /// Override equals operator
        /// </summary>
        public static boolean operator ==(SecurityIdentifier left, SecurityIdentifier right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Override not equals operator
        /// </summary>
        public static boolean operator !=(SecurityIdentifier left, SecurityIdentifier right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns a String that represents the current object.
        /// </summary>
        /// <returns>
        /// A String that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override String ToString()
        {
            props = EncodeBase36(_properties);
            return _symbol + ' ' + props;
        }

        #endregion
    }
}
