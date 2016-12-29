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
import java.math.RoundingMode;
import java.time.Duration;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.Map.Entry;
import java.util.SortedMap;

import org.apache.commons.math3.stat.StatUtils;
import org.apache.commons.math3.stat.correlation.Covariance;
import org.apache.commons.math3.stat.correlation.PearsonsCorrelation;
import org.apache.commons.math3.stat.descriptive.moment.Variance;

import com.quantconnect.lean.Global;

/**
 * The <see cref="PortfolioStatistics"/> class represents a set of statistics calculated from equity and benchmark samples
 */
public class PortfolioStatistics {
    
    private static final BigDecimal RISK_FREE_RATE = BigDecimal.ZERO;

    /**
     * The average rate of return for winning trades
     */
    private BigDecimal averageWinRate;

    /**
     * The average rate of return for losing trades
     */
    private BigDecimal averageLossRate;

    /**
     * The ratio of the average win rate to the average loss rate
     * If the average loss rate is zero, ProfitLossRatio is set to 0
     */
    private BigDecimal profitLossRatio;

    /**
     * The ratio of the number of winning trades to the total number of trades
     * If the total number of trades is zero, WinRate is set to zero
     */
    private BigDecimal winRate;

    /**
     * The ratio of the number of losing trades to the total number of trades
     * If the total number of trades is zero, LossRate is set to zero
     */
    private BigDecimal lossRate;

    /**
     * The expected value of the rate of return
     */
    private BigDecimal expectancy;

    /**
     * Annual compounded returns statistic based on the final-starting capital and years.
     * Also known as Compound Annual Growth Rate (CAGR)
     */
    private BigDecimal compoundingAnnualReturn;

    /**
     * Drawdown maximum percentage.
     */
    private BigDecimal drawdown;

    /**
     * The total net profit percentage.
     */
    private BigDecimal totalNetProfit;

    /**
     * Sharpe ratio with respect to risk free rate: measures excess of return per unit of risk.
     * With risk defined as the algorithm's volatility
     */
    private BigDecimal sharpeRatio;

    /**
     * Algorithm "Alpha" statistic - abnormal returns over the risk free rate and the relationshio (beta) with the benchmark returns.
     */
    private BigDecimal alpha;

    /**
     * Algorithm "beta" statistic - the covariance between the algorithm and benchmark performance, divided by benchmark's variance
     */
    private BigDecimal beta;

    /**
     * Annualized standard deviation
     */
    private BigDecimal annualStandardDeviation;

    /**
     * Annualized variance statistic calculation using the daily performance variance and trading days per year.
     */
    private BigDecimal annualVariance;

    /**
     * Information ratio - risk adjusted return
     * (risk = tracking error volatility, a volatility measures that considers the volatility of both algo and benchmark)
     */
    private BigDecimal informationRatio;

    /**
     * Tracking error volatility (TEV) statistic - a measure of how closely a portfolio follows the index to which it is benchmarked
     * If algo = benchmark, TEV = 0
     */
    private BigDecimal trackingError;

    /**
     * Treynor ratio statistic is a measurement of the returns earned in excess of that which could have been earned on an investment that has no diversifiable risk
     */
    private BigDecimal treynorRatio;

    /**
     * Initializes a new instance of the <see cref="PortfolioStatistics"/> class
     * @param profitLoss Trade record of profits and losses
     * @param equity The list of daily equity values
     * @param listPerformance The list of algorithm performance values
     * @param listBenchmark The list of benchmark values
     * @param startingCapital The algorithm starting capital
     * @param tradingDaysPerYear The number of trading days per year
     */
    public PortfolioStatistics( SortedMap<LocalDate, BigDecimal> profitLoss, SortedMap<LocalDate, BigDecimal> equity, double[] listPerformance,
            double[] listBenchmark, BigDecimal startingCapital ) {
        this( profitLoss, equity, listPerformance, listBenchmark, startingCapital, Global.TRADING_DAYS_PER_YEAR );
    }

    /**
     * Initializes a new instance of the <see cref="PortfolioStatistics"/> class
     * @param profitLoss Trade record of profits and losses
     * @param equity The list of daily equity values
     * @param listPerformance The list of algorithm performance values
     * @param listBenchmark The list of benchmark values
     * @param startingCapital The algorithm starting capital
     * @param tradingDaysPerYear The number of trading days per year
     */
    public PortfolioStatistics( SortedMap<LocalDate,BigDecimal> profitLoss, SortedMap<LocalDate,BigDecimal> equity, double[] listPerformance, 
            double[] listBenchmark, BigDecimal startingCapital, int tradingDaysPerYear ) {
        
        if( startingCapital.signum() == 0 ) 
            return;

        BigDecimal runningCapital = startingCapital;
        BigDecimal totalProfit = BigDecimal.ZERO;
        BigDecimal totalLoss = BigDecimal.ZERO;
        int totalWins = 0;
        int totalLosses = 0;
        for( Entry<LocalDate,BigDecimal> pair : profitLoss.entrySet() ) {
            final BigDecimal tradeProfitLoss = pair.getValue();

            if( tradeProfitLoss.signum() > 0) {
                totalProfit = totalProfit.add( tradeProfitLoss.divide( runningCapital, RoundingMode.HALF_EVEN ) );
                totalWins++;
            }
            else {
                totalLoss = totalLoss.add( tradeProfitLoss.divide( runningCapital, RoundingMode.HALF_EVEN ) );
                totalLosses++;
            }

            runningCapital = runningCapital.add( tradeProfitLoss );
        }

        averageWinRate = totalWins == 0 ? BigDecimal.ZERO : totalProfit.divide( BigDecimal.valueOf( totalWins ), RoundingMode.HALF_UP );
        averageLossRate = totalLosses == 0 ? BigDecimal.ZERO : totalLoss.divide( BigDecimal.valueOf( totalLosses ), RoundingMode.HALF_UP );
        profitLossRatio = averageLossRate.signum() == 0 ? BigDecimal.ZERO : averageWinRate.divide( averageLossRate.abs(), RoundingMode.HALF_UP );

        winRate = profitLoss.isEmpty() ? BigDecimal.ZERO : BigDecimal.valueOf( totalWins / (double)profitLoss.size() );
        lossRate = profitLoss.isEmpty() ? BigDecimal.ZERO : BigDecimal.valueOf( totalLosses / (double)profitLoss.size() );
        expectancy = winRate.multiply( profitLossRatio ).subtract( lossRate );

        final LocalDate lastKey = equity.isEmpty() ? null : equity.lastKey();
        final BigDecimal lastValue = lastKey == null ? BigDecimal.ZERO : equity.get( lastKey );
        if( profitLoss.size() > 0 )
            totalNetProfit = (lastValue.divide( startingCapital, RoundingMode.HALF_UP )).subtract( BigDecimal.ONE );

        final BigDecimal fractionOfYears = lastKey == null ? BigDecimal.ZERO : BigDecimal.valueOf( Duration.between( equity.firstKey(), lastKey ).toDays() / 365.0D );
        compoundingAnnualReturn = compoundingAnnualPerformance( startingCapital, lastValue, fractionOfYears );

        drawdown = drawdownPercent( equity, 3 );

        annualVariance = getAnnualVariance( listPerformance, tradingDaysPerYear );
        annualStandardDeviation = BigDecimal.valueOf( Math.sqrt( annualVariance.doubleValue() ) );

        final BigDecimal annualPerformance = getAnnualPerformance( listPerformance, tradingDaysPerYear );
        sharpeRatio = annualStandardDeviation.signum() == 0 ? BigDecimal.ZERO : (annualPerformance.subtract( RISK_FREE_RATE )).divide( annualStandardDeviation, RoundingMode.HALF_UP );

        final double benchmarkVariance = StatUtils.variance( listBenchmark );
        beta = Double.isNaN( benchmarkVariance ) || benchmarkVariance == 0.0D ? BigDecimal.ZERO : BigDecimal.valueOf( (new Covariance()).covariance( listPerformance, listBenchmark ) / benchmarkVariance );

        alpha = beta.signum() == 0 ? BigDecimal.ZERO : annualPerformance.subtract( (RISK_FREE_RATE.add( beta.multiply( (getAnnualPerformance( listBenchmark, tradingDaysPerYear ).subtract( RISK_FREE_RATE )) ) ) ) );

        final double correlation = (new PearsonsCorrelation()).correlation( listPerformance, listBenchmark );
        final double benchmarkAnnualVariance = benchmarkVariance * tradingDaysPerYear;
        trackingError = Double.isNaN( correlation ) || correlation == 0.0D || Double.isNaN( benchmarkAnnualVariance ) || benchmarkAnnualVariance == 0.0D ? BigDecimal.ZERO :
            BigDecimal.valueOf( Math.sqrt( annualVariance.doubleValue() - 2 * correlation * annualStandardDeviation.doubleValue() * Math.sqrt( benchmarkAnnualVariance ) + benchmarkAnnualVariance ) );

        informationRatio = trackingError.signum() == 0 ? BigDecimal.ZERO : (annualPerformance.subtract( getAnnualPerformance( listBenchmark, tradingDaysPerYear ))).divide( trackingError, RoundingMode.HALF_UP );

        treynorRatio = beta.signum() == 0 ? BigDecimal.ZERO : (annualPerformance.subtract( RISK_FREE_RATE )).divide( beta, RoundingMode.HALF_UP );
    }

    /**
     * Initializes a new instance of the <see cref="PortfolioStatistics"/> class
     */
    public PortfolioStatistics() { }

    
    public BigDecimal getAverageWinRate() {
        return averageWinRate;
    }

    public BigDecimal getAverageLossRate() {
        return averageLossRate;
    }

    public BigDecimal getProfitLossRatio() {
        return profitLossRatio;
    }

    public BigDecimal getWinRate() {
        return winRate;
    }

    public BigDecimal getLossRate() {
        return lossRate;
    }

    public BigDecimal getExpectancy() {
        return expectancy;
    }

    public BigDecimal getCompoundingAnnualReturn() {
        return compoundingAnnualReturn;
    }

    public BigDecimal getDrawdown() {
        return drawdown;
    }

    public BigDecimal getTotalNetProfit() {
        return totalNetProfit;
    }

    public BigDecimal getSharpeRatio() {
        return sharpeRatio;
    }

    public BigDecimal getAlpha() {
        return alpha;
    }

    public BigDecimal getBeta() {
        return beta;
    }

    public BigDecimal getAnnualStandardDeviation() {
        return annualStandardDeviation;
    }

    public BigDecimal getAnnualVariance() {
        return annualVariance;
    }

    public BigDecimal getInformationRatio() {
        return informationRatio;
    }

    public BigDecimal getTrackingError() {
        return trackingError;
    }

    public BigDecimal getTreynorRatio() {
        return treynorRatio;
    }

    /**
     * Annual compounded returns statistic based on the final-starting capital and years.
     * @param startingCapital Algorithm starting capital
     * @param finalCapital Algorithm final capital
     * @param years Years trading
     * @returns Decimal fraction for annual compounding performance
     */
    private static BigDecimal compoundingAnnualPerformance( BigDecimal startingCapital, BigDecimal finalCapital, BigDecimal years ) {
        return years.signum() == 0 ? BigDecimal.ZERO : BigDecimal.valueOf( Math.pow( finalCapital.divide( startingCapital, RoundingMode.HALF_EVEN ).doubleValue(), 1.0D / years.doubleValue() ) - 1 );
    }

//    /**
//     * Drawdown maximum percentage.
//     * @param equityOverTime The list of daily equity values
//     * @returns The drawdown percentage
//     */
//    private static BigDecimal drawdownPercent( SortedMap<LocalDate,BigDecimal> equityOverTime ) {
//        return drawdownPercent( equityOverTime, 2 );
//    }
    
    /**
     * Drawdown maximum percentage.
     * @param equityOverTime The list of daily equity values
     * @param rounding The number of BigDecimal places to round the result
     * @returns The drawdown percentage
     */
    private static BigDecimal drawdownPercent( SortedMap<LocalDate,BigDecimal> equityOverTime, int rounding ) {
        final Collection<BigDecimal> prices = equityOverTime.values();
        if( prices.isEmpty() ) 
            return BigDecimal.ZERO;

        final List<BigDecimal> drawdowns = new ArrayList<BigDecimal>();
        BigDecimal high = prices.iterator().next();
        for( BigDecimal price : prices ) {
            high = price.max( high );
            if( high.signum() > 0 ) 
                drawdowns.add( price.divide( high ).subtract( BigDecimal.ONE ) );
        }

        final BigDecimal minAbs = drawdowns.stream().min( BigDecimal::compareTo ).get().abs();
        return minAbs.setScale( rounding, RoundingMode.HALF_UP );
    }

//    /**     
//     * Annualized return statistic calculated as an average of daily trading performance multiplied by the number of trading days per year.
//     * @param performance Dictionary collection of double performance values
//     * May be unaccurate for forex algorithms with more trading days in a year
//     * @returns Double annual performance percentage
//     */
//    private static BigDecimal getAnnualPerformance( double[] performance ) {
//        return getAnnualPerformance( performance, Global.TRADING_DAYS_PER_YEAR );
//    }
    
    /**     
     * Annualized return statistic calculated as an average of daily trading performance multiplied by the number of trading days per year.
     * @param performance Dictionary collection of double performance values
     * @param tradingDaysPerYear Trading days per year for the assets in portfolio
     * May be unaccurate for forex algorithms with more trading days in a year
     * @returns Double annual performance percentage
     */
    private static BigDecimal getAnnualPerformance( double[] performance, int tradingDaysPerYear ) {
        return BigDecimal.valueOf( StatUtils.mean( performance ) * tradingDaysPerYear );
    }

//    /**
//     * Annualized variance statistic calculation using the daily performance variance and trading days per year.
//     * @param performance
//     * Invokes the variance extension in the MathNet Statistics class
//     * @returns Annual variance value
//     */
//    private static BigDecimal getAnnualVariance( double[] performance ) {
//        return getAnnualVariance( performance, Global.TRADING_DAYS_PER_YEAR );
//    }
    
    /**
     * Annualized variance statistic calculation using the daily performance variance and trading days per year.
     * @param performance
     * @param tradingDaysPerYear
     * Invokes the variance extension in the MathNet Statistics class
     * @returns Annual variance value
     */
    private static BigDecimal getAnnualVariance( double[] performance, int tradingDaysPerYear ) {
        final double variance = (new Variance()).evaluate( performance );
        return Double.isNaN( variance ) ? BigDecimal.ZERO : BigDecimal.valueOf( variance * tradingDaysPerYear );
    }
}
