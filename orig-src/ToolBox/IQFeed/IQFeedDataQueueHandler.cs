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

using NodaTime;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using HistoryRequest = QuantConnect.Data.HistoryRequest;
using Timer = System.Timers.Timer;

package com.quantconnect.lean.ToolBox.IQFeed
{
    /**
    /// IQFeedDataQueueHandler is an implementation of IDataQueueHandler and IHistoryProvider
    */
    public class IQFeedDataQueueHandler : IDataQueueHandler, IHistoryProvider
    {
        private boolean _isConnected;
        private int _dataPointCount;
        private final HashSet<Symbol> _symbols;
        private final object _sync = new object();

        //Socket connections:
        private AdminPort _adminPort;
        private Level1Port _level1Port;
        private HistoryPort _historyPort;
        private BlockingCollection<BaseData> _outputCollection;

        /**
        /// Gets the total number of data points emitted by this history provider
        */
        public int DataPointCount
        {
            get { return _dataPointCount; }
        }

        /**
        /// IQFeedDataQueueHandler is an implementation of IDataQueueHandler:
        */
        public IQFeedDataQueueHandler() {
            _symbols = new HashSet<Symbol>();
            _outputCollection = new BlockingCollection<BaseData>();
            if( !IsConnected) Connect();
        }

        /**
        /// Get the next ticks from the live trading data queue
        */
        @returns IEnumerable list of ticks since the last update.
        public IEnumerable<BaseData> GetNextTicks() {
            foreach (tick in _outputCollection.GetConsumingEnumerable()) {
                yield return tick;
            }
        }

        /**
        /// Adds the specified symbols to the subscription: new IQLevel1WatchItem( "IBM", true)
        */
         * @param job">Job we're subscribing for:
         * @param symbols">The symbols to be added keyed by SecurityType
        public void Subscribe(LiveNodePacket job, IEnumerable<Symbol> symbols) {
            try
            {
                foreach (symbol in symbols) {
                    if( CanSubscribe(symbol)) {
                        lock (_sync) {
                            Log.Trace( "IQFeed.Subscribe(): Subscribe Request: " + symbol.toString());

                            type = symbol.ID.SecurityType;
                            if( _symbols.Add(symbol)) {
                                ticker = symbol.Value;
                                if( type == SecurityType.Forex) ticker += ".FXCM";
                                _level1Port.Subscribe(ticker);

                                Log.Trace( "IQFeed.Subscribe(): Subscribe Processed: " + symbol.toString());
                            }
                        }
                    }
                }
            }
            catch (Exception err) {
                Log.Error( "IQFeed.Subscribe(): " + err.Message);
            }
        }

        /**
        /// Removes the specified symbols to the subscription
        */
         * @param job">Job we're processing.
         * @param symbols">The symbols to be removed keyed by SecurityType
        public void Unsubscribe(LiveNodePacket job, IEnumerable<Symbol> symbols) {
            try
            {
                foreach (symbol in symbols) {
                    lock (_sync) {
                        Log.Trace( "IQFeed.Unsubscribe(): " + symbol.toString());
                        type = symbol.ID.SecurityType;
                        _symbols.Remove(symbol);
                        ticker = symbol.Value;
                        if( type == SecurityType.Forex) ticker += ".FXCM";

                        if( _level1Port.Contains(ticker)) {
                            _level1Port.Unsubscribe(ticker);
                        }
                    }
                }
            }
            catch (Exception err) {
                Log.Error( "IQFeed.Unsubscribe(): " + err.Message);
            }
        }

        /**
        /// Initializes this history provider to work for the specified job
        */
         * @param job">The job
         * @param mapFileProvider">Provider used to get a map file resolver to handle equity mapping
         * @param factorFileProvider">Provider used to get factor files to handle equity price scaling
         * @param statusUpdate">Function used to send status updates
        public void Initialize(AlgorithmNodePacket job, IMapFileProvider mapFileProvider, IFactorFileProvider factorFileProvider, Action<Integer> statusUpdate) {
            return;
        }

        /**
        /// Gets the history for the requested securities
        */
         * @param requests">The historical data requests
         * @param sliceTimeZone">The time zone used when time stamping the slice instances
        @returns An enumerable of the slices of data covering the span specified in each request
        public IEnumerable<Slice> GetHistory(IEnumerable<HistoryRequest> requests, ZoneId sliceTimeZone) {
            foreach (request in requests) {
                foreach (slice in _historyPort.ProcessHistoryRequests(request)) {
                    yield return slice;
                }
            }
        }

        /**
        /// Indicates the connection is live.
        */
        private boolean IsConnected
        {
            get { return _isConnected; }
        }

        /**
        /// Connect to the IQ Feed using supplied username and password information.
        */
        private void Connect() {
            try
            {
                //Launch the IQ Feed Application:
                Log.Trace( "IQFeed.Connect(): Launching client...");

                connector = new IQConnect(Config.Get( "iqfeed-productName"), "1.0");
                connector.Launch();

                // Initialise one admin port
                Log.Trace( "IQFeed.Connect(): Connecting to admin...");
                _adminPort = new AdminPort();
                _adminPort.Connect();
                _adminPort.SetAutoconnect();
                _adminPort.SetClientStats(false);
                _adminPort.SetClientName( "Admin");

                _adminPort.DisconnectedEvent += AdminPortOnDisconnectedEvent;
                _adminPort.ConnectedEvent += AdminPortOnConnectedEvent;

                Log.Trace( "IQFeed.Connect(): Connecting to L1 data...");
                _level1Port = new Level1Port(_outputCollection);
                _level1Port.Connect();
                _level1Port.SetClientName( "Level1");

                Log.Trace( "IQFeed.Connect(): Connecting to Historical data...");
                _historyPort = new HistoryPort();
                _historyPort.Connect();
                _historyPort.SetClientName( "History");

                _isConnected = true;
            }
            catch (Exception err) {
                Log.Error( "IQFeed.Connect(): Error Connecting to IQFeed: " + err.Message);
                _isConnected = false;
            }
        }

        /**
        /// Disconnect from all ports we're subscribed to:
        */
        /// 
        /// Not being used. IQ automatically disconnect on killing LEAN
        /// 
        private void Disconnect() {
            if( _adminPort != null ) _adminPort.Disconnect();
            if( _level1Port != null ) _level1Port.Disconnect();
            _isConnected = false;
            Log.Trace( "IQFeed.Disconnect(): Disconnected");
        }

        /**
        /// Returns true if this data provide can handle the specified symbol
        */
         * @param symbol">The symbol to be handled
        @returns True if this data provider can get data for the symbol, false otherwise
        private boolean CanSubscribe(Symbol symbol) {
            market = symbol.ID.Market;
            securityType = symbol.ID.SecurityType;
            return
                (securityType == SecurityType.Equity && market == Market.USA) ||
                (securityType == SecurityType.Forex && market == Market.FXCM);
        }

        /**
        /// Admin port is connected.
        */
        private void AdminPortOnConnectedEvent(object sender, ConnectedEventArgs connectedEventArgs) {
            _isConnected = true;
            Log.Error( "IQFeed.AdminPortOnConnectedEvent(): ADMIN PORT CONNECTED!");
        }

        /**
        /// Admin port disconnected from the IQFeed server.
        */
        private void AdminPortOnDisconnectedEvent(object sender, DisconnectedEventArgs disconnectedEventArgs) {
            _isConnected = false;
            Log.Error( "IQFeed.AdminPortOnDisconnectedEvent(): ADMIN PORT DISCONNECTED!");
        }

        /**
        /// Admin class type
        */
        public class AdminPort : IQAdminSocketClient
        {
            public AdminPort()
                : base(80) {
            }
        }

        /**
        /// Level 1 Data Request:
        */
        public class Level1Port : IQLevel1Client
        {
            private int count;
            private DateTime start;
            private DateTime _feedTime;
            private Stopwatch _stopwatch = new Stopwatch();
            private final Timer _timer;
            private final BlockingCollection<BaseData> _dataQueue;
            private final ConcurrentMap<String, double> _prices;

            public DateTime FeedTime
            {
                get
                {
                    if( _feedTime == new DateTime()) return DateTime.Now;
                    return _feedTime.AddMilliseconds(_stopwatch.ElapsedMilliseconds);
                }
                set
                {
                    _feedTime = value;
                    _stopwatch = Stopwatch.StartNew();
                }
            }

            public Level1Port(BlockingCollection<BaseData> dataQueue)
                : base(80) {
                start = DateTime.Now;
                _prices = new ConcurrentMap<String, double>();

                _dataQueue = dataQueue;
                Level1SummaryUpdateEvent += OnLevel1SummaryUpdateEvent;
                Level1TimerEvent += OnLevel1TimerEvent;
                Level1ServerDisconnectedEvent += OnLevel1ServerDisconnected;
                Level1ServerReconnectFailed += OnLevel1ServerReconnectFailed;
                Level1UnknownEvent += OnLevel1UnknownEvent;
                Level1FundamentalEvent += OnLevel1FundamentalEvent;

                _timer = new Timer(1000);
                _timer.Enabled = false;
                _timer.AutoReset = true;
                _timer.Elapsed += (sender, args) =>
                {
                    ticksPerSecond = count/(DateTime.Now - start).TotalSeconds;
                    if( ticksPerSecond > 1000 || _dataQueue.Count > 31) {
                        Log.Trace( String.format( "IQFeed.OnSecond(): Ticks/sec: %1$s Engine.Ticks.Count: %2$s CPU%: %3$s",
                            ticksPerSecond.toString( "0000.00"),
                            _dataQueue.Count,
                            OS.CpuUsage.NextValue().toString( "0.0") + "%"
                            ));
                    }

                    count = 0;
                    start = DateTime.Now;
                };

                _timer.Enabled = true;
            }

            private void OnLevel1FundamentalEvent(object sender, Level1FundamentalEventArgs e) {
                // handle split data, they're only valid today, they'll show up around 4:45am EST
                if( e.splitDate1.Date == DateTime.Today && DateTime.Now.TimeOfDay.TotalHours <= 8) // they will always be sent premarket
                {
                    // get the last price, if it doesn't exist then we'll just issue the split claiming the price was zero
                    // this should (ideally) never happen, but sending this without the price is much better then not sending
                    // it at all
                    double referencePrice;
                    _prices.TryGetValue(e.Symbol, out referencePrice);
                    sid = SecurityIdentifier.GenerateEquity(e.Symbol, Market.USA);
                    split = new Split(new Symbol(sid, e.Symbol), FeedTime, (decimal) referencePrice, (decimal) e.splitFactor1);
                    _dataQueue.Add(split);
                }
            }

            /**
            /// Handle a new price update packet:
            */
            private void OnLevel1SummaryUpdateEvent(object sender, Level1SummaryUpdateEventArgs e) {
                // if ticker is not found, unsubscribe
                if( e.NotFound) Unsubscribe(e.Symbol);

                // only update if we have a value
                if( e.Last == 0) return;

                // only accept trade updates
                if( e.TypeOfUpdate != Level1SummaryUpdateEventArgs.UpdateType.ExtendedTrade
                 && e.TypeOfUpdate != Level1SummaryUpdateEventArgs.UpdateType.Trade) return;

                count++;
                time = FeedTime;
                symbol = Symbol.Create(e.Symbol, SecurityType.Equity, Market.USA);
                last = (decimal)(e.TypeOfUpdate == Level1SummaryUpdateEventArgs.UpdateType.ExtendedTrade ? e.ExtendedTradingLast : e.Last);

                if( e.Symbol.Contains( ".FXCM")) {
                    // the feed time is in NYC/EDT, convert it into EST
                    time = FeedTime Extensions.convertTo(  )TimeZones.NewYork, TimeZones.EasternStandard);
                    symbol = Symbol.Create(e.Symbol.Replace( ".FXCM", string.Empty), SecurityType.Forex, Market.FXCM);
                }

                tick = new Tick(time, symbol, last, (decimal)e.Bid, (decimal)e.Ask) {
                    AskSize = e.AskSize,
                    BidSize = e.BidSize,
                    TickType = TickType.Trade,
                    Quantity = e.IncrementalVolume
                };

                _dataQueue.Add(tick);
                _prices[e.Symbol] = e.Last;
            }

            /**
            /// Set the interal clock time.
            */
            private void OnLevel1TimerEvent(object sender, Level1TimerEventArgs e) {
                //If there was a bad tick and the time didn't set right, skip setting it here and just use our millisecond timer to set the time from last time it was set.
                if( e.DateTimeStamp != DateTime.MinValue) {
                    FeedTime = e.DateTimeStamp;
                }
            }

            /**
            /// Server has disconnected, reconnect.
            */
            private void OnLevel1ServerDisconnected(object sender, Level1ServerDisconnectedArgs e) {
                Log.Error( "IQFeed.OnLevel1ServerDisconnected(): LEVEL 1 PORT DISCONNECTED! " + e.TextLine);
            }

            /**
            /// Server has disconnected, reconnect.
            */
            private void OnLevel1ServerReconnectFailed(object sender, Level1ServerReconnectFailedArgs e) {
                Log.Error( "IQFeed.OnLevel1ServerReconnectFailed(): LEVEL 1 PORT DISCONNECT! " + e.TextLine);
            }

            /**
            /// Got a message we don't know about, log it for posterity.
            */
            private void OnLevel1UnknownEvent(object sender, Level1TextLineEventArgs e) {
                Log.Error( "IQFeed.OnUnknownEvent(): " + e.TextLine);
            }
        }

        private static PeriodType GetPeriodType(Resolution resolution) {
            switch (resolution) {
                case Resolution.Second:
                    return PeriodType.Second;
                case Resolution.Minute:
                    return PeriodType.Minute;
                case Resolution.Hour:
                    return PeriodType.Hour;
                case Resolution.Tick:
                case Resolution.Daily:
                default:
                    throw new ArgumentOutOfRangeException( "resolution", resolution, null );
            }
        }

        // this type is expected to be used for exactly one job at a time
        public class HistoryPort : IQLookupHistorySymbolClient
        {
            private boolean _inProgress;
            private ConcurrentMap<String, HistoryRequest> _requestDataByRequestId;
            private ConcurrentMap<String, List<BaseData>> _currentRequest;
            private final String DataDirectory = Config.Get( "data-directory", "../../../Data");
            private final double MaxHistoryRequestMinutes = Config.GetDouble( "max-history-minutes", 5);

            /**
            /// ... 
            */
            public HistoryPort()
                : base(80) {
                _requestDataByRequestId = new ConcurrentMap<String, HistoryRequest>();
                _currentRequest = new ConcurrentMap<String, List<BaseData>>();
            }

            /**
            /// Populate request data
            */
            public IEnumerable<Slice> ProcessHistoryRequests(HistoryRequest request) {                
                // we can only process equity/forex types here
                if( request.SecurityType != SecurityType.Forex && request.SecurityType != SecurityType.Equity) {
                    yield break;
                }

                // Set this process status
                _inProgress = true;

                symbol = request.Symbol.Value;
                if( request.SecurityType == SecurityType.Forex) {
                    symbol = symbol + ".FXCM";
                }

                start = request.StartTimeUtc.ConvertFromUtc(TimeZones.NewYork);
                DateTime? end = request.EndTimeUtc.ConvertFromUtc(TimeZones.NewYork);
                // if we're within a minute of now, don't set the end time
                if( request.EndTimeUtc >= DateTime.UtcNow.AddMinutes(-1)) {
                    end = null;
                }

                Log.Trace( String.format( "HistoryPort.ProcessHistoryJob(): Submitting request: %1$s-%2$s: %3$s {3}->{4}", request.SecurityType, symbol, request.Resolution, start, end ?? DateTime.UtcNow.AddMinutes(-1)));

                int id;
                reqid = string.Empty;

                switch (request.Resolution) {
                    case Resolution.Tick:
                        id = RequestTickData(symbol, start, end, true);
                        reqid = CreateRequestID(LookupType.REQ_HST_TCK, id);
                        break;
                    case Resolution.Daily:
                        id = RequestDailyData(symbol, start, end, true);
                        reqid = CreateRequestID(LookupType.REQ_HST_DWM, id);
                        break;
                    default:
                        interval = new Interval(GetPeriodType(request.Resolution), 1);
                        id = RequestIntervalData(symbol, interval, start, end, true);
                        reqid = CreateRequestID(LookupType.REQ_HST_INT, id);
                        break;
                }

                _requestDataByRequestId[reqid] = request;

                while (_inProgress) {
                    continue;
                }

                // After all data arrive, we pass it to the algorithm through memory and write to a file
                foreach (key in _currentRequest.Keys) {
                    List<BaseData> tradeBars;
                    if( _currentRequest.TryRemove(key, out tradeBars)) {
                        foreach (tradeBar in tradeBars) {
                            // Returns IEnumerable<Slice> object
                            yield return new Slice(tradeBar.Time, new[] { tradeBar });
                        }
                    }
                }
            }

            /**
            /// Created new request ID for a given lookup type (tick, intraday bar, daily bar)
            */
             * @param lookupType">Lookup type: REQ_HST_TCK (tick), REQ_HST_DWM (daily) or REQ_HST_INT (intraday resolutions)
             * @param id">Sequential identifier
            @returns 
            private static String CreateRequestID(LookupType lookupType, int id) {
                return lookupType + id.toString( "0000000");
            }

            /**
            /// Method called when a new Lookup event is fired
            */
             * @param e">Received data
            protected @Override void OnLookupEvent(LookupEventArgs e) {
                try
                {
                    switch (e.Sequence) {
                        case LookupSequence.MessageStart:
                            _currentRequest.AddOrUpdate(e.Id, new List<BaseData>());
                            break;
                        case LookupSequence.MessageDetail:
                            List<BaseData> current;
                            if( _currentRequest.TryGetValue(e.Id, out current)) {
                                HandleMessageDetail(e, current);
                            }
                            break;
                        case LookupSequence.MessageEnd:
                            _inProgress = false;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception err) {
                    Log.Error(err);
                }
            }

            /**
            /// Put received data into current list of BaseData object
            */
             * @param e">Received data
             * @param current">Current list of BaseData object
            private void HandleMessageDetail(LookupEventArgs e, List<BaseData> current) {
                requestData = _requestDataByRequestId[e.Id];
                data = GetData(e, requestData);
                if( data != null && data.Time != DateTime.MinValue) {
                    current.Add(data);
                }
            }

            /**
            /// Transform received data into BaseData object
            */
             * @param e">Received data
             * @param requestData">Request information
            @returns BaseData object
            private BaseData GetData(LookupEventArgs e, HistoryRequest requestData) {
                isEquity = requestData.SecurityType == SecurityType.Equity;
                scale = isEquity ? 1000m : 1m;
                try
                {
                    switch (e.Type) {
                        case LookupType.REQ_HST_TCK:
                            t = (LookupTickEventArgs) e;
                            time = isEquity ? t.DateTimeStamp : t.DateTimeStamp Extensions.convertTo(  )TimeZones.NewYork, TimeZones.EasternStandard);
                            return new Tick(time, requestData.Symbol, (decimal) t.Last*scale, (decimal) t.Bid*scale, (decimal) t.Ask*scale);
                        case LookupType.REQ_HST_INT:
                            i = (LookupIntervalEventArgs) e;
                            if( i.DateTimeStamp == DateTime.MinValue) return null;
                            istartTime = i.DateTimeStamp - requestData.Resolution.ToTimeSpan();
                            if( !isEquity) istartTime = istartTime Extensions.convertTo(  )TimeZones.NewYork, TimeZones.EasternStandard);
                            return new TradeBar(istartTime, requestData.Symbol, (decimal) i.Open*scale, (decimal) i.High*scale, (decimal) i.Low*scale, (decimal) i.Close*scale, i.PeriodVolume);
                        case LookupType.REQ_HST_DWM:
                            d = (LookupDayWeekMonthEventArgs) e;
                            if( d.DateTimeStamp == DateTime.MinValue) return null;
                            dstartTime = d.DateTimeStamp - requestData.Resolution.ToTimeSpan();
                            if( !isEquity) dstartTime = dstartTime Extensions.convertTo(  )TimeZones.NewYork, TimeZones.EasternStandard);
                            return new TradeBar(dstartTime, requestData.Symbol, (decimal) d.Open*scale, (decimal) d.High*scale, (decimal) d.Low*scale, (decimal) d.Close*scale, d.PeriodVolume);

                        // we don't need to handle these other types
                        case LookupType.REQ_SYM_SYM:
                        case LookupType.REQ_SYM_SIC:
                        case LookupType.REQ_SYM_NAC:
                        case LookupType.REQ_TAB_MKT:
                        case LookupType.REQ_TAB_SEC:
                        case LookupType.REQ_TAB_MKC:
                        case LookupType.REQ_TAB_SIC:
                        case LookupType.REQ_TAB_NAC:
                        default:
                            return null;
                    }
                }
                catch (Exception err) {
                    Log.Error( "Encountered error while processing request: " + e.Id);
                    Log.Error(err);
                    return null;
                }
            }
        }
    }
}
