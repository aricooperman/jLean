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

package com.quantconnect.lean.lean.engine.datafeeds;

import java.util.ArrayList;
import java.util.List;

import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.securities.Security;

/**
 * Defines a container type to hold data produced by a data feed subscription
*/
public class DataFeedPacket {
    
    private final List<BaseData> data;
    
    private SubscriptionDataConfig configuration;
    private Security security;

    /**
     * The security
     */
    public Security getSecurity() {
        return security;
    }

    public void setSecurity( final Security security ) {
        this.security = security;
    }

    /**
     * The subscription configuration that produced this data
    */
    public SubscriptionDataConfig getConfiguration() {
        return configuration;
    }

    public void setConfiguration( final SubscriptionDataConfig configuration ) {
        this.configuration = configuration;
    }

    /**
     * Gets the number of data points held within this packet
     */
    public int size() {
        return data.size();
    }

    /**
     * The data for the security
     */
    public List<BaseData> getData() {
        return data;
    }

    /**
     * Initializes a new instance of the <see cref="DataFeedPacket"/> class
     * @param security The security whose data is held in this packet
     * @param configuration The subscription configuration that produced this data
     */
    public DataFeedPacket( final Security security, final SubscriptionDataConfig configuration ) {
        this.security = security;
        this.configuration = configuration;
        data = new ArrayList<>();
    }

    /**
     * Initializes a new instance of the <see cref="DataFeedPacket"/> class
     * @param security The security whose data is held in this packet
     * @param configuration The subscription configuration that produced this data
     * @param data The data to add to this packet. The list reference is reused
     * internally and NOT copied.
     */
    public DataFeedPacket( final Security security, final SubscriptionDataConfig configuration,
            final List<BaseData> data ) {
        this.security = security;
        this.configuration = configuration;
        this.data = data;
    }

    /**
     * Adds the specified data to this packet
     * @param data The data to be added to this packet
     */
    public void add( final BaseData data ) {
        this.data.add( data );
    }
}
