﻿/*
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

using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators
{
    /**
     * The TradeBarIndicator is an indicator that accepts TradeBar data as its input.
     * 
     * This type is more of a shim/typedef to reduce the need to refer to things as IndicatorBase&lt;TradeBar&gt;
    */
    public abstract class TradeBarIndicator : IndicatorBase<TradeBar>
    {
        /**
         * Creates a new TradeBarIndicator with the specified name
        */
         * @param name The name of this indicator
        protected TradeBarIndicator( String name)
            : base(name) {
        }
    }
}