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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using MathNet.Numerics.Statistics;
using QuantConnect.Logging;

package com.quantconnect.lean.Statistics
{
    /**
     * Calculate all the statistics required from the backtest, based on the equity curve and the profit loss statement.
    */
     * This is a particularly ugly class and one of the first ones written. It should be thrown out and re-written.
    public class Statistics
    {
        /**
         * Retrieve a static S-P500 Benchmark for the statistics calculations. Update the benchmark once per day.
        */
        public static SortedMap<DateTime, decimal> YahooSPYBenchmark
        {
            get
            {
                benchmark = new SortedMap<DateTime, decimal>();
                url = "http://real-chart.finance.yahoo.com/table.csv?s=SPY&a=11&b=31&c=1997&d=" + (DateTime.Now.Month - 1) + "&e=" + DateTime.Now.Day + "&f=" + DateTime.Now.Year + "&g=d&ignore=.csv";
                using (net = new WebClient()) {
                    net.Proxy = WebRequest.GetSystemWebProxy();
                    data = net.DownloadString(url);
                    first = true;
                    using (sr = new StreamReader(data.ToStream())) {
                        while (sr.Peek() >= 0) {
                            line = sr.ReadLine();
                            if( first) {
                                first = false;
                                continue;
                            }
                            if( line == null ) continue;
                            csv = line.split(',');
                            benchmark.Add(DateTime.Parse(csv[0]), new BigDecimal( csv[6], CultureInfo.InvariantCulture));
                        }
                    }
                }
                return benchmark;
            }
        }

        /**
         * Convert the charting data into an equity array.
        */
         * This is required to convert the equity plot into a usable form for the statistics calculation
         * @param points ChartPoints Array
        @returns SortedDictionary of the equity BigDecimal values ordered in time
        private static SortedMap<DateTime, decimal> ChartPointToDictionary(IEnumerable<ChartPoint> points) {
            dictionary = new SortedMap<DateTime, decimal>();
            try
            {
                foreach (point in points) {
                    x = Time.UnixTimeStampToDateTime(point.x);
                    if( !dictionary.ContainsKey(x)) {
                        dictionary.Add(x, point.y);
                    }
                    else
                    {
                        dictionary[x] = point.y;
                    }
                }
            }
            catch (Exception err) {
                Log.Error(err);
            }
            return dictionary;
        }


        /**
         * Run a full set of orders and return a Dictionary of statistics.
        */
         * @param pointsEquity Equity value over time.
         * @param profitLoss profit loss from trades
         * @param pointsPerformance"> Daily performance
         * @param unsortedBenchmark"> Benchmark data as dictionary. Data does not need to be ordered
         * @param startingCash Amount of starting cash in USD 
         * @param totalFees The total fees incurred over the life time of the algorithm
         * @param totalTrades Total number of orders executed.
         * @param tradingDaysPerYear Number of trading days per year
        @returns Statistics Array, Broken into Annual Periods
        public static Map<String,String> Generate(IEnumerable<ChartPoint> pointsEquity, 
            SortedMap<DateTime, decimal> profitLoss, 
            IEnumerable<ChartPoint> pointsPerformance, 
            Map<DateTime, decimal> unsortedBenchmark, 
            BigDecimal startingCash, 
            BigDecimal totalFees, 
            BigDecimal totalTrades, 
            double tradingDaysPerYear = 252
            ) {
            //Initialise the response:
            double riskFreeRate = 0;
            BigDecimal totalClosedTrades = 0;
            BigDecimal totalWins = 0;
            BigDecimal totalLosses = 0;
            BigDecimal averageWin = 0;
            BigDecimal averageLoss = 0;
            BigDecimal averageWinRatio = 0;
            BigDecimal winRate = 0;
            BigDecimal lossRate = 0;
            BigDecimal totalNetProfit = 0;
            double fractionOfYears = 1;
            BigDecimal profitLossValue = 0, runningCash = startingCash;
            BigDecimal algoCompoundingPerformance = 0;
            BigDecimal finalBenchmarkCash = 0;
            BigDecimal benchCompoundingPerformance = 0;
            years = new List<Integer>();
            annualTrades = new SortedMap<Integer,Integer>();
            annualWins = new SortedMap<Integer,Integer>();
            annualLosses = new SortedMap<Integer,Integer>();
            annualLossTotal = new SortedMap<Integer, decimal>();
            annualWinTotal = new SortedMap<Integer, decimal>();
            annualNetProfit = new SortedMap<Integer, decimal>();
            statistics = new Map<String,String>();
            dtPrevious = new DateTime();
            listPerformance = new List<double>();
            listBenchmark = new List<double>();
            equity = new SortedMap<DateTime, decimal>();
            performance = new SortedMap<DateTime, decimal>();
            SortedMap<DateTime, decimal>  benchmark = null;
            try
            {
                //Get array versions of the performance:
                performance = ChartPointToDictionary(pointsPerformance);
                equity = ChartPointToDictionary(pointsEquity);
                performance.Values.ToList().ForEach(i -> listPerformance.Add((double)(i / 100)));
                benchmark = new SortedMap<DateTime, decimal>(unsortedBenchmark);

                // to find the delta in benchmark for first day, we need to know the price at the opening
                // moment of the day, but since we cannot find this, we cannot find the first benchmark's delta,
                // so we pad it with Zero. If running a short backtest this will skew results, longer backtests
                // will not be affected much
                listBenchmark.Add(0);

                //Get benchmark performance array for same period:
                benchmark.Keys.ToList().ForEach(dt =>
                {
                    if( dt >= equity.Keys.FirstOrDefault().AddDays(-1) && dt < equity.Keys.LastOrDefault()) {
                        BigDecimal previous;
                        if( benchmark.TryGetValue(dtPrevious, out previous) && previous != 0) {
                            deltaBenchmark = (benchmark[dt] - previous)/previous;
                            listBenchmark.Add((double)(deltaBenchmark));
                        }
                        else
                        {
                            listBenchmark.Add(0);
                        }
                        dtPrevious = dt;
                    }
                });

                // TODO : if these lists are required to be the same length then we should create structure to pair the values, this way, by contract it will be enforced.

                //THIS SHOULD NEVER HAPPEN --> But if it does, log it and fail silently.
                while (listPerformance.Count < listBenchmark.Count) {
                    listPerformance.Add(0);
                    Log.Error( "Statistics.Generate(): Padded Performance");
                }
                while (listPerformance.Count > listBenchmark.Count) {
                    listBenchmark.Add(0);
                    Log.Error( "Statistics.Generate(): Padded Benchmark");
                }
            }
            catch (Exception err) {
                Log.Error(err, "Dic-Array Convert:");
            }

            try
            {
                //Number of years in this dataset:
                fractionOfYears = (equity.Keys.LastOrDefault() - equity.Keys.FirstOrDefault()).TotalDays / 365;
            }
            catch (Exception err) {
                Log.Error(err, "Fraction of Years:");
            }

            try
            {
                if( benchmark != null ) {
                    algoCompoundingPerformance = CompoundingAnnualPerformance(startingCash, equity.Values.LastOrDefault(), (decimal) fractionOfYears);
                    finalBenchmarkCash = ((benchmark.Values.Last() - benchmark.Values.First())/benchmark.Values.First())*startingCash;
                    benchCompoundingPerformance = CompoundingAnnualPerformance(startingCash, finalBenchmarkCash, (decimal) fractionOfYears);
                }
            }
            catch (Exception err) {
                Log.Error(err, "Compounding:");
            }

            try
            {
                //Run over each equity day:
                foreach (closedTrade in profitLoss.Keys) {
                    profitLossValue = profitLoss[closedTrade];

                    //Check if this date is in the "years" array:
                    year = closedTrade.Year;
                    if( !years.Contains(year)) {
                        //Initialise a new year holder:
                        years.Add(year);
                        annualTrades.Add(year, 0);
                        annualWins.Add(year, 0);
                        annualWinTotal.Add(year, 0);
                        annualLosses.Add(year, 0);
                        annualLossTotal.Add(year, 0);
                    }

                    //Add another trade:
                    annualTrades[year]++;

                    //Profit loss tracking:
                    if( profitLossValue > 0) {
                        annualWins[year]++;
                        annualWinTotal[year] += profitLossValue / runningCash;
                    }
                    else
                    {
                        annualLosses[year]++;
                        annualLossTotal[year] += profitLossValue / runningCash;
                    }

                    //Increment the cash:
                    runningCash += profitLossValue;
                }

                //Get the annual percentage of profit and loss:
                foreach (year in years) {
                    annualNetProfit[year] = (annualWinTotal[year] + annualLossTotal[year]);
                }

                //Sum the totals:
                try
                {
                    if( profitLoss.Keys.Count > 0) {
                        totalClosedTrades = annualTrades.Values.Sum();
                        totalWins = annualWins.Values.Sum();
                        totalLosses = annualLosses.Values.Sum();
                        totalNetProfit = (equity.Values.LastOrDefault() / startingCash) - 1;

                        //-> Handle Div/0 Errors
                        if( totalWins == 0) {
                            averageWin = 0;
                        }
                        else
                        {
                            averageWin = annualWinTotal.Values.Sum() / totalWins;
                        }
                        if( totalLosses == 0) {
                            averageLoss = 0;
                            averageWinRatio = 0;
                        }
                        else
                        {
                            averageLoss = annualLossTotal.Values.Sum() / totalLosses;
                            averageWinRatio = Math.Abs(averageWin / averageLoss);
                        }
                        if( totalTrades == 0) {
                            winRate = 0;
                            lossRate = 0;
                        }
                        else
                        {
                            winRate = Math.Round(totalWins / totalClosedTrades, 5);
                            lossRate = Math.Round(totalLosses / totalClosedTrades, 5);
                        }
                    }

                }
                catch (Exception err) {
                    Log.Error(err, "Second Half:");
                }

                profitLossRatio = ProfitLossRatio(averageWin, averageLoss);
                profitLossRatioHuman = profitLossRatio.toString(CultureInfo.InvariantCulture);
                if( profitLossRatio == -1) profitLossRatioHuman = "0";

                //Add the over all results first, break down by year later:
                statistics = new Map<String,String> { 
                    { "Total Trades", Math.Round(totalTrades, 0).toString(CultureInfo.InvariantCulture) },
                    { "Average Win", Math.Round(averageWin * 100, 2) + "%"  },
                    { "Average Loss", Math.Round(averageLoss * 100, 2) + "%" },
                    { "Compounding Annual Return", Math.Round(algoCompoundingPerformance * 100, 3) + "%" },
                    { "Drawdown", (DrawdownPercent(equity, 3) * 100) + "%" },
                    { "Expectancy", Math.Round((winRate * averageWinRatio) - (lossRate), 3).toString(CultureInfo.InvariantCulture) },
                    { "Net Profit", Math.Round(totalNetProfit * 100, 3) + "%"},
                    { "Sharpe Ratio", Math.Round(SharpeRatio(listPerformance, riskFreeRate), 3).toString(CultureInfo.InvariantCulture) },
                    { "Loss Rate", Math.Round(lossRate * 100) + "%" },
                    { "Win Rate", Math.Round(winRate * 100) + "%" }, 
                    { "Profit-Loss Ratio", profitLossRatioHuman },
                    { "Alpha", Math.Round(Alpha(listPerformance, listBenchmark, riskFreeRate), 3).toString(CultureInfo.InvariantCulture) },
                    { "Beta", Math.Round(Beta(listPerformance, listBenchmark), 3).toString(CultureInfo.InvariantCulture) },
                    { "Annual Standard Deviation", Math.Round(AnnualStandardDeviation(listPerformance, tradingDaysPerYear), 3).toString(CultureInfo.InvariantCulture) },
                    { "Annual Variance", Math.Round(AnnualVariance(listPerformance, tradingDaysPerYear), 3).toString(CultureInfo.InvariantCulture) },
                    { "Information Ratio", Math.Round(InformationRatio(listPerformance, listBenchmark), 3).toString(CultureInfo.InvariantCulture) },
                    { "Tracking Error", Math.Round(TrackingError(listPerformance, listBenchmark), 3).toString(CultureInfo.InvariantCulture) },
                    { "Treynor Ratio", Math.Round(TreynorRatio(listPerformance, listBenchmark, riskFreeRate), 3).toString(CultureInfo.InvariantCulture) },
                    { "Total Fees", "$" + totalFees.toString( "0.00") }
                };
            }
            catch (Exception err) {
                Log.Error(err);
            }
            return statistics;
        }

        /**
         * Return profit loss ratio safely avoiding divide by zero errors.
        */
         * @param averageWin">
         * @param averageLoss">
        @returns 
        public static BigDecimal ProfitLossRatio( BigDecimal averageWin, BigDecimal averageLoss) {
            if( averageLoss == 0) return -1;
            return Math.Round(averageWin / Math.Abs(averageLoss), 2);
        }

        /**
         * Drawdown maximum percentage.
        */
         * @param equityOverTime">
         * @param rounding">
        @returns 
        public static BigDecimal DrawdownPercent(SortedMap<DateTime, decimal> equityOverTime, int rounding = 2) {
            dd = BigDecimal.ZERO;
            try
            {
                lPrices = equityOverTime.Values.ToList();
                lDrawdowns = new List<decimal>();
                high = lPrices[0];
                foreach (price in lPrices) {
                    if( price >= high) high = price;
                    lDrawdowns.Add((price/high) - 1);
                }
                dd = Math.Round(Math.Abs(lDrawdowns.Min()), rounding);
            }
            catch (Exception err) {
                Log.Error(err);
            }
            return dd;
        }

        /**
         * Drawdown maximum value
        */
         * @param equityOverTime Array of portfolio value over time.
         * @param rounding Round the drawdown statistics.
        @returns Draw down percentage over period.
        public static BigDecimal DrawdownValue(SortedMap<DateTime, decimal> equityOverTime, int rounding = 2) {
            //Initialise:
            priceMaximum = 0;
            previousMinimum = 0;
            previousMaximum = 0;

            try
            {
                lPrices = equityOverTime.Values.ToList();

                for (id = 0; id < lPrices.Count; id++) {
                    if( lPrices[id] >= lPrices[priceMaximum]) {
                        priceMaximum = id;
                    }
                    else
                    {
                        if( (lPrices[priceMaximum] - lPrices[id]) > (lPrices[previousMaximum] - lPrices[previousMinimum])) {
                            previousMaximum = priceMaximum;
                            previousMinimum = id;
                        }
                    }
                }
                return Math.Round((lPrices[previousMaximum] - lPrices[previousMinimum]), rounding);
            }
            catch (Exception err) {
                Log.Error(err);
            }
            return 0;
        } // End Drawdown:


        /**
         * Annual compounded returns statistic based on the final-starting capital and years.
        */
         * @param startingCapital Algorithm starting capital
         * @param finalCapital Algorithm final capital
         * @param years Years trading
        @returns Decimal fraction for annual compounding performance
        public static BigDecimal CompoundingAnnualPerformance( BigDecimal startingCapital, BigDecimal finalCapital, BigDecimal years) {
            return (Math.Pow((double)finalCapital / (double)startingCapital, (1 / (double)years)) - 1).SafeDecimalCast();
        }

        /**
         * Annualized return statistic calculated as an average of daily trading performance multiplied by the number of trading days per year.
        */
         * @param performance Dictionary collection of double performance values
         * @param tradingDaysPerYear Trading days per year for the assets in portfolio
         * May be unaccurate for forex algorithms with more trading days in a year
        @returns Double annual performance percentage
        public static double AnnualPerformance(List<double> performance, double tradingDaysPerYear = 252) {
            return performance.Average() * tradingDaysPerYear;
        }

        /**
         * Annualized variance statistic calculation using the daily performance variance and trading days per year.
        */
         * @param performance">
         * @param tradingDaysPerYear">
         * Invokes the variance extension in the MathNet Statistics class
        @returns Annual variance value
        public static double AnnualVariance(List<double> performance, double tradingDaysPerYear = 252) {
            return (performance.Variance())*tradingDaysPerYear;
        }

        /**
         * Annualized standard deviation
        */
         * @param performance Collection of double values for daily performance
         * @param tradingDaysPerYear Number of trading days for the assets in portfolio to get annualize standard deviation.
         * 
         *     Invokes the variance extension in the MathNet Statistics class.
         *     Feasibly the trading days per year can be fetched from the dictionary of performance which includes the date-times to get the range; if is more than 1 year data.
         * 
        @returns Value for annual standard deviation
        public static double AnnualStandardDeviation(List<double> performance, double tradingDaysPerYear = 252) {
            return Math.Sqrt(performance.Variance() * tradingDaysPerYear);
        }
        
        /**
         * Algorithm "beta" statistic - the covariance between the algorithm and benchmark performance, divided by benchmark's variance
        */
         * @param algoPerformance Collection of double values for algorithm daily performance.
         * @param benchmarkPerformance Collection of double benchmark daily performance values.
         * Invokes the variance and covariance extensions in the MathNet Statistics class
        @returns Value for beta
        public static double Beta(List<double> algoPerformance, List<double> benchmarkPerformance) {
            return algoPerformance.Covariance(benchmarkPerformance) / benchmarkPerformance.Variance();
        }

        /**
         * Algorithm "Alpha" statistic - abnormal returns over the risk free rate and the relationshio (beta) with the benchmark returns.
        */
         * @param algoPerformance Collection of double algorithm daily performance values.
         * @param benchmarkPerformance Collection of double benchmark daily performance values.
         * @param riskFreeRate Risk free rate of return for the T-Bonds.
        @returns Value for alpha
        public static double Alpha(List<double> algoPerformance, List<double> benchmarkPerformance, double riskFreeRate) {
            return AnnualPerformance(algoPerformance) - (riskFreeRate + Beta(algoPerformance, benchmarkPerformance) * (AnnualPerformance(benchmarkPerformance) - riskFreeRate));
        }

        /**
         * Tracking error volatility (TEV) statistic - a measure of how closely a portfolio follows the index to which it is benchmarked
        */
         * If algo = benchmark, TEV = 0
         * @param algoPerformance Double collection of algorithm daily performance values
         * @param benchmarkPerformance Double collection of benchmark daily performance values
        @returns Value for tracking error
        public static double TrackingError(List<double> algoPerformance, List<double> benchmarkPerformance) {
            return Math.Sqrt(AnnualVariance(algoPerformance) - 2 * Correlation.Pearson(algoPerformance, benchmarkPerformance) * AnnualStandardDeviation(algoPerformance) * AnnualStandardDeviation(benchmarkPerformance) + AnnualVariance(benchmarkPerformance));
        }

        
        /**
         * Information ratio - risk adjusted return
        */
         * @param algoPerformance Collection of doubles for the daily algorithm daily performance
         * @param benchmarkPerformance Collection of doubles for the benchmark daily performance
         * (risk = tracking error volatility, a volatility measures that considers the volatility of both algo and benchmark)
         * <seealso cref="TrackingError"/>
        @returns Value for information ratio
        public static double InformationRatio(List<double> algoPerformance, List<double> benchmarkPerformance) {
            return (AnnualPerformance(algoPerformance) - AnnualPerformance(benchmarkPerformance)) / (TrackingError(algoPerformance, benchmarkPerformance));
        }

        /**
         * Sharpe ratio with respect to risk free rate: measures excess of return per unit of risk.
        */
         * With risk defined as the algorithm's volatility
         * @param algoPerformance Collection of double values for the algorithm daily performance
         * @param riskFreeRate">
        @returns Value for sharpe ratio
        public static double SharpeRatio(List<double> algoPerformance, double riskFreeRate) {
            return (AnnualPerformance(algoPerformance) - riskFreeRate) / (AnnualStandardDeviation(algoPerformance));
        }

        /**
         * Treynor ratio statistic is a measurement of the returns earned in excess of that which could have been earned on an investment that has no diversifiable risk
        */
         * @param algoPerformance Collection of double algorithm daily performance values
         * @param benchmarkPerformance Collection of double benchmark daily performance values
         * @param riskFreeRate Risk free rate of return
        @returns double Treynor ratio
        public static double TreynorRatio(List<double> algoPerformance, List<double> benchmarkPerformance, double riskFreeRate) {
            return (AnnualPerformance(algoPerformance) - riskFreeRate) / (Beta(algoPerformance, benchmarkPerformance));
        }

    } // End of Statistics

} // End of Namespace
