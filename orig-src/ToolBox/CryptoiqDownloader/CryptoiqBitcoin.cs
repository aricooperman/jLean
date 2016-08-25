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

package com.quantconnect.lean.ToolBox.CryptoiqDownloader
{
    /**
     * Defines the JSON response structure from the Cryptoiq API
    */
    public class CryptoiqBitcoin
    {
        /**
         * The time of the tick
        */
        public DateTime Time;
        /**
         * The ask price
        */
        public BigDecimal Ask;
        /**
         * The bid price
        */
        public BigDecimal Bid;
        /**
         * The price of the last trade
        */
        public BigDecimal Last;
        /**
         * The daily high
        */
        public BigDecimal High;
        /**
         * The daily low
        */
        public BigDecimal Low;
        /**
         * The daily running volume
        */
        public BigDecimal Volume;
    }

}
