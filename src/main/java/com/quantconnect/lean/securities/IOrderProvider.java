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

import java.util.function.Function;
import java.util.stream.Stream;

import com.quantconnect.lean.orders.Order;
import com.quantconnect.lean.orders.OrderTicket;

/**
 * Represents a type capable of fetching Order instances by its QC order id or by a brokerage id
 */
public interface IOrderProvider {
    
    /**
     * Gets the current number of orders that have been processed
     */
    int getOrdersCount();

    /**
     * Get the order by its id
     * @param orderId Order id to fetch
     * @returns The order with the specified id, or null if no match is found
     */
    Order getOrderById( int orderId );

    /**
     * Gets the order by its brokerage id
     * @param brokerageId The brokerage id to fetch
     * @returns The first order matching the brokerage id, or null if no match is found
     */
    Order getOrderByBrokerageId( String brokerageId );

    /**
     * Gets and enumerable of <see cref="OrderTicket"/> matching the specified <paramref name="filter"/>
     * @param filter The filter predicate used to find the required order tickets. If null is specified then all tickets are returned
     * @returns An enumerable of <see cref="OrderTicket"/> matching the specified <paramref name="filter"/>
     */
    Stream<OrderTicket> getOrderTickets( Function<OrderTicket,Boolean> filter );

    default Stream<OrderTicket> getOrderTickets() {
        return getOrderTickets( null );
    }
    
    /**
     * Gets the order ticket for the specified order id. Returns null if not found
     * @param orderId The order's id
     * @returns The order ticket with the specified id, or null if not found
    */
    OrderTicket getOrderTicket( int orderId );

    /**
     * Gets all orders matching the specified filter. Specifying null will return an enumerable
     * of all orders.
     * @param filter Delegate used to filter the orders
     * @returns All open orders this order provider currently holds
    */
    Stream<Order> getOrders( Function<Order,Boolean> filter );
    
    default Stream<Order> getOrders() {
        return getOrders( null );
    }

    /**
     * Gets the order by its brokerage id
     * @param orderProvider The order provider to search
     * @param brokerageId The brokerage id to fetch
     * @returns The first order matching the brokerage id, or null if no match is found
     */
    default Order getOrderByBrokerageId( long brokerageId ) {
        return getOrderByBrokerageId( Long.toString( brokerageId ) );
    }

    /**
     * Gets the order by its brokerage id
     * @param orderProvider The order provider to search
     * @param brokerageId The brokerage id to fetch
     * @returns The first order matching the brokerage id, or null if no match is found
     */
    default Order getOrderByBrokerageId( int brokerageId ) {
        return getOrderByBrokerageId( Integer.toString( brokerageId ) );
    }
}
