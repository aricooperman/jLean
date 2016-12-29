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

//using System.Threading.Tasks;

package com.quantconnect.lean.data.market;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.function.Supplier;

import com.quantconnect.lean.DateFormat;
import com.quantconnect.lean.Global;
import com.quantconnect.lean.OptionRight;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.securities.option.OptionPriceModelResult;

import javaslang.Lazy;

/**
 * Defines a single option contract at a specific expiration and strike price
*/
public class OptionContract {
    
    private Lazy<OptionPriceModelResult> optionPriceModelResult = Lazy.of( () -> new OptionPriceModelResult( BigDecimal.ZERO, new FirstOrderGreeks() ) );

    private final Symbol symbol;
    private final Symbol underlyingSymbol;
    private LocalDateTime time;
    private BigDecimal openInterest;
    private BigDecimal lastPrice;
    private BigDecimal bidPrice;
    private long bidSize;
    private BigDecimal askPrice;
    private long askSize;
    private BigDecimal underlyingLastPrice;

    
    
    /**
     * Initializes a new instance of the <see cref="OptionContract"/> class
     * @param symbol The option contract symbol
     * @param underlyingSymbol The symbol of the underlying security
     */
    public OptionContract( final Symbol symbol, final Symbol underlyingSymbol ) {
        this.symbol = symbol;
        this.underlyingSymbol = underlyingSymbol;
    }

    /**
     * Gets the option contract's symbol
     */
    public Symbol getSymbol() {
        return symbol;
    }

    /**
     * Gets the underlying security's symbol
     */
    public Symbol getUnderlyingSymbol() {
        return underlyingSymbol;
    }

    /**
     * Gets the strike price
     */
    public BigDecimal getStrike() {
        return symbol.getId().getStrikePrice();
    }

    /**
     * Gets the expiration date
     */
    public LocalDate getExpiry() {
        return symbol.getId().getDate();
    }

    /**
     * Gets the right being purchased (call [right to buy] or put [right to sell])
     */
    public OptionRight getRight() {
        return symbol.getId().getOptionRight();
    }

    /**
     * Gets the theoretical price of this option contract as computed by the <see cref="IOptionPriceModel"/>
     */
    public BigDecimal getTheoreticalPrice() {
        return optionPriceModelResult.get().getTheoreticalPrice();
    }

    /**
     * Gets the greeks for this contract
     */
    public FirstOrderGreeks getGreeks() {
        return optionPriceModelResult.get().getGreeks();
    }
    
    /**
     * Gets the local date time this contract's data was last updated
     */
    public LocalDateTime getTime() {
        return time;
    }

    public void setTime( final LocalDateTime time ) {
        this.time = time;
    }

    /**
     * Gets the open interest
     */
    public BigDecimal getOpenInterest() {
        return openInterest;
    }

    public void setOpenInterest( final BigDecimal openInterest ) {
        this.openInterest = openInterest;
    }

    /**
     * Gets the last price this contract traded at
     */
    public BigDecimal getLastPrice() {
        return lastPrice;
    }

    public void setLastPrice( final BigDecimal lastPrice ) {
        this.lastPrice = lastPrice;
    }

    /**
     * Gets the current bid price
     */
    public BigDecimal getBidPrice() {
        return bidPrice;
    }

    public void setBidPrice( final BigDecimal bidPrice ) {
        this.bidPrice = bidPrice;
    }

    /**
     * Get the current bid size
     */
    public long getBidSize() {
        return bidSize;
    }

    public void setBidSize( final long bidSize ) {
        this.bidSize = bidSize;
    }

    /**
     * Gets the ask price
     */
    public BigDecimal getAskPrice() {
        return askPrice;
    }

    public void setAskPrice( final BigDecimal askPrice ) {
        this.askPrice = askPrice;
    }

    /**
     * Gets the current ask size
     */
    public long getAskSize() {
        return askSize;
    }

    public void setAskSize( final long askSize ) {
        this.askSize = askSize;
    }

    /**
     * Gets the last price the underlying security traded at
     */
    public BigDecimal getUnderlyingLastPrice() {
        return underlyingLastPrice;
    }

    public void setUnderlyingLastPrice( final BigDecimal underlyingLastPrice ) {
        this.underlyingLastPrice = underlyingLastPrice;
    }

    /**
     * Sets the option price model evaluator function to be used for this contract
     * @param optionPriceModelEvaluator Function delegate used to evaluate the option price model
     */
    public void setOptionPriceModel( final Supplier<OptionPriceModelResult> optionPriceModelEvaluator ) {
        optionPriceModelResult = Lazy.of( optionPriceModelEvaluator );
    }

    /**
     * Returns a String that represents the current object.
     * @returns A String that represents the current object.
     */
    @Override
    public String toString() {
        return String.format( "%1$s%2$s%3$s%4$f", symbol.getId().getSymbol(), getExpiry().format( DateFormat.EightCharacter ), getRight().toString(), getStrike().multiply( Global.ONE_THOUSAND ) );
    }
}
