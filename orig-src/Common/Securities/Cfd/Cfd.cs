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
using QuantConnect.Data;
using QuantConnect.Orders.Fees;
using QuantConnect.Orders.Fills;
using QuantConnect.Orders.Slippage;

package com.quantconnect.lean.Securities.Cfd
{
    /**
    /// CFD Security Object Implementation for CFD Assets
    */
    /// <seealso cref="Security"/>
    public class Cfd : Security
    {
        /**
        /// Constructor for the CFD security
        */
         * @param exchangeHours">Defines the hours this exchange is open
         * @param quoteCurrency">The cash object that represent the quote currency
         * @param config">The subscription configuration for this security
         * @param symbolProperties">The symbol properties for this security
        public Cfd(SecurityExchangeHours exchangeHours, Cash quoteCurrency, SubscriptionDataConfig config, SymbolProperties symbolProperties)
            : base(config,
                quoteCurrency,
                symbolProperties,
                new CfdExchange(exchangeHours),
                new CfdCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new ConstantFeeModel(0),
                new SpreadSlippageModel(),
                new ImmediateSettlementModel(),
                Securities.VolatilityModel.Null,
                new SecurityMarginModel(50m),
                new CfdDataFilter()
                ) {
            Holdings = new CfdHolding(this);
        }

        /**
        /// Constructor for the CFD security
        */
         * @param symbol">The security's symbol
         * @param exchangeHours">Defines the hours this exchange is open
         * @param quoteCurrency">The cash object that represent the quote currency
         * @param symbolProperties">The symbol properties for this security
        public Cfd(Symbol symbol, SecurityExchangeHours exchangeHours, Cash quoteCurrency, SymbolProperties symbolProperties)
            : base(symbol,
                quoteCurrency,
                symbolProperties,
                new CfdExchange(exchangeHours),
                new CfdCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new ConstantFeeModel(0),
                new SpreadSlippageModel(),
                new ImmediateSettlementModel(),
                Securities.VolatilityModel.Null,
                new SecurityMarginModel(50m),
                new CfdDataFilter()
                ) {
            Holdings = new CfdHolding(this);
        }

        /**
        /// Gets the contract multiplier for this CFD security
        */
        /// 
        /// PipValue := ContractMultiplier * PipSize
        /// 
        public BigDecimal ContractMultiplier
        {
            get { return SymbolProperties.ContractMultiplier; }
        }

        /**
        /// Gets the pip size for this CFD security
        */
        public BigDecimal PipSize
        {
            get { return SymbolProperties.PipSize; }
        }
    }
}
