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
    /// Parabolic SAR Indicator 
    /// Based on TA-Lib implementation
    */
    public class ParabolicStopAndReverse : TradeBarIndicator
    {
        private boolean _isLong;
        private TradeBar _previousBar;
        private BigDecimal _sar;
        private BigDecimal _ep;
        private BigDecimal _outputSar;
        private BigDecimal _af;
        private final BigDecimal _afInit;
        private final BigDecimal _afMax;
        private final BigDecimal _afIncrement;

        /**
        /// Create new Parabolic SAR
        */
         * @param name">The name of this indicator
         * @param afStart">Acceleration factor start value
         * @param afIncrement">Acceleration factor increment value
         * @param afMax">Acceleration factor max value
        public ParabolicStopAndReverse( String name, BigDecimal afStart = 0.02m, BigDecimal afIncrement = 0.02m, BigDecimal afMax = 0.2m)
            : base(name) {
            _afInit = afStart;
            _af = afStart;
            _afIncrement = afIncrement;
            _afMax = afMax;
        }

        /**
        /// Create new Parabolic SAR
        */
         * @param afStart">Acceleration factor start value
         * @param afIncrement">Acceleration factor increment value
         * @param afMax">Acceleration factor max value
        public ParabolicStopAndReverse( BigDecimal afStart = 0.02m, BigDecimal afIncrement = 0.02m, BigDecimal afMax = 0.2m)
            : this( String.format( "PSAR(%1$s,%2$s,%3$s)", afStart, afIncrement, afMax), afStart, afIncrement, afMax) {
        }

        /**
        /// Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return Samples >= 2; }
        }

        /**
        /// Resets this indicator to its initial state
        */
        public @Override void Reset() {
            _af = _afInit;
            base.Reset();
        }

        /**
        /// Computes the next value of this indicator from the given state
        */
         * @param input">The trade bar input given to the indicator
        @returns A new value for this indicator
        protected @Override BigDecimal ComputeNextValue(TradeBar input) {
            // On first iteration we can’t produce an SAR value so we save the current bar and return zero
            if( Samples == 1) {
                _previousBar = input;

                // return a value that's close to where we will be, returning 0 doesn't make sense
                return input.Close;
            }

            // On second iteration we initiate the position the extreme point and the SAR
            if( Samples == 2) {
                Init(input);
                _previousBar = input;
                return _sar;
            }

            if( _isLong) {
                HandleLongPosition(input);
            }
            else
            {
                HandleShortPosition(input);
            }

            _previousBar = input;

            return _outputSar;
        }

        /**
        /// Initialize the indicator values 
        */
        private void Init(TradeBar currentBar) {
            // init position
            _isLong = currentBar.Close >= _previousBar.Close;


            // init sar and Extreme price
            if( _isLong) {
                _ep = Math.Min(currentBar.High, _previousBar.High);
                _sar = _previousBar.Low;
            }
            else
            {
                _ep = Math.Min(currentBar.Low, _previousBar.Low);
                _sar = _previousBar.High;
            }
        }

        /**
        /// Calculate indicator value when the position is long
        */
        private void HandleLongPosition(TradeBar currentBar) {
            // Switch to short if the low penetrates the SAR value.
            if( currentBar.Low <= _sar) {
                // Switch and Overide the SAR with the ep
                _isLong = false;
                _sar = _ep;

                // Make sure the overide SAR is within yesterday's and today's range.
                if( _sar < _previousBar.High)
                    _sar = _previousBar.High;
                if( _sar < currentBar.High)
                    _sar = currentBar.High;

                // Output the overide SAR 
                _outputSar = _sar;

                // Adjust af and ep
                _af = _afInit;
                _ep = currentBar.Low;

                // Calculate the new SAR
                _sar = _sar + _af * (_ep - _sar);

                // Make sure the new SAR is within yesterday's and today's range.
                if( _sar < _previousBar.High)
                    _sar = _previousBar.High;
                if( _sar < currentBar.High)
                    _sar = currentBar.High;

            }

            // No switch
            else
            {
                // Output the SAR (was calculated in the previous iteration) 
                _outputSar = _sar;

                // Adjust af and ep.
                if( currentBar.High > _ep) {
                    _ep = currentBar.High;
                    _af += _afIncrement;
                    if( _af > _afMax)
                        _af = _afMax;
                }

                // Calculate the new SAR
                _sar = _sar + _af * (_ep - _sar);

                // Make sure the new SAR is within yesterday's and today's range.
                if( _sar > _previousBar.Low)
                    _sar = _previousBar.Low;
                if( _sar > currentBar.Low)
                    _sar = currentBar.Low;
            }
        }

        /**
        /// Calculate indicator value when the position is short
        */
        private void HandleShortPosition(TradeBar currentBar) {
            // Switch to long if the high penetrates the SAR value.
            if( currentBar.High >= _sar) {
                // Switch and Overide the SAR with the ep
                _isLong = true;
                _sar = _ep;

                // Make sure the overide SAR is within yesterday's and today's range.
                if( _sar > _previousBar.Low)
                    _sar = _previousBar.Low;
                if( _sar > currentBar.Low)
                    _sar = currentBar.Low;

                // Output the overide SAR 
                _outputSar = _sar;

                // Adjust af and ep
                _af = _afInit;
                _ep = currentBar.High;

                // Calculate the new SAR
                _sar = _sar + _af * (_ep - _sar);

                // Make sure the new SAR is within yesterday's and today's range.
                if( _sar > _previousBar.Low)
                    _sar = _previousBar.Low;
                if( _sar > currentBar.Low)
                    _sar = currentBar.Low;
            }

            //No switch
            else
            {
                // Output the SAR (was calculated in the previous iteration)
                _outputSar = _sar;

                // Adjust af and ep.
                if( currentBar.Low < _ep) {
                    _ep = currentBar.Low;
                    _af += _afIncrement;
                    if( _af > _afMax)
                        _af = _afMax;
                }

                // Calculate the new SAR
                _sar = _sar + _af * (_ep - _sar);

                // Make sure the new SAR is within yesterday's and today's range.
                if( _sar < _previousBar.High)
                    _sar = _previousBar.High;
                if( _sar < currentBar.High)
                    _sar = currentBar.High;
            }
        }
    }
}