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

package com.quantconnect.lean.statistics;

import java.math.BigDecimal;
import java.time.Duration;
import java.time.LocalDateTime;

import com.quantconnect.lean.Symbol;

/**
 * Represents a closed trade
*/
public class Trade {
    
    /**
     * The symbol of the traded instrument
     */
    private Symbol symbol;

    /**
     * The date and time the trade was opened
    */
    private LocalDateTime entryTime;

    /**
     * The price at which the trade was opened (or the average price if multiple entries)
    */
    private BigDecimal entryPrice;

    /**
     * The direction of the trade (Long or Short)
    */
    private TradeDirection direction;

    /**
     * The total unsigned quantity of the trade
    */
    private int quantity;

    /**
     * The date and time the trade was closed
    */
    private LocalDateTime exitTime;

    /**
     * The price at which the trade was closed (or the average price if multiple exits)
    */
    private BigDecimal exitPrice;

    /**
     * The gross profit/loss of the trade (as symbol currency)
    */
    private BigDecimal profitLoss;

    /**
     * The total fees associated with the trade (always positive value) (as symbol currency)
    */
    private BigDecimal totalFees;

    /**
     * The Maximum Adverse Excursion (as symbol currency)
    */
    private BigDecimal maxAdvExcursion;

    /**
     * The Maximum Favorable Excursion (as symbol currency)
    */
    private BigDecimal maxFavExcursion;

    /**
     * Returns the duration of the trade
    */
    public Duration getDuration() {
        return Duration.between( entryTime, exitTime );
    }

    /**
     * Returns the amount of profit given back before the trade was closed
     */
    public BigDecimal getEndTradeDrawdown() {
        return profitLoss.subtract( maxFavExcursion );
    }

    public Symbol getSymbol() {
        return symbol;
    }

    public void setSymbol( Symbol symbol ) {
        this.symbol = symbol;
    }

    public LocalDateTime getEntryTime() {
        return entryTime;
    }

    public void setEntryTime( LocalDateTime entryTime ) {
        this.entryTime = entryTime;
    }

    public BigDecimal getEntryPrice() {
        return entryPrice;
    }

    public void setEntryPrice( BigDecimal entryPrice ) {
        this.entryPrice = entryPrice;
    }

    public TradeDirection getDirection() {
        return direction;
    }

    public void setDirection( TradeDirection direction ) {
        this.direction = direction;
    }

    public int getQuantity() {
        return quantity;
    }

    public void setQuantity( int quantity ) {
        this.quantity = quantity;
    }

    public LocalDateTime getExitTime() {
        return exitTime;
    }

    public void setExitTime( LocalDateTime exitTime ) {
        this.exitTime = exitTime;
    }

    public BigDecimal getExitPrice() {
        return exitPrice;
    }

    public void setExitPrice( BigDecimal exitPrice ) {
        this.exitPrice = exitPrice;
    }

    public BigDecimal getProfitLoss() {
        return profitLoss;
    }

    public void setProfitLoss( BigDecimal profitLoss ) {
        this.profitLoss = profitLoss;
    }

    public BigDecimal getTotalFees() {
        return totalFees;
    }

    public void setTotalFees( BigDecimal totalFees ) {
        this.totalFees = totalFees;
    }

    public BigDecimal getMaxAdvExcursion() {
        return maxAdvExcursion;
    }

    public void setMaxAdvExcursion( BigDecimal maxAdvExcursion ) {
        this.maxAdvExcursion = maxAdvExcursion;
    }

    public BigDecimal getMaxFavExcursion() {
        return maxFavExcursion;
    }

    public void setMaxFavExcursion( BigDecimal maxFavExcursion ) {
        this.maxFavExcursion = maxFavExcursion;
    }
}
