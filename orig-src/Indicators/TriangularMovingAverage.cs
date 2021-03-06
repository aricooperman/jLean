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

package com.quantconnect.lean.Indicators
{
    /**
     * This indicator computes the Triangular Moving Average (TRIMA). 
     * The Triangular Moving Average is calculated with the following formula:
     * (1) When the period is even, TRIMA(x,period)=SMA(SMA(x,period/2),(period/2)+1)
     * (2) When the period is odd,  TRIMA(x,period)=SMA(SMA(x,(period+1)/2),(period+1)/2)
    */
    public class TriangularMovingAverage : IndicatorBase<IndicatorDataPoint>
    {
        private final int _period;
        private final SimpleMovingAverage _sma1;
        private final SimpleMovingAverage _sma2;

        /**
         * Initializes a new instance of the <see cref="TriangularMovingAverage"/> class using the specified name and period.
        */ 
         * @param name The name of this indicator
         * @param period The period of the indicator
        public TriangularMovingAverage( String name, int period)
            : base(name) {
            _period = period;

            periodSma1 = period % 2 == 0 ? period / 2 : (period + 1) / 2;
            periodSma2 = period % 2 == 0 ? period / 2 + 1 : (period + 1) / 2;

            _sma1 = new SimpleMovingAverage(name + "_1", periodSma1);
            _sma2 = new SimpleMovingAverage(name + "_2", periodSma2);
        }

        /**
         * Initializes a new instance of the <see cref="TriangularMovingAverage"/> class using the specified period.
        */ 
         * @param period The period of the indicator
        public TriangularMovingAverage(int period)
            : this( "TRIMA" + period, period) {
        }

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= _period; }
        }

        /**
         * Computes the next value of this indicator from the given state
        */
         * @param input The input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(IndicatorDataPoint input) {
            _sma1.Update(input);
            _sma2.Update(_sma1.Current);

            return _sma2;
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _sma1.Reset();
            _sma2.Reset();
            base.Reset();
        }
    }
}
