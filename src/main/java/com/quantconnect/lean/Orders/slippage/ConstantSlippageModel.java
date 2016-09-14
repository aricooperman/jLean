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

package com.quantconnect.lean.Orders.slippage;

import java.math.BigDecimal;

import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.orders.Order;
import com.quantconnect.lean.orders.slippage.SpreadSlippageModel;
import com.quantconnect.lean.securities.Security;

/**
 * Represents a slippage model that uses a constant percentage of slip
 */
public class ConstantSlippageModel extends SpreadSlippageModel {
    
    private final BigDecimal slippagePercent;
    
    /**
     * Initializes a new instance of the <see cref="ConstantSlippageModel"/> class
     * @param slippagePercent The slippage percent for each order. Percent is ranged 0 to 1.
     */
    public ConstantSlippageModel( BigDecimal slippagePercent ) {
        this.slippagePercent = slippagePercent;
    }

    /**
     * Slippage Model. Return a BigDecimal cash slippage approximation on the order.
     */
    @Override
    public BigDecimal getSlippageApproximation( Security asset, Order order) {
        BaseData lastData = asset.getLastData();
        if( lastData == null ) 
            return BigDecimal.ZERO;

        return lastData.getValue().multiply( slippagePercent );
    }
}
