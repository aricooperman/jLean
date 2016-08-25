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

package com.quantconnect.lean
{
    /**
     * A type capable of taking a chart and resampling using a linear interpolation strategy
    */
    public class SeriesSampler
    {
        private final double _seconds;

        /**
         * Creates a new SeriesSampler to sample Series data on the specified resolution
        */
         * @param resolution The desired sampling resolution
        public SeriesSampler(TimeSpan resolution) {
            _seconds = resolution.TotalSeconds;
        }

        /**
         * Samples the given series
        */
         * @param series The series to be sampled
         * @param start The date to start sampling, if before start of data then start of data will be used
         * @param stop The date to stop sampling, if after stop of data, then stop of data will be used
        @returns The sampled series
        public Series Sample(Series series, DateTime start, DateTime stop) {
            sampled = new Series(series.Name, series.SeriesType, series.Index, series.Unit);

            // chart point times are always in universal, so force it here as well
            double nextSample = Time.DateTimeToUnixTimeStamp(start.ToUniversalTime());
            double unixStopDate = Time.DateTimeToUnixTimeStamp(stop.ToUniversalTime());

            // we can't sample a single point and it doesn't make sense to sample scatter plots
            // in this case just copy the raw data
            if( series.Values.Count < 2 || series.SeriesType == SeriesType.Scatter) {
                // we can minimally verify we're within the start/stop interval
                foreach (point in series.Values) {
                    if( point.x >= nextSample && point.x <= unixStopDate) {
                        sampled.Values.Add(point);
                    }
                }
                return sampled;
            }

            enumerator = series.Values.GetEnumerator();

            // initialize current/previous
            enumerator.MoveNext();
            ChartPoint previous = enumerator.Current;
            enumerator.MoveNext();
            ChartPoint current = enumerator.Current;

            // make sure we don't start sampling before the data begins
            if( nextSample < previous.x) {
                nextSample = previous.x;
            }

            // make sure to advance into the requestd time frame before sampling
            while (current.x < nextSample && enumerator.MoveNext()) {
                previous = current;
                current = enumerator.Current;
            }

            do
            {
                // advance our current/previous
                if( nextSample > current.x) {
                    if( enumerator.MoveNext()) {
                        previous = current;
                        current = enumerator.Current;
                    }
                    else
                    {
                        break;
                    }
                }

                // iterate until we pass where we want our next point
                while (nextSample <= current.x && nextSample <= unixStopDate) {
                    value = Interpolate(previous, current, (long) nextSample);
                    sampled.Values.Add(new ChartPoint {x = (long) nextSample, y = value});
                    nextSample += _seconds;
                }

                // if we've passed our stop then we're finished sampling
                if( nextSample > unixStopDate) {
                    break;
                }
            }
            while (true);

            return sampled;
        }

        /**
         * Samples the given charts
        */
         * @param charts The charts to be sampled
         * @param start The date to start sampling
         * @param stop The date to stop sampling
        @returns The sampled charts
        public Map<String, Chart> SampleCharts(Map<String, Chart> charts, DateTime start, DateTime stop) {
            sampledCharts = new Map<String, Chart>();
            foreach (chart in charts.Values) {
                sampledChart = new Chart(chart.Name);
                sampledCharts.Add(sampledChart.Name, sampledChart);
                foreach (series in chart.Series.Values) {
                    sampledSeries = Sample(series, start, stop);
                    sampledChart.AddSeries(sampledSeries);
                }
            }
            return sampledCharts;
        }

        /**
         * Linear interpolation used for sampling
        */
        private static BigDecimal Interpolate(ChartPoint previous, ChartPoint current, long target) {
            deltaTicks = current.x - previous.x;

            // if they're at the same time return the current value
            if( deltaTicks == 0) {
                return current.y;
            }

            double percentage = (target - previous.x) / (double)deltaTicks;

            //  y=mx+b
            return (current.y - previous.y) * (decimal)percentage + previous.y;
        }
    }
}
