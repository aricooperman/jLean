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
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

package com.quantconnect.lean.Indicators
{
    /**
     * The Least Squares Moving Average (LSMA) first calculates a least squares regression line
     * over the preceding time periods, and then projects it forward to the current period. In
     * essence, it calculates what the value would be if the regression line continued.
     * Source: https://rtmath.net/helpFinAnalysis/html/b3fab79c-f4b2-40fb-8709-fdba43cdb363.htm
    */
    public class LeastSquaresMovingAverage : WindowIndicator<IndicatorDataPoint>
    {
        /**
         * Array representing the time.
        */
        private final double[] t;

        /**
         * Initializes a new instance of the <see cref="LeastSquaresMovingAverage"/> class.
        */
         * @param name The name of this indicator
         * @param period The number of data points to hold in the window
        public LeastSquaresMovingAverage( String name, int period)
            : base(name, period) {
            t = Vector<double>.Build.Dense(period, i -> i + 1).ToArray();
        }

        /**
         * Initializes a new instance of the <see cref="LeastSquaresMovingAverage"/> class.
        */
         * @param period The number of data points to hold in the window.
        public LeastSquaresMovingAverage(int period)
            : this( "LSMA" + period, period) {
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param window">
         * @param input The input given to the indicator
        @returns 
         * A new value for this indicator
         * 
        protected @Override BigDecimal ComputeNextValue(IReadOnlyWindow<IndicatorDataPoint> window, IndicatorDataPoint input) {
            // Until the windows is ready, the indicator returns the input value.
            BigDecimal output = input;
            if( IsReady) {
                // Sort the windows by time, convert the observations ton double and transform it to a double array
                double[] series = window
                    .OrderBy(i -> i.Time)
                    .Select(i -> Convert.ToDouble(i.Value))
                    .ToArray<double>();
                // Fit OLS
                Tuple<double, double> ols = Fit.Line(x: t, y: series);
                alfa = (decimal)ols.Item1;
                beta = (decimal)ols.Item2;
                // Make the projection.
                output = alfa + beta * (Period);
            }
            return output;
        }
    }
}