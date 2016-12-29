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
using QuantConnect.Data;
using QuantConnect.Indicators;

package com.quantconnect.lean.Algorithm
{
    public partial class QCAlgorithm
    {
        private Map<String, Chart> _charts = new Map<String, Chart>();
        private Map<String,String> _runtimeStatistics = new Map<String,String>();

        /**
         * Access to the runtime statistics property. User provided statistics.
        */
         *  RuntimeStatistics are displayed in the head banner in live trading
        public Map<String,String> RuntimeStatistics
        {
            get
            {
                return _runtimeStatistics;
            }
        }

        /**
         * Add a Chart object to algorithm collection
        */
         * @param chart Chart object to add to collection.
         * <seealso cref="Plot( String,string,decimal)"/>
        public void AddChart(Chart chart) {
            if( !_charts.ContainsKey(chart.Name)) {
                _charts.Add(chart.Name, chart);
            }
        }

        /**
         * Plot a chart using String series name, with value.
        */
         * @param series Name of the plot series
         * @param value Value to plot
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Plot( String series, BigDecimal value) {
            //By default plot to the primary chart:
            Plot( "Strategy Equity", series, value);
        }


        /**
         * Plot a chart using String series name, with int value. Alias of Plot();
        */
         *  Record( String series, int value)
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Record( String series, int value) {
            Plot(series, value);
        }

        /**
         * Plot a chart using String series name, with double value. Alias of Plot();
        */
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Record( String series, double value) {
            Plot(series, value);
        }

        /**
         * Plot a chart using String series name, with BigDecimal value. Alias of Plot();
        */
         * @param series">
         * @param value">
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Record( String series, BigDecimal value) {
            //By default plot to the primary chart:
            Plot(series, value);
        }

        /**
         * Plot a chart using String series name, with double value.
        */
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Plot( String series, double value) {
            Plot(series, (decimal)value);
        }

        /**
         * Plot a chart using String series name, with int value.
        */
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Plot( String series, int value) {
            Plot(series, (decimal)value);
        }

        /**
        ///Plot a chart using String series name, with float value.
        */
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Plot( String series, float value) {
            Plot(series, (decimal)value);
        }

        /**
         * Plot a chart to String chart name, using String series name, with double value.
        */
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Plot( String chart, String series, double value) {
            Plot(chart, series, (decimal)value);
        }

        /**
         * Plot a chart to String chart name, using String series name, with int value
        */
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Plot( String chart, String series, int value) {
            Plot(chart, series, (decimal)value);
        }

        /**
         * Plot a chart to String chart name, using String series name, with float value
        */
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Plot( String chart, String series, float value) {
            Plot(chart, series, (decimal)value);
        }

        /**
         * Plot a value to a chart of string-chart name, with String series name, and BigDecimal value. If chart does not exist, create it.
        */
         * @param chart Chart name
         * @param series Series name
         * @param value Value of the point
        public void Plot( String chart, String series, BigDecimal value) {
            //Ignore the reserved chart names:
            if( (chart.equals( "Strategy Equity" && series.equals( "Equity") || (chart.equals( "Daily Performance") || (chart.equals( "Meta")) {
                throw new Exception( "Algorithm.Plot(): 'Equity', 'Daily Performance' and 'Meta' are reserved chart names created for all charts.");
            }

            // If we don't have the chart, create it:
            if( !_charts.ContainsKey(chart)) {
                _charts.Add(chart, new Chart(chart)); 
            }

            thisChart = _charts[chart];
            if( !thisChart.Series.ContainsKey(series)) {
                //Number of series in total.
                seriesCount = (from x in _charts.Values select x.Series.Count).Sum();

                if( seriesCount > 10) {
                    Error( "Exceeded maximum series count: Each backtest can have up to 10 series in total.");
                    return;
                }

                //If we don't have the series, create it:
                thisChart.AddSeries(new Series(series, SeriesType.Line, 0, "$"));
            }

            thisSeries = thisChart.Series[series];
            if( thisSeries.Values.Count < 4000 || _liveMode) {
                thisSeries.AddPoint(Time, value, _liveMode);
            }
            else 
            {
                Debug( "Exceeded maximum points per chart, data skipped.");
            }
        }

        /**
         * Plots the value of each indicator on the chart
        */
         * @param chart The chart's name
         * @param indicators The indicatorsto plot
         * <seealso cref="Plot( String,string,decimal)"/>
        public void Plot<T>( String chart, params IndicatorBase<T>[] indicators)
            where T : BaseData
        {
            foreach (indicator in indicators) {
                Plot(chart, indicator.Name, indicator);
            }
        }

        /**
         * Automatically plots each indicator when a new value is available
        */
        public void PlotIndicator<T>( String chart, params IndicatorBase<T>[] indicators)
            where T : BaseData
        {
            foreach (i in indicators) {
                // copy loop variable for usage in closure
                ilocal = i;
                i.Updated += (sender, args) =>
                {
                    Plot(chart, ilocal);
                };
            }
        }

        /**
         * Automatically plots each indicator when a new value is available, optionally waiting for indicator.IsReady to return true
        */
        public void PlotIndicator<T>( String chart, boolean waitForReady, params IndicatorBase<T>[] indicators)
            where T : BaseData
        {
            foreach (i in indicators) {
                // copy loop variable for usage in closure
                ilocal = i;
                i.Updated += (sender, args) =>
                {
                    if( !waitForReady || ilocal.IsReady) {
                        Plot(chart, ilocal);
                    }
                };
            }
        }

        /**
         * Set a runtime statistic for the algorithm. Runtime statistics are shown in the top banner of a live algorithm GUI.
        */
         * @param name Name of your runtime statistic
         * @param value String value of your runtime statistic
         * <seealso cref="LiveMode"/>
        public void SetRuntimeStatistic( String name, String value) {
            //If not set, add it to the dictionary:
            if( !_runtimeStatistics.ContainsKey(name)) {
                _runtimeStatistics.Add(name, value);
            }

            //Set 
            _runtimeStatistics[name] = value;
        }

        /**
         * Set a runtime statistic for the algorithm. Runtime statistics are shown in the top banner of a live algorithm GUI.
        */
         * @param name Name of your runtime statistic
         * @param value Decimal value of your runtime statistic
        public void SetRuntimeStatistic( String name, BigDecimal value) {
            SetRuntimeStatistic(name, value.toString());
        }

        /**
         * Set a runtime statistic for the algorithm. Runtime statistics are shown in the top banner of a live algorithm GUI.
        */
         * @param name Name of your runtime statistic
         * @param value Int value of your runtime statistic
        public void SetRuntimeStatistic( String name, int value) {
            SetRuntimeStatistic(name, value.toString());
        }

        /**
         * Set a runtime statistic for the algorithm. Runtime statistics are shown in the top banner of a live algorithm GUI.
        */
         * @param name Name of your runtime statistic
         * @param value Double value of your runtime statistic
        public void SetRuntimeStatistic( String name, double value) {
            SetRuntimeStatistic(name, value.toString());
        }

        /**
         * Get the chart updates by fetch the recent points added and return for dynamic plotting.
        */
         * @param clearChartData">
        @returns List of chart updates since the last request
         * GetChartUpdates returns the latest updates since previous request.
        public List<Chart> GetChartUpdates( boolean clearChartData = false) {
            updates = _charts.Values.Select(chart -> chart.GetUpdates()).ToList();

            if( clearChartData) {
                // we can clear this data out after getting updates to prevent unnecessary memory usage
                foreach (chart in _charts) {
                    foreach (series in chart.Value.Series) {
                        series.Value.Purge();
                    }
                }
            }
            return updates;
        }

    } // End Partial Algorithm Template - Plotting.

} // End QC Namespace
