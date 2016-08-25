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

import com.quantconnect.lean.orders.OrderEvent;

/**
 * Performs order fill application to portfolio
 */
public interface ISecurityPortfolioModel {

    /**
     * Performs application of an OrderEvent to the portfolio
     * @param portfolio The algorithm's portfolio
     * @param security The fill's security
     * @param fill The order event fill object to be applied
     */
    void processFill( SecurityPortfolioManager portfolio, Security security, OrderEvent fill );
}