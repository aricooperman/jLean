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
    /// Order parent class for deserialization
    /// </summary>
    public class TradierOrdersContainer
    {
        /// Orders Contents:
        @JsonProperty( "orders")]
        [JsonConverter(typeof(NullStringValueConverter<TradierOrders>))]
        public TradierOrders Orders;

        /// Constructor: Orders parent:
        public TradierOrdersContainer()
        { }
    }

    /// <summary>
    /// Order container class
    /// </summary>
    public class TradierOrders
    {
        /// Array of user account details:
        @JsonProperty( "order")]
        [JsonConverter(typeof(SingleValueListConverter<TradierOrder>))]
        public List<TradierOrder> Orders = new List<TradierOrder>();

        /// Null Constructor:
        public TradierOrders() 
        { }
    }

    /// <summary>
    /// Intraday or pending order for user
    /// </summary>
    public class TradierOrder 
    {
        /// Unique order id.
        @JsonProperty( "id")]
        public long Id;

        /// Market, Limit Order etc.
        @JsonProperty( "type")]
        public TradierOrderType Type;

        /// Symbol
        @JsonProperty( "symbol")]
        public String Symbol;

        ///Long short.
        @JsonProperty( "side")]
        public TradierOrderDirection Direction;

        /// Quantity
        @JsonProperty( "quantity")]
        public BigDecimal Quantity;

        /// Status of the order (filled, canceled, open, expired, rejected, pending, partially_filled, submitted).
        @JsonProperty( "status")]
        public TradierOrderStatus Status;

        /// Duration of the order (day, gtc)
        @JsonProperty( "duration")]
        public TradierOrderDuration Duration;

        /// Percentage of gain or loss on the position.
        @JsonProperty( "price")]
        public BigDecimal Price;

        /// Average fill price
        @JsonProperty( "avg_fill_price")]
        public BigDecimal AverageFillPrice;

        /// Quantity executed
        @JsonProperty( "exec_quantity")]
        public BigDecimal QuantityExecuted;

        /// Last fill price
        @JsonProperty( "last_fill_price")]
        public BigDecimal LastFillPrice;

        /// Last amount filled
        @JsonProperty( "last_fill_quantity")]
        public BigDecimal LastFillQuantity;

        /// Quantity Remaining in Order.
        @JsonProperty( "remaining_quantity")]
        public BigDecimal RemainingQuantity;

        /// Date order was created.
        @JsonProperty( "create_date")]
        public DateTime CreatedDate;

        /// Date order was created.
        @JsonProperty( "transaction_date")]
        public DateTime TransactionDate;

        ///Classification of order (equity, option, multileg, combo)
        @JsonProperty( "class")]
        public TradierOrderClass Class;

        ///The number of legs
        @JsonProperty( "num_legs")]
        public int NumberOfLegs;

        /// Numberof legs in order
        @JsonProperty( "leg")]
        public List<TradierOrderLeg> Legs;

        /// Closed position trade summary
        public TradierOrder() 
        { }
    }

    /// <summary>
    /// Detailed order parent class
    /// </summary>
    public class TradierOrderDetailedContainer
    {
        /// Details of the order
        @JsonProperty( "order")]
        public TradierOrderDetailed DetailedOrder;
    }


    /// <summary>
    /// Deserialization wrapper for order response:
    /// </summary>
    public class TradierOrderResponse
    {
        /// Tradier Order information
        @JsonProperty( "order")]
        public TradierOrderResponseOrder Order = new TradierOrderResponseOrder();

        /// Errors in request
        @JsonProperty( "errors")]
        public TradierOrderResponseError Errors = new TradierOrderResponseError();
    }

    /// <summary>
    /// Errors result from an order request.
    /// </summary>
    public class TradierOrderResponseError
    {
        /// List of errors
        @JsonProperty( "error")]
        [JsonConverter(typeof(SingleValueListConverter<String>))]
        public List<String> Errors;
    }

    /// <summary>
    /// Order response when purchasing equity.
    /// </summary>
    public class TradierOrderResponseOrder
    { 
        /// id or order response
        @JsonProperty( "id")]
        public long Id;

        /// Partner id - me
        @JsonProperty( "partner_id")]
        public String PartnerId;

        /// Status of order
        @JsonProperty( "status")]
        public String Status;
    }

    /// <summary>
    /// Detailed order type.
    /// </summary>
    public class TradierOrderDetailed : TradierOrder
    {
        /// Order exchange
        @JsonProperty( "exch")]
        public String Exchange;

        /// Executed Exchange
        @JsonProperty( "exec_exch")]
        public String ExecutionExchange;

        /// Option type
        @JsonProperty( "option_type")]
        public TradierOptionType OptionType;

        /// Expiration date
        @JsonProperty( "expiration_date")]
        public DateTime OptionExpirationDate;

        /// Stop Price
        @JsonProperty( "stop_price")]
        public BigDecimal StopPrice;
    }

    /// <summary>
    /// Leg of a tradier order:
    /// </summary>
    public class TradierOrderLeg
    {
        /// Date order was created.
        @JsonProperty( "type")]
        public TradierOrderType Type;

        /// Symbol
        @JsonProperty( "symbol")]
        public String Symbol;

        ///Long short.
        @JsonProperty( "side")]
        public TradierOrderDirection Direction;

        /// Quantity
        @JsonProperty( "quantity")]
        public BigDecimal Quantity;

        /// Status of the order (filled, canceled, open, expired, rejected, pending, partially_filled, submitted).
        @JsonProperty( "status")]
        public TradierOrderStatus Status;

        /// Duration of the order (day, gtc)
        @JsonProperty( "duration")]
        public TradierOrderDuration Duration;

        /// Percentage of gain or loss on the position.
        @JsonProperty( "price")]
        public BigDecimal Price;

        /// Average fill price
        @JsonProperty( "avg_fill_price")]
        public BigDecimal AverageFillPrice;

        /// Quantity executed
        @JsonProperty( "exec_quantity")]
        public BigDecimal QuantityExecuted;

        /// Last fill price
        @JsonProperty( "last_fill_price")]
        public BigDecimal LastFillPrice;

        /// Last amount filled
        @JsonProperty( "last_fill_quantity")]
        public BigDecimal LastFillQuantity;

        /// Quantity Remaining in Order.
        @JsonProperty( "remaining_quantity")]
        public BigDecimal RemainingQuantity;

        /// Date order was created.
        @JsonProperty( "create_date")]
        public DateTime CreatedDate;

        /// Date order was created.
        @JsonProperty( "transaction_date")]
        public DateTime TransacionDate;

        /// Constructor
        public TradierOrderLeg()
        { }
    }

}
