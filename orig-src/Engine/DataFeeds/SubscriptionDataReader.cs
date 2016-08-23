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
using System.Linq;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Auxiliary;
using QuantConnect.Data.Custom;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Logging;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.DataFeeds
{
    /**
    /// Subscription data reader is a wrapper on the stream reader class to download, unpack and iterate over a data file.
    */
    /// The class accepts any subscription configuration and automatically makes it availble to enumerate
    public class SubscriptionDataReader : IEnumerator<BaseData>
    {
        // Source String to create memory stream:
        private SubscriptionDataSource _source;

        private boolean _endOfStream;

        private IEnumerator<BaseData> _subscriptionFactoryEnumerator;

        /// Configuration of the data-reader:
        private final SubscriptionDataConfig _config;

        /// true if we can find a scale factor file for the security of the form: ..\Lean\Data\equity\market\factor_files\{SYMBOL}.csv
        private final boolean _hasScaleFactors;

        // Symbol Mapping:
        private String _mappedSymbol = "";

        // Location of the datafeed - the type of this data.

        // Create a single instance to invoke all Type Methods:
        private final BaseData _dataFactory;

        //Start finish times of the backtest:
        private final DateTime _periodStart;
        private final DateTime _periodFinish;

        private final FactorFile _factorFile;
        private final MapFile _mapFile;

        // we set the price factor ratio when we encounter a dividend in the factor file
        // and on the next trading day we use this data to produce the dividend instance
        private decimal? _priceFactorRatio;

        // we set the split factor when we encounter a split in the factor file
        // and on the next trading day we use this data to produce the split instance
        private decimal? _splitFactor;

        // we'll use these flags to denote we've already fired off the DelistedType.Warning
        // and a DelistedType.Delisted Delisting object, the _delistingType object is save here
        // since we need to wait for the next trading day before emitting
        private boolean _delisted;
        private boolean _delistedWarning;

        // true if we're in live mode, false otherwise
        private final boolean _isLiveMode;
        private final boolean _includeAuxilliaryData;

        private BaseData _previous;
        private final Queue<BaseData> _auxiliaryData;
        private final IResultHandler _resultHandler;
        private final IEnumerator<DateTime> _tradeableDates;

        // used when emitting aux data from within while loop
        private boolean _emittedAuxilliaryData;
        private BaseData _lastInstanceBeforeAuxilliaryData;

        /**
        /// Last read BaseData object from this type and source
        */
        public BaseData Current
        {
            get;
            private set;
        }

        /**
        /// Explicit Interface Implementation for Current
        */
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /**
        /// Subscription data reader takes a subscription request, loads the type, accepts the data source and enumerate on the results.
        */
         * @param config">Subscription configuration object
         * @param periodStart">Start date for the data request/backtest
         * @param periodFinish">Finish date for the data request/backtest
         * @param resultHandler">Result handler used to push error messages and perform sampling on skipped days
         * @param mapFileResolver">Used for resolving the correct map files
         * @param factorFileProvider">Used for getting factor files
         * @param tradeableDates">Defines the dates for which we'll request data, in order, in the security's exchange time zone
         * @param isLiveMode">True if we're in live mode, false otherwise
         * @param includeAuxilliaryData">True if we want to emit aux data, false to only emit price data
        public SubscriptionDataReader(SubscriptionDataConfig config,
            DateTime periodStart,
            DateTime periodFinish,
            IResultHandler resultHandler,
            MapFileResolver mapFileResolver,
            IFactorFileProvider factorFileProvider,
            IEnumerable<DateTime> tradeableDates,
            boolean isLiveMode,
            boolean includeAuxilliaryData = true) {
            //Save configuration of data-subscription:
            _config = config;

            _auxiliaryData = new Queue<BaseData>();

            //Save Start and End Dates:
            _periodStart = periodStart;
            _periodFinish = periodFinish;

            //Save access to securities
            _isLiveMode = isLiveMode;
            _includeAuxilliaryData = includeAuxilliaryData;

            //Save the type of data we'll be getting from the source.

            //Create the dynamic type-activators:
            objectActivator = ObjectActivator.GetActivator(config.Type);

            _resultHandler = resultHandler;
            _tradeableDates = tradeableDates.GetEnumerator();
            if( objectActivator == null ) {
                _resultHandler.ErrorMessage( "Custom data type '" + config.Type.Name + "' missing parameterless constructor E.g. public " + config.Type.Name + "() { }");
                _endOfStream = true;
                return;
            }

            //Create an instance of the "Type":
            userObj = objectActivator.Invoke(new object[] {});
            _dataFactory = userObj as BaseData;

            //If its quandl set the access token in data factory:
            quandl = _dataFactory as Quandl;
            if( quandl != null ) {
                if( !Quandl.IsAuthCodeSet) {
                    Quandl.SetAuthCode(Config.Get( "quandl-auth-token"));
                }
            }

            _factorFile = new FactorFile(config.Symbol.Value, new List<FactorFileRow>());
            _mapFile = new MapFile(config.Symbol.Value, new List<MapFileRow>());

            // load up the map and factor files for equities
            if( !config.IsCustomData && config.SecurityType == SecurityType.Equity) {
                try
                {
                    mapFile = mapFileResolver.ResolveMapFile(config.Symbol.ID.Symbol, config.Symbol.ID.Date);

                    // only take the resolved map file if it has data, otherwise we'll use the empty one we defined above
                    if( mapFile.Any()) _mapFile = mapFile;

                    factorFile = factorFileProvider.Get(_config.Symbol);
                    _hasScaleFactors = factorFile != null;
                    if( _hasScaleFactors) {
                        _factorFile = factorFile;
                    }
                }
                catch (Exception err) {
                    Log.Error(err, "Fetching Price/Map Factors: " + config.Symbol.ID + ": ");
                }
            }

            _subscriptionFactoryEnumerator = ResolveDataEnumerator(true);
        }

        /**
        /// Advances the enumerator to the next element of the collection.
        */
        @returns 
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// 
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public boolean MoveNext() {
            if( _endOfStream) {
                return false;
            }

            if( Current != null && Current.DataType != MarketDataType.Auxiliary) {
                // only save previous price data
                _previous = Current;
            }

            if( _subscriptionFactoryEnumerator == null ) {
                // in live mode the trade able dates will eventually advance to the next
                if( _isLiveMode) {
                    // HACK attack -- we don't want to block in live mode
                    Current = null;
                    return true;
                }

                _endOfStream = true;
                return false;
            }

            do
            {
                // check for aux data first
                if( HasAuxDataBefore(_lastInstanceBeforeAuxilliaryData)) {
                    // check for any auxilliary data before reading a line, but make sure
                    // it should be going ahead of '_lastInstanceBeforeAuxilliaryData'
                    Current = _auxiliaryData.Dequeue();
                    return true;
                }

                if( _emittedAuxilliaryData) {
                    _emittedAuxilliaryData = false;
                    Current = _lastInstanceBeforeAuxilliaryData;
                    _lastInstanceBeforeAuxilliaryData = null;
                    return true;
                }

                // keep enumerating until we find something that is within our time frame
                while (_subscriptionFactoryEnumerator.MoveNext()) {
                    instance = _subscriptionFactoryEnumerator.Current;
                    if( instance == null ) {
                        // keep reading until we get valid data
                        continue;
                    }

                    // prevent emitting past data, this can happen when switching symbols on daily data
                    if( _previous != null && _config.Resolution != Resolution.Tick) {
                        if( _config.IsCustomData) {
                            // Skip the point if time went backwards for custom data?
                            // TODO: Should this be the case for all datapoints?
                            if( instance.EndTime < _previous.EndTime) continue;
                        }
                        else
                        {
                            // all other resolutions don't allow duplicate end times
                            if( instance.EndTime <= _previous.EndTime) continue;
                        }
                    }

                    if( instance.EndTime < _periodStart) {
                        // keep reading until we get a value on or after the start
                        _previous = instance;
                        continue;
                    }

                    if( instance.Time > _periodFinish) {
                        // stop reading when we get a value after the end
                        _endOfStream = true;
                        return false;
                    }

                    // if we move past our current 'date' then we need to do daily things, such
                    // as updating factors and symbol mapping as well as detecting aux data
                    if( instance.EndTime.Date > _tradeableDates.Current) {
                        // this is fairly hacky and could be solved by removing the aux data from this class
                        // the case is with coarse data files which have many daily sized data points for the
                        // same date,
                        if( !_config.IsInternalFeed) {
                            // this will advance the date enumerator and determine if a new
                            // instance of the subscription enumerator is required
                            _subscriptionFactoryEnumerator = ResolveDataEnumerator(false);
                        }

                        // we produce auxiliary data on date changes, but make sure our current instance
                        // isn't before it in time
                        if( HasAuxDataBefore(instance)) {
                            // since we're emitting this here we need to save off the instance for next time
                            Current = _auxiliaryData.Dequeue();
                            _emittedAuxilliaryData = true;
                            _lastInstanceBeforeAuxilliaryData = instance;
                            return true;
                        }
                    }

                    // we've made it past all of our filters, we're withing the requested start/end of the subscription,
                    // we've satisfied user and market hour filters, so this data is good to go as current
                    Current = instance;
                    return true;
                }

                // we've ended the enumerator, time to refresh
                _subscriptionFactoryEnumerator = ResolveDataEnumerator(true);
            }
            while (_subscriptionFactoryEnumerator != null );

            _endOfStream = true;
            return false;
        }

        private boolean HasAuxDataBefore(BaseData instance) {
            // this function is always used to check for aux data, as such, we'll implement the
            // feature of whether to include or not here so if other aux data is added we won't
            // need to remember this feature. this is mostly here until aux data gets moved into
            // its own subscription class
            if( !_includeAuxilliaryData) _auxiliaryData.Clear();
            if( _auxiliaryData.Count == 0) return false;
            if( instance == null ) return true;
            return _auxiliaryData.Peek().EndTime < instance.EndTime;
        }

        /**
        /// Resolves the next enumerator to be used in <see cref="MoveNext"/>
        */
        private IEnumerator<BaseData> ResolveDataEnumerator( boolean endOfEnumerator) {
            do
            {
                // always advance the date enumerator, this function is intended to be
                // called on date changes, never return null for live mode, we'll always
                // just keep trying to refresh the subscription
                DateTime date;
                if( !TryGetNextDate(out date) && !_isLiveMode) {
                    // if we run out of dates then we're finished with this subscription
                    return null;
                }

                // fetch the new source, using the data time zone for the date
                dateInDataTimeZone = date Extensions.convertTo(  )_config.ExchangeTimeZone, _config.DataTimeZone);
                newSource = _dataFactory.GetSource(_config, dateInDataTimeZone, _isLiveMode);

                // check if we should create a new subscription factory
                sourceChanged = _source != newSource && newSource.Source != "";
                liveRemoteFile = _isLiveMode && (_source == null || _source.TransportMedium == SubscriptionTransportMedium.RemoteFile);
                if( sourceChanged || liveRemoteFile) {
                    // dispose of the current enumerator before creating a new one
                    if( _subscriptionFactoryEnumerator != null ) {
                        _subscriptionFactoryEnumerator.Dispose();
                    }

                    // save off for comparison next time
                    _source = newSource;
                    subscriptionFactory = CreateSubscriptionFactory(newSource);
                    return subscriptionFactory.Read(newSource).GetEnumerator();
                }

                // if there's still more in the enumerator and we received the same source from the GetSource call
                // above, then just keep using the same enumerator as we were before
                if( !endOfEnumerator) // && !sourceChanged is always true here
                {
                    return _subscriptionFactoryEnumerator;
                }

                // keep churning until we find a new source or run out of tradeable dates
                // in live mode tradeable dates won't advance beyond today's date, but
                // TryGetNextDate will return false if it's already at today
            }
            while (true);
        }

        private ISubscriptionDataSourceReader CreateSubscriptionFactory(SubscriptionDataSource source) {
            factory = SubscriptionDataSourceReader.ForSource(source, _config, _tradeableDates.Current, _isLiveMode);
            AttachEventHandlers(factory, source);
            return factory;
        }

        private void AttachEventHandlers(ISubscriptionDataSourceReader dataSourceReader, SubscriptionDataSource source) {
            // handle missing files
            dataSourceReader.InvalidSource += (sender, args) =>
            {
                switch (args.Source.TransportMedium) {
                    case SubscriptionTransportMedium.LocalFile:
                        // the local uri doesn't exist, write an error and return null so we we don't try to get data for today
                        Log.Trace( String.format( "SubscriptionDataReader.GetReader(): Could not find QC Data, skipped: %1$s", source));
                        _resultHandler.SamplePerformance(_tradeableDates.Current, 0);
                        break;

                    case SubscriptionTransportMedium.RemoteFile:
                        _resultHandler.ErrorMessage( String.format( "Error downloading custom data source file, skipped: %1$s Error: %2$s", source, args.Exception.Message), args.Exception.StackTrace);
                        _resultHandler.SamplePerformance(_tradeableDates.Current.Date, 0);
                        break;

                    case SubscriptionTransportMedium.Rest:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            if( dataSourceReader is TextSubscriptionDataSourceReader) {
                // handle empty files/instantiation errors
                textSubscriptionFactory = (TextSubscriptionDataSourceReader)dataSourceReader;
                textSubscriptionFactory.CreateStreamReaderError += (sender, args) =>
                {
                    Log.Error( String.format( "Failed to get StreamReader for data source(%1$s), symbol(%2$s). Skipping date(%3$s). Reader is null.", args.Source.Source, _mappedSymbol, args.Date.ToShortDateString()));
                    if( _config.IsCustomData) {
                        _resultHandler.ErrorMessage( String.format( "We could not fetch the requested data. This may not be valid data, or a failed download of custom data. Skipping source (%1$s).", args.Source.Source));
                    }
                };

                // handle parser errors
                textSubscriptionFactory.ReaderError += (sender, args) =>
                {
                    _resultHandler.RuntimeError( String.format( "Error invoking %1$s data reader. Line: %2$s Error: %3$s", _config.Symbol, args.Line, args.Exception.Message), args.Exception.StackTrace);
                };
            }
        }

        /**
        /// Iterates the tradeable dates enumerator
        */
         * @param date">The next tradeable date
        @returns True if we got a new date from the enumerator, false if it's exhausted, or in live mode if we're already at today
        private boolean TryGetNextDate(out DateTime date) {
            if( _isLiveMode && _tradeableDates.Current >= DateTime.Today) {
                // special behavior for live mode, don't advance past today
                date = _tradeableDates.Current;
                return false;
            }

            while (_tradeableDates.MoveNext()) {
                date = _tradeableDates.Current;

                CheckForDelisting(date);

                if( !_mapFile.HasData(date)) {
                    continue;
                }

                // don't do other checks if we haven't goten data for this date yet
                if( _previous != null && _previous.EndTime > _tradeableDates.Current) {
                    continue;
                }

                // check for dividends and split for this security
                CheckForDividend(date);
                CheckForSplit(date);

                // if we have factor files check to see if we need to update the scale factors
                if( _hasScaleFactors) {
                    // check to see if the symbol was remapped
                    newSymbol = _mapFile.GetMappedSymbol(date);
                    if( _mappedSymbol != "" && newSymbol != _mappedSymbol) {
                        changed = new SymbolChangedEvent(_config.Symbol, date, _mappedSymbol, newSymbol);
                        _auxiliaryData.Enqueue(changed);
                    }
                    _config.MappedSymbol = _mappedSymbol = newSymbol;

                    // update our price scaling factors in light of the normalization mode
                    UpdateScaleFactors(date);
                }

                // we've passed initial checks,now go get data for this date!
                return true;
            }

            // no more tradeable dates, we've exhausted the enumerator
            date = DateTime.MaxValue.Date;
            return false;
        }

        /**
        /// For backwards adjusted data the price is adjusted by a scale factor which is a combination of splits and dividends. 
        /// This backwards adjusted price is used by default and fed as the current price.
        */
         * @param date">Current date of the backtest.
        private void UpdateScaleFactors(DateTime date) {
            switch (_config.DataNormalizationMode) {
                case DataNormalizationMode.Raw:
                    return;

                case DataNormalizationMode.TotalReturn:
                case DataNormalizationMode.splitAdjusted:
                    _config.PriceScaleFactor = _factorFile.GetSplitFactor(date);
                    break;

                case DataNormalizationMode.Adjusted:
                    _config.PriceScaleFactor = _factorFile.GetPriceScaleFactor(date);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /**
        /// Reset the IEnumeration
        */
        /// Not used
        public void Reset() {
            throw new NotImplementedException( "Reset method not implemented. Assumes loop will only be used once.");
        }

        /**
        /// Check for dividends and emit them into the aux data queue
        */
        private void CheckForSplit(DateTime date) {
            if( _splitFactor != null ) {
                close = GetRawClose();
                split = new Split(_config.Symbol, date, close, _splitFactor.Value);
                _auxiliaryData.Enqueue(split);
                _splitFactor = null;
            }

            BigDecimal splitFactor;
            if( _factorFile.HasSplitEventOnNextTradingDay(date, out splitFactor)) {
                _splitFactor = splitFactor;
            }
        }

        /**
        /// Check for dividends and emit them into the aux data queue
        */
        private void CheckForDividend(DateTime date) {
            if( _priceFactorRatio != null ) {
                close = GetRawClose();
                dividend = new Dividend(_config.Symbol, date, close, _priceFactorRatio.Value);
                // let the config know about it for normalization
                _config.SumOfDividends += dividend.Distribution;
                _auxiliaryData.Enqueue(dividend);
                _priceFactorRatio = null;
            }

            // check the factor file to see if we have a dividend event tomorrow
            BigDecimal priceFactorRatio;
            if( _factorFile.HasDividendEventOnNextTradingDay(date, out priceFactorRatio)) {
                _priceFactorRatio = priceFactorRatio;
            }
        }

        /**
        /// Check for delistings and emit them into the aux data queue
        */
        private void CheckForDelisting(DateTime date) {
            // these ifs set flags to tell us to produce a delisting instance
            if( !_delistedWarning && date >= _mapFile.DelistingDate) {
                _delistedWarning = true;
                price = _previous != null ? _previous.Price : 0;
                _auxiliaryData.Enqueue(new Delisting(_config.Symbol, date, price, DelistingType.Warning));
            }
            else if( !_delisted && date > _mapFile.DelistingDate) {
                _delisted = true;
                price = _previous != null ? _previous.Price : 0;
                // delisted at EOD
                _auxiliaryData.Enqueue(new Delisting(_config.Symbol, _mapFile.DelistingDate.AddDays(1), price, DelistingType.Delisted));
            }
        }

        /**
        /// Un-normalizes the Previous.Value
        */
        private BigDecimal GetRawClose() {
            if( _previous == null ) return 0m;

            close = _previous.Value;

            switch (_config.DataNormalizationMode) {
                case DataNormalizationMode.Raw:
                    break;

                case DataNormalizationMode.splitAdjusted:
                case DataNormalizationMode.Adjusted:
                    // we need to 'unscale' the price
                    close = close / _config.PriceScaleFactor;
                    break;

                case DataNormalizationMode.TotalReturn:
                    // we need to remove the dividends since we've been accumulating them in the price
                    close = (close - _config.SumOfDividends) / _config.PriceScaleFactor;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return close;
        }

        /**
        /// Dispose of the Stream Reader and close out the source stream and file connections.
        */
        public void Dispose() {
            if( _subscriptionFactoryEnumerator != null ) {
                _subscriptionFactoryEnumerator.Dispose();
            }
        }
    }
}