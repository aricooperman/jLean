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

package com.quantconnect.lean.orders.fills;

import java.math.BigDecimal;
import java.time.LocalDateTime;

import com.quantconnect.lean.Extensions;
import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.market.Bar;
import com.quantconnect.lean.data.market.IBar;
import com.quantconnect.lean.data.market.QuoteBar;
import com.quantconnect.lean.data.market.Tick;
import com.quantconnect.lean.data.market.TradeBar;
import com.quantconnect.lean.orders.LimitOrder;
import com.quantconnect.lean.orders.MarketOnCloseOrder;
import com.quantconnect.lean.orders.MarketOnOpenOrder;
import com.quantconnect.lean.orders.MarketOrder;
import com.quantconnect.lean.orders.OrderEvent;
import com.quantconnect.lean.orders.OrderTypes.OrderDirection;
import com.quantconnect.lean.orders.OrderTypes.OrderStatus;
import com.quantconnect.lean.orders.StopLimitOrder;
import com.quantconnect.lean.orders.StopMarketOrder;
import com.quantconnect.lean.securities.Security;

/**
 * Represents the default fill model used to simulate order fills
 */
public class ImmediateFillModel implements IFillModel {
    
    /**
     * Default market fill model for the base security class. Fills at the last traded price.
     * <seealso cref="SecurityTransactionModel.StopMarketFill"/>
     * <seealso cref="SecurityTransactionModel.LimitFill"/>
     * @param asset Security asset we're filling
     * @param order Order packet to model
     * @returns Order fill information detailing the average price and quantity filled.
     */
    public OrderEvent marketFill( Security asset, MarketOrder order ) {
        //Default order event to return.
        final LocalDateTime utcTime = Extensions.convertToUtc( asset.getLocalTime(), asset.getExchange().getTimeZone() );
        final OrderEvent fill = new OrderEvent( order, utcTime, BigDecimal.ZERO );

        if( order.getStatus() == OrderStatus.Canceled ) 
            return fill;

        // make sure the exchange is open before filling
        if( !isExchangeOpen( asset ) ) 
            return fill;

        //Order [fill]price for a market order model is the current security price
        fill.fillPrice = getPrices( asset, order.getDirection() ).current;
        fill.status = OrderStatus.Filled;

        //Calculate the model slippage: e.g. 0.01c
        final BigDecimal slip = asset.getSlippageModel().getSlippageApproximation( asset, order );

        //Apply slippage
        switch( order.getDirection() ) {
            case Buy:
                fill.fillPrice = fill.fillPrice.add( slip );
                break;
            case Sell:
                fill.fillPrice = fill.fillPrice.subtract( slip );
                break;
            default:
                break;
        }

        // assume the order completely filled
        if( fill.status == OrderStatus.Filled ) {
            fill.fillQuantity = order.getQuantity();
            fill.orderFee = asset.getFeeModel().getOrderFee( asset, order );
        }

        return fill;
    }

    /**
     * Default stop fill model implementation in base class security. (Stop Market Order Type)
     * <seealso cref="MarketFill(Security, MarketOrder)"/>
     * <seealso cref="SecurityTransactionModel.LimitFill"/>
     * @param asset Security asset we're filling
     * @param order Order packet to model
     * @returns Order fill information detailing the average price and quantity filled.
     */
    public OrderEvent stopMarketFill( Security asset, StopMarketOrder order ) {
        //Default order event to return.
        final LocalDateTime utcTime = Extensions.convertToUtc( asset.getLocalTime(), asset.getExchange().getTimeZone() );
        final OrderEvent fill = new OrderEvent( order, utcTime, BigDecimal.ZERO );

        // make sure the exchange is open before filling
        if( !isExchangeOpen( asset ) ) 
            return fill;

        //If its cancelled don't need anymore checks:
        if( order.getStatus() == OrderStatus.Canceled ) 
            return fill;

        //Get the range of prices in the last bar:
        final Prices prices = getPrices( asset, order.getDirection() );

        //Calculate the model slippage: e.g. 0.01c
        final BigDecimal slip = asset.getSlippageModel().getSlippageApproximation( asset, order );

        //Check if the Stop Order was filled: opposite to a limit order
        switch( order.getDirection() ) {
            case Sell:
                //-> 1.1 Sell Stop: If Price below setpoint, Sell:
                if( prices.low.compareTo( order.getStopPrice() ) < 0 ) {
                    fill.status = OrderStatus.Filled;
                    // Assuming worse case scenario fill - fill at lowest of the stop & asset price.
                    fill.fillPrice = order.getStopPrice().min( prices.current.subtract( slip ) ); 
                }
                break;

            case Buy:
                //-> 1.2 Buy Stop: If Price Above Setpoint, Buy:
                if( prices.high.compareTo( order.getStopPrice() ) > 0 ) {
                    fill.status = OrderStatus.Filled;
                    // Assuming worse case scenario fill - fill at highest of the stop & asset price.
                    fill.fillPrice = order.getStopPrice().max( prices.current.add( slip ) );
                }
                break;
            default:
                break;
        }

        // assume the order completely filled
        if( fill.status == OrderStatus.Filled ) {
            fill.fillQuantity = order.getQuantity();
            fill.orderFee = asset.getFeeModel().getOrderFee( asset, order );
        }

        return fill;
    }

    /**
     * Default stop limit fill model implementation in base class security. (Stop Limit Order Type)
     * <seealso cref="StopMarketFill(Security, StopMarketOrder)"/>
     * <seealso cref="SecurityTransactionModel.LimitFill"/>
     * 
     *     There is no good way to model limit orders with OHLC because we never know whether the market has 
     *     gapped past our fill price. We have to make the assumption of a fluid, high volume market.
     * 
     *     Stop limit orders we also can't be sure of the order of the H - L values for the limit fill. The assumption
     *     was made the limit fill will be done with closing price of the bar after the stop has been triggered..
     * 
     * @param asset Security asset we're filling
     * @param order Order packet to model
     * @returns Order fill information detailing the average price and quantity filled.
  `  */
    public OrderEvent stopLimitFill( Security asset, StopLimitOrder order ) {
        //Default order event to return.
        final LocalDateTime utcTime = Extensions.convertToUtc( asset.getLocalTime(), asset.getExchange().getTimeZone() );
        final OrderEvent fill = new OrderEvent( order, utcTime, BigDecimal.ZERO );

        //If its cancelled don't need anymore checks:
        if( order.getStatus() == OrderStatus.Canceled ) 
            return fill;

        //Get the range of prices in the last bar:
        final Prices prices = getPrices( asset, order.getDirection() );

        //Check if the Stop Order was filled: opposite to a limit order
        switch( order.getDirection() ) {
            case Buy:
                //-> 1.2 Buy Stop: If Price Above Setpoint, Buy:
                if( prices.high.compareTo( order.getStopPrice() ) > 0 || order.isStopTriggered() ) {
                    order.setStopTriggered( true );

                    // Fill the limit order, using closing price of bar:
                    // Note > Can't use minimum price, because no way to be sure minimum wasn't before the stop triggered.
                    if( asset.getPrice().compareTo( order.getLimitPrice() ) < 0 ) {
                        fill.status = OrderStatus.Filled;
                        fill.fillPrice = order.getLimitPrice();
                    }
                }
                break;

            case Sell:
                //-> 1.1 Sell Stop: If Price below setpoint, Sell:
                if( prices.low.compareTo( order.getStopPrice() ) < 0 || order.isStopTriggered() ) {
                    order.setStopTriggered( true );

                    // Fill the limit order, using minimum price of the bar
                    // Note > Can't use minimum price, because no way to be sure minimum wasn't before the stop triggered.
                    if( asset.getPrice().compareTo( order.getLimitPrice() ) > 0 ) {
                        fill.status = OrderStatus.Filled;
                        fill.fillPrice = order.getLimitPrice(); // Fill at limit price not asset price.
                    }
                }
                break;
            default:
                break;
        }

        // assume the order completely filled
        if( fill.status == OrderStatus.Filled ) {
            fill.fillQuantity = order.getQuantity();
            fill.orderFee = asset.getFeeModel().getOrderFee( asset, order );
        }

        return fill;
    }

    /**
     * Default limit order fill model in the base security class.
     * <seealso cref="StopMarketFill(Security, StopMarketOrder)"/>
     * <seealso cref="MarketFill(Security, MarketOrder)"/>
     * @param asset Security asset we're filling
     * @param order Order packet to model
     * @returns Order fill information detailing the average price and quantity filled.
     */
    public OrderEvent limitFill( Security asset, LimitOrder order ) {
        //Initialize;
        final LocalDateTime utcTime = Extensions.convertToUtc( asset.getLocalTime(), asset.getExchange().getTimeZone() );
        final OrderEvent fill = new OrderEvent( order, utcTime, BigDecimal.ZERO );

        //If its cancelled don't need anymore checks:
        if( order.getStatus() == OrderStatus.Canceled ) 
            return fill;

        //Get the range of prices in the last bar:
        final Prices prices = getPrices( asset, order.getDirection() );

        //-> Valid Live/Model Order: 
        switch( order.getDirection() ) {
            case Buy:
                //Buy limit seeks lowest price
                if( prices.low.compareTo( order.getLimitPrice() ) < 0 ) {
                    //Set order fill:
                    fill.status = OrderStatus.Filled;
                    // fill at the worse price this bar or the limit price, this allows far out of the money limits
                    // to be executed properly
                    fill.fillPrice = prices.high.min( order.getLimitPrice() );
                }
                break;
            case Sell:
                //Sell limit seeks highest price possible
                if( prices.high.compareTo( order.getLimitPrice() ) > 0 ) {
                    fill.status = OrderStatus.Filled;
                    // fill at the worse price this bar or the limit price, this allows far out of the money limits
                    // to be executed properly
                    fill.fillPrice = prices.low.max( order.getLimitPrice() );
                }
                break;
            default:
                break;
        }

        // assume the order completely filled
        if( fill.status == OrderStatus.Filled ) {
            fill.fillQuantity = order.getQuantity();
            fill.orderFee = asset.getFeeModel().getOrderFee( asset, order );
        }

        return fill;
    }

    /**
     * Market on Open Fill Model. Return an order event with the fill details
     * @param asset Asset we're trading with this order
     * @param order Order to be filled
     * @returns Order fill information detailing the average price and quantity filled.
     */
    public OrderEvent marketOnOpenFill( Security asset, MarketOnOpenOrder order ) {
        final LocalDateTime utcTime = Extensions.convertToUtc( asset.getLocalTime(), asset.getExchange().getTimeZone() );
        final OrderEvent fill = new OrderEvent( order, utcTime, BigDecimal.ZERO );

        if( order.getStatus() == OrderStatus.Canceled ) 
            return fill;

        // MOO should never fill on the same bar or on stale data
        // Imagine the case where we have a thinly traded equity, ASUR, and another liquid
        // equity, say SPY, SPY gets data every minute but ASUR, if not on fill forward, maybe
        // have large gaps, in which case the currentBar.EndTime will be in the past
        // ASUR  | | |      [order]        | | | | | | |
        //  SPY  | | | | | | | | | | | | | | | | | | | |
        final BaseData currentBar = asset.getLastData();
        final LocalDateTime localOrderTime = Extensions.convertFromUtc( order.getTime(), asset.getExchange().getTimeZone() );
        if( currentBar == null || localOrderTime.compareTo( currentBar.getEndTime() ) >= 0 )
            return fill;

        // if the MOO was submitted during market the previous day, wait for a day to turn over
        if( asset.getExchange().dateTimeIsOpen( localOrderTime ) && localOrderTime.toLocalDate().equals( asset.getLocalTime().toLocalDate() ) )
            return fill;

        // wait until market open
        // make sure the exchange is open before filling
        if( !isExchangeOpen( asset ) ) 
            return fill;

        fill.fillPrice = getPrices( asset, order.getDirection()).open;
        fill.status = OrderStatus.Filled;

        //Calculate the model slippage: e.g. 0.01c
        final BigDecimal slip = asset.getSlippageModel().getSlippageApproximation( asset, order );

        //Apply slippage
        switch( order.getDirection() ) {
            case Buy:
                fill.fillPrice = fill.fillPrice.add( slip );
                break;
            case Sell:
                fill.fillPrice = fill.fillPrice.subtract( slip );
                break;
            default:
                break;
        }

        // assume the order completely filled
        if( fill.status == OrderStatus.Filled ) {
            fill.fillQuantity = order.getQuantity();
            fill.orderFee = asset.getFeeModel().getOrderFee( asset, order );
        }

        return fill;
    }

    /**
     * Market on Close Fill Model. Return an order event with the fill details
     * @param asset Asset we're trading with this order
     * @param order Order to be filled
     * @returns Order fill information detailing the average price and quantity filled.
     */
    public OrderEvent marketOnCloseFill( Security asset, MarketOnCloseOrder order ) {
        final LocalDateTime utcTime = Extensions.convertToUtc( asset.getLocalTime(), asset.getExchange().getTimeZone() );
        final OrderEvent fill = new OrderEvent( order, utcTime, BigDecimal.ZERO );

        if( order.getStatus() == OrderStatus.Canceled ) 
            return fill;

        final LocalDateTime localOrderTime = Extensions.convertFromUtc( order.getTime(), asset.getExchange().getTimeZone() );
        LocalDateTime nextMarketClose = asset.getExchange().getHours().getNextMarketClose( localOrderTime, false );
            
        // wait until market closes after the order time 
        if( asset.getLocalTime().compareTo( nextMarketClose ) < 0 )
            return fill;

        fill.fillPrice = getPrices( asset, order.getDirection() ).close;
        fill.status = OrderStatus.Filled;

        //Calculate the model slippage: e.g. 0.01c
        final BigDecimal slip = asset.getSlippageModel().getSlippageApproximation( asset, order );

        //Apply slippage
        switch( order.getDirection() ) {
            case Buy:
                fill.fillPrice = fill.fillPrice.add( slip );
                break;
            case Sell:
                fill.fillPrice = fill.fillPrice.subtract( slip );
                break;
            default:
                break;
        }

        // assume the order completely filled
        if( fill.status == OrderStatus.Filled ) {
            fill.fillQuantity = order.getQuantity();
            fill.orderFee = asset.getFeeModel().getOrderFee( asset, order );
        }

        return fill;
    }

    /**
     * Get the minimum and maximum price for this security in the last bar:
     * @param asset Security asset we're checking
     * @param direction The order direction, decides whether to pick bid or ask
     */
    private Prices getPrices( Security asset, OrderDirection direction ) {
        final BigDecimal low = asset.getLow();
        final BigDecimal high = asset.getHigh();
        final BigDecimal open = asset.getOpen();
        final BigDecimal close = asset.getClose();
        final BigDecimal current = asset.getPrice();

        if( direction == OrderDirection.Hold )
            return new Prices( current, open, high, low, close );

        final Tick tick = asset.getCache().getData( Tick.class );
        if( tick != null )
            return new Prices( current, open, high, low, close );

        final QuoteBar quoteBar = asset.getCache().getData( QuoteBar.class );
        if( quoteBar != null ) {
            final Bar bar = direction == OrderDirection.Sell ? quoteBar.getBid() : quoteBar.getAsk();
            if( bar != null )
                return new Prices( bar );
        }

        final TradeBar tradeBar = asset.getCache().getData( TradeBar.class );
        if( tradeBar != null )
            return new Prices( tradeBar );

        final BaseData lastData = asset.getLastData();
        if( lastData instanceof IBar )
            return new Prices( (IBar)lastData );

        return new Prices( current, open, high, low, close );
    }

    /**
     * Determines if the exchange is open using the current time of the asset
     */
    private static boolean isExchangeOpen( Security asset ) {
        if( !asset.getExchange().dateTimeIsOpen( asset.getLocalTime() ) ) {
            // if we're not open at the current time exactly, check the bar size, this handle large sized bars (hours/days)
            final BaseData currentBar = asset.getLastData();
            if( !asset.getLocalTime().toLocalDate().equals( currentBar.getEndTime().toLocalDate() ) || 
                    !asset.getExchange().isOpenDuringBar( currentBar.getTime(), currentBar.getEndTime(), false ) )
                return false;
        }
        
        return true;
    }

    private class Prices {
        public final BigDecimal current;
        public final BigDecimal open;
        public final BigDecimal high;
        public final BigDecimal low;
        public final BigDecimal close;

        public Prices( IBar bar ) {
            this( bar.getClose(), bar.getOpen(), bar.getHigh(), bar.getLow(), bar.getClose() );
        }

        public Prices( BigDecimal current, BigDecimal open, BigDecimal high, BigDecimal low, BigDecimal close ) {
            this.current = current;
            this.open = open.signum() == 0 ? current : open;
            this.high = high.signum() == 0 ? current : high;
            this.low = low.signum() == 0 ? current : low;
            this.close = close.signum() == 0 ? current : close;
        }
    }
}
