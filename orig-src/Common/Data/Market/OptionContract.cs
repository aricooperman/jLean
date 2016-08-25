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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;
using QuantConnect.Securities.Option;

package com.quantconnect.lean.Data.Market
{
    /**
     * Defines a single option contract at a specific expiration and strike price
    */
    public class OptionContract
    {
        private Lazy<OptionPriceModelResult> _optionPriceModelResult = new Lazy<OptionPriceModelResult>(() -> new OptionPriceModelResult(0m, new FirstOrderGreeks())); 

        /**
         * Gets the option contract's symbol
        */
        public Symbol Symbol
        {
            get; private set;
        }

        /**
         * Gets the underlying security's symbol
        */
        public Symbol UnderlyingSymbol
        {
            get; private set;
        }

        /**
         * Gets the strike price
        */
        public BigDecimal Strike
        {
            get { return Symbol.ID.StrikePrice; }
        }

        /**
         * Gets the expiration date
        */
        public DateTime Expiry
        {
            get { return Symbol.ID.Date; }
        }

        /**
         * Gets the right being purchased (call [right to buy] or put [right to sell])
        */
        public OptionRight Right
        {
            get { return Symbol.ID.OptionRight; }
        }

        /**
         * Gets the theoretical price of this option contract as computed by the <see cref="IOptionPriceModel"/>
        */
        public BigDecimal TheoreticalPrice
        {
            get { return _optionPriceModelResult.Value.TheoreticalPrice; }
        }

        /**
         * Gets the greeks for this contract
        */
        public FirstOrderGreeks Greeks
        {
            get { return _optionPriceModelResult.Value.Greeks; }
        }

        /**
         * Gets the local date time this contract's data was last updated
        */
        public DateTime Time
        {
            get; set;
        }

        /**
         * Gets the open interest
        */
        public BigDecimal OpenInterest
        {
            get; set;
        }

        /**
         * Gets the last price this contract traded at
        */
        public BigDecimal LastPrice
        {
            get; set;
        }

        /**
         * Gets the current bid price
        */
        public BigDecimal BidPrice
        {
            get; set;
        }

        /**
         * Get the current bid size
        */
        public long BidSize
        {
            get; set;
        }

        /**
         * Gets the ask price
        */
        public BigDecimal AskPrice
        {
            get; set;
        }

        /**
         * Gets the current ask size
        */
        public long AskSize
        {
            get; set;
        }

        /**
         * Gets the last price the underlying security traded at
        */
        public BigDecimal UnderlyingLastPrice
        {
            get; set;
        }

        /**
         * Initializes a new instance of the <see cref="OptionContract"/> class
        */
         * @param symbol The option contract symbol
         * @param underlyingSymbol The symbol of the underlying security
        public OptionContract(Symbol symbol, Symbol underlyingSymbol) {
            Symbol = symbol;
            UnderlyingSymbol = underlyingSymbol;
        }

        /**
         * Sets the option price model evaluator function to be used for this contract
        */
         * @param optionPriceModelEvaluator Function delegate used to evaluate the option price model
        internal void SetOptionPriceModel(Func<OptionPriceModelResult> optionPriceModelEvaluator) {
            _optionPriceModelResult = new Lazy<OptionPriceModelResult>(optionPriceModelEvaluator);
        }

        /**
         * Returns a String that represents the current object.
        */
        @returns 
         * A String that represents the current object.
         * 
        public @Override String toString() {
            return String.format( "%1$s%2$s%3$s{3:00000000}", Symbol.ID.Symbol, Expiry.toString(DateFormat.EightCharacter), Right.toString()[0], Strike*1000m);
        }
    }
}
