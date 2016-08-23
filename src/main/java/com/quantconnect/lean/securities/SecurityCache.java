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
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.market.IBar;
import com.quantconnect.lean.data.market.QuoteBar;
import com.quantconnect.lean.data.market.Tick;
import com.quantconnect.lean.data.market.TradeBar;

/**
 *  Base class caching caching spot for security data and any other temporary properties.
 *  This class is virtually unused and will soon be made obsolete. 
 *  This comment made in a remark to prevent obsolete errors in all users algorithms
 */
public class SecurityCache {
    // this is used to prefer quote bar data over the tradebar data
    private LocalDateTime lastQuoteBarUpdate;
    private BaseData lastData;
    
    private final ConcurrentMap<Class<?>,BaseData> dataByType = new ConcurrentHashMap<>();

    /**
     * Gets the most recent price submitted to this cache
     */
    private BigDecimal price;

    /**
     * Gets the most recent open submitted to this cache
     */
    private BigDecimal open;

    /**
     *  Gets the most recent high submitted to this cache
     */
    private BigDecimal high;

    /**
     * Gets the most recent low submitted to this cache
     */
    private BigDecimal low;

    /**
     *  Gets the most recent close submitted to this cache
     */
    private BigDecimal close;

    /**
     *  Gets the most recent bid submitted to this cache
     */
    private BigDecimal bidPrice;

    /**
     * Gets the most recent ask submitted to this cache
     */
    private BigDecimal askPrice;

    /**
     * Gets the most recent bid size submitted to this cache
     */
    private long bidSize;

    /**
     * Gets the most recent ask size submitted to this cache
     */
    private long askSize;

    private long volume;
    
    
    public BigDecimal getPrice() {
        return price;
    }

    public BigDecimal getOpen() {
        return open;
    }

    public BigDecimal getHigh() {
        return high;
    }

    public BigDecimal getLow() {
        return low;
    }

    public BigDecimal getClose() {
        return close;
    }

    public BigDecimal getBidPrice() {
        return bidPrice;
    }

    public BigDecimal getAskPrice() {
        return askPrice;
    }

    public long getBidSize() {
        return bidSize;
    }

    public long getAskSize() {
        return askSize;
    }

    /**
     * Gets the most recent volume submitted to this cache
     */
    public long getVolume() {
        return volume;
    }

    /**
     *  Add a new market data point to the local security cache for the current market price.
     */
    public void addData( BaseData data ) {
        lastData = data;
        dataByType.put( data.getClass(), data );

        if( data instanceof Tick ) {
            final Tick tick = (Tick)data;
            if( tick.getValue().signum() != 0 ) price = tick.getValue();

            if( tick.bidPrice.signum() != 0 ) bidPrice = tick.bidPrice;
            if( tick.bidSize != 0 ) bidSize = tick.bidSize;

            if( tick.askPrice.signum() != 0 ) askPrice = tick.askPrice;
            if( tick.askSize != 0 ) askSize = tick.askSize;
        }
        if( data instanceof IBar ) {
            final IBar bar = (IBar)data;
            if( !lastQuoteBarUpdate.equals( data.getEndTime() ) ) {
                if( bar.getOpen().signum() != 0 ) open = bar.getOpen();
                if( bar.getHigh().signum() != 0 ) high = bar.getHigh();
                if( bar.getLow().signum() != 0 ) low = bar.getLow();
                if( bar.getClose().signum() != 0 ) {
                    price = bar.getClose();
                    close = bar.getClose();
                }
            }

            if( bar instanceof TradeBar ) {
                final TradeBar tradeBar = (TradeBar)bar;
                if( tradeBar.getVolume() != 0 ) volume = tradeBar.getVolume();
            }
            if( bar instanceof QuoteBar ) {
                final QuoteBar quoteBar = (QuoteBar)bar;
                lastQuoteBarUpdate = quoteBar.getEndTime();
                if( quoteBar.getAsk() != null && quoteBar.getAsk().getClose().signum() != 0 ) askPrice = quoteBar.getAsk().getClose();
                if( quoteBar.getBid() != null && quoteBar.getBid().getClose().signum() != 0 ) bidPrice = quoteBar.getBid().getClose();
                if( quoteBar.getLastBidSize() != 0 ) bidSize = quoteBar.getLastBidSize();
                if( quoteBar.getLastAskSize() != 0 ) askSize = quoteBar.getLastAskSize();
            }
        }
        else
            price = data.getPrice();
    }

    /**
     * Get last data packet recieved for this security
     * @returns BaseData type of the security
     */
    public BaseData getData() {
        return lastData;
    }

    /**
     * Get last data packet recieved for this security of the specified ty[e
     * @returns The last data packet, null if none received of type
     */
    @SuppressWarnings("unchecked")
    public <T extends BaseData> T getData( Class<T> clazz ) {
        BaseData data = dataByType.get( clazz );
        return (T)data;
    }

    /**
     *  Reset cache storage and free memory
     */
    public void reset() {
        dataByType.clear();
    }
}