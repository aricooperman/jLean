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
using System.Linq;
using System.Threading;
using Fasterflect;
using QuantConnect.Algorithm;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.RealTime;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine
{
    /**
     * Algorithm manager class executes the algorithm and generates and passes through the algorithm events.
    */
    public class AlgorithmManager
    {
        private DateTime _previousTime;
        private IAlgorithm _algorithm;
        private final object _synchronized= new object();
        private String _algorithmId = "";
        private DateTime _currentTimeStepTime;
        private final Duration _timeLoopMaximum = Duration.ofMinutes(Config.GetDouble( "algorithm-manager-time-loop-maximum", 10));
        private long _dataPointCount;

        /**
         * Publicly accessible algorithm status
        */
        public AlgorithmStatus State
        {
            get { return _algorithm == null ? AlgorithmStatus.Running : _algorithm.Status; }
        }

        /**
         * Public access to the currently running algorithm id.
        */
        public String AlgorithmId
        {
            get { return _algorithmId; }
        }

        /**
         * Gets the amount of time spent on the current time step
        */
        public Duration CurrentTimeStepElapsed
        {
            get { return _currentTimeStepTime == DateTime.MinValue ? Duration.ZERO : DateTime.UtcNow - _currentTimeStepTime; }
        }

        /**
         * Gets a function used with the Isolator for verifying we're not spending too much time in each
         * algo manager timer loop
        */
        public final Func<String> TimeLoopWithinLimits;

        private final boolean _liveMode;

        /**
         * Quit state flag for the running algorithm. When true the user has requested the backtest stops through a Quit() method.
        */
         * <seealso cref="QCAlgorithm.Quit"/>
        public boolean QuitState
        {
            get { return State == AlgorithmStatus.Deleted; }
        }

        /**
         * Gets the number of data points processed per second
        */
        public long DataPoints
        {
            get { return _dataPointCount; }
        }

        /**
         * Initializes a new instance of the <see cref="AlgorithmManager"/> class
        */
         * @param liveMode True if we're running in live mode, false for backtest mode
        public AlgorithmManager( boolean liveMode) {
            TimeLoopWithinLimits = () =>
            {
                if( CurrentTimeStepElapsed > _timeLoopMaximum) {
                    return "Algorithm took longer than 10 minutes on a single time loop.";
                }
                return null;
            };
            _liveMode = liveMode;
        }

        /**
         * Launch the algorithm manager to run this strategy
        */
         * @param job Algorithm job
         * @param algorithm Algorithm instance
         * @param feed Datafeed object
         * @param transactions Transaction manager object
         * @param results Result handler object
         * @param realtime Realtime processing object
         * @param commands The command queue for relaying extenal commands to the algorithm
         * @param token Cancellation token
         * Modify with caution
        public void Run(AlgorithmNodePacket job, IAlgorithm algorithm, IDataFeed feed, ITransactionHandler transactions, IResultHandler results, IRealTimeHandler realtime, ICommandQueueHandler commands, CancellationToken token) {
            //Initialize:
            _dataPointCount = 0;
            _algorithm = algorithm;
            portfolioValue = algorithm.Portfolio.TotalPortfolioValue;
            backtestMode = (job.Type == PacketType.BacktestNode);
            methodInvokers = new Map<Type, MethodInvoker>();
            marginCallFrequency = Duration.ofMinutes(5);
            nextMarginCallTime = DateTime.MinValue;
            settlementScanFrequency = Duration.ofMinutes(30);
            nextSettlementScanTime = DateTime.MinValue;

            delistingTickets = new List<OrderTicket>();

            //Initialize Properties:
            _algorithmId = job.AlgorithmId;
            _algorithm.Status = AlgorithmStatus.Running;
            _previousTime = algorithm.StartDate.Date;

            //Create the method accessors to push generic types into algorithm: Find all OnData events:

            // Algorithm 2.0 data accessors
            hasOnDataTradeBars = AddMethodInvoker<TradeBars>(algorithm, methodInvokers);
            hasOnDataQuoteBars = AddMethodInvoker<QuoteBars>(algorithm, methodInvokers);
            hasOnDataOptionChains = AddMethodInvoker<OptionChains>(algorithm, methodInvokers);
            hasOnDataTicks = AddMethodInvoker<Ticks>(algorithm, methodInvokers);

            // dividend and split events
            hasOnDataDividends = AddMethodInvoker<Dividends>(algorithm, methodInvokers);
            hasOnDataSplits = AddMethodInvoker<Splits>(algorithm, methodInvokers);
            hasOnDataDelistings = AddMethodInvoker<Delistings>(algorithm, methodInvokers);
            hasOnDataSymbolChangedEvents = AddMethodInvoker<SymbolChangedEvents>(algorithm, methodInvokers);

            // Algorithm 3.0 data accessors
            hasOnDataSlice = algorithm.GetType().GetMethods()
                .Where(x -> x.Name.equals( "OnData" && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof (Slice))
                .FirstOrDefault(x -> x.DeclaringType == algorithm.GetType()) != null;

            //Go through the subscription types and create invokers to trigger the event handlers for each custom type:
            foreach (config in algorithm.SubscriptionManager.Subscriptions) {
                //If type is a custom feed, check for a dedicated event handler
                if( config.IsCustomData) {
                    //Get the matching method for this event handler - e.g. public void OnData(Quandl data) { .. }
                    genericMethod = (algorithm.GetType()).GetMethod( "OnData", new[] { config.Type });

                    //If we already have this Type-handler then don't add it to invokers again.
                    if( methodInvokers.ContainsKey(config.Type)) continue;

                    //If we couldnt find the event handler, let the user know we can't fire that event.
                    if( genericMethod == null && !hasOnDataSlice) {
                        algorithm.RunTimeError = new Exception( "Data event handler not found, please create a function matching this template: public void OnData( " + config.Type.Name + " data) {  }");
                        _algorithm.Status = AlgorithmStatus.RuntimeError;
                        return;
                    }
                    if( genericMethod != null ) {
                        methodInvokers.Add(config.Type, genericMethod.DelegateForCallMethod());
                    }
                }
            }

            //Loop over the queues: get a data collection, then pass them all into relevent methods in the algorithm.
            Log.Trace( "AlgorithmManager.Run(): Begin DataStream - Start: " + algorithm.StartDate + " Stop: " + algorithm.EndDate);
            foreach (timeSlice in Stream(job, algorithm, feed, results, token)) {
                // reset our timer on each loop
                _currentTimeStepTime = DateTime.UtcNow;

                //Check this backtest is still running:
                if( _algorithm.Status != AlgorithmStatus.Running) {
                    Log.Error( String.format( "AlgorithmManager.Run(): Algorithm state changed to %1$s at %2$s", _algorithm.Status, timeSlice.Time));
                    break;
                }

                //Execute with TimeLimit Monitor:
                if( token.IsCancellationRequested) {
                    Log.Error( "AlgorithmManager.Run(): CancellationRequestion at " + timeSlice.Time);
                    return;
                }

                // before doing anything, check our command queue
                foreach (command in commands.GetCommands()) {
                    if( command == null ) continue;
                    Log.Trace( "AlgorithmManager.Run(): Executing %1$s", command);
                    CommandResultPacket result;
                    try
                    {
                        result = command.Run(algorithm);
                    }
                    catch (Exception err) {
                        Log.Error(err);
                        algorithm.Error( String.format( "%1$s Error: %2$s", command.GetType().Name, err.Message));
                        result = new CommandResultPacket(command, false);
                    }

                    // send the result of the command off to the result handler
                    results.Messages.Enqueue(result);
                }

                time = timeSlice.Time;
                _dataPointCount += timeSlice.DataPointCount;

                //If we're in backtest mode we need to capture the daily performance. We do this here directly
                //before updating the algorithm state with the new data from this time step, otherwise we'll
                //produce incorrect samples (they'll take into account this time step's new price values)
                if( backtestMode) {
                    //On day-change sample equity and daily performance for statistics calculations
                    if( _previousTime.Date != time.Date) {
                        SampleBenchmark(algorithm, results, _previousTime.Date);

                        //Sample the portfolio value over time for chart.
                        results.SampleEquity(_previousTime, Math.Round(algorithm.Portfolio.TotalPortfolioValue, 4));

                        //Check for divide by zero
                        if( portfolioValue == BigDecimal.ZERO) {
                            results.SamplePerformance(_previousTime.Date, 0);
                        }
                        else
                        {
                            results.SamplePerformance(_previousTime.Date, Math.Round((algorithm.Portfolio.TotalPortfolioValue - portfolioValue) * 100 / portfolioValue, 10));
                        }
                        portfolioValue = algorithm.Portfolio.TotalPortfolioValue;
                    }
                }
                else
                {
                    // live mode continously sample the benchmark
                    SampleBenchmark(algorithm, results, time);
                }

                //Update algorithm state after capturing performance from previous day

                //Set the algorithm and real time handler's time
                algorithm.SetDateTime(time);

                if( timeSlice.Slice.SymbolChangedEvents.Count != 0) {
                    if( hasOnDataSymbolChangedEvents) {
                        methodInvokers[typeof (SymbolChangedEvents)](algorithm, timeSlice.Slice.SymbolChangedEvents);
                    }
                    foreach (symbol in timeSlice.Slice.SymbolChangedEvents.Keys) {
                        // cancel all orders for the old symbol
                        foreach (ticket in transactions.GetOrderTickets(x -> x.Status.IsOpen() && x.Symbol == symbol)) {
                            ticket.Cancel( "Open order cancelled on symbol changed event");
                        }
                    }
                }

                if( timeSlice.SecurityChanges != SecurityChanges.None) {
                    foreach (security in timeSlice.SecurityChanges.AddedSecurities) {
                        if( !algorithm.Securities.ContainsKey(security.Symbol)) {
                            // add the new security
                            algorithm.Securities.Add(security);
                        }
                    }
                }

                //On each time step push the real time prices to the cashbook so we can have updated conversion rates
                foreach (update in timeSlice.CashBookUpdateData) {
                    cash = update.Target;
                    foreach (data in update.Data) {
                        cash.Update(data);
                    }
                }

                //Update the securities properties: first before calling user code to avoid issues with data
                foreach (update in timeSlice.SecuritiesUpdateData) {
                    security = update.Target;
                    foreach (data in update.Data) {
                        security.SetMarketPrice(data);
                    }

                    // Send market price updates to the TradeBuilder
                    algorithm.TradeBuilder.SetMarketPrice(security.Symbol, security.Price);
                }

                // fire real time events after we've updated based on the new data
                realtime.SetTime(timeSlice.Time);

                // process fill models on the updated data before entering algorithm, applies to all non-market orders
                transactions.ProcessSynchronousEvents();

                if( delistingTickets.Count != 0) {
                    for (int i = 0; i < delistingTickets.Count; i++) {
                        ticket = delistingTickets[i];
                        if( ticket.Status == OrderStatus.Filled) {
                            algorithm.Securities.Remove(ticket.Symbol);
                            delistingTickets.RemoveAt(i--);
                            Log.Trace( "AlgorithmManager.Run(): Delisted Security removed: " + ticket.Symbol.toString());
                        }
                    }
                }

                //Check if the user's signalled Quit: loop over data until day changes.
                if( algorithm.Status == AlgorithmStatus.Stopped) {
                    Log.Trace( "AlgorithmManager.Run(): Algorithm quit requested.");
                    break;
                }
                if( algorithm.RunTimeError != null ) {
                    _algorithm.Status = AlgorithmStatus.RuntimeError;
                    Log.Trace( String.format( "AlgorithmManager.Run(): Algorithm encountered a runtime error at %1$s. Error: %2$s", timeSlice.Time, algorithm.RunTimeError));
                    break;
                }

                // perform margin calls, in live mode we can also use realtime to emit these
                if( time >= nextMarginCallTime || (_liveMode && nextMarginCallTime > DateTime.UtcNow)) {
                    // determine if there are possible margin call orders to be executed
                    boolean issueMarginCallWarning;
                    marginCallOrders = algorithm.Portfolio.ScanForMarginCall(out issueMarginCallWarning);
                    if( marginCallOrders.Count != 0) {
                        executingMarginCall = false;
                        try
                        {
                            // tell the algorithm we're about to issue the margin call
                            algorithm.OnMarginCall(marginCallOrders);

                            executingMarginCall = true;

                            // execute the margin call orders
                            executedTickets = algorithm.Portfolio.MarginCallModel.ExecuteMarginCall(marginCallOrders);
                            foreach (ticket in executedTickets) {
                                algorithm.Error( String.format( "%1$s - Executed MarginCallOrder: %2$s - Quantity: %3$s @ %4$s", algorithm.Time, ticket.Symbol, ticket.Quantity, ticket.AverageFillPrice));
                            }
                        }
                        catch (Exception err) {
                            algorithm.RunTimeError = err;
                            _algorithm.Status = AlgorithmStatus.RuntimeError;
                            locator = executingMarginCall ? "Portfolio.MarginCallModel.ExecuteMarginCall" : "OnMarginCall";
                            Log.Error( String.format( "AlgorithmManager.Run(): RuntimeError: %1$s: ", locator) + err);
                            return;
                        }
                    }
                    // we didn't perform a margin call, but got the warning flag back, so issue the warning to the algorithm
                    else if( issueMarginCallWarning) {
                        try
                        {
                            algorithm.OnMarginCallWarning();
                        }
                        catch (Exception err) {
                            algorithm.RunTimeError = err;
                            _algorithm.Status = AlgorithmStatus.RuntimeError;
                            Log.Error( "AlgorithmManager.Run(): RuntimeError: OnMarginCallWarning: " + err);
                            return;
                        }
                    }

                    nextMarginCallTime = time + marginCallFrequency;
                }

                // perform check for settlement of unsettled funds
                if( time >= nextSettlementScanTime || (_liveMode && nextSettlementScanTime > DateTime.UtcNow)) {
                    algorithm.Portfolio.ScanForCashSettlement(algorithm.UtcTime);

                    nextSettlementScanTime = time + settlementScanFrequency;
                }

                // before we call any events, let the algorithm know about universe changes
                if( timeSlice.SecurityChanges != SecurityChanges.None) {
                    try
                    {
                        algorithm.OnSecuritiesChanged(timeSlice.SecurityChanges);
                    }
                    catch (Exception err) {
                        algorithm.RunTimeError = err;
                        _algorithm.Status = AlgorithmStatus.RuntimeError;
                        Log.Error( "AlgorithmManager.Run(): RuntimeError: OnSecuritiesChanged event: " + err);
                        return;
                    }
                }

                // apply dividends
                foreach (dividend in timeSlice.Slice.Dividends.Values) {
                    Log.Trace( "AlgorithmManager.Run(): %1$s: Applying Dividend for %2$s", algorithm.Time, dividend.Symbol.toString());
                    algorithm.Portfolio.ApplyDividend(dividend);
                }

                // apply splits
                foreach (split in timeSlice.Slice.splits.Values) {
                    try
                    {
                        Log.Trace( "AlgorithmManager.Run(): %1$s: Applying Split for %2$s", algorithm.Time, split.Symbol.toString());
                        algorithm.Portfolio.ApplySplit(split);
                        // apply the split to open orders as well in raw mode, all other modes are split adjusted
                        if( _liveMode || algorithm.Securities[split.Symbol].DataNormalizationMode == DataNormalizationMode.Raw) {
                            // in live mode we always want to have our order match the order at the brokerage, so apply the split to the orders
                            openOrders = transactions.GetOrderTickets(ticket -> ticket.Status.IsOpen() && ticket.Symbol == split.Symbol);
                            algorithm.BrokerageModel.ApplySplit(openOrders.ToList(), split);
                        }
                    }
                    catch (Exception err) {
                        algorithm.RunTimeError = err;
                        _algorithm.Status = AlgorithmStatus.RuntimeError;
                        Log.Error( "AlgorithmManager.Run(): RuntimeError: Split event: " + err);
                        return;
                    }
                }

                //Update registered consolidators for this symbol index
                try
                {
                    foreach (update in timeSlice.ConsolidatorUpdateData) {
                        consolidators = update.Target.Consolidators;
                        foreach (dataPoint in update.Data) {
                            foreach (consolidator in consolidators) {
                                consolidator.Update(dataPoint);
                            }
                        }
                    }
                }
                catch (Exception err) {
                    algorithm.RunTimeError = err;
                    _algorithm.Status = AlgorithmStatus.RuntimeError;
                    Log.Error( "AlgorithmManager.Run(): RuntimeError: Consolidators update: " + err);
                    return;
                }

                // fire custom event handlers
                foreach (update in timeSlice.CustomData) {
                    MethodInvoker methodInvoker;
                    if( !methodInvokers.TryGetValue(update.DataType, out methodInvoker)) {
                        continue;
                    }

                    try
                    {
                        foreach (dataPoint in update.Data) {
                            if( update.DataType.IsInstanceOfType(dataPoint)) {
                                methodInvoker(algorithm, dataPoint);
                            }
                        }
                    }
                    catch (Exception err) {
                        algorithm.RunTimeError = err;
                        _algorithm.Status = AlgorithmStatus.RuntimeError;
                        Log.Error( "AlgorithmManager.Run(): RuntimeError: Custom Data: " + err);
                        return;
                    }
                }

                try
                {
                    // fire off the dividend and split events before pricing events
                    if( hasOnDataDividends && timeSlice.Slice.Dividends.Count != 0) {
                        methodInvokers[typeof(Dividends)](algorithm, timeSlice.Slice.Dividends);
                    }
                    if( hasOnDataSplits && timeSlice.Slice.splits.Count != 0) {
                        methodInvokers[typeof(Splits)](algorithm, timeSlice.Slice.splits);
                    }
                    if( hasOnDataDelistings && timeSlice.Slice.Delistings.Count != 0) {
                        methodInvokers[typeof(Delistings)](algorithm, timeSlice.Slice.Delistings);
                    }
                }
                catch (Exception err) {
                    algorithm.RunTimeError = err;
                    _algorithm.Status = AlgorithmStatus.RuntimeError;
                    Log.Error( "AlgorithmManager.Run(): RuntimeError: Dividends/Splits/Delistings: " + err);
                    return;
                }

                // run the delisting logic after firing delisting events
                HandleDelistedSymbols(algorithm, timeSlice.Slice.Delistings, delistingTickets);

                //After we've fired all other events in this second, fire the pricing events:
                try
                {
                    if( hasOnDataTradeBars && timeSlice.Slice.Bars.Count > 0) methodInvokers[typeof(TradeBars)](algorithm, timeSlice.Slice.Bars);
                    if( hasOnDataQuoteBars && timeSlice.Slice.QuoteBars.Count > 0) methodInvokers[typeof(QuoteBars)](algorithm, timeSlice.Slice.QuoteBars);
                    if( hasOnDataOptionChains && timeSlice.Slice.OptionChains.Count > 0) methodInvokers[typeof(OptionChains)](algorithm, timeSlice.Slice.OptionChains);
                    if( hasOnDataTicks && timeSlice.Slice.Ticks.Count > 0) methodInvokers[typeof(Ticks)](algorithm, timeSlice.Slice.Ticks);
                }
                catch (Exception err) {
                    algorithm.RunTimeError = err;
                    _algorithm.Status = AlgorithmStatus.RuntimeError;
                    Log.Error( "AlgorithmManager.Run(): RuntimeError: New Style Mode: " + err);
                    return;
                }

                try
                {
                    if( timeSlice.Slice.HasData) {
                        // EVENT HANDLER v3.0 -- all data in a single event
                        algorithm.OnData(timeSlice.Slice);
                    }
                }
                catch (Exception err) {
                    algorithm.RunTimeError = err;
                    _algorithm.Status = AlgorithmStatus.RuntimeError;
                    Log.Error( "AlgorithmManager.Run(): RuntimeError: Slice: " + err);
                    return;
                }

                //If its the historical/paper trading models, wait until market orders have been "filled"
                // Manually trigger the event handler to prevent thread switch.
                transactions.ProcessSynchronousEvents();

                //Save the previous time for the sample calculations
                _previousTime = time;

                // Process any required events of the results handler such as sampling assets, equity, or stock prices.
                results.ProcessSynchronousEvents();
            } // End of ForEach feed.Bridge.GetConsumingEnumerable

            // stop timing the loops
            _currentTimeStepTime = DateTime.MinValue;

            //Stream over:: Send the final packet and fire final events:
            Log.Trace( "AlgorithmManager.Run(): Firing On End Of Algorithm...");
            try
            {
                algorithm.OnEndOfAlgorithm();
            }
            catch (Exception err) {
                _algorithm.Status = AlgorithmStatus.RuntimeError;
                algorithm.RunTimeError = new Exception( "Error running OnEndOfAlgorithm(): " + err.Message, err.InnerException);
                Log.Error( "AlgorithmManager.OnEndOfAlgorithm(): " + err);
                return;
            }

            // Process any required events of the results handler such as sampling assets, equity, or stock prices.
            results.ProcessSynchronousEvents(forceProcess: true);

            //Liquidate Holdings for Calculations:
            if( _algorithm.Status == AlgorithmStatus.Liquidated && _liveMode) {
                Log.Trace( "AlgorithmManager.Run(): Liquidating algorithm holdings...");
                algorithm.Liquidate();
                results.LogMessage( "Algorithm Liquidated");
                results.SendStatusUpdate(AlgorithmStatus.Liquidated);
            }

            //Manually stopped the algorithm
            if( _algorithm.Status == AlgorithmStatus.Stopped) {
                Log.Trace( "AlgorithmManager.Run(): Stopping algorithm...");
                results.LogMessage( "Algorithm Stopped");
                results.SendStatusUpdate(AlgorithmStatus.Stopped);
            }

            //Backtest deleted.
            if( _algorithm.Status == AlgorithmStatus.Deleted) {
                Log.Trace( "AlgorithmManager.Run(): Deleting algorithm...");
                results.DebugMessage( "Algorithm Id:( " + job.AlgorithmId + ") Deleted by request.");
                results.SendStatusUpdate(AlgorithmStatus.Deleted);
            }

            //Algorithm finished, send regardless of commands:
            results.SendStatusUpdate(AlgorithmStatus.Completed);

            //Take final samples:
            results.SampleRange(algorithm.GetChartUpdates());
            results.SampleEquity(_previousTime, Math.Round(algorithm.Portfolio.TotalPortfolioValue, 4));
            SampleBenchmark(algorithm, results, _previousTime);
            results.SamplePerformance(_previousTime, Math.Round((algorithm.Portfolio.TotalPortfolioValue - portfolioValue)*100/portfolioValue, 10));
        } // End of Run();

        /**
         * Set the quit state.
        */
        public void SetStatus(AlgorithmStatus state) {
            synchronized(_lock) {
                //We don't want anyone elseto set our internal state to "Running". 
                //This is controlled by the algorithm private variable only.
                if( state != AlgorithmStatus.Running) {
                    _algorithm.Status = state;
                }
            }
        }

        private IEnumerable<TimeSlice> Stream(AlgorithmNodePacket job, IAlgorithm algorithm, IDataFeed feed, IResultHandler results, CancellationToken cancellationToken) {
            boolean setStartTime = false;
            timeZone = algorithm.TimeZone;
            history = algorithm.HistoryProvider;

            // get the required history job from the algorithm
            DateTime? lastHistoryTimeUtc = null;
            historyRequests = algorithm.GetWarmupHistoryRequests().ToList();

            // initialize variables for progress computation
            start = DateTime.UtcNow.Ticks;
            nextStatusTime = DateTime.UtcNow.AddSeconds(1);
            minimumIncrement = algorithm.UniverseManager
                .Select(x -> x.Value.Configuration.Resolution.ToTimeSpan())
                .DefaultIfEmpty(Time.OneSecond)
                .Min();

            minimumIncrement = minimumIncrement == Duration.ZERO ? Time.OneSecond : minimumIncrement;

            if( historyRequests.Count != 0) {
                // rewrite internal feed requests
                subscriptions = algorithm.SubscriptionManager.Subscriptions.Where(x -> !x.IsInternalFeed).ToList();
                minResolution = subscriptions.Count > 0 ? subscriptions.Min(x -> x.Resolution) : Resolution.Second;
                foreach (request in historyRequests) {
                    Security security;
                    if( algorithm.Securities.TryGetValue(request.Symbol, out security) && security.IsInternalFeed()) {
                        if( request.Resolution < minResolution) {
                            request.Resolution = minResolution;
                            request.FillForwardResolution = request.FillForwardResolution.HasValue ? minResolution : (Resolution?) null;
                        }
                    }
                }

                // rewrite all to share the same fill forward resolution
                if( historyRequests.Any(x -> x.FillForwardResolution.HasValue)) {
                    minResolution = historyRequests.Where(x -> x.FillForwardResolution.HasValue).Min(x -> x.FillForwardResolution.Value);
                    foreach (request in historyRequests.Where(x -> x.FillForwardResolution.HasValue)) {
                        request.FillForwardResolution = minResolution;
                    }
                }

                foreach (request in historyRequests) {
                    start = Math.Min(request.StartTimeUtc.Ticks, start);
                    Log.Trace( String.format( "AlgorithmManager.Stream(): WarmupHistoryRequest: %1$s: Start: %2$s End: %3$s Resolution: %4$s", request.Symbol, request.StartTimeUtc, request.EndTimeUtc, request.Resolution));
                }

                // make the history request and build time slices
                foreach (slice in history.GetHistory(historyRequests, timeZone)) {
                    TimeSlice timeSlice;
                    try
                    {
                        // we need to recombine this slice into a time slice
                        paired = new List<DataFeedPacket>();
                        foreach (symbol in slice.Keys) {
                            security = algorithm.Securities[symbol];
                            data = slice[symbol];
                            list = new List<BaseData>();
                            ticks = data as List<Tick>;
                            if( ticks != null ) list.AddRange(ticks);
                            else               list.Add(data);
                            paired.Add(new DataFeedPacket(security, security.Subscriptions.First(), list));
                        }
                        timeSlice = TimeSlice.Create(slice.Time Extensions.convertToUtc(timeZone), timeZone, algorithm.Portfolio.CashBook, paired, SecurityChanges.None);
                    }
                    catch (Exception err) {
                        Log.Error(err);
                        algorithm.RunTimeError = err;
                        yield break;
                    }

                    if( timeSlice != null ) {
                        if( !setStartTime) {
                            setStartTime = true;
                            _previousTime = timeSlice.Time;
                            algorithm.Debug( "Algorithm warming up...");
                        }
                        if( DateTime.UtcNow > nextStatusTime) {
                            // send some status to the user letting them know we're done history, but still warming up,
                            // catching up to real time data
                            nextStatusTime = DateTime.UtcNow.AddSeconds(1);
                            percent = (int)(100 * (timeSlice.Time.Ticks - start) / (double)(DateTime.UtcNow.Ticks - start));
                            results.SendStatusUpdate(AlgorithmStatus.History, String.format( "Catching up to realtime %1$s%...", percent));
                        }
                        yield return timeSlice;
                        lastHistoryTimeUtc = timeSlice.Time;
                    } 
                }
            }

            // if we're not live or didn't event request warmup, then set us as not warming up
            if( !algorithm.LiveMode || historyRequests.Count == 0) {
                algorithm.SetFinishedWarmingUp();
                results.SendStatusUpdate(AlgorithmStatus.Running);
                if( historyRequests.Count != 0) {
                    algorithm.Debug( "Algorithm finished warming up.");
                    Log.Trace( "AlgorithmManager.Stream(): Finished warmup");
                }
            }

            foreach (timeSlice in feed) {
                if( !setStartTime) {
                    setStartTime = true;
                    _previousTime = timeSlice.Time;
                }
                if( algorithm.LiveMode && algorithm.IsWarmingUp) {
                    // this is hand-over logic, we spin up the data feed first and then request
                    // the history for warmup, so there will be some overlap between the data
                    if( lastHistoryTimeUtc.HasValue) {
                        // make sure there's no historical data, this only matters for the handover
                        hasHistoricalData = false;
                        foreach (data in timeSlice.Slice.Ticks.Values.SelectMany(x -> x).Concat<BaseData>(timeSlice.Slice.Bars.Values)) {
                            // check if any ticks in the list are on or after our last warmup point, if so, skip this data
                            if( data.EndTime Extensions.convertToUtc(algorithm.Securities[data.Symbol].Exchange.TimeZone) >= lastHistoryTimeUtc) {
                                hasHistoricalData = true;
                                break;
                            }
                        }
                        if( hasHistoricalData) {
                            continue;
                        }
                        
                        // prevent us from doing these checks every loop
                        lastHistoryTimeUtc = null;
                    }

                    // in live mode wait to mark us as finished warming up when
                    // the data feed has caught up to now within the min increment
                    if( timeSlice.Time > DateTime.UtcNow.Subtract(minimumIncrement)) {
                        algorithm.SetFinishedWarmingUp();
                        results.SendStatusUpdate(AlgorithmStatus.Running);
                        algorithm.Debug( "Algorithm finished warming up.");
                        Log.Trace( "AlgorithmManager.Stream(): Finished warmup");
                    }
                    else if( DateTime.UtcNow > nextStatusTime) {
                        // send some status to the user letting them know we're done history, but still warming up,
                        // catching up to real time data
                        nextStatusTime = DateTime.UtcNow.AddSeconds(1);
                        percent = (int) (100*(timeSlice.Time.Ticks - start)/(double) (DateTime.UtcNow.Ticks - start));
                        results.SendStatusUpdate(AlgorithmStatus.History, String.format( "Catching up to realtime %1$s%...", percent));   
                    }
                }
                yield return timeSlice;
            }
        }

        /**
         * Adds a method invoker if the method exists to the method invokers dictionary
        */
         * <typeparam name="T The data type to check for 'OnData(T data)</typeparam>
         * @param algorithm The algorithm instance
         * @param methodInvokers The dictionary of method invokers
         * @param methodName The name of the method to search for
        @returns True if the method existed and was added to the collection
        private boolean AddMethodInvoker<T>(IAlgorithm algorithm, Map<Type, MethodInvoker> methodInvokers, String methodName = "OnData") {
            newSplitMethodInfo = algorithm.GetType().GetMethod(methodName, new[] {typeof (T)});
            if( newSplitMethodInfo != null ) {
                methodInvokers.Add(typeof(T), newSplitMethodInfo.DelegateForCallMethod());
                return true;
            }
            return false;
        }

        /**
         * Performs delisting logic for the securities specified in <paramref name="newDelistings"/> that are marked as <see cref="DelistingType.Delisted"/>. 
         * This includes liquidating the position and removing the security from the algorithm's collection.
         * If we're unable to liquidate the position (maybe daily data or EOD already) then we'll add it to the <paramref name="delistingTickets"/>
         * for the algo manager time loop to check later
        */
        private static void HandleDelistedSymbols(IAlgorithm algorithm, Delistings newDelistings, ICollection<OrderTicket> delistingTickets) {
            foreach (delisting in newDelistings.Values) {
                // submit an order to liquidate on market close
                if( delisting.Type == DelistingType.Warning) {
                    Log.Trace( "AlgorithmManager.Run(): Security delisting warning: " + delisting.Symbol.toString());
                    security = algorithm.Securities[delisting.Symbol];
                    if( security.Holdings.Quantity == 0) continue;
                    submitOrderRequest = new SubmitOrderRequest(OrderType.MarketOnClose, security.Type, security.Symbol,
                        -security.Holdings.Quantity, 0, 0, algorithm.UtcTime, "Liquidate from delisting");
                    ticket = algorithm.Transactions.ProcessRequest(submitOrderRequest);
                    delisting.SetOrderTicket(ticket);
                    delistingTickets.Add(ticket);
                }
                else
                {
                    Log.Trace( "AlgorithmManager.Run(): Security delisted: " + delisting.Symbol.toString());
                    algorithm.Securities.Remove(delisting.Symbol);
                    Log.Trace( "AlgorithmManager.Run(): Security removed: " + delisting.Symbol.toString());
                }
            }
        }

        /**
         * Samples the benchmark in a  try/catch block
        */
        private void SampleBenchmark(IAlgorithm algorithm, IResultHandler results, DateTime time) {
            try
            {
                // backtest mode, sample benchmark on day changes
                results.SampleBenchmark(time, algorithm.Benchmark.Evaluate(time).SmartRounding());
            }
            catch (Exception err) {
                algorithm.RunTimeError = err;
                _algorithm.Status = AlgorithmStatus.RuntimeError;
                Log.Error(err);
            }
        }
    }
}
