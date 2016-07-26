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
using Newtonsoft.Json;

package com.quantconnect.lean.Brokerages.Tradier
{
    /// <summary>
    /// Token response model from QuantConnect terminal
    /// </summary>
    public class TokenResponse
    {
        /// Access token for current requests:
        @JsonProperty( "sAccessToken")]
        public String AccessToken;

        /// Refersh token for next time
        @JsonProperty( "sRefreshToken")]
        public String RefreshToken;

        /// Seconds the tokens expires
        @JsonProperty( "iExpiresIn")]
        public int ExpiresIn;

        /// Scope of token access
        @JsonProperty( "sScope")]
        public String Scope;

        /// Time the token was issued:
        @JsonProperty( "dtIssuedAt")]
        public DateTime IssuedAt;

        /// Success flag:
        @JsonProperty( "success")]
        public boolean Success;

        /// <summary>
        ///  Default constructor:
        /// </summary>
        public TokenResponse() 
        { }
    }

}
