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

using System.Globalization;
using System.Linq;

package com.quantconnect.lean.Util
{
    /// <summary>
    /// Provides methods for dealing with lean assembly versions
    /// </summary>
    public static class VersionHelper
    {
        private static readonly boolean IgnoreVersionChecks = Configuration.Config.GetBool("ignore-version-checks");

        /// <summary>
        /// Determines whether or not the specified version is older than this instance
        /// </summary>
        /// <param name="version">The version to compare</param>
        /// <returns>True if the specified version is older, false otherwise</returns>
        public static boolean IsOlderVersion( String version)
        {
            return CompareVersions(version, Globals.Version) < 0;
        }

        /// <summary>
        /// Determines whether or not the specified version is newer than this instance
        /// </summary>
        /// <param name="version">The version to compare</param>
        /// <returns>True if the specified version is newer, false otherwise</returns>
        public static boolean IsNewerVersion( String version)
        {
            return CompareVersions(version, Globals.Version) > 0;
        }

        /// <summary>
        /// Determines whether or not the specified version is equal to this instance
        /// </summary>
        /// <param name="version">The version to compare</param>
        /// <returns>True if the specified version is equal, false otherwise</returns>
        public static boolean IsEqualVersion( String version)
        {
            return CompareVersions(version, Globals.Version) == 0;
        }

        /// <summary>
        /// Determines whether or not the specified version is not equal to this instance
        /// </summary>
        /// <param name="version">The version to compare</param>
        /// <returns>True if the specified version is not equal, false otherwise</returns>
        public static boolean IsNotEqualVersion( String version)
        {
            return !IsEqualVersion(version);
        }

        /// <summary>
        /// Compares two versions
        /// </summary>
        /// <returns>1 if the left version is after the right, 0 if they're the same, -1 if the left is before the right</returns>
        public static int CompareVersions( String left, String right)
        {
            if (IgnoreVersionChecks || left == right) return 0;

            // we actually need to parse the ints here, made up of 4 parts separated by '.'
            // sample: 123.45.67.90123
            leftv = ParseVersion(left);
            rightv = ParseVersion(right);
            for (int i = 0; i < leftv.Length; i++)
            {
                int comparison = leftv[i].CompareTo(rightv[i]);
                if (comparison != 0)
                {
                    return comparison;
                }
            }
            return 0;
        }

        private static int[] ParseVersion( String version)
        {
            parts = version.Split('.');
            return parts.Select(x => int.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        }
    }
}
