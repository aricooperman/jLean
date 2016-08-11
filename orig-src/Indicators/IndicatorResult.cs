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
    /// <summary>
    /// Represents the result of an indicator's calculations
    /// </summary>
    public class IndicatorResult
    {
        /// <summary>
        /// The indicator output value
        /// </summary>
        public BigDecimal Value
        {
            get;
            private set;
        }

        /// <summary>
        /// The indicator status
        /// </summary>
        public IndicatorStatus Status
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndicatorResult"/> class
        /// </summary>
        /// <param name="value">The value output by the indicator</param>
        /// <param name="status">The status returned by the indicator</param>
        public IndicatorResult( BigDecimal value, IndicatorStatus status = IndicatorStatus.Success)
        {
            Value = value;
            Status = status;
        }

        /// <summary>
        /// Converts the specified BigDecimal value into a successful indicator result
        /// </summary>
        /// <remarks>
        /// This method is provided for backwards compatibility
        /// </remarks>
        /// <param name="value">The BigDecimal value to be converted into an <see cref="IndicatorResult"/></param>
        public static implicit operator IndicatorResult( BigDecimal value)
        {
            return new IndicatorResult(value);
        }
    }
}
