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

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using QuantConnect.Logging;

package com.quantconnect.lean.configuration;

import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.fasterxml.jackson.core.type.TypeReference;
import com.quantconnect.lean.Global;

import javaslang.Lazy;

// Configuration class loads the required external setup variables to launch the Lean engine.
public class Config {
    private static final String ENVIRONMENT_CONFIG_NAME = "environment";
    private static final String CONFIG_FILE_NAME = "config.json";
    private static final Logger LOG = LoggerFactory.getLogger( Config.class );
    private static final Lazy<Map<String,Object>> settings = Lazy.of( () -> {
//            // initialize settings inside a lazy for free thread-safe, one-time initialization
//        
        final URL configFileUrl = Config.class.getResource( CONFIG_FILE_NAME );
        if( configFileUrl != null ) {
            try {
//                final Path configPath = Paths.get( configFileUrl.toURI() );
//                if( Files.exists( configPath ) ) {
//                    return new JSONObject( Files.lines( configPath ).collect( Collectors.joining() ) );
                return Global.OBJECT_MAPPER.readValue( configFileUrl, new TypeReference<Map<String,Object>>() { } );
//                }
            }
            catch( Exception e ) {
                LOG.warn( "Unable to find config file", e );
            }
        }

        final Map<String,Object> configObject = new HashMap<>();
        configObject.put( "algorithm-type-name", "BasicTemplateAlgorithm" ); //TODO
//                {"live-mode", false},
//                {"data-folder", "../../../Data/"},
//                {"messaging-handler", "QuantConnect.Messaging.Messaging"},
//                {"queue-handler", "QuantConnect.Queues.Queues"},
//                {"api-handler", "QuantConnect.Api.Api"},
//                {"setup-handler", "QuantConnect.Lean.Engine.Setup.ConsoleSetupHandler"},
//                {"result-handler", "QuantConnect.Lean.Engine.Results.BacktestingResultHandler"},
//                {"data-feed-handler", "QuantConnect.Lean.Engine.DataFeeds.FileSystemDataFeed"},
//                {"real-time-handler", "QuantConnect.Lean.Engine.RealTime.BacktestingRealTimeHandler"},
//                {"transaction-handler", "QuantConnect.Lean.Engine.TransactionHandlers.BacktestingTransactionHandler"}
//            };

        return configObject;
    } );

    /// Gets the currently selected environment. If sub-environments are defined,
    /// they'll be returned as {env1}.{env2}
    /// <returns>The fully qualified currently selected environment</returns>
    @SuppressWarnings( "unchecked")
    public static String getEnvironment() {
        final List<String> environments = new ArrayList<String>();

        Map<String,Object> currentEnvironment = settings.get(); //TODO better type checks
        String currentEnv = (String)currentEnvironment.get( ENVIRONMENT_CONFIG_NAME );
        while( currentEnvironment != null && currentEnv != null ) {
            environments.add( currentEnv );
            final Map<String,Object> moreEnvironments = (Map<String,Object>)currentEnvironment.get( "environments" );
            if( moreEnvironments == null )
                break;

            currentEnvironment = (Map<String,Object>)moreEnvironments.get( currentEnv );
            currentEnv = (String)currentEnvironment.get( ENVIRONMENT_CONFIG_NAME );
        }
        
        return environments.stream().collect( Collectors.joining( "." ) );
    }
    
    /// Get the matching config setting from the file searching for this key.
    /// <param name="key">String key value we're seaching for in the config file.</param>
    /// <param name="defaultValue"></param>
    /// <returns>String value of the configuration setting or empty String if nothing found.</returns>
    public static String get( String key ) {
        return get( key, "" );
    }
    
    public static String get( String key, String defaultValue ) {
        // special case environment requests
        if( key.equals( ENVIRONMENT_CONFIG_NAME ) ) 
            return getEnvironment();

        final Map<String,Object> token = getToken( settings.get(), key );
        if( token == null ) {
            LOG.trace( "Config.Get(): Configuration key not found. Key: {} - Using default value: {}", key, defaultValue );
            return defaultValue;
        }
        
        return token.toString();
    }

    /// <summary>
    /// Gets the underlying JToken for the specified key
    /// </summary>
    public static Map<String,Object> getToken( String key ) {
        return getToken( settings.get(), key );
    }

    /// <summary>
    /// Sets a configuration value. This is really only used to help testing. The key heye can be
    /// specified as {environment}.key to set a value on a specific environment
    /// </summary>
    /// <param name="key">The key to be set</param>
    /// <param name="value">The new value</param>
    @SuppressWarnings( "unchecked")
    public static void set( String key, String value ) {
        Map<String,Object> environment = settings.get();
        while( key.contains( "." ) ) {
            String envName = key.substring( 0, key.indexOf( "." ) );
            key = key.substring( key.indexOf( "." ) + 1 );
            Map<String,Object> environments = (Map<String,Object>)environment.get( "environments" );
            if( environments == null )
                environment.put( "environments", environments = new HashMap<String,Object>() );
            environment = (Map<String,Object>)environments.get( envName );
        }
        
        environment.put( key, value );
    }

    /// <summary>
    /// Get a boolean value configuration setting by a configuration key.
    /// </summary>
    /// <param name="key">String value of the configuration key.</param>
    /// <param name="defaultValue">The default value to use if not found in configuration</param>
    /// <returns>Boolean value of the config setting.</returns>
    public static boolean getBool( String key ) {
        return getBool( key, false );
    }
    
    public static boolean getBool( String key, boolean defaultValue ) {
        return getValue(key, defaultValue);
    }

    /// <summary>
    /// Get the int value of a config string.
    /// </summary>
    /// <param name="key">Search key from the config file</param>
    /// <param name="defaultValue">The default value to use if not found in configuration</param>
    /// <returns>Int value of the config setting.</returns>
    public static int getInt( String key ) {
        return getInt( key, 0 );
    }
    
    public static int getInt( String key, int defaultValue ) {
        return getValue(key, defaultValue);
    }

    /// <summary>
    /// Get the double value of a config string.
    /// </summary>
    /// <param name="key">Search key from the config file</param>
    /// <param name="defaultValue">The default value to use if not found in configuration</param>
    /// <returns>Double value of the config setting.</returns>
    public static double getDouble( String key ) {
        return getDouble( key, 0.0 );
    }
    
    public static double getDouble( String key, double defaultValue ) {
        return getValue( key, defaultValue );
    }

    /// <summary>
    /// Gets a value from configuration and converts it to the requested type, assigning a default if
    /// the configuration is null or empty
    /// </summary>
    /// <typeparam name="T">The requested type</typeparam>
    /// <param name="key">Search key from the config file</param>
    /// <param name="defaultValue">The default value to use if not found in configuration</param>
    /// <returns>Converted value of the config setting.</returns>
    public static <T> T getValue( String key ) {
        return getValue( key, null );
    }
    
    public static <T> T getValue( String key, T defaultValue ) {
        return defaultValue; //TODO
//        // special case environment requests
//        if( key.equals( "environment" ) && typeof (T) == typeof ( String)) return (T) (object) GetEnvironment();
//
//        token = GetToken(Settings.Value, key);
//        if( token == null )
//        {
//            Log.Trace( String.format( "Config.GetValue(): %1$s - Using default value: %2$s", key, defaultValue));
//            return defaultValue;
//        }
//
//        type = typeof(T);
//        String value;
//        try
//        {
//            value = token.Value<String>();
//        }
//        catch (Exception err)
//        {
//            value = token.toString();
//        }
//
//        if( type.IsEnum)
//        {
//            return (T) Enum.Parse(type, value);
//        }
//
//        if( typeof(IConvertible).IsAssignableFrom(type))
//        {
//            return (T) Convert.ChangeType(value, type);
//        }
//
//        // try and find a static parse method
//        try
//        {
//            parse = type.GetMethod( "Parse", new[]{typeof( String)});
//            if( parse != null )
//            {
//                result = parse.Invoke(null, new object[] {value});
//                return (T) result;
//            }
//        }
//        catch (Exception err)
//        {
//            Log.Trace( "Config.GetValue<%1$s>(%2$s,%3$s): Failed to parse: {3}. Using default value.", typeof (T).Name, key, defaultValue, value);
//            Log.Error(err);
//            return defaultValue;
//        }
//
//        try
//        {
//            return JsonConvert.DeserializeObject<T>(value);
//        }
//        catch (Exception err)
//        {
//            Log.Trace( "Config.GetValue<%1$s>(%2$s,%3$s): Failed to JSON deserialize: {3}. Using default value.", typeof(T).Name, key, defaultValue, value);
//            Log.Error(err);
//            return defaultValue;
//        }
    }

    /// <summary>
    /// Tries to find the specified key and parse it as a T, using
    /// default(T) if unable to locate the key or unable to parse it
    /// </summary>
    /// <typeparam name="T">The desired output type</typeparam>
    /// <param name="key">The configuration key</param>
    /// <param name="value">The output value</param>
    /// <returns>True on successful parse, false when output value is default(T)</returns>
    public static <T> /*boolean*/ T tryGetValue( String key ) {
        return tryGetValue( key, null );
    }

    /// <summary>
    /// Tries to find the specified key and parse it as a T, using
    /// defaultValue if unable to locate the key or unable to parse it
    /// </summary>
    /// <typeparam name="T">The desired output type</typeparam>
    /// <param name="key">The configuration key</param>
    /// <param name="defaultValue">The default value to use on key not found or unsuccessful parse</param>
    /// <param name="value">The output value</param>
    /// <returns>True on successful parse, false when output value is defaultValue</returns>
    public static <T> /*boolean*/ T tryGetValue( String key, T defaultValue ) {
//        try {
//            value = 
        return getValue( key, defaultValue );
//            return true;
//        }
//        catch
//        {
//            value = defaultValue;
//            return false;
//        }
    }

    /// <summary>
    /// Write the contents of the serialized configuration back to the disk.
    /// </summary>
    public static void write() {
//        if( !settings.get()..IsValueCreated ) return;
//        serialized = JsonConvert.SerializeObject(Settings.Value, Formatting.Indented);
//        File.WriteAllText( "config.json", serialized);
    }

    /// <summary>
    /// Flattens the jobject with respect to the selected environment and then
    /// removes the 'environments' node
    /// </summary>
    /// <param name="@OverrideEnvironment">The environment to use</param>
    /// <returns>The flattened JObject</returns>
    public static Map<String,Object> flatten( String @OverrideEnvironment ) {
        return flatten( settings.get(), @OverrideEnvironment );
    }

    /// <summary>
    /// Flattens the jobject with respect to the selected environment and then
    /// removes the 'environments' node
    /// </summary>
    /// <param name="config">The configuration represented as a JObject</param>
    /// <param name="@OverrideEnvironment">The environment to use</param>
    /// <returns>The flattened JObject</returns>
    public static Map<String,Object> flatten( Map<String,Object> config, String @OverrideEnvironment ) {
        Map<String,Object> clone = null;
//        clone = (Map<String,Object>)config.DeepClone();
//
//        // remove the environment declaration
//        environmentProperty = clone.Property( "environment");
//        if( environmentProperty != null ) environmentProperty.Remove();
//
//        if( !string.IsNullOrEmpty(@OverrideEnvironment))
//        {
//            environmentSections = @OverrideEnvironment.split('.');
//
//            for (int i = 0; i < environmentSections.Length; i++)
//            {
//                env = String.join( ".environments.", environmentSections.Where((x, j) => j <= i));
//
//                environments = config["environments"];
//                if( !(environments is JObject)) continue;
//
//                settings = ((JObject) environments).SelectToken(env);
//                if( settings == null ) continue;
//
//                // copy values for the selected environment to the root
//                foreach (token in settings)
//                {
//                    path = Path.GetExtension(token.Path);
//                    dot = path.IndexOf( ".", StringComparison.InvariantCulture);
//                    if( dot != -1) path = path.Substring(dot + 1);
//
//                    // remove if already exists on clone
//                    jProperty = clone.Property(path);
//                    if( jProperty != null ) jProperty.Remove();
//
//                    value = (token is JProperty ? ((JProperty) token).Value : token).toString();
//                    clone.Add(path, value);
//                }
//            }
//        }
//
//        // remove all environments
//        environmentsProperty = clone.Property( "environments");
//        if( environmentsProperty != null ) environmentsProperty.Remove();

        return clone ;
    }

    @SuppressWarnings( "unchecked")
    private static Map<String,Object> getToken( Map<String,Object> settings, String key ) {
        return getToken( settings, key, (Map<String,Object>)settings.get( key ) );
    }

    @SuppressWarnings( "unchecked")
    private static Map<String,Object> getToken( Map<String,Object> settings, String key, Map<String,Object> current ) {
        final String environmentSetting = (String)settings.get( ENVIRONMENT_CONFIG_NAME );
        if( StringUtils.isNotBlank( environmentSetting ) ) {
            final Map<String,Object> environment = (Map<String,Object>)settings.get( "environments." + environmentSetting );
            if( environment != null ) {
                final Map<String,Object> setting = (Map<String,Object>)environment.get( key );
                if( setting != null )
                    current = setting;
                    // allows nesting of environments, live.tradier, live.interactive, ect...
                return getToken( environment, key, current );
            }
        }
        
        if( current == null )
            return (Map<String,Object>)settings.get( key );

        return current;
    }
}


/*

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Logging;

package com.quantconnect.lean.Configuration
{
    /// <summary>
    /// Configuration class loads the required external setup variables to launch the Lean engine.
    /// </summary>
    public static class Config
    {
        //Location of the configuration file.
        private static final String ConfigurationFileName = "config.json";

        private static readonly Lazy<JObject> Settings = new Lazy<JObject>(() =>
        {
            // initialize settings inside a lazy for free thread-safe, one-time initialization
            if( !File.Exists(ConfigurationFileName)) {
                return new JObject
                {
                    {"algorithm-type-name", "BasicTemplateAlgorithm"},
                    {"live-mode", false},
                    {"data-folder", "../../../Data/"},
                    {"messaging-handler", "QuantConnect.Messaging.Messaging"},
                    {"queue-handler", "QuantConnect.Queues.Queues"},
                    {"api-handler", "QuantConnect.Api.Api"},
                    {"setup-handler", "QuantConnect.Lean.Engine.Setup.ConsoleSetupHandler"},
                    {"result-handler", "QuantConnect.Lean.Engine.Results.BacktestingResultHandler"},
                    {"data-feed-handler", "QuantConnect.Lean.Engine.DataFeeds.FileSystemDataFeed"},
                    {"real-time-handler", "QuantConnect.Lean.Engine.RealTime.BacktestingRealTimeHandler"},
                    {"transaction-handler", "QuantConnect.Lean.Engine.TransactionHandlers.BacktestingTransactionHandler"}
                };
            }

            return JObject.Parse(File.ReadAllText(ConfigurationFileName));
        });

        /// <summary>
        /// Gets the currently selected environment. If sub-environments are defined,
        /// they'll be returned as {env1}.{env2}
        /// </summary>
        /// <returns>The fully qualified currently selected environment</returns>
        public static String GetEnvironment() {
            environments = new List<String>();
            JToken currentEnvironment = Settings.Value;
            env = currentEnvironment["environment"];
            while (currentEnvironment != null && env != null ) {
                currentEnv = env.Value<String>();
                environments.Add(currentEnv);
                moreEnvironments = currentEnvironment["environments"];
                if( moreEnvironments == null ) {
                    break;
                }

                currentEnvironment = moreEnvironments[currentEnv];
                env = currentEnvironment["environment"];
            }
            return String.join( ".", environments);
        }
        
        /// <summary>
        /// Get the matching config setting from the file searching for this key.
        /// </summary>
        /// <param name="key">String key value we're seaching for in the config file.</param>
        /// <param name="defaultValue"></param>
        /// <returns>String value of the configuration setting or empty String if nothing found.</returns>
        public static String Get( String key, String defaultValue = "") {
            // special case environment requests
            if( key == "environment") return GetEnvironment();

            token = GetToken(Settings.Value, key);
            if( token == null ) {
                Log.Trace( String.format( "Config.Get(): Configuration key not found. Key: %1$s - Using default value: %2$s", key, defaultValue));
                return defaultValue;
            }
            return token.toString();
        }

        /// <summary>
        /// Gets the underlying JToken for the specified key
        /// </summary>
        public static JToken GetToken( String key) {
            return GetToken(Settings.Value, key);
        }

        /// <summary>
        /// Sets a configuration value. This is really only used to help testing. The key heye can be
        /// specified as {environment}.key to set a value on a specific environment
        /// </summary>
        /// <param name="key">The key to be set</param>
        /// <param name="value">The new value</param>
        public static void Set( String key, String value) {
            JToken environment = Settings.Value;
            while (key.Contains( ".")) {
                envName = key.Substring(0, key.IndexOf( "."));
                key = key.Substring(key.IndexOf( ".") + 1);
                environments = environment["environments"];
                if( environments == null ) {
                    environment["environments"] = environments = new JObject();
                }
                environment = environments[envName];
            }
            environment[key] = value;
        }

        /// <summary>
        /// Get a boolean value configuration setting by a configuration key.
        /// </summary>
        /// <param name="key">String value of the configuration key.</param>
        /// <param name="defaultValue">The default value to use if not found in configuration</param>
        /// <returns>Boolean value of the config setting.</returns>
        public static boolean GetBool( String key, boolean defaultValue = false) {
            return GetValue(key, defaultValue);
        }

        /// <summary>
        /// Get the int value of a config string.
        /// </summary>
        /// <param name="key">Search key from the config file</param>
        /// <param name="defaultValue">The default value to use if not found in configuration</param>
        /// <returns>Int value of the config setting.</returns>
        public static int GetInt( String key, int defaultValue = 0) {
            return GetValue(key, defaultValue);
        }

        /// <summary>
        /// Get the double value of a config string.
        /// </summary>
        /// <param name="key">Search key from the config file</param>
        /// <param name="defaultValue">The default value to use if not found in configuration</param>
        /// <returns>Double value of the config setting.</returns>
        public static double GetDouble( String key, double defaultValue = 0.0) {
            return GetValue(key, defaultValue);
        }

        /// <summary>
        /// Gets a value from configuration and converts it to the requested type, assigning a default if
        /// the configuration is null or empty
        /// </summary>
        /// <typeparam name="T">The requested type</typeparam>
        /// <param name="key">Search key from the config file</param>
        /// <param name="defaultValue">The default value to use if not found in configuration</param>
        /// <returns>Converted value of the config setting.</returns>
        public static T GetValue<T>( String key, T defaultValue = default(T)) {
            // special case environment requests
            if( key == "environment" && typeof (T) == typeof ( String)) return (T) (object) GetEnvironment();

            token = GetToken(Settings.Value, key);
            if( token == null ) {
                Log.Trace( String.format( "Config.GetValue(): %1$s - Using default value: %2$s", key, defaultValue));
                return defaultValue;
            }

            type = typeof(T);
            String value;
            try
            {
                value = token.Value<String>();
            }
            catch (Exception err) {
                value = token.toString();
            }

            if( type.IsEnum) {
                return (T) Enum.Parse(type, value);
            }

            if( typeof(IConvertible).IsAssignableFrom(type)) {
                return (T) Convert.ChangeType(value, type);
            }

            // try and find a static parse method
            try
            {
                parse = type.GetMethod( "Parse", new[]{typeof( String)});
                if( parse != null ) {
                    result = parse.Invoke(null, new object[] {value});
                    return (T) result;
                }
            }
            catch (Exception err) {
                Log.Trace( "Config.GetValue<%1$s>(%2$s,%3$s): Failed to parse: {3}. Using default value.", typeof (T).Name, key, defaultValue, value);
                Log.Error(err);
                return defaultValue;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception err) {
                Log.Trace( "Config.GetValue<%1$s>(%2$s,%3$s): Failed to JSON deserialize: {3}. Using default value.", typeof(T).Name, key, defaultValue, value);
                Log.Error(err);
                return defaultValue;
            }
        }

        /// <summary>
        /// Tries to find the specified key and parse it as a T, using
        /// default(T) if unable to locate the key or unable to parse it
        /// </summary>
        /// <typeparam name="T">The desired output type</typeparam>
        /// <param name="key">The configuration key</param>
        /// <param name="value">The output value</param>
        /// <returns>True on successful parse, false when output value is default(T)</returns>
        public static boolean TryGetValue<T>( String key, out T value) {
            return TryGetValue(key, default(T), out value);
        }

        /// <summary>
        /// Tries to find the specified key and parse it as a T, using
        /// defaultValue if unable to locate the key or unable to parse it
        /// </summary>
        /// <typeparam name="T">The desired output type</typeparam>
        /// <param name="key">The configuration key</param>
        /// <param name="defaultValue">The default value to use on key not found or unsuccessful parse</param>
        /// <param name="value">The output value</param>
        /// <returns>True on successful parse, false when output value is defaultValue</returns>
        public static boolean TryGetValue<T>( String key, T defaultValue, out T value) {
            try
            {
                value = GetValue(key, defaultValue);
                return true;
            }
            catch
            {
                value = defaultValue;
                return false;
            }
        }

        /// <summary>
        /// Write the contents of the serialized configuration back to the disk.
        /// </summary>
        public static void Write() {
            if( !Settings.IsValueCreated) return;
            serialized = JsonConvert.SerializeObject(Settings.Value, Formatting.Indented);
            File.WriteAllText( "config.json", serialized);
        }

        /// <summary>
        /// Flattens the jobject with respect to the selected environment and then
        /// removes the 'environments' node
        /// </summary>
        /// <param name="@OverrideEnvironment">The environment to use</param>
        /// <returns>The flattened JObject</returns>
        public static JObject Flatten( String @OverrideEnvironment) {
            return Flatten(Settings.Value, @OverrideEnvironment);
        }

        /// <summary>
        /// Flattens the jobject with respect to the selected environment and then
        /// removes the 'environments' node
        /// </summary>
        /// <param name="config">The configuration represented as a JObject</param>
        /// <param name="@OverrideEnvironment">The environment to use</param>
        /// <returns>The flattened JObject</returns>
        public static JObject Flatten(JObject config, String @OverrideEnvironment) {
            clone = (JObject)config.DeepClone();

            // remove the environment declaration
            environmentProperty = clone.Property( "environment");
            if( environmentProperty != null ) environmentProperty.Remove();

            if( !string.IsNullOrEmpty(@OverrideEnvironment)) {
                environmentSections = @OverrideEnvironment.split('.');

                for (int i = 0; i < environmentSections.Length; i++) {
                    env = String.join( ".environments.", environmentSections.Where((x, j) => j <= i));

                    environments = config["environments"];
                    if( !(environments is JObject)) continue;

                    settings = ((JObject) environments).SelectToken(env);
                    if( settings == null ) continue;

                    // copy values for the selected environment to the root
                    foreach (token in settings) {
                        path = Path.GetExtension(token.Path);
                        dot = path.IndexOf( ".", StringComparison.InvariantCulture);
                        if( dot != -1) path = path.Substring(dot + 1);

                        // remove if already exists on clone
                        jProperty = clone.Property(path);
                        if( jProperty != null ) jProperty.Remove();

                        value = (token is JProperty ? ((JProperty) token).Value : token).toString();
                        clone.Add(path, value);
                    }
                }
            }

            // remove all environments
            environmentsProperty = clone.Property( "environments");
            if( environmentsProperty != null ) environmentsProperty.Remove();

            return clone;
        }

        private static JToken GetToken(JToken settings, String key) {
            return GetToken(settings, key, settings.SelectToken(key));
        }

        private static JToken GetToken(JToken settings, String key, JToken current) {
            environmentSetting = settings.SelectToken( "environment");
            if( environmentSetting != null ) {
                environmentSettingValue = environmentSetting.Value<String>();
                if( !string.IsNullOrWhiteSpace(environmentSettingValue)) {
                    environment = settings.SelectToken( "environments." + environmentSettingValue);
                    if( environment != null ) {
                        setting = environment.SelectToken(key);
                        if( setting != null ) {
                            current = setting;
                        }
                        // allows nesting of environments, live.tradier, live.interactive, ect...
                        return GetToken(environment, key, current);
                    }
                }
            }
            if( current == null ) {
                return settings.SelectToken(key);
            }
            return current;
        }
    }
}
*/