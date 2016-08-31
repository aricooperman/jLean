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
using System.Linq.Expressions;
using NodaTime;
using NodaTime.TimeZones;
using QuantConnect.Benchmarks;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Data.Auxiliary;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using QuantConnect.Interfaces;
using QuantConnect.Notifications;
using QuantConnect.Orders;
using QuantConnect.Parameters;
using QuantConnect.Scheduling;
using QuantConnect.Securities;
using QuantConnect.Securities.Cfd;
using QuantConnect.Securities.Equity;
using QuantConnect.Securities.Forex;
using QuantConnect.Securities.Option;
using QuantConnect.Statistics;
using QuantConnect.Util;
using SecurityTypeMarket = System.Tuple<QuantConnect.SecurityType,String>;

package com.quantconnect.lean.Algorithm
{
    /**
     * QC Algorithm Base Class - Handle the basic requirements of a trading algorithm, 
     * allowing user to focus on event methods. The QCAlgorithm class implements Portfolio, 
     * Securities, Transactions and Data Subscription Management.
    */
    public partial class QCAlgorithm : MarshalByRefObject, IAlgorithm
    {
        private final TimeKeeper _timeKeeper;
        private LocalTimeKeeper _localTimeKeeper;

        private DateTime _startDate;   //Default start and end dates.
        private DateTime _endDate;     //Default end to yesterday
        private RunMode _runMode = RunMode.Series;
        private boolean _locked;
        private boolean _liveMode;
        private String _algorithmId = "";
        private List<String> _debugMessages = new List<String>();
        private List<String> _logMessages = new List<String>();
        private List<String> _errorMessages = new List<String>();
        
        //Error tracking to avoid message flooding:
        private String _previousDebugMessage = "";
        private String _previousErrorMessage = "";

        private final MarketHoursDatabase _marketHoursDatabase;
        private final SymbolPropertiesDatabase _symbolPropertiesDatabase;

        // used for calling through to void OnData(Slice) if no @Override specified
        private boolean _checkedForOnDataSlice;
        private Action<Slice> _onDataSlice;

        // set by SetBenchmark helper API functions
        private Symbol _benchmarkSymbol = QuantConnect.Symbol.Empty;

        // flips to true when the user
        private boolean _userSetSecurityInitializer = false;

        // warmup resolution variables
        private TimeSpan? _warmupTimeSpan;
        private OptionalInt _warmupBarCount;
        private Map<String,String> _parameters = new Map<String,String>();

        /**
         * QCAlgorithm Base Class Constructor - Initialize the underlying QCAlgorithm components.
         * QCAlgorithm manages the transactions, portfolio, charting and security subscriptions for the users algorithms.
        */
        public QCAlgorithm() {
            Status = AlgorithmStatus.Running;

            // AlgorithmManager will flip this when we're caught up with realtime
            IsWarmingUp = true;

            //Initialise the Algorithm Helper Classes:
            //- Note - ideally these wouldn't be here, but because of the DLL we need to make the classes shared across 
            //  the Worker & Algorithm, limiting ability to do anything else.

            //Initialise Start and End Dates:
            _startDate = new DateTime(1998, 01, 01);
            _endDate = DateTime.Now.AddDays(-1);

            // intialize our time keeper with only new york
            _timeKeeper = new TimeKeeper(_startDate, new[] { TimeZones.NewYork });
            // set our local time zone
            _localTimeKeeper = _timeKeeper.GetLocalTimeKeeper(TimeZones.NewYork);

            //Initialise Data Manager 
            SubscriptionManager = new SubscriptionManager(_timeKeeper);

            Securities = new SecurityManager(_timeKeeper);
            Transactions = new SecurityTransactionManager(Securities);
            Portfolio = new SecurityPortfolioManager(Securities, Transactions);
            BrokerageModel = new DefaultBrokerageModel();
            Notify = new NotificationManager(false); // Notification manager defaults to disabled.

            //Initialise Algorithm RunMode to Series - Parallel Mode deprecated:
            _runMode = RunMode.Series;

            //Initialise to unlocked:
            _locked = false;

            // get exchange hours loaded from the market-hours-database.csv in /Data/market-hours
            _marketHoursDatabase = MarketHoursDatabase.FromDataFolder();

            // get symbol properties loaded from the symbol-properties-database.csv in /Data/symbol-properties
            _symbolPropertiesDatabase = SymbolPropertiesDatabase.FromDataFolder();

            // universe selection
            UniverseManager = new UniverseManager();
            Universe = new UniverseDefinitions(this);
            UniverseSettings = new UniverseSettings(Resolution.Minute, 2m, true, false, Duration.ofDays(1));

            // initialize our scheduler, this acts as a liason to the real time handler
            Schedule = new ScheduleManager(Securities, TimeZone);

            // initialize the trade builder
            TradeBuilder = new TradeBuilder(FillGroupingMethod.FillToFill, FillMatchingMethod.FIFO);

            SecurityInitializer = new BrokerageModelSecurityInitializer(new DefaultBrokerageModel(AccountType.Margin));

            CandlestickPatterns = new CandlestickPatterns(this);
        }

        /**
         * Security collection is an array of the security objects such as Equities and FOREX. Securities data 
         * manages the properties of tradeable assets such as price, open and close time and holdings information.
        */
        public SecurityManager Securities
        {
            get;
            set;
        }

        /**
         * Portfolio object provieds easy access to the underlying security-holding properties; summed together in a way to make them useful.
         * This saves the user time by providing common portfolio requests in a single 
        */
        public SecurityPortfolioManager Portfolio
        {
            get;
            set;
        }

        /**
         * Generic Data Manager - Required for compiling all data feeds in order, and passing them into algorithm event methods.
         * The subscription manager contains a list of the data feed's we're subscribed to and properties of each data feed.
        */
        public SubscriptionManager SubscriptionManager
        {
            get;
            set;
        }

        /**
         * Gets the brokerage model - used to model interactions with specific brokerages.
        */
        public IBrokerageModel BrokerageModel
        {
            get;
            private set;
        }

        /**
         * Gets the brokerage message handler used to decide what to do
         * with each message sent from the brokerage
        */
        public IBrokerageMessageHandler BrokerageMessageHandler
        {
            get;
            set;
        }

        /**
         * Notification Manager for Sending Live Runtime Notifications to users about important events.
        */
        public NotificationManager Notify
        {
            get;
            set;
        }

        /**
         * Gets schedule manager for adding/removing scheduled events
        */
        public ScheduleManager Schedule
        {
            get;
            private set;
        }

        /**
         * Gets or sets the current status of the algorithm
        */
        public AlgorithmStatus Status
        {
            get;
            set;
        }

        /**
         * Gets an instance that is to be used to initialize newly created securities.
        */
        public ISecurityInitializer SecurityInitializer
        {
            get; 
            private set;
        }

        /**
         * Gets the Trade Builder to generate trades from executions
        */
        public TradeBuilder TradeBuilder
        {
            get;
            private set;
        }

        /**
         * Gets an instance to access the candlestick pattern helper methods
        */
        public CandlestickPatterns CandlestickPatterns
        {
            get;
            private set;
        }

        /**
         * Gets the date rules helper object to make specifying dates for events easier
        */
        public DateRules DateRules
        {
            get { return Schedule.DateRules; }
        }

        /**
         * Gets the time rules helper object to make specifying times for events easier
        */
        public TimeRules TimeRules
        {
            get { return Schedule.TimeRules; }
        }

        /**
         * Public name for the algorithm as automatically generated by the IDE. Intended for helping distinguish logs by noting 
         * the algorithm-id.
        */
         * <seealso cref="AlgorithmId"/>
        public String Name
        {
            get;
            set;
        }

        /**
         * Read-only value for current time frontier of the algorithm in terms of the <see cref="TimeZone"/>
        */
         * During backtesting this is primarily sourced from the data feed. During live trading the time is updated from the system clock.
        public DateTime Time
        {
            get { return _localTimeKeeper.LocalTime; }
        }

        /**
         * Current date/time in UTC.
        */
        public DateTime UtcTime
        {
            get { return _timeKeeper.UtcTime; }
        }

        /**
         * Gets the time zone used for the <see cref="Time"/> property. The default value
         * is <see cref="TimeZones.NewYork"/>
        */
        public ZoneId TimeZone
        {
            get { return _localTimeKeeper.TimeZone; }
        }

        /**
         * Value of the user set start-date from the backtest. 
        */
         * This property is set with SetStartDate() and defaults to the earliest QuantConnect data available - Jan 1st 1998. It is ignored during live trading 
         * <seealso cref="SetStartDate(DateTime)"/>
        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
        }

        /**
         * Value of the user set start-date from the backtest. Controls the period of the backtest.
        */
         *  This property is set with SetEndDate() and defaults to today. It is ignored during live trading.
         * <seealso cref="SetEndDate(DateTime)"/>
        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
        }

        /**
         * Algorithm Id for this backtest or live algorithm. 
        */
         * A unique identifier for 
        public String AlgorithmId
        {
            get
            {
                return _algorithmId;
            }
        }

        /**
         * Control the server setup run style for the backtest: Automatic, Parallel or Series. 
        */
         * <remark>
         *     Series mode runs all days through one computer, allowing memory of the previous days. 
         *     Parallel mode runs all days separately which maximises speed but gives no memory of a previous day trading.
         * </remark>
         * <obsolete>The RunMode enum propert is now obsolete. All algorithms will default to RunMode.Series for series backtests.</obsolete>
        [Obsolete( "The RunMode enum propert is now obsolete. All algorithms will default to RunMode.Series for series backtests.")]
        public RunMode RunMode
        {
            get
            {
                return _runMode;
            }
        }

        /**
         * Boolean property indicating the algorithm is currently running in live mode. 
        */
         * Intended for use where certain behaviors will be enabled while the algorithm is trading live: such as notification emails, or displaying runtime statistics.
        public boolean LiveMode
        {
            get
            {
                return _liveMode;
            }
        }

        /**
         * Storage for debugging messages before the event handler has passed control back to the Lean Engine.
        */
         * <seealso cref="Debug( String)"/>
        public List<String> DebugMessages
        {
            get
            {
                return _debugMessages;
            }
            set
            {
                _debugMessages = value;
            }
        }

        /**
         * Storage for log messages before the event handlers have passed control back to the Lean Engine.
        */
         * <seealso cref="Log( String)"/>
        public List<String> LogMessages
        {
            get
            {
                return _logMessages;
            }
            set
            {
                _logMessages = value;
            }
        }

        /**
         * Gets the run time error from the algorithm, or null if none was encountered.
        */
        public Exception RunTimeError { get; set; }

        /**
         * List of error messages generated by the user's code calling the "Error" function.
        */
         * This method is best used within a try-catch bracket to handle any runtime errors from a user algorithm.
         * <see cref="Error( String)"/>
        public List<String> ErrorMessages
        {
            get
            {
                return _errorMessages;
            }
            set
            {
                _errorMessages = value;
            }
        }

        /**
         * Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        */
         * <seealso cref="SetStartDate(DateTime)"/>
         * <seealso cref="SetEndDate(DateTime)"/>
         * <seealso cref="SetCash(decimal)"/>
        public void Initialize() {
            //Setup Required Data
            throw new NotImplementedException( "Please @Override the Initialize() method");
        }

        /**
         * Called by setup handlers after Initialize and allows the algorithm a chance to organize
         * the data gather in the Initialize method
        */
        public void PostInitialize() {
            // if the benchmark hasn't been set yet, set it
            if( Benchmark == null ) {
                // apply the default benchmark if it hasn't been set
                if( _benchmarkSymbol == null || _benchmarkSymbol == QuantConnect.Symbol.Empty) {
                    _benchmarkSymbol = QuantConnect.Symbol.Create( "SPY", SecurityType.Equity, Market.USA);
                }

                // if the requested benchmark symbol wasn't already added, then add it now
                // we do a simple compare here for simplicity, also it avoids confusion over
                // the desired market.
                Security security;
                if( !Securities.TryGetValue(_benchmarkSymbol, out security)) {
                    // add the security as an internal feed so the algorithm doesn't receive the data
                    Resolution resolution;
                    if( _liveMode) {
                        resolution = Resolution.Second;
                    }
                    else
                    {
                        // check to see if any universes arn't the ones added via AddSecurity
                        hasNonAddSecurityUniverses = (
                            from kvp in UniverseManager
                            let config = kvp.Value.Configuration
                            let symbol = UserDefinedUniverse.CreateSymbol(config.SecurityType, config.Market)
                            where config.Symbol != symbol
                            select kvp).Any();

                        resolution = hasNonAddSecurityUniverses ? UniverseSettings.Resolution : Resolution.Daily;
                    }
                    security = SecurityManager.CreateSecurity(Portfolio, SubscriptionManager, _marketHoursDatabase, _symbolPropertiesDatabase, SecurityInitializer, _benchmarkSymbol, resolution, true, 1m, false, true, false);
                    AddToUserDefinedUniverse(security);
                }

                // just return the current price
                Benchmark = new SecurityBenchmark(security);
            }

            // add option underlying securities if not present
            foreach (option in Securities.Select(x -> x.Value).OfType<Option>()) {
                sid = option.Symbol.ID;
                underlying = QuantConnect.Symbol.Create(sid.Symbol, SecurityType.Equity, sid.Market);
                Security equity;
                if( !Securities.TryGetValue(underlying, out equity)) {
                    // if it wasn't manually added, add a daily subscription for volatility calculations
                    equity = AddEquity(underlying.Value, Resolution.Daily, underlying.ID.Market, false);
                }

                // set the underlying property on the option chain
                option.Underlying = equity;

                // check for the null volatility model and update it
                if( equity.VolatilityModel == VolatilityModel.Null) {
                    static final int period = 50;
                    // define indicator for daily close to prevent multiple consolidators per indicator
                    dailyClose = Identity(equity.Symbol, Resolution.Daily);
                    meanName = CreateIndicatorName(equity.Symbol, "SMA" + period, Resolution.Daily);
                    stdName = CreateIndicatorName(equity.Symbol, "STD" + period, Resolution.Daily);
                    mean = new SimpleMovingAverage(meanName, period).Of(dailyClose);
                    std = new StandardDeviation(stdName, period).Of(dailyClose);
                    // the model wants a % volatility
                    volatility = std.Over(mean);
                    equity.VolatilityModel = new IndicatorVolatilityModel<IndicatorDataPoint>(volatility);
                }
            }
        }

        /**
         * Gets the parameter with the specified name. If a parameter
         * with the specified name does not exist, null is returned
        */
         * @param name The name of the parameter to get
        @returns The value of the specified parameter, or null if not found
        public String GetParameter( String name) {
            String value;
            return _parameters.TryGetValue(name, out value) ? value : null;
        }

        /**
         * Sets the parameters from the dictionary
        */
         * @param parameters Dictionary containing the parameter names to values
        public void SetParameters(Map<String,String> parameters) {
            // save off a copy and try to apply the parameters
            _parameters = parameters.ToDictionary();
            try
            {
                ParameterAttribute.ApplyAttributes(parameters, this);
            }
            catch (Exception err) {
                Error( "Error applying parameter values: " + err.Message);
            }
        }

        /**
         * Sets the security initializer, used to initialize/configure securities after creation
        */
         * @param securityInitializer The security initializer
        public void SetSecurityInitializer(ISecurityInitializer securityInitializer) {
            // this flag will prevent calls to SetBrokerageModel from overwriting this initializer
            _userSetSecurityInitializer = true;
            SecurityInitializer = securityInitializer;
        }

        /**
         * Sets the security initializer function, used to initialize/configure securities after creation
        */
         * @param securityInitializer The security initializer function
        public void SetSecurityInitializer(Action<Security> securityInitializer) {
            SetSecurityInitializer(new FuncSecurityInitializer(securityInitializer));
        }

        /**
         * Event - v3.0 DATA EVENT HANDLER: (Pattern) Basic template for user to @Override for receiving all subscription data in a single event
        */
         * <code>
         * TradeBars bars = slice.Bars;
         * Ticks ticks = slice.Ticks;
         * TradeBar spy = slice["SPY"];
         * List{Tick} aaplTicks = slice["AAPL"]
         * Quandl oil = slice["OIL"]
         * dynamic anySymbol = slice[symbol];
         * DataDictionary{Quandl} allQuandlData = slice.Get{Quand}
         * Quandl oil = slice.Get{Quandl}( "OIL")
         * </code>
         * @param slice The current slice of data keyed by symbol string
        public void OnData(Slice slice) {
            // as a default implementation, let's look for and call OnData(Slice) just in case a user forgot to use the @Override keyword
            if( !_checkedForOnDataSlice) {
                _checkedForOnDataSlice = true;
                
                method = GetType().GetMethods()
                    .Where(x -> x.Name == "OnData")
                    .Where(x -> x.DeclaringType != typeof(QCAlgorithm))
                    .Where(x -> x.GetParameters().Length == 1)
                    .FirstOrDefault(x -> x.GetParameters()[0].ParameterType == typeof (Slice));

                if( method == null ) {
                    return;
                }

                self = Expression.Constant(this);
                parameter = Expression.Parameter(typeof (Slice), "data");
                call = Expression.Call(self, method, parameter);
                lambda = Expression.Lambda<Action<Slice>>(call, parameter);
                _onDataSlice = lambda.Compile();
            }
            // if we have it, then invoke it
            if( _onDataSlice != null ) {
                _onDataSlice(slice);
            }
        }

        /**
         * Event fired each time the we add/remove securities from the data feed
        */
         * @param changes">
        public void OnSecuritiesChanged(SecurityChanges changes) {
        }

        // <summary>
        // Event - v2.0 TRADEBAR EVENT HANDLER: (Pattern) Basic template for user to @Override when requesting tradebar data.
        // </summary>
        // <param name="data">
        //public void OnData(TradeBars data)
        //{
        //
        //}

        // <summary>
        // Event - v2.0 QUOTEBAR EVENT HANDLER: (Pattern) Basic template for user to @Override when requesting quotebar data.
        // </summary>
        // <param name="data">
        //public void OnData(QuoteBars data)
        //{
        //
        //}

        // <summary>
        // Event - v2.0 TICK EVENT HANDLER: (Pattern) Basic template for user to @Override when requesting tick data.
        // </summary>
        // <param name="data List of Tick Data
        //public void OnData(Ticks data)
        //{
        //
        //}

        // <summary>
        // Event - v2.0 OPTIONCHAIN EVENT HANDLER: (Pattern) Basic template for user to @Override when requesting option data.
        // </summary>
        // <param name="data List of Tick Data
        //public void OnData(OptionChains data)
        //{
        //
        //}

        // <summary>
        // Event - v2.0 SPLIT EVENT HANDLER: (Pattern) Basic template for user to @Override when inspecting split data.
        // </summary>
        // <param name="data IDictionary of Split Data Keyed by Symbol String
        //public void OnData(Splits data)
        //{
        //
        //}

        // <summary>
        // Event - v2.0 DIVIDEND EVENT HANDLER: (Pattern) Basic template for user to @Override when inspecting dividend data
        // </summary>
        // <param name="data IDictionary of Dividend Data Keyed by Symbol String
        //public void OnData(Dividends data)
        //{
        //
        //}

        // <summary>
        // Event - v2.0 DELISTING EVENT HANDLER: (Pattern) Basic template for user to @Override when inspecting delisting data
        // </summary>
        // <param name="data IDictionary of Delisting Data Keyed by Symbol String
        //public void OnData(Delistings data)

        // <summary>
        // Event - v2.0 SYMBOL CHANGED EVENT HANDLER: (Pattern) Basic template for user to @Override when inspecting symbol changed data
        // </summary>
        // <param name="data IDictionary of SymbolChangedEvent Data Keyed by Symbol String
        //public void OnData(SymbolChangedEvents data)

        /**
         * Margin call event handler. This method is called right before the margin call orders are placed in the market.
        */
         * @param requests The orders to be executed to bring this algorithm within margin limits
        public void OnMarginCall(List<SubmitOrderRequest> requests) {
        }

        /**
         * Margin call warning event handler. This method is called when Portfolio.MarginRemaining is under 5% of your Portfolio.TotalPortfolioValue
        */
        public void OnMarginCallWarning() {
        }

        /**
         * End of a trading day event handler. This method is called at the end of the algorithm day (or multiple times if trading multiple assets).
        */
         * Method is called 10 minutes before closing to allow user to close out position.
        public void OnEndOfDay() {

        }

        /**
         * End of a trading day event handler. This method is called at the end of the algorithm day (or multiple times if trading multiple assets).
        */
         * 
         * This method is left for backwards compatibility and is invoked via <see cref="OnEndOfDay(Symbol)"/>, if that method is
         * @Override then this method will not be called without a called to base.OnEndOfDay( String)
         * 
         * @param symbol Asset symbol for this end of day event. Forex and equities have different closing hours.
        public void OnEndOfDay( String symbol) {
        }

        /**
         * End of a trading day event handler. This method is called at the end of the algorithm day (or multiple times if trading multiple assets).
        */
         * @param symbol Asset symbol for this end of day event. Forex and equities have different closing hours.
        public void OnEndOfDay(Symbol symbol) {
            OnEndOfDay(symbol.toString());
        }

        /**
         * End of algorithm run event handler. This method is called at the end of a backtest or live trading operation. Intended for closing out logs.
        */
        public void OnEndOfAlgorithm() { 
            
        }

        /**
         * Order fill event handler. On an order fill update the resulting information is passed to this method.
        */
         * @param orderEvent Order event details containing details of the evemts
         * This method can be called asynchronously and so should only be used by seasoned C# experts. Ensure you use proper locks on thread-unsafe objects
        public void OnOrderEvent(OrderEvent orderEvent) {
   
        }

        /**
         * Brokerage message event handler. This method is called for all types of brokerage messages.
        */
        public void OnBrokerageMessage(BrokerageMessageEvent messageEvent) {
            
        }

        /**
         * Brokerage disconnected event handler. This method is called when the brokerage connection is lost.
        */
        public void OnBrokerageDisconnect() {

        }

        /**
         * Brokerage reconnected event handler. This method is called when the brokerage connection is restored after a disconnection.
        */
        public void OnBrokerageReconnect() {

        }

        /**
         * Update the internal algorithm time frontier.
        */
         * For internal use only to advance time.
         * @param frontier Current datetime.
        public void SetDateTime(DateTime frontier) {
            _timeKeeper.SetUtcDateTime(frontier);
        }

        /**
         * Sets the time zone of the <see cref="Time"/> property in the algorithm
        */
         * @param timeZone The desired time zone
        public void SetTimeZone( String timeZone) {
            ZoneId tz;
            try
            {
                tz = ZoneIdProviders.Tzdb[timeZone];
            }
            catch (ZoneIdNotFoundException) {
                throw new IllegalArgumentException( String.format( "TimeZone with id '%1$s' was not found. For a complete list of time zones please visit: http://en.wikipedia.org/wiki/List_of_tz_database_time_zones", timeZone));
            }

            SetTimeZone(tz);
        }

        /**
         * Sets the time zone of the <see cref="Time"/> property in the algorithm
        */
         * @param timeZone The desired time zone
        public void SetTimeZone(ZoneId timeZone) {
            if( _locked) {
                throw new Exception( "Algorithm.SetTimeZone(): Cannot change time zone after algorithm running.");
            }

            if( timeZone == null ) throw new ArgumentNullException( "timeZone");
            _timeKeeper.AddTimeZone(timeZone);
            _localTimeKeeper = _timeKeeper.GetLocalTimeKeeper(timeZone);

            // the time rules need to know the default time zone as well
            TimeRules.SetDefaultTimeZone(timeZone);
        }

        /**
         * Set the RunMode for the Servers. If you are running an overnight algorithm, you must select series.
         * Automatic will analyse the selected data, and if you selected only minute data we'll select series for you.
        */
         * <obsolete>This method is now obsolete and has no replacement. All algorithms now run in Series mode.</obsolete>
         * @param mode Enum RunMode with options Series, Parallel or Automatic. Automatic scans your requested symbols and resolutions and makes a decision on the fastest analysis
        [Obsolete( "This method is now obsolete and has no replacement. All algorithms now run in Series mode.")]
        public void SetRunMode(RunMode mode) {
            if( mode != RunMode.Parallel) return;
            Debug( "Algorithm.SetRunMode(): RunMode-Parallel Type has been deprecated. Series analysis selected instead");
        }

        /**
         * Sets the brokerage to emulate in backtesting or paper trading.
         * This can be used for brokerages that have been implemented in LEAN
        */
         * @param brokerage The brokerage to emulate
         * @param accountType The account type (Cash or Margin)
        public void SetBrokerageModel(BrokerageName brokerage, AccountType accountType = AccountType.Margin) {
            SetBrokerageModel(Brokerages.BrokerageModel.Create(brokerage, accountType));
        }

        /**
         * Sets the brokerage to emulate in backtesting or paper trading.
         * This can be used to set a custom brokerage model.
        */
         * @param model The brokerage model to use
        public void SetBrokerageModel(IBrokerageModel model) {
            BrokerageModel = model;
            if( !_userSetSecurityInitializer) {
                // purposefully use the direct setter vs Set method so we don't flip the switch :/
                SecurityInitializer = new BrokerageModelSecurityInitializer(model);
            }
        }

        /**
         * Sets the implementation used to handle messages from the brokerage.
         * The default implementation will forward messages to debug or error
         * and when a <see cref="BrokerageMessageType.Error"/> occurs, the algorithm
         * is stopped.
        */
         * @param handler The message handler to use
        public void SetBrokerageMessageHandler(IBrokerageMessageHandler handler) {
            if( handler == null ) {
                throw new ArgumentNullException( "handler");
            }

            BrokerageMessageHandler = handler;
        }

        /**
         * Sets the benchmark used for computing statistics of the algorithm to the specified symbol
        */
         * @param symbol symbol to use as the benchmark
         * @param securityType Is the symbol an equity, forex, base, etc. Default SecurityType.Equity
         * 
         * Must use symbol that is available to the trade engine in your data store(not strictly enforced)
         * 
        public void SetBenchmark(SecurityType securityType, String symbol) {
            market = securityType == SecurityType.Forex ? Market.FXCM : Market.USA;
            _benchmarkSymbol = QuantConnect.Symbol.Create(symbol, securityType, market);
        }

        /**
         * Sets the benchmark used for computing statistics of the algorithm to the specified symbol, defaulting to SecurityType.Equity
         * if the symbol doesn't exist in the algorithm
        */
         * @param symbol symbol to use as the benchmark
         * 
         * Overload to accept symbol without passing SecurityType. If symbol is in portfolio it will use that SecurityType, otherwise will default to SecurityType.Equity
         * 
        public void SetBenchmark( String symbol) {
            // check existence
            symbol = symbol.toUpperCase();
            security = Securities.FirstOrDefault(x -> x.Key.Value == symbol).Value;
            _benchmarkSymbol = security == null 
                ? QuantConnect.Symbol.Create(symbol, SecurityType.Equity, Market.USA)
                : security.Symbol;
        }

        /**
         * Sets the benchmark used for computing statistics of the algorithm to the specified symbol
        */
         * @param symbol symbol to use as the benchmark
        public void SetBenchmark(Symbol symbol) {
            _benchmarkSymbol = symbol;
        }

        /**
         * Sets the specified function as the benchmark, this function provides the value of
         * the benchmark at each date/time requested
        */
         * @param benchmark The benchmark producing function
        public void SetBenchmark(Func<DateTime, decimal> benchmark) {
            Benchmark = new FuncBenchmark(benchmark);
        }

        /**
         * Benchmark
        */
         * Use Benchmark to @Override default symbol based benchmark, and create your own benchmark. For example a custom moving average benchmark 
         * 
        public IBenchmark Benchmark
        {
            get;
            private set;
        }

        /**
         * Set initial cash for the strategy while backtesting. During live mode this value is ignored 
         * and replaced with the actual cash of your brokerage account.
        */
         * @param startingCash Starting cash for the strategy backtest
         * Alias of SetCash(decimal)
        public void SetCash(double startingCash) {
            SetCash((decimal)startingCash);
        }

        /**
         * Set initial cash for the strategy while backtesting. During live mode this value is ignored 
         * and replaced with the actual cash of your brokerage account.
        */
         * @param startingCash Starting cash for the strategy backtest
         * Alias of SetCash(decimal)
        public void SetCash(int startingCash) {
            SetCash((decimal)startingCash);
        }

        /**
         * Set initial cash for the strategy while backtesting. During live mode this value is ignored 
         * and replaced with the actual cash of your brokerage account.
        */
         * @param startingCash Starting cash for the strategy backtest
        public void SetCash( BigDecimal startingCash) {
            if( !_locked) {
                Portfolio.SetCash(startingCash);
            }
            else
            {
                throw new Exception( "Algorithm.SetCash(): Cannot change cash available after algorithm initialized.");
            }
        }

        /**
         * Set the cash for the specified symbol
        */
         * @param symbol The cash symbol to set
         * @param startingCash Decimal cash value of portfolio
         * @param conversionRate The current conversion rate for the
        public void SetCash( String symbol, BigDecimal startingCash, BigDecimal conversionRate) {
            if( !_locked) {
                Portfolio.SetCash(symbol, startingCash, conversionRate);
            }
            else
            {
                throw new Exception( "Algorithm.SetCash(): Cannot change cash available after algorithm initialized.");
            }
        }

        /**
         * Set the start date for backtest.
        */
         * @param day Int starting date 1-30
         * @param month Int month starting date
         * @param year Int year starting date
         *  
         *     Wrapper for SetStartDate(DateTime). 
         *     Must be less than end date. 
         *     Ignored in live trading mode.
         * 
        public void SetStartDate(int year, int month, int day) {
            try
            {
                start = new DateTime(year, month, day);

                // We really just want the date of the start, so it's 12am of the requested day (first moment of the day)
                start = start.Date;

                SetStartDate(start);
            }
            catch (Exception err) {
                throw new Exception( "Date Invalid: " + err.Message);
            }
        }

        /**
         * Set the end date for a backtest run 
        */
         * @param day Int end date 1-30
         * @param month Int month end date
         * @param year Int year end date
         * Wrapper for SetEndDate(datetime).
         * <seealso cref="SetEndDate(DateTime)"/>
        public void SetEndDate(int year, int month, int day) {
            try
            {
                end = new DateTime(year, month, day);

                // we want the end date to be just before the next day (last moment of the day)
                end = end.Date.AddDays(1).Subtract(Duration.ofTicks(1));

                SetEndDate(end);
            }
            catch (Exception err) {
                throw new Exception( "Date Invalid: " + err.Message);
            }
        }

        /**
         * Set the algorithm id (backtestId or live deployId for the algorithmm).
        */
         * @param algorithmId String Algorithm Id
         * Intended for internal QC Lean Engine use only as a setter for AlgorihthmId
        public void SetAlgorithmId( String algorithmId) {
            _algorithmId = algorithmId;
        }

        /**
         * Set the start date for the backtest 
        */
         * @param start Datetime Start date for backtest
         * Must be less than end date and within data available
         * <seealso cref="SetStartDate(DateTime)"/>
        public void SetStartDate(DateTime start) {
            // no need to set this value in live mode, will be set using the current time.
            if( _liveMode) return;

            //Validate the start date:
            //1. Check range;
            if( start < (new DateTime(1900, 01, 01))) {
                throw new Exception( "Please select a start date after January 1st, 1900.");
            }

            //2. Check end date greater:
            if( _endDate != new DateTime()) {
                if( start > _endDate) {
                    throw new Exception( "Please select start date less than end date.");
                }
            }

            //3. Round up and subtract one tick:
            start = start.RoundDown(Duration.ofDays(1));

            //3. Check not locked already:
            if( !_locked) {
                // this is only or backtesting
                if( !LiveMode) {
                    _startDate = start;
                    SetDateTime(_startDate Extensions.convertToUtc(TimeZone));
                }
            } 
            else
            {
                throw new Exception( "Algorithm.SetStartDate(): Cannot change start date after algorithm initialized.");
            }
        }

        /**
         * Set the end date for a backtest.
        */
         * @param end Datetime value for end date
         * Must be greater than the start date
         * <seealso cref="SetEndDate(DateTime)"/>
        public void SetEndDate(DateTime end) {
            // no need to set this value in live mode, will be set using the current time.
            if( _liveMode) return;

            //Validate:
            //1. Check Range:
            if( end > DateTime.Now.Date.AddDays(-1)) {
                end = DateTime.Now.Date.AddDays(-1);
            }

            //2. Check start date less:
            if( _startDate != new DateTime()) {
                if( end < _startDate) {
                    throw new Exception( "Please select end date greater than start date.");
                }
            }

            //3. Make this at the very end of the requested date
            end = end.RoundDown(Duration.ofDays(1)).AddDays(1).AddTicks(-1);

            //4. Check not locked already:
            if( !_locked) {
                _endDate = end;
            }
            else 
            {
                throw new Exception( "Algorithm.SetEndDate(): Cannot change end date after algorithm initialized.");
            }
        }

        /**
         * Lock the algorithm initialization to avoid user modifiying cash and data stream subscriptions
        */
         * Intended for Internal QC Lean Engine use only to prevent accidental manipulation of important properties
        public void SetLocked() {
            _locked = true;
        }

        /**
         * Gets whether or not this algorithm has been locked and fully initialized
        */
        public boolean GetLocked() {
            return _locked;
        }

        /**
         * Set live mode state of the algorithm run: Public setter for the algorithm property LiveMode.
        */
        public void SetLiveMode( boolean live) {
            if( !_locked) {
                _liveMode = live;
                Notify = new NotificationManager(live);
                TradeBuilder.SetLiveMode(live);

                if( live) {
                    _startDate = DateTime.Today;
                    _endDate = QuantConnect.Time.EndOfTime;
                }
            }
        }

        /**
         * Add specified data to our data subscriptions. QuantConnect will funnel this data to the handle data routine.
        */
         * @param securityType MarketType Type: Equity, Commodity, Future or FOREX
         * @param symbol Symbol Reference for the MarketType
         * @param resolution Resolution of the Data Required
         * @param fillDataForward When no data available on a tradebar, return the last data that was generated
         * @param extendedMarketHours Show the after market data as well
        public Security AddSecurity(SecurityType securityType, String symbol, Resolution resolution = Resolution.Minute, boolean fillDataForward = true, boolean extendedMarketHours = false) {
            return AddSecurity(securityType, symbol, resolution, fillDataForward, 0, extendedMarketHours);
        }

        /**
         * Add specified data to required list. QC will funnel this data to the handle data routine.
        */
         * @param securityType MarketType Type: Equity, Commodity, Future or FOREX
         * @param symbol Symbol Reference for the MarketType
         * @param resolution Resolution of the Data Required
         * @param fillDataForward When no data available on a tradebar, return the last data that was generated
         * @param leverage Custom leverage per security
         * @param extendedMarketHours Extended market hours
         *  AddSecurity(SecurityType securityType, Symbol symbol, Resolution resolution, boolean fillDataForward, BigDecimal leverage, boolean extendedMarketHours)
        public Security AddSecurity(SecurityType securityType, String symbol, Resolution resolution, boolean fillDataForward, BigDecimal leverage, boolean extendedMarketHours) {
            return AddSecurity(securityType, symbol, resolution, null, fillDataForward, leverage, extendedMarketHours);
        }

        /**
         * Set a required SecurityType-symbol and resolution for algorithm
        */
         * @param securityType SecurityType Enum: Equity, Commodity, FOREX or Future
         * @param symbol Symbol Representation of the MarketType, e.g. AAPL
         * @param resolution Resolution of the MarketType required: MarketData, Second or Minute
         * @param market The market the requested security belongs to, such as 'usa' or 'fxcm'
         * @param fillDataForward If true, returns the last available data even if none in that timeslice.
         * @param leverage leverage for this security
         * @param extendedMarketHours ExtendedMarketHours send in data from 4am - 8pm, not used for FOREX
        public Security AddSecurity(SecurityType securityType, String symbol, Resolution resolution, String market, boolean fillDataForward, BigDecimal leverage, boolean extendedMarketHours) {
            try
            {
                if( market == null ) {
                    if( !BrokerageModel.DefaultMarkets.TryGetValue(securityType, out market)) {
                        throw new Exception( "No default market set for security type: " + securityType);
                    }
                }

                Symbol symbolObject;
                if( !SymbolCache.TryGetSymbol(symbol, out symbolObject)) {
                    symbolObject = QuantConnect.Symbol.Create(symbol, securityType, market);
                }

                security = SecurityManager.CreateSecurity(Portfolio, SubscriptionManager, _marketHoursDatabase, _symbolPropertiesDatabase, SecurityInitializer,
                    symbolObject, resolution, fillDataForward, leverage, extendedMarketHours, false, false);

                AddToUserDefinedUniverse(security);
                return security;
            }
            catch (Exception err) {
                Error( "Algorithm.AddSecurity(): " + err);
                return null;
            }
        }

        /**
         * Creates and adds a new <see cref="Equity"/> security to the algorithm
        */
         * @param ticker The equity ticker symbol
         * @param resolution The <see cref="Resolution"/> of market data, Tick, Second, Minute, Hour, or Daily. Default is <see cref="Resolution.Minute"/>
         * @param market The equity's market, <seealso cref="Market"/>. Default is <see cref="Market.USA"/>
         * @param fillDataForward If true, returns the last available data even if none in that timeslice. Default is <value>true
         * @param leverage The requested leverage for this equity. Default is set by <see cref="SecurityInitializer"/>
         * @param extendedMarketHours True to send data during pre and post market sessions. Default is <value>false
        @returns The new <see cref="Equity"/> security
        public Equity AddEquity( String ticker, Resolution resolution = Resolution.Minute, String market = Market.USA, boolean fillDataForward = true, BigDecimal leverage = BigDecimal.ZERO, boolean extendedMarketHours = false) {
            return AddSecurity<Equity>(SecurityType.Equity, ticker, resolution, market, fillDataForward, leverage, extendedMarketHours);
        }

        /**
         * Creates and adds a new equity <see cref="Option"/> security to the algorithm
        */
         * @param underlying The underlying equity symbol
         * @param resolution The <see cref="Resolution"/> of market data, Tick, Second, Minute, Hour, or Daily. Default is <see cref="Resolution.Minute"/>
         * @param market The equity's market, <seealso cref="Market"/>. Default is <see cref="Market.USA"/>
         * @param fillDataForward If true, returns the last available data even if none in that timeslice. Default is <value>true
         * @param leverage The requested leverage for this equity. Default is set by <see cref="SecurityInitializer"/>
        @returns The new <see cref="Option"/> security
        public Option AddOption( String underlying, Resolution resolution = Resolution.Minute, String market = Market.USA, boolean fillDataForward = true, BigDecimal leverage = BigDecimal.ZERO) {
            if( market == null ) {
                if( !BrokerageModel.DefaultMarkets.TryGetValue(SecurityType.Option, out market)) {
                    throw new Exception( "No default market set for security type: " + SecurityType.Option);
                }
            }

            Symbol canonicalSymbol;
            alias = "?" + underlying;
            if( !SymbolCache.TryGetSymbol(alias, out canonicalSymbol)) {
                canonicalSymbol = QuantConnect.Symbol.Create(underlying, SecurityType.Option, market, alias);
            }

            marketHoursEntry = _marketHoursDatabase.GetEntry(market, underlying, SecurityType.Option);
            symbolProperties = _symbolPropertiesDatabase.GetSymbolProperties(market, underlying, SecurityType.Option, CashBook.AccountCurrency);
            canonicalSecurity = (Option) SecurityManager.CreateSecurity(typeof (ZipEntryName), Portfolio, SubscriptionManager,
                marketHoursEntry.ExchangeHours, marketHoursEntry.DataTimeZone, symbolProperties, SecurityInitializer, canonicalSymbol, resolution,
                fillDataForward, leverage, false, false, false, true, false);
            canonicalSecurity.IsTradable = false;
            Securities.Add(canonicalSecurity);

            // add this security to the user defined universe
            Universe universe;
            if( !UniverseManager.TryGetValue(canonicalSymbol, out universe)) {
                settings = new UniverseSettings(resolution, leverage, false, false, Duration.ZERO);
                universe = new OptionChainUniverse(canonicalSecurity, settings, SecurityInitializer);
                UniverseManager.Add(canonicalSymbol, universe);
            }

            return canonicalSecurity;
        }

        /**
         * Creates and adds a new <see cref="Forex"/> security to the algorithm
        */
         * @param ticker The currency pair
         * @param resolution The <see cref="Resolution"/> of market data, Tick, Second, Minute, Hour, or Daily. Default is <see cref="Resolution.Minute"/>
         * @param market The foreign exchange trading market, <seealso cref="Market"/>. Default is <see cref="Market.FXCM"/>
         * @param fillDataForward If true, returns the last available data even if none in that timeslice. Default is <value>true
         * @param leverage The requested leverage for this equity. Default is set by <see cref="SecurityInitializer"/>
        @returns The new <see cref="Forex"/> security
        public Forex AddForex( String ticker, Resolution resolution = Resolution.Minute, String market = Market.FXCM, boolean fillDataForward = true, BigDecimal leverage = BigDecimal.ZERO) {
            return AddSecurity<Forex>(SecurityType.Forex, ticker, resolution, market, fillDataForward, leverage, false);
        }

        /**
         * Creates and adds a new <see cref="Cfd"/> security to the algorithm
        */
         * @param ticker The currency pair
         * @param resolution The <see cref="Resolution"/> of market data, Tick, Second, Minute, Hour, or Daily. Default is <see cref="Resolution.Minute"/>
         * @param market The cfd trading market, <seealso cref="Market"/>. Default is <see cref="Market.FXCM"/>
         * @param fillDataForward If true, returns the last available data even if none in that timeslice. Default is <value>true
         * @param leverage The requested leverage for this equity. Default is set by <see cref="SecurityInitializer"/>
        @returns The new <see cref="Cfd"/> security
        public Cfd AddCfd( String ticker, Resolution resolution = Resolution.Minute, String market = Market.FXCM, boolean fillDataForward = true, BigDecimal leverage = BigDecimal.ZERO) {
            return AddSecurity<Cfd>(SecurityType.Cfd, ticker, resolution, market, fillDataForward, leverage, false);
        }

        /**
         * Removes the security with the specified symbol. This will cancel all
         * open orders and then liquidate any existing holdings
        */
         * @param symbol The symbol of the security to be removed
        public boolean RemoveSecurity(Symbol symbol) {
            Security security;
            if( Securities.TryGetValue(symbol, out security)) {
                // cancel open orders
                Transactions.CancelOpenOrders(security.Symbol);

                // liquidate if invested
                if( security.Invested) Liquidate(security.Symbol);
                
                universe = UniverseManager.Values.OfType<UserDefinedUniverse>().FirstOrDefault(x -> x.Members.ContainsKey(symbol));
                if( universe != null ) {
                    return universe.Remove(symbol);
                }
            }
            return false;
        }

        /**
         * AddData<typeparam name="T"/> a new user defined data source, requiring only the minimum config options.
         * The data is added with a default time zone of NewYork (Eastern Daylight Savings Time)
        */
         * @param symbol Key/Symbol for data
         * @param resolution Resolution of the data
         * Generic type T must implement base data
        public void AddData<T>( String symbol, Resolution resolution = Resolution.Minute)
            where T : BaseData, new() {
            if( _locked) return;

            //Add this new generic data as a tradeable security: 
            // Defaults:extended market hours"      = true because we want events 24 hours, 
            //          fillforward                 = false because only want to trigger when there's new custom data.
            //          leverage                    = 1 because no leverage on nonmarket data?
            AddData<T>(symbol, resolution, fillDataForward: false, leverage: 1m);
        }

        /**
         * AddData<typeparam name="T"/> a new user defined data source, requiring only the minimum config options.
         * The data is added with a default time zone of NewYork (Eastern Daylight Savings Time)
        */
         * @param symbol Key/Symbol for data
         * @param resolution Resolution of the Data Required
         * @param fillDataForward When no data available on a tradebar, return the last data that was generated
         * @param leverage Custom leverage per security
         * Generic type T must implement base data
        public void AddData<T>( String symbol, Resolution resolution, boolean fillDataForward, BigDecimal leverage = 1.0m)
            where T : BaseData, new() {
            if( _locked) return;

            AddData<T>(symbol, resolution, TimeZones.NewYork, fillDataForward, leverage);
        }

        /**
         * AddData<typeparam name="T"/> a new user defined data source, requiring only the minimum config options.
        */
         * @param symbol Key/Symbol for data
         * @param resolution Resolution of the Data Required
         * @param timeZone Specifies the time zone of the raw data
         * @param fillDataForward When no data available on a tradebar, return the last data that was generated
         * @param leverage Custom leverage per security
         * Generic type T must implement base data
        public void AddData<T>( String symbol, Resolution resolution, ZoneId timeZone, boolean fillDataForward = false, BigDecimal leverage = 1.0m)
            where T : BaseData, new() {
            if( _locked) return;

            marketHoursDbEntry = _marketHoursDatabase.GetEntry(Market.USA, symbol, SecurityType.Base, timeZone);

            //Add this to the data-feed subscriptions
            symbolObject = new Symbol(SecurityIdentifier.GenerateBase(symbol, Market.USA), symbol);
            symbolProperties = _symbolPropertiesDatabase.GetSymbolProperties(Market.USA, symbol, SecurityType.Base, CashBook.AccountCurrency);

            //Add this new generic data as a tradeable security: 
            security = SecurityManager.CreateSecurity(typeof(T), Portfolio, SubscriptionManager, marketHoursDbEntry.ExchangeHours, marketHoursDbEntry.DataTimeZone, 
                symbolProperties, SecurityInitializer, symbolObject, resolution, fillDataForward, leverage, true, false, true);

            AddToUserDefinedUniverse(security);
        }

        /**
         * Send a debug message to the web console:
        */
         * @param message Message to send to debug console
         * <seealso cref="Log"/>
         * <seealso cref="Error( String)"/>
        public void Debug( String message) {
            if( !_liveMode && (message == "" || _previousDebugMessage == message)) return;
            _debugMessages.Add(message);
            _previousDebugMessage = message;
        }

        /**
         * Added another method for logging if user guessed.
        */
         * @param message String message to log.
         * <seealso cref="Debug"/>
         * <seealso cref="Error( String)"/>
        public void Log( String message) {
            if( !_liveMode && message == "") return;
            _logMessages.Add(message);
        }

        /**
         * Send a String error message to the Console.
        */
         * @param message Message to display in errors grid
         * <seealso cref="Debug"/>
         * <seealso cref="Log"/>
        public void Error( String message) {
            if( !_liveMode && (message == "" || _previousErrorMessage == message)) return;
            _errorMessages.Add(message);
            _previousErrorMessage = message;
        }

        /**
         * Send a String error message to the Console.
        */
         * @param error Exception object captured from a try catch loop
         * <seealso cref="Debug"/>
         * <seealso cref="Log"/>
        public void Error(Exception error) {
            message = error.Message;
            if( !_liveMode && (message == "" || _previousErrorMessage == message)) return;
            _errorMessages.Add(message);
            _previousErrorMessage = message;
        }

        /**
         * Terminate the algorithm after processing the current event handler.
        */
         * @param message Exit message to display on quitting
        public void Quit( String message = "") {
            Debug( "Quit(): " + message);
            Status = AlgorithmStatus.Stopped;
        }

        /**
         * Set the Quit flag property of the algorithm.
        */
         * Intended for internal use by the QuantConnect Lean Engine only.
         * @param quit Boolean quit state
         * <seealso cref="Quit"/>
        public void SetQuit( boolean quit) {
            if( quit) {
                Status = AlgorithmStatus.Stopped;
            }
        }

        /**
         * Converts the String 'ticker' symbol into a full <see cref="Symbol"/> object
         * This requires that the String 'ticker' has been added to the algorithm
        */
         * @param ticker The ticker symbol. This should be the ticker symbol
         * as it was added to the algorithm
        @returns The symbol object mapped to the specified ticker
        public Symbol Symbol( String ticker) {
            return SymbolCache.GetSymbol(ticker);
        }

        /**
         * Creates and adds a new <see cref="Security"/> to the algorithm
        */
        private T AddSecurity<T>(SecurityType securityType, String ticker, Resolution resolution, String market, boolean fillDataForward, BigDecimal leverage, boolean extendedMarketHours)
            where T : Security
        {
            if( market == null ) {
                if( !BrokerageModel.DefaultMarkets.TryGetValue(securityType, out market)) {
                    throw new Exception( "No default market set for security type: " + securityType);
                }
            }

            Symbol symbol;
            if( !SymbolCache.TryGetSymbol(ticker, out symbol)) {
                symbol = QuantConnect.Symbol.Create(ticker, securityType, market);
            }

            security = SecurityManager.CreateSecurity(Portfolio, SubscriptionManager, _marketHoursDatabase, _symbolPropertiesDatabase, SecurityInitializer,
                symbol, resolution, fillDataForward, leverage, extendedMarketHours, false, false);
            AddToUserDefinedUniverse(security);
            return (T)security;
        }
    }
}
