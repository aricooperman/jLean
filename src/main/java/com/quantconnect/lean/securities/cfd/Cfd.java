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

package com.quantconnect.lean.securities.cfd;

import java.math.BigDecimal;

import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.orders.fees.ConstantFeeModel;
import com.quantconnect.lean.orders.fills.ImmediateFillModel;
import com.quantconnect.lean.orders.slippage.SpreadSlippageModel;
import com.quantconnect.lean.securities.Cash;
import com.quantconnect.lean.securities.IVolatilityModel;
import com.quantconnect.lean.securities.Security;
import com.quantconnect.lean.securities.SecurityExchangeHours;
import com.quantconnect.lean.securities.SecurityPortfolioModel;
import com.quantconnect.lean.securities.SymbolProperties;
import com.quantconnect.lean.securities.ImmediateSettlementModel;
import com.quantconnect.lean.securities.SecurityMarginModel;
import com.quantconnect.lean.Symbol;

/**
 * CFD Security Object Implementation for CFD Assets
 * <seealso cref="Security"/>
 */
public class Cfd extends Security {
    
    /**
     * Constructor for the CFD security
     * @param exchangeHours Defines the hours this exchange is open
     * @param quoteCurrency The cash object that represent the quote currency
     * @param config The subscription configuration for this security
     * @param symbolProperties The symbol properties for this security
     */
    public Cfd( SecurityExchangeHours exchangeHours, Cash quoteCurrency, SubscriptionDataConfig config, SymbolProperties symbolProperties ) {
        super( config,
                quoteCurrency,
                symbolProperties,
                new CfdExchange( exchangeHours ),
                new CfdCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new ConstantFeeModel( BigDecimal.ZERO ),
                new SpreadSlippageModel(),
                new ImmediateSettlementModel(),
                IVolatilityModel.NULL,
                new SecurityMarginModel( BigDecimal.valueOf( 50 ) ),
                new CfdDataFilter() );
        setHoldings( new CfdHolding( this ) );
    }

    /**
     * Constructor for the CFD security
     * @param symbol The security's symbol
     * @param exchangeHours Defines the hours this exchange is open
     * @param quoteCurrency The cash object that represent the quote currency
     * @param symbolProperties The symbol properties for this security
     */
    public Cfd( Symbol symbol, SecurityExchangeHours exchangeHours, Cash quoteCurrency, SymbolProperties symbolProperties ) {
        super( symbol,
                quoteCurrency,
                symbolProperties,
                new CfdExchange( exchangeHours ),
                new CfdCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new ConstantFeeModel( BigDecimal.ZERO ),
                new SpreadSlippageModel(),
                new ImmediateSettlementModel(),
                IVolatilityModel.NULL,
                new SecurityMarginModel( BigDecimal.valueOf( 50 ) ),
                new CfdDataFilter() );
        setHoldings( new CfdHolding( this ) );
    }

    /**
     * Gets the contract multiplier for this CFD security
     * 
     * PipValue := ContractMultiplier * PipSize
     * 
     */
    public BigDecimal getContractMultiplier() {
        return getSymbolProperties().getContractMultiplier();
    }

    /**
     * Gets the pip size for this CFD security
     */
    public BigDecimal getPipSize() {
        return getSymbolProperties().getPipSize();
    }
}
