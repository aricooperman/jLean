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

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;

package com.quantconnect.lean.Algorithm
{
    /**
     * Provides helpers for defining universes based on the daily dollar volume
    */
    public class DollarVolumeUniverseDefinitions
    {
        private final QCAlgorithm _algorithm;

        /**
         * Initializes a new instance of the <see cref="DollarVolumeUniverseDefinitions"/> class
        */
         * @param algorithm The algorithm instance, used for obtaining the default <see cref="UniverseSettings"/>
        public DollarVolumeUniverseDefinitions(QCAlgorithm algorithm) {
            _algorithm = algorithm;
        }

        /**
         * Creates a new coarse universe that contains the top count of stocks
         * by daily dollar volume
        */
         * @param count The number of stock to select
         * @param universeSettings The settings for stocks added by this universe.
         * Defaults to <see cref="QCAlgorithm.UniverseSettings"/>
        @returns A new coarse universe for the top count of stocks by dollar volume
        public Universe Top(int count, UniverseSettings universeSettings = null ) {
            universeSettings = universeSettings ?? _algorithm.UniverseSettings;
            symbol = CoarseFundamental.CreateUniverseSymbol(Market.USA);
            config = new SubscriptionDataConfig(typeof(CoarseFundamental), symbol, Resolution.Daily, TimeZones.NewYork, TimeZones.NewYork, false, false, true);
            return new FuncUniverse(config, universeSettings, _algorithm.SecurityInitializer, selectionData -> (
                from c in selectionData.OfType<CoarseFundamental>()
                orderby c.DollarVolume descending 
                select c.Symbol).Take(count)
                );
        }

        /**
         * Creates a new coarse universe that contains the bottom count of stocks
         * by daily dollar volume
        */
         * @param count The number of stock to select
         * @param universeSettings The settings for stocks added by this universe.
         * Defaults to <see cref="QCAlgorithm.UniverseSettings"/>
        @returns A new coarse universe for the bottom count of stocks by dollar volume
        public Universe Bottom(int count, UniverseSettings universeSettings = null ) {
            universeSettings = universeSettings ?? _algorithm.UniverseSettings;
            symbol = CoarseFundamental.CreateUniverseSymbol(Market.USA);
            config = new SubscriptionDataConfig(typeof(CoarseFundamental), symbol, Resolution.Daily, TimeZones.NewYork, TimeZones.NewYork, false, false, true);
            return new FuncUniverse(config, universeSettings, _algorithm.SecurityInitializer, selectionData -> (
                from c in selectionData.OfType<CoarseFundamental>()
                orderby c.DollarVolume descending 
                select c.Symbol).Take(count)
                );
        }

        /**
         * Creates a new coarse universe that contains stocks in the specified
         * dollar volume percentile
        */
         * @param percentile The desired dollar volume percentile (0 to 100 inclusive)
         * @param universeSettings The settings for stocks added by this universe.
         * Defaults to <see cref="QCAlgorithm.UniverseSettings"/>
        @returns A new coarse universe for the bottom count of stocks by dollar volume
        public Universe Percentile(double percentile, UniverseSettings universeSettings = null ) {
            universeSettings = universeSettings ?? _algorithm.UniverseSettings;
            symbol = CoarseFundamental.CreateUniverseSymbol(Market.USA);
            config = new SubscriptionDataConfig(typeof(CoarseFundamental), symbol, Resolution.Daily, TimeZones.NewYork, TimeZones.NewYork, false, false, true);
            return new FuncUniverse(config, universeSettings, _algorithm.SecurityInitializer, selectionData =>
            {
                list = selectionData as IReadOnlyList<CoarseFundamental> ?? selectionData.OfType<CoarseFundamental>().ToList();

                // using quantiles since the Percentile implementation requires integers, so scale into quantile space
                lowerBound = (decimal)list.Select(x -> (double)x.DollarVolume).Quantile(percentile / 100d);

                return from c in list
                       where c.DollarVolume >= lowerBound
                       orderby c.DollarVolume descending
                       select c.Symbol;
            });
        }

        /**
         * Creates a new coarse universe that contains stocks in the specified dollar volume percentile range,
         * that is, this universe will produce stocks with dollar volumes between the lower percentile bound
         * and the upper percentile bound
        */
         * @param lowerPercentile The desired lower dollar volume  percentile bound (0 to 100 inclusive)
         * @param upperPercentile The desired upper dollar volume  percentile bound (0 to 100 inclusive)
         * @param universeSettings The settings for stocks added by this universe.
         * Defaults to <see cref="QCAlgorithm.UniverseSettings"/>
        @returns A new coarse universe for the bottom count of stocks by dollar volume
        public Universe Percentile(double lowerPercentile, double upperPercentile, UniverseSettings universeSettings = null ) {
            universeSettings = universeSettings ?? _algorithm.UniverseSettings;
            symbol = CoarseFundamental.CreateUniverseSymbol(Market.USA);
            config = new SubscriptionDataConfig(typeof(CoarseFundamental), symbol, Resolution.Daily, TimeZones.NewYork, TimeZones.NewYork, false, false, true);
            return new FuncUniverse(config, universeSettings, _algorithm.SecurityInitializer, selectionData =>
            {
                list = selectionData as IReadOnlyList<CoarseFundamental> ?? selectionData.OfType<CoarseFundamental>().ToList();

                // using quantiles since the Percentile implementation requires integers, so scale into quantile space
                lowerBound = (decimal) list.Select(x -> (double) x.DollarVolume).Quantile(lowerPercentile/100d);
                upperBound = (decimal) list.Select(x -> (double) x.DollarVolume).Quantile(upperPercentile/100d);

                return from c in list
                       where c.DollarVolume >= lowerBound
                       where c.DollarVolume <= upperBound
                       orderby c.DollarVolume descending
                       select c.Symbol;
            });
        }
    }
}