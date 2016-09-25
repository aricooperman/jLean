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

import com.quantconnect.lean.Orders.slippage.ConstantSlippageModel;
import com.quantconnect.lean.orders.fees.InteractiveBrokersFeeModel;
import com.quantconnect.lean.orders.fills.ImmediateFillModel;
import com.quantconnect.lean.securities.SecurityTransactionModel;

/**
 * Transaction model for equity security trades. 
 * <seealso cref="SecurityTransactionModel"/>
 * <seealso cref="ISecurityTransactionModel"/>
 */
public class EquityTransactionModel extends SecurityTransactionModel {
        
    /**
     * Initializes a new instance of the <see cref="EquityTransactionModel"/>
     */
    public EquityTransactionModel() {
        super( new ImmediateFillModel(), new InteractiveBrokersFeeModel(), new ConstantSlippageModel( BigDecimal.ZERO ) );
    }
}
