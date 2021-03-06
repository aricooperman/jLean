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

package com.quantconnect.lean.Securities
{
    /**
     * Provides an implementation of <see cref="ISecurityInitializer"/> that executes
     * each initializer in order
    */
    public class CompositeSecurityInitializer : ISecurityInitializer
    {
        private final ISecurityInitializer[] _initializers;

        /**
         * Initializes a new instance of the <see cref="CompositeSecurityInitializer"/> class
        */
         * @param initializers The initializers to execute in order
        public CompositeSecurityInitializer(params ISecurityInitializer[] initializers) {
            _initializers = initializers;
        }

        /**
         * Execute each of the internally held initializers in sequence
        */
         * @param security The security to be initialized
        public void Initialize(Security security) {
            foreach (initializer in _initializers) {
                initializer.Initialize(security);
            }
        }
    }
}