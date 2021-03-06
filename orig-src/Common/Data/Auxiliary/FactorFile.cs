﻿﻿/*
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
 *
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuantConnect.Logging;

package com.quantconnect.lean.Data.Auxiliary
{
    /**
     * Represents an entire factor file for a specified symbol
    */
    public class FactorFile
    {
        private final SortedList<DateTime, FactorFileRow> _data;

        /**
         * Gets the symbol this factor file represents
        */
        public String Permtick { get; private set; }

        /**
         * Initializes a new instance of the <see cref="FactorFile"/> class.
        */
        public FactorFile( String permtick, IEnumerable<FactorFileRow> data) {
            Permtick = permtick.toUpperCase();
            _data = new SortedList<DateTime, FactorFileRow>(data.ToDictionary(x -> x.Date));
        }

        /**
         * Reads a FactorFile in from the <see cref="Globals.DataFolder"/>.
        */
        public static FactorFile Read( String permtick, String market) {
            return new FactorFile(permtick, FactorFileRow.Read(permtick, market));
        }

        /**
         * Gets the price scale factor that includes dividend and split adjustments for the specified search date
        */
        public BigDecimal GetPriceScaleFactor(DateTime searchDate) {
            BigDecimal factor = 1;
            //Iterate backwards to find the most recent factor:
            foreach (splitDate in _data.Keys.Reverse()) {
                if( splitDate.Date < searchDate.Date) break;
                factor = _data[splitDate].PriceScaleFactor;
            }
            return factor;
        }

        /**
         * Gets the split factor to be applied at the specified date
        */
        public BigDecimal GetSplitFactor(DateTime searchDate) {
            BigDecimal factor = 1;
            //Iterate backwards to find the most recent factor:
            foreach (splitDate in _data.Keys.Reverse()) {
                if( splitDate.Date < searchDate.Date) break;
                factor = _data[splitDate].splitFactor;
            }
            return factor;
        }

        /**
         * Checks whether or not a symbol has scaling factors
        */
        public static boolean HasScalingFactors( String permtick, String market) {
            // check for factor files
            path = Path.Combine(Globals.DataFolder, "equity", market, "factor_files", permtick.toLowerCase() + ".csv");
            if( File.Exists(path)) {
                return true;
            }
            Log.Trace( "FactorFile.HasScalingFactors(): Factor file not found: " + permtick);
            return false;
        }

        /**
         * Returns true if the specified date is the last trading day before a dividend event
         * is to be fired
        */
         * 
         * NOTE: The dividend event in the algorithm should be fired at the end or AFTER
         * this date. This is the date in the file that a factor is applied, so for example,
         * MSFT has a 31 cent dividend on 2015.02.17, but in the factor file the factor is applied
         * to 2015.02.13, which is the first trading day BEFORE the actual effective date.
         * 
         * @param date The date to check the factor file for a dividend event
         * @param priceFactorRatio When this function returns true, this value will be populated
         * with the price factor ratio required to scale the closing value (pf_i/pf_i+1)
        public boolean HasDividendEventOnNextTradingDay(DateTime date, out BigDecimal priceFactorRatio) {
            priceFactorRatio = 0;
            index = _data.IndexOfKey(date);
            if( index > -1 && index < _data.Count - 1) {
                // grab the next key to ensure it's a dividend event
                thisRow = _data.Values[index];
                nextRow = _data.Values[index + 1];

                // if the price factors have changed then it's a dividend event
                if( thisRow.PriceFactor != nextRow.PriceFactor) {
                    priceFactorRatio = thisRow.PriceFactor/nextRow.PriceFactor;
                    return true;
                }
            }
            return false;
        }

        /**
         * Returns true if the specified date is the last trading day before a split event
         * is to be fired
        */
         * 
         * NOTE: The split event in the algorithm should be fired at the end or AFTER this
         * date. This is the date in the file that a factor is applied, so for example MSFT
         * has a split on 1999.03.29, but in the factor file the split factor is applied on
         * 1999.03.26, which is the first trading day BEFORE the actual split date.
         * 
        public boolean HasSplitEventOnNextTradingDay(DateTime date, out BigDecimal splitFactor) {
            splitFactor = 1;
            index = _data.IndexOfKey(date);
            if( index > -1 && index < _data.Count - 1) {
                // grab the next key to ensure it's a split event
                thisRow = _data.Values[index];
                nextRow = _data.Values[index + 1];

                // if the split factors have changed then it's a split event
                if( thisRow.splitFactor != nextRow.splitFactor) {
                    splitFactor = thisRow.splitFactor/nextRow.splitFactor;
                    return true;
                }
            }
            return false;
        }
    }
}