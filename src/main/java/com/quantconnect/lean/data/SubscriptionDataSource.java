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

import java.nio.file.Path;

import com.quantconnect.lean.Global.SubscriptionTransportMedium;
import com.quantconnect.lean.data.FileFormat;

/// Represents the source location and transport medium for a subscription
public class SubscriptionDataSource {

    /// Identifies where to get the subscription's data from
    public final Path source;

    /// Identifies the format of the data within the source
    public final FileFormat format;

    /// Identifies the transport medium used to access the data, such as a local or remote file, or a polling rest API
    public final SubscriptionTransportMedium transportMedium;

    /// Initializes a new instance of the <see cref="SubscriptionDataSource"/> class.
     * @param source">The subscription's data source location
     * @param transportMedium">The transport medium to be used to retrieve the subscription's data from the source
    public SubscriptionDataSource( Path source, SubscriptionTransportMedium transportMedium ) {
        this( source, transportMedium, FileFormat.Csv );
    }

    /// Initializes a new instance of the <see cref="SubscriptionDataSource"/> class.
     * @param source">The subscription's data source location
     * @param format">The format of the data within the source
     * @param transportMedium">The transport medium to be used to retrieve the subscription's data from the source
    public SubscriptionDataSource( Path source, SubscriptionTransportMedium transportMedium, FileFormat format ) {
        this.source = source;
        this.format = format;
        this.transportMedium = transportMedium;
    }

    /// Indicates whether the current object is equal to another object of the same type.
    @returns 
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// 
     * @param other">An object to compare with this object.
    public boolean equals( SubscriptionDataSource other)  {
        if( null == other ) return false;
        if( this == other ) return true;
        return source.equals( other.source ) && transportMedium == other.transportMedium;
    }

    /// Determines whether the specified instance is equal to the current instance.
    @returns 
    /// true if the specified object  is equal to the current object; otherwise, false.
    /// 
     * @param obj">The object to compare with the current object. <filterpriority>2</filterpriority>
    public boolean equals( Object obj ) {
        if( null == obj ) return false;
        if( this == obj ) return true;
        if( !(obj instanceof SubscriptionDataSource) ) return false;
        return equals( (SubscriptionDataSource)obj );
    }

    /// Serves as a hash function for a particular type. 
    @returns 
    /// A hash code for the current <see cref="T:System.Object"/>.
    /// 
    /// <filterpriority>2</filterpriority>
    public int hashCode() {
        return ((source != null ? source.hashCode() : 0)*397) ^ transportMedium.hashCode();
    }

//    /**
//    /// Indicates whether the current object is equal to another object of the same type.
//    */
//     * @param left">The <see cref="SubscriptionDataSource"/> instance on the left of the operator
//     * @param right">The <see cref="SubscriptionDataSource"/> instance on the right of the operator
//    @returns True if the two instances are considered equal, false otherwise
//    public static boolean operator ==(SubscriptionDataSource left, SubscriptionDataSource right)
//    {
//        return Equals(left, right);
//    }
//
//    /**
//    /// Indicates whether the current object is not equal to another object of the same type.
//    */
//     * @param left">The <see cref="SubscriptionDataSource"/> instance on the left of the operator
//     * @param right">The <see cref="SubscriptionDataSource"/> instance on the right of the operator
//    @returns True if the two instances are not considered equal, false otherwise
//    public static boolean operator !=(SubscriptionDataSource left, SubscriptionDataSource right)
//    {
//        return !Equals(left, right);
//    }

    /// Returns a String that represents the current object.
    @returns 
    /// A String that represents the current object.
    /// 
    /// <filterpriority>2</filterpriority>
    public String toString() {
        return String.format( "%s: %s %s", transportMedium, format, source );
    }
}
