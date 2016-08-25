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
    /**
     * Gain loss parent class for deserialization
    */
    public class TradierGainLossContainer
    {
         * Profit Loss
        @JsonProperty( "gainloss")]
        public TradierGainLossClosed GainLossClosed;

         * Null Constructor
        public TradierGainLossContainer() { }
    }

    /**
     * Gain loss class
    */
    public class TradierGainLossClosed
    {
         * Array of user account details:
        @JsonProperty( "closed_position")]
        [JsonConverter(typeof(SingleValueListConverter<TradierGainLoss>))]
        public List<TradierGainLoss> ClosedPositions = new List<TradierGainLoss>();
    }

    /**
     * Account only settings for a tradier user:
    */
    public class TradierGainLoss 
    {
         * Date the position was closed.
        @JsonProperty( "close_date")]
        public DateTime CloseDate;

         * Date the position was opened
        @JsonProperty( "open_date")]
        public DateTime OpenDate;

         * Total cost of the order.
        @JsonProperty( "cost")]
        public BigDecimal Cost;

         * Gain or loss on the position.
        @JsonProperty( "gain_loss")]
        public BigDecimal GainLoss;

         * Percentage of gain or loss on the position.
        @JsonProperty( "gain_loss_percent")]
        public BigDecimal GainLossPercentage;

         * Total amount received for the order.
        @JsonProperty( "proceeds")]
        public BigDecimal Proceeds;

         * Number of shares/contracts
        @JsonProperty( "quantity")]
        public BigDecimal Quantity;

         * Symbol
        @JsonProperty( "symbol")]
        public String Symbol;

         * Number of shares/contracts
        @JsonProperty( "term")]
        public BigDecimal Term;

        /**
         * Closed position trade summary
        */
        public TradierGainLoss() { }
    }

}
