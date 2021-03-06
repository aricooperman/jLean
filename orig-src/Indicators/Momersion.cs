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

using System;
using System.Linq;

package com.quantconnect.lean.Indicators
{
    /** 
     * Oscillator indicator that measures momentum and mean-reversion over a specified
     * period n.
     * Source: Harris, Michael. "Momersion Indicator." Price Action Lab.,
     *             13 Aug. 2015. Web. http://www.priceactionlab.com/Blog/2015/08/momersion-indicator/.
    */
    public class MomersionIndicator : WindowIndicator<IndicatorDataPoint>
    {
        /**
         * The minimum observations needed to consider the indicator ready. After that observation
         * number is reached, the indicator will continue gathering data until the full period.
        */
        private OptionalInt _minPeriod;

        /**
         * The final full period used to estimate the indicator.
        */
        private int _fullPeriod;

        /**
         * The rolling window used to store the momentum.
        */
        private RollingWindow<decimal> _multipliedDiffWindow;

        /**
         * Initializes a new instance of the <see cref="MomersionIndicator"/> class.
        */
         * @param name The name.
         * @param minPeriod The minimum period.
         * @param fullPeriod The full period.
         * <exception cref="System.ArgumentException The minimum period should be greater of 3.;minPeriod</exception>
        public MomersionIndicator( String name, OptionalInt minPeriod, int fullPeriod)
            : base(name, 3) {
            _fullPeriod = fullPeriod;
            _multipliedDiffWindow = new RollingWindow<decimal>(fullPeriod);
            if( minPeriod < 4) {
                throw new IllegalArgumentException( "The minimum period should be greater of 3.", "minPeriod");
            }
            _minPeriod = minPeriod;
        }

        /**
         * Initializes a new instance of the <see cref="MomersionIndicator"/> class.
        */
         * @param minPeriod The minimum period.
         * @param fullPeriod The full period.
        public MomersionIndicator(int minPeriod, int fullPeriod)
            : this( "Momersion_" + fullPeriod, minPeriod, fullPeriod) {
        }

        /**
         * Initializes a new instance of the <see cref="MomersionIndicator"/> class.
        */
         * @param fullPeriod The full period.
        public MomersionIndicator(int fullPeriod)
            : this( "Momersion_" + fullPeriod, null, fullPeriod) {
        }
        

        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get
            {
                if( _minPeriod.HasValue) {
                    return _multipliedDiffWindow.Count >= _minPeriod;
                }
                else
                {
                    return _multipliedDiffWindow.IsReady;
                }
            }
        }

        /**
         * Resets this indicator to its initial state
        */
        public @Override void Reset() {
            base.Reset();
            _multipliedDiffWindow.Reset();
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
            int Mc = 0;
            int MRc = 0;
            BigDecimal momersion = 50m;

            if( window.Count >= 3) _multipliedDiffWindow.Add((window[0] - window[1]) * (window[1] - window[2]));

            // Estimate the indicator if less than 50% of observation are zero. Avoid division by
            // zero and estimations with few real observations in case of forward filled data.
            if( this.IsReady &&
                _multipliedDiffWindow.Count(obs -> obs == 0) < 0.5 * _multipliedDiffWindow.Count) {
                Mc = _multipliedDiffWindow.Count(obs -> obs > 0);
                MRc = _multipliedDiffWindow.Count(obs -> obs < 0);
                momersion = 100m * Mc / (Mc + MRc);
            }
            return momersion;
        }
    }
}