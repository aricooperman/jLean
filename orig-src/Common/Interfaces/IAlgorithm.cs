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
using NodaTime;
using QuantConnect.Benchmarks;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Notifications;
using QuantConnect.Orders;
using QuantConnect.Scheduling;
using QuantConnect.Securities;
using QuantConnect.Statistics;

package com.quantconnect.lean.Interfaces
{
    /**
     * Interface for QuantConnect algorithm implementations. All algorithms must implement these
     * basic members to allow interaction with the Lean Backtesting Engine.
    */
    public interface IAlgorithm
    {
        /**
         * Data subscription manager controls the information and subscriptions the algorithms recieves.
         * Subscription configurations can be added through the Subscription Manager.
        */
        SubscriptionManager SubscriptionManager
        {
            get;
        }

        /**
         * Security object collection class stores an array of objects representing representing each security/asset
         * we have a subscription for.
        */
         * It is an IDictionary implementation and can be indexed by symbol
        SecurityManager Securities
        {
            get;
        }

        /**
         * Gets the collection of universes for the algorithm
        */
        UniverseManager UniverseManager
        {
            get;
        }

        /**
         * Security portfolio management class provides wrapper and helper methods for the Security.Holdings class such as
         * IsLong, IsShort, TotalProfit
        */
         * Portfolio is a wrapper and helper class encapsulating the Securities[].Holdings objects
        SecurityPortfolioManager Portfolio
        {
            get;
        }

        /**
         * Security transaction manager class controls the store and processing of orders.
        */
         * The orders and their associated events are accessible here. When a new OrderEvent is recieved the algorithm portfolio is updated.
        SecurityTransactionManager Transactions
        {
            get;
        }

        /**
         * Gets the brokerage model used to emulate a real brokerage
        */
        IBrokerageModel BrokerageModel
        {
            get;
        }

        /**
         * Gets the brokerage message handler used to decide what to do
         * with each message sent from the brokerage
        */
        IBrokerageMessageHandler BrokerageMessageHandler
        {
            get;
            set;
        }

        /**
         * Notification manager for storing and processing live event messages
        */
        NotificationManager Notify
        {
            get;
        }

        /**
         * Gets schedule manager for adding/removing scheduled events
        */
        ScheduleManager Schedule
        {
            get;
        }

        /**
         * Gets or sets the history provider for the algorithm
        */
        IHistoryProvider HistoryProvider
        {
            get; 
            set;
        }

        /**
         * Gets or sets the current status of the algorithm
        */
        AlgorithmStatus Status
        {
            get; 
            set;
        }

        /**
         * Gets whether or not this algorithm is still warming up
        */
        boolean IsWarmingUp
        {
            get;
        }

        /**
         * Public name for the algorithm.
        */
         * Not currently used but preserved for API integrity
        String Name
        {
            get;
        }

        /**
         * Current date/time in the algorithm's local time zone
        */
        DateTime Time
        {
            get;
        }

        /**
         * Gets the time zone of the algorithm
        */
        ZoneId TimeZone
        {
            get;
        }

        /**
         * Current date/time in UTC.
        */
        DateTime UtcTime
        {
            get;
        }

        /**
         * Algorithm start date for backtesting, set by the SetStartDate methods.
        */
        DateTime StartDate
        {
            get;
        }

        /**
         * Get Requested Backtest End Date
        */
        DateTime EndDate
        {
            get;
        }

        /**
         * AlgorithmId for the backtest
        */
        String AlgorithmId
        {
            get;
        }

        /**
         * Algorithm is running on a live server.
        */
        boolean LiveMode
        {
            get;
        }

        /**
         * Gets the subscription settings to be used when adding securities via universe selection
        */
        UniverseSettings UniverseSettings
        {
            get;
        }

        /**
         * Debug messages from the strategy:
        */
        List<String> DebugMessages
        {
            get;
        }

        /**
         * Error messages from the strategy:
        */
        List<String> ErrorMessages
        {
            get;
        }

        /**
         * Log messages from the strategy:
        */
        List<String> LogMessages
        {
            get;
        }

        /**
         * Gets the run time error from the algorithm, or null if none was encountered.
        */
        Exception RunTimeError
        {
            get;
            set;
        }

        /**
         * Customizable dynamic statistics displayed during live trading:
        */
        Map<String,String> RuntimeStatistics
        {
            get;
        }

        /**
         * Gets the function used to define the benchmark. This function will return
         * the value of the benchmark at a requested date/time
        */
        IBenchmark Benchmark
        { 
            get;
        }

        /**
         * Gets an instance that is to be used to initialize newly created securities.
        */
        ISecurityInitializer SecurityInitializer
        {
            get;
        }

        /**
         * Gets the Trade Builder to generate trades from executions
        */
        TradeBuilder TradeBuilder
        {
            get;
        }

        /**
         * Initialise the Algorithm and Prepare Required Data:
        */
        void Initialize();

        /**
         * Called by setup handlers after Initialize and allows the algorithm a chance to organize
         * the data gather in the Initialize method
        */
        void PostInitialize();

        /**
         * Gets the parameter with the specified name. If a parameter
         * with the specified name does not exist, null is returned
        */
         * @param name The name of the parameter to get
        @returns The value of the specified parameter, or null if not found
        String GetParameter( String name);

        /**
         * Sets the parameters from the dictionary
        */
         * @param parameters Dictionary containing the parameter names to values
        void SetParameters(Map<String,String> parameters);

        /**
         * Sets the brokerage model used to resolve transaction models, settlement models,
         * and brokerage specified ordering behaviors.
        */
         * @param brokerageModel The brokerage model used to emulate the real
         * brokerage
        void SetBrokerageModel(IBrokerageModel brokerageModel);

        // <summary>
        // v1.0 Handler for Tick Events [DEPRECATED June-2014]
        // </summary>
        // <param name="ticks Tick Data Packet
        //void OnTick(Map<String, List<Tick>> ticks);

        // <summary>
        // v1.0 Handler for TradeBar Events [DEPRECATED June-2014]
        // </summary>
        // <param name="tradebars TradeBar Data Packet
        //void OnTradeBar(Map<String, TradeBar> tradebars);

        // <summary>
        // v2.0 Handler for Generic Data Events
        // </summary>
        //void OnData(Ticks ticks);
        //void OnData(TradeBars tradebars);

        /**
         * v3.0 Handler for all data types
        */
         * @param slice The current slice of data
        void OnData(Slice slice);

        /**
         * Event fired each time the we add/remove securities from the data feed
        */
         * @param changes">
        void OnSecuritiesChanged(SecurityChanges changes);

        /**
         * Send debug message
        */
         * @param message">
        void Debug( String message);

        /**
         * Save entry to the Log
        */
         * @param message String message
        void Log( String message);

        /**
         * Send an error message for the algorithm
        */
         * @param message String message
        void Error( String message);

        /**
         * Margin call event handler. This method is called right before the margin call orders are placed in the market.
        */
         * @param requests The orders to be executed to bring this algorithm within margin limits
        void OnMarginCall(List<SubmitOrderRequest> requests);

        /**
         * Margin call warning event handler. This method is called when Portoflio.MarginRemaining is under 5% of your Portfolio.TotalPortfolioValue
        */
        void OnMarginCallWarning();

        /**
         * Call this method at the end of each day of data.
        */
        void OnEndOfDay();

        /**
         * Call this method at the end of each day of data.
        */
        void OnEndOfDay(Symbol symbol);

        /**
         * Call this event at the end of the algorithm running.
        */
        void OnEndOfAlgorithm();

        /**
         * EXPERTS ONLY:: [-!-Async Code-!-]
         * New order event handler: on order status changes (filled, partially filled, cancelled etc).
        */
         * @param newEvent Event information
        void OnOrderEvent(OrderEvent newEvent);

        /**
         * Brokerage message event handler. This method is called for all types of brokerage messages.
        */
        void OnBrokerageMessage(BrokerageMessageEvent messageEvent);

        /**
         * Brokerage disconnected event handler. This method is called when the brokerage connection is lost.
        */
        void OnBrokerageDisconnect();

        /**
         * Brokerage reconnected event handler. This method is called when the brokerage connection is restored after a disconnection.
        */
        void OnBrokerageReconnect();

        /**
         * Set the DateTime Frontier: This is the master time and is
        */
         * @param time">
        void SetDateTime(DateTime time);

        /**
         * Set the algorithm Id for this backtest or live run. This can be used to identify the order and equity records.
        */
         * @param algorithmId unique 32 character identifier for backtest or live server
        void SetAlgorithmId( String algorithmId);

        /**
         * Set the algorithm as initialized and locked. No more cash or security changes.
        */
        void SetLocked();

        /**
         * Gets whether or not this algorithm has been locked and fully initialized
        */
        boolean GetLocked();

        /**
         * Get the chart updates since the last request:
        */
         * @param clearChartData">
        @returns List of Chart Updates
        List<Chart> GetChartUpdates( boolean clearChartData = false);

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
        Security AddSecurity(SecurityType securityType, String symbol, Resolution resolution, String market, boolean fillDataForward, BigDecimal leverage, boolean extendedMarketHours);

        /**
         * Set the starting capital for the strategy
        */
         * @param startingCash decimal starting capital, default $100,000
        void SetCash( BigDecimal startingCash);

        /**
         * Set the cash for the specified symbol
        */
         * @param symbol The cash symbol to set
         * @param startingCash Decimal cash value of portfolio
         * @param conversionRate The current conversion rate for the
        void SetCash( String symbol, BigDecimal startingCash, BigDecimal conversionRate);

        /**
         * Liquidate your portfolio holdings:
        */
         * @param symbolToLiquidate Specific asset to liquidate, defaults to all.
        @returns list of order ids
        List<Integer> Liquidate(Symbol symbolToLiquidate = null );

        /**
         * Set live mode state of the algorithm run: Public setter for the algorithm property LiveMode.
        */
         * @param live Bool live mode flag
        void SetLiveMode( boolean live);

        /**
         * Sets <see cref="IsWarmingUp"/> to false to indicate this algorithm has finished its warm up
        */
        void SetFinishedWarmingUp();

        /**
         * Gets the date/time warmup should begin
        */
        @returns 
        IEnumerable<HistoryRequest> GetWarmupHistoryRequests();

        /**
         * Set the maximum number of orders the algortihm is allowed to process.
        */
         * @param max Maximum order count int
        void SetMaximumOrders(int max);
    }
}
