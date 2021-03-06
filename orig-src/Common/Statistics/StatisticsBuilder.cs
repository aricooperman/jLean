﻿/*
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
using System.Globalization;
using System.Linq;
using QuantConnect.Logging;

package com.quantconnect.lean.Statistics
{
    /**
     * The <see cref="StatisticsBuilder"/> class creates summary and rolling statistics from trades, equity and benchmark points
    */
    public static class StatisticsBuilder
    {
        /**
         * Generates the statistics and returns the results
        */
         * @param trades The list of closed trades
         * @param profitLoss Trade record of profits and losses
         * @param pointsEquity The list of daily equity values
         * @param pointsPerformance The list of algorithm performance values
         * @param pointsBenchmark The list of benchmark values
         * @param startingCapital The algorithm starting capital
         * @param totalFees The total fees
         * @param totalTransactions The total number of transactions
        @returns Returns a <see cref="StatisticsResults"/> object
        public static StatisticsResults Generate(
            List<Trade> trades, 
            SortedMap<DateTime, decimal> profitLoss,
            List<ChartPoint> pointsEquity, 
            List<ChartPoint> pointsPerformance, 
            List<ChartPoint> pointsBenchmark, 
            BigDecimal startingCapital, 
            BigDecimal totalFees, 
            int totalTransactions) {
            equity = ChartPointToDictionary(pointsEquity);

            firstDate = equity.Keys.FirstOrDefault().Date;
            lastDate = equity.Keys.LastOrDefault().Date;

            totalPerformance = GetAlgorithmPerformance(firstDate, lastDate, trades, profitLoss, equity, pointsPerformance, pointsBenchmark, startingCapital);
            rollingPerformances = GetRollingPerformances(firstDate, lastDate, trades, profitLoss, equity, pointsPerformance, pointsBenchmark, startingCapital);
            summary = GetSummary(totalPerformance, totalFees, totalTransactions);

            return new StatisticsResults(totalPerformance, rollingPerformances, summary);
        }

        /**
         * Returns the performance of the algorithm in the specified date range
        */
         * @param fromDate The initial date of the range
         * @param toDate The final date of the range
         * @param trades The list of closed trades
         * @param profitLoss Trade record of profits and losses
         * @param equity The list of daily equity values
         * @param pointsPerformance The list of algorithm performance values
         * @param pointsBenchmark The list of benchmark values
         * @param startingCapital The algorithm starting capital
        @returns The algorithm performance
        private static AlgorithmPerformance GetAlgorithmPerformance(
            DateTime fromDate, 
            DateTime toDate, 
            List<Trade> trades, 
            SortedMap<DateTime, decimal> profitLoss, 
            SortedMap<DateTime, decimal> equity, 
            List<ChartPoint> pointsPerformance, 
            List<ChartPoint> pointsBenchmark, 
            BigDecimal startingCapital) {
            periodTrades = trades.Where(x -> x.ExitTime.Date >= fromDate && x.ExitTime < toDate.AddDays(1)).ToList();
            periodProfitLoss = new SortedMap<DateTime, decimal>(profitLoss.Where(x -> x.Key >= fromDate && x.Key.Date < toDate.AddDays(1)).ToDictionary(x -> x.Key, y -> y.Value));
            periodEquity = new SortedMap<DateTime, decimal>(equity.Where(x -> x.Key.Date >= fromDate && x.Key.Date < toDate.AddDays(1)).ToDictionary(x -> x.Key, y -> y.Value));

            listPerformance = new List<double>();
            performance = ChartPointToDictionary(pointsPerformance, fromDate, toDate);
            performance.Values.ToList().ForEach(i -> listPerformance.Add((double)(i / 100)));

            benchmark = ChartPointToDictionary(pointsBenchmark, fromDate, toDate);
            listBenchmark = CreateBenchmarkDifferences(benchmark, periodEquity);
            EnsureSameLength(listPerformance, listBenchmark);

            runningCapital = equity.Count == periodEquity.Count ? startingCapital : periodEquity.Values.FirstOrDefault();

            return new AlgorithmPerformance(periodTrades, periodProfitLoss, periodEquity, listPerformance, listBenchmark, runningCapital);
        }

        /**
         * Returns the rolling performances of the algorithm
        */
         * @param firstDate The first date of the total period
         * @param lastDate The last date of the total period
         * @param trades The list of closed trades
         * @param profitLoss Trade record of profits and losses
         * @param equity The list of daily equity values
         * @param pointsPerformance The list of algorithm performance values
         * @param pointsBenchmark The list of benchmark values
         * @param startingCapital The algorithm starting capital
        @returns A dictionary with the rolling performances
        private static Map<String, AlgorithmPerformance> GetRollingPerformances(
            DateTime firstDate, 
            DateTime lastDate, 
            List<Trade> trades, 
            SortedMap<DateTime, decimal> profitLoss, 
            SortedMap<DateTime, decimal> equity, 
            List<ChartPoint> pointsPerformance, 
            List<ChartPoint> pointsBenchmark, 
            BigDecimal startingCapital) {
            rollingPerformances = new Map<String, AlgorithmPerformance>();
            
            monthPeriods = new[] { 1, 3, 6, 12 };
            foreach (monthPeriod in monthPeriods) {
                ranges = GetPeriodRanges(monthPeriod, firstDate, lastDate);

                foreach (period in ranges) {
                    key = "M" + monthPeriod + "_" + period.EndDate.toString( "yyyyMMdd");
                    periodPerformance = GetAlgorithmPerformance(period.StartDate, period.EndDate, trades, profitLoss, equity, pointsPerformance, pointsBenchmark, startingCapital);
                    rollingPerformances[key] = periodPerformance;
                }
            }

            return rollingPerformances;
        }

        /**
         * Returns a summary of the algorithm performance as a dictionary
        */
        private static Map<String,String> GetSummary(AlgorithmPerformance totalPerformance, BigDecimal totalFees, int totalTransactions) {
            return new Map<String,String> 
            { 
                { "Total Trades", totalTransactions.toString(CultureInfo.InvariantCulture) },
                { "Average Win", Math.Round(totalPerformance.PortfolioStatistics.AverageWinRate.SafeMultiply100(), 2) + "%"  },
                { "Average Loss", Math.Round(totalPerformance.PortfolioStatistics.AverageLossRate.SafeMultiply100(), 2) + "%" },
                { "Compounding Annual Return", Math.Round(totalPerformance.PortfolioStatistics.CompoundingAnnualReturn.SafeMultiply100(), 3) + "%" },
                { "Drawdown", (Math.Round(totalPerformance.PortfolioStatistics.Drawdown.SafeMultiply100(), 3)) + "%" },
                { "Expectancy", Math.Round(totalPerformance.PortfolioStatistics.Expectancy, 3).toString(CultureInfo.InvariantCulture) },
                { "Net Profit", Math.Round(totalPerformance.PortfolioStatistics.TotalNetProfit.SafeMultiply100(), 3) + "%"},
                { "Sharpe Ratio", Math.Round((double)totalPerformance.PortfolioStatistics.SharpeRatio, 3).toString(CultureInfo.InvariantCulture) },
                { "Loss Rate", Math.Round(totalPerformance.PortfolioStatistics.LossRate.SafeMultiply100()) + "%" },
                { "Win Rate", Math.Round(totalPerformance.PortfolioStatistics.WinRate.SafeMultiply100()) + "%" }, 
                { "Profit-Loss Ratio", Math.Round(totalPerformance.PortfolioStatistics.ProfitLossRatio, 2).toString(CultureInfo.InvariantCulture) },
                { "Alpha", Math.Round((double)totalPerformance.PortfolioStatistics.Alpha, 3).toString(CultureInfo.InvariantCulture) },
                { "Beta", Math.Round((double)totalPerformance.PortfolioStatistics.Beta, 3).toString(CultureInfo.InvariantCulture) },
                { "Annual Standard Deviation", Math.Round((double)totalPerformance.PortfolioStatistics.AnnualStandardDeviation, 3).toString(CultureInfo.InvariantCulture) },
                { "Annual Variance", Math.Round((double)totalPerformance.PortfolioStatistics.AnnualVariance, 3).toString(CultureInfo.InvariantCulture) },
                { "Information Ratio", Math.Round((double)totalPerformance.PortfolioStatistics.InformationRatio, 3).toString(CultureInfo.InvariantCulture) },
                { "Tracking Error", Math.Round((double)totalPerformance.PortfolioStatistics.TrackingError, 3).toString(CultureInfo.InvariantCulture) },
                { "Treynor Ratio", Math.Round((double)totalPerformance.PortfolioStatistics.TreynorRatio, 3).toString(CultureInfo.InvariantCulture) },
                { "Total Fees", "$" + totalFees.toString( "0.00") }
            };
        }

        private static BigDecimal SafeMultiply100(this BigDecimal value) {
            static final BigDecimal max = decimal.MaxValue/100m;
            if( value >= max) return decimal.MaxValue;
            return value*100m;
        }

        /**
         * Helper class for rolling statistics
        */
        private class PeriodRange
        {
            internal DateTime StartDate { get; set; }
            internal DateTime EndDate { get; set; }
        }

        // 
        /**
         * Gets a list of date ranges for the requested monthly period
        */
         * The first and last ranges created are partial periods
         * @param periodMonths The number of months in the period (valid inputs are [1, 3, 6, 12])
         * @param firstDate The first date of the total period
         * @param lastDate The last date of the total period
        @returns The list of date ranges
        private static IEnumerable<PeriodRange> GetPeriodRanges(int periodMonths, DateTime firstDate, DateTime lastDate) {
            // get end dates
            date = lastDate.Date;
            endDates = new List<DateTime>();
            do
            {
                endDates.Add(date);
                date = new DateTime(date.Year, date.Month, 1).AddDays(-1);
            } while (date >= firstDate);

            // build period ranges
            ranges = new List<PeriodRange> { new PeriodRange { StartDate = firstDate, EndDate = endDates[endDates.Count - 1] } };
            for (i = endDates.Count - 2; i >= 0; i--) {
                startDate = ranges[ranges.Count - 1].EndDate.AddDays(1).AddMonths(1 - periodMonths);
                if( startDate < firstDate) startDate = firstDate;

                ranges.Add(new PeriodRange
                {
                    StartDate = startDate,
                    EndDate = endDates[i]
                });
            }

            return ranges;
        }

        /**
         * Convert the charting data into an equity array.
        */
         * This is required to convert the equity plot into a usable form for the statistics calculation
         * @param points ChartPoints Array
         * @param fromDate An optional starting date
         * @param toDate An optional ending date
        @returns SortedDictionary of the equity BigDecimal values ordered in time
        private static SortedMap<DateTime, decimal> ChartPointToDictionary(IEnumerable<ChartPoint> points, DateTime? fromDate = null, DateTime? toDate = null ) {
            dictionary = new SortedMap<DateTime, decimal>();

            foreach (point in points) {
                x = Time.UnixTimeStampToDateTime(point.x);

                if( fromDate != null && x.Date < fromDate) continue;
                if( toDate != null && x.Date >= ((DateTime)toDate).AddDays(1)) break;
                    
                dictionary[x] = point.y;
            }

            return dictionary;
        }

        /**
         * Creates a list of benchmark differences for the period
        */
         * @param benchmark The benchmark values
         * @param equity The equity values
        @returns The list of benchmark differences
        private static List<double> CreateBenchmarkDifferences(SortedMap<DateTime, decimal> benchmark, SortedMap<DateTime, decimal> equity) {
            // to find the delta in benchmark for first day, we need to know the price at
            // the opening moment of the day, but since we cannot find this, we cannot find
            // the first benchmark's delta, so we start looking for data in a inexistent day. 
            // If running a short backtest this will skew results, longer backtests will not be affected much
            dtPrevious = new DateTime();

            listBenchmark = new List<double>();

            minDate = equity.Keys.FirstOrDefault().AddDays(-1);
            maxDate = equity.Keys.LastOrDefault();

            // Get benchmark performance array for same period:
            benchmark.Keys.ToList().ForEach(dt =>
            {
                if( dt >= minDate && dt <= maxDate) {
                    BigDecimal previous;
                    if( benchmark.TryGetValue(dtPrevious, out previous) && previous != 0) {
                        deltaBenchmark = (benchmark[dt] - previous) / previous;
                        listBenchmark.Add((double)deltaBenchmark);
                    }
                    else
                    {
                        listBenchmark.Add(0);
                    }
                    dtPrevious = dt;
                }
            });

            return listBenchmark;
        }

        /**
         * Ensures the performance list and benchmark list have the same length, padding with trailing zeros
        */
         * @param listPerformance The performance list
         * @param listBenchmark The benchmark list
        private static void EnsureSameLength(List<double> listPerformance, List<double> listBenchmark) {
            // THIS SHOULD NEVER HAPPEN --> But if it does, log it and fail silently.
            while (listPerformance.Count < listBenchmark.Count) {
                listPerformance.Add(0);
                Log.Trace( "StatisticsBuilder.EnsureSameLength(): Padded Performance");
            }
            while (listPerformance.Count > listBenchmark.Count) {
                listBenchmark.Add(0);
                Log.Trace( "StatisticsBuilder.EnsureSameLength(): Padded Benchmark");
            }
        }

    }
}
