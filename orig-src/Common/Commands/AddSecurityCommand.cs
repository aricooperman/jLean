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
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Packets;

package com.quantconnect.lean.Commands
{
    /**
     * Represents a command to add a security to the algorithm
    */
    public class AddSecurityCommand : ICommand
    {
        /**
         * The security type of the security
        */
        public SecurityType SecurityType { get; set; }

        /**
         * The security's ticker symbol
        */
        public String Symbol { get; set; }

        /**
         * The requested resolution, defaults to Resolution.Minute
        */
        public Resolution Resolution { get; set; }

        /**
         * The security's market, defaults to <see cref="QuantConnect.Market.USA"/> except for Forex, defaults to <see cref="QuantConnect.Market.FXCM"/>
        */
        public String Market { get; set; }

        /**
         * The fill forward behavior, true to fill forward, false otherwise - defaults to true
        */
        public boolean FillDataForward { get; set; }

        /**
         * The leverage for the security, defaults to 2 for equity, 50 for forex, and 1 for everything else
        */
        public BigDecimal Leverage { get; set; }

        /**
         * The extended market hours flag, true to allow pre/post market data, false for only in market data
        */
        public boolean ExtendedMarketHours { get; set; }

        /**
         * Default construct that applies default values
        */
        public AddSecurityCommand() {
            Resolution = Resolution.Minute;
            Market = null;
            FillDataForward = true;
            Leverage = -1;
            ExtendedMarketHours = false;
        }

        /**
         * Runs this command against the specified algorithm instance
        */
         * @param algorithm The algorithm to run this command against
        public CommandResultPacket Run(IAlgorithm algorithm) {
            security = algorithm.AddSecurity(SecurityType, Symbol, Resolution, Market, FillDataForward, Leverage, ExtendedMarketHours);
            return new Result(this, true, security.Symbol);
        }

        /**
         * Result packet type for the <see cref="AddSecurityCommand"/> command
        */
        public class Result : CommandResultPacket
        {
            /**
             * The symbol result from the add security command
            */
            public Symbol Symbol { get; set; }

            /**
             * Initializes a new instance of the <see cref="Result"/> class
            */
            public Result(AddSecurityCommand command, boolean success, Symbol symbol)
                : base(command, success) {
                Symbol = symbol;
            }
        }
    }
}
