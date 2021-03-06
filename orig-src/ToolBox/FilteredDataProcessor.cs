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
using QuantConnect.Data;

package com.quantconnect.lean.ToolBox
{
    /**
     * Provides an implementation of <see cref="IDataProcessor"/> that filters the incoming
     * stream of data before passing it along to the wrapped processor
    */
    public class FilteredDataProcessor : IDataProcessor
    {
        private final Func<BaseData, bool> _predicate;
        private final IDataProcessor _processor;

        /**
         * Initializes a new instance of the <see cref="FilteredDataProcessor"/> class
        */
         * @param processor The processor to filter data for
         * @param predicate The filtering predicate to be applied
        public FilteredDataProcessor(IDataProcessor processor, Func<BaseData, bool> predicate) {
            _predicate = predicate;
            _processor = processor;
        }

        /**
         * Invoked for each piece of data from the source file
        */
         * @param data The data to be processed
        public void Process(BaseData data) {
            if( _predicate(data)) {
                _processor.Process(data);
            }
        }

        /**
         * Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        */
        public void Dispose() {
            _processor.Dispose();
        }
    }
}