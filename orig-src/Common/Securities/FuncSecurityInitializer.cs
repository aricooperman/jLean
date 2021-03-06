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

using System;

package com.quantconnect.lean.Securities
{
    /**
     * Provides a functional implementation of <see cref="ISecurityInitializer"/>
    */
    public class FuncSecurityInitializer : ISecurityInitializer
    {
        private final Action<Security> _initializer;

        /**
         * Initializes a new instance of the <see cref="FuncSecurityInitializer"/> class
        */
         * @param initializer The functional implementation of <see cref="ISecurityInitializer.Initialize"/>
        public FuncSecurityInitializer(Action<Security> initializer) {
            _initializer = initializer;
        }

        /**
         * Initializes the specified security
        */
         * @param security The security to be initialized
        public void Initialize(Security security) {
            _initializer(security);
        }
    }
}
