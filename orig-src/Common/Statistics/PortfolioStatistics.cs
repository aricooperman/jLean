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
using MathNet.Numerics.Statistics;

package com.quantconnect.lean.Statistics
{
    /// <summary>
    /// The <see cref="PortfolioStatistics"/> class represents a set of statistics calculated from equity and benchmark samples
    /// </summary>
    public class PortfolioStatistics
    {
        private static final BigDecimal RiskFreeRate = 0;

        /// <summary>
        /// The average rate of return for winning trades
        /// </summary>
        public BigDecimal AverageWinRate { get; private set; }

        /// <summary>
        /// The average rate of return for losing trades
        /// </summary>
        public BigDecimal AverageLossRate { get; private set; }

        /// <summary>
        /// The ratio of the average win rate to the average loss rate
        /// </summary>
        /// <remarks>If the average loss rate is zero, ProfitLossRatio is set to 0</remarks>
        public BigDecimal ProfitLossRatio { get; private set; }

        /// <summary>
        /// The ratio of the number of winning trades to the total number of trades
        /// </summary>
        /// <remarks>If the total number of trades is zero, WinRate is set to zero</remarks>
        public BigDecimal WinRate { get; private set; }

        /// <summary>
        /// The ratio of the number of losing trades to the total number of trades
        /// </summary>
        /// <remarks>If the total number of trades is zero, LossRate is set to zero</remarks>
        public BigDecimal LossRate { get; private set; }

        /// <summary>
        /// The expected value of the rate of return
        /// </summary>
        public BigDecimal Expectancy { get; private set; }

        /// <summary>
        /// Annual compounded returns statistic based on the final-starting capital and years.
        /// </summary>
        /// <remarks>Also known as Compound Annual Growth Rate (CAGR)</remarks>
        public BigDecimal CompoundingAnnualReturn { get; private set; }

        /// <summary>
        /// Drawdown maximum percentage.
        /// </summary>
        public BigDecimal Drawdown { get; private set; }

        /// <summary>
        /// The total net profit percentage.
        /// </summary>
        public BigDecimal TotalNetProfit { get; private set; }

        /// <summary>
        /// Sharpe ratio with respect to risk free rate: measures excess of return per unit of risk.
        /// </summary>
        /// <remarks>With risk defined as the algorithm's volatility</remarks>
        public BigDecimal SharpeRatio { get; private set; }

        /// <summary>
        /// Algorithm "Alpha" statistic - abnormal returns over the risk free rate and the relationshio (beta) with the benchmark returns.
        /// </summary>
        public BigDecimal Alpha { get; private set; }

        /// <summary>
        /// Algorithm "beta" statistic - the covariance between the algorithm and benchmark performance, divided by benchmark's variance
        /// </summary>
        public BigDecimal Beta { get; private set; }

        /// <summary>
        /// Annualized standard deviation
        /// </summary>
        public BigDecimal AnnualStandardDeviation { get; private set; }

        /// <summary>
        /// Annualized variance statistic calculation using the daily performance variance and trading days per year.
        /// </summary>
        public BigDecimal AnnualVariance { get; private set; }

        /// <summary>
        /// Information ratio - risk adjusted return
        /// </summary>
        /// <remarks>(risk = tracking error volatility, a volatility measures that considers the volatility of both algo and benchmark)</remarks>
        public BigDecimal InformationRatio { get; private set; }

        /// <summary>
        /// Tracking error volatility (TEV) statistic - a measure of how closely a portfolio follows the index to which it is benchmarked
        /// </summary>
        /// <remarks>If algo = benchmark, TEV = 0</remarks>
        public BigDecimal TrackingError { get; private set; }

        /// <summary>
        /// Treynor ratio statistic is a measurement of the returns earned in excess of that which could have been earned on an investment that has no diversifiable risk
        /// </summary>
        public BigDecimal TreynorRatio { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioStatistics"/> class
        /// </summary>
        /// <param name="profitLoss">Trade record of profits and losses</param>
        /// <param name="equity">The list of daily equity values</param>
        /// <param name="listPerformance">The list of algorithm performance values</param>
        /// <param name="listBenchmark">The list of benchmark values</param>
        /// <param name="startingCapital">The algorithm starting capital</param>
        /// <param name="tradingDaysPerYear">The number of trading days per year</param>
        public PortfolioStatistics(
            SortedMap<DateTime, decimal> profitLoss,
            SortedMap<DateTime, decimal> equity,
            List<double> listPerformance, 
            List<double> listBenchmark, 
            BigDecimal startingCapital, 
            int tradingDaysPerYear = 252) {
            if( startingCapital == 0) return;

            runningCapital = startingCapital;
            totalProfit = 0m;
            totalLoss = 0m;
            totalWins = 0;
            totalLosses = 0;
            foreach (pair in profitLoss) {
                tradeProfitLoss = pair.Value;

                if( tradeProfitLoss > 0) {
                    totalProfit += tradeProfitLoss / runningCapital;
                    totalWins++;
                }
                else
                {
                    totalLoss += tradeProfitLoss / runningCapital;
                    totalLosses++;
                }

                runningCapital += tradeProfitLoss;
            }

            AverageWinRate = totalWins == 0 ? 0 : totalProfit / totalWins;
            AverageLossRate = totalLosses == 0 ? 0 : totalLoss / totalLosses;
            ProfitLossRatio = AverageLossRate == 0 ? 0 : AverageWinRate / Math.Abs(AverageLossRate);

            WinRate = profitLoss.Count == 0 ? 0 : (decimal)totalWins / profitLoss.Count;
            LossRate = profitLoss.Count == 0 ? 0 : (decimal)totalLosses / profitLoss.Count;
            Expectancy = WinRate * ProfitLossRatio - LossRate;

            if( profitLoss.Count > 0) {
                TotalNetProfit = (equity.Values.LastOrDefault() / startingCapital) - 1;
            }

            fractionOfYears = (decimal)(equity.Keys.LastOrDefault() - equity.Keys.FirstOrDefault()).TotalDays / 365;
            CompoundingAnnualReturn = CompoundingAnnualPerformance(startingCapital, equity.Values.LastOrDefault(), fractionOfYears);

            Drawdown = DrawdownPercent(equity, 3);

            AnnualVariance = GetAnnualVariance(listPerformance, tradingDaysPerYear);
            AnnualStandardDeviation = (decimal)Math.Sqrt((double)AnnualVariance);

            annualPerformance = GetAnnualPerformance(listPerformance, tradingDaysPerYear);
            SharpeRatio = AnnualStandardDeviation == 0 ? 0 : (annualPerformance - RiskFreeRate) / AnnualStandardDeviation;

            benchmarkVariance = listBenchmark.Variance();
            Beta = benchmarkVariance.IsNaNOrZero() ? 0 : (decimal)(listPerformance.Covariance(listBenchmark) / benchmarkVariance);

            Alpha = Beta == 0 ? 0 : annualPerformance - (RiskFreeRate + Beta * (GetAnnualPerformance(listBenchmark, tradingDaysPerYear) - RiskFreeRate));

            correlation = Correlation.Pearson(listPerformance, listBenchmark);
            benchmarkAnnualVariance = benchmarkVariance * tradingDaysPerYear;
            TrackingError = correlation.IsNaNOrZero() || benchmarkAnnualVariance.IsNaNOrZero() ? 0 :
                (decimal)Math.Sqrt((double)AnnualVariance - 2 * correlation * (double)AnnualStandardDeviation * Math.Sqrt(benchmarkAnnualVariance) + benchmarkAnnualVariance);

            InformationRatio = TrackingError == 0 ? 0 : (annualPerformance - GetAnnualPerformance(listBenchmark, tradingDaysPerYear)) / TrackingError;

            TreynorRatio = Beta == 0 ? 0 : (annualPerformance - RiskFreeRate) / Beta;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioStatistics"/> class
        /// </summary>
        public PortfolioStatistics() {
        }

        /// <summary>
        /// Annual compounded returns statistic based on the final-starting capital and years.
        /// </summary>
        /// <param name="startingCapital">Algorithm starting capital</param>
        /// <param name="finalCapital">Algorithm final capital</param>
        /// <param name="years">Years trading</param>
        /// <returns>Decimal fraction for annual compounding performance</returns>
        private static BigDecimal CompoundingAnnualPerformance( BigDecimal startingCapital, BigDecimal finalCapital, BigDecimal years) {
            return (years == 0 ? 0d : Math.Pow((double)finalCapital / (double)startingCapital, 1 / (double)years) - 1).SafeDecimalCast();
        }

        /// <summary>
        /// Drawdown maximum percentage.
        /// </summary>
        /// <param name="equityOverTime">The list of daily equity values</param>
        /// <param name="rounding">The number of BigDecimal places to round the result</param>
        /// <returns>The drawdown percentage</returns>
        private static BigDecimal DrawdownPercent(SortedMap<DateTime, decimal> equityOverTime, int rounding = 2) {
            prices = equityOverTime.Values.ToList();
            if( prices.Count == 0) return 0;

            drawdowns = new List<decimal>();
            high = prices[0];
            foreach (price in prices) {
                if( price > high) high = price;
                if( high > 0) drawdowns.Add(price / high - 1);
            }

            return Math.Round(Math.Abs(drawdowns.Min()), rounding);
        }

        /// <summary>
        /// Annualized return statistic calculated as an average of daily trading performance multiplied by the number of trading days per year.
        /// </summary>
        /// <param name="performance">Dictionary collection of double performance values</param>
        /// <param name="tradingDaysPerYear">Trading days per year for the assets in portfolio</param>
        /// <remarks>May be unaccurate for forex algorithms with more trading days in a year</remarks>
        /// <returns>Double annual performance percentage</returns>
        private static BigDecimal GetAnnualPerformance(List<double> performance, int tradingDaysPerYear = 252) {
            return (decimal)performance.Average() * tradingDaysPerYear;
        }

        /// <summary>
        /// Annualized variance statistic calculation using the daily performance variance and trading days per year.
        /// </summary>
        /// <param name="performance"></param>
        /// <param name="tradingDaysPerYear"></param>
        /// <remarks>Invokes the variance extension in the MathNet Statistics class</remarks>
        /// <returns>Annual variance value</returns>
        private static BigDecimal GetAnnualVariance(List<double> performance, int tradingDaysPerYear = 252) {
            variance = performance.Variance();
            return variance.IsNaNOrZero() ? 0 : (decimal)variance * tradingDaysPerYear;
        }

    }
}
