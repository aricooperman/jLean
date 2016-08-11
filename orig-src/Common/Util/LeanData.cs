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
using System.Globalization;
using System.IO;
using QuantConnect.Data;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Util
{
    /// <summary>
    /// Provides methods for generating lean data file content
    /// </summary>
    public static class LeanData
    {
        /// <summary>
        /// Converts the specified base data instance into a lean data file csv line
        /// </summary>
        public static String GenerateLine(IBaseData data, SecurityType securityType, Resolution resolution)
        {
            milliseconds = data.Time.TimeOfDay.TotalMilliseconds.toString(CultureInfo.InvariantCulture);
            longTime = data.Time.toString(DateFormat.TwelveCharacter);

            switch (securityType)
            {
                case SecurityType.Equity:
                    switch (resolution)
                    {
                        case Resolution.Tick:
                            tick = (Tick) data;
                            return ToCsv(milliseconds, Scale(tick.LastPrice), tick.Quantity, tick.Exchange, tick.SaleCondition, tick.Suspicious ? "1" : "0");

                        case Resolution.Minute:
                        case Resolution.Second:
                            bar = (TradeBar) data;
                            return ToCsv(milliseconds, Scale(bar.Open), Scale(bar.High), Scale(bar.Low), Scale(bar.Close), bar.Volume);

                        case Resolution.Hour:
                        case Resolution.Daily:
                            bigBar = (TradeBar) data;
                            return ToCsv(longTime, Scale(bigBar.Open), Scale(bigBar.High), Scale(bigBar.Low), Scale(bigBar.Close), bigBar.Volume);
                    }
                    break;

                case SecurityType.Forex:
                case SecurityType.Cfd:
                    switch (resolution)
                    {
                        case Resolution.Tick:
                            tick = (Tick) data;
                            return ToCsv(milliseconds, tick.BidPrice, tick.AskPrice);

                        case Resolution.Second:
                        case Resolution.Minute:
                            bar = (TradeBar) data;
                            return ToCsv(milliseconds, bar.Open, bar.High, bar.Low, bar.Close);

                        case Resolution.Hour:
                        case Resolution.Daily:
                            bigBar = (TradeBar) data;
                            return ToCsv(longTime, bigBar.Open, bigBar.High, bigBar.Low, bigBar.Close);
                    }
                    break;

                case SecurityType.Option:
                    putCall = data.Symbol.ID.OptionRight == OptionRight.Put ? "P" : "C";
                    switch (resolution)
                    {
                        case Resolution.Tick:
                            tick = (Tick)data;
                            if (tick.TickType == TickType.Trade)
                            {
                                return ToCsv(milliseconds,
                                    Scale(tick.LastPrice), tick.Quantity, tick.Exchange, tick.SaleCondition, tick.Suspicious ? "1": "0");
                            }
                            if (tick.TickType == TickType.Quote)
                            {
                                return ToCsv(milliseconds,
                                    Scale(tick.BidPrice), tick.BidSize, Scale(tick.AskPrice), tick.AskSize, tick.Exchange, tick.Suspicious ? "1" : "0");
                            }
                            break;

                        case Resolution.Second:
                        case Resolution.Minute:
                            // option data can be quote or trade bars
                            quoteBar = data as QuoteBar;
                            if (quoteBar != null)
                            {
                                return ToCsv(milliseconds,
                                    ToCsv(quoteBar.Bid), quoteBar.LastBidSize, 
                                    ToCsv(quoteBar.Ask), quoteBar.LastAskSize);
                            }
                            tradeBar = data as TradeBar;
                            if (tradeBar != null)
                            {
                                return ToCsv(milliseconds,
                                    Scale(tradeBar.Open), Scale(tradeBar.High), Scale(tradeBar.Low), Scale(tradeBar.Close), tradeBar.Volume);
                            }
                            break;

                        case Resolution.Hour:
                        case Resolution.Daily:
                            // option data can be quote or trade bars
                            bigQuoteBar = data as QuoteBar;
                            if (bigQuoteBar != null)
                            {
                                return ToCsv(longTime,
                                    ToCsv(bigQuoteBar.Bid), bigQuoteBar.LastBidSize,
                                    ToCsv(bigQuoteBar.Ask), bigQuoteBar.LastAskSize);
                            }
                            bigTradeBar = data as TradeBar;
                            if (bigTradeBar != null)
                            {
                                return ToCsv(longTime,
                                    ToCsv(bigTradeBar), bigTradeBar.Volume);
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException("resolution", resolution, null);
                    }
                    break;
            }

            throw new NotImplementedException("LeanData.GenerateLine has not yet been implemented for security type: " + securityType + " at resolution: " + resolution);
        }

        /// <summary>
        /// Generates the full zip file path rooted in the <paramref name="dataDirectory"/>
        /// </summary>
        public static String GenerateZipFilePath( String dataDirectory, Symbol symbol, DateTime date, Resolution resolution, TickType tickType)
        {
            return Path.Combine(dataDirectory, GenerateRelativeZipFilePath(symbol, date, resolution, tickType));
        }

        /// <summary>
        /// Generates the full zip file path rooted in the <paramref name="dataDirectory"/>
        /// </summary>
        public static String GenerateZipFilePath( String dataDirectory, String symbol, SecurityType securityType, String market, DateTime date, Resolution resolution)
        {
            return Path.Combine(dataDirectory, GenerateRelativeZipFilePath(symbol, securityType, market, date, resolution));
        }

        /// <summary>
        /// Generates the relative zip directory for the specified symbol/resolution
        /// </summary>
        public static String GenerateRelativeZipFileDirectory(Symbol symbol, Resolution resolution)
        {
            isHourOrDaily = resolution == Resolution.Hour || resolution == Resolution.Daily;
            securityType = symbol.ID.SecurityType.toLowerCase();
            market = symbol.ID.Market.toLowerCase();
            res = resolution.toLowerCase();
            directory = Path.Combine(securityType, market, res);
            switch (symbol.ID.SecurityType)
            {
                case SecurityType.Base:
                case SecurityType.Equity:
                case SecurityType.Forex:
                case SecurityType.Cfd:
                    return !isHourOrDaily ? Path.Combine(directory, symbol.Value.toLowerCase()) : directory;

                case SecurityType.Option:
                    // options uses the underlying symbol for pathing
                    return !isHourOrDaily ? Path.Combine(directory, symbol.ID.Symbol.toLowerCase()) : directory;

                case SecurityType.Commodity:
                case SecurityType.Future:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Generates the relative zip file path rooted in the /Data directory
        /// </summary>
        public static String GenerateRelativeZipFilePath(Symbol symbol, DateTime date, Resolution resolution, TickType tickType)
        {
            return Path.Combine(GenerateRelativeZipFileDirectory(symbol, resolution), GenerateZipFileName(symbol, date, resolution, tickType));
        }

        /// <summary>
        /// Generates the relative zip file path rooted in the /Data directory
        /// </summary>
        public static String GenerateRelativeZipFilePath( String symbol, SecurityType securityType, String market, DateTime date, Resolution resolution)
        {
            directory = Path.Combine(securityType.toLowerCase(), market.toLowerCase(), resolution.toLowerCase());
            if (resolution != Resolution.Daily && resolution != Resolution.Hour)
            {
                directory = Path.Combine(directory, symbol.toLowerCase());
            }

            return Path.Combine(directory, GenerateZipFileName(symbol, securityType, date, resolution));
        }

        /// <summary>
        /// Generate's the zip entry name to hold the specified data.
        /// </summary>
        public static String GenerateZipEntryName(Symbol symbol, DateTime date, Resolution resolution, TickType tickType)
        {
            formattedDate = date.toString(DateFormat.EightCharacter);
            isHourOrDaily = resolution == Resolution.Hour || resolution == Resolution.Daily;

            switch (symbol.ID.SecurityType)
            {
                case SecurityType.Base:
                case SecurityType.Equity:
                case SecurityType.Forex:
                case SecurityType.Cfd:
                    if (isHourOrDaily)
                    {
                        return String.format("{0}.csv", 
                            symbol.Value.toLowerCase()
                            );
                    }

                    return String.format("{0}_{1}_{2}_{3}.csv", 
                        formattedDate, 
                        symbol.Value.toLowerCase(), 
                        resolution.toLowerCase(), 
                        tickType.toLowerCase()
                        );

                case SecurityType.Option:
                    if (isHourOrDaily)
                    {
                        return string.Join("_",
                            symbol.ID.Symbol.toLowerCase(), // underlying
                            tickType.toLowerCase(),
                            symbol.ID.OptionStyle.toLowerCase(),
                            symbol.ID.OptionRight.toLowerCase(),
                            Scale(symbol.ID.StrikePrice),
                            symbol.ID.Date.toString(DateFormat.EightCharacter)
                            ) + ".csv";
                    }

                    return string.Join("_",
                        formattedDate,
                        symbol.ID.Symbol.toLowerCase(), // underlying
                        resolution.toLowerCase(),
                        tickType.toLowerCase(),
                        symbol.ID.OptionStyle.toLowerCase(),
                        symbol.ID.OptionRight.toLowerCase(),
                        Scale(symbol.ID.StrikePrice),
                        symbol.ID.Date.toString(DateFormat.EightCharacter)
                        ) + ".csv";

                case SecurityType.Commodity:
                case SecurityType.Future:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates the entry name for a QC zip data file
        /// </summary>
        public static String GenerateZipEntryName( String symbol, SecurityType securityType, DateTime date, Resolution resolution, TickType dataType = TickType.Trade)
        {
            if (securityType != SecurityType.Base && securityType != SecurityType.Equity && securityType != SecurityType.Forex && securityType != SecurityType.Cfd)
            {
                throw new NotImplementedException("This method only implements base, equity, forex and cfd security type.");
            }

            symbol = symbol.toLowerCase();

            if (resolution == Resolution.Hour || resolution == Resolution.Daily)
            {
                return symbol + ".csv";
            }

            //All fx is quote data.
            if (securityType == SecurityType.Forex || securityType == SecurityType.Cfd)
            {
                dataType = TickType.Quote;
            }

            return String.format("{0}_{1}_{2}_{3}.csv", date.toString(DateFormat.EightCharacter), symbol, resolution.toLowerCase(), dataType.toLowerCase());
        }

        /// <summary>
        /// Generates the zip file name for the specified date of data.
        /// </summary>
        public static String GenerateZipFileName(Symbol symbol, DateTime date, Resolution resolution, TickType tickType)
        {
            tickTypeString = tickType.toLowerCase();
            formattedDate = date.toString(DateFormat.EightCharacter);
            isHourOrDaily = resolution == Resolution.Hour || resolution == Resolution.Daily;

            switch (symbol.ID.SecurityType)
            {
                case SecurityType.Base:
                case SecurityType.Equity:
                case SecurityType.Forex:
                case SecurityType.Cfd:
                    if (isHourOrDaily)
                    {
                        return String.format("{0}.zip", 
                            symbol.Value.toLowerCase()
                            );
                    }

                    return String.format("{0}_{1}.zip", 
                        formattedDate, 
                        tickTypeString
                        );

                case SecurityType.Option:
                    if (isHourOrDaily)
                    {
                        return String.format("{0}_{1}_{2}.zip", 
                            symbol.ID.Symbol.toLowerCase(), // underlying
                            tickTypeString,
                            symbol.ID.OptionStyle.toLowerCase()
                            );
                    }

                    return String.format("{0}_{1}_{2}.zip", 
                        formattedDate, 
                        tickTypeString,
                        symbol.ID.OptionStyle.toLowerCase()
                        );

                case SecurityType.Commodity:
                case SecurityType.Future:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates the zip file name for a QC zip data file
        /// </summary>
        public static String GenerateZipFileName( String symbol, SecurityType securityType, DateTime date, Resolution resolution, TickType? tickType = null)
        {
            if (resolution == Resolution.Hour || resolution == Resolution.Daily)
            {
                return symbol.toLowerCase() + ".zip";
            }

            zipFileName = date.toString(DateFormat.EightCharacter);
            tickType = tickType ?? (securityType == SecurityType.Forex || securityType == SecurityType.Cfd ? TickType.Quote : TickType.Trade);
            suffix = String.format("_{0}.zip", tickType.Value.toLowerCase());
            return zipFileName + suffix;
        }

        /// <summary>
        /// Gets the tick type most commonly associated with the specified security type
        /// </summary>
        /// <param name="securityType">The security type</param>
        /// <returns>The most common tick type for the specified security type</returns>
        public static TickType GetCommonTickType(SecurityType securityType)
        {
            if (securityType == SecurityType.Forex || securityType == SecurityType.Cfd)
            {
                return TickType.Quote;
            }
            return TickType.Trade;
        }

        /// <summary>
        /// Creates a symbol from the specified zip entry name
        /// </summary>
        /// <param name="securityType">The security type of the output symbol</param>
        /// <param name="resolution">The resolution of the data source producing the zip entry name</param>
        /// <param name="zipEntryName">The zip entry name to be parsed</param>
        /// <returns>A new symbol representing the zip entry name</returns>
        public static Symbol ReadSymbolFromZipEntry(SecurityType securityType, Resolution resolution, String zipEntryName)
        {
            isHourlyOrDaily = resolution == Resolution.Hour || resolution == Resolution.Daily;
            switch (securityType)
            {
                case SecurityType.Option:
                    parts = zipEntryName.Replace(".csv", string.Empty).Split('_');
                    if (isHourlyOrDaily)
                    {
                        style = (OptionStyle)Enum.Parse(typeof(OptionStyle), parts[2], true);
                        right = (OptionRight)Enum.Parse(typeof(OptionRight), parts[3], true);
                        strike = decimal.Parse(parts[4]) / 10000m;
                        expiry = DateTime.ParseExact(parts[5], DateFormat.EightCharacter, null);
                        return Symbol.CreateOption(parts[0], Market.USA, style, right, strike, expiry);
                    }
                    else
                    {
                        style = (OptionStyle)Enum.Parse(typeof(OptionStyle), parts[4], true);
                        right = (OptionRight)Enum.Parse(typeof(OptionRight), parts[5], true);
                        strike = decimal.Parse(parts[6]) / 10000m;
                        expiry = DateTime.ParseExact(parts[7], DateFormat.EightCharacter, null);
                        return Symbol.CreateOption(parts[1], Market.USA, style, right, strike, expiry);
                    }

                default:
                    throw new NotImplementedException("ReadSymbolFromZipEntry is not implemented for " + securityType + " " + resolution);
            }
        }

        /// <summary>
        /// Scale and convert the resulting number to deci-cents int.
        /// </summary>
        private static long Scale( BigDecimal value)
        {
            return (long)(value*10000);
        }

        /// <summary>
        /// Create a csv line from the specified arguments
        /// </summary>
        private static String ToCsv(params object[] args)
        {
            // use culture neutral formatting for decimals
            for (i = 0; i < args.Length; i++)
            {
                value = args[i];
                if (value is decimal)
                {
                    args[i] = ((decimal) value).toString(CultureInfo.InvariantCulture);
                }
            }

            return string.Join(",", args);
        }

        /// <summary>
        /// Creates a csv line for the bar, if null fills in empty strings
        /// </summary>
        private static String ToCsv(IBar bar)
        {
            if (bar == null)
            {
                return ToCsv( String.Empty, string.Empty, string.Empty, string.Empty);
            }
            return ToCsv(Scale(bar.Open), Scale(bar.High), Scale(bar.Low), Scale(bar.Close));
        }
    }
}
