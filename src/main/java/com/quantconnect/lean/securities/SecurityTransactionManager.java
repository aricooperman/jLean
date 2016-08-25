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

package com.quantconnect.lean.securities;

import java.time.Duration;
import java.time.LocalDateTime;
import java.util.Map;

//using System.Threading;
//using QuantConnect.Logging;
//using QuantConnect.Orders;

/**
 * Algorithm Transactions Manager - Recording Transactions
*/
public class SecurityTransactionManager implements IOrderProvider {
    private int _orderId;
    private final SecurityManager _securities;
    private static final BigDecimal _minimumOrderSize = 0;
    private static final int _minimumOrderQuantity = 1;
    private Duration _marketOrderFillTimeout = Duration.ofSeconds(5);

    private IOrderProcessor _orderProcessor;
    private Map<DateTime, decimal> _transactionRecord;

    /**
     * Gets the time the security information was last updated
    */
    public LocalDateTime UtcTime
    {
        get { return _securities.UtcTime; }
    }
    
    /**
     * Initialise the transaction manager for holding and processing orders.
    */
    public SecurityTransactionManager(SecurityManager security) {
        //Private reference for processing transactions
        _securities = security;

        //Internal storage for transaction records:
        _transactionRecord = new Map<DateTime, decimal>();
    }

    /**
     * Trade record of profits and losses for each trade statistics calculations
    */
    public Map<DateTime, decimal> TransactionRecord
    {
        get
        {
            return _transactionRecord;
        }
        set
        {
            _transactionRecord = value;
        }
    }

    /**
     * Configurable minimum order value to ignore bad orders, or orders with unrealistic sizes
    */
     * Default minimum order size is $0 value
    public BigDecimal MinimumOrderSize 
    {
        get 
        {
            return _minimumOrderSize;
        }
    }

    /**
     * Configurable minimum order size to ignore bad orders, or orders with unrealistic sizes
    */
     * Default minimum order size is 0 shares
    public int MinimumOrderQuantity 
    {
        get 
        {
            return _minimumOrderQuantity;
        }
    }

    /**
     * Get the last order id.
    */
    public int LastOrderId
    {
        get
        {
            return _orderId;
        }
    }

    /**
     * Configurable timeout for market order fills
    */
     * Default value is 5 seconds
    public Duration MarketOrderFillTimeout
    {
        get
        {
            return _marketOrderFillTimeout;
        }
        set
        {
            _marketOrderFillTimeout = value;
        }
    }

    /**
     * Processes the order request
     * @param request The request to be processed
     * @returns The order ticket for the request
     */
    public OrderTicket processRequest( OrderRequest request ) {
        if( request instanceof SubmitOrderRequest )
            submit = (SubmitOrderRequest)request;
            submit.setOrderId( getIncrementOrderId() );
        
        return _orderProcessor.process( request );
    }

    /**
     * Add an order to collection and return the unique order id or negative if an error.
    */
     * @param request A request detailing the order to be submitted
    @returns New unique, increasing orderid
    public OrderTicket AddOrder(SubmitOrderRequest request) {
        return ProcessRequest(request);
    }

    /**
     * Update an order yet to be filled such as stop or limit orders.
     * @param request Request detailing how the order should be updated
     * Does not apply if the order is already fully filled
     */
    public OrderTicket updateOrder( UpdateOrderRequest request ) {
        return processRequest( request );
    }

    /**
     * Added alias for RemoveOrder - 
     * @param orderId Order id we wish to cancel
     */
    public OrderTicket CancelOrder(int orderId) {
        return RemoveOrder(orderId);
    }

    /**
     * Cancels all open orders for the specified symbol
    */
     * @param symbol The symbol whose orders are to be cancelled
    @returns List containing the cancelled order tickets
    public List<OrderTicket> CancelOpenOrders(Symbol symbol) {
        cancelledOrders = new List<OrderTicket>();
        foreach (ticket in GetOrderTickets(x -> x.Symbol == symbol && x.Status.IsOpen())) {
            ticket.Cancel();
            cancelledOrders.Add(ticket);
        }
        return cancelledOrders;
    }

    /**
     * Remove this order from outstanding queue: user is requesting a cancel.
    */
     * @param orderId Specific order id to remove
    public OrderTicket RemoveOrder(int orderId) {
        return ProcessRequest(new CancelOrderRequest(_securities.UtcTime, orderId, string.Empty));
    }

    /**
     * Gets and enumerable of <see cref="OrderTicket"/> matching the specified <paramref name="filter"/>
    */
     * @param filter The filter predicate used to find the required order tickets
    @returns An enumerable of <see cref="OrderTicket"/> matching the specified <paramref name="filter"/>
    public IEnumerable<OrderTicket> GetOrderTickets(Func<OrderTicket, bool> filter = null ) {
        return _orderProcessor.GetOrderTickets(filter ?? (x -> true));
    }

    /**
     * Gets the order ticket for the specified order id. Returns null if not found
    */
     * @param orderId The order's id
    @returns The order ticket with the specified id, or null if not found
    public OrderTicket GetOrderTicket(int orderId) {
        return _orderProcessor.GetOrderTicket(orderId);
    }

    /**
     * Wait for a specific order to be either Filled, Invalid or Canceled
    */
     * @param orderId The id of the order to wait for
    @returns True if we successfully wait for the fill, false if we were unable
     * to wait. This may be because it is not a market order or because the timeout
     * was reached
    public boolean WaitForOrder(int orderId) {
        orderTicket = GetOrderTicket(orderId);
        if( orderTicket == null ) {
            Log.Error( "SecurityTransactionManager.WaitForOrder(): Unable to locate ticket for order: " + orderId);
            return false;
        }

        if( !orderTicket.OrderClosed.WaitOne(_marketOrderFillTimeout)) {
            Log.Error( "SecurityTransactionManager.WaitForOrder(): Order did not fill within %1$s seconds.", _marketOrderFillTimeout.TotalSeconds);
            return false;
        }

        return true;
    }

    /**
     * Get a list of all open orders.
    */
    @returns List of open orders.
    public List<Order> GetOpenOrders() {
        return _orderProcessor.GetOrders(x -> x.Status.IsOpen()).ToList();
    }

    /**
     * Get a list of all open orders for a symbol.
    */
     * @param symbol The symbol for which to return the orders
    @returns List of open orders.
    public List<Order> GetOpenOrders(Symbol symbol) {
        return _orderProcessor.GetOrders(x -> x.Symbol == symbol && x.Status.IsOpen()).ToList();
    }

    /**
     * Gets the current number of orders that have been processed
    */
    public int OrdersCount
    {
        get { return _orderProcessor.OrdersCount; }
    }

    /**
     * Get the order by its id
    */
     * @param orderId Order id to fetch
    @returns The order with the specified id, or null if no match is found
    public Order GetOrderById(int orderId) {
        return _orderProcessor.GetOrderById(orderId);
    }

    /**
     * Gets the order by its brokerage id
    */
     * @param brokerageId The brokerage id to fetch
    @returns The first order matching the brokerage id, or null if no match is found
    public Order GetOrderByBrokerageId( String brokerageId) {
        return _orderProcessor.GetOrderByBrokerageId(brokerageId);
    }

    /**
     * Gets all orders matching the specified filter
    */
     * @param filter Delegate used to filter the orders
    @returns All open orders this order provider currently holds
    public IEnumerable<Order> GetOrders(Func<Order, bool> filter) {
        return _orderProcessor.GetOrders(filter);
    }

    /**
     * Check if there is sufficient capital to execute this order.
    */
     * @param portfolio Our portfolio
     * @param order Order we're checking
    @returns True if sufficient capital.
    public boolean GetSufficientCapitalForOrder(SecurityPortfolioManager portfolio, Order order) {
        // short circuit the div 0 case
        if( order.Quantity == 0) return true;

        security = _securities[order.Symbol];

        ticket = GetOrderTicket(order.Id);
        if( ticket == null ) {
            Log.Error( "SecurityTransactionManager.GetSufficientCapitalForOrder(): Null order ticket for id: " + order.Id);
            return false;
        }

        // When order only reduces or closes a security position, capital is always sufficient
        if( security.Holdings.Quantity * order.Quantity < 0 && Math.Abs(security.Holdings.Quantity) >= Math.Abs(order.Quantity)) return true;

        freeMargin = security.MarginModel.GetMarginRemaining(portfolio, security, order.Direction);
        initialMarginRequiredForOrder = security.MarginModel.GetInitialMarginRequiredForOrder(security, order);

        // pro-rate the initial margin required for order based on how much has already been filled
        percentUnfilled = (Math.Abs(order.Quantity) - Math.Abs(ticket.QuantityFilled))/Math.Abs(order.Quantity);
        initialMarginRequiredForRemainderOfOrder = percentUnfilled*initialMarginRequiredForOrder;

        if( Math.Abs(initialMarginRequiredForRemainderOfOrder) > freeMargin) {
            Log.Error( String.format( "SecurityTransactionManager.GetSufficientCapitalForOrder(): Id: %1$s, Initial Margin: %2$s, Free Margin: %3$s", order.Id, initialMarginRequiredForOrder, freeMargin));
            return false;
        }
        return true;
    }

    /**
     * Get a new order id, and increment the internal counter.
    */
    @returns New unique int order id.
    public int GetIncrementOrderId() {
        return Interlocked.Increment(ref _orderId);
    }

    /**
     * Sets the <see cref="IOrderProvider"/> used for fetching orders for the algorithm
    */
     * @param orderProvider The <see cref="IOrderProvider"/> to be used to manage fetching orders
    public void SetOrderProcessor(IOrderProcessor orderProvider) {
        _orderProcessor = orderProvider;
    }

    /**
     * Returns true when the specified order is in a completed state
    */
    private static boolean Completed(Order order) {
        return order.Status == OrderStatus.Filled || order.Status == OrderStatus.PartiallyFilled || order.Status == OrderStatus.Invalid || order.Status == OrderStatus.Canceled;
    }
}
