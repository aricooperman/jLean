using System;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Logging;
using QuantConnect.Securities;
using QuantConnect.Securities.Interfaces;

package com.quantconnect.lean.Lean.Engine.DataFeeds.Enumerators
{
    /**
     * Implements a wrapper around a base data enumerator to provide a final filtering step
    */
    public class SubscriptionFilterEnumerator : IEnumerator<BaseData>
    {
        /**
         * Fired when there's an error executing a user's data filter
        */
        public event EventHandler<Exception> DataFilterError;

        private final Security _security;
        private final DateTime _endTime;
        private final IEnumerator<BaseData> _enumerator;
        private final SecurityExchange _exchange;
        private final ISecurityDataFilter _dataFilter;

        /**
         * Convenience method to wrap the enumerator and attach the data filter event to log and alery users of errors
        */
         * @param resultHandler Result handler reference used to send errors
         * @param enumerator The source enumerator to be wrapped
         * @param security The security who's data is being enumerated
         * @param endTime The end time of the subscription
        @returns A new instance of the <see cref="SubscriptionFilterEnumerator"/> class that has had it's <see cref="DataFilterError"/>
         * event subscribed to to send errors to the result handler
        public static SubscriptionFilterEnumerator WrapForDataFeed(IResultHandler resultHandler, IEnumerator<BaseData> enumerator, Security security, DateTime endTime) {
            filter = new SubscriptionFilterEnumerator(enumerator, security, endTime);
            filter.DataFilterError += (sender, exception) =>
            {
                Log.Error(exception, "WrapForDataFeed");
                resultHandler.RuntimeError( "Runtime error applying data filter. Assuming filter pass: " + exception.Message, exception.StackTrace);
            };
            return filter;
        }

        /**
         * Initializes a new instance of the <see cref="SubscriptionFilterEnumerator"/> class
        */
         * @param enumerator The source enumerator to be wrapped
         * @param security The security containing an exchange and data filter
         * @param endTime The end time of the subscription
        public SubscriptionFilterEnumerator(IEnumerator<BaseData> enumerator, Security security, DateTime endTime) {
            _enumerator = enumerator;
            _security = security;
            _endTime = endTime;
            _exchange = _security.Exchange;
            _dataFilter = _security.DataFilter;
        }

        /**
         * Gets the element in the collection at the current position of the enumerator.
        */
        @returns 
         * The element in the collection at the current position of the enumerator.
         * 
        public BaseData Current
        {
            get;
            private set;
        }

        /**
         * Gets the current element in the collection.
        */
        @returns 
         * The current element in the collection.
         * 
         * <filterpriority>2</filterpriority>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /**
         * Advances the enumerator to the next element of the collection.
        */
        @returns 
         * true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
         * 
         * <exception cref="T:System.InvalidOperationException The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public boolean MoveNext() {
            while (_enumerator.MoveNext()) {
                current = _enumerator.Current;
                if( current != null ) {
                    try
                    {
                        // execute user data filters
                        if( current.DataType != MarketDataType.Auxiliary && !_dataFilter.Filter(_security, current)) {
                            continue;
                        }
                    }
                    catch (Exception err) {
                        OnDataFilterError(err);
                        continue;
                    }

                    // verify that the bar is within the exchange's market hours
                    if( current.DataType != MarketDataType.Auxiliary && !_exchange.IsOpenDuringBar(current.Time, current.EndTime, _security.IsExtendedMarketHours)) {
                        continue;
                    }

                    // make sure we haven't passed the end
                    if( current.Time > _endTime) {
                        return false;
                    }
                }

                Current = current;
                return true;
            }

            return false;
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
         * <filterpriority>2</filterpriority>
        public void Dispose() {
            _enumerator.Dispose();
        }

        /**
         * Sets the enumerator to its initial position, which is before the first element in the collection.
        */
         * <exception cref="T:System.InvalidOperationException The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public void Reset() {
            _enumerator.Reset();
        }

        /**
         * Event invocated for the <see cref="DataFilterError"/> event
        */
         * @param exception The exception that was thrown when trying to perform data filtering
        private void OnDataFilterError(Exception exception) {
            handler = DataFilterError;
            if( handler != null ) handler(this, exception);
        }
    }
}