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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Packets;
using QuantConnect.Util;

package com.quantconnect.lean.Brokerages.Tradier
{
    /**
     * Provides an implementations of IBrokerageFactory that produces a TradierBrokerage
    */
    public class TradierBrokerageFactory : BrokerageFactory
    {
        /**
         * Gets tradier values from configuration
        */
        public static class Configuration
        {
            /**
             * Gets the account ID to be used when instantiating a brokerage
            */
            public static int QuantConnectUserID
            {
                get { return Config.GetInt( "qc-user-id"); }
            }

            /**
             * Gets the account ID to be used when instantiating a brokerage
            */
            public static long AccountID
            {
                get { return Config.GetInt( "tradier-account-id"); }
            }

            /**
             * Gets the access token from configuration
            */
            public static String AccessToken
            {
                get { return Config.Get( "tradier-access-token"); }
            }

            /**
             * Gets the refresh token from configuration
            */
            public static String RefreshToken
            {
                get { return Config.Get( "tradier-refresh-token"); }
            }

            /**
             * Gets the date time the tokens were issued at from configuration
            */
            public static DateTime TokensIssuedAt
            {
                get { return Config.GetValue<DateTime>( "tradier-issued-at"); }
            }

            /**
             * Gets the life span of the tokens from configuration
            */
            public static Duration LifeSpan
            {
                get { return Duration.ofSeconds(Config.GetInt( "tradier-lifespan")); }
            }
        }

        /**
         * File path used to store tradier token data
        */
        public static final String TokensFile = "tradier-tokens.txt";

        /**
         * Initializes a new instance of he TradierBrokerageFactory class
        */
        public TradierBrokerageFactory()
            : base(typeof(TradierBrokerage)) {
        }

        /**
         * Gets the brokerage data required to run the brokerage from configuration/disk
        */
         * 
         * The implementation of this property will create the brokerage data dictionary required for
         * running live jobs. See <see cref="IJobQueueHandler.NextJob"/>
         * 
        public @Override Map<String,String> BrokerageData
        {
            get
            {
                String accessToken, refreshToken, issuedAt, lifeSpan;
                
                // always need to grab account ID from configuration
                accountID = Configuration.AccountID.toString();
                data = new Map<String,String>();
                if( File.Exists(TokensFile)) {
                    tokens = JsonConvert.DeserializeObject<TokenResponse>(File.ReadAllText(TokensFile));
                    accessToken = tokens.AccessToken;
                    refreshToken = tokens.RefreshToken;
                    issuedAt = tokens.IssuedAt.toString(CultureInfo.InvariantCulture);
                    lifeSpan = "86399";
                }
                else
                {
                    accessToken = Configuration.AccessToken;
                    refreshToken = Configuration.RefreshToken;
                    issuedAt = Configuration.TokensIssuedAt.toString(CultureInfo.InvariantCulture);
                    lifeSpan = Configuration.LifeSpan.TotalSeconds.toString(CultureInfo.InvariantCulture);
                }
                data.Add( "tradier-account-id", accountID);
                data.Add( "tradier-access-token", accessToken);
                data.Add( "tradier-refresh-token", refreshToken);
                data.Add( "tradier-issued-at", issuedAt);
                data.Add( "tradier-lifespan", lifeSpan);
                return data;
            }
        }

        /**
         * Gets a new instance of the <see cref="TradierBrokerageModel"/>
        */
        public @Override IBrokerageModel BrokerageModel
        {
            get { return new TradierBrokerageModel(); }
        }

        /**
         * Creates a new IBrokerage instance
        */
         * @param job The job packet to create the brokerage for
         * @param algorithm The algorithm instance
        @returns A new brokerage instance
        public @Override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm) {
            errors = new List<String>();
            accountID = Read<long>(job.BrokerageData, "tradier-account-id", errors);
            accessToken = Read<String>(job.BrokerageData, "tradier-access-token", errors);
            refreshToken = Read<String>(job.BrokerageData, "tradier-refresh-token", errors);
            issuedAt = Read<DateTime>(job.BrokerageData, "tradier-issued-at", errors);
            lifeSpan = Duration.ofSeconds(Read<double>(job.BrokerageData, "tradier-lifespan", errors));

            brokerage = new TradierBrokerage(algorithm.Transactions, algorithm.Portfolio, accountID);

            // if we're running live locally we'll want to save any new tokens generated so that they can easily be retrieved
            if( Config.GetBool( "tradier-save-tokens")) {
                brokerage.SessionRefreshed += (sender, args) =>
                {
                    File.WriteAllText(TokensFile, JsonConvert.SerializeObject(args, Formatting.Indented));
                };
            }
        
            brokerage.SetTokens(job.UserId, accessToken, refreshToken, issuedAt, lifeSpan);

            return brokerage;
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
         * <filterpriority>2</filterpriority>
        public @Override void Dispose() {
        }


        /**
         * Reads the tradier tokens from the <see cref="TokensFile"/> or from configuration
        */
        public static TokenResponse GetTokens() {
            // pick a source for our tokens
            if( File.Exists(TokensFile)) {
                Log.Trace( "Reading tradier tokens from " + TokensFile);
                return JsonConvert.DeserializeObject<TokenResponse>(File.ReadAllText(TokensFile));
            }
            
            return new TokenResponse
            {
                AccessToken = Config.Get( "tradier-access-token"),
                RefreshToken = Config.Get( "tradier-refresh-token"),
                IssuedAt = Config.GetValue<DateTime>( "tradier-tokens-issued-at"),
                ExpiresIn = Config.GetInt( "tradier-lifespan")
            };
        }
    }
}
