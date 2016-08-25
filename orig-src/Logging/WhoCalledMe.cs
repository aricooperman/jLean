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

using System.Diagnostics;
using System.Runtime.CompilerServices;

package com.quantconnect.lean.Logging
{
    /**
     * Provides methods for determining higher stack frames
    */
    public static class WhoCalledMe
    {
        /**
         * Gets the method name of the caller
        */
         * @param frame The number of stack frames to retrace from the caller's position
        @returns The method name of the containing scope 'frame' stack frames above the caller
        [MethodImpl(MethodImplOptions.NoInlining)] // inlining messes this up pretty badly
        public static String GetMethodName(int frame = 1) {
            // we need to increment the frame to account for this method
            methodBase = new StackFrame(frame + 1).GetMethod();
            declaringType = methodBase.DeclaringType;
            return declaringType != null ? declaringType.Name + "." + methodBase.Name : methodBase.Name;
        }
    }
}
