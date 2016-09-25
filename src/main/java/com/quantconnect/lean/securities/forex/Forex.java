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

package com.quantconnect.lean.securities.forex;

import java.math.BigDecimal;

import org.apache.commons.lang3.tuple.Pair;

import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.SubscriptionDataConfig;
import com.quantconnect.lean.orders.fees.InteractiveBrokersFeeModel;
import com.quantconnect.lean.orders.fills.ImmediateFillModel;
import com.quantconnect.lean.orders.slippage.SpreadSlippageModel;
import com.quantconnect.lean.securities.Cash;
import com.quantconnect.lean.securities.IVolatilityModel;
import com.quantconnect.lean.securities.ImmediateSettlementModel;
import com.quantconnect.lean.securities.Security;
import com.quantconnect.lean.securities.SecurityExchangeHours;
import com.quantconnect.lean.securities.SecurityMarginModel;
import com.quantconnect.lean.securities.SecurityPortfolioModel;
import com.quantconnect.lean.securities.SymbolProperties;

/**
 * FOREX Security Object Implementation for FOREX Assets
 * <seealso cref="Security"/>
 */
public class Forex extends Security {
    
    private String baseCurrencySymbol;

    /**
     * Constructor for the forex security
     * @param exchangeHours Defines the hours this exchange is open
     * @param quoteCurrency The cash object that represent the quote currency
     * @param config The subscription configuration for this security
     * @param symbolProperties The symbol properties for this security
     */
    public Forex( SecurityExchangeHours exchangeHours, Cash quoteCurrency, SubscriptionDataConfig config, SymbolProperties symbolProperties ) {
        super( config,
                quoteCurrency,
                symbolProperties,
                new ForexExchange( exchangeHours ),
                new ForexCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new InteractiveBrokersFeeModel(),
                new SpreadSlippageModel(),
                new ImmediateSettlementModel(),
                IVolatilityModel.NULL,
                new SecurityMarginModel( BigDecimal.valueOf( 50 ) ),
                new ForexDataFilter() );
        
        setHoldings( new ForexHolding( this ) );

        // decompose the symbol into each currency pair
        baseCurrencySymbol = decomposeCurrencyPair( config.getSymbol().getValue() ).getLeft();
    }

    /**
     * Constructor for the forex security
     * @param symbol The security's symbol
     * @param exchangeHours Defines the hours this exchange is open
     * @param quoteCurrency The cash object that represent the quote currency
     * @param symbolProperties The symbol properties for this security
     */
    public Forex( Symbol symbol, SecurityExchangeHours exchangeHours, Cash quoteCurrency, SymbolProperties symbolProperties ) {
        super( symbol,
                quoteCurrency,
                symbolProperties,
                new ForexExchange(exchangeHours),
                new ForexCache(),
                new SecurityPortfolioModel(),
                new ImmediateFillModel(),
                new InteractiveBrokersFeeModel(),
                new SpreadSlippageModel(),
                new ImmediateSettlementModel(),
                IVolatilityModel.NULL,
                new SecurityMarginModel( BigDecimal.valueOf( 50 ) ),
                new ForexDataFilter() );
        
        setHoldings( new ForexHolding( this ) );

        // decompose the symbol into each currency pair
        baseCurrencySymbol = decomposeCurrencyPair( symbol.getValue() ).getLeft();
    }

    /**
     * Gets the currency acquired by going long this currency pair
     * 
     * For example, the EUR/USD has a base currency of the euro, and as a result
     * of going long the EUR/USD a trader is acquiring euros in exchange for US dollars
     * 
     */
    public String getBaseCurrencySymbol() {
        return baseCurrencySymbol;
    }

    /**
     * Decomposes the specified currency pair into a base and quote currency provided as out parameters
     * @param currencyPair The input currency pair to be decomposed, for example, "EURUSD"
     * @return Pair of the output base currency & the output quote currency
     */
    public static Pair<String,String> decomposeCurrencyPair( String currencyPair ) {
        if( currencyPair == null || currencyPair.length() != 6 )
            throw new IllegalArgumentException( "Currency pairs must be exactly 6 characters: " + currencyPair );
        
        return Pair.of( currencyPair.substring( 0, 3 ), currencyPair.substring( 3 ) );
    }
}
