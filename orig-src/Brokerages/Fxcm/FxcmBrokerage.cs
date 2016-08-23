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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using com.fxcm.external.api.transport;
using com.fxcm.external.api.transport.listeners;
using com.fxcm.external.api.util;
using com.fxcm.fix;
using com.fxcm.fix.pretrade;
using com.fxcm.fix.trade;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Securities;

package com.quantconnect.lean.Brokerages.Fxcm
{
    /**
    /// FXCM brokerage - implementation of IBrokerage interface
    */
    public partial class FxcmBrokerage : Brokerage, IDataQueueHandler, IGenericMessageListener, IStatusMessageListener
    {
        private final IOrderProvider _orderProvider;
        private final ISecurityProvider _securityProvider;
        private final String _server;
        private final String _terminal;
        private final String _userName;
        private final String _password;
        private final String _accountId;

        private Thread _orderEventThread;
        private Thread _connectionMonitorThread;

        private final object _lockerConnectionMonitor = new object();
        private DateTime _lastReadyMessageTime;
        private volatile boolean _connectionLost;
        private volatile boolean _connectionError;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private final ConcurrentQueue<OrderEvent> _orderEventQueue = new ConcurrentQueue<OrderEvent>();
        private final FxcmSymbolMapper _symbolMapper = new FxcmSymbolMapper();

        /**
        /// Creates a new instance of the <see cref="FxcmBrokerage"/> class
        */
         * @param orderProvider">The order provider
         * @param securityProvider">The holdings provider
         * @param server">The url of the server
         * @param terminal">The terminal name
         * @param userName">The user name (login id)
         * @param password">The user password
         * @param accountId">The account id
        public FxcmBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider, String server, String terminal, String userName, String password, String accountId)
            : base( "FXCM Brokerage") {
            _orderProvider = orderProvider;
            _securityProvider = securityProvider;
            _server = server;
            _terminal = terminal;
            _userName = userName;
            _password = password;
            _accountId = accountId;
        }

        #region IBrokerage implementation

        /**
        /// Returns true if we're currently connected to the broker
        */
        public @Override boolean IsConnected
        {
            get
            {
                return _gateway != null && _gateway.isConnected() && !_connectionLost;
            }
        }

        /**
        /// Connects the client to the broker's remote servers
        */
        public @Override void Connect() {
            if( IsConnected) return;

            Log.Trace( "FxcmBrokerage.Connect()");

            _cancellationTokenSource = new CancellationTokenSource();

            // create new thread to fire order events in queue
            _orderEventThread = new Thread(() =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested) {
                    OrderEvent orderEvent;
                    if( !_orderEventQueue.TryDequeue(out orderEvent)) {
                        Thread.Sleep(1);
                        continue;
                    }

                    OnOrderEvent(orderEvent);
                }
            });
            _orderEventThread.Start();
            while (!_orderEventThread.IsAlive) {
                Thread.Sleep(1);
            }

            // create the gateway
            _gateway = GatewayFactory.createGateway();

            // register the message listeners with the gateway
            _gateway.registerGenericMessageListener(this);
            _gateway.registerStatusMessageListener(this);

            // create local login properties
            loginProperties = new FXCMLoginProperties(_userName, _password, _terminal, _server);

            // log in
            _gateway.login(loginProperties);

            // create new thread to manage disconnections and reconnections
            _connectionMonitorThread = new Thread(() =>
            {
                _lastReadyMessageTime = DateTime.UtcNow;

                try
                {
                    while (!_cancellationTokenSource.IsCancellationRequested) {
                        Duration elapsed;
                        lock (_lockerConnectionMonitor) {
                            elapsed = DateTime.UtcNow - _lastReadyMessageTime;
                        }

                        if( !_connectionLost && elapsed > Duration.ofSeconds(5)) {
                            _connectionLost = true;

                            OnMessage(BrokerageMessageEvent.Disconnected( "Connection with FXCM server lost. " +
                                                                         "This could be because of internet connectivity issues. "));
                        }
                        else if( _connectionLost && elapsed <= Duration.ofSeconds(5)) {
                            try
                            {
                                _gateway.relogin();

                                _connectionLost = false;

                                OnMessage(BrokerageMessageEvent.Reconnected( "Connection with FXCM server restored."));
                            }
                            catch (Exception exception) {
                                Log.Error(exception);
                            }
                        }
                        else if( _connectionError) {
                            try
                            {
                                // log out
                                _gateway.logout();

                                // remove the message listeners
                                _gateway.removeGenericMessageListener(this);
                                _gateway.removeStatusMessageListener(this);

                                // register the message listeners with the gateway
                                _gateway.registerGenericMessageListener(this);
                                _gateway.registerStatusMessageListener(this);

                                // log in
                                _gateway.login(loginProperties);

                                // load instruments, accounts, orders, positions
                                LoadInstruments();
                                LoadAccounts();
                                LoadOpenOrders();
                                LoadOpenPositions();

                                _connectionError = false;
                                _connectionLost = false;

                                OnMessage(BrokerageMessageEvent.Reconnected( "Connection with FXCM server restored."));
                            }
                            catch (Exception exception) {
                                Log.Error(exception);
                            }
                        }

                        Thread.Sleep(5000);
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

            // load instruments, accounts, orders, positions
            LoadInstruments();
            LoadAccounts();
            LoadOpenOrders();
            LoadOpenPositions();
        }

        /**
        /// Disconnects the client from the broker's remote servers
        */
        public @Override void Disconnect() {
            if( !IsConnected) return;

            Log.Trace( "FxcmBrokerage.Disconnect()");

            // log out
            _gateway.logout();

            // remove the message listeners
            _gateway.removeGenericMessageListener(this);
            _gateway.removeStatusMessageListener(this);

            // request and wait for thread to stop
            _cancellationTokenSource.Cancel();
            _orderEventThread.Join();
            _connectionMonitorThread.Join();
        }

        /**
        /// Gets all open orders on the account. 
        /// NOTE: The order objects returned do not have QC order IDs.
        */
        @returns The open orders returned from FXCM
        public @Override List<Order> GetOpenOrders() {
            Log.Trace( String.format( "FxcmBrokerage.GetOpenOrders(): Located %1$s orders", _openOrders.Count));
            orders = _openOrders.Values.ToList()
                .Where(x -> OrderIsOpen(x.getFXCMOrdStatus().getCode()))
                .Select(ConvertOrder)
                .ToList();
            return orders;
        }

        /**
        /// Gets all holdings for the account
        */
        @returns The current holdings from the account
        public @Override List<Holding> GetAccountHoldings() {
            Log.Trace( "FxcmBrokerage.GetAccountHoldings()");

            holdings = _openPositions.Values.Select(ConvertHolding).Where(x -> x.Quantity != 0).ToList();

            // Set MarketPrice in each Holding
            fxcmSymbols = holdings
                .Select(x -> _symbolMapper.GetBrokerageSymbol(x.Symbol))
                .ToList();

            if( fxcmSymbols.Count > 0) {
                quotes = GetQuotes(fxcmSymbols).ToDictionary(x -> x.getInstrument().getSymbol());
                foreach (holding in holdings) {
                    MarketDataSnapshot quote;
                    if( quotes.TryGetValue(_symbolMapper.GetBrokerageSymbol(holding.Symbol), out quote)) {
                        holding.MarketPrice = new BigDecimal( (quote.getBidClose() + quote.getAskClose()) / 2);
                    }
                }
            }

            return holdings;
        }

        /**
        /// Gets the current cash balance for each currency held in the brokerage account
        */
        @returns The current cash balance for each currency available for trading
        public @Override List<Cash> GetCashBalance() {
            Log.Trace( "FxcmBrokerage.GetCashBalance()");
            cashBook = new List<Cash>();

            //Adds the account currency USD to the cashbook.
            cashBook.Add(new Cash(_fxcmAccountCurrency,
                        new BigDecimal( _accounts[_accountId].getCashOutstanding()),
                        GetUsdConversion(_fxcmAccountCurrency)));

            foreach (trade in _openPositions.Values) {
                //settlement price for the trade
                settlementPrice = new BigDecimal( trade.getSettlPrice());
                //direction of trade
                direction = trade.getPositionQty().getLongQty() > 0 ? 1 : -1;
                //quantity of the asset
                quantity = new BigDecimal( trade.getPositionQty().getQty());
                //quantity of base currency
                baseQuantity = direction * quantity;
                //quantity of quote currency
                quoteQuantity = -direction * quantity * settlementPrice;
                //base currency
                baseCurrency = trade.getCurrency();
                //quote currency
                quoteCurrency = FxcmSymbolMapper.ConvertFxcmSymbolToLeanSymbol(trade.getInstrument().getSymbol());
                quoteCurrency = quoteCurrency.Substring(quoteCurrency.Length - 3);

                baseCurrencyObject = (from cash in cashBook where cash.Symbol == baseCurrency select cash).FirstOrDefault();
                //update the value of the base currency
                if( baseCurrencyObject != null ) {
                    baseCurrencyObject.AddAmount(baseQuantity);
                }
                else
                {
                    //add the base currency if not present
                    cashBook.Add(new Cash(baseCurrency, baseQuantity, GetUsdConversion(baseCurrency)));
                }

                quoteCurrencyObject = (from cash in cashBook where cash.Symbol == quoteCurrency select cash).FirstOrDefault();
                //update the value of the quote currency
                if( quoteCurrencyObject != null ) {
                    quoteCurrencyObject.AddAmount(quoteQuantity);
                }
                else
                {
                    //add the quote currency if not present
                    cashBook.Add(new Cash(quoteCurrency, quoteQuantity, GetUsdConversion(quoteCurrency)));
                }
            }
            return cashBook;
        }

        /**
        /// Places a new order and assigns a new broker ID to the order
        */
         * @param order">The order to be placed
        @returns True if the request for a new order has been placed, false otherwise
        public @Override boolean PlaceOrder(Order order) {
            Log.Trace( "FxcmBrokerage.PlaceOrder(): %1$s", order);

            if( !IsConnected)
                throw new InvalidOperationException( "FxcmBrokerage.PlaceOrder(): Unable to place order while not connected.");

            if( order.Direction != OrderDirection.Buy && order.Direction != OrderDirection.Sell)
                throw new ArgumentException( "FxcmBrokerage.PlaceOrder(): Invalid Order Direction");

            fxcmSymbol = _symbolMapper.GetBrokerageSymbol(order.Symbol);
            orderSide = order.Direction == OrderDirection.Buy ? SideFactory.BUY : SideFactory.SELL;
            quantity = (double)order.AbsoluteQuantity;

            OrderSingle orderRequest;
            switch (order.Type) {
                case OrderType.Market:
                    orderRequest = MessageGenerator.generateMarketOrder(_accountId, quantity, orderSide, fxcmSymbol, "");
                    break;

                case OrderType.Limit:
                    limitPrice = (double)((LimitOrder)order).LimitPrice;
                    orderRequest = MessageGenerator.generateOpenOrder(limitPrice, _accountId, quantity, orderSide, fxcmSymbol, "");
                    orderRequest.setOrdType(OrdTypeFactory.LIMIT);
                    orderRequest.setTimeInForce(TimeInForceFactory.GOOD_TILL_CANCEL);
                    break;

                case OrderType.StopMarket:
                    stopPrice = (double)((StopMarketOrder)order).StopPrice;
                    orderRequest = MessageGenerator.generateOpenOrder(stopPrice, _accountId, quantity, orderSide, fxcmSymbol, "");
                    orderRequest.setOrdType(OrdTypeFactory.STOP);
                    orderRequest.setTimeInForce(TimeInForceFactory.GOOD_TILL_CANCEL);
                    break;

                default:
                    throw new NotSupportedException( "FxcmBrokerage.PlaceOrder(): Order type " + order.Type + " is not supported.");
            }

            _isOrderSubmitRejected = false;
            AutoResetEvent autoResetEvent;
            lock (_locker) {
                _currentRequest = _gateway.sendMessage(orderRequest);
                _mapRequestsToOrders[_currentRequest] = order;
                autoResetEvent = new AutoResetEvent(false);
                _mapRequestsToAutoResetEvents[_currentRequest] = autoResetEvent;
            }
            if( !autoResetEvent.WaitOne(ResponseTimeout))
                throw new TimeoutException( String.format( "FxcmBrokerage.PlaceOrder(): Operation took longer than %1$s seconds.", (decimal)ResponseTimeout / 1000));

            return !_isOrderSubmitRejected;
        }

        /**
        /// Updates the order with the same id
        */
         * @param order">The new order information
        @returns True if the request was made for the order to be updated, false otherwise
        public @Override boolean UpdateOrder(Order order) {
            Log.Trace( "FxcmBrokerage.UpdateOrder(): %1$s", order);

            if( !IsConnected)
                throw new InvalidOperationException( "FxcmBrokerage.UpdateOrder(): Unable to update order while not connected.");

            if( !order.BrokerId.Any()) {
                // we need the brokerage order id in order to perform an update
                Log.Trace( "FxcmBrokerage.UpdateOrder(): Unable to update order without BrokerId.");
                return false;
            }

            fxcmOrderId = order.BrokerId[0].toString();

            ExecutionReport fxcmOrder;
            if( !_openOrders.TryGetValue(fxcmOrderId, out fxcmOrder))
                throw new ArgumentException( "FxcmBrokerage.UpdateOrder(): FXCM order id not found: " + fxcmOrderId);

            double price;
            switch (order.Type) {
                case OrderType.Limit:
                    price = (double)((LimitOrder)order).LimitPrice;
                    break;

                case OrderType.StopMarket:
                    price = (double)((StopMarketOrder)order).StopPrice;
                    break;

                default:
                    throw new NotSupportedException( "FxcmBrokerage.UpdateOrder(): Invalid order type.");
            }

            _isOrderUpdateOrCancelRejected = false;
            orderReplaceRequest = MessageGenerator.generateOrderReplaceRequest( "", fxcmOrder.getOrderID(), fxcmOrder.getSide(), fxcmOrder.getOrdType(), price, fxcmOrder.getAccount());
            orderReplaceRequest.setInstrument(fxcmOrder.getInstrument());
            orderReplaceRequest.setOrderQty((double)order.AbsoluteQuantity);

            AutoResetEvent autoResetEvent;
            lock (_locker) {
                _currentRequest = _gateway.sendMessage(orderReplaceRequest);
                autoResetEvent = new AutoResetEvent(false);
                _mapRequestsToAutoResetEvents[_currentRequest] = autoResetEvent;
            }
            if( !autoResetEvent.WaitOne(ResponseTimeout))
                throw new TimeoutException( String.format( "FxcmBrokerage.UpdateOrder(): Operation took longer than %1$s seconds.", (decimal)ResponseTimeout / 1000));

            return !_isOrderUpdateOrCancelRejected;
        }

        /**
        /// Cancels the order with the specified ID
        */
         * @param order">The order to cancel
        @returns True if the request was made for the order to be canceled, false otherwise
        public @Override boolean CancelOrder(Order order) {
            Log.Trace( "FxcmBrokerage.CancelOrder(): %1$s", order);

            if( !IsConnected)
                throw new InvalidOperationException( "FxcmBrokerage.UpdateOrder(): Unable to cancel order while not connected.");

            if( !order.BrokerId.Any()) {
                // we need the brokerage order id in order to perform a cancellation
                Log.Trace( "FxcmBrokerage.CancelOrder(): Unable to cancel order without BrokerId.");
                return false;
            }

            fxcmOrderId = order.BrokerId[0].toString();

            ExecutionReport fxcmOrder;
            if( !_openOrders.TryGetValue(fxcmOrderId, out fxcmOrder))
                throw new ArgumentException( "FxcmBrokerage.CancelOrder(): FXCM order id not found: " + fxcmOrderId);

            _isOrderUpdateOrCancelRejected = false;
            orderCancelRequest = MessageGenerator.generateOrderCancelRequest( "", fxcmOrder.getOrderID(), fxcmOrder.getSide(), fxcmOrder.getAccount());
            AutoResetEvent autoResetEvent;
            lock (_locker) {
                _currentRequest = _gateway.sendMessage(orderCancelRequest);
                autoResetEvent = new AutoResetEvent(false);
                _mapRequestsToAutoResetEvents[_currentRequest] = autoResetEvent;
            }
            if( !autoResetEvent.WaitOne(ResponseTimeout))
                throw new TimeoutException( String.format( "FxcmBrokerage.CancelOrder(): Operation took longer than %1$s seconds.", (decimal)ResponseTimeout / 1000));

            return !_isOrderUpdateOrCancelRejected;
        }

        #endregion

    }
}
