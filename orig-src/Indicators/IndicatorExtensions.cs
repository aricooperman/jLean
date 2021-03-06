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
using System.Globalization;
using QuantConnect.Data;
using System.Collections.Generic;
using System.Linq;

package com.quantconnect.lean.Indicators
{
    /**
     * Provides extension methods for Indicator
    */
    public static class IndicatorExtensions
    {
        /**
         * Updates the state of this indicator with the given value and returns true
         * if this indicator is ready, false otherwise
        */
         * @param indicator The indicator to be updated
         * @param time The time associated with the value
         * @param value The value to use to update this indicator
        @returns True if this indicator is ready, false otherwise
        public static boolean Update(this IndicatorBase<IndicatorDataPoint> indicator, DateTime time, BigDecimal value) {
            return indicator.Update(new IndicatorDataPoint(time, value));
        }

        /**
         * Configures the second indicator to receive automatic updates from the first by attaching an event handler
         * to first.DataConsolidated
        */
         * @param second The indicator that receives data from the first
         * @param first The indicator that sends data via DataConsolidated even to the second
         * @param waitForFirstToReady True to only send updates to the second if first.IsReady returns true, false to alway send updates to second
        @returns The reference to the second indicator to allow for method chaining
        public static TSecond Of<T, TSecond>(this TSecond second, IndicatorBase<T> first, boolean waitForFirstToReady = true)
            where T : BaseData
            where TSecond : IndicatorBase<IndicatorDataPoint>
        {
            first.Updated += (sender, consolidated) =>
            {
                // only send the data along if we're ready
                if( !waitForFirstToReady || first.IsReady) {
                    second.Update(consolidated);
                }
            };

            return second;
        }

        /**
         * Creates a new CompositeIndicator such that the result will be average of a first indicator weighted by a second one
        */
         * @param value Indicator that will be averaged
         * @param weight Indicator that provides the average weights
         * @param period Average period
        @returns Indicator that results of the average of first by weights given by second
        public static CompositeIndicator<IndicatorDataPoint> WeightedBy<T, TWeight>(this IndicatorBase<T> value, TWeight weight, int period)
            where T : BaseData
            where TWeight : IndicatorBase<IndicatorDataPoint>
        {
            x = new WindowIdentity(period);
            y = new WindowIdentity(period);
            numerator = new Sum( "Sum_xy", period);
            denominator = new Sum( "Sum_y", period);

            value.Updated += (sender, consolidated) =>
            {
                x.Update(consolidated);
                if( x.Samples == y.Samples) {
                    numerator.Update(consolidated.Time, consolidated.Value * y.Current.Value);
                }  
            };

            weight.Updated += (sender, consolidated) =>
            {
                y.Update(consolidated);
                if( x.Samples == y.Samples) {
                    numerator.Update(consolidated.Time, consolidated.Value * x.Current.Value);
                }
                denominator.Update(consolidated);
            };
            
            return numerator.Over(denominator);
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the sum of the left and the constant
        */
         * 
         * value = left + constant
         * 
         * @param left The left indicator
         * @param constant The addend
        @returns The sum of the left and right indicators
        public static CompositeIndicator<IndicatorDataPoint> Plus(this IndicatorBase<IndicatorDataPoint> left, BigDecimal constant) {
            constantIndicator = new ConstantIndicator<IndicatorDataPoint>(constant.toString(CultureInfo.InvariantCulture), constant);
            return left.Plus(constantIndicator);
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the sum of the left and right
        */
         * 
         * value = left + right
         * 
         * @param left The left indicator
         * @param right The right indicator
        @returns The sum of the left and right indicators
        public static CompositeIndicator<IndicatorDataPoint> Plus(this IndicatorBase<IndicatorDataPoint> left, IndicatorBase<IndicatorDataPoint> right) {
            return new CompositeIndicator<IndicatorDataPoint>(left, right, (l, r) -> l + r);
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the sum of the left and right
        */
         * 
         * value = left + right
         * 
         * @param left The left indicator
         * @param right The right indicator
         * @param name The name of this indicator
        @returns The sum of the left and right indicators
        public static CompositeIndicator<IndicatorDataPoint> Plus(this IndicatorBase<IndicatorDataPoint> left, IndicatorBase<IndicatorDataPoint> right, String name) {
            return new CompositeIndicator<IndicatorDataPoint>(name, left, right, (l, r) -> l + r);
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the difference of the left and constant
        */
         * 
         * value = left - constant
         * 
         * @param left The left indicator
         * @param constant The subtrahend
        @returns The difference of the left and right indicators
        public static CompositeIndicator<IndicatorDataPoint> Minus(this IndicatorBase<IndicatorDataPoint> left, BigDecimal constant) {
            constantIndicator = new ConstantIndicator<IndicatorDataPoint>(constant.toString(CultureInfo.InvariantCulture), constant);
            return left.Minus(constantIndicator);
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the difference of the left and right
        */
         * 
         * value = left - right
         * 
         * @param left The left indicator
         * @param right The right indicator
        @returns The difference of the left and right indicators
        public static CompositeIndicator<IndicatorDataPoint> Minus(this IndicatorBase<IndicatorDataPoint> left, IndicatorBase<IndicatorDataPoint> right) {
            return new CompositeIndicator<IndicatorDataPoint>(left, right, (l, r) -> l - r);
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the difference of the left and right
        */
         * 
         * value = left - right
         * 
         * @param left The left indicator
         * @param right The right indicator
         * @param name The name of this indicator
        @returns The difference of the left and right indicators
        public static CompositeIndicator<IndicatorDataPoint> Minus(this IndicatorBase<IndicatorDataPoint> left, IndicatorBase<IndicatorDataPoint> right, String name) {
            return new CompositeIndicator<IndicatorDataPoint>(name, left, right, (l, r) -> l - r);
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the ratio of the left to the constant
        */
         * 
         * value = left/constant
         * 
         * @param left The left indicator
         * @param constant The constant value denominator
        @returns The ratio of the left to the right indicator
        public static CompositeIndicator<IndicatorDataPoint> Over(this IndicatorBase<IndicatorDataPoint> left, BigDecimal constant) {
            constantIndicator = new ConstantIndicator<IndicatorDataPoint>(constant.toString(CultureInfo.InvariantCulture), constant);
            return left.Over(constantIndicator);
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the ratio of the left to the right
        */
         * 
         * value = left/right
         * 
         * @param left The left indicator
         * @param right The right indicator
        @returns The ratio of the left to the right indicator
        public static CompositeIndicator<IndicatorDataPoint> Over(this IndicatorBase<IndicatorDataPoint> left, IndicatorBase<IndicatorDataPoint> right) {
            return new CompositeIndicator<IndicatorDataPoint>(left, right, (l, r) -> r == BigDecimal.ZERO ? new IndicatorResult(0m, IndicatorStatus.MathError) : new IndicatorResult(l / r));
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the ratio of the left to the right
        */
         * 
         * value = left/right
         * 
         * @param left The left indicator
         * @param right The right indicator
         * @param name The name of this indicator
        @returns The ratio of the left to the right indicator
        public static CompositeIndicator<IndicatorDataPoint> Over(this IndicatorBase<IndicatorDataPoint> left, IndicatorBase<IndicatorDataPoint> right, String name) {
            return new CompositeIndicator<IndicatorDataPoint>(name, left, right, (l, r) -> r == BigDecimal.ZERO ? new IndicatorResult(0m, IndicatorStatus.MathError) : new IndicatorResult(l / r));
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the product of the left and the constant
        */
         * 
         * value = left*constant
         * 
         * @param left The left indicator
         * @param constant The constant value to multiple by
        @returns The product of the left to the right indicators
        public static CompositeIndicator<IndicatorDataPoint> Times(this IndicatorBase<IndicatorDataPoint> left, BigDecimal constant) {
            constantIndicator = new ConstantIndicator<IndicatorDataPoint>(constant.toString(CultureInfo.InvariantCulture), constant);
            return left.Times(constantIndicator);
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the product of the left to the right
        */
         * 
         * value = left*right
         * 
         * @param left The left indicator
         * @param right The right indicator
        @returns The product of the left to the right indicators
        public static CompositeIndicator<IndicatorDataPoint> Times(this IndicatorBase<IndicatorDataPoint> left, IndicatorBase<IndicatorDataPoint> right) {
            return new CompositeIndicator<IndicatorDataPoint>(left, right, (l, r) -> l * r);
        }

        /**
         * Creates a new CompositeIndicator such that the result will be the product of the left to the right
        */
         * 
         * value = left*right
         * 
         * @param left The left indicator
         * @param right The right indicator
         * @param name The name of this indicator
        @returns The product of the left to the right indicators
        public static CompositeIndicator<IndicatorDataPoint> Times(this IndicatorBase<IndicatorDataPoint> left, IndicatorBase<IndicatorDataPoint> right, String name) {
            return new CompositeIndicator<IndicatorDataPoint>(name, left, right, (l, r) -> l * r);
        }

        /**Creates a new ExponentialMovingAverage indicator with the specified period and smoothingFactor from the left indicator
        */
         * @param left The ExponentialMovingAverage indicator will be created using the data from left
         * @param period The period of the ExponentialMovingAverage indicators
         * @param smoothingFactor The percentage of data from the previous value to be carried into the next value
         * @param waitForFirstToReady True to only send updates to the second if left.IsReady returns true, false to alway send updates
        @returns A reference to the ExponentialMovingAverage indicator to allow for method chaining
        public static ExponentialMovingAverage EMA<T>(this IndicatorBase<T> left, int period, Optional<BigDecimal> smoothingFactor = null, boolean waitForFirstToReady = true)
            where T : BaseData
        {
            BigDecimal k = smoothingFactor.HasValue ? k = smoothingFactor.Value : ExponentialMovingAverage.SmoothingFactorDefault(period);
            ExponentialMovingAverage emaOfLeft = new ExponentialMovingAverage( String.format( "EMA%1$s_Of_%2$s", period, left.Name), period, k).Of(left, waitForFirstToReady);
            return emaOfLeft;
        }

        /**Creates a new Maximum indicator with the specified period from the left indicator
        */
         * @param left The Maximum indicator will be created using the data from left
         * @param period The period of the Maximum indicator
         * @param waitForFirstToReady True to only send updates to the second if left.IsReady returns true, false to alway send updates
        @returns A reference to the Maximum indicator to allow for method chaining
        public static Maximum MAX<T>(this IndicatorBase<T> left, int period, boolean waitForFirstToReady = true)
            where T : BaseData
        {
            Maximum maxOfLeft = new Maximum( String.format( "MAX%1$s_Of_%2$s", period, left.Name), period).Of(left, waitForFirstToReady);
            return maxOfLeft;
        }

        /**Creates a new Minimum indicator with the specified period from the left indicator
        */
         * @param left The Minimum indicator will be created using the data from left
         * @param period The period of the Minimum indicator
         * @param waitForFirstToReady True to only send updates to the second if left.IsReady returns true, false to alway send updates
        @returns A reference to the Minimum indicator to allow for method chaining
        public static Minimum MIN<T>(this IndicatorBase<T> left, int period, boolean waitForFirstToReady = true)
            where T : BaseData
        {
            Minimum minOfLeft = new Minimum( String.format( "MIN%1$s_Of_%2$s", period, left.Name), period).Of(left, waitForFirstToReady);
            return minOfLeft;
        }

        /**Initializes a new instance of the SimpleMovingAverage class with the specified name and period from the left indicator
        */
         * @param left The SimpleMovingAverage indicator will be created using the data from left
         * @param period The period of the SMA
         * @param waitForFirstToReady True to only send updates to the second if first.IsReady returns true, false to alway send updates to second
        @returns The reference to the SimpleMovingAverage indicator to allow for method chaining
        public static SimpleMovingAverage SMA<T>(this IndicatorBase<T> left, int period, boolean waitForFirstToReady = true)
            where T : BaseData
        {
            SimpleMovingAverage smaOfLeft = new SimpleMovingAverage( String.format( "SMA%1$s_Of_%2$s", period, left.Name), period).Of(left, waitForFirstToReady);
            return smaOfLeft;
        }
    }
}
