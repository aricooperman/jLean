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
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data.Market;

package com.quantconnect.lean.Algorithm.Examples
{
    /**
    /// Randomly selects the specified number of symbols from the lists below
    */
    public class StressSymbolsAlgorithm : QCAlgorithm
    {
        public static final int TickSymbolsToRun = 0;
        public static final int SecondSymbolsToRun = 0;
        public static final int MinuteSymbolsToRun = 0;
        public static final int HourSymbolsToRun = 0;
        public static final int DailySymbolsToRun = 1000;

        /**
        /// Add Hundreds of Stock and Forex Symbol
        */
        public @Override void Initialize() {
            SetStartDate(2001, 10, 07);
            SetEndDate(2010, 10, 11);
            SetCash(250000);

            allSymbols = StressSymbols.StockSymbols.ToList();//.Concat(ForexSymbols).ToList();
            if( TickSymbolsToRun + SecondSymbolsToRun + HourSymbolsToRun + DailySymbolsToRun > allSymbols.Count) {
                throw new Exception( "Too many symbols, all symbols: " + allSymbols.Count);
            }


            hash = new HashSet<String> {"DNY", "MLNK"};
            ticks = GetRandomSymbols(allSymbols, hash, TickSymbolsToRun).ToList();
            seconds = GetRandomSymbols(allSymbols, hash, SecondSymbolsToRun).ToList();
            minutes = GetRandomSymbols(allSymbols, hash, MinuteSymbolsToRun).ToList();
            hours = GetRandomSymbols(allSymbols, hash, HourSymbolsToRun).ToList();
            daily = GetRandomSymbols(allSymbols, hash, DailySymbolsToRun).ToList();

            AddSecurity(ticks, Resolution.Tick);
            AddSecurity(seconds, Resolution.Second);
            AddSecurity(minutes, Resolution.Minute);
            AddSecurity(hours, Resolution.Hour);
            AddSecurity(daily, Resolution.Daily);

            //SetUniverse(coarse -> coarse.Take(1));
        }

        private void AddSecurity(IEnumerable<String> symbols, Resolution resolution) {
            foreach (symbol in symbols) {
                securityType = StressSymbols.ForexSymbols.Contains(symbol) ? SecurityType.Forex : SecurityType.Equity;
                AddSecurity(securityType, symbol, resolution);
            }
        }

        private IEnumerable<String> GetRandomSymbols(List<String> allSymbols, HashSet<String> hash, int numberOfSymbols) {
            return Enumerable.Range(0, numberOfSymbols).Select(x -> GetRandomItem(allSymbols, hash));
        }

        private final Random _random = new Random();
        private String GetRandomItem(IReadOnlyList<String> list, HashSet<String> hash) {
            count = 0;
            String item;
            do
            {
                item = list[_random.Next(list.Count)];
                count++;
            }
            while (!hash.Add(item) && count < list.Count*2);
            return item;
        }

        /**
        /// TradeBar data event handler
        */
        public void OnData(TradeBars data) {

        }
    }
}