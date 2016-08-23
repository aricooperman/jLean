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

using System;
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Enumerators
{
    /**
    /// An implementation of the <see cref="FillForwardEnumerator"/> that uses an <see cref="ITimeProvider"/>
    /// to determine if a fill forward bar needs to be emitted
    */
    public class LiveFillForwardEnumerator : FillForwardEnumerator
    {
        private final ITimeProvider _timeProvider;

        /**
        /// Initializes a new instance of the <see cref="LiveFillForwardEnumerator"/> class that accepts
        /// a reference to the fill forward resolution, useful if the fill forward resolution is dynamic
        /// and changing as the enumeration progresses
        */
         * @param timeProvider">The source of time used to gauage when this enumerator should emit extra bars when
        /// null data is returned from the source enumerator
         * @param enumerator">The source enumerator to be filled forward
         * @param exchange">The exchange used to determine when to insert fill forward data
         * @param fillForwardResolution">The resolution we'd like to receive data on
         * @param isExtendedMarketHours">True to use the exchange's extended market hours, false to use the regular market hours
         * @param subscriptionEndTime">The end time of the subscrition, once passing this date the enumerator will stop
         * @param dataResolution">The source enumerator's data resolution
        public LiveFillForwardEnumerator(ITimeProvider timeProvider, IEnumerator<BaseData> enumerator, SecurityExchange exchange, IReadOnlyRef<TimeSpan> fillForwardResolution, boolean isExtendedMarketHours, DateTime subscriptionEndTime, Duration dataResolution)
            : base(enumerator, exchange, fillForwardResolution, isExtendedMarketHours, subscriptionEndTime, dataResolution) {
            _timeProvider = timeProvider;
        }

        /**
        /// Determines whether or not fill forward is required, and if true, will produce the new fill forward data
        */
         * @param fillForwardResolution">
         * @param previous">The last piece of data emitted by this enumerator
         * @param next">The next piece of data on the source enumerator, this may be null
         * @param fillForward">When this function returns true, this will have a non-null value, null when the function returns false
        @returns True when a new fill forward piece of data was produced and should be emitted by this enumerator
        protected @Override boolean RequiresFillForwardData(TimeSpan fillForwardResolution, BaseData previous, BaseData next, out BaseData fillForward) {
            fillForward = null;
            nextExpectedDataPointTime = (previous.EndTime + fillForwardResolution);
            if( next != null ) {
                // if not future data, just return the 'next'
                if( next.EndTime <= nextExpectedDataPointTime) {
                    return false;
                }
                // next is future data, fill forward in between
                clone = previous.Clone(true);
                clone.Time = previous.Time + fillForwardResolution;
                fillForward = clone;
                return true;
            }

            // the underlying enumerator returned null, check to see if time has passed for fill fowarding
            currentLocalTime = _timeProvider.GetUtcNow().ConvertFromUtc(Exchange.TimeZone);
            if( nextExpectedDataPointTime <= currentLocalTime) {
                clone = previous.Clone(true);
                clone.Time = previous.Time + fillForwardResolution;
                fillForward = clone;
                return true;
            }

            return false;
        }
    }
}