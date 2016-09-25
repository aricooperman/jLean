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


package com.quantconnect.lean.securities;

import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.StreamSupport;

import com.quantconnect.lean.orders.OrderTicket;
import com.quantconnect.lean.orders.SubmitOrderRequest;

/**
 * Represents the model responsible for picking which orders should be executed during a margin call
 * 
 * This is a default implementation that orders the generated margin call orders by the unrealized
 * profit (losers first) and executes each order synchronously until we're within the margin requirements
 * 
 */
public class MarginCallModel {
        
    private SecurityPortfolioManager portfolio;

    /**
     * Gets the portfolio that margin calls will be transacted against
     */
    public SecurityPortfolioManager getPortfolio() {
        return portfolio;
    }

    /**
     * Initializes a new instance of the <see cref="MarginCallModel"/> class
     * @param portfolio The portfolio object to receive margin calls
     */
    public MarginCallModel( SecurityPortfolioManager portfolio ) {
        this.portfolio = portfolio;
    }

    /**
     * Executes synchronous orders to bring the account within margin requirements.
     * @param generatedMarginCallOrders These are the margin call orders that were generated
     * by individual security margin models.
     * @returns The list of orders that were actually executed
     */
    public List<OrderTicket> executeMarginCall( Iterable<SubmitOrderRequest> generatedMarginCallOrders ) {
        // if our margin used is back under the portfolio value then we can stop liquidating
        if( portfolio.getMarginRemaining().signum() >= 0 )
            return Collections.emptyList();

        // order by losers first
        final List<OrderTicket> executedOrders = new ArrayList<OrderTicket>();
        final Collection<SubmitOrderRequest> orderedByLosers = StreamSupport.stream( generatedMarginCallOrders.spliterator(), false )
                .sorted( Comparator.comparing( e -> portfolio.get( e.getSymbol() ).getUnrealizedProfit(), BigDecimal::compareTo ) )
                .collect( Collectors.toList() );
        for( SubmitOrderRequest request : orderedByLosers ) {
            final OrderTicket ticket = portfolio.transactions.addOrder( request );
            portfolio.transactions.waitForOrder( request.getOrderId() );
            executedOrders.add( ticket );

            // if our margin used is back under the portfolio value then we can stop liquidating
            if( portfolio.getMarginRemaining().signum() >= 0 )
                break;
        }
        
        return executedOrders;
    }
}