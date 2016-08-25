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
     * Represents a type capable of initializing a new security
    */
    public interface ISecurityInitializer
    {
        /**
         * Initializes the specified security
        */
         * @param security The security to be initialized
        void Initialize(Security security);
    }

    /**
     * Provides static access to the <see cref="Null"/> security initializer
    */
    public static class SecurityInitializer
    {
        /**
         * Gets an implementation of <see cref="ISecurityInitializer"/> that is a no-op
        */
        public static final ISecurityInitializer Null = new NullSecurityInitializer();

        private sealed class NullSecurityInitializer : ISecurityInitializer
        {
            public void Initialize(Security security) { }
        }
    }
}