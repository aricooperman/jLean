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

package com.quantconnect.lean.data;

import java.time.ZoneId;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

import com.quantconnect.lean.Resolution;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.TimeKeeper;
import com.quantconnect.lean.data.consolidators.IDataConsolidator;
import com.quantconnect.lean.data.market.Tick;
import com.quantconnect.lean.data.market.TradeBar;


 * Enumerable Subscription Management Class
public class SubscriptionManager {
    
    private final TimeKeeper timeKeeper;

     * Generic Market Data Requested and Object[] Arguments to Get it:
    private final List<SubscriptionDataConfig> subscriptions;

     * Initialise the Generic Data Manager Class
     * @param timeKeeper The algoritm's time keeper
    public SubscriptionManager( TimeKeeper timeKeeper ) {
        this.timeKeeper = timeKeeper;
        //Generic Type Data Holder:
        this.subscriptions = new ArrayList<SubscriptionDataConfig>();
    }

     * Get the count of assets:
    public int getCount() {
        return subscriptions.size(); 
    }

     * Add Market Data Required (Overloaded method for backwards compatibility).
     * @param symbol Symbol of the asset we're like
     * @param resolution Resolution of Asset Required
     * @param timeZone The time zone the subscription's data is time stamped in
     * @param exchangeTimeZone Specifies the time zone of the exchange for the security this subscription is for. This
     * is this output time zone, that is, the time zone that will be used on BaseData instances
     * @param isCustomData True if this is custom user supplied data, false for normal QC data
     * @param fillDataForward when there is no data pass the last tradebar forward
     * @param extendedMarketHours Request premarket data as well when true 
    @returns The newly created <see cref="SubscriptionDataConfig"/>
    public SubscriptionDataConfig add( Symbol symbol, Resolution resolution, ZoneId timeZone, ZoneId exchangeTimeZone ) {
        return add( symbol, resolution, timeZone, exchangeTimeZone, false, true, false );
    }

    public SubscriptionDataConfig add( Symbol symbol, Resolution resolution, ZoneId timeZone, ZoneId exchangeTimeZone, 
            boolean isCustomData, boolean fillDataForward, boolean extendedMarketHours ) {
        //Set the type: market data only comes in two forms -- ticks(trade by trade) or tradebar(time summaries)
        
        Class<? extends BaseData> dataType = TradeBar.class;
        if( resolution == Resolution.Tick ) 
            dataType = Tick.class;

        return add( dataType, symbol, resolution, timeZone, exchangeTimeZone, isCustomData, fillDataForward, extendedMarketHours, false, true );
    }

     * Add Market Data Required - generic data typing support as long as Type implements BaseData.
     * @param dataType Set the type of the data we're subscribing to.
     * @param symbol Symbol of the asset we're like
     * @param resolution Resolution of Asset Required
     * @param dataTimeZone The time zone the subscription's data is time stamped in
     * @param exchangeTimeZone Specifies the time zone of the exchange for the security this subscription is for. This
     * is this output time zone, that is, the time zone that will be used on BaseData instances
     * @param isCustomData True if this is custom user supplied data, false for normal QC data
     * @param fillDataForward when there is no data pass the last tradebar forward
     * @param extendedMarketHours Request premarket data as well when true 
     * @param isInternalFeed Set to true to prevent data from this subscription from being sent into the algorithm's OnData events
     * @param isFilteredSubscription True if this subscription should have filters applied to it (market hours/user filters from security), false otherwise
    @returns The newly created <see cref="SubscriptionDataConfig"/>
    public SubscriptionDataConfig add( Class<? extends BaseData> dataType, Symbol symbol, Resolution resolution, ZoneId dataTimeZone, ZoneId exchangeTimeZone, 
            boolean isCustomData ) {
        return add( dataType, symbol, resolution, dataTimeZone, exchangeTimeZone, isCustomData, true, false, false, true );
    }

    public SubscriptionDataConfig add( Class<? extends BaseData> dataType, Symbol symbol, Resolution resolution, ZoneId dataTimeZone, ZoneId exchangeTimeZone, 
            boolean isCustomData, boolean fillDataForward, boolean extendedMarketHours, boolean isInternalFeed, boolean isFilteredSubscription ) {
        Objects.requireNonNull( dataTimeZone, 
                "DataTimeZone is a required parameter for new subscriptions.  Set to the time zone the raw data is time stamped in." );

        if( exchangeTimeZone == null )
            throw new NullPointerException( "ExchangeTimeZone is a required parameter for new subscriptions.  Set to the time zone the security exchange resides in." );
        
        //Create:
        final SubscriptionDataConfig newConfig = new SubscriptionDataConfig( dataType, symbol, resolution, dataTimeZone, exchangeTimeZone, fillDataForward, extendedMarketHours, 
                isInternalFeed, isCustomData, null, isFilteredSubscription );

        //Add to subscription list: make sure we don't have his symbol:
        subscriptions.add( newConfig );

        // add the time zone to our time keeper
        timeKeeper.addTimeZone( exchangeTimeZone );

        return newConfig;
    }

     * Add a consolidator for the symbol
     * @param symbol Symbol of the asset to consolidate
     * @param consolidator The consolidator
    public void addConsolidator( Symbol symbol, IDataConsolidator consolidator ) {
        //Find the right subscription and add the consolidator to it
        for( int i = 0; i < subscriptions.size(); i++ ) {
            if( subscriptions.get( i ).getSymbol().equals( symbol ) ) {
                // we need to be able to pipe data directly from the data feed into the consolidator
                if( !consolidator.getInputType().isAssignableFrom( subscriptions.get( i ).type ) ) {
                    throw new IllegalArgumentException( String.format( "Type mismatch found between consolidator and symbol. " +
                        "Symbol: %1$s expects type %2$s but tried to register consolidator with input type %3$s", 
                        symbol, subscriptions.get( i ).type.getName(), consolidator.getInputType().getName() ) );
                }
                
                subscriptions.get( i ).consolidators.add( consolidator );
                return;
            }
        }

        //If we made it here it is because we never found the symbol in the subscription list
        throw new IllegalArgumentException( "Please subscribe to this symbol before adding a consolidator for it. Symbol: " + symbol.toString() );
    }

} // End Algorithm MetaData Manager Class
