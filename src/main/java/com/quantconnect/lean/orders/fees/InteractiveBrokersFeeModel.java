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

package com.quantconnect.lean.orders.fees;

import java.math.BigDecimal;

import com.quantconnect.lean.Global;
import com.quantconnect.lean.orders.Order;
import com.quantconnect.lean.securities.Security;

/**
 * Provides the default implementation of <see cref="IFeeModel"/>
 */
public class InteractiveBrokersFeeModel implements IFeeModel {
    private static final BigDecimal ONE_AND_QUARTER_DOLLARS = new BigDecimal( "1.25" );
    private static final BigDecimal ONE_AND_HALF_DOLLARS = new BigDecimal( "1.50" );
    
    private static final BigDecimal EIGHT_PERCENT = new BigDecimal( "0.08" );
    private static final BigDecimal TEN_PERCENT = new BigDecimal( "0.10" );
    private static final BigDecimal HALF_PERCENT = new BigDecimal( "0.005" );
    private static final BigDecimal TWENTY_PERCENT = new BigDecimal( "0.20" );
    private static final BigDecimal FIFTEEN_PERCENT = new BigDecimal( "0.15" );

    private static final BigDecimal ONE_BILLION = BigDecimal.valueOf( 1_000_000_000L );
    private static final BigDecimal TWO_BILLION = BigDecimal.valueOf( 2_000_000_000L );
    private static final BigDecimal FIVE_BILLION = BigDecimal.valueOf( 5_000_000_000L );

    private static final BigDecimal BP = new BigDecimal( "0.0001" );

    private final BigDecimal _forexCommissionRate;
    private final BigDecimal _forexMinimumOrderFee;

    /**
     * Initializes a new instance of the <see cref="ImmediateFillModel"/>
     * @param monthlyForexTradeAmountInUSDollars Monthly dollar volume traded
     */
    public InteractiveBrokersFeeModel() {
        this( BigDecimal.ZERO );
    }
    
    /**
     * Initializes a new instance of the <see cref="ImmediateFillModel"/>
     * @param monthlyForexTradeAmountInUSDollars Monthly dollar volume traded
     */
    public InteractiveBrokersFeeModel( BigDecimal monthlyForexTradeAmountInUSDollars ) {
        if( monthlyForexTradeAmountInUSDollars.compareTo( ONE_BILLION ) <= 0 ) {
            _forexCommissionRate = TWENTY_PERCENT.multiply( BP );
            _forexMinimumOrderFee = Global.TWO;
        }
        else if( monthlyForexTradeAmountInUSDollars.compareTo( TWO_BILLION ) <= 0 ) {
            _forexCommissionRate = FIFTEEN_PERCENT.multiply( BP );
            _forexMinimumOrderFee = ONE_AND_HALF_DOLLARS;
        }
        else if( monthlyForexTradeAmountInUSDollars.compareTo( FIVE_BILLION ) <= 0 ) {
            _forexCommissionRate = TEN_PERCENT.multiply( BP );
            _forexMinimumOrderFee = ONE_AND_QUARTER_DOLLARS;
        }
        else {
            _forexCommissionRate = EIGHT_PERCENT.multiply( BP );
            _forexMinimumOrderFee = BigDecimal.ONE;
        }
    }

    /**
     * Gets the order fee associated with the specified order. This returns the cost
     * of the transaction in the account currency
     * @param security The security matching the order
     * @param order The order to compute fees for
     * @returns The cost of the order in units of the account currency
     */
    public BigDecimal getOrderFee( Security security, Order order ) {
        switch( security.getType() ) {
            case Forex:
                // get the total order value in the account currency
                final BigDecimal totalOrderValue = order.getValue( security );
                final BigDecimal fee = _forexCommissionRate.multiply( totalOrderValue ).abs();
                return _forexMinimumOrderFee.max( fee );

            case Equity:
                final BigDecimal tradeValue = order.getValue( security ).abs();

                //Per share fees
                BigDecimal tradeFee = HALF_PERCENT.multiply( BigDecimal.valueOf( order.getAbsoluteQuantity() ) );

                //Maximum Per Order: 0.5%
                //Minimum per order. $1.0
                final BigDecimal maximumPerOrder = HALF_PERCENT.multiply( tradeValue );
                tradeFee = tradeFee.max( BigDecimal.ONE ).min( maximumPerOrder );

                //Always return a positive fee.
                return tradeFee.abs();
            default:
                // all other types default to zero fees
                return BigDecimal.ZERO;
        }
    }
}
