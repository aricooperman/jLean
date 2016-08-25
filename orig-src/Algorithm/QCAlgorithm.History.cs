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
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Algorithm
{
    public partial class QCAlgorithm
    {
        /**
         * Gets or sets the history provider for the algorithm
        */
        public IHistoryProvider HistoryProvider
        {
            get;
            set;
        }

        /**
         * Gets whether or not this algorithm is still warming up
        */
        public boolean IsWarmingUp
        {
            get;
            private set;
        }

        /**
         * Sets the warm up period to the specified value
        */
         * @param timeSpan The amount of time to warm up, this does not take into account market hours/weekends
        public void SetWarmup(TimeSpan timeSpan) {
            _warmupBarCount = null;
            _warmupTimeSpan = timeSpan;
        }

        /**
         * Sets the warm up period to the specified value
        */
         * @param timeSpan The amount of time to warm up, this does not take into account market hours/weekends
        public void SetWarmUp(TimeSpan timeSpan) {
            SetWarmup(timeSpan);
        }

        /**
         * Sets the warm up period by resolving a start date that would send that amount of data into
         * the algorithm. The highest (smallest) resolution in the securities collection will be used.
         * For example, if an algorithm has minute and daily data and 200 bars are requested, that would
         * use 200 minute bars.
        */
         * @param barCount The number of data points requested for warm up
        public void SetWarmup(int barCount) {
            _warmupTimeSpan = null;
            _warmupBarCount = barCount;
        }

        /**
         * Sets the warm up period by resolving a start date that would send that amount of data into
         * the algorithm. The highest (smallest) resolution in the securities collection will be used.
         * For example, if an algorithm has minute and daily data and 200 bars are requested, that would
         * use 200 minute bars.
        */
         * @param barCount The number of data points requested for warm up
        public void SetWarmUp(int barCount) {
            SetWarmup(barCount);
        }

        /**
         * Sets <see cref="IAlgorithm.IsWarmingUp"/> to false to indicate this algorithm has finished its warm up
        */
        public void SetFinishedWarmingUp() {
            IsWarmingUp = false;
        }

        /**
         * Gets the history requests required for provide warm up data for the algorithm
        */
        @returns 
        public IEnumerable<HistoryRequest> GetWarmupHistoryRequests() {
            if( _warmupBarCount.HasValue) {
                return CreateBarCountHistoryRequests(Securities.Keys, _warmupBarCount.Value);
            }
            if( _warmupTimeSpan.HasValue) {
                end = UtcTime.ConvertFromUtc(TimeZone);
                return CreateDateRangeHistoryRequests(Securities.Keys, end - _warmupTimeSpan.Value, end);
            }

            // if not warmup requested return nothing
            return Enumerable.Empty<HistoryRequest>();
        }

        /**
         * Get the history for all configured securities over the requested span.
         * This will use the resolution and other subscription settings for each security.
         * The symbols must exist in the Securities collection.
        */
         * @param span The span over which to request data. This is a calendar span, so take into consideration weekends and such
         * @param resolution The resolution to request
        @returns An enumerable of slice containing data over the most recent span for all configured securities
        public IEnumerable<Slice> History(TimeSpan span, Resolution? resolution = null ) {
            return History(Securities.Keys, Time - span, Time, resolution).Memoize();
        }

        /**
         * Get the history for all configured securities over the requested span.
         * This will use the resolution and other subscription settings for each security.
         * The symbols must exist in the Securities collection.
        */
         * @param periods The number of bars to request
         * @param resolution The resolution to request
        @returns An enumerable of slice containing data over the most recent span for all configured securities
        public IEnumerable<Slice> History(int periods, Resolution? resolution = null ) {
            return History(Securities.Keys, periods, resolution).Memoize();
        }

        /**
         * Gets the historical data for all symbols of the requested type over the requested span.
         * The symbol's configured values for resolution and fill forward behavior will be used
         * The symbols must exist in the Securities collection.
        */
         * @param span The span over which to retrieve recent historical data
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<DataMap<T>> History<T>(TimeSpan span, Resolution? resolution = null )
            where T : BaseData
        {
            return History<T>(Securities.Keys, span, resolution).Memoize();
        }

        /**
         * Gets the historical data for the specified symbols over the requested span.
         * The symbols must exist in the Securities collection.
        */
         * <typeparam name="T The data type of the symbols</typeparam>
         * @param symbols The symbols to retrieve historical data for
         * @param span The span over which to retrieve recent historical data
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<DataMap<T>> History<T>(IEnumerable<Symbol> symbols, Duration span, Resolution? resolution = null )
            where T : BaseData
        {
            return History<T>(symbols, Time - span, Time, resolution).Memoize();
        }

        /**
         * Gets the historical data for the specified symbols. The exact number of bars will be returned for
         * each symbol. This may result in some data start earlier/later than others due to when various
         * exchanges are open. The symbols must exist in the Securities collection.
        */
         * <typeparam name="T The data type of the symbols</typeparam>
         * @param symbols The symbols to retrieve historical data for
         * @param periods The number of bars to request
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<DataMap<T>> History<T>(IEnumerable<Symbol> symbols, int periods, Resolution? resolution = null ) 
            where T : BaseData
        {
            requests = symbols.Select(x =>
            {
                security = Securities[x];
                config = GetMatchingSubscription(security, typeof(T));
                if( config == null ) return null;

                Resolution? res = resolution ?? security.Resolution;
                start = GetStartTimeAlgoTz(x, periods, resolution).ConvertToUtc(TimeZone);
                return CreateHistoryRequest(security, config, start, UtcTime.RoundDown(res.Value.ToTimeSpan()), resolution);
            });

            return History(requests.Where(x -> x != null )).Get<T>().Memoize();
        }

        /**
         * Gets the historical data for the specified symbols between the specified dates. The symbols must exist in the Securities collection.
        */
         * <typeparam name="T The data type of the symbols</typeparam>
         * @param symbols The symbols to retrieve historical data for
         * @param start The start time in the algorithm's time zone
         * @param end The end time in the algorithm's time zone
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<DataMap<T>> History<T>(IEnumerable<Symbol> symbols, DateTime start, DateTime end, Resolution? resolution = null ) 
            where T : BaseData
        {
            requests = symbols.Select(x =>
            {
                security = Securities[x];
                config = GetMatchingSubscription(security, typeof(T));
                if( config == null ) return null;

                return CreateHistoryRequest(security, config, start, end, resolution);
            });

            return History(requests.Where(x -> x != null )).Get<T>().Memoize();
        }

        /**
         * Gets the historical data for the specified symbol over the request span. The symbol must exist in the Securities collection.
        */
         * <typeparam name="T The data type of the symbol</typeparam>
         * @param symbol The symbol to retrieve historical data for
         * @param span The span over which to retrieve recent historical data
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<T> History<T>(Symbol symbol, Duration span, Resolution? resolution = null )
            where T : BaseData
        {
            return History<T>(symbol, Time - span, Time, resolution).Memoize();
        }

        /**
         * Gets the historical data for the specified symbol. The exact number of bars will be returned. 
         * The symbol must exist in the Securities collection.
        */
         * @param symbol The symbol to retrieve historical data for
         * @param periods The number of bars to request
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<TradeBar> History(Symbol symbol, int periods, Resolution? resolution = null ) {
            security = Securities[symbol];
            start = GetStartTimeAlgoTz(symbol, periods, resolution);
            return History(new[] {symbol}, start, Time.RoundDown((resolution ?? security.Resolution).ToTimeSpan()), resolution).Get(symbol).Memoize();
        }

        /**
         * Gets the historical data for the specified symbol. The exact number of bars will be returned. 
         * The symbol must exist in the Securities collection.
        */
         * <typeparam name="T The data type of the symbol</typeparam>
         * @param symbol The symbol to retrieve historical data for
         * @param periods The number of bars to request
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<T> History<T>(Symbol symbol, int periods, Resolution? resolution = null )
            where T : BaseData
        {
            if( resolution == Resolution.Tick) throw new IllegalArgumentException( "History functions that accept a 'periods' parameter can not be used with Resolution.Tick");
            security = Securities[symbol];
            // verify the types match
            requestedType = typeof(T);
            config = GetMatchingSubscription(security, requestedType);
            if( config == null ) {
                actualType = security.Subscriptions.Select(x -> x.Type.Name).DefaultIfEmpty( "[None]").FirstOrDefault();
                throw new IllegalArgumentException( "The specified security is not of the requested type. Symbol: " + symbol.toString() + " Requested Type: " + requestedType.Name + " Actual Type: " + actualType);
            }

            start = GetStartTimeAlgoTz(symbol, periods, resolution);
            return History<T>(symbol, start, Time.RoundDown((resolution ?? security.Resolution).ToTimeSpan()), resolution).Memoize();
        }

        /**
         * Gets the historical data for the specified symbol between the specified dates. The symbol must exist in the Securities collection.
        */
         * @param symbol The symbol to retrieve historical data for
         * @param start The start time in the algorithm's time zone
         * @param end The end time in the algorithm's time zone
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<T> History<T>(Symbol symbol, DateTime start, DateTime end, Resolution? resolution = null )
            where T : BaseData
        {
            security = Securities[symbol];
            // verify the types match
            requestedType = typeof(T);
            config = GetMatchingSubscription(security, requestedType);
            if( config == null ) {
                actualType = security.Subscriptions.Select(x -> x.Type.Name).DefaultIfEmpty( "[None]").FirstOrDefault();
                throw new IllegalArgumentException( "The specified security is not of the requested type. Symbol: " + symbol.toString() + " Requested Type: " + requestedType.Name + " Actual Type: " + actualType);
            }

            request = CreateHistoryRequest(security, config, start, end, resolution);
            return History(request).Get<T>(symbol).Memoize();
        }

        /**
         * Gets the historical data for the specified symbol over the request span. The symbol must exist in the Securities collection.
        */
         * @param symbol The symbol to retrieve historical data for
         * @param span The span over which to retrieve recent historical data
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<TradeBar> History(Symbol symbol, Duration span, Resolution? resolution = null ) {
            return History(new[] {symbol}, span, resolution).Get(symbol).Memoize();
        }

        /**
         * Gets the historical data for the specified symbol over the request span. The symbol must exist in the Securities collection.
        */
         * @param symbol The symbol to retrieve historical data for
         * @param start The start time in the algorithm's time zone
         * @param end The end time in the algorithm's time zone
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<TradeBar> History(Symbol symbol, DateTime start, DateTime end, Resolution? resolution = null ) {
            return History(new[] {symbol}, start, end, resolution).Get(symbol).Memoize();
        }

        /**
         * Gets the historical data for the specified symbols over the requested span.
         * The symbol's configured values for resolution and fill forward behavior will be used
         * The symbols must exist in the Securities collection.
        */
         * @param symbols The symbols to retrieve historical data for
         * @param span The span over which to retrieve recent historical data
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<Slice> History(IEnumerable<Symbol> symbols, Duration span, Resolution? resolution = null ) {
            return History(symbols, Time - span, Time, resolution).Memoize();
        }

        /**
         * Gets the historical data for the specified symbols. The exact number of bars will be returned for
         * each symbol. This may result in some data start earlier/later than others due to when various
         * exchanges are open. The symbols must exist in the Securities collection.
        */
         * @param symbols The symbols to retrieve historical data for
         * @param periods The number of bars to request
         * @param resolution The resolution to request
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<Slice> History(IEnumerable<Symbol> symbols, int periods, Resolution? resolution = null ) {
            if( resolution == Resolution.Tick) throw new IllegalArgumentException( "History functions that accept a 'periods' parameter can not be used with Resolution.Tick");
            return History(CreateBarCountHistoryRequests(symbols, periods, resolution)).Memoize();
        }

        /**
         * Gets the historical data for the specified symbols between the specified dates. The symbols must exist in the Securities collection.
        */
         * @param symbols The symbols to retrieve historical data for
         * @param start The start time in the algorithm's time zone
         * @param end The end time in the algorithm's time zone
         * @param resolution The resolution to request
         * @param fillForward True to fill forward missing data, false otherwise
         * @param extendedMarket True to include extended market hours data, false otherwise
        @returns An enumerable of slice containing the requested historical data
        public IEnumerable<Slice> History(IEnumerable<Symbol> symbols, DateTime start, DateTime end, Resolution? resolution = null, bool? fillForward = null, bool? extendedMarket = null ) {
            return History(CreateDateRangeHistoryRequests(symbols, start, end, resolution, fillForward, extendedMarket)).Memoize();
        }

        /**
         * Gets the start time required for the specified bar count in terms of the algorithm's time zone
        */
        private DateTime GetStartTimeAlgoTz(Symbol symbol, int periods, Resolution? resolution = null ) {
            security = Securities[symbol];
            timeSpan = (resolution ?? security.Resolution).ToTimeSpan();
            // make this a minimum of one second
            timeSpan = timeSpan < QuantConnect.Time.OneSecond ? QuantConnect.Time.OneSecond : timeSpan;
            localStartTime = QuantConnect.Time.GetStartTimeForTradeBars(security.Exchange.Hours, UtcTime.ConvertFromUtc(security.Exchange.TimeZone), timeSpan, periods, security.IsExtendedMarketHours);
            return localStartTime Extensions.convertTo(  )security.Exchange.TimeZone, TimeZone);
        }

        /**
         * Executes the specified history request
        */
         * @param request the history request to execute
        @returns An enumerable of slice satisfying the specified history request
        public IEnumerable<Slice> History(HistoryRequest request) {
            return History(new[] {request}).Memoize();
        }

        /**
         * Executes the specified history requests
        */
         * @param requests the history requests to execute
        @returns An enumerable of slice satisfying the specified history request
        public IEnumerable<Slice> History(IEnumerable<HistoryRequest> requests) {
            return History(requests, TimeZone).Memoize();
        }

        private IEnumerable<Slice> History(IEnumerable<HistoryRequest> requests, ZoneId timeZone) {
            sentMessage = false;
            reqs = requests.ToList();
            foreach (request in reqs) {
                // prevent future requests
                if( request.EndTimeUtc > UtcTime) {
                    request.EndTimeUtc = UtcTime;
                    if( request.StartTimeUtc > request.EndTimeUtc) {
                        request.StartTimeUtc = request.EndTimeUtc;
                    }
                    if( !sentMessage) {
                        sentMessage = true;
                        Debug( "Request for future history modified to end now.");
                    }
                }
            }

            // filter out future data to prevent look ahead bias
            return ((IAlgorithm) this).HistoryProvider.GetHistory(reqs, timeZone);
        }

        /**
         * Helper method to create history requests from a date range
        */
        private IEnumerable<HistoryRequest> CreateDateRangeHistoryRequests(IEnumerable<Symbol> symbols, DateTime startAlgoTz, DateTime endAlgoTz, Resolution? resolution = null, bool? fillForward = null, bool? extendedMarket = null ) {
            return symbols.Select(x =>
            {
                security = Securities[x];
                config = GetMatchingSubscription(security, typeof (BaseData));
                request = CreateHistoryRequest(security, config, startAlgoTz, endAlgoTz, resolution);

                // apply @Overrides
                Resolution? res = resolution ?? security.Resolution;
                if( fillForward.HasValue) request.FillForwardResolution = fillForward.Value ? res : null;
                if( extendedMarket.HasValue) request.IncludeExtendedMarketHours = extendedMarket.Value;
                return request;
            });
        }

        /**
         * Helper methods to create a history request for the specified symbols and bar count
        */
        private IEnumerable<HistoryRequest> CreateBarCountHistoryRequests(IEnumerable<Symbol> symbols, int periods, Resolution? resolution = null ) {
            return symbols.Select(x =>
            {
                security = Securities[x];
                Resolution? res = resolution ?? security.Resolution;
                start = GetStartTimeAlgoTz(x, periods, res);
                config = GetMatchingSubscription(security, typeof(BaseData));
                return CreateHistoryRequest(security, config, start, Time.RoundDown(res.Value.ToTimeSpan()), resolution);
            });
        }

        private HistoryRequest CreateHistoryRequest(Security security, SubscriptionDataConfig subscription, DateTime startAlgoTz, DateTime endAlgoTz, Resolution? resolution) {
            resolution = resolution ?? security.Resolution;
            request = new HistoryRequest(subscription, security.Exchange.Hours, startAlgoTz.ConvertToUtc(TimeZone), endAlgoTz.ConvertToUtc(TimeZone)) {
                DataType = subscription.IsCustomData ? subscription.Type : resolution == Resolution.Tick ? typeof(Tick) : typeof(TradeBar),
                Resolution = resolution.Value,
                FillForwardResolution = subscription.FillDataForward ? resolution : null
            };
            return request;
        }

        private static SubscriptionDataConfig GetMatchingSubscription(Security security, Type type) {
            // find a subscription matchin the requested type with a higher resolution than requested
            return (from sub in security.Subscriptions.OrderByDescending(s -> s.Resolution)
                    where type.IsAssignableFrom(sub.Type)
                    select sub).FirstOrDefault();
        }
    }
}