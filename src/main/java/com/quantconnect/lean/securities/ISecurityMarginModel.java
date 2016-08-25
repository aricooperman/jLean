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

import com.quantconnect.lean.orders.Order;
import com.quantconnect.lean.orders.OrderTypes.OrderDirection;
import com.quantconnect.lean.orders.SubmitOrderRequest;
import com.quantconnect.lean.securities.Security;

/**
 * Represents a security's model of margining
*/
public interface ISecurityMarginModel {
    /**
     * Gets the current leverage of the security
     * @param security The security to get leverage for
     * @returns The current leverage in the security
    */
    BigDecimal getLeverage( Security security );

    /**
     * Sets the leverage for the applicable securities, i.e, equities
     * 
     * This is added to maintain backwards compatibility with the old margin/leverage system
     *  
     * @param security The security to set leverage for
     * @param leverage The new leverage
     */
    void setLeverage( Security security, BigDecimal leverage );

    /**
     * Gets the total margin required to execute the specified order
     * @param security
     * @param order The order to be executed
     * @returns The total margin in terms of the currency quoted in the order
     */
    BigDecimal getInitialMarginRequiredForOrder( Security security, Order order );

    /**
     * Gets the margin currently alloted to the specified holding
     * @param security The security to compute maintenance margin for
     * @returns The maintenance margin required for the 
     */
    BigDecimal getMaintenanceMargin( Security security );

    /**
     * Gets the margin cash available for a trade
     * @param portfolio The algorithm's portfolio
     * @param security The security to be traded
     * @param direction The direction of the trade
     * @returns The margin available for the trade
     */
    BigDecimal getMarginRemaining( SecurityPortfolioManager portfolio, Security security, OrderDirection direction );

    /**
     * Generates a new order for the specified security taking into account the total margin
     * used by the account. Returns null when no margin call is to be issued.
     * @param security The security to generate a margin call order for
     * @param netLiquidationValue The net liquidation value for the entire account
     * @param totalMargin The totl margin used by the account in units of base currency
     * @returns An order object representing a liquidation order to be executed to bring the account within margin requirements
     */
    SubmitOrderRequest generateMarginCallOrder( Security security, BigDecimal netLiquidationValue, BigDecimal totalMargin );
}
