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

package com.quantconnect.lean.orders.slippage;

import java.math.BigDecimal;
import java.math.RoundingMode;

import com.quantconnect.lean.Global;
import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.data.market.Tick;
import com.quantconnect.lean.orders.Order;
import com.quantconnect.lean.securities.Security;

/**
 * A slippage model that uses half of the bid/ask spread if available,
 * if not available, zero slippage is assumed.
 */
public class SpreadSlippageModel implements ISlippageModel {
       
    /**
     * Slippage Model. Return a BigDecimal cash slippage approximation on the order.
     */
    public BigDecimal getSlippageApproximation( Security asset, Order order ) {
        final BaseData lastData = asset.getLastData();

        if( lastData instanceof Tick ) {
            final Tick lastTick = (Tick)lastData;
            // if we have tick data use the spread
            return (lastTick.askPrice.subtract( lastTick.bidPrice )).divide( Global.TWO, RoundingMode.HALF_UP );
        }

        return BigDecimal.ZERO;
    }
}