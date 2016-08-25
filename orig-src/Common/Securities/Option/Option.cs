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
using QuantConnect.Data.Market;
using QuantConnect.Orders.Fees;
using QuantConnect.Orders.Fills;
using QuantConnect.Orders.Slippage;

package com.quantconnect.lean.Securities.Option
{
    /**
     * Option Security Object Implementation for Option Assets
    */
     * <seealso cref="Security"/>
    public class Option : Security
    {
        /**
         * The default number of days required to settle an equity sale
        */
        public static final int DefaultSettlementDays = 1;

        /**
         * The default time of day for settlement
        */
        public static final Duration DefaultSettlementTime = new TimeSpan(8, 0, 0);

        /**
         * Constructor for the option security
        */
         * @param exchangeHours Defines the hours this exchange is open
         * @param quoteCurrency The cash object that represent the quote currency
         * @param config The subscription configuration for this security
         * @param symbolProperties The symbol properties for this security
        public Option(SecurityExchangeHours exchangeHours, SubscriptionDataConfig config, Cash quoteCurrency, SymbolProperties symbolProperties)
            : base(config,
                quoteCurrency,
                symbolProperties,
                new OptionExchange(exchangeHours),
                new OptionCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new InteractiveBrokersFeeModel(),
                new SpreadSlippageModel(),
                new ImmediateSettlementModel(),
                Securities.VolatilityModel.Null,
                new SecurityMarginModel(2m),
                new OptionDataFilter()
                ) {
            PriceModel = new CurrentPriceOptionPriceModel();
            ContractFilter = new StrikeExpiryOptionFilter(-5, 5, Duration.ZERO, Duration.ofDays(35));
        }

        /**
         * Gets or sets the underlying security object.
        */
        public Security Underlying
        {
            get; set;
        }

        /**
         * Gets or sets the price model for this option security
        */
        public IOptionPriceModel PriceModel
        {
            get; set;
        }

        /**
         * Gets or sets the contract filter
        */
        public IDerivativeSecurityFilter ContractFilter
        {
            get; set;
        }

        /**
         * Sets the <see cref="ContractFilter"/> to a new instance of the <see cref="StrikeExpiryOptionFilter"/>
         * using the specified min and max strike values. Contracts with expirations further than 35
         * days out will also be filtered.
        */
         * @param minStrike The min strike rank relative to market price, for example, -1 would put
         * a lower bound of one strike under market price, where a +1 would put a lower bound of one strike
         * over market price
         * @param maxStrike The max strike rank relative to market place, for example, -1 would put
         * an upper bound of on strike under market price, where a +1 would be an upper bound of one strike
         * over market price
        public void SetFilter(int minStrike, int maxStrike) {
            SetFilter(minStrike, maxStrike, Duration.ZERO, Duration.ofDays(35));
        }

        /**
         * Sets the <see cref="ContractFilter"/> to a new instance of the <see cref="StrikeExpiryOptionFilter"/>
         * using the specified min and max strike and expiration range alues
        */
         * @param minStrike The min strike rank relative to market price, for example, -1 would put
         * a lower bound of one strike under market price, where a +1 would put a lower bound of one strike
         * over market price
         * @param maxStrike The max strike rank relative to market place, for example, -1 would put
         * an upper bound of on strike under market price, where a +1 would be an upper bound of one strike
         * over market price
         * @param minExpiry The minimum time until expiry to include, for example, Duration.ofDays(10)
         * would exclude contracts expiring in less than 10 days
         * @param maxExpiry The maxmium time until expiry to include, for example, Duration.ofDays(10)
         * would exclude contracts expiring in more than 10 days
        public void SetFilter(int minStrike, int maxStrike, Duration minExpiry, Duration maxExpiry) {
            ContractFilter = new StrikeExpiryOptionFilter(minStrike, maxStrike, minExpiry, maxExpiry);
        }
    }
}
