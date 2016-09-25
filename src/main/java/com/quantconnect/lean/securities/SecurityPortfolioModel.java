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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.quantconnect.lean.Extensions;
import com.quantconnect.lean.Global;
import com.quantconnect.lean.SecurityType;
import com.quantconnect.lean.orders.OrderEvent;
import com.quantconnect.lean.orders.OrderTypes.OrderDirection;
import com.quantconnect.lean.securities.forex.Forex;

/**
 * Provides a default implementation of <see cref="ISecurityPortfolioModel"/> that simply
 * applies the fills to the algorithm's portfolio. This implementation is intended to 
 * handle all security types.
 */
public class SecurityPortfolioModel implements ISecurityPortfolioModel {
 
    private final Logger log = LoggerFactory.getLogger( getClass() );

    /**
     * Performs application of an OrderEvent to the portfolio
     * @param portfolio The algorithm's portfolio
     * @param security The fill's security
     * @param fill The order event fill object to be applied
     */
    public void processFill( SecurityPortfolioManager portfolio, Security security, OrderEvent fill ) {
        final Cash quoteCash = security.getQuoteCurrency();
        //Get the required information from the vehicle this order will affect
        final SecurityHolding holdings = security.getHoldings();
        final boolean isLong = holdings.isLong();
        final boolean isShort = holdings.isShort();
        //Make local decimals to avoid any rounding errors from int multiplication
        final int absoluteHoldingsQuantity = holdings.getAbsoluteQuantity();

        BigDecimal averageHoldingsPrice = holdings.getAveragePrice();
        boolean closedPosition = false;
        int quantityHoldings = holdings.getQuantity();

        try {
            // apply sales value to holdings in the account currency
            final BigDecimal saleValueInQuoteCurrency = fill.fillPrice.multiply( BigDecimal.valueOf( fill.getAbsoluteFillQuantity() ) ).multiply( security.getSymbolProperties().getContractMultiplier() );
            final BigDecimal saleValue = saleValueInQuoteCurrency.multiply( quoteCash.getConversionRate() );
            holdings.addNewSale( saleValue );

            // subtract transaction fees from the portfolio (assumes in account currency)
            final BigDecimal feeThisOrder = fill.orderFee.abs();
            holdings.addNewFee( feeThisOrder );
            portfolio.getCashBook().get( CashBook.ACCOUNT_CURRENCY ).addAmount( feeThisOrder.negate() );

            // apply the funds using the current settlement model
            security.getSettlementModel().applyFunds( portfolio, security, fill.utcTime, quoteCash.getSymbol(), BigDecimal.valueOf( -fill.fillQuantity ).multiply( fill.fillPrice ).multiply( security.getSymbolProperties().getContractMultiplier() ) );
            if( security.getType() == SecurityType.Forex ) {
                // model forex fills as currency swaps
                final Forex forex = (Forex) security;
                security.getSettlementModel().applyFunds( portfolio, security, fill.utcTime, forex.getBaseCurrencySymbol(), BigDecimal.valueOf( fill.fillQuantity ) );
            }
            
            // did we close or open a position further?
            closedPosition = isLong && fill.getDirection() == OrderDirection.Sell || isShort && fill.getDirection() == OrderDirection.Buy;

            // calculate the last trade profit
            if( closedPosition ) {
                // profit = (closed sale value - cost)*conversion to account currency
                // closed sale value = quantity closed * fill price       BUYs are deemed negative cash flow
                // cost = quantity closed * average holdings price        SELLS are deemed positive cash flow
                final int absoluteQuantityClosed = Math.min( fill.getAbsoluteFillQuantity(), absoluteHoldingsQuantity );
                final BigDecimal closedSaleValueInQuoteCurrency = fill.fillPrice.multiply( BigDecimal.valueOf( Math.signum( -fill.fillQuantity ) * absoluteQuantityClosed ) );
                final BigDecimal closedCost = averageHoldingsPrice.multiply( BigDecimal.valueOf( Math.signum( -fill.fillQuantity ) * absoluteQuantityClosed ) );
                final BigDecimal conversionFactor = security.getQuoteCurrency().getConversionRate().multiply( security.getSymbolProperties().getContractMultiplier() );
                final BigDecimal lastTradeProfit = (closedSaleValueInQuoteCurrency.subtract( closedCost )).multiply( conversionFactor );

                //Update Vehicle Profit Tracking:
                holdings.addNewProfit( lastTradeProfit );
                holdings.setLastTradeProfit( lastTradeProfit );
                portfolio.addTransactionRecord( Extensions.convertToUtc( security.getLocalTime(), security.getExchange().getTimeZone() ), lastTradeProfit.subtract( Global.TWO.multiply( feeThisOrder ) ) );
            }

            //UPDATE HOLDINGS QUANTITY, AVG PRICE:
            //Currently NO holdings. The order is ALL our holdings.
            if( quantityHoldings == 0 ) {
                //First transaction just subtract order from cash and set our holdings:
                averageHoldingsPrice = fill.fillPrice;
                quantityHoldings = fill.fillQuantity;
            }
            else if( isLong ) {
                //If we're currently LONG on the stock.
                switch( fill.getDirection() ) {
                    case Buy:
                        //Update the Holding Average Price: Total Value / Total Quantity:
                        averageHoldingsPrice = ((averageHoldingsPrice.multiply( BigDecimal.valueOf( quantityHoldings ) )).add( fill.fillPrice.multiply( BigDecimal.valueOf( fill.fillQuantity ) )) )
                            .divide( BigDecimal.valueOf( quantityHoldings + fill.fillQuantity ) );
                        //Add the new quantity:
                        quantityHoldings += fill.fillQuantity;
                        break;

                    case Sell:
                        quantityHoldings += fill.fillQuantity; //+ a short = a subtraction
                        if( quantityHoldings < 0 ) {
                            //If we've now passed through zero from selling stock: new avg price:
                            averageHoldingsPrice = fill.fillPrice;
                        }
                        else if( quantityHoldings == 0 )
                            averageHoldingsPrice = BigDecimal.ZERO;
                        break;
                    default:
                        break;
                }
            }
            else if( isShort ) {
                //We're currently SHORTING the stock: What is the new position now?
                switch( fill.getDirection() ) {
                    case Buy:
                        //Buying when we're shorting moves to close position:
                        quantityHoldings += fill.fillQuantity;
                        if( quantityHoldings > 0 ) {
                            //If we were short but passed through zero, new average price is what we paid. The short position was closed.
                            averageHoldingsPrice = fill.fillPrice;
                        }
                        else if( quantityHoldings == 0 )
                            averageHoldingsPrice = BigDecimal.ZERO;
                        break;

                    case Sell:
                        //We are increasing a Short position:
                        //E.g.  -100 @ $5, adding -100 @ $10: Avg: $7.5
                        //      dAvg = (-500 + -1000) / -200 = 7.5
//                        averageHoldingsPrice = ((averageHoldingsPrice*quantityHoldings) + (fill.FillQuantity*fill.FillPrice))/(quantityHoldings + fill.FillQuantity);
                        averageHoldingsPrice = ((averageHoldingsPrice.multiply( BigDecimal.valueOf( quantityHoldings ))).add( (fill.fillPrice.multiply( BigDecimal.valueOf( fill.fillQuantity ) ) ) ) )
                            .divide( BigDecimal.valueOf( quantityHoldings + fill.fillQuantity ) );
                        quantityHoldings += fill.fillQuantity;
                        break;
                    default:
                        break;
                }
            }
        }
        catch( Exception err ) {
            log.error( err.getMessage(), err );
        }

        //Set the results back to the vehicle.
        security.getHoldings().setHoldings( averageHoldingsPrice,  quantityHoldings );
    }
}
