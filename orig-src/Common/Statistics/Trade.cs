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

using System;

package com.quantconnect.lean.Statistics
{
    /**
     * Represents a closed trade
    */
    public class Trade
    {
        /**
         * The symbol of the traded instrument
        */
        public Symbol Symbol { get; set; }

        /**
         * The date and time the trade was opened
        */
        public DateTime EntryTime { get; set; }

        /**
         * The price at which the trade was opened (or the average price if multiple entries)
        */
        public BigDecimal EntryPrice { get; set; }

        /**
         * The direction of the trade (Long or Short)
        */
        public TradeDirection Direction { get; set; }

        /**
         * The total unsigned quantity of the trade
        */
        public int Quantity { get; set; }

        /**
         * The date and time the trade was closed
        */
        public DateTime ExitTime { get; set; }

        /**
         * The price at which the trade was closed (or the average price if multiple exits)
        */
        public BigDecimal ExitPrice { get; set; }

        /**
         * The gross profit/loss of the trade (as symbol currency)
        */
        public BigDecimal ProfitLoss { get; set; }

        /**
         * The total fees associated with the trade (always positive value) (as symbol currency)
        */
        public BigDecimal TotalFees { get; set; }

        /**
         * The Maximum Adverse Excursion (as symbol currency)
        */
        public BigDecimal MAE { get; set; }

        /**
         * The Maximum Favorable Excursion (as symbol currency)
        */
        public BigDecimal MFE { get; set; }

        /**
         * Returns the duration of the trade
        */
        public Duration Duration
        {
            get { return ExitTime - EntryTime; }
        }

        /**
         * Returns the amount of profit given back before the trade was closed
        */
        public BigDecimal EndTradeDrawdown
        {
            get { return ProfitLoss - MFE; }
        }

    }
}
