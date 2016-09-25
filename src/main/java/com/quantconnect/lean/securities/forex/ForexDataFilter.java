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

package com.quantconnect.lean.securities.forex;

import com.quantconnect.lean.data.BaseData;
import com.quantconnect.lean.securities.Security;
import com.quantconnect.lean.securities.SecurityDataFilter;

/**
 * Forex packet by packet data filtering mechanism for dynamically detecting bad ticks.
 * <seealso cref="SecurityDataFilter"/>
 */
public class ForexDataFilter extends SecurityDataFilter {
        
    /**
     * Initialize forex data filter class:
     */
    public ForexDataFilter() {
        super();
    }

    /**
     * Forex data filter: a true value means accept the packet, a false means fail.
     * @param data Data object we're scanning to filter
     * @param vehicle Security asset
     */
    @Override
    public boolean filter( Security vehicle, BaseData data ) {
        //FX data is from FXCM and fairly clean already. Accept all packets.
        return true;
    }
} //End Filter
