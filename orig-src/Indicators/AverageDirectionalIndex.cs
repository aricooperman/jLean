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
using QuantConnect.Data.Market;

package com.quantconnect.lean.Indicators
{
    /**
     * This indicator computes Average Directional Index which measures trend strength without regard to trend direction.
     * Firstly, it calculates the Directional Movement and the True Range value, and then the values are accumulated and smoothed
     * using a custom smoothing method proposed by Wilder. For an n period smoothing, 1/n of each period's value is added to the total period.
     * From these accumulated values we are therefore able to derived the 'Positive Directional Index' (+DI) and 'Negative Directional Index' (-DI)
     * which is used to calculate the Average Directional Index.
    */
    public class AverageDirectionalIndex : IndicatorBase<TradeBar>
    {
        private TradeBar _previousInput;

        private final int _period;

        private IndicatorBase<TradeBar> TrueRange { get; set; }

        private IndicatorBase<TradeBar> DirectionalMovementPlus { get; set; }

        private IndicatorBase<TradeBar> DirectionalMovementMinus { get; set; }

        private IndicatorBase<IndicatorDataPoint> SmoothedDirectionalMovementPlus { get; set; }

        private IndicatorBase<IndicatorDataPoint> SmoothedDirectionalMovementMinus { get; set; }

        private IndicatorBase<IndicatorDataPoint> SmoothedTrueRange { get; set; }

        /**
         * Gets or sets the index of the Plus Directional Indicator
        */
         * <value>
         * The  index of the Plus Directional Indicator.
         * 
        public IndicatorBase<IndicatorDataPoint> PositiveDirectionalIndex { get; private set; }

        /**
         * Gets or sets the index of the Minus Directional Indicator
        */
         * <value>
         * The index of the Minus Directional Indicator.
         * 
        public IndicatorBase<IndicatorDataPoint> NegativeDirectionalIndex { get; private set; }

        /**
         * Initializes a new instance of the <see cref="AverageDirectionalIndex"/> class.
        */
         * @param name The name.
         * @param period The period.
        public AverageDirectionalIndex( String name, int period)
            : base(name) {
            _period = period;

            TrueRange = new FunctionalIndicator<TradeBar>(name + "_TrueRange",
                currentBar =>
                {
                    value = ComputeTrueRange(currentBar);
                    return value;
                },
                isReady -> _previousInput != null
                );

            DirectionalMovementPlus = new FunctionalIndicator<TradeBar>(name + "_PositiveDirectionalMovement",
                currentBar =>
                {
                    value = ComputePositiveDirectionalMovement(currentBar);
                    return value;
                },
                isReady -> _previousInput != null
                );


            DirectionalMovementMinus = new FunctionalIndicator<TradeBar>(name + "_NegativeDirectionalMovement",
                currentBar =>
                {
                    value = ComputeNegativeDirectionalMovement(currentBar);
                    return value;
                },
                isReady -> _previousInput != null
                );

            PositiveDirectionalIndex = new FunctionalIndicator<IndicatorDataPoint>(name + "_PositiveDirectionalIndex",
                input -> ComputePositiveDirectionalIndex(),
                positiveDirectionalIndex -> DirectionalMovementPlus.IsReady && TrueRange.IsReady,
                () =>
                {
                    DirectionalMovementPlus.Reset();
                    TrueRange.Reset();
                }
                );

            NegativeDirectionalIndex = new FunctionalIndicator<IndicatorDataPoint>(name + "_NegativeDirectionalIndex",
                input -> ComputeNegativeDirectionalIndex(),
                negativeDirectionalIndex -> DirectionalMovementMinus.IsReady && TrueRange.IsReady,
                () =>
                {
                    DirectionalMovementMinus.Reset();
                    TrueRange.Reset();
                }
                );

            SmoothedTrueRange = new FunctionalIndicator<IndicatorDataPoint>(name + "_SmoothedTrueRange",
                    currentBar -> ComputeSmoothedTrueRange(period),
                    isReady -> _previousInput != null
                );


            SmoothedDirectionalMovementPlus = new FunctionalIndicator<IndicatorDataPoint>(name + "_SmoothedDirectionalMovementPlus",
                    currentBar -> ComputeSmoothedDirectionalMovementPlus(period),
                    isReady -> _previousInput != null
                );

            SmoothedDirectionalMovementMinus = new FunctionalIndicator<IndicatorDataPoint>(name + "_SmoothedDirectionalMovementMinus",
                    currentBar -> ComputeSmoothedDirectionalMovementMinus(period),
                    isReady -> _previousInput != null
                );
        }

        /**
         * Computes the Smoothed Directional Movement Plus value.
        */
         * @param period The period.
        @returns 
        private BigDecimal ComputeSmoothedDirectionalMovementPlus(int period) {

            BigDecimal value;

            if( Samples < period) {
                value = SmoothedDirectionalMovementPlus.Current + DirectionalMovementPlus.Current;
            }
            else
            {
                value = SmoothedDirectionalMovementPlus.Current - (SmoothedDirectionalMovementPlus.Current / period) + DirectionalMovementPlus.Current;
            }

            return value;
        }

        /**
         * Computes the Smoothed Directional Movement Minus value.
        */
         * @param period The period.
        @returns 
        private BigDecimal ComputeSmoothedDirectionalMovementMinus(int period) {
            BigDecimal value;

            if( Samples < period) {
                value = SmoothedDirectionalMovementMinus.Current + DirectionalMovementMinus.Current;
            }
            else
            {
                value = SmoothedDirectionalMovementMinus.Current - (SmoothedDirectionalMovementMinus.Current / 14) + DirectionalMovementMinus.Current;
            }

            return value;
        }

        /**
         * Computes the Smoothed True Range value.
        */
         * @param period The period.
        @returns 
        private BigDecimal ComputeSmoothedTrueRange(int period) {
            BigDecimal value;

            if( Samples < period) {
                value = SmoothedTrueRange.Current + TrueRange.Current;
            }
            else
            {
                value = SmoothedTrueRange.Current - (SmoothedTrueRange.Current / period) + TrueRange.Current;
            }
            return value;
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= _period; }
        }

        /**
         * Computes the True Range value.
        */
         * @param input The input.
        @returns 
        private BigDecimal ComputeTrueRange(TradeBar input) {
            trueRange = new decimal(0.0);

            if( _previousInput == null ) return trueRange;

            trueRange = (Math.Max(Math.Abs(input.Low - _previousInput.Close), Math.Max(TrueRange.Current, Math.Abs(input.High - _previousInput.Close))));

            return trueRange;
        }

        /**
         * Computes the positive directional movement.
        */
         * @param input The input.
        @returns 
        private BigDecimal ComputePositiveDirectionalMovement(TradeBar input) {
            postiveDirectionalMovement = new decimal(0.0);

            if( _previousInput == null ) return postiveDirectionalMovement;

            if( (input.High - _previousInput.High) >= (_previousInput.Low - input.Low)) {
                if( (input.High - _previousInput.High) > 0) {
                    postiveDirectionalMovement = input.High - _previousInput.High;
                }
            }

            return postiveDirectionalMovement;
        }

        /**
         * Computes the negative directional movement.
        */
         * @param input The input.
        @returns 
        private BigDecimal ComputeNegativeDirectionalMovement(TradeBar input) {
            negativeDirectionalMovement = new decimal(0.0);

            if( _previousInput == null ) return negativeDirectionalMovement;

            if( (_previousInput.Low - input.Low) > (input.High - _previousInput.High)) {
                if( (_previousInput.Low - input.Low) > 0) {
                    negativeDirectionalMovement = _previousInput.Low - input.Low;
                }
            }

            return negativeDirectionalMovement;
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            TrueRange.Update(input);
            DirectionalMovementPlus.Update(input);
            DirectionalMovementMinus.Update(input);
            SmoothedTrueRange.Update(Current);
            SmoothedDirectionalMovementMinus.Update(Current);
            SmoothedDirectionalMovementPlus.Update(Current);
            if( _previousInput != null ) {
                PositiveDirectionalIndex.Update(Current);
                NegativeDirectionalIndex.Update(Current);
            }
            diff = Math.Abs(PositiveDirectionalIndex - NegativeDirectionalIndex);
            sum = PositiveDirectionalIndex + NegativeDirectionalIndex;
            value = sum == 0 ? 50 : ((_period - 1) * Current.Value + 100 * diff / sum ) / _period;
            _previousInput = input;
            return value;
        }

        /**
         * Computes the Plus Directional Indicator (+DI period).
        */
        @returns 
        private BigDecimal ComputePositiveDirectionalIndex() {
            if( SmoothedTrueRange == 0) return new decimal(0.0);

            positiveDirectionalIndex = (SmoothedDirectionalMovementPlus.Current.Value / SmoothedTrueRange.Current.Value) * 100;

            return positiveDirectionalIndex;
        }

        /**
         * Computes the Minus Directional Indicator (-DI period).
        */
        @returns 
        private BigDecimal ComputeNegativeDirectionalIndex() {
            if( SmoothedTrueRange == 0) return new decimal(0.0);

            negativeDirectionalIndex = (SmoothedDirectionalMovementMinus.Current.Value / SmoothedTrueRange.Current.Value) * 100;

            return negativeDirectionalIndex;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            base.Reset();
            _previousInput = null;
            TrueRange.Reset();
            DirectionalMovementPlus.Reset();
            DirectionalMovementMinus.Reset();
            SmoothedTrueRange.Reset();
            SmoothedDirectionalMovementMinus.Reset();
            SmoothedDirectionalMovementPlus.Reset();
            PositiveDirectionalIndex.Reset();
            NegativeDirectionalIndex.Reset();
        }
    }
}
