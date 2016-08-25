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

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

package com.quantconnect.lean.Brokerages.Tradier
{
    /**
     * Rate limiting categorization
    */
    public enum TradierApiRequestType
    { 
         * Standard Rate Limit
        Standard,
         * Data API Rate Limiting
        Data,
         * Orders API Rate Limit
        Orders
    }
    
    /**
     * Tradier account type:
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierAccountType 
    {
         * Account Type: Trader
        [EnumMember(Value = "pdt")]
        DayTrader,
         * Account Type: Cash
        [EnumMember(Value = "cash")]
        Cash,
         * Account Type: Margin
        [EnumMember(Value = "margin")]
        Margin
    }

    /**
     * Direction of the order
     * (buy, buy_to_open, buy_to_cover, buy_to_close, sell, sell_short, sell_to_open, sell_to_close)
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierOrderDirection
    {
         * TradierOrderDirection: Buy          -- Equity -- Open Buy New Position
        [EnumMember(Value = "buy")]
        Buy,
         * TradierOrderDirection: Sell Short   -- Equity -- Open New Short Sell 
        [EnumMember(Value = "sell_short")]
        SellShort,
         * TradierOrderDirection: Sell         -- Equity -- Closing Long Existing Positions
        [EnumMember(Value = "sell")]
        Sell,
         * TradierOrderDirection: Buy to Cover -- Equity -- Closing a short equity
        [EnumMember(Value = "buy_to_cover")]
        BuyToCover,


         * OPTIONS ONLY vvvvvvvvvvvvvvvvvvvvvvv
         * TradierOrderDirection: Sell to Open
        [EnumMember(Value = "sell_to_open")]
        SellToOpen,
         * TradierOrderDirection: Sell to Close
        [EnumMember(Value = "sell_to_close")]
        SellToClose,
         * TradierOrderDirection: Buy to Close
        [EnumMember(Value = "buy_to_close")]
        BuyToClose,
         * TradierOrderDirection: Buy to Open 
        [EnumMember(Value = "buy_to_open")]
        BuyToOpen,

        ///Order Fail Case:
        [EnumMember(Value = "none")]
        None,
    }

    /**
     * Status of the tradier order.
     *  (filled, canceled, open, expired, rejected, pending, partially_filled, submitted)
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierOrderStatus
    {
         * TradierOrderStatus: Fill
        [EnumMember(Value = "filled")]
        Filled,
         * TradierOrderStatus: Cancelled
        [EnumMember(Value = "canceled")]
        Canceled,
         * TradierOrderStatus: Open
        [EnumMember(Value = "open")]
        Open,
         * TradierOrderStatus: Expired
        [EnumMember(Value = "expired")]
        Expired,
         * TradierOrderStatus: Rejected
        [EnumMember(Value = "rejected")]
        Rejected,
         * TradierOrderStatus: Pending
        [EnumMember(Value = "pending")]
        Pending,
         * TradierOrderStatus: Partially Filled
        [EnumMember(Value = "partially_filled")]
        PartiallyFilled,
         * TradierOrderStatus: Submitted
        [EnumMember(Value = "submitted")]
        Submitted
    }

    /**
     * Length of the order offer.
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierOrderDuration
    {
         * TradierOrderDuration: Good to Cancelled
        [EnumMember(Value = "gtc")]
        GTC,
         * TradierOrderDuration: Day Period
        [EnumMember(Value = "day")]
        Day
    }

    /**
     * Class of the order.
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierOrderClass
    {
         * TradierOrderClass: Equity
        [EnumMember(Value = "equity")]
        Equity,
         * TradierOrderClass: Option
        [EnumMember(Value = "option")]
        Option,
         * TradierOrderClass: Multi
        [EnumMember(Value = "multileg")]
        Multileg,
         * TradierOrderClass: Combo
        [EnumMember(Value = "combo")]
        Combo
    }

    /**
     * Account status flag.
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierAccountStatus
    {
         * TradierAccountStatus: New
        [EnumMember(Value = "New")]
        New,
         * TradierAccountStatus: Approved
        [EnumMember(Value = "Approved")]
        Approved,
         * TradierAccountStatus: Closed
        [EnumMember(Value = "Closed")]
        Closed
    }

    /**
     * Tradier options status
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierOptionStatus
    {
         * TradierOptionStatus: exercise
        [EnumMember(Value = "exercise")]
        Exercise,
         * TradierOptionStatus: Expired
        [EnumMember(Value = "expired")]
        Expired,
         * TradierOptionStatus: Assignment
        [EnumMember(Value = "assignment")]
        Assignment
    }

    /**
     * TradeBar windows for Tradier's data histories
    */
    public enum TradierTimeSeriesIntervals
    {
         * TradierTimeSeriesIntervals: Tick
        [EnumMember(Value = "tick")]
        Tick,
         * TradierTimeSeriesIntervals: 1min
        [EnumMember(Value = "1min")]
        OneMinute,
         * TradierTimeSeriesIntervals: 5min
        [EnumMember(Value = "5min")]
        FiveMinutes,
         * TradierTimeSeriesIntervals: 15min
        [EnumMember(Value = "15min")]
        FifteenMinutes,
    }

    /**
     * Historical data intervals for tradier requests:
    */
    public enum TradierHistoricalDataIntervals
    {
         * TradierTimeSeriesIntervals: Daily
        [EnumMember(Value = "daily")]
        Daily,
         * TradierTimeSeriesIntervals: Weekly
        [EnumMember(Value = "weekly")]
        Weekly,
         * TradierTimeSeriesIntervals: Molnthly
        [EnumMember(Value = "monthly")]
        Monthly
    }


    /**
     * Tradier option type
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierOptionType
    {
         * Option Type
        [EnumMember(Value = "put")]
        Put,
         * Option Type
        [EnumMember(Value = "call")]
        Call
    }

    /**
     * Tradier options expiration
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierOptionExpirationType
    {
         * Option Expiration std.
        [EnumMember(Value = "standard")]
        Standard,
         * Option Expiration std.
        [EnumMember(Value = "weekly")]
        Weekly
    }

    /**
     * Account classification
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierAccountClassification
    {
         * Account Classification Individual
        [EnumMember(Value = "individual")]
        Individual,
         * Account Classification IRA
        [EnumMember(Value = "ira")]
        IRA,
         * Account Classification Roth_Ira
        [EnumMember(Value = "roth_ira")]
        Roth_Ira,
         * Account Classification Joint
        [EnumMember(Value = "joint")]
        Joint,
         * Account Classification Entity
        [EnumMember(Value = "entity")]
        Entity
    }

    /**
     * Tradier event type:
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierEventType
    {
         * Trade Event
        [EnumMember(Value = "trade")]
        Trade,
         * Journal Event
        [EnumMember(Value = "journal")]
        Journal,
         * Option Event
        [EnumMember(Value = "option")]
        Option,
         * Dividend Event
        [EnumMember(Value = "dividend")]
        Dividend
    }

    /**
     * Market type of the trade: 
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierTradeType
    {
         * Equity Trade Type
        [EnumMember(Value = "equity")]
        Equity,
         * Option Trade Type
        [EnumMember(Value = "option")]
        Option
    }

    /**
     * Tradier order type: (market, limit, stop, stop_limit or market) //credit, debit, even
    */
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradierOrderType
    {
         * Order Type: Limit
        [EnumMember(Value = "limit")]
        Limit,
         * Order Type: Market
        [EnumMember(Value = "market")]
        Market,
         * Order Type: Stop Limit
        [EnumMember(Value = "stop_limit")]
        StopLimit,
         * Order Type: Stop Market
        [EnumMember(Value = "stop")]
        StopMarket,

        // OPTIONS ONLY vvvvvvvvvvvvvvvvvvvvvvvv
         * Order Type: Credit
        [EnumMember(Value = "credit")]
        Credit,
         * Order Type: Debit
        [EnumMember(Value = "debit")]
        Debit,
         * Order Type: Even
        [EnumMember(Value = "even")]
        Even
    }

}
