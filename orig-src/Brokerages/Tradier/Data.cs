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
using System.Collections.Generic;
using Newtonsoft.Json;
using QuantConnect.Util;

package com.quantconnect.lean.Brokerages.Tradier
{
    /// <summary>
    /// Container for timeseries array
    /// </summary>
    public class TradierTimeSeriesContainer
    {
        /// Data Time Series
        @JsonProperty( "data")]
        [JsonConverter(typeof(SingleValueListConverter<TradierTimeSeries>))]
        public List<TradierTimeSeries> TimeSeries;
    }

    /// <summary>
    /// One bar of historical Tradier data.
    /// </summary>
    public class TradierTimeSeries
    {
        /// Time of Price Sample
        @JsonProperty( "time")]
        public DateTime Time;

        /// Tick data requests:
        @JsonProperty( "price")]
        public BigDecimal Price;

        /// Bar Requests: Open
        @JsonProperty( "open")]
        public BigDecimal Open;

        /// Bar Requests: High
        @JsonProperty( "high")]
        public BigDecimal High;

        /// Bar Requests: Low
        @JsonProperty( "low")]
        public BigDecimal Low;

        /// Bar Requests: Close
        @JsonProperty( "close")]
        public BigDecimal Close;

        /// Bar Requests: Volume
        @JsonProperty( "volume")]
        public long Volume;
    }


    /// <summary>
    /// Container for quotes:
    /// </summary>
    public class TradierQuoteContainer
    {
        /// Price Quotes:
        @JsonProperty( "quote")]
        [JsonConverter(typeof(SingleValueListConverter<TradierQuote>))]
        public List<TradierQuote> Quotes;
    }

    /// <summary>
    /// Quote data from Tradier:
    /// </summary>
    public class TradierQuote
    {
        /// Quote Symbol
        @JsonProperty( "symbol")]
        public String Symbol = "";

        /// Quote Description
        @JsonProperty( "description")]
        public String Description = "";

        /// Quote Exchange
        @JsonProperty( "exch")]
        public String Exchange = "";

        /// Quote Type
        @JsonProperty( "type")]
        public String Type = "";

        /// Quote Last Price
        @JsonProperty( "last")]
        public BigDecimal Last = 0;

        /// Quote Change Absolute
        @JsonProperty( "change")]
        public BigDecimal Change = 0;

        /// Quote Change Percentage
        @JsonProperty( "change_percentage")]
        public BigDecimal PercentageChange = 0;

        /// Quote Volume
        @JsonProperty( "volume")]
        public BigDecimal Volume = 0;

        /// Quote Average Volume
        @JsonProperty( "average_volume")]
        public BigDecimal AverageVolume = 0;

        /// Quote Last Volume
        @JsonProperty( "last_volume")]
        public BigDecimal LastVolume = 0;

        /// Last Trade Date in Unix Time
        @JsonProperty( "trade_date")]
        public long TradeDateUnix = 0;

        /// Open Price
        @JsonProperty( "open")]
        public decimal? Open = 0;

        /// High Price
        @JsonProperty( "high")]
        public decimal? High = 0;

        /// Low Price
        @JsonProperty( "low")]
        public decimal? Low = 0;

        /// Closng Price
        @JsonProperty( "close")]
        public decimal? Close = 0;

        /// Previous Close
        @JsonProperty( "prevclose")]
        public BigDecimal PreviousClose = 0;

        /// 52 W high
        @JsonProperty( "week_52_high")]
        public BigDecimal Week52High = 0;

        /// 52 W Low
        @JsonProperty( "week_52_low")]
        public BigDecimal Week52Low = 0;

        /// Bid Price
        @JsonProperty( "bid")]
        public BigDecimal Bid = 0;

        /// Bid Size:
        @JsonProperty( "bidsize")]
        public BigDecimal BidSize = 0;
        
        /// Bid Exchange
        @JsonProperty( "bidexch")]
        public String BigExchange = "";

        /// Bid Date Unix
        @JsonProperty( "bid_date")]
        private long BidDateUnix = 0;

        /// Asking Price
        @JsonProperty( "ask")]
        public BigDecimal Ask = 0;

        /// Asking Quantity
        @JsonProperty( "asksize")]
        public BigDecimal AskSize = 0;

        /// Ask Exchange
        @JsonProperty( "askexch")]
        public String AskExchange = "";

        /// Date of Ask
        @JsonProperty( "ask_date")]
        private long AskDateUnix = 0;

        /// Open Interest
        @JsonProperty( "open_interest")]
        private long Options_OpenInterest = 0;

        ///Option Underlying Asset
        @JsonProperty( "underlying")]
        private String Options_UnderlyingAsset = "";

        ///Option Strike Price
        @JsonProperty( "strike")]
        private BigDecimal Options_Strike = 0;

        ///Option Constract Size
        @JsonProperty( "contract_size")]
        private int Options_ContractSize = 0;

        ///Option Exp Date
        @JsonProperty( "expiration_date")]
        private long Options_ExpirationDate = 0;

        ///Option Exp Type
        @JsonProperty( "expiration_type")]
        private TradierOptionExpirationType Options_ExpirationType = TradierOptionExpirationType.Standard;

        /// Option Type
        @JsonProperty( "option_type")]
        private TradierOptionType Options_OptionType = TradierOptionType.Call;

        /// Empty Constructor
        public TradierQuote()
        { }
    }

    /// <summary>
    /// Container for deserializing history classes
    /// </summary>
    public class TradierHistoryDataContainer
    {
        /// Historical Data Contents
        @JsonProperty( "day")]
        [JsonConverter(typeof(SingleValueListConverter<TradierHistoryBar>))]
        public List<TradierHistoryBar> Data;
    }

    /// <summary>
    /// "Bar" for a history unit.
    /// </summary>
    public class TradierHistoryBar
    {
        /// Historical Data Bar: Date
        @JsonProperty( "date")]
        public DateTime Time;

        /// Historical Data Bar: Open
        @JsonProperty( "open")]
        public BigDecimal Open;

        /// Historical Data Bar: High
        @JsonProperty( "high")]
        public BigDecimal High;

        /// Historical Data Bar: Low
        @JsonProperty( "low")]
        public BigDecimal Low;

        /// Historical Data Bar: Close
        @JsonProperty( "close")]
        public BigDecimal Close;

        /// Historical Data Bar: Volume
        @JsonProperty( "volume")]
        public long Volume;
    }

    /// <summary>
    /// Current market status description
    /// </summary>
    public class TradierMarketStatus
    {
        /// Market Status: Date
        @JsonProperty( "date")]
        public DateTime Date;

        /// Market Status: Description
        @JsonProperty( "description")]
        public String Description;

        /// Market Status: Next Change in Status
        @JsonProperty( "next_change")]
        public String NextChange;

        /// Market Status: State 
        @JsonProperty( "state")]
        public String State;

        /// Market Status: Timestamp
        @JsonProperty( "timestamp")]
        public long TimeStamp;
    }

    /// <summary>
    /// Calendar status:
    /// </summary>
    public class TradierCalendarStatus
    {
        /// Trading Calendar: Day
        @JsonProperty( "days")]
        public TradierCalendarDayContainer Days;

        /// Trading Calendar: month
        @JsonProperty( "month")]
        public int Month;

        /// Trading Calendar: year
        @JsonProperty( "year")]
        public int Year;
    }

    /// <summary>
    /// Container for the days array:
    /// </summary>
    public class TradierCalendarDayContainer
    {
        /// Trading Calendar: Days List
        @JsonProperty( "day")]
        [JsonConverter(typeof(SingleValueListConverter<TradierCalendarDay>))]
        public List<TradierCalendarDay> Days;
    }

    /// <summary>
    /// Single days properties from the calendar:
    /// </summary>
    public class TradierCalendarDay
    {
        /// Trading Calendar: Day
        @JsonProperty( "date")]
        public DateTime Date;

        /// Trading Calendar: Sattus
        @JsonProperty( "status")]
        public String Status;

        /// Trading Calendar: Description
        @JsonProperty( "description")]
        public String Description;

        /// Trading Calendar: Premarket Hours
        @JsonProperty( "premarket")]
        public TradierCalendarDayMarketHours Premarket;

        /// Trading Calendar: Open Hours
        @JsonProperty( "open")]
        public TradierCalendarDayMarketHours Open;

        /// Trading Calendar: Post Hours
        @JsonProperty( "postmarket")]
        public TradierCalendarDayMarketHours Postmarket;
    }

    /// <summary>
    /// Start and finish time of market hours for this market.
    /// </summary>
    public class TradierCalendarDayMarketHours
    {
        /// Trading Calendar: Start Hours
        @JsonProperty( "start")]
        public DateTime Start;

        /// Trading Calendar: End Hours
        @JsonProperty( "end")]
        public DateTime End;
    }

    /// <summary>
    /// Tradier Search Container for Deserialization:
    /// </summary>
    public class TradierSearchContainer
    {
        /// Trading Search container
        @JsonProperty( "security")]
        public List<TradierSearchResult> Results;
    }

    /// <summary>
    /// One search result from API
    /// </summary>
    public class TradierSearchResult
    {
        /// Trading Search: Symbol
        @JsonProperty( "symbol")]
        public String Symbol;

        /// Trading Search: Exch
        @JsonProperty( "exchange")]
        public String Exchange;

        /// Trading Search: Type
        @JsonProperty( "type")]
        public String Type;

        /// Trading Search: Description
        @JsonProperty( "description")]
        public String Description;
    }

    /// <summary>
    /// Create a new stream session
    /// </summary>
    public class TradierStreamSession
    {
        /// Trading Stream: Session Id 
        public String SessionId;
        /// Trading Stream: Stream URL
        public String Url;
    }

    /// <summary>
    /// One data packet from a tradier stream:
    /// </summary>
    public class TradierStreamData
    {
        /// Trading Stream: Type
        @JsonProperty( "type")]
        public String Type;

        /// Trading Stream: Symbol
        @JsonProperty( "symbol")]
        public String Symbol;

        /// Trading Stream: Open
        @JsonProperty( "open")]
        public BigDecimal SummaryOpen;

        /// Trading Stream: High
        @JsonProperty( "high")]
        public BigDecimal SummaryHigh;

        /// Trading Stream: Low
        @JsonProperty( "low")]
        public BigDecimal SummaryLow;

        /// Trading Stream: Close
        @JsonProperty( "close")]
        public BigDecimal SummaryClose;

        /// Trading Stream: Bid Price
        @JsonProperty( "bid")]
        public BigDecimal BidPrice;

        /// Trading Stream: BidSize
        @JsonProperty( "bidsz")]
        public int BidSize;

        /// Trading Stream: Bid Exhc
        @JsonProperty( "bidexch")]
        public String BidExchange;

        /// Trading Stream: Bid Time
        @JsonProperty( "biddate")]
        public long BidDateUnix;

        /// Trading Stream: Last Price
        @JsonProperty( "price")]
        public BigDecimal TradePrice;

        /// Trading Stream: Last Size
        @JsonProperty( "size")]
        public BigDecimal TradeSize;

        /// Trading Stream: Last Exh
        @JsonProperty( "exch")]
        public String TradeExchange;

        /// Trading Stream: Last Vol
        @JsonProperty( "cvol")]
        public long TradeCVol;

        /// Trading Stream: Ask Price
        @JsonProperty( "ask")]
        public BigDecimal AskPrice;

        /// Trading Stream: Ask Size
        @JsonProperty( "asksz")]
        public int AskSize;

        /// Trading Stream: Ask Exhc
        @JsonProperty( "askexch")]
        public String AskExchange;

        /// Trading Stream: Ask Date
        @JsonProperty( "askdate")]
        public long AskDateUnix;
    }
}
