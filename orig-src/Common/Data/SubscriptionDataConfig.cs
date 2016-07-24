﻿/*
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NodaTime;
using QuantConnect.Data.Consolidators;
using QuantConnect.Securities;

namespace QuantConnect.Data
{
    /// <summary>
    /// Subscription data required including the type of data.
    /// </summary>
    public class SubscriptionDataConfig : IEquatable<SubscriptionDataConfig>
    {
        private Symbol _symbol;
        private String _mappedSymbol;
        private readonly SecurityIdentifier _sid;

        /// <summary>
        /// Type of data
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// Security type of this data subscription
        /// </summary>
        public readonly SecurityType SecurityType;

        /// <summary>
        /// Symbol of the asset we're requesting: this is really a perm tick!!
        /// </summary>
        public Symbol Symbol
        {
            get { return _symbol; }
        }

        /// <summary>
        /// Trade or quote data
        /// </summary>
        public readonly TickType TickType;

        /// <summary>
        /// Resolution of the asset we're requesting, second minute or tick
        /// </summary>
        public readonly Resolution Resolution;

        /// <summary>
        /// Timespan increment between triggers of this data:
        /// </summary>
        public readonly TimeSpan Increment;

        /// <summary>
        /// True if wish to send old data when time gaps in data feed.
        /// </summary>
        public readonly boolean FillDataForward;

        /// <summary>
        /// Boolean Send Data from between 4am - 8am (Equities Setting Only)
        /// </summary>
        public readonly boolean ExtendedMarketHours;

        /// <summary>
        /// True if this subscription was added for the sole purpose of providing currency conversion rates via <see cref="CashBook.EnsureCurrencyDataFeeds"/>
        /// </summary>
        public readonly boolean IsInternalFeed;

        /// <summary>
        /// True if this subscription is for custom user data, false for QC data
        /// </summary>
        public readonly boolean IsCustomData;

        /// <summary>
        /// The sum of dividends accrued in this subscription, used for scaling total return prices
        /// </summary>
        public BigDecimal SumOfDividends;

        /// <summary>
        /// Gets the normalization mode used for this subscription
        /// </summary>
        public DataNormalizationMode DataNormalizationMode = DataNormalizationMode.Adjusted;

        /// <summary>
        /// Price Scaling Factor:
        /// </summary>
        public BigDecimal PriceScaleFactor;

        /// <summary>
        /// Symbol Mapping: When symbols change over time (e.g. CHASE-> JPM) need to update the symbol requested.
        /// </summary>
        public String MappedSymbol
        {
            get { return _mappedSymbol; }
            set
            {
                _mappedSymbol = value;
                _symbol = new Symbol(_sid, value);
            }
        }

        /// <summary>
        /// Gets the market / scope of the symbol
        /// </summary>
        public readonly String Market;

        /// <summary>
        /// Gets the data time zone for this subscription
        /// </summary>
        public readonly DateTimeZone DataTimeZone;

        /// <summary>
        /// Gets the exchange time zone for this subscription
        /// </summary>
        public readonly DateTimeZone ExchangeTimeZone;

        /// <summary>
        /// Consolidators that are registred with this subscription
        /// </summary>
        public readonly HashSet<IDataConsolidator> Consolidators;

        /// <summary>
        /// Gets whether or not this subscription should have filters applied to it (market hours/user filters from security)
        /// </summary>
        public readonly boolean IsFilteredSubscription;

        /// <summary>
        /// Constructor for Data Subscriptions
        /// </summary>
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
        public SubscriptionDataConfig(Type objectType,
            Symbol symbol,
            Resolution resolution,
            DateTimeZone dataTimeZone,
            DateTimeZone exchangeTimeZone,
            boolean fillForward,
            boolean extendedHours,
            boolean isInternalFeed,
            boolean isCustom = false,
            TickType? tickType = null,
            boolean isFilteredSubscription = true)
        {
            if (objectType == null) throw new ArgumentNullException("objectType");
            if (symbol == null) throw new ArgumentNullException("symbol");
            if (dataTimeZone == null) throw new ArgumentNullException("dataTimeZone");
            if (exchangeTimeZone == null) throw new ArgumentNullException("exchangeTimeZone");

            Type = objectType;
            SecurityType = symbol.ID.SecurityType;
            Resolution = resolution;
            _sid = symbol.ID;
            FillDataForward = fillForward;
            ExtendedMarketHours = extendedHours;
            PriceScaleFactor = 1;
            MappedSymbol = symbol.Value;
            IsInternalFeed = isInternalFeed;
            IsCustomData = isCustom;
            Market = symbol.ID.Market;
            DataTimeZone = dataTimeZone;
            ExchangeTimeZone = exchangeTimeZone;
            IsFilteredSubscription = isFilteredSubscription;
            Consolidators = new HashSet<IDataConsolidator>();

            if (!tickType.HasValue)
            {
                TickType = TickType.Trade;
                if (SecurityType == SecurityType.Forex || SecurityType == SecurityType.Cfd || SecurityType == SecurityType.Option)
                {
                    TickType = TickType.Quote;
                }
            }
            else
            {
                TickType = tickType.Value;
            }

            switch (resolution)
            {
                case Resolution.Tick:
                    //Ticks are individual sales and fillforward doesn't apply.
                    Increment = TimeSpan.FromSeconds(0);
                    FillDataForward = false;
                    break;
                case Resolution.Second:
                    Increment = TimeSpan.FromSeconds(1);
                    break;
                case Resolution.Minute:
                    Increment = TimeSpan.FromMinutes(1);
                    break;
                case Resolution.Hour:
                    Increment = TimeSpan.FromHours(1);
                    break;
                case Resolution.Daily:
                    Increment = TimeSpan.FromDays(1);
                    break;
                default:
                    throw new InvalidEnumArgumentException("Unexpected Resolution: " + resolution);
            }
        }

        /// <summary>
        /// Copy constructor with overrides
        /// </summary>
        /// <param name="config">The config to copy, then overrides are applied and all option</param>
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
        public SubscriptionDataConfig(SubscriptionDataConfig config,
            Type objectType = null,
            Symbol symbol = null,
            Resolution? resolution = null,
            DateTimeZone dataTimeZone = null,
            DateTimeZone exchangeTimeZone = null,
            bool? fillForward = null,
            bool? extendedHours = null,
            bool? isInternalFeed = null,
            bool? isCustom = null,
            TickType? tickType = null,
            bool? isFilteredSubscription = null)
            : this(
            objectType ?? config.Type,
            symbol ?? config.Symbol,
            resolution ?? config.Resolution,
            dataTimeZone ?? config.DataTimeZone, 
            exchangeTimeZone ?? config.ExchangeTimeZone,
            fillForward ?? config.FillDataForward,
            extendedHours ?? config.ExtendedMarketHours,
            isInternalFeed ?? config.IsInternalFeed,
            isCustom ?? config.IsCustomData,
            tickType ?? config.TickType,
            isFilteredSubscription ?? config.IsFilteredSubscription
            )
        {
        }

        /// <summary>
        /// Normalizes the specified price based on the DataNormalizationMode
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BigDecimal GetNormalizedPrice(decimal price)
        {
            switch (DataNormalizationMode)
            {
                case DataNormalizationMode.Raw:
                    return price;
                
                // the price scale factor will be set accordingly based on the mode in update scale factors
                case DataNormalizationMode.Adjusted:
                case DataNormalizationMode.SplitAdjusted:
                    return price*PriceScaleFactor;
                
                case DataNormalizationMode.TotalReturn:
                    return (price*PriceScaleFactor) + SumOfDividends;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public boolean Equals(SubscriptionDataConfig other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _sid.Equals(other._sid) && Type == other.Type 
                && TickType == other.TickType 
                && Resolution == other.Resolution
                && FillDataForward == other.FillDataForward 
                && ExtendedMarketHours == other.ExtendedMarketHours 
                && IsInternalFeed == other.IsInternalFeed
                && IsCustomData == other.IsCustomData 
                && DataTimeZone.Equals(other.DataTimeZone) 
                && ExchangeTimeZone.Equals(other.ExchangeTimeZone)
                && IsFilteredSubscription == other.IsFilteredSubscription;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override boolean Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SubscriptionDataConfig) obj);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                hashCode = _sid.GetHashCode();
                hashCode = (hashCode*397) ^ Type.GetHashCode();
                hashCode = (hashCode*397) ^ (int) TickType;
                hashCode = (hashCode*397) ^ (int) Resolution;
                hashCode = (hashCode*397) ^ FillDataForward.GetHashCode();
                hashCode = (hashCode*397) ^ ExtendedMarketHours.GetHashCode();
                hashCode = (hashCode*397) ^ IsInternalFeed.GetHashCode();
                hashCode = (hashCode*397) ^ IsCustomData.GetHashCode();
                hashCode = (hashCode*397) ^ DataTimeZone.GetHashCode();
                hashCode = (hashCode*397) ^ ExchangeTimeZone.GetHashCode();
                hashCode = (hashCode*397) ^ IsFilteredSubscription.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Override equals operator
        /// </summary>
        public static boolean operator ==(SubscriptionDataConfig left, SubscriptionDataConfig right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Override not equals operator
        /// </summary>
        public static boolean operator !=(SubscriptionDataConfig left, SubscriptionDataConfig right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns a String that represents the current object.
        /// </summary>
        /// <returns>
        /// A String that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override String ToString()
        {
            return Symbol.ToString() + "," + MappedSymbol + "," + Resolution;
        }
    }
}
