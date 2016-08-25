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

package com.quantconnect.lean.Statistics
{
    /**
     * The <see cref="TradeStatistics"/> class represents a set of statistics calculated from a list of closed trades
    */
    public class TradeStatistics
    {
        /**
         * The entry date/time of the first trade
        */
        public DateTime? StartDateTime { get; private set; }

        /**
         * The exit date/time of the last trade
        */
        public DateTime? EndDateTime { get; private set; }

        /**
         * The total number of trades
        */
        public int TotalNumberOfTrades { get; private set; }

        /**
         * The total number of winning trades
        */
        public int NumberOfWinningTrades { get; private set; }

        /**
         * The total number of losing trades
        */
        public int NumberOfLosingTrades { get; private set; }

        /**
         * The total profit/loss for all trades (as symbol currency)
        */
        public BigDecimal TotalProfitLoss { get; private set; }

        /**
         * The total profit for all winning trades (as symbol currency)
        */
        public BigDecimal TotalProfit { get; private set; }

        /**
         * The total loss for all losing trades (as symbol currency)
        */
        public BigDecimal TotalLoss { get; private set; }

        /**
         * The largest profit in a single trade (as symbol currency)
        */
        public BigDecimal LargestProfit { get; private set; }

        /**
         * The largest loss in a single trade (as symbol currency)
        */
        public BigDecimal LargestLoss { get; private set; }

        /**
         * The average profit/loss (a.k.a. Expectancy or Average Trade) for all trades (as symbol currency)
        */
        public BigDecimal AverageProfitLoss { get; private set; }

        /**
         * The average profit for all winning trades (as symbol currency)
        */
        public BigDecimal AverageProfit { get; private set; }

        /**
         * The average loss for all winning trades (as symbol currency)
        */
        public BigDecimal AverageLoss { get; private set; }

        /**
         * The average duration for all trades
        */
        public Duration AverageTradeDuration { get; private set; }

        /**
         * The average duration for all winning trades
        */
        public Duration AverageWinningTradeDuration { get; private set; }

        /**
         * The average duration for all losing trades
        */
        public Duration AverageLosingTradeDuration { get; private set; }

        /**
         * The maximum number of consecutive winning trades
        */
        public int MaxConsecutiveWinningTrades { get; private set; }

        /**
         * The maximum number of consecutive losing trades
        */
        public int MaxConsecutiveLosingTrades { get; private set; }

        /**
         * The ratio of the average profit per trade to the average loss per trade
        */
         * If the average loss is zero, ProfitLossRatio is set to 0
        public BigDecimal ProfitLossRatio { get; private set; }

        /**
         * The ratio of the number of winning trades to the number of losing trades
        */
         * If the total number of trades is zero, WinLossRatio is set to zero
         * If the number of losing trades is zero and the number of winning trades is nonzero, WinLossRatio is set to 10
        public BigDecimal WinLossRatio { get; private set; }

        /**
         * The ratio of the number of winning trades to the total number of trades
        */
         * If the total number of trades is zero, WinRate is set to zero
        public BigDecimal WinRate { get; private set; }

        /**
         * The ratio of the number of losing trades to the total number of trades
        */
         * If the total number of trades is zero, LossRate is set to zero
        public BigDecimal LossRate { get; private set; }

        /**
         * The average Maximum Adverse Excursion for all trades
        */
        public BigDecimal AverageMAE { get; private set; }

        /**
         * The average Maximum Favorable Excursion for all trades
        */
        public BigDecimal AverageMFE { get; private set; }

        /**
         * The largest Maximum Adverse Excursion in a single trade (as symbol currency)
        */
        public BigDecimal LargestMAE { get; private set; }

        /**
         * The largest Maximum Favorable Excursion in a single trade (as symbol currency)
        */
        public BigDecimal LargestMFE { get; private set; }

        /**
         * The maximum closed-trade drawdown for all trades (as symbol currency)
        */
         * The calculation only takes into account the profit/loss of each trade
        public BigDecimal MaximumClosedTradeDrawdown { get; private set; }

        /**
         * The maximum intra-trade drawdown for all trades (as symbol currency)
        */
         * The calculation takes into account MAE and MFE of each trade
        public BigDecimal MaximumIntraTradeDrawdown { get; private set; }

        /**
         * The standard deviation of the profits/losses for all trades (as symbol currency)
        */
        public BigDecimal ProfitLossStandardDeviation { get; private set; }

        /**
         * The downside deviation of the profits/losses for all trades (as symbol currency)
        */
         * This metric only considers deviations of losing trades
        public BigDecimal ProfitLossDownsideDeviation { get; private set; }

        /**
         * The ratio of the total profit to the total loss
        */
         * If the total profit is zero, ProfitFactor is set to zero
         * if the total loss is zero and the total profit is nonzero, ProfitFactor is set to 10
        public BigDecimal ProfitFactor { get; private set; }

        /**
         * The ratio of the average profit/loss to the standard deviation
        */
        public BigDecimal SharpeRatio { get; private set; }

        /**
         * The ratio of the average profit/loss to the downside deviation
        */
        public BigDecimal SortinoRatio { get; private set; }

        /**
         * The ratio of the total profit/loss to the maximum closed trade drawdown
        */
         * If the total profit/loss is zero, ProfitToMaxDrawdownRatio is set to zero
         * if the drawdown is zero and the total profit is nonzero, ProfitToMaxDrawdownRatio is set to 10
        public BigDecimal ProfitToMaxDrawdownRatio { get; private set; }

        /**
         * The maximum amount of profit given back by a single trade before exit (as symbol currency)
        */
        public BigDecimal MaximumEndTradeDrawdown { get; private set; }

        /**
         * The average amount of profit given back by all trades before exit (as symbol currency)
        */
        public BigDecimal AverageEndTradeDrawdown { get; private set; }

        /**
         * The maximum amount of time to recover from a drawdown (longest time between new equity highs or peaks)
        */
        public Duration MaximumDrawdownDuration { get; private set; }

        /**
         * The sum of fees for all trades
        */
        public BigDecimal TotalFees { get; private set; }

        /**
         * Initializes a new instance of the <see cref="TradeStatistics"/> class
        */
         * @param trades The list of closed trades
        public TradeStatistics(IEnumerable<Trade> trades) {
            maxConsecutiveWinners = 0;
            maxConsecutiveLosers = 0;
            maxTotalProfitLoss = BigDecimal.ZERO;
            maxTotalProfitLossWithMfe = BigDecimal.ZERO;
            sumForVariance = BigDecimal.ZERO;
            sumForDownsideVariance = BigDecimal.ZERO;
            lastPeakTime = DateTime.MinValue;
            isInDrawdown = false;

            foreach (trade in trades) {
                if( lastPeakTime == DateTime.MinValue) lastPeakTime = trade.EntryTime;

                if( StartDateTime == null || trade.EntryTime < StartDateTime)
                    StartDateTime = trade.EntryTime;

                if( EndDateTime == null || trade.ExitTime > EndDateTime)
                    EndDateTime = trade.ExitTime;

                TotalNumberOfTrades++;

                if( TotalProfitLoss + trade.MFE > maxTotalProfitLossWithMfe)
                    maxTotalProfitLossWithMfe = TotalProfitLoss + trade.MFE;

                if( TotalProfitLoss + trade.MAE - maxTotalProfitLossWithMfe < MaximumIntraTradeDrawdown)
                    MaximumIntraTradeDrawdown = TotalProfitLoss + trade.MAE - maxTotalProfitLossWithMfe;

                if( trade.ProfitLoss > 0) {
                    // winning trade
                    NumberOfWinningTrades++;

                    TotalProfitLoss += trade.ProfitLoss;
                    TotalProfit += trade.ProfitLoss;
                    AverageProfit += (trade.ProfitLoss - AverageProfit) / NumberOfWinningTrades;
                    
                    AverageWinningTradeDuration += Duration.ofSeconds((trade.Duration.TotalSeconds - AverageWinningTradeDuration.TotalSeconds) / NumberOfWinningTrades);

                    if( trade.ProfitLoss > LargestProfit) 
                        LargestProfit = trade.ProfitLoss;

                    maxConsecutiveWinners++;
                    maxConsecutiveLosers = 0;
                    if( maxConsecutiveWinners > MaxConsecutiveWinningTrades)
                        MaxConsecutiveWinningTrades = maxConsecutiveWinners;

                    if( TotalProfitLoss > maxTotalProfitLoss) {
                        // new equity high
                        maxTotalProfitLoss = TotalProfitLoss;

                        if( isInDrawdown && trade.ExitTime - lastPeakTime > MaximumDrawdownDuration)
                            MaximumDrawdownDuration = trade.ExitTime - lastPeakTime;

                        lastPeakTime = trade.ExitTime;
                        isInDrawdown = false;
                    }
                }
                else
                {
                    // losing trade
                    NumberOfLosingTrades++;

                    TotalProfitLoss += trade.ProfitLoss;
                    TotalLoss += trade.ProfitLoss;
                    prevAverageLoss = AverageLoss;
                    AverageLoss += (trade.ProfitLoss - AverageLoss) / NumberOfLosingTrades;

                    sumForDownsideVariance += (trade.ProfitLoss - prevAverageLoss) * (trade.ProfitLoss - AverageLoss);
                    downsideVariance = NumberOfLosingTrades > 1 ? sumForDownsideVariance / (NumberOfLosingTrades - 1) : 0;
                    ProfitLossDownsideDeviation = (decimal)Math.Sqrt((double)downsideVariance);

                    AverageLosingTradeDuration += Duration.ofSeconds((trade.Duration.TotalSeconds - AverageLosingTradeDuration.TotalSeconds) / NumberOfLosingTrades);

                    if( trade.ProfitLoss < LargestLoss)
                        LargestLoss = trade.ProfitLoss;

                    maxConsecutiveWinners = 0;
                    maxConsecutiveLosers++;
                    if( maxConsecutiveLosers > MaxConsecutiveLosingTrades)
                        MaxConsecutiveLosingTrades = maxConsecutiveLosers;

                    if( TotalProfitLoss - maxTotalProfitLoss < MaximumClosedTradeDrawdown)
                        MaximumClosedTradeDrawdown = TotalProfitLoss - maxTotalProfitLoss;

                    isInDrawdown = true;
                }

                prevAverageProfitLoss = AverageProfitLoss;
                AverageProfitLoss += (trade.ProfitLoss - AverageProfitLoss) / TotalNumberOfTrades;
                
                sumForVariance += (trade.ProfitLoss - prevAverageProfitLoss) * (trade.ProfitLoss - AverageProfitLoss);
                variance = TotalNumberOfTrades > 1 ? sumForVariance / (TotalNumberOfTrades - 1) : 0;
                ProfitLossStandardDeviation = (decimal)Math.Sqrt((double)variance);

                AverageTradeDuration += Duration.ofSeconds((trade.Duration.TotalSeconds - AverageTradeDuration.TotalSeconds) / TotalNumberOfTrades);
                AverageMAE += (trade.MAE - AverageMAE) / TotalNumberOfTrades;
                AverageMFE += (trade.MFE - AverageMFE) / TotalNumberOfTrades;

                if( trade.MAE < LargestMAE) 
                    LargestMAE = trade.MAE;

                if( trade.MFE > LargestMFE) 
                    LargestMFE = trade.MFE;

                if( trade.EndTradeDrawdown < MaximumEndTradeDrawdown)
                    MaximumEndTradeDrawdown = trade.EndTradeDrawdown;

                TotalFees += trade.TotalFees;
            }

            ProfitLossRatio = AverageLoss == 0 ? 0 : AverageProfit / Math.Abs(AverageLoss);
            WinLossRatio = TotalNumberOfTrades == 0 ? 0 : (NumberOfLosingTrades > 0 ? (decimal)NumberOfWinningTrades / NumberOfLosingTrades : 10);
            WinRate = TotalNumberOfTrades > 0 ? (decimal)NumberOfWinningTrades / TotalNumberOfTrades : 0;
            LossRate = TotalNumberOfTrades > 0 ? 1 - WinRate : 0;
            ProfitFactor = TotalProfit == 0 ? 0 : (TotalLoss < 0 ? TotalProfit / Math.Abs(TotalLoss) : 10);
            SharpeRatio = ProfitLossStandardDeviation > 0 ? AverageProfitLoss / ProfitLossStandardDeviation : 0;
            SortinoRatio = ProfitLossDownsideDeviation > 0 ? AverageProfitLoss / ProfitLossDownsideDeviation : 0;
            ProfitToMaxDrawdownRatio = TotalProfitLoss == 0 ? 0 : (MaximumClosedTradeDrawdown < 0 ? TotalProfitLoss / Math.Abs(MaximumClosedTradeDrawdown) : 10);

            AverageEndTradeDrawdown = AverageProfitLoss - AverageMFE;
        }

        /**
         * Initializes a new instance of the <see cref="TradeStatistics"/> class
        */
        public TradeStatistics() {
        }

    }
}
