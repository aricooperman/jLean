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
using System.Threading.Tasks;
using QuantConnect.Brokerages;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Util;

package com.quantconnect.lean.Lean.Engine.TransactionHandlers
{
    /**
    /// Transaction handler for all brokerages
    */
    public class BrokerageTransactionHandler : ITransactionHandler
    {
        private IAlgorithm _algorithm;
        private IBrokerage _brokerage;
        private boolean _syncedLiveBrokerageCashToday;

        // this boolean is used to check if the warning message for the rounding of order quantity has been displayed for the first time
        private boolean _firstRoundOffMessage = false;

        // this value is used for determining how confident we are in our cash balance update
        private long _lastFillTimeTicks;
        private long _lastSyncTimeTicks;
        private final object _performCashSyncReentranceGuard = new object();
        private static final Duration _liveBrokerageCashSyncTime = new TimeSpan(7, 45, 0); // 7:45 am

        /**
        /// OrderQueue holds the newly updated orders from the user algorithm waiting to be processed. Once
        /// orders are processed they are moved into the Orders queue awaiting the brokerage response.
        */
        private final BusyBlockingCollection<OrderRequest> _orderRequestQueue = new BusyBlockingCollection<OrderRequest>();
        private final CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /**
        /// The orders dictionary holds orders which are sent to exchange, partially filled, completely filled or cancelled.
        /// Once the transaction thread has worked on them they get put here while witing for fill updates.
        */
        private final ConcurrentMap<Integer, Order> _orders = new ConcurrentMap<Integer, Order>();

        /**
        /// The orders tickets dictionary holds order tickets that the algorithm can use to reference a specific order. This
        /// includes invoking update and cancel commands. In the future, we can add more features to the ticket, such as events
        /// and async events (such as run this code when this order fills)
        */
        private final ConcurrentMap<Integer, OrderTicket> _orderTickets = new ConcurrentMap<Integer, OrderTicket>();

        private IResultHandler _resultHandler;

        /**
        /// Gets the permanent storage for all orders
        */
        public ConcurrentMap<Integer, Order> Orders
        {
            get { return _orders; }
        }

        /**
        /// Gets the current number of orders that have been processed
        */
        public int OrdersCount
        {
            get { return _orders.Count; }
        }

        /**
        /// Creates a new BrokerageTransactionHandler to process orders using the specified brokerage implementation
        */
         * @param algorithm">The algorithm instance
         * @param brokerage">The brokerage implementation to process orders and fire fill events
         * @param resultHandler">
        public virtual void Initialize(IAlgorithm algorithm, IBrokerage brokerage, IResultHandler resultHandler) {
            if( brokerage == null ) {
                throw new ArgumentNullException( "brokerage");
            }

            // we don't need to do this today because we just initialized/synced
            _resultHandler = resultHandler;
            _syncedLiveBrokerageCashToday = true;
            _lastSyncTimeTicks = DateTime.Now.Ticks;

            _brokerage = brokerage;
            _brokerage.OrderStatusChanged += (sender, fill) =>
            {
                // log every fill in live mode
                if( algorithm.LiveMode) {
                    brokerIds = string.Empty;
                    order = GetOrderById(fill.OrderId);
                    if( order != null && order.BrokerId.Count > 0) brokerIds = String.join( ", ", order.BrokerId);
                    Log.Trace( "BrokerageTransactionHandler.OrderStatusChanged(): " + fill + " BrokerId: " + brokerIds);
                }

                HandleOrderEvent(fill);
            };

            _brokerage.AccountChanged += (sender, account) =>
            {
                HandleAccountChanged(account);
            };

            IsActive = true;

            _algorithm = algorithm;
        }

        /**
        /// Boolean flag indicating the Run thread method is busy. 
        /// False indicates it is completely finished processing and ready to be terminated.
        */
        public boolean IsActive { get; private set; }

        #region Order Request Processing

        /**
        /// Adds the specified order to be processed
        */
         * @param request">The order to be processed
        public OrderTicket Process(OrderRequest request) {
            if( _algorithm.LiveMode) {
                Log.Trace( "BrokerageTransactionHandler.Process(): " + request);
            }

            switch (request.OrderRequestType) {
                case OrderRequestType.Submit:
                    return AddOrder((SubmitOrderRequest) request);

                case OrderRequestType.Update:
                    return UpdateOrder((UpdateOrderRequest) request);

                case OrderRequestType.Cancel:
                    return CancelOrder((CancelOrderRequest) request);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /**
        /// Add an order to collection and return the unique order id or negative if an error.
        */
         * @param request">A request detailing the order to be submitted
        @returns New unique, increasing orderid
        public OrderTicket AddOrder(SubmitOrderRequest request) {
            response = !_algorithm.IsWarmingUp
                ? OrderResponse.Success(request) 
                : OrderResponse.WarmingUp(request);

            request.SetResponse(response);
            ticket = new OrderTicket(_algorithm.Transactions, request);
            _orderTickets.TryAdd(ticket.OrderId, ticket);

            // send the order to be processed after creating the ticket
            if( response.IsSuccess) {
                _orderRequestQueue.Add(request);
            }
            else
            {
                // add it to the orders collection for recall later
                order = Order.CreateOrder(request);

                // ensure the order is tagged with a currency
                security = _algorithm.Securities[order.Symbol];
                order.PriceCurrency = security.SymbolProperties.QuoteCurrency;

                order.Status = OrderStatus.Invalid;
                order.Tag = "Algorithm warming up.";
                ticket.SetOrder(order);
                _orders.TryAdd(request.OrderId, order);
            }
            return ticket;
        }

        /**
        /// Update an order yet to be filled such as stop or limit orders.
        */
         * @param request">Request detailing how the order should be updated
        /// Does not apply if the order is already fully filled
        public OrderTicket UpdateOrder(UpdateOrderRequest request) {
            OrderTicket ticket;
            if( !_orderTickets.TryGetValue(request.OrderId, out ticket)) {
                return OrderTicket.InvalidUpdateOrderId(_algorithm.Transactions, request);
            }

            ticket.AddUpdateRequest(request);

            try
            {
                //Update the order from the behaviour
                order = GetOrderByIdInternal(request.OrderId);
                if( order == null ) {
                    // can't update an order that doesn't exist!
                    request.SetResponse(OrderResponse.UnableToFindOrder(request));
                }
                else if( order.Status.IsClosed()) {
                    // can't update a completed order
                    request.SetResponse(OrderResponse.InvalidStatus(request, order));
                }
                else if( request.Quantity.HasValue && request.Quantity.Value == 0) {
                    request.SetResponse(OrderResponse.ZeroQuantity(request));
                }
                else if( _algorithm.IsWarmingUp) {
                    request.SetResponse(OrderResponse.WarmingUp(request));
                }
                else
                {
                    request.SetResponse(OrderResponse.Success(request), OrderRequestStatus.Processing);
                    _orderRequestQueue.Add(request);
                }
            }
            catch (Exception err) {
                Log.Error(err);
                request.SetResponse(OrderResponse.Error(request, OrderResponseErrorCode.ProcessingError, err.Message));
            }

            return ticket;
        }

        /**
        /// Remove this order from outstanding queue: user is requesting a cancel.
        */
         * @param request">Request containing the specific order id to remove
        public OrderTicket CancelOrder(CancelOrderRequest request) {
            OrderTicket ticket;
            if( !_orderTickets.TryGetValue(request.OrderId, out ticket)) {
                Log.Error( "BrokerageTransactionHandler.CancelOrder(): Unable to locate ticket for order.");
                return OrderTicket.InvalidCancelOrderId(_algorithm.Transactions, request);
            }

            try
            {
                // if we couldn't set this request as the cancellation then another thread/someone
                // else is already doing it or it in fact has already been cancelled
                if( !ticket.TrySetCancelRequest(request)) {
                    // the ticket has already been cancelled
                    request.SetResponse(OrderResponse.Error(request, OrderResponseErrorCode.InvalidRequest, "Cancellation is already in progress."));
                    return ticket;
                }

                //Error check
                order = GetOrderByIdInternal(request.OrderId);
                if( order != null && request.Tag != null ) {
                    order.Tag = request.Tag;
                }
                if( order == null ) {
                    Log.Error( "BrokerageTransactionHandler.CancelOrder(): Cannot find this id.");
                    request.SetResponse(OrderResponse.UnableToFindOrder(request));
                }
                else if( order.Status.IsClosed()) {
                    Log.Error( "BrokerageTransactionHandler.CancelOrder(): Order already " + order.Status);
                    request.SetResponse(OrderResponse.InvalidStatus(request, order));
                }
                else if( _algorithm.IsWarmingUp) {
                    request.SetResponse(OrderResponse.WarmingUp(request));
                }
                else
                {
                    // send the request to be processed
                    request.SetResponse(OrderResponse.Success(request), OrderRequestStatus.Processing);
                    _orderRequestQueue.Add(request);
                }
            }
            catch (Exception err) {
                Log.Error(err);
                request.SetResponse(OrderResponse.Error(request, OrderResponseErrorCode.ProcessingError, err.Message));
            }

            return ticket;
        }

        /**
        /// Gets and enumerable of <see cref="OrderTicket"/> matching the specified <paramref name="filter"/>
        */
         * @param filter">The filter predicate used to find the required order tickets
        @returns An enumerable of <see cref="OrderTicket"/> matching the specified <paramref name="filter"/>
        public IEnumerable<OrderTicket> GetOrderTickets(Func<OrderTicket, bool> filter = null ) {
            return _orderTickets.Select(x -> x.Value).Where(filter ?? (x -> true));
        }

        /**
        /// Gets the order ticket for the specified order id. Returns null if not found
        */
         * @param orderId">The order's id
        @returns The order ticket with the specified id, or null if not found
        public OrderTicket GetOrderTicket(int orderId) {
            OrderTicket ticket;
            _orderTickets.TryGetValue(orderId, out ticket);
            return ticket;
        }

        #endregion

        /**
        /// Get the order by its id
        */
         * @param orderId">Order id to fetch
        @returns The order with the specified id, or null if no match is found
        public Order GetOrderById(int orderId) {
            Order order = GetOrderByIdInternal(orderId);
            return order != null ? order.Clone() : null;
        }

        private Order GetOrderByIdInternal(int orderId) {
            // this function can be invoked by brokerages when getting open orders, guard against null ref
            if( _orders == null ) return null;
            
            Order order;
            return _orders.TryGetValue(orderId, out order) ? order : null;
        }

        /**
        /// Gets the order by its brokerage id
        */
         * @param brokerageId">The brokerage id to fetch
        @returns The first order matching the brokerage id, or null if no match is found
        public Order GetOrderByBrokerageId( String brokerageId) {
            // this function can be invoked by brokerages when getting open orders, guard against null ref
            if( _orders == null ) return null;
            
            order = _orders.FirstOrDefault(x -> x.Value.BrokerId.Contains(brokerageId)).Value;
            return order != null ? order.Clone() : null;
        }

        /**
        /// Gets all orders matching the specified filter
        */
         * @param filter">Delegate used to filter the orders
        @returns All open orders this order provider currently holds
        public IEnumerable<Order> GetOrders(Func<Order, bool> filter = null ) {
            if( _orders == null ) {
                // this is the case when we haven't initialize yet, backtesting brokerage
                // will end up calling this through the transaction manager
                return Enumerable.Empty<Order>();
            }

            if( filter != null ) {
                // return a clone to prevent object reference shenanigans, you must submit a request to change the order
                return _orders.Select(x -> x.Value).Where(filter).Select(x -> x.Clone());
            }
            return _orders.Select(x -> x.Value).Select(x -> x.Clone());
        }

        /**
        /// Primary thread entry point to launch the transaction thread.
        */
        public void Run() {
            try
            {
                foreach(request in _orderRequestQueue.GetConsumingEnumerable(_cancellationTokenSource.Token)) {
                    HandleOrderRequest(request);
                    ProcessAsynchronousEvents();
                }
            }
            catch (Exception err) {
                // unexpected error, we need to close down shop
                Log.Error(err);
                // quit the algorithm due to error
                _algorithm.RunTimeError = err;
            }

            Log.Trace( "BrokerageTransactionHandler.Run(): Ending Thread...");
            IsActive = false;
        }

        /**
        /// Processes asynchronous events on the transaction handler's thread
        */
        public virtual void ProcessAsynchronousEvents() {
            // NOP
        }

        /**
        /// Processes all synchronous events that must take place before the next time loop for the algorithm
        */
        public virtual void ProcessSynchronousEvents() {
            // how to do synchronous market orders for real brokerages?

            // in backtesting we need to wait for orders to be removed from the queue and finished processing
            if( !_algorithm.LiveMode) {
                if( _orderRequestQueue.IsBusy && !_orderRequestQueue.WaitHandle.WaitOne(Time.OneSecond, _cancellationTokenSource.Token)) {
                    Log.Error( "BrokerageTransactionHandler.ProcessSynchronousEvents(): Timed out waiting for request queue to finish processing.");
                }
                return;
            }

            Log.Debug( "BrokerageTransactionHandler.ProcessSynchronousEvents(): Enter");

            // every morning flip this switch back
            if( _syncedLiveBrokerageCashToday && DateTime.Now.Date != LastSyncDate) {
                _syncedLiveBrokerageCashToday = false;
            }

            // we want to sync up our cash balance before market open
            if( _algorithm.LiveMode && !_syncedLiveBrokerageCashToday && DateTime.Now.TimeOfDay >= _liveBrokerageCashSyncTime) {
                try
                {
                    // only perform cash syncs if we haven't had a fill for at least 10 seconds
                    if( TimeSinceLastFill > Duration.ofSeconds(10)) {
                        PerformCashSync();
                    }
                }
                catch (Exception err) {
                    Log.Error(err, "Updating cash balances");
                }
            }

            // we want to remove orders older than 10k records, but only in live mode
            static final int maxOrdersToKeep = 10000;
            if( _orders.Count < maxOrdersToKeep + 1) {
                Log.Debug( "BrokerageTransactionHandler.ProcessSynchronousEvents(): Exit");
                return;
            }

            int max = _orders.Max(x -> x.Key);
            int lowestOrderIdToKeep = max - maxOrdersToKeep;
            foreach (item in _orders.Where(x -> x.Key <= lowestOrderIdToKeep)) {
                Order value;
                OrderTicket ticket;
                _orders.TryRemove(item.Key, out value);
                _orderTickets.TryRemove(item.Key, out ticket);
            }

            Log.Debug( "BrokerageTransactionHandler.ProcessSynchronousEvents(): Exit");
        }

        /**
        /// Syncs cash from brokerage with portfolio object
        */
        private void PerformCashSync() {
            try
            {
                // prevent reentrance in this method
                if( !Monitor.TryEnter(_performCashSyncReentranceGuard)) {
                    return;
                }

                Log.Trace( "BrokerageTransactionHandler.PerformCashSync(): Sync cash balance");

                balances = new List<Cash>();
                try
                {
                    balances = _brokerage.GetCashBalance();
                }
                catch (Exception err) {
                    Log.Error(err);
                }

                if( balances.Count == 0) {
                    return;
                }

                //Adds currency to the cashbook that the user might have deposited
                foreach (balance in balances) {
                    Cash cash;
                    if( !_algorithm.Portfolio.CashBook.TryGetValue(balance.Symbol, out cash)) {
                        Log.LogHandler.Trace( "BrokerageTransactionHandler.PerformCashSync(): Unexpected cash found %1$s %2$s", balance.Amount, balance.Symbol);
                        _algorithm.Portfolio.SetCash(balance.Symbol, balance.Amount, balance.ConversionRate);
                    }
                }

                // if we were returned our balances, update everything and flip our flag as having performed sync today
                foreach (cash in _algorithm.Portfolio.CashBook.Values) {
                    balanceCash = balances.FirstOrDefault(balance -> balance.Symbol == cash.Symbol);
                    //update the cash if the entry if found in the balances
                    if( balanceCash != null ) {
                        // compare in dollars
                        delta = cash.Amount - balanceCash.Amount;
                        if( Math.Abs(_algorithm.Portfolio.CashBook.ConvertToAccountCurrency(delta, cash.Symbol)) > 5) {
                            // log the delta between 
                            Log.LogHandler.Trace( "BrokerageTransactionHandler.PerformCashSync(): %1$s Delta: %2$s", balanceCash.Symbol,
                                delta.toString( "0.00"));
                        }
                        _algorithm.Portfolio.SetCash(balanceCash.Symbol, balanceCash.Amount, balanceCash.ConversionRate);
                    }
                    else
                    {
                        //Set the cash amount to zero if cash entry not found in the balances
                        _algorithm.Portfolio.SetCash(cash.Symbol, 0, cash.ConversionRate);
                    }
                }
                _syncedLiveBrokerageCashToday = true;
            }
            finally
            {
                Monitor.Exit(_performCashSyncReentranceGuard);
            }

            // fire off this task to check if we've had recent fills, if we have then we'll invalidate the cash sync
            // and do it again until we're confident in it
            Task.Delay(Duration.ofSeconds(10)).ContinueWith(_ =>
            {
                // we want to make sure this is a good value, so check for any recent fills
                if( TimeSinceLastFill <= Duration.ofSeconds(20)) {
                    // this will cause us to come back in and reset cash again until we 
                    // haven't processed a fill for +- 10 seconds of the set cash time
                    _syncedLiveBrokerageCashToday = false;
                    Log.Trace( "BrokerageTransactionHandler.PerformCashSync(): Unverified cash sync - resync required.");
                }
                else
                {
                    _lastSyncTimeTicks = DateTime.Now.Ticks;
                    Log.Trace( "BrokerageTransactionHandler.PerformCashSync(): Verified cash sync.");
                }
            });
        }

        /**
        /// Signal a end of thread request to stop montioring the transactions.
        */
        public void Exit() {
            timeout = Duration.ofSeconds(60);
            if( !_orderRequestQueue.WaitHandle.WaitOne(timeout)) {
                Log.Error( "BrokerageTransactionHandler.Exit(): Exceed timeout: " + (int)(timeout.TotalSeconds) + " seconds.");
            }
            _cancellationTokenSource.Cancel();
        }

        /**
        /// Handles a generic order request
        */
         * @param request"><see cref="OrderRequest"/> to be handled
        @returns <see cref="OrderResponse"/> for request
        public void HandleOrderRequest(OrderRequest request) {
            OrderResponse response;
            switch (request.OrderRequestType) {
                case OrderRequestType.Submit:
                    response = HandleSubmitOrderRequest((SubmitOrderRequest)request);
                    break;
                case OrderRequestType.Update:
                    response = HandleUpdateOrderRequest((UpdateOrderRequest)request);
                    break;
                case OrderRequestType.Cancel:
                    response = HandleCancelOrderRequest((CancelOrderRequest)request);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // mark request as processed
            request.SetResponse(response, OrderRequestStatus.Processed);
        }

        /**
        /// Handles a request to submit a new order
        */
        private OrderResponse HandleSubmitOrderRequest(SubmitOrderRequest request) {
            OrderTicket ticket;
            order = Order.CreateOrder(request);

            // ensure the order is tagged with a currency
            security = _algorithm.Securities[order.Symbol];
            order.PriceCurrency = security.SymbolProperties.QuoteCurrency;

            // rounds off the order towards 0 to the nearest multiple of lot size
            order.Quantity = RoundOffOrder(order, security);

            if( !_orders.TryAdd(order.Id, order)) {
                Log.Error( "BrokerageTransactionHandler.HandleSubmitOrderRequest(): Unable to add new order, order not processed.");
                return OrderResponse.Error(request, OrderResponseErrorCode.OrderAlreadyExists, "Cannot process submit request because order with id %1$s already exists");
            }
            if( !_orderTickets.TryGetValue(order.Id, out ticket)) {
                Log.Error( "BrokerageTransactionHandler.HandleSubmitOrderRequest(): Unable to retrieve order ticket, order not processed.");
                return OrderResponse.UnableToFindOrder(request);
            }

            // update the ticket's internal storage with this new order reference
            ticket.SetOrder(order);

            if( order.Quantity == 0) {
                order.Status = OrderStatus.Invalid;
                response = OrderResponse.ZeroQuantity(request);
                _algorithm.Error(response.ErrorMessage);
                HandleOrderEvent(new OrderEvent(order, _algorithm.UtcTime, 0m, "Unable to add order for zero quantity"));
                return response;
            }

            // check to see if we have enough money to place the order
            boolean sufficientCapitalForOrder;
            try
            {
                sufficientCapitalForOrder = _algorithm.Transactions.GetSufficientCapitalForOrder(_algorithm.Portfolio, order);
            }
            catch (Exception err) {
                Log.Error(err);
                _algorithm.Error( String.format( "Order Error: id: %1$s, Error executing margin models: %2$s", order.Id, err.Message));
                HandleOrderEvent(new OrderEvent(order, _algorithm.UtcTime, 0m, "Error executing margin models"));
                return OrderResponse.Error(request, OrderResponseErrorCode.ProcessingError, "Error in GetSufficientCapitalForOrder");
            }

            if( !sufficientCapitalForOrder) {
                order.Status = OrderStatus.Invalid;
                response = OrderResponse.Error(request, OrderResponseErrorCode.InsufficientBuyingPower, String.format( "Order Error: id: %1$s, Insufficient buying power to complete order (Value:%2$s).", order.Id, order.GetValue(security).SmartRounding()));
                _algorithm.Error(response.ErrorMessage);
                HandleOrderEvent(new OrderEvent(order, _algorithm.UtcTime, 0m, "Insufficient buying power to complete order"));
                return response;
            }

            // verify that our current brokerage can actually take the order
            BrokerageMessageEvent message;
            if( !_algorithm.LiveMode && !_algorithm.BrokerageModel.CanSubmitOrder(security, order, out message)) {
                // if we couldn't actually process the order, mark it as invalid and bail
                order.Status = OrderStatus.Invalid;
                if( message == null ) message = new BrokerageMessageEvent(BrokerageMessageType.Warning, "InvalidOrder", "BrokerageModel declared unable to submit order: " + order.Id);
                response = OrderResponse.Error(request, OrderResponseErrorCode.BrokerageModelRefusedToSubmitOrder, "OrderID: " + order.Id + " " + message);
                _algorithm.Error(response.ErrorMessage);
                HandleOrderEvent(new OrderEvent(order, _algorithm.UtcTime, 0m, "BrokerageModel declared unable to submit order"));
                return response;
            }

            // set the order status based on whether or not we successfully submitted the order to the market
            boolean orderPlaced;
            try
            {
                orderPlaced = _brokerage.PlaceOrder(order);
            }
            catch (Exception err) {
                Log.Error(err);
                orderPlaced = false;
            }

            if( !orderPlaced) {
                // we failed to submit the order, invalidate it
                order.Status = OrderStatus.Invalid;
                errorMessage = "Brokerage failed to place order: " + order.Id;
                response = OrderResponse.Error(request, OrderResponseErrorCode.BrokerageFailedToSubmitOrder, errorMessage);
                _algorithm.Error(response.ErrorMessage);
                HandleOrderEvent(new OrderEvent(order, _algorithm.UtcTime, 0m, "Brokerage failed to place order"));
                return response;
            }
            
            order.Status = OrderStatus.Submitted;
            return OrderResponse.Success(request);
        }

        /**
        /// Handles a request to update order properties
        */
        private OrderResponse HandleUpdateOrderRequest(UpdateOrderRequest request) {
            Order order;
            OrderTicket ticket;
            if( !_orders.TryGetValue(request.OrderId, out order) || !_orderTickets.TryGetValue(request.OrderId, out ticket)) {
                Log.Error( "BrokerageTransactionHandler.HandleUpdateOrderRequest(): Unable to update order with ID " + request.OrderId);
                return OrderResponse.UnableToFindOrder(request);
            }
            
            if( !CanUpdateOrder(order)) {
                return OrderResponse.InvalidStatus(request, order);
            }

            // rounds off the order towards 0 to the nearest multiple of lot size
            security = _algorithm.Securities[order.Symbol];
            order.Quantity = RoundOffOrder(order, security);

            // verify that our current brokerage can actually update the order
            BrokerageMessageEvent message;
            if( !_algorithm.LiveMode && !_algorithm.BrokerageModel.CanUpdateOrder(_algorithm.Securities[order.Symbol], order, request, out message)) {
                // if we couldn't actually process the order, mark it as invalid and bail
                order.Status = OrderStatus.Invalid;
                if( message == null ) message = new BrokerageMessageEvent(BrokerageMessageType.Warning, "InvalidOrder", "BrokerageModel declared unable to update order: " + order.Id);
                response = OrderResponse.Error(request, OrderResponseErrorCode.BrokerageModelRefusedToUpdateOrder, "OrderID: " + order.Id + " " + message);
                _algorithm.Error(response.ErrorMessage);
                HandleOrderEvent(new OrderEvent(order, _algorithm.UtcTime, 0m, "BrokerageModel declared unable to update order"));
                return response;
            }

            // modify the values of the order object
            order.ApplyUpdateOrderRequest(request);
            ticket.SetOrder(order);

            boolean orderUpdated;
            try
            {
                orderUpdated = _brokerage.UpdateOrder(order);
            }
            catch (Exception err) {
                Log.Error(err);
                orderUpdated = false;
            }
            
            if( !orderUpdated) {
                // we failed to update the order for some reason
                errorMessage = "Brokerage failed to update order with id " + request.OrderId;
                _algorithm.Error(errorMessage);
                HandleOrderEvent(new OrderEvent(order, _algorithm.UtcTime, 0m, "Brokerage failed to update order"));
                return OrderResponse.Error(request, OrderResponseErrorCode.BrokerageFailedToUpdateOrder, errorMessage);
            }

            return OrderResponse.Success(request);
        }

        /**
        /// Returns true if the specified order can be updated
        */
         * @param order">The order to check if we can update
        @returns True if the order can be updated, false otherwise
        private boolean CanUpdateOrder(Order order) {
            return order.Status != OrderStatus.Filled
                && order.Status != OrderStatus.Canceled
                && order.Status != OrderStatus.PartiallyFilled
                && order.Status != OrderStatus.Invalid;
        }

        /**
        /// Handles a request to cancel an order
        */
        private OrderResponse HandleCancelOrderRequest(CancelOrderRequest request) {
            Order order;
            OrderTicket ticket;
            if( !_orders.TryGetValue(request.OrderId, out order) || !_orderTickets.TryGetValue(request.OrderId, out ticket)) {
                Log.Error( "BrokerageTransactionHandler.HandleCancelOrderRequest(): Unable to cancel order with ID " + request.OrderId + ".");
                return OrderResponse.UnableToFindOrder(request);
            }

            if( order.Status.IsClosed()) {
                return OrderResponse.InvalidStatus(request, order);
            }

            ticket.SetOrder(order);

            boolean orderCanceled;
            try
            {
                orderCanceled = _brokerage.CancelOrder(order);
            }
            catch (Exception err) {
                Log.Error(err);
                orderCanceled = false;
            }

            if( !orderCanceled) {
                // failed to cancel the order
                message = "Brokerage failed to cancel order with id " + order.Id;
                _algorithm.Error(message);
                HandleOrderEvent(new OrderEvent(order, _algorithm.UtcTime, 0m, "Brokerage failed to cancel order"));
                return OrderResponse.Error(request, OrderResponseErrorCode.BrokerageFailedToCancelOrder, message);
            }

            // we succeeded to cancel the order
            order.Status = OrderStatus.Canceled;

            if( request.Tag != null ) {
                // update the tag, useful for 'why' we canceled the order
                order.Tag = request.Tag;
            }

            return OrderResponse.Success(request);
        }

        private void HandleOrderEvent(OrderEvent fill) {
            // update the order status
            order = GetOrderByIdInternal(fill.OrderId);
            if( order == null ) {
                Log.Error( "BrokerageTransactionHandler.HandleOrderEvent(): Unable to locate Order with id " + fill.OrderId);
                return;
            }

            // set the status of our order object based on the fill event
            order.Status = fill.Status;

            // save that the order event took place, we're initializing the list with a capacity of 2 to reduce number of mallocs
            //these hog memory
            //List<OrderEvent> orderEvents = _orderEvents.GetOrAdd(orderEvent.OrderId, i -> new List<OrderEvent>(2));
            //orderEvents.Add(orderEvent);

            //Apply the filled order to our portfolio:
            if( fill.Status == OrderStatus.Filled || fill.Status == OrderStatus.PartiallyFilled) {
                Interlocked.Exchange(ref _lastFillTimeTicks, DateTime.Now.Ticks);
                
                // check if the fill currency and the order currency match the symbol currency
                security = _algorithm.Securities[fill.Symbol];
                // Bug in FXCM API flipping the currencies -- disabling for now. 5/17/16 RFB
                //if( fill.FillPriceCurrency != security.SymbolProperties.QuoteCurrency)
                //{
                //    Log.Error( String.format( "Currency mismatch: Fill currency: %1$s, Symbol currency: %2$s", fill.FillPriceCurrency, security.SymbolProperties.QuoteCurrency));
                //}
                //if( order.PriceCurrency != security.SymbolProperties.QuoteCurrency)
                //{
                //    Log.Error( String.format( "Currency mismatch: Order currency: %1$s, Symbol currency: %2$s", order.PriceCurrency, security.SymbolProperties.QuoteCurrency));
                //}

                try
                {
                    _algorithm.Portfolio.ProcessFill(fill);

                    conversionRate = security.QuoteCurrency.ConversionRate;

                    _algorithm.TradeBuilder.ProcessFill(fill, conversionRate);
                }
                catch (Exception err) {
                    Log.Error(err);
                    _algorithm.Error( String.format( "Order Error: id: %1$s, Error in Portfolio.ProcessFill: %2$s", order.Id, err.Message));
                }
            }

            // update the ticket and order after we've processed the fill, but before the event, this way everything is ready for user code
            OrderTicket ticket;
            if( _orderTickets.TryGetValue(fill.OrderId, out ticket)) {
                ticket.AddOrderEvent(fill);
                order.Price = ticket.AverageFillPrice;
            }
            else
            {
                Log.Error( "BrokerageTransactionHandler.HandleOrderEvent(): Unable to resolve ticket: " + fill.OrderId);
            }

            //We have an event! :) Order filled, send it in to be handled by algorithm portfolio.
            if( fill.Status != OrderStatus.None) //order.Status != OrderStatus.Submitted
            {
                //Create new order event:
                _resultHandler.OrderEvent(fill);
                try
                {
                    //Trigger our order event handler
                    _algorithm.OnOrderEvent(fill);
                }
                catch (Exception err) {
                    _algorithm.Error( "Order Event Handler Error: " + err.Message);
                    // kill the algorithm
                    _algorithm.RunTimeError = err;
                }
            }
        }

        /**
        /// Brokerages can send account updates, this include cash balance updates. Since it is of
        /// utmost important to always have an accurate picture of reality, we'll trust this information
        /// as truth
        */
        private void HandleAccountChanged(AccountEvent account) {
            // how close are we?
            delta = _algorithm.Portfolio.CashBook[account.CurrencySymbol].Amount - account.CashBalance;
            if( delta != 0) {
                Log.Trace( String.format( "BrokerageTransactionHandler.HandleAccountChanged(): %1$s Cash Delta: %2$s", account.CurrencySymbol, delta));
            }

            // maybe we don't actually want to do this, this data can be delayed. Must be explicitly supported by brokerage
            if( _brokerage.AccountInstantlyUpdated) {
                // @Override the current cash value so we're always guaranteed to be in sync with the brokerage's push updates
                _algorithm.Portfolio.CashBook[account.CurrencySymbol].SetAmount(account.CashBalance);
            }
        }

        /**
        /// Gets the amount of time since the last call to algorithm.Portfolio.ProcessFill(fill)
        */
        private Duration TimeSinceLastFill
        {
            get { return DateTime.Now - new DateTime(Interlocked.Read(ref _lastFillTimeTicks)); }
        }

        /**
        /// Gets the date of the last sync
        */
        private DateTime LastSyncDate
        {
            get { return new DateTime(Interlocked.Read(ref _lastSyncTimeTicks)).Date; }
        }

        /**
        /// Rounds off the order towards 0 to the nearest multiple of Lot Size
        */
        private int RoundOffOrder(Order order, Security security) {
            orderLotMod = order.Quantity% Integer.parseInt( security.SymbolProperties.LotSize);

            if( orderLotMod != 0) {
                order.Quantity = order.Quantity - orderLotMod;

                if( !_firstRoundOffMessage) {
                    _algorithm.Error(
                        String.format(
                            "Warning: Due to brokerage limitations, orders will be rounded to the nearest lot size of %1$s",
                             Integer.parseInt( security.SymbolProperties.LotSize)));
                    _firstRoundOffMessage = true;
                }
                return order.Quantity;
            }
            else
            {
                return order.Quantity;
            }
        }
    }
}
