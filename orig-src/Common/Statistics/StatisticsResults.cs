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

using System.Collections.Generic;

package com.quantconnect.lean.Statistics
{
    /**
     * The <see cref="StatisticsResults"/> class represents total and rolling statistics for an algorithm
    */
    public class StatisticsResults
    {
        /**
         * The performance of the algorithm over the whole period
        */
        public AlgorithmPerformance TotalPerformance { get; private set; }

        /**
         * The rolling performance of the algorithm over 1, 3, 6, 12 month periods
        */
        public Map<String, AlgorithmPerformance> RollingPerformances { get; private set; }

        /**
         * Returns a summary of the algorithm performance as a dictionary
        */
        public Map<String,String> Summary { get; private set; }

        /**
         * Initializes a new instance of the <see cref="StatisticsResults"/> class
        */
         * @param totalPerformance The algorithm total performance
         * @param rollingPerformances The algorithm rolling performances
         * @param summary The summary performance Map
        public StatisticsResults(AlgorithmPerformance totalPerformance, Map<String, AlgorithmPerformance> rollingPerformances, Map<String,String> summary) {
            TotalPerformance = totalPerformance;
            RollingPerformances = rollingPerformances;
            Summary = summary;
        }

        /**
         * Initializes a new instance of the <see cref="StatisticsResults"/> class
        */
        public StatisticsResults() {
            TotalPerformance = new AlgorithmPerformance();
            RollingPerformances = new Map<String, AlgorithmPerformance>();
            Summary = new Map<String,String>();
        }
    }
}
