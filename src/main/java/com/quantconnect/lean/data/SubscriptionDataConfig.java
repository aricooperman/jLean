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

import java.math.BigDecimal;
import java.time.Duration;
import java.time.ZoneId;
import java.util.HashSet;
import java.util.Objects;
import java.util.Set;

import com.quantconnect.lean.Global.DataNormalizationMode;
import com.quantconnect.lean.Global.Resolution;
import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.Global.TickType;
import com.quantconnect.lean.SecurityIdentifier;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.consolidators.IDataConsolidator;

//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using NodaTime;
//using QuantConnect.Data.Consolidators;
//using QuantConnect.Securities;

/// Subscription data required including the type of data.
public class SubscriptionDataConfig {
    
    private Symbol symbol;
    private String mappedSymbol;
    private final SecurityIdentifier sid;

    /// Type of data
    public final Class<?> type;

    /// Security type of this data subscription
    public final SecurityType securityType;

    /// Symbol of the asset we're requesting: this is really a perm tick!!
    public Symbol getSymbol() {
        return symbol;
    }

    /// Trade or quote data
    public final TickType tickType;

    /// Resolution of the asset we're requesting, second minute or tick
    public final Resolution resolution;

    /// Timespan increment between triggers of this data:
    public final Duration increment;

    /// True if wish to send old data when time gaps in data feed.
    public final boolean fillDataForward;

    /// Boolean Send Data from between 4am - 8am (Equities Setting Only)
    public final boolean extendedMarketHours;

    /// True if this subscription was added for the sole purpose of providing currency conversion rates via <see cref="CashBook.EnsureCurrencyDataFeeds"/>
    public final boolean isInternalFeed;

    /// True if this subscription is for custom user data, false for QC data
    public final boolean isCustomData;

    /// The sum of dividends accrued in this subscription, used for scaling total return prices
    public BigDecimal sumOfDividends;

    /// Gets the normalization mode used for this subscription
    public DataNormalizationMode dataNormalizationMode = DataNormalizationMode.Adjusted;

    /// Price Scaling Factor:
    public BigDecimal priceScaleFactor;

    /// Symbol Mapping: When symbols change over time (e.g. CHASE-> JPM) need to update the symbol requested.
    public String getMappedSymbol() { 
        return mappedSymbol;
    }
    
    public void setMappedSymbol( String value ) { 
        mappedSymbol = value;
        symbol = new Symbol( sid, value );
    }

    /// Gets the market / scope of the symbol
    public final String market;

    /// Gets the data time zone for this subscription
    public final ZoneId dataTimeZone;

    /// Gets the exchange time zone for this subscription
    public final ZoneId exchangeTimeZone;

    /// Consolidators that are registred with this subscription
    public final Set<IDataConsolidator> consolidators;

    /// Gets whether or not this subscription should have filters applied to it (market hours/user filters from security)
    public final boolean isFilteredSubscription;

    /// Constructor for Data Subscriptions
    /// <param name="objectType">Type of the data objects.</param>
    /// <param name="symbol">Symbol of the asset we're requesting</param>
    /// <param name="resolution">Resolution of the asset we're requesting</param>
    /// <param name="dataTimeZone">The time zone the raw data is time stamped in</param>
    /// <param name="exchangeTimeZone">Specifies the time zone of the exchange for the security this subscription is for. This
    /// is this output time zone, that is, the time zone that will be used on BaseData instances</param>
    /// <param name="fillForward">Fill in gaps with historical data</param>
    /// <param name="extendedHours">Equities only - send in data from 4am - 8pm</param>
    /// <param name="isInternalFeed">Set to true if this subscription is added for the sole purpose of providing currency conversion rates,
    /// setting this flag to true will prevent the data from being sent into the algorithm's OnData methods</param>
    /// <param name="isCustom">True if this is user supplied custom data, false for normal QC data</param>
    /// <param name="tickType">Specifies if trade or quote data is subscribed</param>
    /// <param name="isFilteredSubscription">True if this subscription should have filters applied to it (market hours/user filters from security), false otherwise</param>
    public SubscriptionDataConfig( Class<?> objectType,
            Symbol symbol,
            Resolution resolution,
            ZoneId dataTimeZone,
            ZoneId exchangeTimeZone,
            boolean fillForward,
            boolean extendedHours,
            boolean isInternalFeed ) {
        this( objectType, symbol, resolution, dataTimeZone, exchangeTimeZone, fillForward, extendedHours, isInternalFeed, false, null, true );
    }
    
    public SubscriptionDataConfig( Class<?> objectType,
            Symbol symbol,
            Resolution resolution,
            ZoneId dataTimeZone,
            ZoneId exchangeTimeZone,
            boolean fillForward,
            boolean extendedHours,
            boolean isInternalFeed,
            boolean isCustom,
            TickType tickType,
            boolean isFilteredSubscription ) {
        
        Objects.requireNonNull( objectType );
        Objects.requireNonNull( symbol );
        Objects.requireNonNull( dataTimeZone );
        Objects.requireNonNull( exchangeTimeZone );

        final SecurityIdentifier id = symbol.getId();

        this.type = objectType;
        this.securityType = id.getSecurityType();
        this.resolution = resolution;
        this.sid = id;
        this.extendedMarketHours = extendedHours;
        this.priceScaleFactor = BigDecimal.ONE;
        setMappedSymbol( symbol.getValue() );
        this.isInternalFeed = isInternalFeed;
        this.isCustomData = isCustom;
        this.market = id.getMarket();
        this.dataTimeZone = dataTimeZone;
        this.exchangeTimeZone = exchangeTimeZone;
        this.isFilteredSubscription = isFilteredSubscription;
        this.consolidators = new HashSet<IDataConsolidator>();

        if( tickType == null ) {
            tickType = TickType.Trade;
            if( securityType == SecurityType.Forex || securityType == SecurityType.Cfd || securityType == SecurityType.Option )
                tickType = TickType.Quote;
        }
        
        this.tickType = tickType;

        switch( resolution ) {
            case Tick:
                //Ticks are individual sales and fillforward doesn't apply.
                increment = Duration.ofSeconds( 0 );
                fillForward = false;
                break;
            case Second:
                increment = Duration.ofSeconds( 1 );
                break;
            case Minute:
                increment = Duration.ofMinutes( 1 );
                break;
            case Hour:
                increment = Duration.ofHours( 1 );
                break;
            case Daily:
                increment = Duration.ofDays( 1 );
                break;
            default:
                throw new IllegalArgumentException( "Unexpected Resolution: " + resolution );
        }

        this.fillDataForward = fillForward;
    }

    /// Copy constructor with @Overrides
    /// <param name="config">The config to copy, then @Overrides are applied and all option</param>
    /// <param name="objectType">Type of the data objects.</param>
    /// <param name="symbol">Symbol of the asset we're requesting</param>
    /// <param name="resolution">Resolution of the asset we're requesting</param>
    /// <param name="dataTimeZone">The time zone the raw data is time stamped in</param>
    /// <param name="exchangeTimeZone">Specifies the time zone of the exchange for the security this subscription is for. This
    /// is this output time zone, that is, the time zone that will be used on BaseData instances</param>
    /// <param name="fillForward">Fill in gaps with historical data</param>
    /// <param name="extendedHours">Equities only - send in data from 4am - 8pm</param>
    /// <param name="isInternalFeed">Set to true if this subscription is added for the sole purpose of providing currency conversion rates,
    /// setting this flag to true will prevent the data from being sent into the algorithm's OnData methods</param>
    /// <param name="isCustom">True if this is user supplied custom data, false for normal QC data</param>
    /// <param name="tickType">Specifies if trade or quote data is subscribed</param>
    /// <param name="isFilteredSubscription">True if this subscription should have filters applied to it (market hours/user filters from security), false otherwise</param>
    public SubscriptionDataConfig( SubscriptionDataConfig config ) {
        this( config, null, null, null, null, null, null, null, null, null, null, null );
    }
    
    public SubscriptionDataConfig( SubscriptionDataConfig config,
            Class<?> objectType,
            Symbol symbol,
            Resolution resolution,
            ZoneId dataTimeZone,
            ZoneId exchangeTimeZone,
            Boolean fillForward,
            Boolean extendedHours,
            Boolean isInternalFeed,
            Boolean isCustom,
            TickType tickType,
            Boolean isFilteredSubscription ) {
        
        this( objectType != null ? objectType : config.type,
              symbol != null ? symbol : config.symbol,
              resolution != null ? resolution : config.resolution,
              dataTimeZone != null ? dataTimeZone : config.dataTimeZone, 
              exchangeTimeZone != null ? exchangeTimeZone : config.exchangeTimeZone,
              fillForward != null ? fillForward : config.fillDataForward,
              extendedHours != null ? extendedHours : config.extendedMarketHours,
              isInternalFeed != null ? isInternalFeed : config.isInternalFeed,
              isCustom != null ? isCustom : config.isCustomData,
              tickType != null ? tickType : config.tickType,
              isFilteredSubscription != null ? isFilteredSubscription : config.isFilteredSubscription );
    }

    /// Normalizes the specified price based on the DataNormalizationMode
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BigDecimal getNormalizedPrice( BigDecimal price ) {
        switch( dataNormalizationMode ) {
            case Raw:
                return price;
            
            // the price scale factor will be set accordingly based on the mode in update scale factors
            case Adjusted:
            case SplitAdjusted:
                return price.multiply( priceScaleFactor );
            
            case TotalReturn:
                return (price.multiply( priceScaleFactor ) ).add( sumOfDividends );
            
            default:
                throw new IllegalArgumentException( dataNormalizationMode + " is out of range" );
        }
    }

    /// Indicates whether the current object is equal to another object of the same type.
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
    /// <param name="other">An object to compare with this object.</param>
    public boolean equals( SubscriptionDataConfig other ) {
        if( null == other ) return false;
        if( this == other ) return true;
        return sid.equals( other.sid ) && type == other.type 
            && tickType == other.tickType 
            && resolution == other.resolution
            && fillDataForward == other.fillDataForward 
            && extendedMarketHours == other.extendedMarketHours 
            && isInternalFeed == other.isInternalFeed
            && isCustomData == other.isCustomData 
            && dataTimeZone.equals( other.dataTimeZone ) 
            && exchangeTimeZone.equals( other.exchangeTimeZone )
            && isFilteredSubscription == other.isFilteredSubscription;
    }

    /// Determines whether the specified object is equal to the current object.
    /// <returns>
    /// true if the specified object  is equal to the current object; otherwise, false.
    /// </returns>
    /// <param name="obj">The object to compare with the current object. </param>
    public boolean equals( Object obj ) {
        if( null == obj ) return false;
        if( this == obj ) return true;
        if( !(obj instanceof SubscriptionDataConfig) ) return false;
        return equals( (SubscriptionDataConfig) obj );
    }

    /// Serves as the default hash function. 
    /// <returns>
    /// A hash code for the current object.
    /// </returns>
    public int hashCode() {
        int hashCode = sid.hashCode();
        hashCode = (hashCode*397) ^ type.hashCode();
        hashCode = (hashCode*397) ^ tickType.hashCode();
        hashCode = (hashCode*397) ^ resolution.hashCode();
        hashCode = (hashCode*397) ^ Boolean.hashCode( fillDataForward );
        hashCode = (hashCode*397) ^ Boolean.hashCode( extendedMarketHours );
        hashCode = (hashCode*397) ^ Boolean.hashCode( isInternalFeed );
        hashCode = (hashCode*397) ^ Boolean.hashCode( isCustomData );
        hashCode = (hashCode*397) ^ dataTimeZone.hashCode();
        hashCode = (hashCode*397) ^ exchangeTimeZone.hashCode();
        hashCode = (hashCode*397) ^ Boolean.hashCode( isFilteredSubscription );
        return hashCode;
    }

//    /// Override equals operator
//    public static boolean operator ==(SubscriptionDataConfig left, SubscriptionDataConfig right)
//    {
//        return Equals(left, right);
//    }
//
//    /// <summary>
//    /// Override not equals operator
//    /// </summary>
//    public static boolean operator !=(SubscriptionDataConfig left, SubscriptionDataConfig right)
//    {
//        return !Equals(left, right);
//    }

    /// Returns a String that represents the current object.
    /// <returns>
    /// A String that represents the current object.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public String toString() {
        return symbol.toString() + "," + getMappedSymbol() + "," + resolution;
    }
}
