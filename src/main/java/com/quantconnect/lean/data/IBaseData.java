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

package com.quantconnect.lean.data;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;

import com.quantconnect.lean.MarketDataType;
import com.quantconnect.lean.Symbol;

/// Base Data Class: Type, Timestamp, Key -- Base Features.
public interface IBaseData extends Cloneable {

    MarketDataType getDataType();

    void setDataType( MarketDataType dataType );

    LocalDateTime getTime();

    void setTime( LocalDateTime time );

    Symbol getSymbol();

    void setSymbol( Symbol symbol );

    BigDecimal getValue();

    void setValue( BigDecimal value );

    BigDecimal getPrice();

    /** Reader Method :: using set of arguements we specify read out type. Enumerate
     * until the end of the data stream or file. E.g. Read CSV file line by line and convert
     * into data types.
     * @returns BaseData type set by Subscription Method.
     */
    BaseData reader( SubscriptionDataConfig config, String line, LocalDate date, boolean isLiveMode );

    /**
     * Return the URL String source of the file. This will be converted to a stream 
     * @param config Configuration object
     * @param date Date of this source file
     * @param isLiveMode Type of datafeed we're reqesting - backtest or live
     * @return
     */
    /// <returns>String URL of source file.</returns>
    SubscriptionDataSource getSource( SubscriptionDataConfig config, LocalDate date, boolean isLiveMode );

    /**
     * Return a new instance clone of this object
     * @return
     */
    BaseData clone();
}
