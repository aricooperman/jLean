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
     * Model for a TradierUser returned from the API.
    */
    public class TradierUserContainer
    {
         * User Profile Contents
        @JsonProperty( "profile")]
        public TradierUser Profile;

         * Constructor: Create user from tradier data.
        public TradierUserContainer() { }
    }

    /**
     * User profile array:
    */
    public class TradierUser
    {
         * Unique brokerage user id.
        @JsonProperty( "id")]
        public String Id { get; set; }

         * Name of user:
        @JsonProperty( "name")]
        public String Name { get; set; }

         * Array of user account details:
        @JsonProperty( "account")]
        [JsonConverter(typeof(SingleValueListConverter<TradierUserAccount>))]
        public List<TradierUserAccount> Accounts { get; set; }

         * Empty Constructor
        public TradierUser() {
            Id = "";
            Name = "";
            Accounts = new List<TradierUserAccount>();
        }
    }

    /**
     * Account only settings for a tradier user:
    */
    public class TradierUserAccount 
    {
         * Users account number
        @JsonProperty( "account_number")]
        public long AccountNumber { get; set; }

         * Pattern Trader:
        @JsonProperty( "day_trader")]
        public boolean DayTrader { get; set; }

         * Options level permissions on account.
        @JsonProperty( "option_level")]
        public int OptionLevel { get; set; }

         * Cash or Margin Account:
        @JsonProperty( "type")]
        public TradierAccountType Class { get; set; }

         * Date time of the last update:
        @JsonProperty( "last_update_date")]
        public DateTime LastUpdated { get; set; }

         * Status of the users account:
        @JsonProperty( "status")]
        public TradierAccountStatus Status { get; set; }

         * Class of user account
        @JsonProperty( "classification")]
        public TradierAccountClassification Classification { get; set; }

        /**
         * Create a new account:
        */
        public TradierUserAccount() {
            AccountNumber = 0;
            DayTrader = false;
            OptionLevel = 1;
            Class = TradierAccountType.Cash;
            LastUpdated = new DateTime();
            Status = TradierAccountStatus.Closed;
            Classification = TradierAccountClassification.Individual;
        }
    }

}
