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

namespace QuantConnect.Data
{
    /// <summary>
    /// Represents the source location and transport medium for a subscription
    /// </summary>
    public class SubscriptionDataSource : IEquatable<SubscriptionDataSource>
    {
        /// <summary>
        /// Identifies where to get the subscription's data from
        /// </summary>
        public readonly String Source;

        /// <summary>
        /// Identifies the format of the data within the source
        /// </summary>
        public readonly FileFormat Format;

        /// <summary>
        /// Identifies the transport medium used to access the data, such as a local or remote file, or a polling rest API
        /// </summary>
        public readonly SubscriptionTransportMedium TransportMedium;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionDataSource"/> class.
        /// </summary>
        /// <param name="source">The subscription's data source location</param>
        /// <param name="transportMedium">The transport medium to be used to retrieve the subscription's data from the source</param>
        public SubscriptionDataSource( String source, SubscriptionTransportMedium transportMedium)
        {
            Source = source;
            Format = FileFormat.Csv;
            TransportMedium = transportMedium;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionDataSource"/> class.
        /// </summary>
        /// <param name="source">The subscription's data source location</param>
        /// <param name="format">The format of the data within the source</param>
        /// <param name="transportMedium">The transport medium to be used to retrieve the subscription's data from the source</param>
        public SubscriptionDataSource( String source, SubscriptionTransportMedium transportMedium, FileFormat format)
        {
            Source = source;
            Format = format;
            TransportMedium = transportMedium;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public boolean Equals(SubscriptionDataSource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Source, other.Source) && TransportMedium == other.TransportMedium;
        }

        /// <summary>
        /// Determines whether the specified instance is equal to the current instance.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override boolean Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SubscriptionDataSource) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0)*397) ^ (int) TransportMedium;
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="left">The <see cref="SubscriptionDataSource"/> instance on the left of the operator</param>
        /// <param name="right">The <see cref="SubscriptionDataSource"/> instance on the right of the operator</param>
        /// <returns>True if the two instances are considered equal, false otherwise</returns>
        public static boolean operator ==(SubscriptionDataSource left, SubscriptionDataSource right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Indicates whether the current object is not equal to another object of the same type.
        /// </summary>
        /// <param name="left">The <see cref="SubscriptionDataSource"/> instance on the left of the operator</param>
        /// <param name="right">The <see cref="SubscriptionDataSource"/> instance on the right of the operator</param>
        /// <returns>True if the two instances are not considered equal, false otherwise</returns>
        public static boolean operator !=(SubscriptionDataSource left, SubscriptionDataSource right)
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
            return string.Format("{0}: {1} {2}", TransportMedium, Format, Source);
        }
    }
}