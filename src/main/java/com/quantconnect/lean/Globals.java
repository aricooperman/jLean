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

package com.quantconnect.lean;

import org.apache.commons.lang3.StringUtils;

import com.quantconnect.lean.configuration.Config;

/**
 * Provides application level constant values
 */
public class Globals {

    /**
     * The directory used for storing downloaded remote files
     */
    public static final String CACHE = "./cache/data";
    
    /**
     * The root directory of the data folder for this application
     */
    private static String dataFolder;
    
    /**
     * The version of lean
     */
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
