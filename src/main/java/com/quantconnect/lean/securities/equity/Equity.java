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

package com.quantconnect.lean.securities.equity;

import java.math.BigDecimal;
import java.time.Duration;

import com.quantconnect.lean.Global;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.Orders.slippage.ConstantSlippageModel;
import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.orders.fees.InteractiveBrokersFeeModel;
import com.quantconnect.lean.orders.fills.ImmediateFillModel;
import com.quantconnect.lean.securities.Cash;
import com.quantconnect.lean.securities.IVolatilityModel;
import com.quantconnect.lean.securities.ImmediateSettlementModel;
import com.quantconnect.lean.securities.Security;
import com.quantconnect.lean.securities.SecurityCache;
import com.quantconnect.lean.securities.SecurityExchangeHours;
import com.quantconnect.lean.securities.SecurityMarginModel;
import com.quantconnect.lean.securities.SecurityPortfolioModel;
import com.quantconnect.lean.securities.SymbolProperties;

/**
 * Equity Security Class : Extension of the underlying Security class for equity specific behaviors.
 * <seealso cref="Security"/>
 */
public class Equity extends Security {
   
    /**
     * The default number of days required to settle an equity sale
     */
    public static final int DEFAULT_SETTLEMENT_DAYS = 3;

    /**
     * The default time of day for settlement
     */
    public static final Duration DEFAULT_SETTLEMENT_TIME = Duration.ZERO;

    /**
     * Construct the Equity Object
     */
    public Equity( Symbol symbol, SecurityExchangeHours exchangeHours, Cash quoteCurrency, SymbolProperties symbolProperties ) {
        super( 
                symbol, 
                quoteCurrency,
                symbolProperties,
                new EquityExchange( exchangeHours ),
                new SecurityCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new InteractiveBrokersFeeModel(),
                new ConstantSlippageModel( BigDecimal.ZERO ),
                new ImmediateSettlementModel(),
                IVolatilityModel.NULL,
                new SecurityMarginModel( Global.TWO ),
                new EquityDataFilter() );
        
        setHoldings( new EquityHolding( this ) );
    }

    /**
     * Construct the Equity Object
     */
    public Equity( SecurityExchangeHours exchangeHours, SubscriptionDataConfig config, Cash quoteCurrency, SymbolProperties symbolProperties ) {
        super(
                config,
                quoteCurrency,
                symbolProperties,
                new EquityExchange(exchangeHours),
                new SecurityCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new InteractiveBrokersFeeModel(),
                new ConstantSlippageModel( BigDecimal.ZERO ),
                new ImmediateSettlementModel(),
                IVolatilityModel.NULL,
                new SecurityMarginModel( Global.TWO ),
                new EquityDataFilter() );
        
        setHoldings( new EquityHolding( this ) );
    }
}
