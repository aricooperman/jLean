﻿/*
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
using QuantConnect.Brokerages.Fxcm;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;

package com.quantconnect.lean.Tests.Brokerages.Fxcm
{
    public class FxcmLimitOrderTestParameters : LimitOrderTestParameters
    {
        public FxcmLimitOrderTestParameters(Symbol symbol, BigDecimal highLimit, BigDecimal lowLimit)
            : base(symbol, highLimit, lowLimit) {
        }

        public @Override boolean ModifyOrderToFill(IBrokerage brokerage, Order order, BigDecimal lastMarketPrice) {
            // FXCM Buy Limit orders will be rejected if the limit price is above the market price
            // FXCM Sell Limit orders will be rejected if the limit price is below the market price

            limit = (LimitOrder)order;
            previousLimit = limit.LimitPrice;

            fxcmBrokerage = (FxcmBrokerage)brokerage;
            quotes = fxcmBrokerage.GetBidAndAsk(new List<String> { new FxcmSymbolMapper().GetBrokerageSymbol(order.Symbol) });

            if( order.Quantity > 0) {
                // for limit buys we need to increase the limit price
                // buy limit price must be at bid price or below
                bidPrice = new BigDecimal( quotes.Single().BidPrice);
                Log.Trace( "FxcmLimitOrderTestParameters.ModifyOrderToFill(): Bid: " + bidPrice);
                limit.LimitPrice = Math.Max(previousLimit, Math.Min(bidPrice, limit.LimitPrice * 2));
            }
            else
            {
                // for limit sells we need to decrease the limit price
                // sell limit price must be at ask price or above
                askPrice = new BigDecimal( quotes.Single().AskPrice);
                Log.Trace( "FxcmLimitOrderTestParameters.ModifyOrderToFill(): Ask: " + askPrice);
                limit.LimitPrice = Math.Min(previousLimit, Math.Max(askPrice, limit.LimitPrice / 2));
            }

            return limit.LimitPrice != previousLimit;
        }
    }
}
