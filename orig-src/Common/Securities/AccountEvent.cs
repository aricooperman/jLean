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

package com.quantconnect.lean.Securities
{
    /**
     * Messaging class signifying a change in a user's account
    */
    public class AccountEvent
    {
        /**
         * Gets the total cash balance of the account in units of <see cref="CurrencySymbol"/>
        */
        public BigDecimal CashBalance { get; private set; }

        /**
         * Gets the currency symbol
        */
        public String CurrencySymbol { get; private set; }

        /**
         * Creates an AccountEvent
        */
         * @param currencySymbol The currency's symbol
         * @param cashBalance The total cash balance of the account
        public AccountEvent( String currencySymbol, BigDecimal cashBalance) {
            CashBalance = cashBalance;
            CurrencySymbol = currencySymbol;
        }

        /**
         * Returns a String that represents the current object.
        */
        @returns 
         * A String that represents the current object.
         * 
         * <filterpriority>2</filterpriority>
        public @Override String toString() {
            return String.format( "Account %1$s Balance: %2$s", CurrencySymbol, CashBalance.toString( "0.00"));
        }
    }
}
