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

package com.quantconnect.lean.securities;

import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.securities.interfaces.ISecurityDataFilter;

/**
 * Base class implementation for packet by packet data filtering mechanism to dynamically detect bad ticks.
 */
public class SecurityDataFilter implements ISecurityDataFilter {
    
    /**
     * Filter the data packet passing through this method by returning true to accept, or false to fail/reject the data point.
     * @param data BasData data object we're filtering
     * @param vehicle Security vehicle for filter
     */
    public boolean filter( Security vehicle, BaseData data ) {
        //By default the filter does not change data.
        return true;
    }
} //End Filter
