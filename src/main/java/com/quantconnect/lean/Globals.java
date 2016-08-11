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

//using System.Reflection;
//using QuantConnect.Configuration;

package com.quantconnect.lean;

import org.apache.commons.lang3.StringUtils;

import com.quantconnect.lean.configuration.Config;

/// Provides application level constant values
public class Globals {
    /// The directory used for storing downloaded remote files
    public static final String Cache = "./cache/data";
    /// The root directory of the data folder for this application
    private static String dataFolder;
    /// The version of lean
    private static String version;

    static {
        dataFolder = Config.get( "data-folder", Config.get( "data-directory", "../../../Data/" ) );
        version = "jLean"; //Assembly.GetExecutingAssembly().GetName().Version.toString();
        String versionid = Config.get( "version-id" );
        if( StringUtils.isNotBlank( versionid ) )
            version += "." + versionid;
    }
    

    private Globals() { }

    public static String getDataFolder() {
        return dataFolder;
    }

    public static String getVersion() {
        return version;
    }
}

/*
using System.Reflection;
using QuantConnect.Configuration;

package com.quantconnect.lean
{
    /// <summary>
    /// Provides application level constant values
    /// </summary>
    public static class Globals
    {
        static Globals()
        {
            Reset();
        }

        /// <summary>
        /// The root directory of the data folder for this application
        /// </summary>
        public static String DataFolder { get; private set; }

        /// <summary>
        /// Resets global values with the Config data.
        /// </summary>
        public static void Reset ()
        {
            DataFolder = Config.Get("data-folder", Config.Get("data-directory", @"../../../Data/"));

            Version = Assembly.GetExecutingAssembly().GetName().Version.toString();
            versionid = Config.Get("version-id");
            if (!string.IsNullOrWhiteSpace(versionid))
            {
                Version += "." + versionid;
            }
        }

        /// <summary>
        /// The directory used for storing downloaded remote files
        /// </summary>
        public static final String Cache = "./cache/data";

        /// <summary>
        /// The version of lean
        /// </summary>
        public static String Version { get; private set; }
    }
}
*/
