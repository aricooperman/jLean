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

package com.quantconnect.lean.statistics;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.Duration;
import java.time.LocalDateTime;
import java.util.Optional;

/**
 * The <see cref="TradeStatistics"/> class represents a set of statistics calculated from a list of closed trades
 */
public class TradeStatistics {
 
    /**
     * The entry date/time of the first trade
    */
    private LocalDateTime startDateTime;

    /**
     * The exit date/time of the last trade
    */
    private LocalDateTime endDateTime;

    /**
     * The total number of trades
    */
    private int totalNumberOfTrades;

    /**
     * The total number of winning trades
    */
    private int numberOfWinningTrades;

    /**
     * The total number of losing trades
    */
    private int numberOfLosingTrades;

    /**
     * The total profit/loss for all trades (as symbol currency)
    */
    private BigDecimal totalProfitLoss;

    /**
     * The total profit for all winning trades (as symbol currency)
    */
    private BigDecimal totalProfit;

    /**
     * The total loss for all losing trades (as symbol currency)
    */
    private BigDecimal totalLoss;

    /**
     * The largest profit in a single trade (as symbol currency)
    */
    private BigDecimal largestProfit;

    /**
     * The largest loss in a single trade (as symbol currency)
    */
    private BigDecimal largestLoss;

    /**
     * The average profit/loss (a.k.a. Expectancy or Average Trade) for all trades (as symbol currency)
    */
    private BigDecimal averageProfitLoss;

    /**
     * The average profit for all winning trades (as symbol currency)
    */
    private BigDecimal averageProfit;

    /**
     * The average loss for all winning trades (as symbol currency)
    */
    private BigDecimal averageLoss;

    /**
     * The average duration for all trades
    */
    private Duration averageTradeDuration;

    /**
     * The average duration for all winning trades
    */
    private Duration averageWinningTradeDuration;

    /**
     * The average duration for all losing trades
    */
    private Duration averageLosingTradeDuration;

    /**
     * The maximum number of consecutive winning trades
    */
    private int maxConsecutiveWinningTrades;

    /**
     * The maximum number of consecutive losing trades
    */
    private int maxConsecutiveLosingTrades;

    /**
     * The ratio of the average profit per trade to the average loss per trade
     * If the average loss is zero, ProfitLossRatio is set to 0
    */
    private BigDecimal profitLossRatio;

    /**
     * The ratio of the number of winning trades to the number of losing trades
     * If the total number of trades is zero, WinLossRatio is set to zero
     * If the number of losing trades is zero and the number of winning trades is nonzero, WinLossRatio is set to 10
    */
    private BigDecimal winLossRatio;

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
     * The average Maximum Adverse Excursion for all trades
    */
    private BigDecimal averageMAE;

    /**
     * The average Maximum Favorable Excursion for all trades
    */
    private BigDecimal averageMFE;

    /**
     * The largest Maximum Adverse Excursion in a single trade (as symbol currency)
    */
    private BigDecimal largestMAE;

    /**
     * The largest Maximum Favorable Excursion in a single trade (as symbol currency)
    */
    private BigDecimal largestMFE;

    /**
     * The maximum closed-trade drawdown for all trades (as symbol currency)
     * The calculation only takes into account the profit/loss of each trade
    */
    private BigDecimal maximumClosedTradeDrawdown;

    /**
     * The maximum intra-trade drawdown for all trades (as symbol currency)
     * The calculation takes into account MAE and MFE of each trade
    */
    private BigDecimal maximumIntraTradeDrawdown;

    /**
     * The standard deviation of the profits/losses for all trades (as symbol currency)
    */
    private BigDecimal profitLossStandardDeviation;

    /**
     * The downside deviation of the profits/losses for all trades (as symbol currency)
     * This metric only considers deviations of losing trades
    */
    private BigDecimal profitLossDownsideDeviation;

    /**
     * The ratio of the total profit to the total loss
     * If the total profit is zero, ProfitFactor is set to zero
     * if the total loss is zero and the total profit is nonzero, ProfitFactor is set to 10
    */
    private BigDecimal profitFactor;

    /**
     * The ratio of the average profit/loss to the standard deviation
    */
    private BigDecimal sharpeRatio;

    /**
     * The ratio of the average profit/loss to the downside deviation
    */
    private BigDecimal sortinoRatio;

    /**
     * The ratio of the total profit/loss to the maximum closed trade drawdown
     * If the total profit/loss is zero, ProfitToMaxDrawdownRatio is set to zero
     * if the drawdown is zero and the total profit is nonzero, ProfitToMaxDrawdownRatio is set to 10
    */
    private BigDecimal profitToMaxDrawdownRatio;

    /**
     * The maximum amount of profit given back by a single trade before exit (as symbol currency)
    */
    private BigDecimal maximumEndTradeDrawdown;

    /**
     * The average amount of profit given back by all trades before exit (as symbol currency)
    */
    private BigDecimal averageEndTradeDrawdown;

    /**
     * The maximum amount of time to recover from a drawdown (longest time between new equity highs or peaks)
    */
    private Duration maximumDrawdownDuration;

    /**
     * The sum of fees for all trades
    */
    private BigDecimal totalFees;

    /**
     * Initializes a new instance of the <see cref="TradeStatistics"/> class
     * @param trades The list of closed trades
    */
    public TradeStatistics( Iterable<Trade> trades ) {
        int maxConsecutiveWinners = 0;
        int maxConsecutiveLosers = 0;
        BigDecimal maxTotalProfitLoss = BigDecimal.ZERO;
        BigDecimal maxTotalProfitLossWithMfe = BigDecimal.ZERO;
        BigDecimal sumForVariance = BigDecimal.ZERO;
        BigDecimal sumForDownsideVariance = BigDecimal.ZERO;
        LocalDateTime lastPeakTime = LocalDateTime.MIN;
        boolean isInDrawdown = false;

        for( Trade trade : trades ) {
            final LocalDateTime entryTime = trade.getEntryTime();
            if( lastPeakTime == LocalDateTime.MIN ) 
                lastPeakTime = entryTime;

            if( startDateTime == null || entryTime.isBefore( startDateTime ) )
                startDateTime = entryTime;

            final LocalDateTime exitTime = trade.getExitTime();
            if( endDateTime == null || exitTime.isAfter( endDateTime ) )
                endDateTime = exitTime;

            totalNumberOfTrades++;

            final BigDecimal maxFavExcursion = trade.getMaxFavExcursion();
            final BigDecimal totalPnlWithMfe = totalProfitLoss.add( maxFavExcursion );
            if( totalPnlWithMfe.compareTo( maxTotalProfitLossWithMfe ) > 0 )
                maxTotalProfitLossWithMfe = totalPnlWithMfe;

            final BigDecimal maxAdvExcursion = trade.getMaxAdvExcursion();
            final BigDecimal intraTradeDrawdown = totalProfitLoss.add( maxAdvExcursion ).subtract( maxTotalProfitLossWithMfe );
            if( intraTradeDrawdown.compareTo( maximumIntraTradeDrawdown ) < 0 )
                maximumIntraTradeDrawdown = intraTradeDrawdown;

            final BigDecimal profitLoss = trade.getProfitLoss();
            if( profitLoss.signum() > 0 ) {
                // winning trade
                numberOfWinningTrades++;

                totalProfitLoss = totalProfitLoss.add( profitLoss );
                totalProfit = totalProfit.add( profitLoss );
                averageProfit = averageProfit.add( (profitLoss.subtract( averageProfit )).divide( BigDecimal.valueOf( numberOfWinningTrades ), RoundingMode.HALF_UP ) );
                
                averageWinningTradeDuration = averageWinningTradeDuration.plusSeconds( (trade.getDuration().getSeconds() - averageWinningTradeDuration.getSeconds()) / numberOfWinningTrades );

                if( profitLoss.compareTo( largestProfit ) > 0 )
                    largestProfit = profitLoss;

                maxConsecutiveWinners++;
                maxConsecutiveLosers = 0;
                if( maxConsecutiveWinners > maxConsecutiveWinningTrades )
                    maxConsecutiveWinningTrades = maxConsecutiveWinners;

                if( totalProfitLoss.compareTo( maxTotalProfitLoss ) > 0 ) {
                    // new equity high
                    maxTotalProfitLoss = totalProfitLoss;

                    final Duration drawdownDuration = Duration.between( lastPeakTime, exitTime );
                    if( isInDrawdown && drawdownDuration.compareTo( maximumDrawdownDuration ) > 0 )
                        maximumDrawdownDuration = drawdownDuration;

                    lastPeakTime = exitTime;
                    isInDrawdown = false;
                }
            }
            else {
                // losing trade
                numberOfLosingTrades++;

                totalProfitLoss = totalProfitLoss.add( profitLoss );
                totalLoss = totalLoss.add( profitLoss );
                BigDecimal prevAverageLoss = averageLoss;
                averageLoss = averageLoss.add( (profitLoss.subtract( averageLoss )).divide( BigDecimal.valueOf( numberOfLosingTrades ), RoundingMode.HALF_UP ) );

                sumForDownsideVariance = sumForDownsideVariance.add( (profitLoss.subtract( prevAverageLoss )).multiply( (profitLoss.subtract( averageLoss )) ) );
                BigDecimal downsideVariance = numberOfLosingTrades > 1 ? sumForDownsideVariance.divide( BigDecimal.valueOf( (numberOfLosingTrades - 1) ), RoundingMode.HALF_UP ) : BigDecimal.ZERO;
                profitLossDownsideDeviation = BigDecimal.valueOf( Math.sqrt( downsideVariance.doubleValue() ) );

                averageLosingTradeDuration = averageLosingTradeDuration.plusSeconds( (trade.getDuration().getSeconds() - averageLosingTradeDuration.getSeconds()) / numberOfLosingTrades );

                if( profitLoss.compareTo( largestLoss ) < 0 )
                    largestLoss = profitLoss;

                maxConsecutiveWinners = 0;
                maxConsecutiveLosers++;
                if( maxConsecutiveLosers > maxConsecutiveLosingTrades)
                    maxConsecutiveLosingTrades = maxConsecutiveLosers;

                final BigDecimal closedTradeDrawdown = totalProfitLoss.subtract( maxTotalProfitLoss );
                if( closedTradeDrawdown.compareTo( maximumClosedTradeDrawdown ) < 0 )
                    maximumClosedTradeDrawdown = closedTradeDrawdown;

                isInDrawdown = true;
            }

            BigDecimal prevAverageProfitLoss = averageProfitLoss;
            averageProfitLoss = averageProfitLoss.add( (profitLoss.subtract( averageProfitLoss )).divide( BigDecimal.valueOf( totalNumberOfTrades ), RoundingMode.HALF_UP ) );
            
            sumForVariance = sumForVariance.add( (profitLoss.subtract( prevAverageProfitLoss )).multiply( (profitLoss.subtract( averageProfitLoss )) ) );
            final BigDecimal variance = totalNumberOfTrades > 1 ? sumForVariance.divide( BigDecimal.valueOf( (totalNumberOfTrades - 1) ), RoundingMode.HALF_UP ) : BigDecimal.ZERO;
            profitLossStandardDeviation = BigDecimal.valueOf( Math.sqrt( variance.doubleValue() ) );

            averageTradeDuration = averageTradeDuration.plusSeconds( (trade.getDuration().getSeconds() - averageTradeDuration.getSeconds()) / totalNumberOfTrades);
            averageMAE = averageMAE.add( (maxAdvExcursion.subtract( averageMAE )).divide( BigDecimal.valueOf( totalNumberOfTrades ), RoundingMode.HALF_UP ) );
            averageMFE = averageMFE.add( (maxFavExcursion.subtract( averageMFE )).divide( BigDecimal.valueOf( totalNumberOfTrades ), RoundingMode.HALF_UP ) );

            if( maxAdvExcursion.compareTo( largestMAE ) < 0 )
                largestMAE = maxAdvExcursion;

            if( maxFavExcursion.compareTo( largestMFE ) > 0 ) 
                largestMFE = maxFavExcursion;

            final BigDecimal endTradeDrawdown = trade.getEndTradeDrawdown();
            if( endTradeDrawdown.compareTo( maximumEndTradeDrawdown ) < 0 )
                maximumEndTradeDrawdown = endTradeDrawdown;

            totalFees = totalFees.add( trade.getTotalFees() );
        }

        profitLossRatio = averageLoss.signum() == 0 ? BigDecimal.ZERO : averageProfit.divide( averageLoss.abs(), RoundingMode.HALF_UP );
        winLossRatio = totalNumberOfTrades == 0 ? BigDecimal.ZERO : (numberOfLosingTrades > 0 ? BigDecimal.valueOf( numberOfWinningTrades / (double)numberOfLosingTrades ) : BigDecimal.TEN);
        winRate = totalNumberOfTrades > 0 ? BigDecimal.valueOf( numberOfWinningTrades / (double)totalNumberOfTrades ) : BigDecimal.ZERO;
        lossRate = totalNumberOfTrades > 0 ? BigDecimal.ONE.subtract( winRate ) : BigDecimal.ZERO;
        profitFactor = totalProfit.signum() == 0 ? BigDecimal.ZERO : (totalLoss.signum() < 0 ? totalProfit.divide( totalLoss.abs(), RoundingMode.HALF_UP ) : BigDecimal.TEN);
        sharpeRatio = profitLossStandardDeviation.signum() > 0 ? averageProfitLoss.divide( profitLossStandardDeviation, RoundingMode.HALF_UP ) : BigDecimal.ZERO;
        sortinoRatio = profitLossDownsideDeviation.signum() > 0 ? averageProfitLoss.divide( profitLossDownsideDeviation, RoundingMode.HALF_UP ) : BigDecimal.ZERO;
        profitToMaxDrawdownRatio = totalProfitLoss.signum() == 0 ? BigDecimal.ZERO : (maximumClosedTradeDrawdown.signum() < 0 ? totalProfitLoss.divide( maximumClosedTradeDrawdown.abs(), RoundingMode.HALF_UP ) : BigDecimal.TEN );

        averageEndTradeDrawdown = averageProfitLoss.subtract( averageMFE );
    }

    /**
     * Initializes a new instance of the <see cref="TradeStatistics"/> class
     */
    public TradeStatistics() { }

    public Optional<LocalDateTime> getStartDateTime() {
        return Optional.ofNullable( startDateTime );
    }

    public Optional<LocalDateTime> getEndDateTime() {
        return Optional.ofNullable( endDateTime );
    }

    public int getTotalNumberOfTrades() {
        return totalNumberOfTrades;
    }

    public int getNumberOfWinningTrades() {
        return numberOfWinningTrades;
    }

    public int getNumberOfLosingTrades() {
        return numberOfLosingTrades;
    }

    public BigDecimal getTotalProfitLoss() {
        return totalProfitLoss;
    }

    public BigDecimal getTotalProfit() {
        return totalProfit;
    }

    public BigDecimal getTotalLoss() {
        return totalLoss;
    }

    public BigDecimal getLargestProfit() {
        return largestProfit;
    }

    public BigDecimal getLargestLoss() {
        return largestLoss;
    }

    public BigDecimal getAverageProfitLoss() {
        return averageProfitLoss;
    }

    public BigDecimal getAverageProfit() {
        return averageProfit;
    }

    public BigDecimal getAverageLoss() {
        return averageLoss;
    }

    public Duration getAverageTradeDuration() {
        return averageTradeDuration;
    }

    public Duration getAverageWinningTradeDuration() {
        return averageWinningTradeDuration;
    }

    public Duration getAverageLosingTradeDuration() {
        return averageLosingTradeDuration;
    }

    public int getMaxConsecutiveWinningTrades() {
        return maxConsecutiveWinningTrades;
    }

    public int getMaxConsecutiveLosingTrades() {
        return maxConsecutiveLosingTrades;
    }

    public BigDecimal getProfitLossRatio() {
        return profitLossRatio;
    }

    public BigDecimal getWinLossRatio() {
        return winLossRatio;
    }

    public BigDecimal getWinRate() {
        return winRate;
    }

    public BigDecimal getLossRate() {
        return lossRate;
    }

    public BigDecimal getAverageMAE() {
        return averageMAE;
    }

    public BigDecimal getAverageMFE() {
        return averageMFE;
    }

    public BigDecimal getLargestMAE() {
        return largestMAE;
    }

    public BigDecimal getLargestMFE() {
        return largestMFE;
    }

    public BigDecimal getMaximumClosedTradeDrawdown() {
        return maximumClosedTradeDrawdown;
    }

    public BigDecimal getMaximumIntraTradeDrawdown() {
        return maximumIntraTradeDrawdown;
    }

    public BigDecimal getProfitLossStandardDeviation() {
        return profitLossStandardDeviation;
    }

    public BigDecimal getProfitLossDownsideDeviation() {
        return profitLossDownsideDeviation;
    }

    public BigDecimal getProfitFactor() {
        return profitFactor;
    }

    public BigDecimal getSharpeRatio() {
        return sharpeRatio;
    }

    public BigDecimal getSortinoRatio() {
        return sortinoRatio;
    }

    public BigDecimal getProfitToMaxDrawdownRatio() {
        return profitToMaxDrawdownRatio;
    }

    public BigDecimal getMaximumEndTradeDrawdown() {
        return maximumEndTradeDrawdown;
    }

    public BigDecimal getAverageEndTradeDrawdown() {
        return averageEndTradeDrawdown;
    }

    public Duration getMaximumDrawdownDuration() {
        return maximumDrawdownDuration;
    }

    public BigDecimal getTotalFees() {
        return totalFees;
    }
}
