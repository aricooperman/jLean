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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Custom;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds.Enumerators;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Logging;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
     * Provides an implementation of <see cref="IDataFeed"/> that is designed to deal with
     * live, remote data sources
    */
    public class LiveTradingDataFeed : IDataFeed
    {
        private SecurityChanges _changes = SecurityChanges.None;
        private static final Symbol DataQueueHandlerSymbol = Symbol.Create( "data-queue-handler-symbol", SecurityType.Base, Market.USA);

        private LiveNodePacket _job;
        private IAlgorithm _algorithm;
        // used to get current time
        private ITimeProvider _timeProvider;
        // used to keep time constant during a time sync iteration
        private ManualTimeProvider _frontierTimeProvider;

        private Ref<TimeSpan> _fillForwardResolution;
        private IResultHandler _resultHandler;
        private IDataQueueHandler _dataQueueHandler;
        private BaseDataExchange _exchange;
        private BaseDataExchange _customExchange;
        private SubscriptionCollection _subscriptions;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private BusyBlockingCollection<TimeSlice> _bridge;
        private UniverseSelection _universeSelection;
        private DateTime _frontierUtc;

        /**
         * Gets all of the current subscriptions this data feed is processing
        */
        public IEnumerable<Subscription> Subscriptions
        {
            get { return _subscriptions; }
        }

        /**
         * Public flag indicator that the thread is still busy.
        */
        public boolean IsActive
        {
            get; private set;
        }

        /**
         * Initializes the data feed for the specified job and algorithm
        */
        public void Initialize(IAlgorithm algorithm, AlgorithmNodePacket job, IResultHandler resultHandler, IMapFileProvider mapFileProvider, IFactorFileProvider factorFileProvider) {
            if( !(job is LiveNodePacket)) {
                throw new IllegalArgumentException( "The LiveTradingDataFeed requires a LiveNodePacket.");
            }

            _cancellationTokenSource = new CancellationTokenSource();

            _algorithm = algorithm;
            _job = (LiveNodePacket) job;
            _resultHandler = resultHandler;
            _timeProvider = GetTimeProvider();
            _dataQueueHandler = GetDataQueueHandler();

            _frontierTimeProvider = new ManualTimeProvider(_timeProvider.GetUtcNow());
            _customExchange = new BaseDataExchange( "CustomDataExchange") {SleepInterval = 10};
            // sleep is controlled on this exchange via the GetNextTicksEnumerator
            _exchange = new BaseDataExchange( "DataQueueExchange"){SleepInterval = 0};
            _exchange.AddEnumerator(DataQueueHandlerSymbol, GetNextTicksEnumerator());
            _subscriptions = new SubscriptionCollection();

            _bridge = new BusyBlockingCollection<TimeSlice>();
            _universeSelection = new UniverseSelection(this, algorithm, job.Controls);

            // run the exchanges
            Task.Run(() -> _exchange.Start(_cancellationTokenSource.Token));
            Task.Run(() -> _customExchange.Start(_cancellationTokenSource.Token));

            // this value will be modified via calls to AddSubscription/RemoveSubscription
            ffres = Time.OneMinute;
            _fillForwardResolution = Ref.Create(() -> ffres, v -> ffres = v);

            // wire ourselves up to receive notifications when universes are added/removed
            start = _timeProvider.GetUtcNow();
            algorithm.UniverseManager.CollectionChanged += (sender, args) =>
            {
                switch (args.Action) {
                    case NotifyCollectionChangedAction.Add:
                        foreach (universe in args.NewItems.OfType<Universe>()) {
                            if( !_subscriptions.Contains(universe.Configuration)) {
                                _subscriptions.TryAdd(CreateUniverseSubscription(universe, start, Time.EndOfTime));
                            }

                            // Not sure if this is needed but left here because of this:
                            // https://github.com/QuantConnect/Lean/commit/029d70bde6ca83a1eb0c667bb5cc4444bea05678
                            UpdateFillForwardResolution();
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (universe in args.OldItems.OfType<Universe>()) {
                            RemoveSubscription(universe.Configuration);
                        }
                        break;

                    default:
                        throw new NotImplementedException( "The specified action is not implemented: " + args.Action);
                }
            };
        }

        /**
         * Adds a new subscription to provide data for the specified security.
        */
         * @param universe The universe the subscription is to be added to
         * @param security The security to add a subscription for
         * @param config The subscription config to be added
         * @param utcStartTime The start time of the subscription
         * @param utcEndTime The end time of the subscription
        @returns True if the subscription was created and added successfully, false otherwise
        public boolean AddSubscription(Universe universe, Security security, SubscriptionDataConfig config, DateTime utcStartTime, DateTime utcEndTime) {
            // create and add the subscription to our collection
            subscription = CreateSubscription(universe, security, config, utcStartTime, utcEndTime);
            
            // for some reason we couldn't create the subscription
            if( subscription == null ) {
                Log.Trace( "Unable to add subscription for: " + security.Symbol.toString());
                return false;
            }

            Log.Trace( "LiveTradingDataFeed.AddSubscription(): Added " + security.Symbol.toString());

            _subscriptions.TryAdd(subscription);

            // send the subscription for the new symbol through to the data queuehandler
            // unless it is custom data, custom data is retrieved using the same as backtest
            if( !subscription.Configuration.IsCustomData) {
                _dataQueueHandler.Subscribe(_job, new[] {security.Symbol});
            }

            // keep track of security changes, we emit these to the algorithm
            // as notifications, used in universe selection
            _changes += SecurityChanges.Added(security);

            UpdateFillForwardResolution();

            return true;
        }

        /**
         * Removes the subscription from the data feed, if it exists
        */
         * @param configuration The configuration of the subscription to remove
        @returns True if the subscription was successfully removed, false otherwise
        public boolean RemoveSubscription(SubscriptionDataConfig configuration) {
            // remove the subscription from our collection
            Subscription subscription;
            if( !_subscriptions.TryGetValue(configuration, out subscription)) {
                return false;
            }

            security = subscription.Security;

            // remove the subscriptions
            if( subscription.Configuration.IsCustomData) {
                _customExchange.RemoveEnumerator(security.Symbol);
                _customExchange.RemoveDataHandler(security.Symbol);
            }
            else
            {
                _dataQueueHandler.Unsubscribe(_job, new[] { security.Symbol });
                _exchange.RemoveDataHandler(security.Symbol);
            }

            subscription.Dispose();

            // keep track of security changes, we emit these to the algorithm
            // as notications, used in universe selection
            _changes += SecurityChanges.Removed(security);


            Log.Trace( "LiveTradingDataFeed.RemoveSubscription(): Removed " + configuration.toString());
            UpdateFillForwardResolution();

            return true;
        }

        /**
         * Primary entry point.
        */
        public void Run() {
            IsActive = true;

            // we want to emit to the bridge minimally once a second since the data feed is
            // the heartbeat of the application, so this value will contain a second after
            // the last emit time, and if we pass this time, we'll emit even with no data
            nextEmit = DateTime.MinValue;

            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested) {
                    // perform sleeps to wake up on the second?
                    _frontierUtc = _timeProvider.GetUtcNow();
                    _frontierTimeProvider.SetCurrentTime(_frontierUtc);

                    data = new List<DataFeedPacket>();
                    foreach (subscription in Subscriptions) {
                        packet = new DataFeedPacket(subscription.Security, subscription.Configuration);

                        // dequeue data that is time stamped at or before this frontier
                        while (subscription.MoveNext() && subscription.Current != null ) {
                            packet.Add(subscription.Current);
                        }

                        // if we have data, add it to be added to the bridge
                        if( packet.Count > 0) data.Add(packet);

                        // we have new universe data to select based on
                        if( subscription.IsUniverseSelectionSubscription && packet.Count > 0) {
                            universe = subscription.Universe;

                            // always wait for other thread to sync up
                            if( !_bridge.WaitHandle.WaitOne(Timeout.Infinite, _cancellationTokenSource.Token)) {
                                break;
                            }

                            // assume that if the first item is a base data collection then the enumerator handled the aggregation,
                            // otherwise, load all the the data into a new collection instance
                            collection = packet.Data[0] as BaseDataCollection ?? new BaseDataCollection(_frontierUtc, subscription.Configuration.Symbol, packet.Data);

                            _changes += _universeSelection.ApplyUniverseSelection(universe, _frontierUtc, collection);
                        }
                    }

                    // check for cancellation
                    if( _cancellationTokenSource.IsCancellationRequested) return;

                    // emit on data or if we've elapsed a full second since last emit
                    if( data.Count != 0 || _frontierUtc >= nextEmit) {
                        _bridge.Add(TimeSlice.Create(_frontierUtc, _algorithm.TimeZone, _algorithm.Portfolio.CashBook, data, _changes), _cancellationTokenSource.Token);

                        // force emitting every second
                        nextEmit = _frontierUtc.RoundDown(Time.OneSecond).Add(Time.OneSecond);
                    }

                    // reset our security changes
                    _changes = SecurityChanges.None;

                    // take a short nap
                    Thread.Sleep(1);
                }
            }
            catch (Exception err) {
                Log.Error(err);
                _algorithm.RunTimeError = err;
            }

            Log.Trace( "LiveTradingDataFeed.Run(): Exited thread.");
            IsActive = false;
        }

        /**
         * External controller calls to signal a terminate of the thread.
        */
        public void Exit() {
            if( _subscriptions != null ) {
                // remove each subscription from our collection
                foreach (subscription in Subscriptions) {
                    try
                    {
                        RemoveSubscription(subscription.Configuration);
                    }
                    catch (Exception err) {
                        Log.Error(err, "Error removing: " + subscription.Configuration);
                    }
                }
            }

            if( _exchange != null ) _exchange.Stop();
            if( _customExchange != null ) _customExchange.Stop();

            Log.Trace( "LiveTradingDataFeed.Exit(): Setting cancellation token...");
            _cancellationTokenSource.Cancel();
            
            if( _bridge != null ) _bridge.Dispose();
        }

        /**
         * Gets the <see cref="IDataQueueHandler"/> to use. By default this will try to load
         * the type specified in the configuration via the 'data-queue-handler'
        */
        @returns The loaded <see cref="IDataQueueHandler"/>
        protected IDataQueueHandler GetDataQueueHandler() {
            return Composer.Instance.GetExportedValueByTypeName<IDataQueueHandler>(Config.Get( "data-queue-handler", "LiveDataQueue"));
        }

        /**
         * Gets the <see cref="ITimeProvider"/> to use. By default this will load the
         * <see cref="RealTimeProvider"/> which use's the system's <see cref="DateTime.UtcNow"/>
         * for the current time
        */
        @returns he loaded <see cref="ITimeProvider"/>
        protected ITimeProvider GetTimeProvider() {
            return new RealTimeProvider();
        }

        /**
         * Creates a new subscription for the specified security
        */
         * @param universe">
         * @param security The security to create a subscription for
         * @param config The subscription config to be added
         * @param utcStartTime The start time of the subscription in UTC
         * @param utcEndTime The end time of the subscription in UTC
        @returns A new subscription instance of the specified security
        protected Subscription CreateSubscription(Universe universe, Security security, SubscriptionDataConfig config, DateTime utcStartTime, DateTime utcEndTime) {
            Subscription subscription = null;
            try
            {
                localEndTime = utcEndTime Extensions.convertFromUtc(security.Exchange.TimeZone);
                timeZoneOffsetProvider = new TimeZoneOffsetProvider(security.Exchange.TimeZone, utcStartTime, utcEndTime);

                IEnumerator<BaseData> enumerator;
                if( config.IsCustomData) {
                    if( !Quandl.IsAuthCodeSet) {
                        // we're not using the SubscriptionDataReader, so be sure to set the auth token here
                        Quandl.SetAuthCode(Config.Get( "quandl-auth-token"));
                    }

                    // each time we exhaust we'll new up this enumerator stack
                    refresher = new RefreshEnumerator<BaseData>(() =>
                    {
                        sourceProvider = (BaseData)Activator.CreateInstance(config.Type);
                        dateInDataTimeZone = DateTime.UtcNow Extensions.convertFromUtc(config.DataTimeZone).Date;
                        source = sourceProvider.GetSource(config, dateInDataTimeZone, true);
                        factory = SubscriptionDataSourceReader.ForSource(source, config, dateInDataTimeZone, false);
                        factoryReadEnumerator = factory.Read(source).GetEnumerator();
                        maximumDataAge = Duration.ofTicks(Math.Max(config.Increment.Ticks, Duration.ofSeconds(5).Ticks));
                        return new FastForwardEnumerator(factoryReadEnumerator, _timeProvider, security.Exchange.TimeZone, maximumDataAge);
                    });

                    // rate limit the refreshing of the stack to the requested interval
                    minimumTimeBetweenCalls = Math.Min(config.Increment.Ticks, Duration.ofMinutes(30).Ticks);
                    rateLimit = new RateLimitEnumerator(refresher, _timeProvider, Duration.ofTicks(minimumTimeBetweenCalls));
                    frontierAware = new FrontierAwareEnumerator(rateLimit, _timeProvider, timeZoneOffsetProvider);
                    _customExchange.AddEnumerator(config.Symbol, frontierAware);

                    enqueable = new EnqueueableEnumerator<BaseData>();
                    _customExchange.SetDataHandler(config.Symbol, data =>
                    {
                        enqueable.Enqueue(data);
                        if( subscription != null ) subscription.RealtimePrice = data.Value;
                    });
                    enumerator = enqueable;
                }
                else if( config.Resolution != Resolution.Tick) {
                    // this enumerator allows the exchange to pump ticks into the 'back' of the enumerator,
                    // and the time sync loop can pull aggregated trade bars off the front
                    aggregator = new TradeBarBuilderEnumerator(config.Increment, security.Exchange.TimeZone, _timeProvider);
                    _exchange.SetDataHandler(config.Symbol, data =>
                    {
                        aggregator.ProcessData((Tick) data);
                        if( subscription != null ) subscription.RealtimePrice = data.Value;
                    });
                    enumerator = aggregator;
                }
                else
                {
                    // tick subscriptions can pass right through
                    tickEnumerator = new EnqueueableEnumerator<BaseData>();
                    _exchange.SetDataHandler(config.Symbol, data =>
                    {
                        tickEnumerator.Enqueue(data);
                        if( subscription != null ) subscription.RealtimePrice = data.Value;
                    });
                    enumerator = tickEnumerator;
                }

                if( config.FillDataForward) {
                    enumerator = new LiveFillForwardEnumerator(_frontierTimeProvider, enumerator, security.Exchange, _fillForwardResolution, config.ExtendedMarketHours, localEndTime, config.Increment);
                }

                // define market hours and user filters to incoming data
                if( config.IsFilteredSubscription) {
                    enumerator = new SubscriptionFilterEnumerator(enumerator, security, localEndTime);
                }

                // finally, make our subscriptions aware of the frontier of the data feed, prevents future data from spewing into the feed
                enumerator = new FrontierAwareEnumerator(enumerator, _frontierTimeProvider, timeZoneOffsetProvider);

                subscription = new Subscription(universe, security, config, enumerator, timeZoneOffsetProvider, utcStartTime, utcEndTime, false);
            }
            catch (Exception err) {
                Log.Error(err);
            }

            return subscription;
        }

        /**
         * Creates a new subscription for universe selection
        */
         * @param universe The universe to add a subscription for
         * @param startTimeUtc The start time of the subscription in utc
         * @param endTimeUtc The end time of the subscription in utc
        protected Subscription CreateUniverseSubscription(Universe universe, DateTime startTimeUtc, DateTime endTimeUtc) {
            // TODO : Consider moving the creating of universe subscriptions to a separate, testable class

            // grab the relevant exchange hours
            config = universe.Configuration;

            marketHoursDatabase = MarketHoursDatabase.FromDataFolder();
            exchangeHours = marketHoursDatabase.GetExchangeHours(config);
            
            Security security;
            if( !_algorithm.Securities.TryGetValue(config.Symbol, out security)) {
                // create a canonical security object
                security = new Security(exchangeHours, config, _algorithm.Portfolio.CashBook[CashBook.AccountCurrency], SymbolProperties.GetDefault(CashBook.AccountCurrency));
            }

            tzOffsetProvider = new TimeZoneOffsetProvider(security.Exchange.TimeZone, startTimeUtc, endTimeUtc);

            IEnumerator<BaseData> enumerator;
            
            userDefined = universe as UserDefinedUniverse;
            if( userDefined != null ) {
                Log.Trace( "LiveTradingDataFeed.CreateUniverseSubscription(): Creating user defined universe: " + config.Symbol.toString());

                // spoof a tick on the requested interval to trigger the universe selection function
                enumerator = userDefined.GetTriggerTimes(startTimeUtc, endTimeUtc, marketHoursDatabase)
                    .Select(dt -> new Tick { Time = dt }).GetEnumerator();

                enumerator = new FrontierAwareEnumerator(enumerator, _timeProvider, tzOffsetProvider);

                enqueueable = new EnqueueableEnumerator<BaseData>();
                _customExchange.AddEnumerator(new EnumeratorHandler(config.Symbol, enumerator, enqueueable));
                enumerator = enqueueable;
            }
            else if( config.Type == typeof (CoarseFundamental)) {
                Log.Trace( "LiveTradingDataFeed.CreateUniverseSubscription(): Creating coarse universe: " + config.Symbol.toString());

                // since we're binding to the data queue exchange we'll need to let him
                // know that we expect this data
                _dataQueueHandler.Subscribe(_job, new[] {security.Symbol});

                enqueable = new EnqueueableEnumerator<BaseData>();
                _exchange.SetDataHandler(config.Symbol, data =>
                {
                    enqueable.Enqueue(data);
                });
                enumerator = enqueable;
            }
            else
            {
                Log.Trace( "LiveTradingDataFeed.CreateUniverseSubscription(): Creating custom universe: " + config.Symbol.toString());

                // each time we exhaust we'll new up this enumerator stack
                refresher = new RefreshEnumerator<BaseDataCollection>(() =>
                {
                    sourceProvider = (BaseData)Activator.CreateInstance(config.Type);
                    dateInDataTimeZone = DateTime.UtcNow Extensions.convertFromUtc(config.DataTimeZone).Date;
                    source = sourceProvider.GetSource(config, dateInDataTimeZone, true);
                    factory = SubscriptionDataSourceReader.ForSource(source, config, dateInDataTimeZone, false);
                    factorEnumerator = factory.Read(source).GetEnumerator();
                    fastForward = new FastForwardEnumerator(factorEnumerator, _timeProvider, security.Exchange.TimeZone, config.Increment);
                    frontierAware = new FrontierAwareEnumerator(fastForward, _frontierTimeProvider, tzOffsetProvider);
                    return new BaseDataCollectionAggregatorEnumerator(frontierAware, config.Symbol);
                });
                
                // rate limit the refreshing of the stack to the requested interval
                minimumTimeBetweenCalls = Math.Min(config.Increment.Ticks, Duration.ofMinutes(30).Ticks);
                rateLimit = new RateLimitEnumerator(refresher, _timeProvider, Duration.ofTicks(minimumTimeBetweenCalls));
                enqueueable = new EnqueueableEnumerator<BaseData>();
                _customExchange.AddEnumerator(new EnumeratorHandler(config.Symbol, rateLimit, enqueueable));
                enumerator = enqueueable;
            }

            // create the subscription
            subscription = new Subscription(universe, security, config, enumerator, tzOffsetProvider, startTimeUtc, endTimeUtc, true);

            return subscription;
        }

        /**
         * Provides an <see cref="IEnumerator{BaseData}"/> that will continually dequeue data
         * from the data queue handler while we're not cancelled
        */
        @returns 
        private IEnumerator<BaseData> GetNextTicksEnumerator() {
            while (!_cancellationTokenSource.IsCancellationRequested) {
                int ticks = 0;
                foreach (data in _dataQueueHandler.GetNextTicks()) {
                    ticks++;
                    yield return data;
                }
                if( ticks == 0) Thread.Sleep(1);
            }

            Log.Trace( "LiveTradingDataFeed.GetNextTicksEnumerator(): Exiting enumerator thread...");
        }

        /**
         * Updates the fill forward resolution by checking all existing subscriptions and
         * selecting the smallest resoluton not equal to tick
        */
        private void UpdateFillForwardResolution() {
            _fillForwardResolution.Value = _subscriptions
                .Where(x -> !x.Configuration.IsInternalFeed)
                .Select(x -> x.Configuration.Resolution)
                .Where(x -> x != Resolution.Tick)
                .DefaultIfEmpty(Resolution.Minute)
                .Min().ToTimeSpan();
        }

        /**
         * Returns an enumerator that iterates through the collection.
        */
        @returns 
         * A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
         * 
         * <filterpriority>1</filterpriority>
        public IEnumerator<TimeSlice> GetEnumerator() {
            return _bridge.GetConsumingEnumerable(_cancellationTokenSource.Token).GetEnumerator();
        }

        /**
         * Returns an enumerator that iterates through a collection.
        */
        @returns 
         * An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
         * 
         * <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }


        /**
         * Overrides methods of the base data exchange implementation
        */
        class EnumeratorHandler : BaseDataExchange.EnumeratorHandler
        {
            private final EnqueueableEnumerator<BaseData> _enqueueable;
            public EnumeratorHandler(Symbol symbol, IEnumerator<BaseData> enumerator, EnqueueableEnumerator<BaseData> enqueueable)
                : base(symbol, enumerator, true) {
                _enqueueable = enqueueable;
            }
            /**
             * Returns true if this enumerator should move next
            */
            public @Override boolean ShouldMoveNext() { return true; }
            /**
             * Calls stop on the internal enqueueable enumerator
            */
            public @Override void OnEnumeratorFinished() { _enqueueable.Stop(); }
            /**
             * Enqueues the data
            */
             * @param data The data to be handled
            public @Override void HandleData(BaseData data) {
                _enqueueable.Enqueue(data);
            }
        }
    }
}
