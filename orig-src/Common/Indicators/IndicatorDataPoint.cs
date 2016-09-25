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
using QuantConnect.Data;

package com.quantconnect.lean.Indicators
{
    /**
     * Represents a piece of data at a specific time
    */
    public class IndicatorDataPoint : BaseData, IEquatable<IndicatorDataPoint>, IComparable<IndicatorDataPoint>, IComparable
    {
        /**
         * Initializes a new default instance of IndicatorDataPoint with a time of
         * DateTime.MinValue and a Value of BigDecimal.ZERO.
        */
        public IndicatorDataPoint() {
            Value = BigDecimal.ZERO;
            Time = DateTime.MinValue;
        }

        /**
         * Initializes a new instance of the DataPoint type using the specified time/data
        */
         * @param time The time this data was produced
         * @param value The data
        public IndicatorDataPoint(DateTime time, BigDecimal value) {
            Time = time;
            Value = value;
        }

        /**
         * Initializes a new instance of the DataPoint type using the specified time/data
        */
         * @param symbol The symbol associated with this data
         * @param time The time this data was produced
         * @param value The data
        public IndicatorDataPoint(Symbol symbol, DateTime time, BigDecimal value) {
            Symbol = symbol;
            Time = time;
            Value = value;
        }

        /**
         * Indicates whether the current object is equal to another object of the same type.
        */
        @returns 
         * true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
         * 
         * @param other An object to compare with this object.
        public boolean Equals(IndicatorDataPoint other) {
            if( other == null ) {
                return false;
            }
            return other.Time == Time && other.Value == Value;
        }

        /**
         * Compares the current object with another object of the same type.
        */
        @returns 
         * A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
         * 
         * @param other An object to compare with this object.
        public int CompareTo(IndicatorDataPoint other) {
            if( ReferenceEquals(other, null )) {
                // everything is greater than null via MSDN
                return 1;
            }
            return Value.CompareTo(other.Value);
        }

        /**
         * Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        */
        @returns 
         * A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj"/> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj"/>. Greater than zero This instance follows <paramref name="obj"/> in the sort order. 
         * 
         * @param obj An object to compare with this instance. <exception cref="T:System.ArgumentException"><paramref name="obj"/> is not the same type as this instance. </exception><filterpriority>2</filterpriority>
        public int CompareTo(object obj) {
            other = obj as IndicatorDataPoint;
            if( other == null ) {
                throw new IllegalArgumentException( "Object must be of type " + GetType().GetBetterTypeName());
            }
            return CompareTo(other);
        }

        /**
         * Returns a String representation of this DataPoint instance using ISO8601 formatting for the date
        */
        @returns 
         * A <see cref="T:System.String" /> containing a fully qualified type name.
         * 
         * <filterpriority>2</filterpriority>
        public @Override String toString() {
            return String.format( "%1$s - %2$s", Time.toString( "s"), Value);
        }

        /**
         * Indicates whether this instance and a specified object are equal.
        */
        @returns 
         * true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.
         * 
         * @param obj Another object to compare to. 
         * <filterpriority>2</filterpriority>
        public @Override boolean Equals(object obj) {
            if( null == obj ) return false;
            return obj is IndicatorDataPoint && Equals((IndicatorDataPoint) obj);
        }

        /**
         * Returns the hash code for this instance.
        */
        @returns 
         * A 32-bit signed integer that is the hash code for this instance.
         * 
         * <filterpriority>2</filterpriority>
        public @Override int hashCode() {
            unchecked
            {
                return (Value.hashCode()*397) ^ Time.hashCode();
            }
        }

        /**
         * Returns the data held within the instance
        */
         * @param instance The DataPoint instance
        @returns The data held within the instance
        public static implicit operator decimal(IndicatorDataPoint instance) {
            return instance.Value;
        }

        /**
         * This function is purposefully not implemented.
        */
        public @Override BaseData Reader(SubscriptionDataConfig config, String line, DateTime date, boolean isLiveMode) {
            throw new UnsupportedOperationException( "IndicatorDataPoint does not support the Reader function. This function should never be called on this type.");
        }

        /**
         * This function is purposefully not implemented.
        */
        public @Override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, boolean isLiveMode) {
            throw new UnsupportedOperationException( "IndicatorDataPoint does not support the GetSource function. This function should never be called on this type.");
        }
    }
}