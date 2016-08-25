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
using System.Threading;
using QuantConnect.Brokerages.Oanda.DataType;
using QuantConnect.Brokerages.Oanda.Framework;
using QuantConnect.Brokerages.Oanda.Session;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Securities;
using Order = QuantConnect.Orders.Order;

package com.quantconnect.lean.Brokerages.Oanda
{
    /**
     * Oanda Brokerage - implementation of IBrokerage interface
    */
    public partial class OandaBrokerage : Brokerage, IDataQueueHandler
    {
        private final IOrderProvider _orderProvider;
        private final ISecurityProvider _securityProvider;
        private final Environment _environment;
        private final String _accessToken;
        private final int _accountId;

        private EventsSession _eventsSession;
        private Map<String, Instrument> _oandaInstruments; 
        private final OandaSymbolMapper _symbolMapper = new OandaSymbolMapper();

        private boolean _isConnected;

        private DateTime _lastHeartbeatUtcTime;
        private Thread _connectionMonitorThread;
        private final object _lockerConnectionMonitor = new object();
        private volatile boolean _connectionLost;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /**
         * Initializes a new instance of the <see cref="OandaBrokerage"/> class.
        */
         * @param orderProvider The order provider.
         * @param securityProvider The holdings provider.
         * @param environment The Oanda environment (Trade or Practice)
         * @param accessToken The Oanda access token (can be the user's personal access token or the access token obtained with OAuth by QC on behalf of the user)
         * @param accountId The account identifier.
        public OandaBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider, Environment environment, String accessToken, int accountId)
            : base( "Oanda Brokerage") {
            _orderProvider = orderProvider;
            _securityProvider = securityProvider;

            if( environment != Environment.Trade && environment != Environment.Practice)
                throw new NotSupportedException( "Oanda Environment not supported: " + environment);

            _environment = environment;
            _accessToken = accessToken;
            _accountId = accountId;
        }

        #region IBrokerage implementation

        /**
         * Returns true if we're currently connected to the broker
        */
        public @Override boolean IsConnected
        {
            get { return _isConnected && !_connectionLost; }
        }

        /**
         * Connects the client to the broker's remote servers
        */
        public @Override void Connect() {
            if( IsConnected) return;

            // Load the list of instruments
            _oandaInstruments = GetInstruments().ToDictionary(x -> x.instrument);

            // Register to the event session to receive events.
            _eventsSession = new EventsSession(this, _accountId);
            _eventsSession.DataReceived += OnEventReceived;
            _eventsSession.StartSession();

            _isConnected = true;

            // create new thread to manage disconnections and reconnections
            _cancellationTokenSource = new CancellationTokenSource();
            _connectionMonitorThread = new Thread(() =>
            {
                nextReconnectionAttemptUtcTime = DateTime.UtcNow;
                double nextReconnectionAttemptSeconds = 1;

                synchronized(_lockerConnectionMonitor) {
                    _lastHeartbeatUtcTime = DateTime.UtcNow;
                }

                try
                {
                    while (!_cancellationTokenSource.IsCancellationRequested) {
                        Duration elapsed;
                        synchronized(_lockerConnectionMonitor) {
                            elapsed = DateTime.UtcNow - _lastHeartbeatUtcTime;
                        }

                        if( !_connectionLost && elapsed > Duration.ofSeconds(20)) {
                            _connectionLost = true;
                            nextReconnectionAttemptUtcTime = DateTime.UtcNow.AddSeconds(nextReconnectionAttemptSeconds);

                            OnMessage(BrokerageMessageEvent.Disconnected( "Connection with Oanda server lost. " +
                                                                         "This could be because of internet connectivity issues. "));
                        }
                        else if( _connectionLost) {
                            try
                            {
                                if( elapsed <= Duration.ofSeconds(20)) {
                                    _connectionLost = false;
                                    nextReconnectionAttemptSeconds = 1;

                                    OnMessage(BrokerageMessageEvent.Reconnected( "Connection with Oanda server restored."));
                                }
                                else
                                {
                                    if( DateTime.UtcNow > nextReconnectionAttemptUtcTime) {
                                        try
                                        {
                                            // check if we have a connection
                                            GetInstruments();

                                            // restore events session
                                            if( _eventsSession != null ) {
                                                _eventsSession.DataReceived -= OnEventReceived;
                                                _eventsSession.StopSession();
                                            }
                                            _eventsSession = new EventsSession(this, _accountId);
                                            _eventsSession.DataReceived += OnEventReceived;
                                            _eventsSession.StartSession();

                                            // restore rates session
                                            List<Symbol> symbolsToSubscribe;
                                            synchronized(_lockerSubscriptions) {
                                                symbolsToSubscribe = _subscribedSymbols.ToList();
                                            }
                                            SubscribeSymbols(symbolsToSubscribe);
                                        }
                                        catch (Exception) {
                                            // double the interval between attempts (capped to 1 minute)
                                            nextReconnectionAttemptSeconds = Math.Min(nextReconnectionAttemptSeconds * 2, 60);
                                            nextReconnectionAttemptUtcTime = DateTime.UtcNow.AddSeconds(nextReconnectionAttemptSeconds);
                                        }
                                    }
                                }
                            }
                            catch (Exception exception) {
                                Log.Error(exception);
                            }
                        }

                        Thread.Sleep(1000);
                    }
                }
                catch (Exception exception) {
                    Log.Error(exception);
                }
            });
            _connectionMonitorThread.Start();
            while (!_connectionMonitorThread.IsAlive) {
                Thread.Sleep(1);
            }
        }

        /**
         * Disconnects the client from the broker's remote servers
        */
        public @Override void Disconnect() {
            if( _eventsSession != null ) {
                _eventsSession.DataReceived -= OnEventReceived;
                _eventsSession.StopSession();
            }

            if( _ratesSession != null ) {
                _ratesSession.DataReceived -= OnDataReceived;
                _ratesSession.StopSession();
            }

            // request and wait for thread to stop
            _cancellationTokenSource.Cancel();
            _connectionMonitorThread.Join();

            _isConnected = false;
        }

        /**
         * Gets all open orders on the account. 
         * NOTE: The order objects returned do not have QC order IDs.
        */
        @returns The open orders returned from Oanda
        public @Override List<Order> GetOpenOrders() {
            oandaOrders = GetOrderList();

            orderList = oandaOrders.Select(ConvertOrder).ToList();
            return orderList;
        }

        /**
         * Gets all holdings for the account
        */
        @returns The current holdings from the account
        public @Override List<Holding> GetAccountHoldings() {
            holdings = GetPositions(_accountId).Select(ConvertHolding).Where(x -> x.Quantity != 0).ToList();

            // Set MarketPrice in each Holding
            oandaSymbols = holdings
                .Select(x -> _symbolMapper.GetBrokerageSymbol(x.Symbol))
                .ToList();

            if( oandaSymbols.Count > 0) {
                quotes = GetRates(oandaSymbols).ToDictionary(x -> x.instrument);
                foreach (holding in holdings) {
                    oandaSymbol = _symbolMapper.GetBrokerageSymbol(holding.Symbol);
                    Price quote;
                    if( quotes.TryGetValue(oandaSymbol, out quote)) {
                        holding.MarketPrice = new BigDecimal( (quote.bid + quote.ask) / 2);
                    }
                }
            }

            return holdings;
        }

        /**
         * Gets the current cash balance for each currency held in the brokerage account
        */
        @returns The current cash balance for each currency available for trading
        public @Override List<Cash> GetCashBalance() {
            getAccountRequestString = EndpointResolver.ResolveEndpoint(_environment, Server.Account) + "accounts/" + _accountId;
            accountResponse = MakeRequest<Account>(getAccountRequestString);

            return new List<Cash>
            {
                new Cash(accountResponse.accountCurrency, accountResponse.balance new BigDecimal(  ),
                    GetUsdConversion(accountResponse.accountCurrency))
            };
        }

        /**
         * Places a new order and assigns a new broker ID to the order
        */
         * @param order The order to be placed
        @returns True if the request for a new order has been placed, false otherwise
        public @Override boolean PlaceOrder(Order order) {
            requestParams = new Map<String,String>
            {
                { "instrument", _symbolMapper.GetBrokerageSymbol(order.Symbol) },
                { "units",  Integer.parseInt( order.AbsoluteQuantity).toString() }
            };

            PopulateOrderRequestParameters(order, requestParams);

            postOrderResponse = PostOrderAsync(requestParams);
            if( postOrderResponse == null ) 
                return false;

            // if market order, find fill quantity and price
            marketOrderFillPrice = BigDecimal.ZERO;
            if( order.Type == OrderType.Market) {
                marketOrderFillPrice = new BigDecimal( postOrderResponse.price);
            }

            marketOrderFillQuantity = 0;
            if( postOrderResponse.tradeOpened != null && postOrderResponse.tradeOpened.id > 0) {
                if( order.Type == OrderType.Market) {
                    marketOrderFillQuantity = postOrderResponse.tradeOpened.units;
                }
                else
                {
                    order.BrokerId.Add(postOrderResponse.tradeOpened.id.toString());
                }
            }

            if( postOrderResponse.tradeReduced != null && postOrderResponse.tradeReduced.id > 0) {
                if( order.Type == OrderType.Market) {
                    marketOrderFillQuantity = postOrderResponse.tradeReduced.units;
                }
                else
                {
                    order.BrokerId.Add(postOrderResponse.tradeReduced.id.toString());
                }
            }

            if( postOrderResponse.orderOpened != null && postOrderResponse.orderOpened.id > 0) {
                if( order.Type != OrderType.Market) {
                    order.BrokerId.Add(postOrderResponse.orderOpened.id.toString());
                }
            }

            if( postOrderResponse.tradesClosed != null && postOrderResponse.tradesClosed.Count > 0) {
                marketOrderFillQuantity += postOrderResponse.tradesClosed
                    .Where(trade -> order.Type == OrderType.Market)
                    .Sum(trade -> trade.units);
            }

            // send Submitted order event
            static final int orderFee = 0;
            order.PriceCurrency = _securityProvider.GetSecurity(order.Symbol).SymbolProperties.QuoteCurrency;
            OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, orderFee) { Status = OrderStatus.Submitted });

            if( order.Type == OrderType.Market) {
                // if market order, also send Filled order event
                OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, orderFee) {
                    Status = OrderStatus.Filled,
                    FillPrice = marketOrderFillPrice,
                    FillQuantity = marketOrderFillQuantity * Math.Sign(order.Quantity)
                });
            }

            return true;
        }


        /**
         * Updates the order with the same id
        */
         * @param order The new order information
        @returns True if the request was made for the order to be updated, false otherwise
        public @Override boolean UpdateOrder(Order order) {
            Log.Trace( "OandaBrokerage.UpdateOrder(): " + order);
            
            if( !order.BrokerId.Any()) {
                // we need the brokerage order id in order to perform an update
                Log.Trace( "OandaBrokerage.UpdateOrder(): Unable to update order without BrokerId.");
                return false;
            }
            
            requestParams = new Map<String,String>
            {
                { "instrument", _symbolMapper.GetBrokerageSymbol(order.Symbol) },
                { "units",  Integer.parseInt( order.AbsoluteQuantity).toString() },
            };

            // we need the brokerage order id in order to perform an update
            PopulateOrderRequestParameters(order, requestParams);

            UpdateOrder(long.Parse(order.BrokerId.First()), requestParams);

            return true;
        }

        /**
         * Cancels the order with the specified ID
        */
         * @param order The order to cancel
        @returns True if the request was made for the order to be canceled, false otherwise
        public @Override boolean CancelOrder(Order order) {
            Log.Trace( "OandaBrokerage.CancelOrder(): " + order);
            
            if( !order.BrokerId.Any()) {
                Log.Trace( "OandaBrokerage.CancelOrder(): Unable to cancel order without BrokerId.");
                return false;
            }

            foreach (orderId in order.BrokerId) {
                CancelOrder(long.Parse(orderId));
                OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, 0, "Oanda Cancel Order Event") { Status = OrderStatus.Canceled });
            }

            return true;
        }

        #endregion

    }
}
