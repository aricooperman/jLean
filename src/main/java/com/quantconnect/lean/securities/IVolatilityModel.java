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

package com.quantconnect.lean.securities;

import java.math.BigDecimal;

import com.quantconnect.lean.data.BaseData;

/**
 * Represents a model that computes the volatility of a security
*/
public interface IVolatilityModel {

    /**
     * Gets an instance of <see cref="IVolatilityModel"/> that will always
     * return 0 for its volatility and does nothing during Update.
     */
    public static final IVolatilityModel Null = new NullVolatilityModel();
   
    /**
     * Gets the volatility of the security as a percentage
     */
    BigDecimal getVolatility();

    /**
     * Updates this model using the new price information in
     * the specified security instance
     * @param security The security to calculate volatility for
     * @param data The new data used to update the model
     */
    void update( Security security, BaseData data );

    /**
     * Provides access to a null implementation for <see cref="IVolatilityModel"/>
     */
    class NullVolatilityModel implements IVolatilityModel {
        public BigDecimal volatility;
        
        public BigDecimal getVolatility() {
            return volatility;
        }
        
        public void update( Security security, BaseData data ) { }
    }
}
