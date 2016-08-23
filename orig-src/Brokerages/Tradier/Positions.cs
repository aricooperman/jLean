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
    /// Empty class for deserializing positions held.
    */
    public class TradierPositionsContainer
    {
        /// Positions Class
        @JsonProperty( "positions")]
        [JsonConverter(typeof(NullStringValueConverter<TradierPositions>))]
        public TradierPositions TradierPositions;

        /// Default Constructor:
        public TradierPositionsContainer() { }
    }

    /**
    /// Position array container.
    */
    public class TradierPositions 
    { 
        /// Positions Class List
        @JsonProperty( "position")]
        [JsonConverter(typeof(SingleValueListConverter<TradierPosition>))]
        public List<TradierPosition> Positions;

        /// Default Constructor for JSON
        public TradierPositions() { }
    }


    /**
    /// Individual Tradier position model.
    */
    public class TradierPosition
    { 
        /// Position Id
        @JsonProperty( "id")]
        public long Id;

        /// Postion Date Acquired,
        @JsonProperty( "date_acquired")]
        public DateTime DateAcquired;

        /// Position Quantity
        @JsonProperty( "quantity")]
        public long Quantity;

        /// Position Cost:
        @JsonProperty( "cost_basis")]
        public BigDecimal CostBasis;

        ///Position Symbol
        @JsonProperty( "symbol")]
        public String Symbol;
    }

}
