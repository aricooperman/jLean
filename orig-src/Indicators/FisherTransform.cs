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
 *
*/
using QuantConnect.Data.Market;
using System;

package com.quantconnect.lean.Indicators
{
    /**
     * The Fisher transform is a mathematical process which is used to convert any data set to a modified
     * data set whose Probabilty Distrbution Function is approximately Gaussian.  Once the Fisher transform
     * is computed, the transformed data can then be analyzed in terms of it's deviation from the mean.
     * 
     * The equation is y = .5 * ln [ 1 + x / 1 - x ] where
     * x is the input
     * y is the output
     * ln is the natural logarithm
     * 
     * The Fisher transform has much sharper turning points than other indicators such as MACD
     * 
     * For more info, read chapter 1 of Cybernetic Analysis for Stocks and Futures by John F. Ehlers
     * 
     * We are implementing the lastest version of this indicator found at Fig. 4 of
     * http://www.mesasoftware.com/papers/UsingTheFisherTransform.pdf
     * 
    */
    public class FisherTransform : TradeBarIndicator
    {
        private double _alpha;
        private double _previous;
        private final Minimum _medianMin;
        private final Maximum _medianMax;

        /**
         *     Initializes a new instance of the FisherTransform class with the default name and period
        */
         * @param period The period of the WMA
        public FisherTransform(int period)
            : this( "FISH_" + period, period) {
        }

        /**
         * A Fisher Transform of Prices
        */
         * @param name string - the name of the indicator
         * @param period The number of periods for the indicator
        public FisherTransform( String name, int period)
            : base(name) {
            _alpha = .33;

            // Initialize the local variables
            _medianMax = new Maximum( "MedianMax", period);
            _medianMin = new Minimum( "MedianMin", period);
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return _medianMax.IsReady && _medianMax.IsReady; }
        }

        /**
         * Computes the next value in the transform. 
         * value1 is a function used to normalize price withing the last _period day range.
         * value1 is centered on its midpoint and then doubled so that value1 wil swing between -1 and +1.  
         * value1 is also smoothed with an exponential moving average whose alpha is 0.33.  
         * 
         * Since the smoothing may allow value1 to exceed the _period day price range, limits are introduced to 
         * preclude the transform from blowing up by having an input larger than unity.
        */
         * @param input IndicatorDataPoint - the time and value of the next price
        @returns 
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            x = 0.0;
            y = 0.0;
            price = (input.Low + input.High) / 2m;
            _medianMin.Update(input.Time, price);
            _medianMax.Update(input.Time, price);

            if( !IsReady) return 0;

            minL = _medianMin.Current.Value;
            maxH = _medianMax.Current.Value;
            
            if( minL != maxH) {
                x = _alpha * 2 * ((double)((price - minL) / (maxH - minL)) - .5) + (1 - _alpha) * _previous;
                y = FisherTransformFunction(x);
            }
            _previous = x;

            return new BigDecimal( y) + .5m * Current.Value;
        }

        /**
         * The Fisher transform is a mathematical process which is used to convert any data set to a modified
         * data set whose Probabilty Distrbution Function is approximately Gaussian.  Once the Fisher transform
         * is computed, the transformed data can then be analyzed in terms of it's deviation from the mean.
         * 
         * The equation is y = .5 * ln [ 1 + x / 1 - x ] where
         * x is the input
         * y is the output
         * ln is the natural logarithm
         * 
         * The Fisher transform has much sharper turning points than other indicators such as MACD
         * 
         * For more info, read chapter 1 of Cybernetic Analysis for Stocks and Futures by John F. Ehlers
        */
         * @param x Input
        @returns Output
        private double FisherTransformFunction(double x) {
            if( x > .99) {
                x = .999;
            }
            if( x < -.99) {
                x = -.999;
            }

            return .5 * Math.Log((1.0 + x) / (1.0 - x));
        }
    }
}
