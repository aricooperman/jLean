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
 *
*/

package com.quantconnect.lean.lean.engine.datafeeds;

import java.util.List;

import com.google.common.collect.ImmutableList;
import com.quantconnect.lean.data.BaseData;

/**
 * Transport type for algorithm update data. This is intended to provide a
 * list of base data used to perform updates against the specified target
 * <typeparam name="T The target type</typeparam>
 */
public class UpdateData<T> {
    /**
     * The target, such as a security or subscription data config
     */
    public final T target;

    /**
     * The data used to update the target
     */
    public final ImmutableList<BaseData> data;

    /**
     * The type of data in the data list
     */
    public final Class<? extends T> dataType;

    /**
     * Initializes a new instance of the <see cref="UpdateData{T}"/> class
     * @param target The end consumer/user of the dat
     * @param dataType The type of data in the list
     * @param data The update data
     */
    public UpdateData( final T target, final Class<? extends T> dataType, final List<BaseData> data ) {
        this.target = target;
        this.dataType = dataType;
        this.data = ImmutableList.copyOf( data );
    }
}
