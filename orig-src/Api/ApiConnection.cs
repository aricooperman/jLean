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
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using QuantConnect.Logging;
using RestSharp;
using RestSharp.Authenticators;

package com.quantconnect.lean.Api
{
    /**
     * API Connection and Hash Manager
    */
    public class ApiConnection
    {
        /**
         * Authorized client to use for requests.
        */
        public RestClient Client;

        // Authorization Credentials
        private final String _userId;
        private final String _token;

        /**
         * Create a new Api Connection Class.
        */
         * @param userId User Id number from QuantConnect.com account. Found at www.quantconnect.com/account 
         * @param token Access token for the QuantConnect account. Found at www.quantconnect.com/account 
        public ApiConnection(int userId, String token) {
            _token = token;
            _userId = userId.toString();
            Client = new RestClient( "https://www.quantconnect.com/api/v2/");
        }

        /**
         * Return true if connected successfully.
        */
        public boolean Connected
        {
            get
            {
                request = new RestRequest( "authenticate", Method.GET);
                AuthenticationResponse response;
                if( TryRequest(request, out response)) {
                    return response.Success;
                }
                return false;
            }
        }

        /**
         * Place a secure request and get back an object of type T.
        */
         * <typeparam name="T"></typeparam>
         * @param request">
         * @param result Result object from the 
        @returns T typed object response
        public boolean TryRequest<T>(RestRequest request, out T result)
            where T : RestResponse
        {
            try
            {
                //Generate the hash each request
                // Add the UTC timestamp to the request header.
                // Timestamps older than 1800 seconds will not work.
                timestamp = (int)Time.TimeStamp();
                hash = CreateSecureHash(timestamp);
                request.AddHeader( "Timestamp", timestamp.toString());
                Client.Authenticator = new HttpBasicAuthenticator(_userId, hash);
                
                // Execute the authenticated REST API Call
                restsharpResponse = Client.Execute(request);

                //Verify success
                result = JsonConvert.DeserializeObject<T>(restsharpResponse.Content);
                if( !result.Success) {
                    //result;
                    return false;
                }
            }
            catch (Exception err) {
                Log.Error(err, "Api.ApiConnection() Failed to make REST request.");
                result = null;
                return false;
            }
            return true;
        }

        /**
         * Generate a secure hash for the authorization headers.
        */
        @returns Time based hash of user token and timestamp.
        private String CreateSecureHash(int timestamp) {
            // Create a new hash using current UTC timestamp.
            // Hash must be generated fresh each time.
            data = String.format( "%1$s:%2$s", _token, timestamp);
            return SHA256(data);
        }

        /**
         * Encrypt the token:time data to make our API hash.
        */
         * @param data Data to be hashed by SHA256
        @returns Hashed string.
        private String SHA256( String data) {
            crypt = new SHA256Managed();
            hash = new StringBuilder();
            crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(data), 0, Encoding.UTF8.GetByteCount(data));
            foreach (theByte in crypto) {
                hash.Append(theByte.toString( "x2"));
            }
            return hash.toString();
        }
    }
}
