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
using System.Diagnostics;
using QuantConnect.Data;

package com.quantconnect.lean.Indicators
{
    /**
     * Provides a base type for all indicators
    */
     * <typeparam name="T The type of data input into this indicator</typeparam>
    [DebuggerDisplay( "{ToDetailedString()}")]
    public abstract partial class IndicatorBase<T> : IIndicator<T>
        where T : BaseData
    {
        /**the most recent input that was given to this indicator</summary>
        private T _previousInput;

        /**
         * Event handler that fires after this indicator is updated
        */
        public event IndicatorUpdatedHandler Updated;

        /**
         * Initializes a new instance of the Indicator class using the specified name.
        */
         * @param name The name of this indicator
        protected IndicatorBase( String name) {
            Name = name;
            Current = new IndicatorDataPoint(DateTime.MinValue, BigDecimal.ZERO);
        }

        /**
         * Gets a name for this indicator
        */
        public String Name { get; private set; }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public abstract boolean IsReady { get; }

        /**
         * Gets the current state of this indicator. If the state has not been updated
         * then the time on the value will equal DateTime.MinValue.
        */
        public IndicatorDataPoint Current { get; protected set; }

        /**
         * Gets the number of samples processed by this indicator
        */
        public long Samples { get; private set; }

        /**
         * Updates the state of this indicator with the given value and returns true
         * if this indicator is ready, false otherwise
        */
         * @param input The value to use to update this indicator
        @returns True if this indicator is ready, false otherwise
        public boolean Update(T input) {
            if( _previousInput != null && input.Time < _previousInput.Time) {
                // if we receive a time in the past, throw
                throw new IllegalArgumentException( String.format( "This is a forward only indicator: %1$s Input: %2$s Previous: %3$s", Name, input.Time.toString( "u"), _previousInput.Time.toString( "u")));
            }
            if( !ReferenceEquals(input, _previousInput)) {
                // compute a new value and update our previous time
                Samples++;
                _previousInput = input;

                nextResult = ValidateAndComputeNextValue(input);
                if( nextResult.Status == IndicatorStatus.Success) {
                    Current = new IndicatorDataPoint(input.Time, nextResult.Value);

                    // let others know we've produced a new data point
                    OnUpdated(Current);
                }
            }
            return IsReady;
        }

        /**
         * Resets this indicator to its initial state
        */
        public void Reset() {
            Samples = 0;
            _previousInput = null;
            Current = new IndicatorDataPoint(DateTime.MinValue, default(decimal));
        }

        /**
         * Compares the current object with another object of the same type.
        */
        @returns 
         * A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
         * 
         * @param other An object to compare with this object.
        public int CompareTo(IIndicator<T> other) {
            if( ReferenceEquals(other, null )) {
                // everything is greater than null via MSDN
                return 1;
            }

            return Current.CompareTo(other.Current);
        }

        /**
         * Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        */
        @returns 
         * A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj"/> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj"/>. Greater than zero This instance follows <paramref name="obj"/> in the sort order. 
         * 
         * @param obj An object to compare with this instance. <exception cref="T:System.ArgumentException"><paramref name="obj"/> is not the same type as this instance. </exception><filterpriority>2</filterpriority>
        public int CompareTo(object obj) {
            other = obj as IndicatorBase<T>;
            if( other == null ) {
                throw new IllegalArgumentException( "Object must be of type " + GetType().GetBetterTypeName());
            }

            return CompareTo(other);
        }

        /**
         * Determines whether the specified object is equal to the current object.
        */
        @returns 
         * true if the specified object  is equal to the current object; otherwise, false.
         * 
         * @param obj The object to compare with the current object. 
        public @Override boolean Equals(object obj) {
            // this implementation acts as a liason to prevent inconsistency between the operators
            // == and != against primitive types. the core impl for equals between two indicators
            // is still reference equality, however, when comparing value types (floats/int, ect..)
            // we'll use value type semantics on Current.Value
            // because of this, we shouldn't need to @Override GetHashCode as well since we're still
            // solely relying on reference semantics (think hashset/dictionary impls)

            if( ReferenceEquals(obj, null )) return false;
            if( obj.GetType().IsSubclassOf(typeof (IndicatorBase<>))) return ReferenceEquals(this, obj);

            // the obj is not an indicator, so let's check for value types, try converting to decimal
            converted = new BigDecimal( obj);
            return Current.Value == converted;
        }

        /**
         * toString Overload for Indicator Base
        */
        @returns String representation of the indicator
        public @Override String toString() {
            return Current.Value.toString( "#######0.0####");
        }

        /**
         * Provides a more detailed String of this indicator in the form of {Name} - {Value}
        */
        @returns A detailed String of this indicator's current state
        public String ToDetailedString() {
            return String.format( "%1$s - %2$s", Name, this);
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected abstract BigDecimal ComputeNextValue(T input);

        /**
         * Computes the next value of this indicator from the given state
         * and returns an instance of the <see cref="IndicatorResult"/> class
        */
         * @param input The input given to the indicator
        @returns An IndicatorResult object including the status of the indicator
        protected IndicatorResult ValidateAndComputeNextValue(T input) {
            // default implementation always returns IndicatorStatus.Success
            return new IndicatorResult(ComputeNextValue(input));
        }

        /**
         * Event invocator for the Updated event
        */
         * @param consolidated This is the new piece of data produced by this indicator
        protected void OnUpdated(IndicatorDataPoint consolidated) {
            handler = Updated;
            if( handler != null ) handler(this, consolidated);
        }
    }
}