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

//using MathNet.Numerics.Statistics;

package com.quantconnect.lean.statistics;

import java.math.BigDecimal;
import java.util.SortedMap;

/**
 * The <see cref="PortfolioStatistics"/> class represents a set of statistics calculated from equity and benchmark samples
 */
public class PortfolioStatistics {
    
    private static final BigDecimal RiskFreeRate = BigDecimal.ZERO;

    /**
     * The average rate of return for winning trades
     */
    private BigDecimal AverageWinRate;

    /**
     * The average rate of return for losing trades
     */
    private BigDecimal AverageLossRate;

    /**
     * The ratio of the average win rate to the average loss rate
     * If the average loss rate is zero, ProfitLossRatio is set to 0
     */
    private BigDecimal ProfitLossRatio;

    /**
     * The ratio of the number of winning trades to the total number of trades
     * If the total number of trades is zero, WinRate is set to zero
     */
    private BigDecimal WinRate;

    /**
     * The ratio of the number of losing trades to the total number of trades
     * If the total number of trades is zero, LossRate is set to zero
     */
    private BigDecimal LossRate;

    /**
     * The expected value of the rate of return
     */
    private BigDecimal Expectancy;

    /**
     * Annual compounded returns statistic based on the final-starting capital and years.
     * Also known as Compound Annual Growth Rate (CAGR)
     */
    private BigDecimal CompoundingAnnualReturn;

    /**
     * Drawdown maximum percentage.
     */
    private BigDecimal Drawdown;

    /**
     * The total net profit percentage.
     */
    private BigDecimal TotalNetProfit;

    /**
     * Sharpe ratio with respect to risk free rate: measures excess of return per unit of risk.
     * With risk defined as the algorithm's volatility
     */
    private BigDecimal SharpeRatio;

    /**
     * Algorithm "Alpha" statistic - abnormal returns over the risk free rate and the relationshio (beta) with the benchmark returns.
     */
    private BigDecimal Alpha;

    /**
     * Algorithm "beta" statistic - the covariance between the algorithm and benchmark performance, divided by benchmark's variance
     */
    private BigDecimal Beta;

    /**
     * Annualized standard deviation
     */
    private BigDecimal AnnualStandardDeviation;

    /**
     * Annualized variance statistic calculation using the daily performance variance and trading days per year.
     */
    private BigDecimal AnnualVariance;

    /**
     * Information ratio - risk adjusted return
     * (risk = tracking error volatility, a volatility measures that considers the volatility of both algo and benchmark)
     */
    private BigDecimal InformationRatio;

    /**
     * Tracking error volatility (TEV) statistic - a measure of how closely a portfolio follows the index to which it is benchmarked
     * If algo = benchmark, TEV = 0
     */
    private BigDecimal TrackingError;

    /**
     * Treynor ratio statistic is a measurement of the returns earned in excess of that which could have been earned on an investment that has no diversifiable risk
     */
    private BigDecimal TreynorRatio;


    /**
     * Initializes a new instance of the <see cref="PortfolioStatistics"/> class
     * @param profitLoss Trade record of profits and losses
     * @param equity The list of daily equity values
     * @param listPerformance The list of algorithm performance values
     * @param listBenchmark The list of benchmark values
     * @param startingCapital The algorithm starting capital
     * @param tradingDaysPerYear The number of trading days per year
     */
    public PortfolioStatistics(
            SortedMap<DateTime, BigDecimal> profitLoss,
            SortedMap<DateTime, BigDecimal> equity,
            List<double> listPerformance, 
            List<double> listBenchmark, 
            BigDecimal startingCapital, 
            int tradingDaysPerYear = 252 ) {
        
        if( startingCapital == 0 ) 
            return;

        runningCapital = startingCapital;
        totalProfit = BigDecimal.ZERO;
        totalLoss = BigDecimal.ZERO;
        totalWins = 0;
        totalLosses = 0;
        for( pair : profitLoss ) {
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

        WinRate = profitLoss.Count == 0 ? 0 : (BigDecimal)totalWins / profitLoss.Count;
        LossRate = profitLoss.Count == 0 ? 0 : (BigDecimal)totalLosses / profitLoss.Count;
        Expectancy = WinRate * ProfitLossRatio - LossRate;

        if( profitLoss.Count > 0) {
            TotalNetProfit = (equity.Values.LastOrDefault() / startingCapital) - 1;
        }

        fractionOfYears = (BigDecimal)(equity.Keys.LastOrDefault() - equity.Keys.FirstOrDefault()).TotalDays / 365;
        CompoundingAnnualReturn = CompoundingAnnualPerformance(startingCapital, equity.Values.LastOrDefault(), fractionOfYears);

        Drawdown = DrawdownPercent(equity, 3);

        AnnualVariance = GetAnnualVariance(listPerformance, tradingDaysPerYear);
        AnnualStandardDeviation = (BigDecimal)Math.Sqrt((double)AnnualVariance);

        annualPerformance = GetAnnualPerformance(listPerformance, tradingDaysPerYear);
        SharpeRatio = AnnualStandardDeviation == 0 ? 0 : (annualPerformance - RiskFreeRate) / AnnualStandardDeviation;

        benchmarkVariance = listBenchmark.Variance();
        Beta = benchmarkVariance.IsNaNOrZero() ? 0 : (BigDecimal)(listPerformance.Covariance(listBenchmark) / benchmarkVariance);

        Alpha = Beta == 0 ? 0 : annualPerformance - (RiskFreeRate + Beta * (GetAnnualPerformance(listBenchmark, tradingDaysPerYear) - RiskFreeRate));

        correlation = Correlation.Pearson(listPerformance, listBenchmark);
        benchmarkAnnualVariance = benchmarkVariance * tradingDaysPerYear;
        TrackingError = correlation.IsNaNOrZero() || benchmarkAnnualVariance.IsNaNOrZero() ? 0 :
            (BigDecimal)Math.Sqrt((double)AnnualVariance - 2 * correlation * (double)AnnualStandardDeviation * Math.Sqrt(benchmarkAnnualVariance) + benchmarkAnnualVariance);

        InformationRatio = TrackingError == 0 ? 0 : (annualPerformance - GetAnnualPerformance(listBenchmark, tradingDaysPerYear)) / TrackingError;

        TreynorRatio = Beta == 0 ? 0 : (annualPerformance - RiskFreeRate) / Beta;
    }

    /**
     * Initializes a new instance of the <see cref="PortfolioStatistics"/> class
     */
    public PortfolioStatistics() { }

    
    public BigDecimal getAverageWinRate() {
        return AverageWinRate;
    }

    public BigDecimal getAverageLossRate() {
        return AverageLossRate;
    }

    public BigDecimal getProfitLossRatio() {
        return ProfitLossRatio;
    }

    public BigDecimal getWinRate() {
        return WinRate;
    }

    public BigDecimal getLossRate() {
        return LossRate;
    }

    public BigDecimal getExpectancy() {
        return Expectancy;
    }

    public BigDecimal getCompoundingAnnualReturn() {
        return CompoundingAnnualReturn;
    }

    public BigDecimal getDrawdown() {
        return Drawdown;
    }

    public BigDecimal getTotalNetProfit() {
        return TotalNetProfit;
    }

    public BigDecimal getSharpeRatio() {
        return SharpeRatio;
    }

    public BigDecimal getAlpha() {
        return Alpha;
    }

    public BigDecimal getBeta() {
        return Beta;
    }

    public BigDecimal getAnnualStandardDeviation() {
        return AnnualStandardDeviation;
    }

    public BigDecimal getAnnualVariance() {
        return AnnualVariance;
    }

    public BigDecimal getInformationRatio() {
        return InformationRatio;
    }

    public BigDecimal getTrackingError() {
        return TrackingError;
    }

    public BigDecimal getTreynorRatio() {
        return TreynorRatio;
    }

    /**
     * Annual compounded returns statistic based on the final-starting capital and years.
     * @param startingCapital Algorithm starting capital
     * @param finalCapital Algorithm final capital
     * @param years Years trading
     * @returns Decimal fraction for annual compounding performance
     */
    private static BigDecimal compoundingAnnualPerformance( BigDecimal startingCapital, BigDecimal finalCapital, BigDecimal years ) {
        return (years.signum() == 0 ? BigDecimal.ZERO : Math.pow((double)finalCapital / (double)startingCapital, 1 / (double)years) - 1).SafeDecimalCast();
    }

    /**
     * Drawdown maximum percentage.
     * @param equityOverTime The list of daily equity values
     * @returns The drawdown percentage
     */
    private static BigDecimal drawdownPercent(SortedMap<DateTime,BigDecimal> equityOverTime ) {
        return drawdownPercent( equityOverTime, 2 );
    }
    
    /**
     * Drawdown maximum percentage.
     * @param equityOverTime The list of daily equity values
     * @param rounding The number of BigDecimal places to round the result
     * @returns The drawdown percentage
     */
    private static BigDecimal drawdownPercent(SortedMap<DateTime,BigDecimal> equityOverTime, int rounding ) {
        prices = equityOverTime.Values.ToList();
        if( prices.Count == 0) return 0;

        drawdowns = new List<BigDecimal>();
        high = prices[0];
        for( price : prices ) {
            if( price > high) high = price;
            if( high > 0) drawdowns.Add(price / high - 1);
        }

        return Math.Round(Math.Abs(drawdowns.Min()), rounding);
    }

    /**     
     * Annualized return statistic calculated as an average of daily trading performance multiplied by the number of trading days per year.
     * @param performance Dictionary collection of double performance values
     * @param tradingDaysPerYear Trading days per year for the assets in portfolio
     * May be unaccurate for forex algorithms with more trading days in a year
     * @returns Double annual performance percentage
     */
    private static BigDecimal GetAnnualPerformance( List<double> performance, int tradingDaysPerYear = 252 ) {
        return (BigDecimal)performance.Average() * tradingDaysPerYear;
    }

    /**
     * Annualized variance statistic calculation using the daily performance variance and trading days per year.
     * @param performance">
     * @param tradingDaysPerYear">
     * Invokes the variance extension in the MathNet Statistics class
     * @returns Annual variance value
     */
    private static BigDecimal GetAnnualVariance(List<double> performance, int tradingDaysPerYear = 252) {
        variance = performance.Variance();
    }
}
