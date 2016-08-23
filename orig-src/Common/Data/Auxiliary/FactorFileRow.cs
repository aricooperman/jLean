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
 *
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

package com.quantconnect.lean.Data.Auxiliary
{
    /**
    /// Defines a single row in a factor_factor file. This is a csv file ordered as {date, price factor, split factor}
    */
    public class FactorFileRow
    {
        /**
        /// Gets the date associated with this data
        */
        public DateTime Date { get; private set; }

        /**
        /// Gets the price factor associated with this data
        */
        public BigDecimal PriceFactor { get; private set; }

        /**
        /// Gets te split factored associated with the date
        */
        public BigDecimal SplitFactor { get; private set; }

        /**
        /// Gets the combined factor used to create adjusted prices from raw prices
        */
        public BigDecimal PriceScaleFactor
        {
            get { return PriceFactor*SplitFactor; }
        }

        /**
        /// Initializes a new instance of the <see cref="FactorFileRow"/> class
        */
        public FactorFileRow(DateTime date, BigDecimal priceFactor, BigDecimal splitFactor) {
            Date = date;
            PriceFactor = priceFactor;
            SplitFactor = splitFactor;
        }

        /**
        /// Reads in the factor file for the specified equity symbol
        */
        public static IEnumerable<FactorFileRow> Read( String permtick, String market) {
            String path = Path.Combine(Globals.DataFolder, "equity", market, "factor_files", permtick.toLowerCase() + ".csv");
            return File.ReadAllLines(path).Where(l -> !string.IsNullOrWhiteSpace(l)).Select(Parse);
        }

        /**
        /// Parses the specified line as a factor file row
        */
        public static FactorFileRow Parse( String line) {
            csv = line.split(',');
            return new FactorFileRow(
                DateTime.ParseExact(csv[0], DateFormat.EightCharacter, CultureInfo.InvariantCulture, DateTimeStyles.None),
                decimal.Parse(csv[1], CultureInfo.InvariantCulture),
                decimal.Parse(csv[2], CultureInfo.InvariantCulture)
                );
        }

        /**
        /// Returns a String that represents the current object.
        */
        @returns 
        /// A String that represents the current object.
        /// 
        /// <filterpriority>2</filterpriority>
        public @Override String toString() {
            return Date + ": " + PriceScaleFactor.toString( "0.0000");
        }
    }
}