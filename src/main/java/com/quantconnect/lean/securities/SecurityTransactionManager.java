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

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.function.Predicate;
import java.util.stream.Collectors;
import java.util.stream.Stream;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.orders.CancelOrderRequest;
import com.quantconnect.lean.orders.Order;
import com.quantconnect.lean.orders.OrderRequest;
import com.quantconnect.lean.orders.OrderTicket;
import com.quantconnect.lean.orders.SubmitOrderRequest;
import com.quantconnect.lean.orders.UpdateOrderRequest;

//using System.Threading;

/**
 * Algorithm Transactions Manager - Recording Transactions
 */
public class SecurityTransactionManager implements IOrderProvider {
    
    private static final BigDecimal minimumOrderSize = BigDecimal.ZERO;
    private static final int minimumOrderQuantity = 1;
    
    private final Logger log = LoggerFactory.getLogger( getClass() );
    private final SecurityManager securities;

    private AtomicInteger orderId;
    private long marketOrderFillTimeout = TimeUnit.SECONDS.toMillis( 5 );
    private IOrderProcessor _orderProcessor;
    private Map<LocalDateTime,BigDecimal> _transactionRecord;

    /**
     * Gets the time the security information was last updated
     */
    public LocalDateTime getUtcTime() {
        return securities.getUtcTime();
    }
    
    /**
     * Initialize the transaction manager for holding and processing orders.
     */
    public SecurityTransactionManager( SecurityManager security ) {
        //Private reference for processing transactions
        this.securities = security;

        //Internal storage for transaction records:
        this._transactionRecord = new HashMap<>();
    }

    /**
     * Trade record of profits and losses for each trade statistics calculations
     */
    public Map<LocalDateTime,BigDecimal> getTransactionRecord() {
        return _transactionRecord;
    }
    
    public void setTransactionRecord( Map<LocalDateTime,BigDecimal> value ) {
        _transactionRecord = value;
    }

    /**
     * Configurable minimum order value to ignore bad orders, or orders with unrealistic sizes
     * Default minimum order size is $0 value
     */
    public BigDecimal getMinimumOrderSize() {
        return minimumOrderSize;
    }

    /**
     * Configurable minimum order size to ignore bad orders, or orders with unrealistic sizes
     * Default minimum order size is 0 shares
     */
    public int getMinimumOrderQuantity() {
        return minimumOrderQuantity;
    }

    /**
     * Get the last order id.
     */
    public int getLastOrderId() {
        return orderId.get();
    }

    /**
     * Configurable timeout for market order fills in millis
     * Default value is 5 seconds
     */
    public long getMarketOrderFillTimeout() {
        return marketOrderFillTimeout;
    }
    
    public void setMarketOrderFillTimeout( long value ) {
        marketOrderFillTimeout = value;
    }

    /**
     * Processes the order request
     * @param request The request to be processed
     * @returns The order ticket for the request
     */
    public OrderTicket processRequest( OrderRequest request ) {
        if( request instanceof SubmitOrderRequest ) {
            final SubmitOrderRequest submit = (SubmitOrderRequest)request;
            submit.setOrderId( getIncrementOrderId() );
        }
        
        return _orderProcessor.process( request );
    }

    /**
     * Add an order to collection and return the unique order id or negative if an error.
     * @param request A request detailing the order to be submitted
     * @returns New unique, increasing orderid
     */
    public OrderTicket addOrder( SubmitOrderRequest request ) {
        return processRequest( request );
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
    public OrderTicket cancelOrder( int orderId ) {
        return removeOrder( orderId );
    }

    /**
     * Cancels all open orders for the specified symbol
     * @param symbol The symbol whose orders are to be cancelled
     * @returns List containing the cancelled order tickets
     */
    public List<OrderTicket> cancelOpenOrders( Symbol symbol ) {
        final List<OrderTicket> cancelledOrders = new ArrayList<OrderTicket>();
        getOrderTickets( x -> x.getSymbol().equals( symbol ) && x.getStatus().isOpen() ).forEach( ticket -> {
            ticket.cancel();
            cancelledOrders.add( ticket );
        } );
        
        return cancelledOrders;
    }

    /**
     * Remove this order from outstanding queue: user is requesting a cancel.
     * @param orderId Specific order id to remove
     */
    public OrderTicket removeOrder( int orderId ) {
        return processRequest( new CancelOrderRequest( securities.getUtcTime(), orderId, null ) );
    }

    /**
     * Gets and enumerable of <see cref="OrderTicket"/> matching the specified <paramref name="filter"/>
     * @returns An enumerable of <see cref="OrderTicket"/> matching the specified <paramref name="filter"/>
     */
    public Stream<OrderTicket> getOrderTickets() {
        return getOrderTickets( null );
    }
    
    /**
     * Gets and enumerable of <see cref="OrderTicket"/> matching the specified <paramref name="filter"/>
     * @param filter The filter predicate used to find the required order tickets
     * @returns An enumerable of <see cref="OrderTicket"/> matching the speStream<T> <paramref name="filter"/>
     */
    public Stream<OrderTicket> getOrderTickets( Predicate<OrderTicket> filter ) {
        return _orderProcessor.getOrderTickets( filter != null ? filter : (x -> true) );
    }

    /**
     * Gets the order ticket for the specified order id. Returns null if not found
     * @param orderId The order's id
     * @returns The order ticket with the specified id, or null if not found
     */
    public OrderTicket getOrderTicket( int orderId ) {
        return _orderProcessor.getOrderTicket( orderId );
    }

    /**
     * Wait for a specific order to be either Filled, Invalid or Canceled
     * @param orderId The id of the order to wait for
     * @returns True if we successfully wait for the fill, false if we were unable
     * to wait. This may be because it is not a market order or because the timeout
     * was reached
     */
    public boolean waitForOrder( int orderId ) {
        final OrderTicket orderTicket = getOrderTicket( orderId );
        if( orderTicket == null ) {
            log .error( "SecurityTransactionManager.waitForOrder(): Unable to locate ticket for order: " + orderId );
            return false;
        }

        boolean awaitResult;
        try {
            awaitResult = orderTicket.getOrderClosed().await(marketOrderFillTimeout, TimeUnit.MILLISECONDS );
        }
        catch( InterruptedException ie ) {
            awaitResult = false;
        }
        
        if( !awaitResult ) {
            log.error( "SecurityTransactionManager.waitForOrder(): Order did not fill within {} seconds.", TimeUnit.MILLISECONDS.toSeconds( marketOrderFillTimeout ) );
            return false;
        }

        return true;
    }

    /**
     * Get a list of all open orders.
     * @returns List of open orders.
     */
    public List<Order> getOpenOrders() {
        return _orderProcessor.getOrders( x -> x.getStatus().isOpen() ).collect( Collectors.toList() );
    }

    /**
     * Get a list of all open orders for a symbol.
     * @param symbol The symbol for which to return the orders
     * @returns List of open orders.
     */
    public List<Order> getOpenOrders( Symbol symbol ) {
        return _orderProcessor.getOrders( x -> x.getSymbol().equals( symbol ) && x.getStatus().isOpen() ).collect( Collectors.toList() );
    }

    /**
     * Gets the current number of orders that have been processed
    */
    public int getOrdersCount() {
        return _orderProcessor.getOrdersCount();
    }

    /**
     * Get the order by its id
     * @param orderId Order id to fetch
     * @returns The order with the specified id, or null if no match is found
     */
    public Order getOrderById( int orderId ) {
        return _orderProcessor.getOrderById( orderId );
    }

    /**
     * Gets the order by its brokerage id
     * @param brokerageId The brokerage id to fetch
     * @returns The first order matching the brokerage id, or null if no match is found
     */
    public Order getOrderByBrokerageId( String brokerageId ) {
        return _orderProcessor.getOrderByBrokerageId( brokerageId );
    }

    /**
     * Gets all orders matching the specified filter
     * @param filter Delegate used to filter the orders
     * @returns All open orders this order provider currently holds
     */
    public Stream<Order> getOrders( Predicate<Order> filter ) {
        return _orderProcessor.getOrders( filter );
    }

    /**
     * Check if there is sufficient capital to execute this order.
     * @param portfolio Our portfolio
     * @param order Order we're checking
     * @returns True if sufficient capital.
     */
    public boolean getSufficientCapitalForOrder( SecurityPortfolioManager portfolio, Order order ) {
        // short circuit the div 0 case
        if( order.getQuantity() == 0 ) 
            return true;

        final Security security = securities.get( order.getSymbol() );

        final OrderTicket ticket = getOrderTicket( order.getId() );
        if( ticket == null ) {
            log.error( "SecurityTransactionManager.getSufficientCapitalForOrder(): Null order ticket for id: " + order.getId() );
            return false;
        }

        // When order only reduces or closes a security position, capital is always sufficient
        if( security.getHoldings().getQuantity() * order.getQuantity() < 0 && 
                Math.abs( security.getHoldings().getQuantity() ) >= Math.abs( order.getQuantity() ) )
            return true;

        final BigDecimal freeMargin = security.getMarginModel().getMarginRemaining( portfolio, security, order.getDirection() );
        final BigDecimal initialMarginRequiredForOrder = security.getMarginModel().getInitialMarginRequiredForOrder( security, order );

        // pro-rate the initial margin required for order based on how much has already been filled
        final double percentUnfilled = (Math.abs( order.getQuantity() ) - Math.abs( ticket.getQuantityFilled() ) ) / (double)Math.abs( order.getQuantity() );
        final BigDecimal initialMarginRequiredForRemainderOfOrder = initialMarginRequiredForOrder.multiply( BigDecimal.valueOf( percentUnfilled ) );

        if( initialMarginRequiredForRemainderOfOrder.abs().compareTo( freeMargin ) > 0 ) {
            log.error( String.format( "SecurityTransactionManager.GetSufficientCapitalForOrder(): Id: %1$s, Initial Margin: %2$s, Free Margin: %3$s", order.getId(), initialMarginRequiredForOrder, freeMargin ) );
            return false;
        }
        
        return true;
    }

    /**
     * Get a new order id, and increment the internal counter.
     * @returns New unique int order id.
     */
    public int getIncrementOrderId() {
        return orderId.incrementAndGet();
    }

    /**
     * Sets the <see cref="IOrderProvider"/> used for fetching orders for the algorithm
     * @param orderProvider The <see cref="IOrderProvider"/> to be used to manage fetching orders
     */
    public void setOrderProcessor( IOrderProcessor orderProvider ) {
        this._orderProcessor = orderProvider;
    }

//    /**
//     * Returns true when the specified order is in a completed state
//     */
//    private static boolean completed( Order order ) {
//        final OrderStatus status = order.getStatus();
//        return status == OrderStatus.Filled || status == OrderStatus.PartiallyFilled || status == OrderStatus.Invalid || status == OrderStatus.Canceled;
//    }
}
