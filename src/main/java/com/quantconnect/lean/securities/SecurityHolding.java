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
import com.quantconnect.lean.SecurityType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.orders.MarketOrder;

/**
 * SecurityHolding is a base class for purchasing and holding a market item which manages the asset portfolio
 */
public class SecurityHolding {
    
    private final Security security;
    
    //Working Variables
    private BigDecimal averagePrice;
    private int        quantity;
    private BigDecimal price;
    private BigDecimal totalSaleVolume;
    private BigDecimal profit;
    private BigDecimal lastTradeProfit;
    private BigDecimal totalFees;

    /**
     * Create a new holding class instance setting the initial properties to $0.
     * @param security The security being held
     */
    public SecurityHolding( Security security ) {
        this.security = security;
        //Total Sales Volume for the day
        this.totalSaleVolume = BigDecimal.ZERO;
        this.lastTradeProfit = BigDecimal.ZERO;
    }

    /**
     * Average price of the security holdings.
     */
    public BigDecimal getAveragePrice() {
        return averagePrice;
    }

    /**
     * Quantity of the security held.
     * Positive indicates long holdings, negative quantity indicates a short holding
     * <seealso cref="AbsoluteQuantity"/>
     */
    public int getQuantity() {
        return quantity;
    }

    /**
     * Symbol identifier of the underlying security.
     */
    public Symbol getSymbol() {
        return security.getSymbol();
    }

    /**
     * The security type of the symbol
     */
    public SecurityType getType() {
        return security.getType();
    }

    /**
     * Leverage of the underlying security.
     */
    public BigDecimal getLeverage() {
        return security.getMarginModel().getLeverage( security );
    }
    

    /**
     * Acquisition cost of the security total holdings.
     */
    public BigDecimal getHoldingsCost() {
        return averagePrice.multiply( BigDecimal.valueOf( quantity ) ).multiply( security.getQuoteCurrency().getConversionRate() ).multiply( security.getSymbolProperties().getContractMultiplier() );
    }

    /**
     * Unlevered Acquisition cost of the security total holdings.
     */
    public BigDecimal getUnleveredHoldingsCost() {
        return getHoldingsCost().divide( getLeverage(), RoundingMode.HALF_UP );
    }

    /**
     * Current market price of the security.
     */
    public BigDecimal getPrice() {
        return price;
    }

    /**
     * Absolute holdings cost for current holdings in units of the account's currency
     * <seealso cref="HoldingsCost"/>
     */
    public BigDecimal getAbsoluteHoldingsCost() {
        return getHoldingsCost().abs();
    }

    /**
     * Unlevered absolute acquisition cost of the security total holdings.
     */
    public BigDecimal getUnleveredAbsoluteHoldingsCost() {
        return getUnleveredHoldingsCost().abs();
    }

    /**
     * Market value of our holdings.
     */
    public BigDecimal getHoldingsValue() {
         return price.multiply( BigDecimal.valueOf( quantity ) ).multiply( security.getQuoteCurrency().getConversionRate() ).multiply( security.getSymbolProperties().getContractMultiplier() );
    }

    /**
     * Absolute of the market value of our holdings.
     * <seealso cref="HoldingsValue"/>
     */
    public BigDecimal getAbsoluteHoldingsValue() {
         return getHoldingsValue().abs();
    }

    /**
     * Boolean flat indicating if we hold any of the security
     */
    public boolean isHoldStock() {
        return getAbsoluteQuantity() > 0;
    }

    /**
     * Boolean flat indicating if we hold any of the security
     * Alias of HoldStock
     * <seealso cref="HoldStock"/>
     */
    public boolean isInvested() {
        return isHoldStock();
    }

    /**
     *  The total transaction volume for this security since the algorithm started.
     */
    public BigDecimal getTotalSaleVolume() {
        return totalSaleVolume;
    }

    /**
     * Total fees for this company since the algorithm started.
     */
    public BigDecimal getTotalFees() {
        return totalFees;
    }

    /**
     * Boolean flag indicating we have a net positive holding of the security.
     * <seealso cref="IsShort"/>
     */
    public boolean isLong() {
        return quantity > 0;
    }

    /**
     * Boolean flag indicating we have a net negative holding of the security.
     * <seealso cref="IsLong"/>
     */
    public boolean isShort() {
        return quantity < 0;
    }

    /**
     * Absolute quantity of holdings of this security
     * <seealso cref="Quantity"/>
     */
    public int getAbsoluteQuantity() {
        return Math.abs( quantity );
    }

    /**
     * Record of the closing profit from the last trade conducted.
     */
    public BigDecimal getLastTradeProfit() {
        return lastTradeProfit;
    }

    /**
     * Calculate the total profit for this security.
     * <seealso cref="NetProfit"/>
     */
    public BigDecimal getProfit() {
         return profit; 
    }

    /**
     * Return the net for this company measured by the profit less fees.
     * <seealso cref="Profit"/>
     * <seealso cref="TotalFees"/>
     */
    public BigDecimal getNetProfit() {
        return profit.subtract( totalFees );
    }

    /**
     *  Gets the unrealized profit as a percenage of holdings cost
     */
    public BigDecimal getUnrealizedProfitPercent() {
        final BigDecimal absoluteHoldingsCost = getAbsoluteHoldingsCost();
        if( absoluteHoldingsCost.signum() == 0 ) 
            return BigDecimal.ZERO;
        return getUnrealizedProfitPercent().divide( absoluteHoldingsCost, RoundingMode.HALF_UP );
    }

    /**
     *  Unrealized profit of this security when absolute quantity held is more than zero.
     */
    public BigDecimal getUnrealizedProfit() {
        return getTotalCloseProfit();
    }

    /**
     * Adds a fee to the running total of total fees.
     * @param newFee
     */
    public void addNewFee( BigDecimal newFee ) {
        totalFees = totalFees.add( newFee );
    }

    /**
     *  Adds a profit record to the running total of profit.
     * @param profitLoss The cash change in portfolio from closing a position
     */
    public void addNewProfit( BigDecimal profitLoss ) {
        profit = profit.add( profitLoss );
    }

    /**
     * Adds a new sale value to the running total trading volume in terms of the account currency
     * @param saleValue">
     */
    public void addNewSale( BigDecimal saleValue ) {
        totalSaleVolume = totalSaleVolume.add( saleValue );
    }

    /**
     * Set the last trade profit for this security from a Portfolio.ProcessFill call.
     * @param lastTradeProfit Value of the last trade profit
     */
    public void setLastTradeProfit( BigDecimal lastTradeProfit ) {
        this.lastTradeProfit = lastTradeProfit;
    }
        
    /**
     * Set the quantity of holdings and their average price after processing a portfolio fill.
     */
    public void setHoldings( BigDecimal averagePrice, int quantity ) {
        this.averagePrice = averagePrice;
        this.quantity = quantity;
    }

    /**
     * Update local copy of closing price value.
     * @param closingPrice Price of the underlying asset to be used for calculating market price / portfolio value
     */
    public void updateMarketPrice( BigDecimal closingPrice ) {
        price = closingPrice;
    }

    /**
     * Profit if we closed the holdings right now including the approximate fees.
     * Does not use the transaction model for market fills but should.
     */
    public BigDecimal getTotalCloseProfit() {
        if( getAbsoluteQuantity() == 0 )
            return BigDecimal.ZERO;

        // this is in the account currency
        final MarketOrder marketOrder = new MarketOrder( security.getSymbol(), -quantity, Extensions.convertToUtc( security.getLocalTime(), security.getExchange().getTimeZone() ) );
        final BigDecimal orderFee = security.getFeeModel().getOrderFee( security, marketOrder );

        return (price.subtract( averagePrice ))
                    .multiply( BigDecimal.valueOf( quantity ) )
                    .multiply( security.getQuoteCurrency().getConversionRate() )
                    .multiply( security.getSymbolProperties().getContractMultiplier() )
                    .subtract( orderFee );
    }
}
