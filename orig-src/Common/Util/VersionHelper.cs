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
*/

using System.Globalization;
using System.Linq;

package com.quantconnect.lean.Util
{
    /**
     * Provides methods for dealing with lean assembly versions
    */
    public static class VersionHelper
    {
        private static final boolean IgnoreVersionChecks = Configuration.Config.GetBool( "ignore-version-checks");

        /**
         * Determines whether or not the specified version is older than this instance
        */
         * @param version The version to compare
        @returns True if the specified version is older, false otherwise
        public static boolean IsOlderVersion( String version) {
            return CompareVersions(version, Globals.Version) < 0;
        }

        /**
         * Determines whether or not the specified version is newer than this instance
        */
         * @param version The version to compare
        @returns True if the specified version is newer, false otherwise
        public static boolean IsNewerVersion( String version) {
            return CompareVersions(version, Globals.Version) > 0;
        }

        /**
         * Determines whether or not the specified version is equal to this instance
        */
         * @param version The version to compare
        @returns True if the specified version is equal, false otherwise
        public static boolean IsEqualVersion( String version) {
            return CompareVersions(version, Globals.Version) == 0;
        }

        /**
         * Determines whether or not the specified version is not equal to this instance
        */
         * @param version The version to compare
        @returns True if the specified version is not equal, false otherwise
        public static boolean IsNotEqualVersion( String version) {
            return !IsEqualVersion(version);
        }

        /**
         * Compares two versions
        */
        @returns 1 if the left version is after the right, 0 if they're the same, -1 if the left is before the right
        public static int CompareVersions( String left, String right) {
            if( IgnoreVersionChecks || left == right) return 0;

            // we actually need to parse the ints here, made up of 4 parts separated by '.'
            // sample: 123.45.67.90123
            leftv = ParseVersion(left);
            rightv = ParseVersion(right);
            for (int i = 0; i < leftv.Length; i++) {
                int comparison = leftv[i].CompareTo(rightv[i]);
                if( comparison != 0) {
                    return comparison;
                }
            }
            return 0;
        }

        private static int[] ParseVersion( String version) {
            parts = version.split('.');
            return parts.Select(x -> int.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        }
    }
}
