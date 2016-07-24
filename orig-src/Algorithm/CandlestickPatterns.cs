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
 *
*/

using System;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators.CandlestickPatterns;

namespace QuantConnect.Algorithm
{
    /// <summary>
    /// Provides helpers for using candlestick patterns
    /// </summary>
    public class CandlestickPatterns
    {
        private readonly QCAlgorithm _algorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="CandlestickPatterns"/> class
        /// </summary>
        /// <param name="algorithm">The algorithm instance</param>
        public CandlestickPatterns(QCAlgorithm algorithm)
        {
            _algorithm = algorithm;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.TwoCrows"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public TwoCrows TwoCrows(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "TWOCROWS", resolution);
            pattern = new TwoCrows(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.ThreeBlackCrows"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public ThreeBlackCrows ThreeBlackCrows(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "THREEBLACKCROWS", resolution);
            pattern = new ThreeBlackCrows(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.ThreeInside"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public ThreeInside ThreeInside(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "THREEINSIDE", resolution);
            pattern = new ThreeInside(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.ThreeLineStrike"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public ThreeLineStrike ThreeLineStrike(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "THREELINESTRIKE", resolution);
            pattern = new ThreeLineStrike(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.ThreeOutside"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public ThreeOutside ThreeOutside(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "THREEOUTSIDE", resolution);
            pattern = new ThreeOutside(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.ThreeStarsInSouth"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public ThreeStarsInSouth ThreeStarsInSouth(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "THREESTARSINSOUTH", resolution);
            pattern = new ThreeStarsInSouth(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.ThreeWhiteSoldiers"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public ThreeWhiteSoldiers ThreeWhiteSoldiers(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "THREEWHITESOLDIERS", resolution);
            pattern = new ThreeWhiteSoldiers(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.AbandonedBaby"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="penetration">Percentage of penetration of a candle within another candle</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public AbandonedBaby AbandonedBaby(Symbol symbol, BigDecimal penetration = 0.3m, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "ABANDONEDBABY", resolution);
            pattern = new AbandonedBaby(name, penetration);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.AdvanceBlock"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public AdvanceBlock AdvanceBlock(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "ADVANCEBLOCK", resolution);
            pattern = new AdvanceBlock(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.BeltHold"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public BeltHold BeltHold(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "BELTHOLD", resolution);
            pattern = new BeltHold(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Breakaway"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Breakaway Breakaway(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "BREAKAWAY", resolution);
            pattern = new Breakaway(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.ClosingMarubozu"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public ClosingMarubozu ClosingMarubozu(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "CLOSINGMARUBOZU", resolution);
            pattern = new ClosingMarubozu(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.ConcealedBabySwallow"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public ConcealedBabySwallow ConcealedBabySwallow(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "CONCEALEDBABYSWALLOW", resolution);
            pattern = new ConcealedBabySwallow(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Counterattack"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Counterattack Counterattack(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "COUNTERATTACK", resolution);
            pattern = new Counterattack(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.DarkCloudCover"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="penetration">Percentage of penetration of a candle within another candle</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public DarkCloudCover DarkCloudCover(Symbol symbol, BigDecimal penetration = 0.5m, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "DARKCLOUDCOVER", resolution);
            pattern = new DarkCloudCover(name, penetration);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Doji"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Doji Doji(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "DOJI", resolution);
            pattern = new Doji(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.DojiStar"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public DojiStar DojiStar(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "DOJISTAR", resolution);
            pattern = new DojiStar(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.DragonflyDoji"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public DragonflyDoji DragonflyDoji(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "DRAGONFLYDOJI", resolution);
            pattern = new DragonflyDoji(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Engulfing"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Engulfing Engulfing(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "ENGULFING", resolution);
            pattern = new Engulfing(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.EveningDojiStar"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="penetration">Percentage of penetration of a candle within another candle</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public EveningDojiStar EveningDojiStar(Symbol symbol, BigDecimal penetration = 0.3m, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "EVENINGDOJISTAR", resolution);
            pattern = new EveningDojiStar(name, penetration);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.EveningStar"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="penetration">Percentage of penetration of a candle within another candle</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public EveningStar EveningStar(Symbol symbol, BigDecimal penetration = 0.3m, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "EVENINGSTAR", resolution);
            pattern = new EveningStar(name, penetration);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.GapSideBySideWhite"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public GapSideBySideWhite GapSideBySideWhite(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "GAPSIDEBYSIDEWHITE", resolution);
            pattern = new GapSideBySideWhite(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.GravestoneDoji"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public GravestoneDoji GravestoneDoji(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "GRAVESTONEDOJI", resolution);
            pattern = new GravestoneDoji(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Hammer"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Hammer Hammer(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "HAMMER", resolution);
            pattern = new Hammer(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.HangingMan"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public HangingMan HangingMan(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "HANGINGMAN", resolution);
            pattern = new HangingMan(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Harami"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Harami Harami(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "HARAMI", resolution);
            pattern = new Harami(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.HaramiCross"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public HaramiCross HaramiCross(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "HARAMICROSS", resolution);
            pattern = new HaramiCross(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.HighWaveCandle"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public HighWaveCandle HighWaveCandle(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "HIGHWAVECANDLE", resolution);
            pattern = new HighWaveCandle(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Hikkake"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Hikkake Hikkake(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "HIKKAKE", resolution);
            pattern = new Hikkake(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.HikkakeModified"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public HikkakeModified HikkakeModified(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "HIKKAKEMODIFIED", resolution);
            pattern = new HikkakeModified(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.HomingPigeon"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public HomingPigeon HomingPigeon(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "HOMINGPIGEON", resolution);
            pattern = new HomingPigeon(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.IdenticalThreeCrows"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public IdenticalThreeCrows IdenticalThreeCrows(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "IDENTICALTHREECROWS", resolution);
            pattern = new IdenticalThreeCrows(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.InNeck"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public InNeck InNeck(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "INNECK", resolution);
            pattern = new InNeck(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.InvertedHammer"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public InvertedHammer InvertedHammer(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "INVERTEDHAMMER", resolution);
            pattern = new InvertedHammer(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Kicking"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Kicking Kicking(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "KICKING", resolution);
            pattern = new Kicking(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.KickingByLength"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public KickingByLength KickingByLength(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "KICKINGBYLENGTH", resolution);
            pattern = new KickingByLength(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.LadderBottom"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public LadderBottom LadderBottom(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "LADDERBOTTOM", resolution);
            pattern = new LadderBottom(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.LongLeggedDoji"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public LongLeggedDoji LongLeggedDoji(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "LONGLEGGEDDOJI", resolution);
            pattern = new LongLeggedDoji(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.LongLineCandle"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public LongLineCandle LongLineCandle(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "LONGLINECANDLE", resolution);
            pattern = new LongLineCandle(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Marubozu"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Marubozu Marubozu(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "MARUBOZU", resolution);
            pattern = new Marubozu(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.MatchingLow"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public MatchingLow MatchingLow(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "MATCHINGLOW", resolution);
            pattern = new MatchingLow(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.MatHold"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="penetration">Percentage of penetration of a candle within another candle</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public MatHold MatHold(Symbol symbol, BigDecimal penetration = 0.5m, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "MATHOLD", resolution);
            pattern = new MatHold(name, penetration);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.MorningDojiStar"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="penetration">Percentage of penetration of a candle within another candle</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public MorningDojiStar MorningDojiStar(Symbol symbol, BigDecimal penetration = 0.3m, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "MORNINGDOJISTAR", resolution);
            pattern = new MorningDojiStar(name, penetration);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.MorningStar"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="penetration">Percentage of penetration of a candle within another candle</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public MorningStar MorningStar(Symbol symbol, BigDecimal penetration = 0.3m, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "MORNINGSTAR", resolution);
            pattern = new MorningStar(name, penetration);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.OnNeck"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public OnNeck OnNeck(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "ONNECK", resolution);
            pattern = new OnNeck(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Piercing"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Piercing Piercing(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "PIERCING", resolution);
            pattern = new Piercing(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.RickshawMan"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public RickshawMan RickshawMan(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "RICKSHAWMAN", resolution);
            pattern = new RickshawMan(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.RiseFallThreeMethods"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public RiseFallThreeMethods RiseFallThreeMethods(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "RISEFALLTHREEMETHODS", resolution);
            pattern = new RiseFallThreeMethods(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.SeparatingLines"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public SeparatingLines SeparatingLines(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "SEPARATINGLINES", resolution);
            pattern = new SeparatingLines(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.ShootingStar"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public ShootingStar ShootingStar(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "SHOOTINGSTAR", resolution);
            pattern = new ShootingStar(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.ShortLineCandle"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public ShortLineCandle ShortLineCandle(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "SHORTLINECANDLE", resolution);
            pattern = new ShortLineCandle(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.SpinningTop"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public SpinningTop SpinningTop(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "SPINNINGTOP", resolution);
            pattern = new SpinningTop(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.StalledPattern"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public StalledPattern StalledPattern(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "STALLEDPATTERN", resolution);
            pattern = new StalledPattern(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.StickSandwich"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public StickSandwich StickSandwich(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "STICKSANDWICH", resolution);
            pattern = new StickSandwich(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Takuri"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Takuri Takuri(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "TAKURI", resolution);
            pattern = new Takuri(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.TasukiGap"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public TasukiGap TasukiGap(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "TASUKIGAP", resolution);
            pattern = new TasukiGap(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Thrusting"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Thrusting Thrusting(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "THRUSTING", resolution);
            pattern = new Thrusting(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.Tristar"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public Tristar Tristar(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "TRISTAR", resolution);
            pattern = new Tristar(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.UniqueThreeRiver"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public UniqueThreeRiver UniqueThreeRiver(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "UNIQUETHREERIVER", resolution);
            pattern = new UniqueThreeRiver(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.UpsideGapTwoCrows"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public UpsideGapTwoCrows UpsideGapTwoCrows(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "UPSIDEGAPTWOCROWS", resolution);
            pattern = new UpsideGapTwoCrows(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }

        /// <summary>
        /// Creates a new <see cref="Indicators.CandlestickPatterns.UpDownGapThreeMethods"/> pattern indicator.
        /// The indicator will be automatically updated on the given resolution.
        /// </summary>
        /// <param name="symbol">The symbol whose pattern we seek</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="selector">Selects a value from the BaseData to send into the indicator, if null defaults to casting the input value to a TradeBar</param>
        /// <returns>The pattern indicator for the requested symbol.</returns>
        public UpDownGapThreeMethods UpDownGapThreeMethods(Symbol symbol, Resolution? resolution = null, Func<BaseData, TradeBar> selector = null)
        {
            name = _algorithm.CreateIndicatorName(symbol, "UPDOWNGAPTHREEMETHODS", resolution);
            pattern = new UpDownGapThreeMethods(name);
            _algorithm.RegisterIndicator(symbol, pattern, resolution, selector);
            return pattern;
        }
    }
}
