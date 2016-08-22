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
 *
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using QuantConnect.Data;
using QuantConnect.Data.Auxiliary;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds.Enumerators;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Logging;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Securities.Option;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /// <summary>
    /// Historical datafeed stream reader for processing files on a local disk.
    /// </summary>
    /// <remarks>Filesystem datafeeds are incredibly fast</remarks>
    public class FileSystemDataFeed : IDataFeed
    {
        private IAlgorithm _algorithm;
        private ParallelRunnerController _controller;
        private IResultHandler _resultHandler;
        private Ref<TimeSpan> _fillForwardResolution;
        private IMapFileProvider _mapFileProvider;
        private IFactorFileProvider _factorFileProvider;
        private SubscriptionCollection _subscriptions;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private UniverseSelection _universeSelection;
        private DateTime _frontierUtc;

        /// <summary>
        /// Gets all of the current subscriptions this data feed is processing
        /// </summary>
        public IEnumerable<Subscription> Subscriptions
        {
            get { return _subscriptions; }
        }

        /// <summary>
        /// Flag indicating the hander thread is completely finished and ready to dispose.
        /// </summary>
        public boolean IsActive { get; private set; }

        /// <summary>
        /// Initializes the data feed for the specified job and algorithm
        /// </summary>
        public void Initialize(IAlgorithm algorithm, AlgorithmNodePacket job, IResultHandler resultHandler, IMapFileProvider mapFileProvider, IFactorFileProvider factorFileProvider) {
            _algorithm = algorithm;
            _resultHandler = resultHandler;
            _mapFileProvider = mapFileProvider;
            _factorFileProvider = factorFileProvider;
            _subscriptions = new SubscriptionCollection();
            _universeSelection = new UniverseSelection(this, algorithm, job.Controls);
            _cancellationTokenSource = new CancellationTokenSource();

            IsActive = true;
            threadCount = Math.Max(1, Math.Min(4, Environment.ProcessorCount - 3));
            _controller = new ParallelRunnerController(threadCount);
            _controller.Start(_cancellationTokenSource.Token);

            ffres = Time.OneMinute;
            _fillForwardResolution = Ref.Create(() => ffres, res => ffres = res);

            // wire ourselves up to receive notifications when universes are added/removed
            algorithm.UniverseManager.CollectionChanged += (sender, args) =>
            {
                switch (args.Action) {
                    case NotifyCollectionChangedAction.Add:
                        foreach (universe in args.NewItems.OfType<Universe>()) {
                            start = _frontierUtc != DateTime.MinValue ? _frontierUtc : _algorithm.StartDate.ConvertToUtc(_algorithm.TimeZone);
                            AddUniverseSubscription(universe, start, _algorithm.EndDate.ConvertToUtc(_algorithm.TimeZone));
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

        private Subscription CreateSubscription(Universe universe, Security security, SubscriptionDataConfig config, DateTime startTimeUtc, DateTime endTimeUtc) {
            localStartTime = startTimeUtc.ConvertFromUtc(security.Exchange.TimeZone);
            localEndTime = endTimeUtc.ConvertFromUtc(security.Exchange.TimeZone);

            tradeableDates = Time.EachTradeableDayInTimeZone(security.Exchange.Hours, localStartTime, localEndTime, config.DataTimeZone, config.ExtendedMarketHours);

            // ReSharper disable once PossibleMultipleEnumeration
            if( !tradeableDates.Any()) {
                _algorithm.Error( String.format( "No data loaded for %1$s because there were no tradeable dates for this security.", security.Symbol));
                return null;
            }

            // get the map file resolver for this market
            mapFileResolver = MapFileResolver.Empty;
            if( config.SecurityType == SecurityType.Equity) mapFileResolver = _mapFileProvider.Get(config.Market);

            // ReSharper disable once PossibleMultipleEnumeration
            enumerator = CreateSubscriptionEnumerator(security, config, localStartTime, localEndTime, mapFileResolver, tradeableDates, true, false);

            enqueueable = new EnqueueableEnumerator<BaseData>(true);

            // add this enumerator to our exchange
            ScheduleEnumerator(enumerator, enqueueable, GetLowerThreshold(config.Resolution), GetUpperThreshold(config.Resolution));

            timeZoneOffsetProvider = new TimeZoneOffsetProvider(security.Exchange.TimeZone, startTimeUtc, endTimeUtc);
            subscription = new Subscription(universe, security, config, enqueueable, timeZoneOffsetProvider, startTimeUtc, endTimeUtc, false);
            return subscription;
        }

        private void ScheduleEnumerator(IEnumerator<BaseData> enumerator, EnqueueableEnumerator<BaseData> enqueueable, int lowerThreshold, int upperThreshold, int firstLoopCount = 5) {
            // schedule the work on the controller
            firstLoop = true;
            FuncParallelRunnerWorkItem workItem = null;
            workItem = new FuncParallelRunnerWorkItem(() => enqueueable.Count < lowerThreshold, () =>
            {
                count = 0;
                while (enumerator.MoveNext()) {
                    // drop the data into the back of the enqueueable
                    enqueueable.Enqueue(enumerator.Current);

                    count++;

                    // special behavior for first loop to spool up quickly
                    if( firstLoop && count > firstLoopCount) {
                        // there's more data in the enumerator, reschedule to run again
                        firstLoop = false;
                        _controller.Schedule(workItem);
                        return;
                    }

                    // stop executing if we've dequeued more than the lower threshold or have
                    // more total that upper threshold in the enqueueable's queue
                    if( count > lowerThreshold || enqueueable.Count > upperThreshold) {
                        // there's more data in the enumerator, reschedule to run again
                        _controller.Schedule(workItem);
                        return;
                    }
                }

                // we made it here because MoveNext returned false, stop the enqueueable and don't reschedule
                enqueueable.Stop();
            });
            _controller.Schedule(workItem);
        }

        /// <summary>
        /// Adds a new subscription to provide data for the specified security.
        /// </summary>
        /// <param name="universe">The universe the subscription is to be added to</param>
        /// <param name="security">The security to add a subscription for</param>
        /// <param name="config">The subscription config to be added</param>
        /// <param name="utcStartTime">The start time of the subscription</param>
        /// <param name="utcEndTime">The end time of the subscription</param>
        public boolean AddSubscription(Universe universe, Security security, SubscriptionDataConfig config, DateTime utcStartTime, DateTime utcEndTime) {
            subscription = CreateSubscription(universe, security, config, utcStartTime, utcEndTime);
            if( subscription == null ) {
                // subscription will be null when there's no tradeable dates for the security between the requested times, so
                // don't even try to load the data
                return false;
            }

            Log.Debug( "FileSystemDataFeed.AddSubscription(): Added " + security.Symbol.ID + " Start: " + utcStartTime + " End: " + utcEndTime);

            if( _subscriptions.TryAdd(subscription)) {
            UpdateFillForwardResolution();
            }

            return true;
        }

        /// <summary>
        /// Removes the subscription from the data feed, if it exists
        /// </summary>
        /// <param name="configuration">The configuration of the subscription to remove</param>
        /// <returns>True if the subscription was successfully removed, false otherwise</returns>
        public boolean RemoveSubscription(SubscriptionDataConfig configuration) {
            Subscription subscription;
            if( !_subscriptions.TryRemove(configuration, out subscription)) {
                Log.Error( "FileSystemDataFeed.RemoveSubscription(): Unable to remove: " + configuration.toString());
                return false;
            }

                subscription.Dispose();
            Log.Debug( "FileSystemDataFeed.RemoveSubscription(): Removed " + configuration.toString());

            UpdateFillForwardResolution();

            return true;
        }

        /// <summary>
        /// Main routine for datafeed analysis.
        /// </summary>
        /// <remarks>This is a hot-thread and should be kept extremely lean. Modify with caution.</remarks>
        public void Run() {
            try
            {
                _controller.WaitHandle.WaitOne();
            }
            catch (Exception err) {
                Log.Error( "FileSystemDataFeed.Run(): Encountered an error: " + err.Message); 
                if( !_cancellationTokenSource.IsCancellationRequested) {
                    _cancellationTokenSource.Cancel();
                }
            }
            finally
            {
                Log.Trace( "FileSystemDataFeed.Run(): Ending Thread... ");
                if( _controller != null ) _controller.Dispose();
                IsActive = false;
            }
        }

        private DateTime GetInitialFrontierTime() {
            frontier = DateTime.MaxValue;
            foreach (subscription in Subscriptions) {
                current = subscription.Current;
                if( current == null ) {
                    continue;
                }

                // we need to initialize both the frontier time and the offset provider, in order to do
                // this we'll first convert the current.EndTime to UTC time, this will allow us to correctly
                // determine the offset in ticks using the OffsetProvider, we can then use this to recompute
                // the UTC time. This seems odd, but is necessary given Noda time's lenient mapping, the
                // OffsetProvider exists to give forward marching mapping

                // compute the initial frontier time
                currentEndTimeUtc = current.EndTime.ConvertToUtc(subscription.TimeZone);
                endTime = current.EndTime.Ticks - subscription.OffsetProvider.GetOffsetTicks(currentEndTimeUtc);
                if( endTime < frontier.Ticks) {
                    frontier = new DateTime(endTime);
                }
            }

            if( frontier == DateTime.MaxValue) {
                frontier = _algorithm.StartDate.ConvertToUtc(_algorithm.TimeZone);
            }
            return frontier;
        }

        /// <summary>
        /// Adds a new subscription for universe selection
        /// </summary>
        /// <param name="universe">The universe to add a subscription for</param>
        /// <param name="startTimeUtc">The start time of the subscription in utc</param>
        /// <param name="endTimeUtc">The end time of the subscription in utc</param>
        public void AddUniverseSubscription(Universe universe, DateTime startTimeUtc, DateTime endTimeUtc) {
            // TODO : Consider moving the creating of universe subscriptions to a separate, testable class

            // grab the relevant exchange hours
            config = universe.Configuration;

            marketHoursDatabase = MarketHoursDatabase.FromDataFolder();
            exchangeHours = marketHoursDatabase.GetExchangeHours(config);

            Security security;
            if( !_algorithm.Securities.TryGetValue(config.Symbol, out security)) {
                // create a canonical security object if it doesn't exist
                security = new Security(exchangeHours, config, _algorithm.Portfolio.CashBook[CashBook.AccountCurrency], SymbolProperties.GetDefault(CashBook.AccountCurrency));
            }

            localStartTime = startTimeUtc.ConvertFromUtc(security.Exchange.TimeZone);
            localEndTime = endTimeUtc.ConvertFromUtc(security.Exchange.TimeZone);

            // define our data enumerator
            IEnumerator<BaseData> enumerator;

            tradeableDates = Time.EachTradeableDayInTimeZone(security.Exchange.Hours, localStartTime, localEndTime, config.DataTimeZone, config.ExtendedMarketHours);

            userDefined = universe as UserDefinedUniverse;
            if( userDefined != null ) {
                // spoof a tick on the requested interval to trigger the universe selection function
                enumerator = userDefined.GetTriggerTimes(startTimeUtc, endTimeUtc, marketHoursDatabase)
                    .Select(x => new Tick { Time = x, Symbol = config.Symbol }).GetEnumerator();

                // route these custom subscriptions through the exchange for buffering
                enqueueable = new EnqueueableEnumerator<BaseData>(true);

                // add this enumerator to our exchange
                ScheduleEnumerator(enumerator, enqueueable, GetLowerThreshold(config.Resolution), GetUpperThreshold(config.Resolution));

                enumerator = enqueueable;
            }
            else if( config.Type == typeof (CoarseFundamental)) {
                cf = new CoarseFundamental();

                // load coarse data day by day
                enumerator = (from date in Time.EachTradeableDayInTimeZone(security.Exchange.Hours, _algorithm.StartDate, _algorithm.EndDate, config.DataTimeZone, config.ExtendedMarketHours)
                             let source = cf.GetSource(config, date, false)
                             let factory = SubscriptionDataSourceReader.ForSource(source, config, date, false)
                             let coarseFundamentalForDate = factory.Read(source)
                             select new BaseDataCollection(date.AddDays(1), config.Symbol, coarseFundamentalForDate)
                             ).GetEnumerator();
                
                enqueueable = new EnqueueableEnumerator<BaseData>(true);
                ScheduleEnumerator(enumerator, enqueueable, 5, 100000, 2);

                enumerator = enqueueable;
            }
            else if( config.SecurityType == SecurityType.Option && security is Option) {
                configs = universe.GetSubscriptions(security);
                enumerators = configs.Select(c =>
                    CreateSubscriptionEnumerator(security, c, localStartTime, localEndTime, _mapFileProvider.Get(c.Market), tradeableDates, false, true)
                    ).ToList();

                sync = new SynchronizingEnumerator(enumerators);
                enumerator = new OptionChainUniverseDataCollectionAggregatorEnumerator(sync, config.Symbol);

                enqueueable = new EnqueueableEnumerator<BaseData>(true);
                
                // add this enumerator to our exchange
                ScheduleEnumerator(enumerator, enqueueable, GetLowerThreshold(config.Resolution), GetUpperThreshold(config.Resolution));

                enumerator = enqueueable;
            }
            else
            {
                // normal reader for all others
                enumerator = CreateSubscriptionEnumerator(security, config, localStartTime, localEndTime, MapFileResolver.Empty, tradeableDates, true, false);

                // route these custom subscriptions through the exchange for buffering
                enqueueable = new EnqueueableEnumerator<BaseData>(true);

                // add this enumerator to our exchange
                ScheduleEnumerator(enumerator, enqueueable, GetLowerThreshold(config.Resolution), GetUpperThreshold(config.Resolution));

                enumerator = enqueueable;
            }

            // create the subscription
            timeZoneOffsetProvider = new TimeZoneOffsetProvider(security.Exchange.TimeZone, startTimeUtc, endTimeUtc);
            subscription = new Subscription(universe, security, config, enumerator, timeZoneOffsetProvider, startTimeUtc, endTimeUtc, true);

            _subscriptions.TryAdd(subscription);

            UpdateFillForwardResolution();
        }

        /// <summary>
        /// Send an exit signal to the thread.
        /// </summary>
        public void Exit() {
            Log.Trace( "FileSystemDataFeed.Exit(): Exit triggered.");
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Updates the fill forward resolution by checking all existing subscriptions and
        /// selecting the smallest resoluton not equal to tick
        /// </summary>
        private void UpdateFillForwardResolution() {
            _fillForwardResolution.Value = _subscriptions
                .Where(x => !x.Configuration.IsInternalFeed)
                .Select(x => x.Configuration.Resolution)
                .Where(x => x != Resolution.Tick)
                .DefaultIfEmpty(Resolution.Minute)
                .Min().ToTimeSpan();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TimeSlice> GetEnumerator() {
            // compute initial frontier time
            _frontierUtc = GetInitialFrontierTime();
            Log.Trace( String.format( "FileSystemDataFeed.GetEnumerator(): Begin: %1$s UTC", _frontierUtc));

            syncer = new SubscriptionSynchronizer(_universeSelection);
            syncer.SubscriptionFinished += (sender, subscription) =>
            {
                RemoveSubscription(subscription.Configuration);
                    Log.Debug( String.format( "FileSystemDataFeed.GetEnumerator(): Finished subscription: %1$s at %2$s UTC", subscription.Security.Symbol.ID, _frontierUtc));
            };

            while (!_cancellationTokenSource.IsCancellationRequested) {
                TimeSlice timeSlice;
                DateTime nextFrontier;

                try
                {
                    timeSlice = syncer.Sync(_frontierUtc, Subscriptions, _algorithm.TimeZone, _algorithm.Portfolio.CashBook, out nextFrontier);
                }
                catch (Exception err) {
                    Log.Error(err);
                    continue;
                }
                
                // syncer returns MaxValue on failure/end of data
                if( timeSlice.Time != DateTime.MaxValue) {
                    yield return timeSlice;

                    // end of data signal
                    if( nextFrontier == DateTime.MaxValue) break;

                    _frontierUtc = nextFrontier;    
                }
                else if( timeSlice.SecurityChanges == SecurityChanges.None) {
                    // there's no more data to pull off, we're done (frontier is max value and no security changes)
                    break;
                }
            }

            //Close up all streams:
            foreach (subscription in Subscriptions) {
                subscription.Dispose();
            }

            Log.Trace( String.format( "FileSystemDataFeed.Run(): Data Feed Completed at %1$s UTC", _frontierUtc));
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Creates an enumerator for the specified security/configuration
        /// </summary>
        private IEnumerator<BaseData> CreateSubscriptionEnumerator(Security security,
            SubscriptionDataConfig config,
            DateTime localStartTime,
            DateTime localEndTime,
            MapFileResolver mapFileResolver,
            IEnumerable<DateTime> tradeableDates,
            boolean useSubscriptionDataReader,
            boolean aggregate) {
            IEnumerator<BaseData> enumerator;
            if( useSubscriptionDataReader) {
                enumerator = new SubscriptionDataReader(config, localStartTime, localEndTime, _resultHandler, mapFileResolver,
                _factorFileProvider, tradeableDates, false);
            }
            else
            {
                sourceFactory = (BaseData)Activator.CreateInstance(config.Type);
                enumerator = (from date in tradeableDates
                              let source = sourceFactory.GetSource(config, date, false)
                              let factory = SubscriptionDataSourceReader.ForSource(source, config, date, false)
                              let entriesForDate = factory.Read(source)
                              from entry in entriesForDate
                              select entry).GetEnumerator();
            }

            if( aggregate) {
                enumerator = new BaseDataCollectionAggregatorEnumerator(enumerator, config.Symbol);
            }

            // optionally apply fill forward logic, but never for tick data
            if( config.FillDataForward && config.Resolution != Resolution.Tick) {
                enumerator = new FillForwardEnumerator(enumerator, security.Exchange, _fillForwardResolution,
                    security.IsExtendedMarketHours, localEndTime, config.Resolution.ToTimeSpan());
            }

            // optionally apply exchange/user filters
            if( config.IsFilteredSubscription) {
                enumerator = SubscriptionFilterEnumerator.WrapForDataFeed(_resultHandler, enumerator, security, localEndTime);
            }
            return enumerator;
        }

        private static int GetLowerThreshold(Resolution resolution) {
            switch (resolution) {
                case Resolution.Tick:
                    return 500;

                case Resolution.Second:
                case Resolution.Minute:
                case Resolution.Hour:
                case Resolution.Daily:
                    return 250;

                default:
                    throw new ArgumentOutOfRangeException( "resolution", resolution, null );
            }
        }

        private static int GetUpperThreshold(Resolution resolution) {
            switch (resolution) {
                case Resolution.Tick:
                    return 10000;

                case Resolution.Second:
                case Resolution.Minute:
                case Resolution.Hour:
                case Resolution.Daily:
                    return 5000;

                default:
                    throw new ArgumentOutOfRangeException( "resolution", resolution, null );
            }
        }
    }
}
