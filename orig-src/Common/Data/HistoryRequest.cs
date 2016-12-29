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

using System;
using NodaTime;
using QuantConnect.Data.Market;
using QuantConnect.Securities;

package com.quantconnect.lean.Data
{
    /**
     * Represents a request for historical data
    */
    public class HistoryRequest
    {
        /**
         * Gets the start time of the request.
        */
        public DateTime StartTimeUtc { get; set; }
        /**
         * Gets the end time of the request. 
        */
        public DateTime EndTimeUtc { get; set; }
        /**
         * Gets the symbol to request data for
        */
        public Symbol Symbol { get; set; }
        /**
         * Gets the exchange hours used for processing fill forward requests
        */
        public SecurityExchangeHours ExchangeHours { get; set; }
        /**
         * Gets the requested data resolution
        */
        public Resolution Resolution { get; set; }
        /**
         * Gets the requested fill forward resolution, set to null for no fill forward behavior
        */
        public Resolution? FillForwardResolution { get; set; }
        /**
         * Gets whether or not to include extended market hours data, set to false for only normal market hours
        */
        public boolean IncludeExtendedMarketHours { get; set; }
        /**
         * Gets the data type used to process the subscription request, this type must derive from BaseData
        */
        public Class DataType { get; set; }
        /**
         * Gets the security type of the subscription
        */
        public SecurityType SecurityType { get; set; }
        /**
         * Gets the time zone of the time stamps on the raw input data
        */
        public ZoneId TimeZone { get; set; }
        /**
         * Gets the market for this subscription
        */
        public String Market { get; set; }
        /**
         * Gets true if this is a custom data request, false for normal QC data
        */
        public boolean IsCustomData { get; set; }

        /**
         * Initializes a new default instance of the <see cref="HistoryRequest"/> class
        */
        public HistoryRequest() {
            StartTimeUtc = EndTimeUtc = DateTime.UtcNow;
            Symbol = Symbol.Empty;
            ExchangeHours = SecurityExchangeHours.AlwaysOpen(TimeZones.NewYork);
            Resolution = Resolution.Minute;
            FillForwardResolution = Resolution.Minute;
            IncludeExtendedMarketHours = false;
            DataType = typeof (TradeBar);
            SecurityType = SecurityType.Equity;
            TimeZone = TimeZones.NewYork;
            Market = QuantConnect.Market.USA;
            IsCustomData = false;
        }

        /**
         * Initializes a new instance of the <see cref="HistoryRequest"/> class from the specified parameters
        */
         * @param startTimeUtc The start time for this request,
         * @param endTimeUtc The start time for this request
         * @param dataType The data type of the output data
         * @param symbol The symbol to request data for
         * @param securityType The security type of the symbol
         * @param resolution The requested data resolution
         * @param market The market this data belongs to
         * @param exchangeHours The exchange hours used in fill forward processing
         * @param fillForwardResolution The requested fill forward resolution for this request
         * @param includeExtendedMarketHours True to include data from pre/post market hours
         * @param isCustomData True for custom user data, false for normal QC data
        public HistoryRequest(DateTime startTimeUtc, 
            DateTime endTimeUtc,
            Class dataType,
            Symbol symbol,
            SecurityType securityType,
            Resolution resolution,
            String market,
            SecurityExchangeHours exchangeHours,
            Resolution? fillForwardResolution,
            boolean includeExtendedMarketHours,
            boolean isCustomData
            ) {
            StartTimeUtc = startTimeUtc;
            EndTimeUtc = endTimeUtc;
            Symbol = symbol;
            ExchangeHours = exchangeHours;
            Resolution = resolution;
            FillForwardResolution = fillForwardResolution;
            IncludeExtendedMarketHours = includeExtendedMarketHours;
            DataType = dataType;
            SecurityType = securityType;
            Market = market;
            IsCustomData = isCustomData;
            TimeZone = exchangeHours.TimeZone;
        }

        /**
         * Initializes a new instance of the <see cref="HistoryRequest"/> class from the specified config and exchange hours
        */
         * @param config The subscription data config used to initalize this request
         * @param hours The exchange hours used for fill forward processing
         * @param startTimeUtc The start time for this request,
         * @param endTimeUtc The start time for this request
        public HistoryRequest(SubscriptionDataConfig config, SecurityExchangeHours hours, DateTime startTimeUtc, DateTime endTimeUtc) {
            StartTimeUtc = startTimeUtc;
            EndTimeUtc = endTimeUtc;
            Symbol = config.Symbol;
            ExchangeHours = hours;
            Resolution = config.Resolution;
            FillForwardResolution = config.FillDataForward ? config.Resolution : (Resolution?) null;
            IncludeExtendedMarketHours = config.ExtendedMarketHours;
            DataType = config.Type;
            SecurityType = config.SecurityType;
            Market = config.Market;
            IsCustomData = config.IsCustomData;
            TimeZone = config.DataTimeZone;
        }
    }
}
