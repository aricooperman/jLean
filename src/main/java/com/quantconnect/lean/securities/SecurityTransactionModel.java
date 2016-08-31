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

import com.quantconnect.lean.orders.LimitOrder;
import com.quantconnect.lean.orders.MarketOnCloseOrder;
import com.quantconnect.lean.orders.MarketOnOpenOrder;
import com.quantconnect.lean.orders.MarketOrder;
import com.quantconnect.lean.orders.Order;
import com.quantconnect.lean.orders.OrderEvent;
import com.quantconnect.lean.orders.StopLimitOrder;
import com.quantconnect.lean.orders.StopMarketOrder;
import com.quantconnect.lean.orders.fees.ConstantFeeModel;
import com.quantconnect.lean.orders.fees.IFeeModel;
import com.quantconnect.lean.orders.fills.IFillModel;
import com.quantconnect.lean.orders.fills.ImmediateFillModel;
import com.quantconnect.lean.orders.slippage.ISlippageModel;
import com.quantconnect.lean.orders.slippage.SpreadSlippageModel;
import com.quantconnect.lean.securities.interfaces.ISecurityTransactionModel;

/**
 * Default security transaction model for user defined securities.
 */
public class SecurityTransactionModel implements ISecurityTransactionModel {
    
    private final IFillModel fillModel;
    private final IFeeModel feeModel;
    private final ISlippageModel slippageModel;

    /**
     * Initializes a new default instance of the <see cref="SecurityTransactionModel"/> class.
     * This will use default slippage and fill models.
     */
    public SecurityTransactionModel() {
        this( new ImmediateFillModel(), new ConstantFeeModel( BigDecimal.ZERO ), new SpreadSlippageModel() );
    }

    /**
     * Initializes a new instance of the <see cref="SecurityTransactionManager"/> class
     * @param fillModel The fill model to use
     * @param feeModel The order fee model to use
     * @param slippageModel The slippage model to use
     */
    public SecurityTransactionModel( IFillModel fillModel, IFeeModel feeModel, ISlippageModel slippageModel ) {
        this.fillModel = fillModel;
        this.feeModel = feeModel;
        this.slippageModel = slippageModel;
    }

    /**
     * Default market fill model for the base security class. Fills at the last traded price.
     * <seealso cref="StopMarketFill(Security, StopMarketOrder)"/>
     * <seealso cref="LimitFill(Security, LimitOrder)"/>
     * @param asset Security asset we're filling
     * @param order Order packet to model
     * @returns Order fill information detailing the average price and quantity filled.
     */
    public OrderEvent marketFill( Security asset, MarketOrder order ) {
        return fillModel.marketFill( asset, order );
    }

    /**
     * Default stop fill model implementation in base class security. (Stop Market Order Type)
     * <seealso cref="MarketFill(Security, MarketOrder)"/>
     * <seealso cref="LimitFill(Security, LimitOrder)"/>
     * @param asset Security asset we're filling
     * @param order Order packet to model
     * @returns Order fill information detailing the average price and quantity filled.
     */
    public OrderEvent stopMarketFill( Security asset, StopMarketOrder order ) {
        return fillModel.stopMarketFill( asset, order );
    }

    /**
     * Default stop limit fill model implementation in base class security. (Stop Limit Order Type)
     * <seealso cref="StopMarketFill(Security, StopMarketOrder)"/>
     * <seealso cref="LimitFill(Security, LimitOrder)"/>
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
     */
    public OrderEvent stopLimitFill( Security asset, StopLimitOrder order ) {
        return fillModel.stopLimitFill( asset, order );
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
        return fillModel.limitFill( asset, order );
    }

    /**
     * Market on Open Fill Model. Return an order event with the fill details
     * @param asset Asset we're trading with this order
     * @param order Order to be filled
     * @returns Order fill information detailing the average price and quantity filled.
     */
    public OrderEvent marketOnOpenFill( Security asset, MarketOnOpenOrder order ) {
        return fillModel.marketOnOpenFill( asset, order );
    }

    /**
     * Market on Close Fill Model. Return an order event with the fill details
     * @param asset Asset we're trading with this order
     * @param order Order to be filled
     * @returns Order fill information detailing the average price and quantity filled.
     */
    public OrderEvent marketOnCloseFill( Security asset, MarketOnCloseOrder order) {
        return fillModel.marketOnCloseFill( asset, order );
    }

    /**
     * Get the slippage approximation for this order
     * @param security Security asset we're filling
     * @param order Order packet to model
     * @returns decimal approximation for slippage
     */
    public BigDecimal getSlippageApproximation( Security security, Order order ) {
        return slippageModel.getSlippageApproximation( security, order );
    }

    /**
     * Default implementation returns 0 for fees.
     * @param security The security matching the order
     * @param order The order to compute fees for
     * @returns The cost of the order in units of the account currency
     */
    public BigDecimal getOrderFee( Security security, Order order ) {
        return feeModel.getOrderFee( security, order ).abs();
    }
}
