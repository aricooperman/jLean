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

package com.quantconnect.lean.Benchmarks
{
    /**
     * Creates a benchmark defined by a function
    */
    public class FuncBenchmark : IBenchmark
    {
        private final Func<DateTime, decimal> _benchmark;

        /**
         * Initializes a new instance of the <see cref="FuncBenchmark"/> class
        */
         * @param benchmark The functional benchmark implementation
        public FuncBenchmark(Func<DateTime, decimal> benchmark) {
            if( benchmark == null ) {
                throw new NullPointerException( "benchmark");
            }
            _benchmark = benchmark;
        }

        /**
         * Evaluates this benchmark at the specified time
        */
         * @param time The time to evaluate the benchmark at
        @returns The value of the benchmark at the specified time
        public BigDecimal Evaluate(DateTime time) {
            return _benchmark(time);
        }
    }
}