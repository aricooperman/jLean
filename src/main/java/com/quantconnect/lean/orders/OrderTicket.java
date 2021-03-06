﻿/*
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

package com.quantconnect.lean.orders;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;
import java.util.Optional;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.ReentrantLock;
import java.util.function.Function;

import com.google.common.collect.ImmutableList;
import com.quantconnect.lean.SecurityType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.orders.OrderTypes.OrderStatus;
import com.quantconnect.lean.orders.OrderTypes.OrderType;
import com.quantconnect.lean.securities.SecurityTransactionManager;

//using System.Threading;

/**
 * Provides a single reference to an order for the algorithm to maintain. As the order gets
 * updated this ticket will also get updated
 */
public final class OrderTicket {
    
    private final ReentrantLock orderEventsLock = new ReentrantLock();
    private final Object updateRequestsLock = new Object();
    private final Object setCancelRequestLock = new Object();

    private Order order;
    private Optional<OrderStatus> orderStatusOverride;
    private CancelOrderRequest cancelRequest;

    private int quantityFilled;
    private BigDecimal averageFillPrice;

    private final int orderId;
    private final List<OrderEvent> orderEvents; 
    private final SubmitOrderRequest submitRequest;
    private final Condition /*ManualResetEvent*/ orderStatusClosedEvent;
    private final LinkedList<UpdateOrderRequest> updateRequests;

    // we pull this in to provide some behavior/simplicity to the ticket API
    private final SecurityTransactionManager transactionManager;

    /**
     * Gets the order id of this ticket
     */
    public int getOrderId() {
        return orderId;
    }

    /**
     * Gets the current status of this order ticket
     */
    public OrderStatus getStatus() {
        if( orderStatusOverride.isPresent() ) 
            return orderStatusOverride.get();
        return order == null ? OrderStatus.New : order.getStatus();
    }

    /**
     * Gets the symbol being ordered
     */
    public Symbol getSymbol() {
         return submitRequest.getSymbol();
    }

    /**
     * Gets the <see cref="Symbol"/>'s <see cref="SecurityType"/>
     */
    public SecurityType getSecurityType() {
        return submitRequest.getSecurityType();
    }

    /**
     * Gets the number of units ordered
     */
    public int getQuantity() {
        return order == null ? submitRequest.getQuantity() : order.quantity;
    }

    /**
     * Gets the average fill price for this ticket. If no fills have been processed
     * then this will return a value of zero.
     */
    public BigDecimal getAverageFillPrice() {
        return averageFillPrice;
    }

    /**
     * Gets the total qantity filled for this ticket. If no fills have been processed
     * then this will return a value of zero.
     */
    public int getQuantityFilled() {
        return quantityFilled;
    }

    /**
     * Gets the time this order was last updated
     */
    public LocalDateTime getTime() {
        return getMostRecentOrderRequest().getTime();
    }

    /**
     * Gets the type of order
     */
    public OrderType getOrderType() {
        return submitRequest.getOrderType();
    }

    /**
     * Gets the order's current tag
     */
    public String getTag() {
        return order == null ? submitRequest.getTag() : order.getTag();
    }

    /**
     * Gets the <see cref="SubmitOrderRequest"/> that initiated this order
     */
    public SubmitOrderRequest getSubmitRequest() {
        return submitRequest;
    }

    /**
     * Gets a list of <see cref="UpdateOrderRequest"/> containing an item for each
     * <see cref="UpdateOrderRequest"/> that was sent for this order id
     */
    public List<UpdateOrderRequest> getUpdateRequests() {
        synchronized( updateRequestsLock ) {
            return ImmutableList.copyOf( updateRequests );
        }
    }

    /**
     * Gets the <see cref="CancelOrderRequest"/> if this order was canceled. If this order
     * was not canceled, this will return null
     */
    public CancelOrderRequest getCancelRequest() {
        return cancelRequest;
    }

    /**
     * Gets a list of all order events for this ticket
     */
    public List<OrderEvent> getOrderEvents() {
        synchronized( orderEventsLock ) {
            return ImmutableList.copyOf( orderEvents );
        }
    }

    /**
     * Gets a Condition that can be used to wait until this order has filled
    */
    public Condition getOrderClosed() {
        return orderStatusClosedEvent;
    }

    /**
     * Initializes a new instance of the <see cref="OrderTicket"/> class
     * @param transactionManager The transaction manager used for submitting updates and cancels for this ticket
     * @param submitRequest The order request that initiated this order ticket
     */
    public OrderTicket( SecurityTransactionManager transactionManager, SubmitOrderRequest submitRequest ) {
        this.submitRequest = submitRequest;
        this.orderId = submitRequest.orderId;
        this.transactionManager = transactionManager;

        this.orderEvents = new ArrayList<OrderEvent>();
        this.updateRequests = new LinkedList<UpdateOrderRequest>();
        this.orderStatusClosedEvent = orderEventsLock.newCondition(); // new ManualResetEvent( false );
    }

    /**
     * Gets the specified field from the ticket
     * @param field The order field to get
     * @returns The value of the field
     * @throws IllegalArgumentException
     */
    public BigDecimal get( OrderField field ) {
        switch( field ) {
            case LimitPrice:
                if( submitRequest.getOrderType() == OrderType.Limit )
                    return OrderTicket.<LimitOrder>accessOrder( this, field, o -> o.getLimitPrice(), r -> r.getLimitPrice() );
                
                if( submitRequest.getOrderType() == OrderType.StopLimit )
                    return OrderTicket.<StopLimitOrder>accessOrder( this, field, o -> o.getLimitPrice(), r -> r.getLimitPrice() );
                
                break;

            case StopPrice:
                if( submitRequest.getOrderType() == OrderType.StopLimit )
                    return OrderTicket.<StopLimitOrder>accessOrder( this, field, o -> o.getStopPrice(), r -> r.getStopPrice() );
                
                if( submitRequest.getOrderType() == OrderType.StopMarket )
                    return OrderTicket.<StopMarketOrder>accessOrder( this, field, o -> o.getStopPrice(), r -> r.getStopPrice() );
                
                break;

            default:
                throw new IllegalArgumentException( field + " is not supported" );
        }
        
        throw new IllegalArgumentException( "Unable to get field " + field + " on order of type " + submitRequest.getOrderType() );
    }

    /**
     * Submits an <see cref="UpdateOrderRequest"/> with the <see cref="SecurityTransactionManager"/> to update
     * the ticket with data specified in <paramref name="fields"/>
     * @param fields Defines what properties of the order should be updated
     * @returns The <see cref="OrderResponse"/> from updating the order
     */
    public OrderResponse update( UpdateOrderFields fields ) {
        transactionManager.updateOrder( new UpdateOrderRequest( transactionManager.getUtcTime(), submitRequest.orderId, fields ) );
        return updateRequests.getLast().getResponse();
    }

    /**
     * Submits a new request to cancel this order
     */
    public OrderResponse cancel( String tag ) {
        final CancelOrderRequest request = new CancelOrderRequest( transactionManager.getUtcTime(), orderId, tag );
        transactionManager.processRequest( request );
        return cancelRequest.getResponse();
    }
    
    public OrderResponse cancel() {
        return cancel( null );
    }

    /**
     * Gets the most recent <see cref="OrderResponse"/> for this ticket
     * @returns The most recent <see cref="OrderResponse"/> for this ticket
     */
    public OrderResponse getMostRecentOrderResponse() {
        return getMostRecentOrderRequest().getResponse();
    }

    /**
     * Gets the most recent <see cref="OrderRequest"/> for this ticket
     * @returns The most recent <see cref="OrderRequest"/> for this ticket
    */
    public OrderRequest getMostRecentOrderRequest() {
        if( cancelRequest != null )
            return cancelRequest;
        
        final UpdateOrderRequest lastUpdate = updateRequests.peekLast();
        if( lastUpdate != null )
            return lastUpdate;
        
        return getSubmitRequest();
    }

    /**
     * Adds an order event to this ticket
     * @param orderEvent The order event to be added
     */
    void addOrderEvent( OrderEvent orderEvent ) {
        orderEventsLock.lock();
        try {
            orderEvents.add( orderEvent );
            if( orderEvent.fillQuantity != 0 ) {
                // keep running totals of quantity filled and the average fill price so we
                // don't need to compute these on demand
                quantityFilled += orderEvent.fillQuantity;
                final BigDecimal quantityWeightedFillPrice = orderEvents.stream()
                        .filter( x -> x.status.isFill() )
                        .map( x -> x.fillPrice.multiply( BigDecimal.valueOf( x.getAbsoluteFillQuantity() ) ) )
                        .reduce( BigDecimal.ZERO, BigDecimal::add );
                averageFillPrice = quantityWeightedFillPrice.divide( BigDecimal.valueOf( Math.abs( quantityFilled ) ), RoundingMode.HALF_UP );
            }
        
            // fire the wait handle indicating this order is closed
            if( orderEvent.status.isClosed() )
                orderStatusClosedEvent.signalAll(); //.Set();
        } 
        finally {
            orderEventsLock.unlock();
        }
    }

    /**
     * Updates the internal order object with the current state
     * @param order The order
     */
    void setOrder( Order order ) {
        if( order != null && order.id != order.id )
            throw new IllegalArgumentException( "Order id mismatch" );

        this.order = order;
    }

    /**
     * Adds a new <see cref="UpdateOrderRequest"/> to this ticket.
     * @param request The recently processed <see cref="UpdateOrderRequest"/>
     */
     void addUpdateRequest( UpdateOrderRequest request ) {
        if( request.orderId != orderId )
            throw new IllegalArgumentException( "Received UpdateOrderRequest for incorrect order id." );

        synchronized( updateRequestsLock ) {
            updateRequests.add( request );
        }
    }

    /**
     * Sets the <see cref="CancelOrderRequest"/> for this ticket. This can only be performed once.
     * 
     * This method is thread safe.
     * 
     * @param request The <see cref="CancelOrderRequest"/> that canceled this ticket.
     * @returns False if the the CancelRequest has already been set, true if this call set it
     */
    boolean trySetCancelRequest( CancelOrderRequest request ) {
        if( request.orderId != orderId )
            throw new IllegalArgumentException( "Received CancelOrderRequest for incorrect order id." );
        
        synchronized( setCancelRequestLock ) {
            if( cancelRequest != null )
                return false;
            
            cancelRequest = request;
        }
        
        return true;
    }

    /**
     * Creates a new <see cref="OrderTicket"/> that represents trying to cancel an order for which no ticket exists
     */
    public static OrderTicket invalidCancelOrderId( SecurityTransactionManager transactionManager, CancelOrderRequest request ) {
        final SubmitOrderRequest submit = new SubmitOrderRequest( OrderType.Market, SecurityType.Base, Symbol.EMPTY, 0, BigDecimal.ZERO, BigDecimal.ZERO, LocalDateTime.MAX, null );
        submit.setResponse( OrderResponse.unableToFindOrder( request ) );
        final OrderTicket ticket = new OrderTicket( transactionManager, submit );
        request.setResponse( OrderResponse.unableToFindOrder( request ) );
        ticket.trySetCancelRequest( request );
        ticket.orderStatusOverride = Optional.of( OrderStatus.Invalid );
        return ticket;
    }

    /**
     * Creates a new <see cref="OrderTicket"/> tht represents trying to update an order for which no ticket exists
     */
    public static OrderTicket invalidUpdateOrderId( SecurityTransactionManager transactionManager, UpdateOrderRequest request ) {
        final SubmitOrderRequest submit = new SubmitOrderRequest( OrderType.Market, SecurityType.Base, Symbol.EMPTY, 0, BigDecimal.ZERO, BigDecimal.ZERO, LocalDateTime.MAX, null );
        submit.setResponse( OrderResponse.unableToFindOrder( request ) );
        final OrderTicket ticket = new OrderTicket( transactionManager, submit );
        request.setResponse( OrderResponse.unableToFindOrder( request ) );
        ticket.addUpdateRequest( request );
        ticket.orderStatusOverride = Optional.of( OrderStatus.Invalid );
        return ticket;
    }

    /**
     * Creates a new <see cref="OrderTicket"/> that represents trying to submit a new order that had errors embodied in the <paramref name="response"/>
     */
    public static OrderTicket invalidSubmitRequest( SecurityTransactionManager transactionManager, SubmitOrderRequest request, OrderResponse response ) {
        request.setResponse( response );
        final OrderTicket orderTicket = new OrderTicket( transactionManager, request );
        orderTicket.orderStatusOverride = Optional.of( OrderStatus.Invalid );
        return  orderTicket;
    }

    /**
     * Creates a new <see cref="OrderTicket"/> that is invalidated because the algorithm was in the middle of warm up still
     */
    public static OrderTicket invalidWarmingUp( SecurityTransactionManager transactionManager, SubmitOrderRequest submit ) {
        submit.setResponse( OrderResponse.warmingUp( submit ) );
        final OrderTicket ticket = new OrderTicket( transactionManager, submit );
        ticket.orderStatusOverride = Optional.of( OrderStatus.Invalid );
        return ticket;
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        final String counts = "Request Count: " + requestCount() + " Response Count: " + responseCount();
        if( order != null )
            return orderId + ": " + order + " " + counts;
        
        return orderId + ": " + counts;
    }

    private int responseCount() {
        return ( submitRequest.getResponse() == OrderResponse.UNPROCESSED ? 0 : 1 ) 
             + ( cancelRequest == null || cancelRequest.getResponse() == OrderResponse.UNPROCESSED ? 0 : 1 )
             + (int)updateRequests.stream().filter( x -> x.getResponse() != OrderResponse.UNPROCESSED ).count();
    }

    private int requestCount() {
        return 1 + updateRequests.size() + (cancelRequest == null ? 0 : 1);
    }

    /**
     * This is provided for API backward compatibility and will resolve to the order ID, except during
     * an error, where it will return the integer value of the <see cref="OrderResponseErrorCode"/> from
     * the most recent response
    */
    public int getOrderIdAsInt() {
        final OrderResponse response = getMostRecentOrderResponse();
        if( response != null && response.isError() )
            return response.getErrorCode().getCode();
        
        return getOrderId();
    }


    @SuppressWarnings("unchecked")
    private static <T extends Order> BigDecimal accessOrder( OrderTicket ticket, OrderField field, Function<T,BigDecimal> orderSelector, Function<SubmitOrderRequest,BigDecimal> requestSelector ) {
        final Order order = ticket.order;
        if( order == null )
            return requestSelector.apply( ticket.submitRequest );
        
        try {
            return orderSelector.apply( (T)order );
        }
        catch( ClassCastException e ) {
            throw new IllegalArgumentException( String.format( "Unable to access property %1$s on order of type %2$s", field, order.getType() ) );
        }
    }
}
