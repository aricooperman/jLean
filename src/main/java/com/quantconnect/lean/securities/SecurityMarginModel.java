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
import java.math.RoundingMode;

import com.quantconnect.lean.Extensions;
import com.quantconnect.lean.orders.Order;
import com.quantconnect.lean.orders.OrderTypes.OrderDirection;
import com.quantconnect.lean.orders.OrderTypes.OrderType;
import com.quantconnect.lean.orders.SubmitOrderRequest;

/**
 * Represents a simple, constant margining model by specifying the percentages of required margin.
 */
public class SecurityMarginModel implements ISecurityMarginModel {

    private static final BigDecimal MARGIN_BUFFER = new BigDecimal( "0.10" );
    
    private BigDecimal initialMarginRequirement;
    private BigDecimal maintenanceMarginRequirement;

    /**
     * Initializes a new instance of the <see cref="SecurityMarginModel"/>
     * @param initialMarginRequirement The percentage of an order's absolute cost
     *      that must be held in free cash in order to place the order
     * @param maintenanceMarginRequirement The percentage of the holding's absolute
     *      cost that must be held in free cash in order to avoid a margin call
     */
    public SecurityMarginModel( BigDecimal initialMarginRequirement, BigDecimal maintenanceMarginRequirement ) {
        if( initialMarginRequirement.compareTo( BigDecimal.ZERO ) < 0 || initialMarginRequirement.compareTo( BigDecimal.ONE ) > 0 )
            throw new IllegalArgumentException( "Initial margin requirement must be between 0 and 1" );

        if( maintenanceMarginRequirement.compareTo( BigDecimal.ZERO ) < 0 || maintenanceMarginRequirement.compareTo( BigDecimal.ONE ) > 0 )
            throw new IllegalArgumentException( "Maintenance margin requirement must be between 0 and 1" );

        this.initialMarginRequirement = initialMarginRequirement;
        this.maintenanceMarginRequirement = maintenanceMarginRequirement;
    }

    /**
     * Initializes a new instance of the <see cref="SecurityMarginModel"/>
     * @param leverage The leverage
     */
    public SecurityMarginModel( BigDecimal leverage ) {
        if( leverage.compareTo( BigDecimal.ONE ) < 0 )
            throw new IllegalArgumentException( "Leverage must be greater than or equal to 1.");

        final BigDecimal inverseLeverage = BigDecimal.ONE.divide( leverage, RoundingMode.HALF_UP );
        initialMarginRequirement = inverseLeverage;
        maintenanceMarginRequirement = inverseLeverage;
    }

    /**
     * Gets the current leverage of the security
     * @param security The security to get leverage for
     * @returns The current leverage in the security
     */
    public BigDecimal getLeverage( Security security ) {
        return BigDecimal.ONE.divide( getMaintenanceMarginRequirement( security ), RoundingMode.HALF_UP );
    }

    /**
     * Sets the leverage for the applicable securities, i.e, equities
     * 
     * This is added to maintain backwards compatibility with the old margin/leverage system
     * 
     * @param security">
     * @param leverage The new leverage
     */
    public void setLeverage( Security security, BigDecimal leverage ) {
        if( leverage.compareTo( BigDecimal.ONE ) < 0 )
            throw new IllegalArgumentException( "Leverage must be greater than or equal to 1.");

        final BigDecimal margin = BigDecimal.ONE.divide( leverage, RoundingMode.HALF_EVEN );
        initialMarginRequirement = margin;
        maintenanceMarginRequirement = margin;
    }

    /**
     * Gets the total margin required to execute the specified order in units of the account currency including fees
     * @param security The security to compute initial margin for
     * @param order The order to be executed
     * @returns The total margin in terms of the currency quoted in the order
     */
    public BigDecimal getInitialMarginRequiredForOrder( Security security, Order order ) {
        //Get the order value from the non-abstract order classes (MarketOrder, LimitOrder, StopMarketOrder)
        //Market order is approximated from the current security price and set in the MarketOrder Method in QCAlgorithm.
        final BigDecimal orderFees = security.getFeeModel().getOrderFee( security, order );

        final BigDecimal orderValue = order.getValue( security ).multiply( getInitialMarginRequirement( security ) );
        return orderValue.add( (orderValue.signum() < 0 ? orderFees.negate() : orderFees) );
    }

    /**
     * Gets the margin currently alloted to the specified holding
     * @param security The security to compute maintenance margin for
     * @returns The maintenance margin required for the 
     */
    public BigDecimal getMaintenanceMargin( Security security ) {
        return security.getHoldings().getAbsoluteHoldingsCost().multiply( getMaintenanceMarginRequirement( security ) );
    }

    /**
     * Gets the margin cash available for a trade
     * @param portfolio The algorithm's portfolio
     * @param security The security to be traded
     * @param direction The direction of the trade
     * @returns The margin available for the trade
     */
    public BigDecimal getMarginRemaining( SecurityPortfolioManager portfolio, Security security, OrderDirection direction ) {
        final SecurityHolding holdings = security.getHoldings();

        if( direction == OrderDirection.Hold )
            return portfolio.getMarginRemaining();

        //If the order is in the same direction as holdings, our remaining cash is our cash
        //In the opposite direction, our remaining cash is 2 x current value of assets + our cash
        if( holdings.isLong() ) {
            switch( direction ) {
                case Buy:
                    return portfolio.getMarginRemaining();

                case Sell:
                    return 
                            // portion of margin to close the existing position
                            getMaintenanceMargin( security ).add( 
                                    // portion of margin to open the new position
                                    holdings.getAbsoluteHoldingsValue().multiply( getInitialMarginRequirement( security ) ) )
                            .add( portfolio.getMarginRemaining() );
                default:
                    break;
            }
        }
        else if( holdings.isShort() ) {
            switch( direction ) {
                case Buy:
                    return
                            // portion of margin to close the existing position
                            getMaintenanceMargin( security ).add(
                                    // portion of margin to open the new position
                                    holdings.getAbsoluteHoldingsValue().multiply( getInitialMarginRequirement( security ) ) )
                            .add( portfolio.getMarginRemaining() );

                case Sell:
                    return portfolio.getMarginRemaining();
                default:
                    break;
            }
        }

        //No holdings, return cash
        return portfolio.getMarginRemaining();
    }

    /**
     * Generates a new order for the specified security taking into account the total margin
     * used by the account. Returns null when no margin call is to be issued.
     * @param security The security to generate a margin call order for
     * @param netLiquidationValue The net liquidation value for the entire account
     * @param totalMargin The total margin used by the account in units of base currency
     * @returns An order object representing a liquidation order to be executed to bring the account within margin requirements
     */
    public SubmitOrderRequest generateMarginCallOrder( Security security, BigDecimal netLiquidationValue, BigDecimal totalMargin ) {
        // leave a buffer in default implementation
        if( totalMargin.compareTo( netLiquidationValue.multiply( BigDecimal.ONE.add( MARGIN_BUFFER ) ) ) <= 0 )
            return null;

        final SecurityHolding holdings = security.getHoldings();
        if( !holdings.isInvested() )
            return null;

        if( security.getQuoteCurrency().getConversionRate().signum() == 0 ) {
            // check for div 0 - there's no conv rate, so we can't place an order
            return null;
        }

        // compute the amount of quote currency we need to liquidate in order to get within margin requirements
        final BigDecimal deltaInQuoteCurrency = (totalMargin.subtract( netLiquidationValue )).divide( security.getQuoteCurrency().getConversionRate(), RoundingMode.HALF_EVEN );

        // compute the number of shares required for the order, rounding up
        final BigDecimal unitPriceInQuoteCurrency = security.getPrice().multiply( security.getSymbolProperties().getContractMultiplier() );
        int quantity = deltaInQuoteCurrency.divide( unitPriceInQuoteCurrency, RoundingMode.HALF_UP ).divideToIntegralValue( getMaintenanceMarginRequirement( security ) ).intValue();

        // don't try and liquidate more share than we currently hold, minimum value of 1, maximum value for absolute quantity
        quantity = Math.max( 1, Math.min( holdings.getAbsoluteQuantity(), quantity ) );
        if( holdings.isLong() ) {
            // adjust to a sell for long positions
            quantity *= -1;
        }

        return new SubmitOrderRequest( OrderType.Market, security.getType(), security.getSymbol(), quantity, BigDecimal.ZERO, BigDecimal.ZERO, 
                Extensions.convertToUtc( security.getLocalTime(), security.getExchange().getTimeZone() ), "Margin Call" );
    }

    /**
     * The percentage of an order's absolute cost that must be held in free cash in order to place the order
     */
    protected BigDecimal getInitialMarginRequirement( Security security ) {
        return initialMarginRequirement;
    }

    /**
     * The percentage of the holding's absolute cost that must be held in free cash in order to avoid a margin call
     */
    protected BigDecimal getMaintenanceMarginRequirement( Security security ) {
        return maintenanceMarginRequirement;
    }
}