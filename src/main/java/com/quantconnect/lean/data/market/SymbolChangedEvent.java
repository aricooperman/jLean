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

package com.quantconnect.lean.data.market;

import java.time.LocalDateTime;

import com.quantconnect.lean.MarketDataType;
import com.quantconnect.lean.Symbol;
import com.quantconnect.lean.data.BaseData;

/**
 * Symbol changed event of a security. This is generated when a symbol is remapped for a given
 * security, for example, at EOD 2014.04.02 GOOG turned into GOOGL, but are the same
 */
public class SymbolChangedEvent extends BaseData {
     
    private String oldSymbol;
    private String newSymbol;

    /**
     * Gets the symbol before the change
     */
    public String getOldSymbol() {
        return oldSymbol;
    }
    
    /**
     * Gets the symbol after the change
     */
    public String getNewSymbol() {
        return newSymbol;
    }

    /**
     * Initializes a new default instance of the <see cref="SymbolChangedEvent"/> class
    */
    public SymbolChangedEvent() {
        setDataType( MarketDataType.Auxiliary );
    }

    /**
     * Initializes a new instance of the <see cref="SymbolChangedEvent"/>
     * @param requestedSymbol The symbol that was originally requested
     * @param date The date/time this symbol remapping took place
     * @param oldSymbol The old symbol mapping
     * @param newSymbol The new symbol mapping
     */
    public SymbolChangedEvent( Symbol requestedSymbol, LocalDateTime date, String oldSymbol, String newSymbol ) {
        this();
        setTime( date );
        setSymbol( requestedSymbol );
        this.oldSymbol = oldSymbol;
        this.newSymbol = newSymbol;
    }

    /**
     * Return a new instance clone of this object, used in fill forward
     * 
     * This base implementation uses reflection to copy all public fields and properties
     * @returns A clone of the current object
     */
    @Override
    public BaseData clone() {
        return new SymbolChangedEvent( getSymbol(), getTime(), oldSymbol, newSymbol );
    }
}
