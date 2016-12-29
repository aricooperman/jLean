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
using System.Collections.Specialized;
using System.Net;
using Newtonsoft.Json;
using QuantConnect.Configuration;
using QuantConnect.Logging;
using QuantConnect.Packets;
using RestSharp;

package com.quantconnect.lean.Messaging
{
    /**
     * Provides a common transmit method for utilizing the QC streaming API
    */
    public static class StreamingApi
    {
        /**
         * Gets a flag indicating whether or not the streaming api is enabled
        */
        public static final boolean IsEnabled = Config.GetBool( "send-via-api");

        // Client for sending asynchronous requests.
        private static final RestClient Client = new RestClient( "http://streaming.quantconnect.com");

        /**
         * Send a message to the QuantConnect Chart Streaming API.
        */
         * @param userId User Id
         * @param apiToken API token for authentication
         * @param packet Packet to transmit
        public static void Transmit(int userId, String apiToken, Packet packet) {
            try
            {
                tx = JsonConvert.SerializeObject(packet);
                if( tx.Length > 10000) {
                    Log.Trace( "StreamingApi.Transmit(): Packet too long: " + packet.GetType());
                    return;
                }
                if( userId == 0) {
                    Log.Error( "StreamingApi.Transmit(): UserId is not set. Check your config.json file 'job-user-id' property.");
                    return;
                }
                if( apiToken.equals( "") {
                    Log.Error( "StreamingApi.Transmit(): API Access token not set. Check your config.json file 'api-access-token' property.");
                    return;
                }

                request = new RestRequest();
                request.AddParameter( "uid", userId);
                request.AddParameter( "token", apiToken);
                request.AddParameter( "tx", tx);
                Client.ExecuteAsyncPost(request, (response, handle) =>
                {
                    try
                    {
                        result = JsonConvert.DeserializeObject<Response>(response.Content);
                        if( result.Type.equals( "error") {
                            Log.Error(new Exception(result.Message), "PacketType: " + packet.Type);
                        }
                    }
                    catch
                    {
                        Log.Error( "StreamingApi.Client.ExecuteAsyncPost(): Error deserializing JSON content.");
                    }
                }, "POST");
            }
            catch (Exception err) {
                Log.Error(err, "PacketType: " + packet.Type);
            }
        }

        /**
         * Response object from the Streaming API.
        */
        private class Response
        {
            /**
             * Class of response from the streaming api.
            */
             * success or error
            public String Type;

            /**
             * Message description of the error or success state.
            */
            public String Message;
        }
    }
}