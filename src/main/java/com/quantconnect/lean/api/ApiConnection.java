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

//using System.Security.Cryptography;
//using System.Text;
//using Newtonsoft.Json;
//using RestSharp;
//using RestSharp.Authenticators;

package com.quantconnect.lean.api;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Optional;

import org.springframework.web.client.RestTemplate;

/**
 * API Connection and Hash Manager
*/
public class ApiConnection {
    private static final String SERVICE_URL = "https://www.quantconnect.com/api/v2/";
    
    /**
     * Authorized client to use for requests.
     */
    public RestTemplate Client;

    // Authorization Credentials
    private final String _userId;
    private final String _token;

    /**
     * Create a new Api Connection Class.
     * @param userId User Id number from QuantConnect.com account. Found at www.quantconnect.com/account 
     * @param token Access token for the QuantConnect account. Found at www.quantconnect.com/account 
     */
    public ApiConnection( int userId, String token ) {
        this._token = token;
        this._userId = Integer.toString( userId );
        this.Client = new RestTemplate();
    }

    /**
     * Return true if connected successfully.
    */
    public boolean getConnected() {
        //TODO
//        request = new RestRequest( "authenticate", Method.GET );
//        AuthenticationResponse response;
//        if( TryRequest(request, out response)) {
//            return response.Success;
//        }
//        return false;
        
        return false;
    }

    /**
     * Place a secure request and get back an object of type T.
     * <typeparam name="T"></typeparam>
     * @param request">
     * @param result Result object from the 
     * @returns T typed object response
    */
    public <T extends RestResponse> Optional<T> tryRequest( /*RestRequest request*/ ) {
        //TODO
//        try {
//            //Generate the hash each request
//            // Add the UTC timestamp to the request header.
//            // Timestamps older than 1800 seconds will not work.
//            final int timestamp = (int)Time.timeStamp();
//            final String hash = createSecureHash( timestamp );
//            request.addHeader( "Timestamp", Integer.toString( timestamp ) );
//            Client.Authenticator = new HttpBasicAuthenticator( _userId, hash );
//            
//            // Execute the authenticated REST API Call
//            restsharpResponse = Client.Execute(request);
//
//            //Verify success
//            result = JsonConvert.DeserializeObject<T>(restsharpResponse.Content);
//            if( !result.Success) {
//                //result;
//                return false;
//            }
//        }
//        catch( Exception err ) {
//            log.error( "Api.ApiConnection() Failed to make REST request.", err );
//            result = null;
//            return false;
//        }
        
        return null;
    }

    /**
     * Generate a secure hash for the authorization headers.
     * @throws NoSuchAlgorithmException 
     * @returns Time based hash of user token and timestamp.
     */
    private String createSecureHash( int timestamp ) throws NoSuchAlgorithmException {
        // Create a new hash using current UTC timestamp.
        // Hash must be generated fresh each time.
        final String data = String.format( "%1$s:%2$s", _token, timestamp);
        return SHA256( data );
    }

    /**
     * Encrypt the token:time data to make our API hash.
     * @param data Data to be hashed by SHA256
     * @throws NoSuchAlgorithmException 
     * @returns Hashed string.
     */
    private String SHA256( String data ) throws NoSuchAlgorithmException {
        MessageDigest crypt = MessageDigest.getInstance( "SHA-256" );
        
        final StringBuilder hash = new StringBuilder();
        crypt.update( data.getBytes() );
        final byte[] crypto = crypt.digest();
        
        for( byte theByte : crypto )
            hash.append( Integer.toString( (theByte & 0xff) + 0x100, 16 ).substring( 1 ) );
        
        return hash.toString();
    }
}
