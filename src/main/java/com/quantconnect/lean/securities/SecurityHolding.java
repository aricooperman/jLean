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

import com.quantconnect.lean.Global.SecurityType;
import com.quantconnect.lean.Symbol;

/**
 * SecurityHolding is a base class for purchasing and holding a market item which manages the asset portfolio
 */
public class SecurityHolding {
    
    //Working Variables
    private BigDecimal _averagePrice;
    private int        _quantity;
    private BigDecimal _price;
    private BigDecimal _totalSaleVolume;
    private BigDecimal _profit;
    private BigDecimal _lastTradeProfit;
    private BigDecimal _totalFees;
    
    private final Security _security;

    /**
     * Create a new holding class instance setting the initial properties to $0.
     * @param security">The security being held
     */
    public SecurityHolding( Security security ) {
        _security = security;
        //Total Sales Volume for the day
        _totalSaleVolume = BigDecimal.ZERO;
        _lastTradeProfit = BigDecimal.ZERO;
    }

    /**
     * Average price of the security holdings.
     */
    public BigDecimal getAveragePrice() {
        return _averagePrice;
    }

    /**
     * Quantity of the security held.
     * Positive indicates long holdings, negative quantity indicates a short holding
    /// <seealso cref="AbsoluteQuantity"/>
     */
    public BigDecimal getQuantity() {
        return BigDecimal.valueOf( _quantity );
    }

    /**
     * Symbol identifier of the underlying security.
     */
    public Symbol getSymbol() {
        return _security.getSymbol();
    }

    /**
     * The security type of the symbol
     */
    public SecurityType getType() {
        return _security.getType();
    }

    /**
     * Leverage of the underlying security.
     */
    public BigDecimal getLeverage() {
        return _security.MarginModel.getLeverage( _security );
    }
    

    /**
     * Acquisition cost of the security total holdings.
     */
    public BigDecimal getHoldingsCost() {
        return _averagePrice.multiply( BigDecimal.valueOf( _quantity ) ).multiply( _security.QuoteCurrency.ConversionRate ).multiply( _security.SymbolProperties.ContractMultiplier );
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
        return _price;
    }

    /**
     * Absolute holdings cost for current holdings in units of the account's currency
    /// <seealso cref="HoldingsCost"/>
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
         return _price.multiply( BigDecimal.valueOf( _quantity ) ).multiply( _security.QuoteCurrency.ConversionRate ).multiply( _security.SymbolProperties.ContractMultiplier );
    }

    /**
     * Absolute of the market value of our holdings.
    /// <seealso cref="HoldingsValue"/>
     */
    public BigDecimal getAbsoluteHoldingsValue() {
         return getHoldingsValue().abs();
    }

    /**
     * Boolean flat indicating if we hold any of the security
     */
    public boolean isHoldStock() {
        return getAbsoluteQuantity().signum() > 0;
    }

    /**
     * Boolean flat indicating if we hold any of the security
     * Alias of HoldStock
    /// <seealso cref="HoldStock"/>
     */
    public boolean isInvested() {
        return isHoldStock();
    }

    /**
     *  The total transaction volume for this security since the algorithm started.
     */
    public BigDecimal getTotalSaleVolume() {
        return _totalSaleVolume;
    }

    /**
     * Total fees for this company since the algorithm started.
     */
    public BigDecimal getTotalFees() {
        return _totalFees;
    }

    /**
     * Boolean flag indicating we have a net positive holding of the security.
    /// <seealso cref="IsShort"/>
     */
    public boolean isLong() {
        return _quantity > 0;
    }

    /**
     * Boolean flag indicating we have a net negative holding of the security.
    /// <seealso cref="IsLong"/>
     */
    public boolean isShort() {
        return _quantity < 0;
    }

    /**
     * Absolute quantity of holdings of this security
    /// <seealso cref="Quantity"/>
     */
    public int getAbsoluteQuantity() {
        return Math.abs( _quantity );
    }

    /**
     * Record of the closing profit from the last trade conducted.
     */
    public BigDecimal getLastTradeProfit() {
        return _lastTradeProfit;
    }

    /**
     * Calculate the total profit for this security.
    /// <seealso cref="NetProfit"/>
     */
    public BigDecimal getProfit() {
         return _profit; 
    }

    /**
     * Return the net for this company measured by the profit less fees.
    /// <seealso cref="Profit"/>
    /// <seealso cref="TotalFees"/>
     */
    public BigDecimal getNetProfit() {
        return _profit.subtract( _totalFees );
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
        _totalFees = _totalFees.add( newFee );
    }

    /**
     *  Adds a profit record to the running total of profit.
     * @param profitLoss The cash change in portfolio from closing a position
     */
    public void addNewProfit( BigDecimal profitLoss ) {
        _profit = _profit.add( profitLoss );
    }

    /**
     * Adds a new sale value to the running total trading volume in terms of the account currency
     * @param saleValue">
     */
    public void addNewSale( BigDecimal saleValue ) {
        _totalSaleVolume = _totalSaleVolume.add( saleValue );
    }

    /**
     * Set the last trade profit for this security from a Portfolio.ProcessFill call.
     * @param lastTradeProfit Value of the last trade profit
     */
    public void setLastTradeProfit( BigDecimal lastTradeProfit ) {
        _lastTradeProfit = lastTradeProfit;
    }
        
    /**
     * Set the quantity of holdings and their average price after processing a portfolio fill.
     */
    public void setHoldings( BigDecimal averagePrice, int quantity ) {
        _averagePrice = averagePrice;
        _quantity = quantity;
    }

    /**
     * Update local copy of closing price value.
     * @param closingPrice Price of the underlying asset to be used for calculating market price / portfolio value
     */
    public void updateMarketPrice( BigDecimal closingPrice ) {
        _price = closingPrice;
    }

    /**
     * Profit if we closed the holdings right now including the approximate fees.
     * Does not use the transaction model for market fills but should.
     */
    public BigDecimal getTotalCloseProfit() {
        if( getAbsoluteQuantity() == 0 )
            return BigDecimal.ZERO;

        // this is in the account currency
        marketOrder = new MarketOrder( _security.Symbol, -_quantity, _security.LocalTime.ConvertToUtc( _security.getExchange().TimeZone ) );
        orderFee = _security.FeeModel.GetOrderFee( _security, marketOrder );

        return (_price.subtract( _averagePrice )).multiply( BigDecimal.valueOf( _quantity ) ).multiply( _security.QuoteCurrency.ConversionRate ).multiply( _security.SymbolProperties.ContractMultiplier ).subtract( orderFee );
    }
}
