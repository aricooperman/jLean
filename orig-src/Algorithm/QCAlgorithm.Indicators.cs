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
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.Consolidators;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;

package com.quantconnect.lean.Algorithm
{
    public partial class QCAlgorithm
    {
        /**
        /// Creates a new Identity indicator for the symbol The indicator will be automatically
        /// updated on the symbol's subscription resolution
        */
         * @param symbol">The symbol whose values we want as an indicator
         * @param selector">Selects a value from the BaseData, if null defaults to the .Value property (x -> x.Value)
         * @param fieldName">The name of the field being selected
        @returns A new Identity indicator for the specified symbol and selector
        public Identity Identity(Symbol symbol, Func<BaseData, decimal> selector = null, String fieldName = null ) {
            resolution = GetSubscription(symbol).Resolution;
            return Identity(symbol, resolution, selector, fieldName);
        }

        /**
        /// Creates a new Identity indicator for the symbol The indicator will be automatically
        /// updated on the symbol's subscription resolution
        */
         * @param symbol">The symbol whose values we want as an indicator
         * @param resolution">The desired resolution of the data
         * @param selector">Selects a value from the BaseData, if null defaults to the .Value property (x -> x.Value)
         * @param fieldName">The name of the field being selected
        @returns A new Identity indicator for the specified symbol and selector
        public Identity Identity(Symbol symbol, Resolution resolution, Func<BaseData, decimal> selector = null, String fieldName = null ) {
            String name = CreateIndicatorName(symbol, fieldName ?? "close", resolution);
            identity = new Identity(name);
            RegisterIndicator(symbol, identity, resolution, selector);
            return identity;
        }

        /**
        /// Creates a new Identity indicator for the symbol The indicator will be automatically
        /// updated on the symbol's subscription resolution
        */
         * @param symbol">The symbol whose values we want as an indicator
         * @param resolution">The desired resolution of the data
         * @param selector">Selects a value from the BaseData, if null defaults to the .Value property (x -> x.Value)
         * @param fieldName">The name of the field being selected
        @returns A new Identity indicator for the specified symbol and selector
        public Identity Identity(Symbol symbol, Duration resolution, Func<BaseData, decimal> selector = null, String fieldName = null ) {
            String name = String.format( "%1$s(%2$s_%3$s)", symbol, fieldName ?? "close", resolution);
            identity = new Identity(name);
            RegisterIndicator(symbol, identity, ResolveConsolidator(symbol, resolution), selector);
            return identity;
        }

        /**
        /// Creates a new IchimokuKinkoHyo indicator for the symbol. The indicator will be automatically
        /// updated on the given resolution.
        */
         * @param symbol">The symbol whose ICHIMOKU we want
         * @param tenkanPeriod">The period to calculate the Tenkan-sen period
         * @param kijunPeriod">The period to calculate the Kijun-sen period
         * @param senkouAPeriod">The period to calculate the Tenkan-sen period
         * @param senkouBPeriod">The period to calculate the Tenkan-sen period
         * @param senkouADelayPeriod">The period to calculate the Tenkan-sen period
         * @param senkouBDelayPeriod">The period to calculate the Tenkan-sen period
         * @param resolution">The resolution
        @returns A new IchimokuKinkoHyo indicator with the specified periods and delays
        public IchimokuKinkoHyo ICHIMOKU(Symbol symbol, int tenkanPeriod, int kijunPeriod, int senkouAPeriod, int senkouBPeriod, int senkouADelayPeriod, int senkouBDelayPeriod, Resolution? resolution = null ) {
            name = CreateIndicatorName(symbol, String.format( "ICHIMOKU(%1$s,%2$s)", tenkanPeriod, kijunPeriod), resolution);
            ichimoku = new IchimokuKinkoHyo(name, tenkanPeriod, kijunPeriod, senkouAPeriod, senkouBPeriod, senkouADelayPeriod, senkouBDelayPeriod);
            RegisterIndicator(symbol, ichimoku, resolution);
            return ichimoku;
        }

        /**
        /// Creates a new AverageTrueRange indicator for the symbol. The indicator will be automatically
        /// updated on the given resolution.
        */
         * @param symbol">The symbol whose ATR we want
         * @param period">The smoothing period used to smooth the computed TrueRange values
         * @param type">The type of smoothing to use
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns A new AverageTrueRange indicator with the specified smoothing type and period
        public AverageTrueRange ATR(Symbol symbol, int period, MovingAverageType type = MovingAverageType.Simple, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            String name = CreateIndicatorName(symbol, "ATR" + period, resolution);
            atr = new AverageTrueRange(name, period, type);
            RegisterIndicator(symbol, atr, resolution, selector);
            return atr;
        }

        /**
        /// Creates an ExponentialMovingAverage indicator for the symbol. The indicator will be automatically
        /// updated on the given resolution.
        */
         * @param symbol">The symbol whose EMA we want
         * @param period">The period of the EMA
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The ExponentialMovingAverage for the given parameters
        public ExponentialMovingAverage EMA(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            String name = CreateIndicatorName(symbol, "EMA" + period, resolution);
            ema = new ExponentialMovingAverage(name, period);
            RegisterIndicator(symbol, ema, resolution, selector);
            return ema;
        }

        /**
        /// Creates an SimpleMovingAverage indicator for the symbol. The indicator will be automatically
        /// updated on the given resolution.
        */
         * @param symbol">The symbol whose SMA we want
         * @param period">The period of the SMA
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The SimpleMovingAverage for the given parameters
        public SimpleMovingAverage SMA(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            String name = CreateIndicatorName(symbol, "SMA" + period, resolution);
            sma = new SimpleMovingAverage(name, period);
            RegisterIndicator(symbol, sma, resolution, selector);
            return sma;
        }

        /**
        /// Creates a MACD indicator for the symbol. The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose MACD we want
         * @param fastPeriod">The period for the fast moving average
         * @param slowPeriod">The period for the slow moving average
         * @param signalPeriod">The period for the signal moving average
         * @param type">The type of moving average to use for the MACD
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The moving average convergence divergence between the fast and slow averages
        public MovingAverageConvergenceDivergence MACD(Symbol symbol, int fastPeriod, int slowPeriod, int signalPeriod, MovingAverageType type = MovingAverageType.Simple, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, String.format( "MACD(%1$s,%2$s)", fastPeriod, slowPeriod), resolution);
            macd = new MovingAverageConvergenceDivergence(name, fastPeriod, slowPeriod, signalPeriod, type);
            RegisterIndicator(symbol, macd, resolution, selector);
            return macd;
        }

        /**
        /// Creates a new Maximum indicator to compute the maximum value
        */
         * @param symbol">The symbol whose max we want
         * @param period">The look back period over which to compute the max value
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null and the symbol is of type TradeBar defaults to the High property, 
        /// otherwise it defaults to Value property of BaseData (x -> x.Value)
        @returns A Maximum indicator that compute the max value and the periods since the max value
        public Maximum MAX(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "MAX" + period, resolution);
            max = new Maximum(name, period);

            // assign a default value for the selector function
            if( selector == null ) {
                subscription = GetSubscription(symbol);
                if( typeof(TradeBar).IsAssignableFrom(subscription.Type)) {
                    // if we have trade bar data we'll use the High property, if not x -> x.Value will be set in RegisterIndicator
                    selector = x -> ((TradeBar)x).High;
                }
            }

            RegisterIndicator(symbol, max, ResolveConsolidator(symbol, resolution), selector);
            return max;
        }

        /**
        /// Creates a new Minimum indicator to compute the minimum value
        */
         * @param symbol">The symbol whose min we want
         * @param period">The look back period over which to compute the min value
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null and the symbol is of type TradeBar defaults to the Low property, 
        /// otherwise it defaults to Value property of BaseData (x -> x.Value)
        @returns A Minimum indicator that compute the in value and the periods since the min value
        public Minimum MIN(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "MIN" + period, resolution);
            min = new Minimum(name, period);

            // assign a default value for the selector function
            if( selector == null ) {
                subscription = GetSubscription(symbol);
                if( typeof (TradeBar).IsAssignableFrom(subscription.Type)) {
                    // if we have trade bar data we'll use the Low property, if not x -> x.Value will be set in RegisterIndicator
                    selector = x -> ((TradeBar) x).Low;
                }
            }

            RegisterIndicator(symbol, min, ResolveConsolidator(symbol, resolution), selector);
            return min;
        }

        /**
        /// Creates a new AroonOscillator indicator which will compute the AroonUp and AroonDown (as well as the delta)
        */
         * @param symbol">The symbol whose Aroon we seek
         * @param period">The look back period for computing number of periods since maximum and minimum
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns An AroonOscillator configured with the specied periods
        public AroonOscillator AROON(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            return AROON(symbol, period, period, resolution, selector);
        }
        
        /**
        /// Creates a new AroonOscillator indicator which will compute the AroonUp and AroonDown (as well as the delta)
        */
         * @param symbol">The symbol whose Aroon we seek
         * @param upPeriod">The look back period for computing number of periods since maximum
         * @param downPeriod">The look back period for computing number of periods since minimum
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns An AroonOscillator configured with the specified periods
        public AroonOscillator AROON(Symbol symbol, int upPeriod, int downPeriod, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, String.format( "AROON(%1$s,%2$s)", upPeriod, downPeriod), resolution);
            aroon = new AroonOscillator(name, upPeriod, downPeriod);
            RegisterIndicator(symbol, aroon, resolution, selector);
            return aroon;
        }

        /**
        /// Creates a new Momentum indicator. This will compute the absolute n-period change in the security.
        /// The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose momentum we want
         * @param period">The period over which to compute the momentum
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The momentum indicator for the requested symbol over the specified period
        public Momentum MOM(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            String name = CreateIndicatorName(symbol, "MOM" + period, resolution);
            momentum = new Momentum(name, period);
            RegisterIndicator(symbol, momentum, resolution, selector);
            return momentum;
        }

        /**
        /// Creates a new MomentumPercent indicator. This will compute the n-period percent change in the security.
        /// The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose momentum we want
         * @param period">The period over which to compute the momentum
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The momentum indicator for the requested symbol over the specified period
        public MomentumPercent MOMP(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            String name = CreateIndicatorName(symbol, "MOMP" + period, resolution);
            momentum = new MomentumPercent(name, period);
            RegisterIndicator(symbol, momentum, resolution, selector);
            return momentum;
        }

        /**
        /// Creates a new RelativeStrengthIndex indicator. This will produce an oscillator that ranges from 0 to 100 based
        /// on the ratio of average gains to average losses over the specified period.
        */
         * @param symbol">The symbol whose RSI we want
         * @param period">The period over which to compute the RSI
         * @param movingAverageType">The type of moving average to use in computing the average gain/loss values
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The RelativeStrengthIndex indicator for the requested symbol over the specified period
        public RelativeStrengthIndex RSI(Symbol symbol, int period, MovingAverageType movingAverageType = MovingAverageType.Simple, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "RSI" + period, resolution);
            rsi = new RelativeStrengthIndex(name, period, movingAverageType);
            RegisterIndicator(symbol, rsi, resolution, selector);
            return rsi;
        }

        /**
        /// Creates a new CommodityChannelIndex indicator. The indicator will be automatically
        /// updated on the given resolution.
        */
         * @param symbol">The symbol whose CCI we want
         * @param period">The period over which to compute the CCI
         * @param movingAverageType">The type of moving average to use in computing the typical price averge
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns The CommodityChannelIndex indicator for the requested symbol over the specified period
        public CommodityChannelIndex CCI(Symbol symbol, int period, MovingAverageType movingAverageType = MovingAverageType.Simple, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "CCI" + period, resolution);
            cci = new CommodityChannelIndex(name, period, movingAverageType);
            RegisterIndicator(symbol, cci, resolution, selector);
            return cci;
        }

        /**
        /// Creates a new MoneyFlowIndex indicator. The indicator will be automatically
        /// updated on the given resolution.
        */
         * @param symbol">The symbol whose MFI we want
         * @param period">The period over which to compute the MFI
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The MoneyFlowIndex indicator for the requested symbol over the specified period
        public MoneyFlowIndex MFI(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "MFI" + period, resolution);
            mfi = new MoneyFlowIndex(name, period);
            RegisterIndicator(symbol, mfi, resolution, selector);
            return mfi;
        }

        /**
        /// Creates a new StandardDeviation indicator. This will return the population standard deviation of samples over the specified period.
        */
         * @param symbol">The symbol whose STD we want
         * @param period">The period over which to compute the STD
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The StandardDeviation indicator for the requested symbol over the speified period
        public StandardDeviation STD(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "STD" + period, resolution);
            std = new StandardDeviation(name, period);
            RegisterIndicator(symbol, std, resolution, selector);
            return std;
        }

        /**
        /// Creates a new BollingerBands indicator which will compute the MiddleBand, UpperBand, LowerBand, and StandardDeviation
        */
         * @param symbol">The symbol whose BollingerBands we seek
         * @param period">The period of the standard deviation and moving average (middle band)
         * @param k">The number of standard deviations specifying the distance between the middle band and upper or lower bands
         * @param movingAverageType">The type of moving average to be used
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns A BollingerBands configured with the specied period
        public BollingerBands BB(Symbol symbol, int period, BigDecimal k, MovingAverageType movingAverageType = MovingAverageType.Simple, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, String.format( "BB(%1$s,%2$s)", period, k), resolution);
            bb = new BollingerBands(name, period, k, movingAverageType);
            RegisterIndicator(symbol, bb, resolution, selector);
            return bb;
        }

        /**
        /// Creates a new RateOfChange indicator. This will compute the n-period rate of change in the security.
        /// The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose RateOfChange we want
         * @param period">The period over which to compute the RateOfChange
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The RateOfChange indicator for the requested symbol over the specified period
        public RateOfChange ROC(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            String name = CreateIndicatorName(symbol, "ROC" + period, resolution);
            rateofchange = new RateOfChange(name, period);
            RegisterIndicator(symbol, rateofchange, resolution, selector);
            return rateofchange;
        }

        /**
        /// Creates a new RateOfChangePercent indicator. This will compute the n-period percentage rate of change in the security.
        /// The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose RateOfChangePercent we want
         * @param period">The period over which to compute the RateOfChangePercent
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The RateOfChangePercent indicator for the requested symbol over the specified period
        public RateOfChangePercent ROCP(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            String name = CreateIndicatorName(symbol, "ROCP" + period, resolution);
            rateofchangepercent = new RateOfChangePercent(name, period);
            RegisterIndicator(symbol, rateofchangepercent, resolution, selector);
            return rateofchangepercent;
        }

        /**
        /// Creates a new Williams %R indicator. This will compute the percentage change of
        /// the current closing price in relation to the high and low of the past N periods.
        /// The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose Williams %R we want
         * @param period">The period over which to compute the Williams %R
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The rateofchangepercent indicator for the requested symbol over the specified period
        public WilliamsPercentR WILR(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            String name = CreateIndicatorName(symbol, "WILR" + period, resolution);
            williamspercentr = new WilliamsPercentR(name, period);
            RegisterIndicator(symbol, williamspercentr, resolution, selector);
            return williamspercentr;
        }

        /**
        /// Creates a new LinearWeightedMovingAverage indicator.  This indicator will linearly distribute
        /// the weights across the periods.  
        */
         * @param symbol">The symbol whose LWMA we want
         * @param period">The period over which to compute the LWMA
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns 
        public LinearWeightedMovingAverage LWMA(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            String name = CreateIndicatorName(symbol, "LWMA" + period, resolution);
            lwma = new LinearWeightedMovingAverage(name, period);
            RegisterIndicator(symbol, lwma, resolution, selector);
            return lwma;
        }

        /**
        /// Creates a new On Balance Volume indicator. This will compute the cumulative total volume
        /// based on whether the close price being higher or lower than the previous period.
        /// The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose On Balance Volume we seek
         * @param resolution">The resolution.
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns The On Balance Volume indicator for the requested symbol.
        public OnBalanceVolume OBV(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "OBV", resolution);
            onBalanceVolume = new OnBalanceVolume(name);
            RegisterIndicator(symbol, onBalanceVolume, resolution, selector);
            return onBalanceVolume;
        }

        /**
        /// Creates a new Average Directional Index indicator. 
        /// The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose Average Directional Index we seek
         * @param resolution">The resolution. 
         * @param period">The period over which to compute the Average Directional Index
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns The Average Directional Index indicator for the requested symbol.
        public AverageDirectionalIndex ADX(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "ADX", resolution);
            averageDirectionalIndex = new AverageDirectionalIndex(name, period);
            RegisterIndicator(symbol, averageDirectionalIndex, resolution, selector);
            return averageDirectionalIndex;
        }

        /**
        /// Creates a new Keltner Channels indicator. 
        /// The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose Keltner Channel we seek
         * @param period">The period over which to compute the Keltner Channels
         * @param k">The number of multiples of the <see cref="AverageTrueRange"/> from the middle band of the Keltner Channels
         * @param movingAverageType">Specifies the type of moving average to be used as the middle line of the Keltner Channel
         * @param resolution">The resolution. 
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns The Keltner Channel indicator for the requested symbol.
        public KeltnerChannels KCH(Symbol symbol, int period, BigDecimal k, MovingAverageType movingAverageType = MovingAverageType.Simple, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "KCH", resolution);
            keltnerChannels = new KeltnerChannels(name, period, k, movingAverageType);
            RegisterIndicator(symbol, keltnerChannels, resolution, selector);
            return keltnerChannels;
        }

        /**
        /// Creates a new Donchian Channel indicator which will compute the Upper Band and Lower Band.
        /// The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose Donchian Channel we seek.
         * @param upperPeriod">The period over which to compute the upper Donchian Channel.
         * @param lowerPeriod">The period over which to compute the lower Donchian Channel.
         * @param resolution">The resolution.
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns The Donchian Channel indicator for the requested symbol.
        public DonchianChannel DCH(Symbol symbol, int upperPeriod, int lowerPeriod, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "DCH", resolution);
            donchianChannel = new DonchianChannel(name, upperPeriod, lowerPeriod);
            RegisterIndicator(symbol, donchianChannel, resolution, selector);
            return donchianChannel;
        }

        /**
        /// Overload shorthand to create a new symmetric Donchian Channel indicator which
        /// has the upper and lower channels set to the same period length.
        */
         * @param symbol">The symbol whose Donchian Channel we seek.
         * @param period">The period over which to compute the Donchian Channel.
         * @param resolution">The resolution.
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns The Donchian Channel indicator for the requested symbol.
        public DonchianChannel DCH(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            return DCH(symbol, period, period, resolution, selector);
        }

        /**
        /// Creates a new Stochastic indicator.
        */
         * @param symbol">The symbol whose stochastic we seek
         * @param resolution">The resolution.
         * @param period">The period of the stochastic. Normally 14
         * @param kPeriod">The sum period of the stochastic. Normally 14
         * @param dPeriod">The sum period of the stochastic. Normally 3
        @returns Stochastic indicator for the requested symbol.
        public Stochastic STO(Symbol symbol, int period, int kPeriod, int dPeriod, Resolution? resolution = null ) {
            String name = CreateIndicatorName(symbol, "STO", resolution);
            stoch = new Stochastic(name, period, kPeriod, dPeriod);
            RegisterIndicator(symbol, stoch, resolution);
            return stoch;
        }

        /**
        /// Overload short hand to create a new Stochastic indicator; defaulting to the 3 period for dStoch
        */
         * @param symbol">The symbol whose stochastic we seek
         * @param resolution">The resolution.
         * @param period">The period of the stochastic. Normally 14
        @returns Stochastic indicator for the requested symbol.
        public Stochastic STO(Symbol symbol, int period, Resolution? resolution = null ) {
            return STO(symbol, period, period, 3, resolution);
        }

        /**
        /// Creates a new LogReturn indicator.
        */
         * @param symbol">The symbol whose log return we seek
         * @param period">The period of the log return.
         * @param resolution">The resolution.
        @returns log return indicator for the requested symbol.
        public LogReturn LOGR(Symbol symbol, int period, Resolution? resolution = null ) {
            String name = CreateIndicatorName(symbol, "LOGR", resolution);
            logr = new LogReturn(name, period);
            RegisterIndicator(symbol, logr, resolution);
            return logr;
        }

        /**
        /// Creates and registers a new Least Squares Moving Average instance.
        */
         * @param symbol">The symbol whose LSMA we seek.
         * @param period">The LSMA period. Normally 14.
         * @param resolution">The resolution.
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar.
        @returns A LeastSquaredMovingAverage configured with the specified period
        public LeastSquaresMovingAverage LSMA(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "LSMA" + period, resolution);
            lsma = new LeastSquaresMovingAverage(name, period);
            RegisterIndicator(symbol, lsma, resolution, selector);
            return lsma;
        }

        /**
        /// Creates a new Parabolic SAR indicator
        */
         * @param symbol">The symbol whose PSAR we seek
         * @param afStart">Acceleration factor start value. Normally 0.02
         * @param afIncrement">Acceleration factor increment value. Normally 0.02
         * @param afMax">Acceleration factor max value. Normally 0.2
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns A ParabolicStopAndReverse configured with the specified periods
        public ParabolicStopAndReverse PSAR(Symbol symbol, BigDecimal afStart = 0.02m, BigDecimal afIncrement = 0.02m, BigDecimal afMax = 0.2m, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, String.format( "PSAR(%1$s,%2$s,%3$s)", afStart, afIncrement, afMax), resolution);
            psar = new ParabolicStopAndReverse(name, afStart, afIncrement, afMax);
            RegisterIndicator(symbol, psar, resolution, selector);
            return psar;
        }

        /**
        /// Creates a new Balance Of Power indicator.
        /// The indicator will be automatically updated on the given resolution.
        */
         * @param symbol">The symbol whose Balance Of Power we seek
         * @param resolution">The resolution.
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar
        @returns The Balance Of Power indicator for the requested symbol.
        public BalanceOfPower BOP(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "BOP", resolution);
            bop = new BalanceOfPower(name);
            RegisterIndicator(symbol, bop, resolution, selector);
            return bop;
        }

        /**
        /// Creates a new Variance indicator. This will return the population variance of samples over the specified period.
        */
         * @param symbol">The symbol whose VAR we want
         * @param period">The period over which to compute the VAR
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The Variance indicator for the requested symbol over the speified period
        public Variance VAR(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "VAR" + period, resolution);
            variance = new Variance(name, period);
            RegisterIndicator(symbol, variance, resolution, selector);
            return variance;
        }

        /**
        /// Creates a new AccumulationDistribution indicator.
        */
         * @param symbol">The symbol whose AD we want
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The AccumulationDistribution indicator for the requested symbol over the speified period
        public AccumulationDistribution AD(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "AD", resolution);
            ad = new AccumulationDistribution(name);
            RegisterIndicator(symbol, ad, resolution, selector);
            return ad;
        }

        /**
        /// Creates a new AccumulationDistributionOscillator indicator.
        */
         * @param symbol">The symbol whose ADOSC we want
         * @param fastPeriod">The fast moving average period
         * @param slowPeriod">The slow moving average period
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The AccumulationDistributionOscillator indicator for the requested symbol over the speified period
        public AccumulationDistributionOscillator ADOSC(Symbol symbol, int fastPeriod, int slowPeriod, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, String.format( "ADOSC(%1$s,%2$s)", fastPeriod, slowPeriod), resolution);
            adOsc = new AccumulationDistributionOscillator(name, fastPeriod, slowPeriod);
            RegisterIndicator(symbol, adOsc, resolution, selector);
            return adOsc;
        }

        /**
        /// Creates a new TrueRange indicator.
        */
         * @param symbol">The symbol whose TR we want
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The TrueRange indicator for the requested symbol.
        public TrueRange TR(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "TR", resolution);
            tr = new TrueRange(name);
            RegisterIndicator(symbol, tr, resolution, selector);
            return tr;
        }

        /**
        /// Creates a new ChandeMomentumOscillator indicator.
        */
         * @param symbol">The symbol whose CMO we want
         * @param period">The period over which to compute the CMO
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The ChandeMomentumOscillator indicator for the requested symbol over the specified period
        public ChandeMomentumOscillator CMO(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "CMO" + period, resolution);
            cmo = new ChandeMomentumOscillator(name, period);
            RegisterIndicator(symbol, cmo, resolution, selector);
            return cmo;
        }

        /**
        /// Creates a new DoubleExponentialMovingAverage indicator.
        */
         * @param symbol">The symbol whose DEMA we want
         * @param period">The period over which to compute the DEMA
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The DoubleExponentialMovingAverage indicator for the requested symbol over the specified period
        public DoubleExponentialMovingAverage DEMA(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "DEMA" + period, resolution);
            dema = new DoubleExponentialMovingAverage(name, period);
            RegisterIndicator(symbol, dema, resolution, selector);
            return dema;
        }

        /**
        /// Creates a new TripleExponentialMovingAverage indicator.
        */
         * @param symbol">The symbol whose TEMA we want
         * @param period">The period over which to compute the TEMA
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The TripleExponentialMovingAverage indicator for the requested symbol over the specified period
        public TripleExponentialMovingAverage TEMA(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "TEMA" + period, resolution);
            tema = new TripleExponentialMovingAverage(name, period);
            RegisterIndicator(symbol, tema, resolution, selector);
            return tema;
        }

        /**
        /// Creates a new TriangularMovingAverage indicator.
        */
         * @param symbol">The symbol whose TRIMA we want
         * @param period">The period over which to compute the TRIMA
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The TriangularMovingAverage indicator for the requested symbol over the specified period
        public TriangularMovingAverage TRIMA(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "TRIMA" + period, resolution);
            trima = new TriangularMovingAverage(name, period);
            RegisterIndicator(symbol, trima, resolution, selector);
            return trima;
        }

        /**
        /// Creates a new RateOfChangeRatio indicator.
        */
         * @param symbol">The symbol whose ROCR we want
         * @param period">The period over which to compute the ROCR
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The RateOfChangeRatio indicator for the requested symbol over the specified period
        public RateOfChangeRatio ROCR(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "ROCR" + period, resolution);
            rocr = new RateOfChangeRatio(name, period);
            RegisterIndicator(symbol, rocr, resolution, selector);
            return rocr;
        }

        /**
        /// Creates a new MeanAbsoluteDeviation indicator.
        */
         * @param symbol">The symbol whose MeanAbsoluteDeviation we want
         * @param period">The period over which to compute the MeanAbsoluteDeviation
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The MeanAbsoluteDeviation indicator for the requested symbol over the specified period
        public MeanAbsoluteDeviation MAD(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "MAD" + period, resolution);
            mad = new MeanAbsoluteDeviation(name, period);
            RegisterIndicator(symbol, mad, resolution, selector);
            return mad;
        }

        /**
        /// Creates a new Momersion indicator.
        */
         * @param symbol">The symbol whose Momersion we want
         * @param minPeriod">The minimum period over which to compute the Momersion
         * @param fullPeriod">The full period over which to compute the Momersion
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The Momersion indicator for the requested symbol over the specified period
        public MomersionIndicator MOMERSION(Symbol symbol, int minPeriod, int fullPeriod, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, String.format( "MOMERSION(%1$s,%2$s)", minPeriod, fullPeriod), resolution);
            momersion = new MomersionIndicator(name, minPeriod, fullPeriod);
            RegisterIndicator(symbol, momersion, resolution, selector);
            return momersion;
        }

        /**
        /// Creates a new Sum indicator.
        */
         * @param symbol">The symbol whose Sum we want
         * @param period">The period over which to compute the Sum
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The Sum indicator for the requested symbol over the specified period
        public Sum SUM(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "SUM" + period, resolution);
            sum = new Sum(name, period);
            RegisterIndicator(symbol, sum, resolution, selector);
            return sum;
        }

        /**
        /// Creates a new T3MovingAverage indicator.
        */
         * @param symbol">The symbol whose T3 we want
         * @param period">The period over which to compute the T3
         * @param volumeFactor">The volume factor to be used for the T3 (value must be in the [0,1] range, defaults to 0.7)
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The T3MovingAverage indicator for the requested symbol over the specified period
        public T3MovingAverage T3(Symbol symbol, int period, BigDecimal volumeFactor = 0.7m, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, String.format( "T3(%1$s,%2$s)", period, volumeFactor), resolution);
            t3 = new T3MovingAverage(name, period, volumeFactor);
            RegisterIndicator(symbol, t3, resolution, selector);
            return t3;
        }

        /**
        /// Creates a new NormalizedAverageTrueRange indicator.
        */
         * @param symbol">The symbol whose NATR we want
         * @param period">The period over which to compute the NATR
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The NormalizedAverageTrueRange indicator for the requested symbol over the specified period
        public NormalizedAverageTrueRange NATR(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "NATR" + period, resolution);
            natr = new NormalizedAverageTrueRange(name, period);
            RegisterIndicator(symbol, natr, resolution, selector);
            return natr;
        }

        /**
        /// Creates a new Heikin-Ashi indicator.
        */
         * @param symbol">The symbol whose Heikin-Ashi we want
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The Heikin-Ashi indicator for the requested symbol over the specified period
        public HeikinAshi HeikinAshi(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "HA", resolution);
            ha = new HeikinAshi(name);
            RegisterIndicator(symbol, ha, resolution, selector);
            return ha;
        }

        /**
        /// Creates a new AverageDirectionalMovementIndexRating indicator.
        */
         * @param symbol">The symbol whose ADXR we want
         * @param period">The period over which to compute the ADXR
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The AverageDirectionalMovementIndexRating indicator for the requested symbol over the specified period
        public AverageDirectionalMovementIndexRating ADXR(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "ADXR" + period, resolution);
            adxr = new AverageDirectionalMovementIndexRating(name, period);
            RegisterIndicator(symbol, adxr, resolution, selector);
            return adxr;
        }

        /**
        /// Creates a new KaufmanAdaptiveMovingAverage indicator.
        */
         * @param symbol">The symbol whose KAMA we want
         * @param period">The period over which to compute the KAMA
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The KaufmanAdaptiveMovingAverage indicator for the requested symbol over the specified period
        public KaufmanAdaptiveMovingAverage KAMA(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "KAMA" + period, resolution);
            kama = new KaufmanAdaptiveMovingAverage(name, period);
            RegisterIndicator(symbol, kama, resolution, selector);
            return kama;
        }

        /**
        /// Creates a new MidPoint indicator.
        */
         * @param symbol">The symbol whose MIDPOINT we want
         * @param period">The period over which to compute the MIDPOINT
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The MidPoint indicator for the requested symbol over the specified period
        public MidPoint MIDPOINT(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "MIDPOINT" + period, resolution);
            midpoint = new MidPoint(name, period);
            RegisterIndicator(symbol, midpoint, resolution, selector);
            return midpoint;
        }

        /**
        /// Creates a new UltimateOscillator indicator.
        */
         * @param symbol">The symbol whose ULTOSC we want
         * @param period1">The first period over which to compute the ULTOSC
         * @param period2">The second period over which to compute the ULTOSC
         * @param period3">The third period over which to compute the ULTOSC
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The UltimateOscillator indicator for the requested symbol over the specified period
        public UltimateOscillator ULTOSC(Symbol symbol, int period1, int period2, int period3, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, String.format( "ULTOSC(%1$s,%2$s,%3$s)", period1, period2, period3), resolution);
            ultosc = new UltimateOscillator(name, period1, period2, period3);
            RegisterIndicator(symbol, ultosc, resolution, selector);
            return ultosc;
        }

        /**
        /// Creates a new Trix indicator.
        */
         * @param symbol">The symbol whose TRIX we want
         * @param period">The period over which to compute the TRIX
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The Trix indicator for the requested symbol over the specified period
        public Trix TRIX(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, "TRIX" + period, resolution);
            trix = new Trix(name, period);
            RegisterIndicator(symbol, trix, resolution, selector);
            return trix;
        }

        /**
        /// Creates a new MidPrice indicator.
        */
         * @param symbol">The symbol whose MIDPRICE we want
         * @param period">The period over which to compute the MIDPRICE
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The MidPrice indicator for the requested symbol over the specified period
        public MidPrice MIDPRICE(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "MIDPRICE" + period, resolution);
            midprice = new MidPrice(name, period);
            RegisterIndicator(symbol, midprice, resolution, selector);
            return midprice;
        }

        /**
        /// Creates a new AbsolutePriceOscillator indicator.
        */
         * @param symbol">The symbol whose APO we want
         * @param fastPeriod">The fast moving average period
         * @param slowPeriod">The slow moving average period
         * @param movingAverageType">The type of moving average to use
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The AbsolutePriceOscillator indicator for the requested symbol over the specified period
        public AbsolutePriceOscillator APO(Symbol symbol, int fastPeriod, int slowPeriod, MovingAverageType movingAverageType, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, String.format( "APO(%1$s,%2$s)", fastPeriod, slowPeriod), resolution);
            apo = new AbsolutePriceOscillator(name, fastPeriod, slowPeriod, movingAverageType);
            RegisterIndicator(symbol, apo, resolution, selector);
            return apo;
        }

        /**
        /// Creates a new PercentagePriceOscillator indicator.
        */
         * @param symbol">The symbol whose PPO we want
         * @param fastPeriod">The fast moving average period
         * @param slowPeriod">The slow moving average period
         * @param movingAverageType">The type of moving average to use
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The PercentagePriceOscillator indicator for the requested symbol over the specified period
        public PercentagePriceOscillator PPO(Symbol symbol, int fastPeriod, int slowPeriod, MovingAverageType movingAverageType, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            name = CreateIndicatorName(symbol, String.format( "PPO(%1$s,%2$s)", fastPeriod, slowPeriod), resolution);
            ppo = new PercentagePriceOscillator(name, fastPeriod, slowPeriod, movingAverageType);
            RegisterIndicator(symbol, ppo, resolution, selector);
            return ppo;
        }

        /**
        /// Creates an VolumeWeightedAveragePrice (VWAP) indicator for the symbol. The indicator will be automatically
        /// updated on the given resolution.
        */
         * @param symbol">The symbol whose VWAP we want
         * @param period">The period of the VWAP
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The VolumeWeightedAveragePrice for the given parameters
        public VolumeWeightedAveragePriceIndicator VWAP(Symbol symbol, int period, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null ) {
            name = CreateIndicatorName(symbol, "VWAP" + period, resolution);
            vwap = new VolumeWeightedAveragePriceIndicator(name, period);
            RegisterIndicator(symbol, vwap, resolution, selector);
            return vwap;
        }

        /**
        /// Creates an FractalAdaptiveMovingAverage (FRAMA) indicator for the symbol. The indicator will be automatically
        /// updated on the given resolution.
        */
         * @param symbol">The symbol whose FRAMA we want
         * @param period">The period of the FRAMA
         * @param longPeriod">The long period of the FRAMA
         * @param resolution">The resolution
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The FRAMA for the given parameters
        public FractalAdaptiveMovingAverage FRAMA(Symbol symbol, int period, int longPeriod = 198, Resolution? resolution = null ) {
            name = CreateIndicatorName(symbol, "FRAMA" + period, resolution);
            frama = new FractalAdaptiveMovingAverage(name, period, longPeriod);
            RegisterIndicator(symbol, frama, resolution);
            return frama;
        }

        /**
        /// Creates Swiss Army Knife transformation for the symbol. The indicator will be automatically
        /// updated on the given resolution.
        */
         * @param symbol">The symbol to use for calculations
         * @param period">The period of the calculation
         * @param delta">The delta scale of the BandStop or BandPass
         * @param tool">The tool os the Swiss Army Knife
         * @param resolution">The resolution
         * @param selector">elects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        @returns The calculation using the given tool
        public SwissArmyKnife SWISS(Symbol symbol, int period, double delta, SwissArmyKnifeTool tool, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            String name = CreateIndicatorName(symbol, "SWISS" + period, resolution);
            swiss = new SwissArmyKnife(name, period, delta, tool);
            RegisterIndicator(symbol, swiss, resolution, selector);
            return swiss;
        }

        /**
        /// Creates and registers a new consolidator to receive automatic updates at the specified resolution as well as configures
        /// the indicator to receive updates from the consolidator.
        */
         * @param symbol">The symbol to register against
         * @param indicator">The indicator to receive data from the consolidator
         * @param resolution">The resolution at which to send data to the indicator, null to use the same resolution as the subscription
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        public void RegisterIndicator(Symbol symbol, IndicatorBase<IndicatorDataPoint> indicator, Resolution? resolution = null, Func<BaseData, decimal> selector = null ) {
            RegisterIndicator(symbol, indicator, ResolveConsolidator(symbol, resolution), selector ?? (x -> x.Value));
        }

        /**
        /// Creates and registers a new consolidator to receive automatic updates at the specified resolution as well as configures
        /// the indicator to receive updates from the consolidator.
        */
         * @param symbol">The symbol to register against
         * @param indicator">The indicator to receive data from the consolidator
         * @param resolution">The resolution at which to send data to the indicator, null to use the same resolution as the subscription
         * @param selector">Selects a value from the BaseData to send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        public void RegisterIndicator(Symbol symbol, IndicatorBase<IndicatorDataPoint> indicator, TimeSpan? resolution = null, Func<BaseData, decimal> selector = null ) {
            RegisterIndicator(symbol, indicator, ResolveConsolidator(symbol, resolution), selector ?? (x -> x.Value));
        }

        /**
        /// Registers the consolidator to receive automatic updates as well as configures the indicator to receive updates
        /// from the consolidator.
        */
         * @param symbol">The symbol to register against
         * @param indicator">The indicator to receive data from the consolidator
         * @param consolidator">The consolidator to receive raw subscription data
         * @param selector">Selects a value from the BaseData send into the indicator, if null defaults to the Value property of BaseData (x -> x.Value)
        public void RegisterIndicator(Symbol symbol, IndicatorBase<IndicatorDataPoint> indicator, IDataConsolidator consolidator, Func<BaseData, decimal> selector = null ) {
            // default our selector to the Value property on BaseData
            selector = selector ?? (x -> x.Value);

            // register the consolidator for automatic updates via SubscriptionManager
            SubscriptionManager.AddConsolidator(symbol, consolidator);

            // attach to the DataConsolidated event so it updates our indicator
            consolidator.DataConsolidated += (sender, consolidated) =>
            {
                value = selector(consolidated);
                indicator.Update(new IndicatorDataPoint(consolidated.Symbol, consolidated.EndTime, value));
            };
        }

        /**
        /// Registers the consolidator to receive automatic updates as well as configures the indicator to receive updates
        /// from the consolidator.
        */
         * @param symbol">The symbol to register against
         * @param indicator">The indicator to receive data from the consolidator
         * @param resolution">The resolution at which to send data to the indicator, null to use the same resolution as the subscription
        public void RegisterIndicator<T>(Symbol symbol, IndicatorBase<T> indicator, Resolution? resolution = null )
            where T : BaseData
        {
            RegisterIndicator(symbol, indicator, ResolveConsolidator(symbol, resolution));
        }

        /**
        /// Registers the consolidator to receive automatic updates as well as configures the indicator to receive updates
        /// from the consolidator.
        */
         * @param symbol">The symbol to register against
         * @param indicator">The indicator to receive data from the consolidator
         * @param resolution">The resolution at which to send data to the indicator, null to use the same resolution as the subscription
         * @param selector">Selects a value from the BaseData send into the indicator, if null defaults to a cast (x -> (T)x)
        public void RegisterIndicator<T>(Symbol symbol, IndicatorBase<T> indicator, Resolution? resolution, Func<BaseData, T> selector)
            where T : BaseData
        {
            RegisterIndicator(symbol, indicator, ResolveConsolidator(symbol, resolution), selector);
        }

        /**
        /// Registers the consolidator to receive automatic updates as well as configures the indicator to receive updates
        /// from the consolidator.
        */
         * @param symbol">The symbol to register against
         * @param indicator">The indicator to receive data from the consolidator
         * @param resolution">The resolution at which to send data to the indicator, null to use the same resolution as the subscription
         * @param selector">Selects a value from the BaseData send into the indicator, if null defaults to a cast (x -> (T)x)
        public void RegisterIndicator<T>(Symbol symbol, IndicatorBase<T> indicator, TimeSpan? resolution, Func<BaseData, T> selector = null )
            where T : BaseData
        {
            RegisterIndicator(symbol, indicator, ResolveConsolidator(symbol, resolution), selector);
        }

        /**
        /// Registers the consolidator to receive automatic updates as well as configures the indicator to receive updates
        /// from the consolidator.
        */
         * @param symbol">The symbol to register against
         * @param indicator">The indicator to receive data from the consolidator
         * @param consolidator">The consolidator to receive raw subscription data
         * @param selector">Selects a value from the BaseData send into the indicator, if null defaults to a cast (x -> (T)x)
        public void RegisterIndicator<T>(Symbol symbol, IndicatorBase<T> indicator, IDataConsolidator consolidator, Func<BaseData, T> selector = null ) 
            where T : BaseData
        {
            // assign default using cast
            selector = selector ?? (x -> (T) x);

            // register the consolidator for automatic updates via SubscriptionManager
            SubscriptionManager.AddConsolidator(symbol, consolidator);

            // check the output type of the consolidator and verify we can assign it to T
            type = typeof(T);
            if( !type.IsAssignableFrom(consolidator.OutputType)) {
                throw new ArgumentException( String.format( "Type mismatch found between consolidator and indicator for symbol: %1$s." +
                    "Consolidator outputs type %2$s but indicator expects input type %3$s",
                    symbol, consolidator.OutputType.Name, type.Name)
                    );
            }

            // attach to the DataConsolidated event so it updates our indicator
            consolidator.DataConsolidated += (sender, consolidated) =>
            {
                value = selector(consolidated);
                indicator.Update(value);
            };
        }

        /**
        /// Gets the default consolidator for the specified symbol and resolution
        */
         * @param symbol">The symbo whose data is to be consolidated
         * @param resolution">The resolution for the consolidator, if null, uses the resolution from subscription
        @returns The new default consolidator
        public IDataConsolidator ResolveConsolidator(Symbol symbol, Resolution? resolution) {
            subscription = GetSubscription(symbol);

            // if the resolution is null or if the requested resolution matches the subscription, return identity
            if( !resolution.HasValue || subscription.Resolution == resolution.Value) {
                // since there's a generic type parameter that we don't have access to, we'll just use the activator
                identityConsolidatorType = typeof(IdentityDataConsolidator<>).MakeGenericType(subscription.Type);
                return (IDataConsolidator)Activator.CreateInstance(identityConsolidatorType);
            }

            timeSpan = resolution.Value.ToTimeSpan();

            // verify this consolidator will give reasonable results, if someone asks for second consolidation but we have minute
            // data we won't be able to do anything good, we'll call it second, but it would really just be minute!
            if( timeSpan < subscription.Resolution.ToTimeSpan()) {
                throw new ArgumentException( String.format( "Unable to create %1$s %2$s consolidator because %1$s is registered for %3$s data. " +
                    "Consolidators require higher resolution data to produce lower resolution data.",
                    symbol, resolution.Value, subscription.Resolution)
                    );
            }

            return ResolveConsolidator(symbol, timeSpan);
        }

        /**
        /// Gets the default consolidator for the specified symbol and resolution
        */
         * @param symbol">The symbo whose data is to be consolidated
         * @param timeSpan">The requested time span for the consolidator, if null, uses the resolution from subscription
        @returns The new default consolidator
        public IDataConsolidator ResolveConsolidator(Symbol symbol, TimeSpan? timeSpan) {
            subscription = GetSubscription(symbol);

            // if the time span is null or if the requested time span matches the subscription, return identity
            if( !timeSpan.HasValue || subscription.Resolution.ToTimeSpan() == timeSpan.Value) {
                // since there's a generic type parameter that we don't have access to, we'll just use the activator
                identityConsolidatorType = typeof(IdentityDataConsolidator<>).MakeGenericType(subscription.Type);
                return (IDataConsolidator)Activator.CreateInstance(identityConsolidatorType);
            }

            // verify this consolidator will give reasonable results, if someone asks for second consolidation but we have minute
            // data we won't be able to do anything good, we'll call it second, but it would really just be minute!
            if( timeSpan.Value < subscription.Resolution.ToTimeSpan()) {
                throw new ArgumentException( String.format( "Unable to create %1$s consolidator because %1$s is registered for %2$s data. " +
                    "Consolidators require higher resolution data to produce lower resolution data.",
                    symbol, subscription.Resolution)
                    );
            }

            // if our type can be used as a trade bar, then let's just make one of those
            // we use IsAssignableFrom instead of IsSubclassOf so that we can account for types that are able to be cast to TradeBar
            if( typeof(TradeBar).IsAssignableFrom(subscription.Type)) {
                return new TradeBarConsolidator(timeSpan.Value);
            }

            // if our type can be used as a tick then we'll use the tick consolidator
            // we use IsAssignableFrom instead of IsSubclassOf so that we can account for types that are able to be cast to Tick
            if( typeof(Tick).IsAssignableFrom(subscription.Type)) {
                return new TickConsolidator(timeSpan.Value);
            }

            // if our type can be used as a DynamicData then we'll use the DynamicDataConsolidator
            if( typeof(DynamicData).IsAssignableFrom(subscription.Type)) {
                return new DynamicDataConsolidator(timeSpan.Value);
            }

            // no matter what we can always consolidate based on the time-value pair of BaseData
            return new BaseDataConsolidator(timeSpan.Value);
        }

        /**
        /// Gets the SubscriptionDataConfig for the specified symbol
        */
        /// <exception cref="InvalidOperationException">Thrown if no configuration is found for the requested symbol</exception>
         * @param symbol">The symbol to retrieve configuration for
        @returns The SubscriptionDataConfig for the specified symbol
        protected SubscriptionDataConfig GetSubscription(Symbol symbol) {
            SubscriptionDataConfig subscription;
            try
            {
                // find our subscription to this symbol
                subscription = SubscriptionManager.Subscriptions.First(x -> x.Symbol == symbol);
            }
            catch (InvalidOperationException) {
                // this will happen if we did not find the subscription, let's give the user a decent error message
                throw new Exception( "Please register to receive data for symbol '" + symbol.toString() + "' using the AddSecurity() function.");
            }
            return subscription;
        }

        /**
        /// Creates a new name for an indicator created with the convenience functions (SMA, EMA, ect...)
        */
         * @param symbol">The symbol this indicator is registered to
         * @param type">The indicator type, for example, 'SMA5'
         * @param resolution">The resolution requested
        @returns A unique for the given parameters
        public String CreateIndicatorName(Symbol symbol, String type, Resolution? resolution) {
            if( !resolution.HasValue) {
                resolution = GetSubscription(symbol).Resolution;
            }
            String res;
            switch (resolution) {
                case Resolution.Tick:
                    res = "_tick";
                    break;
                case Resolution.Second:
                    res = "_sec";
                    break;
                case Resolution.Minute:
                    res = "_min";
                    break;
                case Resolution.Hour:
                    res = "_hr";
                    break;
                case Resolution.Daily:
                    res = "_day";
                    break;
                case null:
                    res = string.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException( "resolution");
            }

            return String.format( "%1$s(%2$s%3$s)", type, symbol.toString(), res);
        }

    } // End Partial Algorithm Template - Indicators.

} // End QC Namespace
